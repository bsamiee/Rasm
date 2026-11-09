import { linked } from '@rasm/atelier-protocol/vite';
import { cep } from '@rasm/cep/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, { script: './extendscript.ts', panel: '@rasm/atelier-protocol/panel', config: { plugins: [linked()] } });
