const fs = require('node:fs');
const core = require('./core.cjs');

function handle(event, root) {
  const command = 'node scripts/harness/verify.cjs';
  if (event.hook_event_name === 'SessionStart') return {
    hookSpecificOutput: { hookEventName: 'SessionStart', additionalContext:
      `WFRP : suivre AGENTS.md. Pour valider une correction, exécuter ${command} depuis la racine. Suivi : docs/WFRP.fiabilisation.md. Aucun service ni migration n'est démarré par les hooks.` },
  };
  if (event.hook_event_name === 'PostToolUse') {
    try { core.checkWhitespace(root); return {}; }
    catch { return { systemMessage: 'Le diff contient des erreurs de whitespace ; exécuter git diff --check et git diff --cached --check.' }; }
  }
  if (event.hook_event_name !== 'Stop') return {};
  if (!core.hasChanges(root) || core.validReceipt(root)) return {};
  const reason = `Modifications applicatives ou harness sans validation du contenu courant. Exécuter ${command}, corriger les échecs puis mettre à jour le suivi. Si l'environnement bloque, expliquer précisément la vérification non exécutée.`;
  // One continuation only: an unavailable SDK must not create an endless loop.
  return event.stop_hook_active ? { systemMessage: reason } : { decision: 'block', reason };
}
if (require.main === module) {
  try {
    const event = JSON.parse(fs.readFileSync(0, 'utf8'));
    console.log(JSON.stringify(handle(event, core.rootFrom(__dirname))));
  } catch (error) {
    console.error('Hook WFRP : ' + error.message);
    process.exitCode = 1;
  }
}
module.exports = { handle };
