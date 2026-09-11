/** Build battle-sim input XML for Game.exe /battle-sim. Reusable by the visual tool. */

function escapeAttr(value) {
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/"/g, '&quot;')
    .replace(/</g, '&lt;');
}

function appendStackXml(lines, stack, indent, idOverride) {
  const id = idOverride || stack.id;
  let open = `${indent}<stack id="${escapeAttr(id)}" type="${escapeAttr(stack.type)}"`;
  if (stack.name) open += ` name="${escapeAttr(stack.name)}"`;
  if (stack.quantity && stack.quantity !== 1) open += ` quantity="${stack.quantity}"`;
  if (stack.tactic) open += ` tactic="${escapeAttr(stack.tactic)}"`;

  const hasItems = Array.isArray(stack.items) && stack.items.length > 0;
  const hasNested = Array.isArray(stack.nested) && stack.nested.length > 0;
  if (!hasItems && !hasNested) {
    lines.push(`${open}/>`);
    return;
  }

  lines.push(`${open}>`);
  if (hasItems) {
    lines.push(`${indent}  <items>`);
    for (const item of stack.items) {
      lines.push(`${indent}    <item type="${escapeAttr(item.type)}" quantity="${item.quantity || 1}" />`);
    }
    lines.push(`${indent}  </items>`);
  }
  if (hasNested) {
    lines.push(`${indent}  <nested>`);
    stack.nested.forEach((child, index) => {
      const childId = child.id || `${id}_n${index + 1}`;
      appendStackXml(lines, child, `${indent}    `, childId);
    });
    lines.push(`${indent}  </nested>`);
  }
  lines.push(`${indent}</stack>`);
}

export function cloneStackForSide(stack, sidePrefix) {
  const clone = JSON.parse(JSON.stringify(stack));
  clone.id = `sim_${sidePrefix}1`;
  if (Array.isArray(clone.nested)) {
    clone.nested = clone.nested.map((child, index) => ({
      ...child,
      id: child.id || `sim_${sidePrefix}1_n${index + 1}`,
    }));
  }
  return clone;
}

export function buildSimInputXml({
  seed,
  locationType,
  attackers,
  defenders,
}) {
  const lines = [
    '<?xml version="1.0" encoding="windows-1251"?>',
    `<battle-sim seed="${seed}" location-type="${escapeAttr(locationType)}">`,
    `  <attackers faction="${escapeAttr(attackers.faction)}" name="${escapeAttr(attackers.name)}">`,
  ];

  attackers.stacks.forEach((stack, index) => {
    const sideStack = cloneStackForSide(stack, `a${index + 1}_`);
    sideStack.id = stack.id || `sim_a${index + 1}`;
    appendStackXml(lines, sideStack, '    ', sideStack.id);
  });

  lines.push('  </attackers>');
  lines.push(`  <defenders faction="${escapeAttr(defenders.faction)}" name="${escapeAttr(defenders.name)}">`);

  defenders.stacks.forEach((stack, index) => {
    const sideStack = cloneStackForSide(stack, `d${index + 1}_`);
    sideStack.id = stack.id || `sim_d${index + 1}`;
    appendStackXml(lines, sideStack, '    ', sideStack.id);
  });

  lines.push('  </defenders>');
  lines.push('</battle-sim>');
  return lines.join('\n');
}
