import { AfterEffects } from '@rasm/aftereffects';
import { atelierLink } from '@rasm/atelier';
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, AfterEffects, atelierLink);
