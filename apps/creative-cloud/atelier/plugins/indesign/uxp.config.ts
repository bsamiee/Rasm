import { atelierLink, atelierMark } from '@rasm/atelier';
import { InDesign } from '@rasm/indesign';
import { uxp } from '@rasm/uxp/vite';
import { Record } from 'effect';
import { panels } from '#/panels';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { ...InDesign.manifest, entrypoints: Record.collect(panels, (id, label) => ({ type: 'panel', id, label: { default: label } })) }, atelierLink, atelierMark);
