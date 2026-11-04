import { atelierLink } from '@rasm/atelier';
import { Illustrator } from '@rasm/illustrator';
import { cep } from '../../../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, Illustrator, atelierLink);
