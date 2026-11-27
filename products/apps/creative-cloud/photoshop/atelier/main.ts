import { Description } from '@rasm/applications/description';
import { application, events, execute, Notifications, render, suppressDialogs, toolbar } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application, ...Plugin.scripting({ execute, render }), source: Description.tools(toolbar), events, dialogs: suppressDialogs }).pipe(Layer.provide(Notifications.layer)) });
