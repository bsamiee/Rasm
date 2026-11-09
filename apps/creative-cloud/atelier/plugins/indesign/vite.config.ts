import { atelierBuild, atelierMark } from '@rasm/atelier/plugin.config';
import { uxp } from '@rasm/uxp/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, { mark: atelierMark, ...atelierBuild });
