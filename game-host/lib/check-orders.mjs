const KNOWN_VERBS = new Set([
  'MOVE', 'USE', 'ATTACK', 'TACTIC', 'GIVE', 'GET', 'SET', 'STACK', 'HAS',
  'BUY', 'SELL', 'RESEARCH', 'TRAIN', 'COPY', 'TRANSFER', 'CAPTURE', 'DECLARE',
  'CONTRACT', 'PRESS', 'JUMP', 'REPAIR', 'SURVEY', 'PRODUCE', 'ONLINE', 'OFFLINE',
]);

export function validateOrderText(body, factionId, expectedPassword) {
  const warnings = [];
  const lines = body.split(/\r?\n/);
  let sawFaction = false;
  let currentStack = null;

  for (let i = 0; i < lines.length; i++) {
    const raw = lines[i];
    const line = raw.replace(/;.*$|\/\/.*$/, '').trim();
    if (!line) continue;

    const upper = line.toUpperCase();
    if (upper.startsWith('#FACTION')) {
      sawFaction = true;
      const m = line.match(/^#faction\s+(\d+)\s+"([^"]*)"/i);
      if (!m) {
        warnings.push(`Line ${i + 1}: malformed #faction header`);
        continue;
      }
      if (parseInt(m[1], 10) !== factionId) {
        warnings.push(`Line ${i + 1}: faction id ${m[1]} does not match session (${factionId})`);
      }
      if (m[2] !== expectedPassword) {
        warnings.push(`Line ${i + 1}: password does not match gamein for faction ${factionId}`);
      }
      continue;
    }

    if (upper.startsWith('#MODULESTACK')) {
      const m = line.match(/^#modulestack\s+(\S+)/i);
      currentStack = m ? m[1] : null;
      if (!currentStack) {
        warnings.push(`Line ${i + 1}: malformed #modulestack`);
      }
      continue;
    }

    if (upper.startsWith('#PERSON')) {
      continue;
    }

    if (!sawFaction) {
      warnings.push(`Line ${i + 1}: #faction header must precede orders`);
    }

    const verb = line.split(/\s+/)[0].toUpperCase();
    if (!KNOWN_VERBS.has(verb)) {
      warnings.push(`Line ${i + 1}: unknown or unsupported verb "${verb}"`);
    }

    if (verb === 'MOVE' && !/\s+[A-Z]\d+/i.test(line)) {
      warnings.push(`Line ${i + 1}: MOVE should specify a destination location id`);
    }

    if ((verb === 'USE' || verb === 'PRODUCE') && /as\s+new/i.test(line) && !currentStack && !/\bfor\s+\d+/i.test(line)) {
      warnings.push(`Line ${i + 1}: USE/PRODUCE may need #modulestack context or FOR target`);
    }
  }

  if (!sawFaction) {
    warnings.unshift('Missing #faction header');
  }

  return warnings;
}
