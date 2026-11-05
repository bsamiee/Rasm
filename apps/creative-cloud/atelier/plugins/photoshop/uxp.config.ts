import { atelierBuild, atelierMark } from '@rasm/atelier/plugin.config';
import { Photoshop } from '@rasm/photoshop';
import { uxp } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { ...Photoshop.manifest, mark: atelierMark, ...atelierBuild(import.meta.dirname) });
