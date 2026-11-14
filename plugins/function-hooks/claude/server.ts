import { createServer } from 'node:http';
import process from 'node:process';
import { layer, layerHttpServices } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { Array, Cause, Console, Effect, FileSystem, Function, Layer, Path, Schedule, type Schema, Stream } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { observed, quit, watched } from '#/claude/instances';
import { paused } from '#/claude/owner';
import { Routes } from '#/claude/routes';
import { Host } from '#/host';
import manifest from '#/manifest' with { type: 'json' };
import { decide, Engine } from '#/rules/decide';

// --- [COMPOSITION] ---------------------------------------------------------------------

const answering = <I, O, E, R>(route: keyof Routes, [input, output]: readonly [Schema.Decoder<I>, Schema.Codec<O, unknown>], respond: (value: I) => Effect.Effect<O, E, R>) =>
    HttpRouter.add(
        'POST',
        `/${route}`,
        HttpServerRequest.schemaBodyJson(input).pipe(
            Effect.flatMap(respond),
            Effect.flatMap(HttpServerResponse.schemaJson(output)),
            Effect.catchCause((cause) => Effect.succeed(HttpServerResponse.text(Cause.pretty(cause), { status: 500 }))),
        ),
    );

const Router = Layer.mergeAll(
    answering('decide', Routes.decide, decide),
    answering('observe', Routes.observe, Function.flow(observed, Effect.succeed)),
    answering('owners', Routes.owners, (names) =>
        Effect.map(
            Host.use((host) => host.installations(names)),
            Array.map(({ identifier }) => identifier),
        ),
    ),
    answering('paused', Routes.paused, paused),
    answering('quit', Routes.quit, quit),
    answering('watched', Routes.watched, Function.flow(watched, Effect.succeed)),
);

const Server = Layer.unwrap(
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const socket = path.join(yield* fs.makeTempDirectoryScoped({ prefix: `${manifest.name}-` }), 'decide.sock');
        return HttpRouter.serve(Router, { disableListenLog: true, disableLogger: true }).pipe(
            Layer.provide(layer(createServer, { path: socket })),
            Layer.tap(() => Console.log(socket)),
        );
    }),
);

const changed = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = path.join(import.meta.dirname, '..');
    yield* Stream.runHead(Stream.merge(fs.watch(root, { recursive: true }), fs.watch(path.join(root, '../../.gitignore'))));
});

const orphaned = Effect.suspend(() => {
    const parent = process.ppid;
    return Effect.repeat(
        Effect.sync(() => process.ppid),
        { schedule: Schedule.spaced('2 seconds'), until: (now) => now !== parent },
    );
});

Effect.raceAllFirst([Layer.launch(Server), changed, orphaned]).pipe(Effect.provide(Layer.provideMerge(Engine, layerHttpServices)), Effect.tapCause(Function.flow(Cause.pretty, Console.error)), runMain({ disableErrorReporting: true }));
