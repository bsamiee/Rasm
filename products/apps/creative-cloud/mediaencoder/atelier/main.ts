import { Description } from '@rasm/applications/description';
import { events, execute, MediaEncoder } from '@rasm/mediaencoder/uxp';
import { EvaluationRpcs } from '@rasm/object-model/rpcs';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Effect, Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({
    link: Plugin.dial({
        application: Uxp.application,
        group: EvaluationRpcs,
        handlers: EvaluationRpcs.toLayer({ evaluate: ({ code }) => Effect.map(execute(code), (result) => ({ result })) }),
        source: Description.merge(events.renderQueue, events.watchFolder),
        dialogs: Effect.void,
    }).pipe(Layer.provide(MediaEncoder.layer)),
});
