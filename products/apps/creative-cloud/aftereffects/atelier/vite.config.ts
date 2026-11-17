import { cep } from '@rasm/cep/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = cep(import.meta.dirname, { script: './extendscript.ts', panel: './panel.ts' });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
