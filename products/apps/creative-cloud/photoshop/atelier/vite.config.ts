import { join } from 'node:path';
import { Peer } from '@rasm/atelier-protocol/peer';
import { uxp } from '@rasm/uxp/vite';
import type { UserConfigFnPromise } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config: UserConfigFnPromise = uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg') }, network: [Peer.link.url.origin] });

// --- [EXPORTS] -------------------------------------------------------------------------

export default config;
