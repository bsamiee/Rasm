import { cep } from '@rasm/cep/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = cep(import.meta.dirname, { script: './extendscript.ts', panel: '@rasm/atelier-protocol/panel' });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
