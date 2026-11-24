import { createServer } from 'node:http';
import process from 'node:process';
import { layer, layerHttpServices } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layerNodeFromConfig, teardown } from '@rasm/telemetry/node';
import { Array, Cause, Console, Effect, Layer, Schedule, Schema, Stdio } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import manifest from '#/.claude-plugin/plugin.json' with { type: 'json' };
import { HostError } from '#/host';
import { decide, Engine } from '#/rules/decide';
import { Decision, Request } from '#/rules/decision';
import { Writing, wrote } from '#/rules/git';
import { recorded, watched } from '#/rules/waits';

// --- [COMPOSITION] ---------------------------------------------------------------------

const routed = <A, I, B, O, E, R>(route: string, asked: Schema.Codec<A, I>, answered: Schema.Codec<B, O>, answer: (body: A) => Effect.Effect<B, E, R>) =>
    HttpRouter.route(
        'POST',
        `/${route}`,
        HttpServerRequest.schemaBodyJson(asked).pipe(
            Effect.flatMap(answer),
            Effect.flatMap(HttpServerResponse.schemaJson(answered)),
            Effect.tapCause(Effect.logError),
            Effect.catchCause((cause) => Effect.succeed(HttpServerResponse.text(Cause.pretty(cause), { status: 500 }))),
        ),
    );

const Router = HttpRouter.addAll([
    routed('decide', Request, Decision, decide),
    routed('read', Schema.Struct({ file: Schema.String }), Decision, ({ file }) => Effect.succeed(recorded(file))),
    routed('wrote', Writing, Schema.Struct({}), (body) => Effect.as(wrote(body), {})),
    routed(
        'tasks',
        Schema.Struct({ files: Schema.Array(Schema.String) }),
        Schema.Array(Schema.Struct({ file: Schema.String, live: Schema.Boolean, stuck: Schema.optionalKey(Schema.Struct({ key: Schema.String, text: Schema.String })) })).annotate({
            description: 'Each background task output file in request order: whether a process still writes it, and the evidence it loops on one line, keyed so one notice goes per kind',
        }),
        ({ files }) => watched(files),
    ),
]);

const Server = Layer.unwrap(
    Effect.gen(function* () {
        const { args } = yield* Stdio.Stdio;
        const socket = yield* Effect.fromOption(Array.head(yield* args));
        return HttpRouter.serve(Router, { disableListenLog: true, disableLogger: true }).pipe(
            Layer.provide(layer(createServer, { path: socket })),
            Layer.tap(() => Console.log(socket)),
        );
    }),
);

const orphaned = Effect.ignore(Effect.repeat(Effect.try({ try: () => process.kill(process.ppid, 0), catch: HostError.of('kill') }), Schedule.spaced('2 seconds')));

Effect.raceAllFirst([Layer.launch(Server), orphaned]).pipe(Effect.provide(Layer.provideMerge(Layer.provideMerge(Engine, layerHttpServices), layerNodeFromConfig({ resource: { serviceName: manifest.name, serviceVersion: manifest.version }, target: 'file' }))), runMain({ teardown }));
