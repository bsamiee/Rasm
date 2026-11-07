import { join } from 'node:path';
import { atelierLink } from '#/atelier-link.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const atelierMark = join(import.meta.dirname, 'macos/Resources/AppIcon.icon/Assets/Atelier.svg');
const atelierBuild = { link: atelierLink } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { atelierBuild, atelierMark };
