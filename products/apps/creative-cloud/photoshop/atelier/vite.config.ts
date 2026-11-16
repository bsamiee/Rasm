import { join } from 'node:path';
import { linked } from '@rasm/atelier-protocol/vite';
import { uxp } from '@rasm/uxp/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg') }, config: { plugins: [linked()] } });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
