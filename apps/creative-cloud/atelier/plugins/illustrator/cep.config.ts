import { atelierBuild } from '@rasm/atelier/plugin.config';
import { cep } from '@rasm/cep/vite';
import { Illustrator } from '@rasm/illustrator';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default cep(import.meta.dirname, { ...Illustrator, ...atelierBuild(import.meta.dirname) });
