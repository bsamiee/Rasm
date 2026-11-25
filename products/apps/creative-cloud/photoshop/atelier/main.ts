import { application, events, execute, render } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.link({ application, execute, render, events });
