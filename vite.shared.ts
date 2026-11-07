import { join, relative } from 'node:path';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = import.meta.dirname;

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (folder: string, directory: string) => join(ROOT, folder, relative(ROOT, directory));
const hostedLayout = (directory: string) => ({ nodeOutDir: located('.artifacts', directory), state: { path: located(join('.cache', 'cloudflare'), directory), ephemeralMode: 'e2e' } });

// --- [COMPOSITION] ---------------------------------------------------------------------

const react = () => import('@vitejs/plugin-react').then(({ default: viteReact }) => viteReact({ compiler: { panicThreshold: 'all_errors' } }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { hostedLayout, located, react };
