import { AfterEffects } from '@rasm/aftereffects';
import { atelierLink } from '@rasm/atelier';
import { cep } from '@rasm/cep/vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, AfterEffects, atelierLink);
