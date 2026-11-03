import { Illustrator } from '@rasm/illustrator/contract';
import { cep } from '../../../../../vite.shared.ts';
import { link } from '../../link.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, Illustrator, link);
