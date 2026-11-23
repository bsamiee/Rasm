import { chrome } from '@rasm/vite/chrome';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = chrome(import.meta.dirname);

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
