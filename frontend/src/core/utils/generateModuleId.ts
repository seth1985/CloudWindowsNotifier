// Core utility: generate a safe module ID from a title
export function generateModuleId(title: string): string {
  const rand = Math.floor(10000 + Math.random() * 90000);
  const safe = title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
  const tail = safe || 'module';
  return `module-${rand}-${tail}`;
}
