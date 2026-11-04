import { atelierLink } from '@rasm/atelier';
import { Photoshop } from '@rasm/photoshop';
import { uxp } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, Photoshop.manifest, atelierLink);
