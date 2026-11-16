import { ExecuteScript, HostSession } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { application, execute, objectModel, open, rasterize, session } from '@rasm/photoshop/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Effect, Layer, Option, Schema } from 'effect';
import { Photoshop } from '#/contract';

// --- [COMPOSITION] ---------------------------------------------------------------------

const handlers = Layer.merge(
    Photoshop.toLayer({
        execute_script: ({ code, label }) => Effect.flatMap(execute(code, label), (result) => Schema.decodeUnknownEffect(ExecuteScript.Result)(Option.match(result, { onNone: () => ({}), onSome: (value) => ({ result: value }) }))),
        open: ({ file, label }) => open(file, label),
        rasterize: (payload) => rasterize(payload, payload.label),
        object_model: ({ name }) => objectModel(name),
    }),
    Layer.succeed(HostSession)((effect) => Effect.scoped(Effect.andThen(session, effect))),
);

Plugin.serve({ link: Peer.dial(Photoshop, handlers, application) });
