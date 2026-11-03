import { InDesign } from '@rasm/indesign/contract';
import { Record } from 'effect';
import { panels } from '#/panels';
import { uxp } from '../../../../../vite.shared.ts';
import { link } from '../../link.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { ...InDesign.manifest, entrypoints: Record.collect(panels, (id, label) => ({ type: 'panel' as const, id, label: { default: label } })) }, link);
