import { execute, render } from '@rasm/illustrator/uxp';
import { Plugin, Uxp } from '@rasm/uxp/plugin';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.extension({ application: Uxp.application, execute, render });
