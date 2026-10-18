import { served } from '@rasm/indesign/panel';
import { launch } from '@rasm/uxp/runtime';

// --- [COMPOSITION] ---------------------------------------------------------------------

launch(served, { peer: 'Atelier', origin: import.meta.env.ATELIER });
