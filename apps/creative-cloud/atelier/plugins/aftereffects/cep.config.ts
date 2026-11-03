import { AfterEffects } from '@rasm/aftereffects/contract';
import { cep } from '../../../../../vite.shared.ts';
import { link } from '../../link.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, AfterEffects, link);
