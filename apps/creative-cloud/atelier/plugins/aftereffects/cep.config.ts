import { AfterEffects } from '@rasm/aftereffects';
import { atelierBuild } from '@rasm/atelier/plugin.config';
import { cep } from '@rasm/cep/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, { ...AfterEffects, ...atelierBuild });
