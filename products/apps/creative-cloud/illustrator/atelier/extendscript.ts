import { serve } from '@rasm/atelier-protocol/extendscript';
import { containment, open, rasterize, session } from '@rasm/illustrator/extendscript';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve({ global: $.global, containment, session, open, rasterize });
