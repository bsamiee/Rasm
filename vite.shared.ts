import { join, relative } from 'node:path';
import viteReact from '@vitejs/plugin-react';

// --- [CONSTANTS] -----------------------------------------------------------------------

const e2eMode = 'e2e';
const ROOT = import.meta.dirname;

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (folder: string, directory: string) => join(ROOT, folder, relative(ROOT, directory));
const hostedLayout = (directory: string) => ({ nodeOutDir: located('.artifacts', directory), state: { path: located(join('.cache', 'cloudflare'), directory), ephemeralMode: e2eMode } });

// --- [COMPOSITION] ---------------------------------------------------------------------

const react = () => viteReact({ compiler: { panicThreshold: 'all_errors' } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { e2eMode, hostedLayout, located, react };
