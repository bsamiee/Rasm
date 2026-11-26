import { Description } from '@rasm/applications/source';
import { application, execute, Notifications, render, suppressDialogs, toolbar } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Layer, Stream, Struct } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({
    link: Plugin.dial({ application, ...Plugin.scripting({ execute, render }), source: Description.merge(Description.tools(toolbar), { changes: Stream.unwrap(Notifications.useSync(Struct.get('events'))) }), dialogs: suppressDialogs }).pipe(Layer.provide(Notifications.layer)),
});
