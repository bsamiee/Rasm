import { atelierLink } from '@rasm/atelier';
import { cep } from '@rasm/cep/vite';
import { Illustrator } from '@rasm/illustrator';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, Illustrator, atelierLink);
