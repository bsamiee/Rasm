import { join } from 'node:path';
import { linked } from '@rasm/atelier-protocol/vite';
import { uxp } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../macos/Resources/AppIcon.icon/Assets/Atelier.svg') }, config: { plugins: [linked()] } });
