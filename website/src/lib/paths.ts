/** Internal site URL respecting Astro `base` (GitHub Pages project path). */
export function withBase(path = ''): string {
  let base = import.meta.env.BASE_URL;
  if (!base.endsWith('/')) {
    base = `${base}/`;
  }
  const segment = path.replace(/^\//, '');
  return `${base}${segment}`;
}
