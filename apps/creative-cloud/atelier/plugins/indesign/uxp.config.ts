import { atelierBuild, atelierMark } from '@rasm/atelier/plugin.config';
import { InDesign } from '@rasm/indesign';
import { uxp } from '@rasm/uxp/vite';
import { panels } from '#/panels.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { ...InDesign.manifest, panels, mark: atelierMark, ...atelierBuild });
