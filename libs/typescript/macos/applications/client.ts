import process from 'node:process';
import { spawn } from '@rasm/process';
import { child } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, type Duration, Effect, Fiber, FileSystem, Formatter, flow, Layer, Option, Path, Schema, SchemaIssue, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { RpcClient } from 'effect/rpc';
import { parse } from 'plist';
import { Automation, Launching } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this Node helper' });

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/client/ApplicationServer', {
    make: Effect.fnUntraced(function* (handle: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Automation);
        const [first, rest] = yield* Stream.peel(client.installations({}), Sink.head());
        const installations = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before its first value')));
        const discovering = yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(installations, value)));
        return {
            ...client,
            launched: (launch: Omit<Parameters<typeof client.launch>[0], 'wait'>, wait: Option.Option<Duration.Input>) =>
                client.launch({ ...launch, wait: Option.isSome(wait) }).pipe(
                    Stream.mapEffect(Launching.matchOrElse({ failed: ({ failed }) => new Cause.UnknownError(failed, [`${failed.subject} ${failed.reason}`, ...Array.fromNullishOr(failed.message)].join(': ')) }, Effect.succeed)),
                    Stream.interruptWhen(Option.match(wait, { onNone: () => Effect.never, onSome: Effect.sleep })),
                    Stream.runCollect,
                    Effect.flatMap((events) =>
                        Effect.map(Effect.fromOption(Array.findLast(events, Launching.guards.launched)), ({ launched }) => ({
                            ...launched,
                            state: Option.map(Array.findLast(events, Launching.guards.state), flow(Struct.get('state'), Struct.get('state'))),
                            opened: Option.map(Array.findLast(events, Launching.guards.opened), Struct.get('opened')),
                        })),
                    ),
                ),
            installations,
            installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult),
            exited: Effect.raceFirst(handle.exitCode, Effect.andThen(Fiber.join(discovering), handle.exitCode)),
        };
    }),
}) {
    static readonly layer = <E, R>(spawned: Effect.Effect<ChildProcessSpawner.ChildProcessHandle, E, R>) => Layer.unwrap(Effect.map(spawned, (handle) => Layer.effect(ApplicationServer, ApplicationServer.make(handle)).pipe(Layer.provideMerge(child(handle)))));
    static readonly bundled = Effect.gen(function* () {
        const [path, files, started] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Deferred.make<ApplicationServer['Service']>()]);
        const contents = path.join(path.dirname(process.execPath), '..');
        const { CFBundleExecutable, CFBundleIdentifier, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
            Effect.flatMap((text) => Effect.try({ try: () => parse(text), catch: (cause) => new Schema.SchemaError(new SchemaIssue.InvalidValue({ message: `Info.plist holds no property list: ${Formatter.format(cause)}` }, text)) })),
            Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
        );
        return {
            version: CFBundleShortVersionString,
            identifier: CFBundleIdentifier,
            layer: Layer.tap(ApplicationServer.layer(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' })), (context) => Deferred.succeed(started, Context.get(context, ApplicationServer))),
            ended: Deferred.await(started).pipe(
                Effect.flatMap(({ exited }) => exited),
                Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `${CFBundleExecutable} exited ${code}`))),
            ),
        };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
