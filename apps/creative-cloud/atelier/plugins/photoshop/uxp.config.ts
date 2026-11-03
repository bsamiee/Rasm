import { Photoshop } from '@rasm/photoshop/contract';
import { uxp } from '../../../../../vite.shared.ts';
import { link } from '../../link.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, Photoshop.manifest, link);
