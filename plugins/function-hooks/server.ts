import { createServer } from 'node:http';
import process from 'node:process';
import { layer, layerHttpServices } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { Array, Cause, Console, Effect, FileSystem, Function, Layer, Option, Path, Predicate, Ref, Schedule, type Schema } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Routes } from '#/decision';
import { Host } from '#/host';
import { observed, quit, watched } from '#/instances';
import { decide, live } from '#/policies/rules';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = ['.', 'shell', 'policies'];

// --- [COMPOSITION] ---------------------------------------------------------------------

const answering = <I, O, R>(route: keyof Routes, [input, output]: readonly [Schema.Codec<I>, Schema.Codec<O>], respond: (value: I) => Effect.Effect<O, never, R>) =>
    HttpRouter.add('POST', `/${route}`, HttpServerRequest.schemaBodyJson(input).pipe(Effect.flatMap(respond), Effect.flatMap(HttpServerResponse.schemaJson(output))));

const Router = Layer.mergeAll(answering('decide', Routes.decide, decide), answering('observe', Routes.observe, observed), answering('quit', Routes.quit, quit), answering('watched', Routes.watched, watched));

const Server = Layer.unwrap(
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const socket = path.join(yield* fs.makeTempDirectoryScoped({ prefix: 'function-hooks-' }), 'decide.sock');
        return HttpRouter.serve(Router, { disableListenLog: true, disableLogger: true }).pipe(
            Layer.provide(layer(createServer, { path: socket })),
            Layer.tap(() => Console.log(socket)),
        );
    }),
);

const entries = ({ fs, path }: typeof Host.Service, folder: string) =>
    Effect.map(
        fs.readDirectory(path.join(import.meta.dirname, folder)),
        Array.map((name) => path.join(import.meta.dirname, folder, name)),
    );

const stamp = Effect.gen(function* () {
    const host = yield* Host;
    const files = yield* Effect.forEach(SOURCES, (folder) => entries(host, folder));
    const stats = yield* Effect.forEach([...files.flat(), host.path.join(import.meta.dirname, '../../.gitignore')], (file) => Effect.option(host.fs.stat(file)));
    return Array.getSomes(stats)
        .map(({ mtime }) => `${Option.getOrUndefined(mtime)?.getTime()}`)
        .join(' ');
});

const decidesProbe = Host.use(({ run, path }) => Effect.map(run(['node', '--import', path.join(import.meta.dirname, 'instances.ts'), path.join(import.meta.dirname, 'codex.ts')], { stdin: '{"cwd":"/","tool_input":{"command":"echo ok"}}' }), ({ code, stdout }) => code === 0 && stdout.trim() === ''));

const untilStale = Effect.gen(function* () {
    const host = process.ppid;
    const seen = yield* Ref.make(yield* stamp);
    const stale = Effect.gen(function* () {
        const now = yield* stamp;
        const changed = now !== (yield* Ref.getAndSet(seen, now));
        return process.ppid !== host || (changed && (yield* decidesProbe));
    });
    yield* Effect.repeat(stale, { schedule: Schedule.spaced('2 seconds'), until: Predicate.isTruthy });
});

Effect.raceAllFirst([Layer.launch(Server), untilStale]).pipe(Effect.provide(Layer.provideMerge(live, layerHttpServices)), Effect.tapCause(Function.flow(Cause.pretty, Console.error)), runMain({ disableErrorReporting: true }));
