import { bridge, peer, servePanel, takeImage } from '@rasm/cep/panel';
import { Content, Link } from '@rasm/mcp';
import { Failure, Open, Rasterize } from '@rasm/object-model';
import { Context, Effect, Layer, Schema } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import { RpcGroup } from 'effect/rpc';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Reads = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Destructive, false));
const Tools = Toolkit.make(
    Tool.make('open', { description: 'Opens project file in After Effects with dialogs suppressed, replacing the open project, returns its reference', parameters: Open, success: Open.Result, failure: Failure }),
    Tool.make('rasterize', {
        description: 'Renders current frame of After Effects composition to PNG at the finest resolution factor up to 99 keeping its longer side within longestSide pixels, takes no options',
        parameters: Rasterize,
        success: Schema.Struct({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }),
        failure: Failure,
    }).annotateMerge(Reads),
);

servePanel({
    link: (cep) => {
        const host = bridge(cep);
        const handlers = Tools.toLayer({
            open: host(Tools.tools.open),
            rasterize: (request) => Effect.flatMap(host({ name: 'rasterize', parametersSchema: Rasterize, successSchema: Schema.String })(request), takeImage),
        });
        return Link.dial(RpcGroup.make(), Link.serve(Tools).pipe(Layer.provide(handlers)), new URL('ws://localhost:7490'), peer(cep));
    },
});
