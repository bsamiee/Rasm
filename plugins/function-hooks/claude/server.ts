import { createServer } from 'node:http';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import process from 'node:process';
import { layer, layerHttpServices } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layerNodeFromConfig, teardown } from '@rasm/telemetry/node';
import { Array, Cause, Console, Effect, Layer, Option, Schedule, type Schema, Stdio, String } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { paused } from '#/claude/owner';
import { Routes } from '#/claude/routes';
import { Host } from '#/host';
import manifest from '#/manifest' with { type: 'json' };
import { decide, Engine } from '#/rules/decide';
import { readiness } from '#/rules/tasks';

// --- [COMPOSITION] ---------------------------------------------------------------------

const answering = <I, O, E, R>(route: keyof Routes, [input, output]: readonly [Schema.Decoder<I>, Schema.Codec<O, unknown>], respond: (value: I) => Effect.Effect<O, E, R>) =>
    HttpRouter.add(
        'POST',
        `/${route}`,
        HttpServerRequest.schemaBodyJson(input).pipe(
            Effect.flatMap(respond),
            Effect.flatMap(HttpServerResponse.schemaJson(output)),
            Effect.tapCause(Effect.logError),
            Effect.catchCause((cause) => Effect.succeed(HttpServerResponse.text(Cause.pretty(cause), { status: 500 }))),
        ),
    );

const Router = Layer.mergeAll(answering('decide', Routes.decide, decide), answering('paused', Routes.paused, paused), answering('ready', Routes.ready, readiness));

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
    yield* Effect.ignore(
        Effect.repeat(
            Effect.try(() => process.kill(owner, 0)),
            Schedule.spaced('2 seconds'),
        ),
    );
    yield* Effect.sync(() => watching && process.kill(process.ppid, 'SIGTERM'));
});

Effect.raceAllFirst([Layer.launch(Server), orphaned]).pipe(Effect.provide(Layer.provideMerge(Layer.provideMerge(Engine, layerHttpServices), layerNodeFromConfig({ resource: { serviceName: manifest.name, serviceVersion: manifest.version }, target: 'file' }))), runMain({ teardown }));
