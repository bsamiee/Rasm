import process from 'node:process';
import { spawn } from '@rasm/process';
import { layerProtocolChildProcess } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, type Duration, Effect, Fiber, FileSystem, Formatter, Layer, Option, Path, Schema, SchemaGetter, SchemaIssue, Sink, Stream, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { RpcClient, type RpcClientError } from 'effect/rpc';
import { parse } from 'plist';
import { ApplicationRpcs, LaunchEvent } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ message: `Info.plist holds no property list: ${Formatter.format(cause)}` }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String })),
).annotate({ description: 'Info.plist of the app bundle holding this Node helper' });

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/client/ApplicationServer', {
    make: Effect.fnUntraced(function* (client: RpcClient.FromGroup<typeof ApplicationRpcs, RpcClientError.RpcClientError>, exited: ChildProcessSpawner.ChildProcessHandle['exitCode']) {
        const [first, rest] = yield* Stream.peel(client.installations({}), Sink.head());
        const installations = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before its first value')));
        const discovering = yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(installations, value)));
        return {
            ...client,
            runLaunch: Effect.fnUntraced(function* (launch: Omit<Parameters<typeof client.launch>[0], 'wait'>, wait: Option.Option<Duration.Input>) {
                const { launched, ...outcome } = yield* client.launch({ ...launch, wait: Option.isSome(wait) }).pipe(
                    Stream.interruptWhen(Option.match(wait, { onNone: () => Effect.never, onSome: Effect.sleep })),
                    Stream.runFold(
                        () => ({
                            launched: Option.none<typeof LaunchEvent.cases.launched.Type>(),
                            state: Option.none<(typeof LaunchEvent.cases.state.Type)['state']>(),
                            opened: Option.none<(typeof LaunchEvent.cases.opened.Type)['reply']>(),
                            failed: Array.empty<typeof LaunchEvent.cases.failed.Type>(),
                        }),
                        (held, event) =>
                            LaunchEvent.match(event, {
                                launched: (started) => ({ ...held, launched: Option.some(started) }),
                                state: ({ state }) => ({ ...held, state: Option.some(state) }),
                                opened: ({ reply }) => ({ ...held, opened: Option.some(reply) }),
                                failed: (failure) => ({ ...held, failed: Array.append(held.failed, failure) }),
                            }),
                    ),
                );
                const refusal = outcome.failed.map(({ subject, reason, message }) => [`${subject} ${reason}`, ...Array.fromNullishOr(message)].join(': '));
                const instance = yield* Effect.fromOption(launched, () => new Cause.UnknownError(outcome.failed, Array.isReadonlyArrayNonEmpty(refusal) ? refusal.join('; ') : `launch of ${launch.url.href} ended before an instance launched`));
                return { ...instance, ...outcome };
            }),
            installations,
            currentInstallations: Effect.map(SubscriptionRef.get(installations), ({ installations: listed }) => listed),
            exitCode: Effect.raceFirst(exited, Effect.andThen(Fiber.join(discovering), exited)),
        };
    }),
}) {
    static readonly fromBundle = Effect.gen(function* () {
        const [path, files, started] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Deferred.make<ApplicationServer['Service']>()]);
        const contents = path.join(process.execPath, '..', '..');
        const { CFBundleExecutable, CFBundleIdentifier, CFBundleShortVersionString } = yield* Effect.flatMap(files.readFileString(path.join(contents, 'Info.plist')), Schema.decodeEffect(Bundle));
        const served = Effect.map(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' }), (handle) =>
            Layer.effect(
                ApplicationServer,
                Effect.flatMap(RpcClient.make(ApplicationRpcs), (client) => ApplicationServer.make(client, handle.exitCode)),
            ).pipe(Layer.provide(layerProtocolChildProcess(handle))),
        );
        return {
            version: CFBundleShortVersionString,
            identifier: CFBundleIdentifier,
            layer: Layer.tap(Layer.unwrap(served), (context) => Deferred.succeed(started, Context.get(context, ApplicationServer))),
            ended: Deferred.await(started).pipe(
                Effect.flatMap(({ exitCode }) => exitCode),
                Effect.flatMap((code) => new Cause.UnknownError(code, `${CFBundleExecutable} exited ${code}`)),
            ),
        };
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
