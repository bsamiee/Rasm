import indesign from 'adobe:indesign';
import { server } from '@rasm/indesign/contract';
import { script } from '@rasm/uxp/entrypoints';

// --- [COMPOSITION] ---------------------------------------------------------------------

script(server(indesign));
