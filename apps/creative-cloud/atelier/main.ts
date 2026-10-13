import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime } from '@effect/platform-node';
import { Executions, publish, routed } from '@rasm/ai/extension';
import { Galley } from '@rasm/galley/extension';
import { Array, Config, Effect, Layer, Logger, Struct } from 'effect';
import { McpProtocol, McpServer } from 'effect/ai';
import { HttpRouter } from 'effect/http';
import { Atelier, applications, font } from './applications.ts';
import { execute } from './execute.ts';
import manifest from './package.json' with { type: 'json' };
import { Scripting } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extensions = [Galley];

const served = Layer.effectDiscard(
    Effect.gen(function* () {
        const [routing, folded] = yield* Effect.all([applications(extensions), routed([...extensions, Atelier])]);
        yield* publish(folded.verbs, routing);
        yield* execute(folded, routing);
        yield* font(folded, routing);
    }),
).pipe(
    Layer.provide(Layer.mergeAll(Atelier.layer, ...Array.map(extensions, Struct.get('layer')))),
    Layer.provide(Layer.merge(Scripting.layer, Executions.layer('adobe://executions'))),
    Layer.provide(
        McpServer.layerHttp({
            name: manifest.name,
            version: manifest.version,
            path: '/',
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18],
            allowSessionTermination: true,
            instructions: Array.join(Array.map([Atelier, ...extensions], Struct.get('instructions')), '\n'),
        }),
    ),
);

HttpRouter.serve(served, { disableLogger: true }).pipe(
    Layer.provide(
        NodeHttpServer.layerConfig(
            createServer,
            Config.map(Config.URL('ATELIER_URL'), ({ hostname, port }) => ({ host: hostname, port: Number(port) })),
        ),
    ),
    Layer.launch,
    Effect.provideService(Logger.LogToStderr, true),
    NodeRuntime.runMain,
);
