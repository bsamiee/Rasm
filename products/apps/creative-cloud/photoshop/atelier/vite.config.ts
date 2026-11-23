import { uxp } from '@rasm/uxp/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = uxp(import.meta.dirname);

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
