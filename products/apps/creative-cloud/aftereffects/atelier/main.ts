import { execute, render } from '@rasm/aftereffects/uxp';
import { Plugin, Uxp } from '@rasm/uxp/plugin';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.link({ application: Uxp.application, execute, render });
