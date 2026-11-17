import { join } from 'node:path';
import { uxp } from '@rasm/uxp/vite';
import { ATELIER } from '#/tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg'), panels: { typesetting: join(import.meta.dirname, 'typesetting.svg') } }, network: [ATELIER.origin] });
