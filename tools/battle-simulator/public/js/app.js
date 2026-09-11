import { buildSimInputXml } from './sim-xml.js';
import { loadCustomTemplates, saveCustomTemplate, deleteCustomTemplate } from './storage.js';

const state = {
  catalog: null,
  attackerStacks: [emptyStack()],
  defenderStacks: [emptyStack()],
  seed: 42,
  locationType: 'orbit',
};

function emptyStack() {
  return {
    id: '',
    type: 'inftry',
    name: '',
    quantity: 1,
    tactic: 'destroy',
    items: [],
    nested: [],
  };
}

function byId(id) {
  return document.getElementById(id);
}

async function loadCatalog() {
  const response = await fetch('./presets.json');
  state.catalog = await response.json();
}

function allPresets() {
  const custom = loadCustomTemplates().map((entry) => ({
    ...entry,
    isCustom: true,
  }));
  return [...state.catalog.presets, ...custom];
}

function presetOptions() {
  return allPresets()
    .map((preset) => `<option value="${preset.id}">${preset.displayName}${preset.isCustom ? ' (custom)' : ''}</option>`)
    .join('');
}

function renderSelect(id, options, selected) {
  const select = byId(id);
  select.innerHTML = options
    .map((option) => `<option value="${option.id}"${option.id === selected ? ' selected' : ''}>${option.label}</option>`)
    .join('');
}

function renderStackEditor(containerId, stacks, side) {
  const container = byId(containerId);
  container.innerHTML = stacks
    .map((stack, index) => stackCardHtml(stack, side, index))
    .join('');
}

function stackCardHtml(stack, side, index) {
  const hullOptions = state.catalog.hullTypes
    .map((hull) => `<option value="${hull.id}"${hull.id === stack.type ? ' selected' : ''}>${hull.label}</option>`)
    .join('');
  const tacticOptions = state.catalog.tactics
    .map((tactic) => `<option value="${tactic}"${tactic === stack.tactic ? ' selected' : ''}>${tactic}</option>`)
    .join('');

  const nestedHtml = (stack.nested || [])
    .map(
      (mod, modIndex) => `
      <div class="nested-row" data-side="${side}" data-stack="${index}" data-nested="${modIndex}">
        <select class="nested-type">${moduleOptions(mod.type)}</select>
        <input type="number" class="nested-qty" min="1" value="${mod.quantity || 1}" />
        <button type="button" class="remove-nested">Remove</button>
      </div>`
    )
    .join('');

  return `
    <article class="stack-card" data-side="${side}" data-index="${index}">
      <header>
        <strong>Stack ${index + 1}</strong>
        ${stacksLength(side) > 1 ? `<button type="button" class="remove-stack" data-side="${side}" data-index="${index}">Remove stack</button>` : ''}
      </header>
      <label>Name <input type="text" class="stack-name" value="${stack.name || ''}" placeholder="Display name" /></label>
      <label>Hull / unit
        <select class="stack-type">${hullOptions}</select>
      </label>
      <label>Quantity <input type="number" class="stack-qty" min="1" value="${stack.quantity || 1}" /></label>
      <label>Tactic
        <select class="stack-tactic">${tacticOptions}</select>
      </label>
      <div class="nested-block">
        <div class="nested-header">
          <span>Nested modules</span>
          <button type="button" class="add-nested" data-side="${side}" data-index="${index}">Add module</button>
        </div>
        ${nestedHtml}
      </div>
    </article>`;
}

function stacksLength(side) {
  return side === 'attackers' ? state.attackerStacks.length : state.defenderStacks.length;
}

function moduleOptions(selected) {
  return state.catalog.moduleTypes
    .map((mod) => `<option value="${mod.id}"${mod.id === selected ? ' selected' : ''}>${mod.label}</option>`)
    .join('');
}

function readStacksFromDom(side) {
  const containerId = side === 'attackers' ? 'attackers-editor' : 'defenders-editor';
  const cards = byId(containerId).querySelectorAll('.stack-card');
  const stacks = [];

  cards.forEach((card, index) => {
    const nested = [];
    card.querySelectorAll('.nested-row').forEach((row) => {
      nested.push({
        type: row.querySelector('.nested-type').value,
        quantity: Number(row.querySelector('.nested-qty').value) || 1,
      });
    });

    stacks.push({
      id: `sim_${side === 'attackers' ? 'a' : 'd'}${index + 1}`,
      type: card.querySelector('.stack-type').value,
      name: card.querySelector('.stack-name').value.trim(),
      quantity: Number(card.querySelector('.stack-qty').value) || 1,
      tactic: card.querySelector('.stack-tactic').value,
      items: [],
      nested,
    });
  });

  return stacks;
}

function syncFromDom() {
  state.attackerStacks = readStacksFromDom('attackers');
  state.defenderStacks = readStacksFromDom('defenders');
  state.seed = Number(byId('seed').value) || 1;
  state.locationType = byId('location-type').value;
}

function renderAll() {
  renderStackEditor('attackers-editor', state.attackerStacks, 'attackers');
  renderStackEditor('defenders-editor', state.defenderStacks, 'defenders');
  byId('seed').value = state.seed;
  byId('location-type').value = state.locationType;

  const attackerPreset = byId('attacker-preset');
  const defenderPreset = byId('defender-preset');
  attackerPreset.innerHTML = '<option value="">— preset —</option>' + presetOptions();
  defenderPreset.innerHTML = '<option value="">— preset —</option>' + presetOptions();
}

function applyPreset(side, presetId) {
  if (!presetId) return;
  const preset = allPresets().find((entry) => entry.id === presetId);
  if (!preset) return;

  const stack = JSON.parse(JSON.stringify(preset.stack));
  if (side === 'attackers') {
    state.attackerStacks = [stack];
  } else {
    state.defenderStacks = [stack];
  }
  if (preset.locationType) {
    state.locationType = preset.locationType;
  }
  renderAll();
}

function buildXmlPreview() {
  syncFromDom();
  return buildSimInputXml({
    seed: state.seed,
    locationType: state.locationType,
    attackers: {
      faction: 'sim_a',
      name: byId('attacker-side-name').value.trim() || 'Attackers',
      stacks: state.attackerStacks,
    },
    defenders: {
      faction: 'sim_d',
      name: byId('defender-side-name').value.trim() || 'Defenders',
      stacks: state.defenderStacks,
    },
  });
}

async function runSimulation() {
  syncFromDom();
  const xml = buildXmlPreview();
  byId('xml-preview').value = xml;
  byId('run-status').textContent = 'Running simulation…';
  byId('log-output').textContent = '';

  const response = await fetch('/api/run', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ xml, seed: state.seed }),
  });

  const payload = await response.json();
  if (!response.ok) {
    byId('run-status').textContent = payload.error || 'Simulation failed.';
    return;
  }

  byId('run-status').textContent = 'Simulation complete.';
  byId('log-output').textContent = payload.output;
}

function saveCurrentAsTemplate() {
  syncFromDom();
  const name = prompt('Template name');
  if (!name) return;
  const id = `custom-${Date.now()}`;
  saveCustomTemplate({
    id,
    displayName: name,
    role: 'Custom',
    locationType: state.locationType,
    stack: state.attackerStacks[0],
    isCustom: true,
  });
  renderAll();
}

function wireEvents() {
  byId('run-button').addEventListener('click', () => runSimulation());
  byId('preview-xml').addEventListener('click', () => {
    syncFromDom();
    byId('xml-preview').value = buildXmlPreview();
  });
  byId('save-template').addEventListener('click', saveCurrentAsTemplate);

  byId('attacker-preset').addEventListener('change', (event) => applyPreset('attackers', event.target.value));
  byId('defender-preset').addEventListener('change', (event) => applyPreset('defenders', event.target.value));

  byId('add-attacker-stack').addEventListener('click', () => {
    syncFromDom();
    state.attackerStacks.push(emptyStack());
    renderAll();
  });
  byId('add-defender-stack').addEventListener('click', () => {
    syncFromDom();
    state.defenderStacks.push(emptyStack());
    renderAll();
  });

  document.body.addEventListener('click', (event) => {
    if (event.target.matches('.add-nested')) {
      syncFromDom();
      const side = event.target.dataset.side;
      const index = Number(event.target.dataset.index);
      const stacks = side === 'attackers' ? state.attackerStacks : state.defenderStacks;
      stacks[index].nested = stacks[index].nested || [];
      stacks[index].nested.push({ type: 'pdltur', quantity: 1 });
      renderAll();
    }
    if (event.target.matches('.remove-nested')) {
      syncFromDom();
      const row = event.target.closest('.nested-row');
      const side = row.dataset.side;
      const stackIndex = Number(row.dataset.stack);
      const nestedIndex = Number(row.dataset.nested);
      const stacks = side === 'attackers' ? state.attackerStacks : state.defenderStacks;
      stacks[stackIndex].nested.splice(nestedIndex, 1);
      renderAll();
    }
    if (event.target.matches('.remove-stack')) {
      syncFromDom();
      const side = event.target.dataset.side;
      const index = Number(event.target.dataset.index);
      if (side === 'attackers') {
        state.attackerStacks.splice(index, 1);
      } else {
        state.defenderStacks.splice(index, 1);
      }
      renderAll();
    }
  });
}

async function init() {
  await loadCatalog();
  renderAll();
  wireEvents();
}

init().catch((error) => {
  byId('run-status').textContent = `Failed to load simulator: ${error.message}`;
});
