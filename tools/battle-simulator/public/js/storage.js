const CUSTOM_TEMPLATES_KEY = 'spaceage.battleSim.customTemplates';

export function loadCustomTemplates() {
  try {
    const raw = localStorage.getItem(CUSTOM_TEMPLATES_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

export function saveCustomTemplate(template) {
  const templates = loadCustomTemplates();
  const existing = templates.findIndex((entry) => entry.id === template.id);
  if (existing >= 0) {
    templates[existing] = template;
  } else {
    templates.push(template);
  }
  localStorage.setItem(CUSTOM_TEMPLATES_KEY, JSON.stringify(templates));
  return templates;
}

export function deleteCustomTemplate(id) {
  const templates = loadCustomTemplates().filter((entry) => entry.id !== id);
  localStorage.setItem(CUSTOM_TEMPLATES_KEY, JSON.stringify(templates));
  return templates;
}
