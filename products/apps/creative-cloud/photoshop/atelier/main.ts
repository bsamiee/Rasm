import { SourceRpcs } from '@rasm/applications/source';
import { application, describe, execute, Notifications, operate, render } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Layer, Stream, Struct } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const scripting = Plugin.scripting({ execute, render });

Plugin.serve({
    link: Plugin.dial({
        application,
        group: scripting.group.merge(SourceRpcs),
        toolkit: scripting.toolkit,
        handlers: Layer.merge(scripting.handlers, SourceRpcs.toLayer({ describe, operate })),
        events: Stream.unwrap(Notifications.useSync(Struct.get('events'))),
    }).pipe(Layer.provide(Notifications.layer)),
});
