import { join } from 'node:path';
import { uxp } from '@rasm/uxp/vite';
import type { UserConfigFnPromise } from 'vite';
import { ATELIER } from '#/tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg') }, network: [ATELIER.origin] });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
