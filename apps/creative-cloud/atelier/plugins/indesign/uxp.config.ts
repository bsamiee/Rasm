import { atelierLink } from '@rasm/atelier';
import { InDesign } from '@rasm/indesign';
import { Record } from 'effect';
import { panels } from '#/panels';
import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { ...InDesign.manifest, entrypoints: Record.collect(panels, (id, label) => ({ type: 'panel', id, label: { default: label } })) }, atelierLink);
