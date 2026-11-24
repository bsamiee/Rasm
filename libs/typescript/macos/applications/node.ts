import process from 'node:process';
import { spawn } from '@rasm/process';
import { child } from '@rasm/rpc';
import { Array, Cause, Context, Effect, Fiber, FileSystem, Formatter, Layer, Path, Schema, SchemaIssue, Sink, Stream, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { RpcClient } from 'effect/rpc';
import { parse } from 'plist';
import { Automation, Launching } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this Node helper' });

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/node/ApplicationServer', {
    make: Effect.fnUntraced(function* (handle: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Automation);
        const [first, rest] = yield* Stream.peel(client.installations({}), Sink.head());
        const installations = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before its first value')));
        const discovering = yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(installations, value)));
        return {
            ...client,
            launched: (launch: Parameters<typeof client.launch>[0]) =>
                client.launch(launch).pipe(
                    Stream.mapEffect(Launching.matchOrElse({ failed: ({ failed }) => new Cause.UnknownError(failed, [`${failed.subject} ${failed.reason}`, ...Array.fromNullishOr(failed.message)].join(': ')) }, Effect.succeed)),
                    Stream.filter(Launching.guards.launched),
                    Stream.runLast,
                    Effect.flatMap(Effect.fromOption),
                    Effect.map(({ launched }) => launched),
                ),
            installations,
            installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult),
            exited: Effect.raceFirst(handle.exitCode, Effect.andThen(Fiber.join(discovering), handle.exitCode)),
        };
    }),
}) {
    static readonly layer = <E, R>(spawned: Effect.Effect<ChildProcessSpawner.ChildProcessHandle, E, R>) => Layer.unwrap(Effect.map(spawned, (handle) => Layer.effect(ApplicationServer, ApplicationServer.make(handle)).pipe(Layer.provideMerge(child(handle)))));
    static readonly bundled = Effect.gen(function* () {
        const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const contents = path.join(path.dirname(process.execPath), '..');
        const { CFBundleExecutable, CFBundleIdentifier, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
            Effect.flatMap((text) => Effect.try({ try: () => parse(text), catch: (cause) => new Schema.SchemaError(new SchemaIssue.InvalidValue({ message: `Info.plist holds no property list: ${Formatter.format(cause)}` }, text)) })),
            Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
        );
        return { version: CFBundleShortVersionString, identifier: CFBundleIdentifier, layer: ApplicationServer.layer(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' })) };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
