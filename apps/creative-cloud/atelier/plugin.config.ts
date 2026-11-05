import { join } from 'node:path';
import { atelierLink } from '#/atelier-link';
import { located } from '../../../vite.shared.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const atelierMark = join(import.meta.dirname, 'macos/Resources/AppIcon.icon/Assets/Atelier.svg');

// --- [OPERATIONS] ----------------------------------------------------------------------

const atelierBuild = (directory: string) => ({ link: atelierLink, config: { build: { outDir: located('.artifacts', directory) } } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { atelierBuild, atelierMark };
