const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const cp = require('node:child_process');
const core = require('./core.cjs');
const { handle } = require('./hook.cjs');
const { verify } = require('./verify.cjs');

function fixture(t) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'wfrp-harness-'));
  t.after(() => {
    const relative = path.relative(fs.realpathSync(os.tmpdir()), fs.realpathSync(root));
    assert.ok(!relative.startsWith('..') && !path.isAbsolute(relative) && relative.startsWith('wfrp-harness-'));
    fs.rmSync(root, { recursive: true, force: true });
  });
  const git = (...args) => cp.execFileSync('git', ['-C', root, ...args], { stdio: 'pipe' });
  git('init'); git('config', 'user.name', 'Harness test'); git('config', 'user.email', 'harness@example.invalid');
  fs.mkdirSync(path.join(root, 'app'));
  fs.writeFileSync(path.join(root, 'app', 'example.cs'), 'original\n');
  git('add', '.'); git('commit', '-m', 'fixture');
  return { root, git };
}
test('une preuve devient invalide après ajout, édition ou suppression', t => {
  const { root } = fixture(t);
  const receipt = core.receiptPath(root);
  fs.mkdirSync(path.dirname(receipt), { recursive: true });
  const save = () => fs.writeFileSync(receipt, JSON.stringify({ schemaVersion: 1, passed: true, fingerprint: core.fingerprint(root) }));
  save(); assert.equal(core.validReceipt(root), true);
  fs.writeFileSync(path.join(root, 'app', 'new.cs'), 'new');
  assert.equal(core.validReceipt(root), false);
  save(); fs.writeFileSync(path.join(root, 'app', 'example.cs'), 'changed');
  assert.equal(core.validReceipt(root), false);
  save(); fs.unlinkSync(path.join(root, 'app', 'example.cs'));
  assert.equal(core.validReceipt(root), false);
});
test('les artefacts de build ne périment pas la preuve, les fichiers source oui', t => {
  const { root } = fixture(t); const before = core.fingerprint(root);
  fs.mkdirSync(path.join(root, 'app', 'obj'));
  fs.writeFileSync(path.join(root, 'app', 'obj', 'generated.cs'), 'generated');
  assert.equal(core.fingerprint(root), before);
  assert.equal(core.hasChanges(root), false);
  fs.writeFileSync(path.join(root, 'app', 'new.cs'), 'new');
  assert.notEqual(core.fingerprint(root), before);
});
test('Stop ignore un arbre propre, bloque une fois, accepte une preuve fraîche', t => {
  const { root } = fixture(t);
  assert.deepEqual(handle({ hook_event_name: 'Stop' }, root), {});
  fs.writeFileSync(path.join(root, 'app', 'example.cs'), 'changed');
  assert.equal(handle({ hook_event_name: 'Stop' }, root).decision, 'block');
  assert.equal(handle({ hook_event_name: 'Stop', stop_hook_active: true }, root).decision, undefined);
  fs.mkdirSync(path.dirname(core.receiptPath(root)), { recursive: true });
  fs.writeFileSync(core.receiptPath(root), JSON.stringify({ schemaVersion: 1, passed: true, fingerprint: core.fingerprint(root) }));
  assert.deepEqual(handle({ hook_event_name: 'Stop' }, root), {});
});
test('un contrôle échoué retire la preuve précédente et libère le verrou', t => {
  const { root } = fixture(t);
  const receipt = core.receiptPath(root);
  fs.mkdirSync(path.dirname(receipt), { recursive: true });
  fs.writeFileSync(receipt, JSON.stringify({ schemaVersion: 1, passed: true, fingerprint: core.fingerprint(root) }));
  // This isolated fixture deliberately lacks the harness test entrypoint.
  assert.throws(() => verify(root), /harness a échoué/);
  assert.equal(fs.existsSync(receipt), false);
  assert.equal(fs.existsSync(path.join(path.dirname(receipt), 'running.lock')), false);
});
