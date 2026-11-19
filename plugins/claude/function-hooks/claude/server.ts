import { createServer } from 'node:http';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import process from 'node:process';
import { layer, layerHttpServices } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layerNodeFromConfig, teardown } from '@rasm/telemetry/node';
import { Array, Cause, Console, Effect, Layer, Option, Schedule, type Schema, Stdio, String } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Routes } from '#/claude/routes';
import { Host, HostError } from '#/host';
import manifest from '#/manifest' with { type: 'json' };
import { decide, Engine } from '#/rules/decide';
import { watched } from '#/rules/waits';

// --- [COMPOSITION] ---------------------------------------------------------------------

const routed = <K extends keyof typeof Routes, E, R>(route: K, answer: (body: (typeof Routes)[K][0]['Type']) => Effect.Effect<(typeof Routes)[K][1]['Type'], E, R>) => {
    const [asked, answered]: readonly [Schema.Codec<(typeof Routes)[K][0]['Type'], (typeof Routes)[K][0]['Encoded']>, Schema.Codec<(typeof Routes)[K][1]['Type'], (typeof Routes)[K][1]['Encoded']>] = Routes[route];
    return HttpRouter.route(
        'POST',
        `/${route}`,
        HttpServerRequest.schemaBodyJson(asked).pipe(
            Effect.flatMap(answer),
            Effect.flatMap(HttpServerResponse.schemaJson(answered)),
            Effect.tapCause(Effect.logError),
            Effect.catchCause((cause) => Effect.succeed(HttpServerResponse.text(Cause.pretty(cause), { status: 500 }))),
        ),
    );
};

const Router = HttpRouter.addAll([routed('decide', decide), routed('tasks', ({ files }) => watched(files))]);

const Server = Layer.unwrap(
    Effect.gen(function* () {
        const { args } = yield* Stdio.Stdio;
        const socket = Option.getOrElse(Array.head(yield* args), () => join(tmpdir(), `${manifest.name}-${process.pid}.sock`));
        return HttpRouter.serve(Router, { disableListenLog: true, disableLogger: true }).pipe(
            Layer.provide(layer(createServer, { path: socket })),
            Layer.tap(() => Console.log(socket)),
        );
    }),
);

const orphaned = Effect.gen(function* () {
    const watching = process.channel !== undefined;
    const owner = watching ? Number(String.trim(yield* Host.use((host) => host.output(['ps', '-o', 'ppid=', '-p', `${process.ppid}`])))) : process.ppid;
    yield* Effect.ignore(Effect.repeat(Effect.try({ try: () => process.kill(owner, 0), catch: HostError.of('kill') }), Schedule.spaced('2 seconds')));
    yield* Effect.sync(() => watching && process.kill(process.ppid, 'SIGTERM'));
});

Effect.raceAllFirst([Layer.launch(Server), orphaned]).pipe(Effect.provide(Layer.provideMerge(Layer.provideMerge(Engine, layerHttpServices), layerNodeFromConfig({ resource: { serviceName: manifest.name, serviceVersion: manifest.version }, target: 'file' }))), runMain({ teardown }));
