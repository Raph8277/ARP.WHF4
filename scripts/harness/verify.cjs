// Run from any repository directory: node <repo>/scripts/harness/verify.cjs
const fs = require('node:fs');
const path = require('node:path');
const cp = require('node:child_process');
const core = require('./core.cjs');

function verify(root) {
  const directory = path.join(root, 'tmp', 'verification');
  fs.mkdirSync(directory, { recursive: true });
  const lockPath = path.join(directory, 'running.lock');
  let lock;
  try { lock = fs.openSync(lockPath, 'wx'); }
  catch { throw new Error('Une vérification est déjà en cours (tmp/verification/running.lock).'); }
  try {
    fs.writeFileSync(lock, String(process.pid));
    fs.rmSync(core.receiptPath(root), { force: true });
    const before = core.fingerprint(root);
    core.checkWhitespace(root);
    const steps = [
      ['harness', process.execPath, ['--test', 'scripts/harness/core.test.cjs']],
      ['build', 'dotnet', ['build', 'app/Wfrp4.sln', '--no-restore', '--configuration', 'Release', '--verbosity', 'minimal']],
      ['tests', 'dotnet', ['test', 'app/Wfrp4.sln', '--no-build', '--no-restore', '--configuration', 'Release', '--logger', 'trx', '--results-directory', directory]],
    ];
    for (const [name, command, args] of steps) {
      console.log(`Vérification : ${name}`);
      const result = cp.spawnSync(command, args, {
        cwd: root, encoding: 'utf8', timeout: 600000, maxBuffer: 16 * 1024 * 1024,
        windowsHide: true,
      });
      const output = (result.stdout || '') + (result.stderr || '') + (result.error?.message || '');
      fs.writeFileSync(path.join(directory, `${name}.log`), output);
      if (result.status !== 0) {
        console.error(output.slice(-12000));
        throw new Error(`${name} a échoué ; consulter tmp/verification/${name}.log.`);
      }
    }
    if (before !== core.fingerprint(root)) throw new Error('Les sources ont changé pendant la validation. Relancer le vérificateur.');
    fs.writeFileSync(core.receiptPath(root), JSON.stringify({
      schemaVersion: 1, passed: true, fingerprint: before,
      completedAt: new Date().toISOString(), checks: steps.map(([name]) => name),
    }, null, 2) + '\n');
    console.log('Validation réussie : harness, compilation Release et tests. Preuve enregistrée.');
  } finally {
    fs.closeSync(lock);
    fs.rmSync(lockPath, { force: true });
  }
}
if (require.main === module) {
  try { verify(core.rootFrom(__dirname)); }
  catch (error) { console.error(error.message); process.exitCode = 1; }
}
module.exports = { verify };
