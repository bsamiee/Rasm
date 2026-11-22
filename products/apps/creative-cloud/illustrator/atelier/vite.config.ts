import { join } from 'node:path';
import { uxp } from '@rasm/uxp/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = uxp(import.meta.dirname, join(import.meta.dirname, '../../multi-app/atelier/icons/mark.svg'));

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
