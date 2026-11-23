import process from 'node:process';
import { spawn } from '@rasm/process';
import { child } from '@rasm/rpc';
import { Cause, Context, Effect, Fiber, FileSystem, Formatter, Layer, Path, Result, Schema, SchemaIssue, Sink, Stream, SubscriptionRef } from 'effect';
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
        const watched = Effect.fnUntraced(function* <A, E>(subject: string, changes: Stream.Stream<A, E>) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
            return [latest, yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)))] as const;
        });
        const [installations, discovering] = yield* watched('installations', client.installations({}));
        return {
            ...client,
            launched: (launch: Parameters<typeof client.launch>[0]) =>
                client.launch(launch).pipe(
                    Stream.filterMapEffect(
                        Launching.matchOrElse(
                            {
                                failed: ({ failed }) => Effect.fail(new Cause.UnknownError(failed, [`${failed.subject} ${failed.reason}`, ...(failed.message === undefined ? [] : [failed.message])].join(': '))),
                                launched: ({ launched }) => Effect.succeed(Result.succeed(launched)),
                            },
                            () => Effect.succeed(Result.failVoid),
                        ),
                    ),
                    Stream.runLast,
                    Effect.flatMap(Effect.fromOption),
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
