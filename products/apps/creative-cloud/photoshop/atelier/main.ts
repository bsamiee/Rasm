import { application, execute, notices, render } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.extension({ application, execute, render, notices });
