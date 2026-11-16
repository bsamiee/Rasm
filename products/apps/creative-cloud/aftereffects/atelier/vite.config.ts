import { linked } from '@rasm/atelier-protocol/vite';
import { cep } from '@rasm/cep/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = cep(import.meta.dirname, { script: './extendscript.ts', panel: '@rasm/atelier-protocol/panel', config: { plugins: [linked()] } });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
