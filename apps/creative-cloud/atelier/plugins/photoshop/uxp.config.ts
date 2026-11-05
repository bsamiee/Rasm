import { atelierLink, atelierMark } from '@rasm/atelier';
import { Photoshop } from '@rasm/photoshop';
import { uxp } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, Photoshop.manifest, atelierLink, atelierMark);
