import { AfterEffects } from '@rasm/aftereffects';
import { link } from '@rasm/atelier';
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, AfterEffects, link);
