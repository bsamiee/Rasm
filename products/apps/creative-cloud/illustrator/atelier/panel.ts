import { bridge, peer, servePanel, takeImage } from '@rasm/cep/panel';
import { Content, Link } from '@rasm/mcp';
import { Failure, ObjectModel, Open, Rasterize } from '@rasm/object-model';
import { Context, Effect, Layer, Schema } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcGroup } from 'effect/rpc';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Reads = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Destructive, false));
const Tools = Toolkit.make(
    Tool.make('open', { description: 'Opens document in Illustrator with alerts off, returns its reference', parameters: Open, success: Open.Result, failure: Failure }).annotate(Tool.Destructive, false),
    Tool.make('rasterize', {
        description: 'Renders Illustrator document, artboard, or page item bounds to PNG with longestSide pixels on its longer side, options set ExportOptionsPNG24 fields',
        parameters: Rasterize,
        success: Schema.Struct({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }),
        failure: Failure,
    }).annotateMerge(Reads),
    Tool.make('object_model', { description: 'Lists Illustrator scripting classes and enumerations, or describes one by name', parameters: ObjectModel, success: ObjectModel.Result, failure: Failure }).annotateMerge(Reads),
);

servePanel({
    link: (cep) => {
        const host = bridge(cep);
        const handlers = Tools.toLayer({
            open: host(Tools.tools.open),
            rasterize: (request) => Effect.flatMap(host({ name: 'rasterize', parametersSchema: Rasterize, successSchema: Schema.String })(request), takeImage),
            object_model: host(Tools.tools.object_model),
        });
        return Link.dial(RpcGroup.make(), Link.serve(Tools).pipe(Layer.provide(handlers)), new URL('ws://localhost:7490'), peer(cep));
    },
});
