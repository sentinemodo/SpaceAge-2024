import { defineConfig } from 'astro/config';

// GitHub Pages project site: https://sentinemodo.github.io/SpaceAge-2024/
// Local dev / Playwright preview: leave PUBLIC_BASE_PATH unset (base `/`).
const base = process.env.PUBLIC_BASE_PATH ?? '/';
const site = process.env.PUBLIC_SITE_URL ?? undefined;

// https://astro.build/config
export default defineConfig({
  site,
  base,
  output: 'static',
  integrations: [],
});
