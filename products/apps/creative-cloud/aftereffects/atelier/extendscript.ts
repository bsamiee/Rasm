import { containment, open, rasterize, session, undo } from '@rasm/aftereffects/extendscript';
import { serve } from '@rasm/atelier-protocol/extendscript';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve({ global: $.global, containment, base: 1, session, undo, open, rasterize });
