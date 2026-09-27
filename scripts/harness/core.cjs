const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');

const inputs = ['app', 'scripts/harness', '.agents/skills', '.codex', 'AGENTS.md', 'global.json', '.github/workflows'];
function git(root, args) {
  return cp.execFileSync('git', ['-C', root, ...args], { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
}
function rootFrom(cwd) { return git(cwd, ['rev-parse', '--show-toplevel']).trim(); }
function relevant(file) {
  return !/(^|\/)(bin|obj|node_modules|TestResults)(\/|$)/i.test(file)
    && (!/(^|\/)\.env($|\.)/.test(file) || file.endsWith('/.env.example'))
    && !/\.(trx|log)$/i.test(file);
}
function files(root) {
  return [...new Set(git(root, ['ls-files', '-z', '--cached', '--others', '--exclude-standard', '--', ...inputs])
    .split('\0').filter(Boolean).filter(relevant))].sort();
}
function fingerprint(root) {
  const hash = crypto.createHash('sha256');
  hash.update(git(root, ['rev-parse', 'HEAD']).trim());
  for (const file of files(root)) {
    hash.update('\0' + file + '\0');
    const absolute = path.join(root, file);
    hash.update(fs.existsSync(absolute) ? fs.readFileSync(absolute) : '<deleted>');
  }
  return hash.digest('hex');
}
function hasChanges(root) {
  const tracked = git(root, ['diff', '--name-only', 'HEAD', '--', ...inputs]).split('\n');
  const untracked = git(root, ['ls-files', '--others', '--exclude-standard', '--', ...inputs]).split('\n');
  return [...tracked, ...untracked].some(file => file && relevant(file));
}
function checkWhitespace(root) {
  git(root, ['diff', '--check']);
  git(root, ['diff', '--cached', '--check']);
}
function receiptPath(root) { return path.join(root, 'tmp', 'verification', 'receipt.json'); }
function validReceipt(root) {
  try {
    const receipt = JSON.parse(fs.readFileSync(receiptPath(root), 'utf8'));
    return receipt.schemaVersion === 1 && receipt.passed === true
      && receipt.fingerprint === fingerprint(root);
  } catch { return false; }
}
module.exports = { rootFrom, fingerprint, hasChanges, checkWhitespace, receiptPath, validReceipt };
