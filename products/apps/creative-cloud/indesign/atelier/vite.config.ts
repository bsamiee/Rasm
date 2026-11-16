import { join } from 'node:path';
import { Peer } from '@rasm/atelier-protocol/peer';
import { uxp } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg'), panels: { typesetting: join(import.meta.dirname, 'typesetting.svg') } }, network: [Peer.link.url.origin] });
