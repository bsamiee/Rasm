import { AfterEffects, execute, render, suppressDialogs, toolbar } from '@rasm/aftereffects/uxp';
import { Description } from '@rasm/applications/source';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application: Uxp.application, ...Plugin.scripting({ execute, render }), source: Description.tools(toolbar), dialogs: suppressDialogs }).pipe(Layer.provide(AfterEffects.layer)) });
