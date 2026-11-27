import process from 'node:process';
import { spawn } from '@rasm/process';
import { layerProtocolChildProcess } from '@rasm/rpc';
import { Array, Cause, Context, Deferred, type Duration, Effect, Fiber, Layer, MutableRef, Number, Option, Path, Ref, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { RpcClient, type RpcClientError, RpcMessage } from 'effect/rpc';
import { ApplicationRpcs } from '#/contract';
import { LaunchEvent } from '#/installations';

// --- [CONSTANTS] -----------------------------------------------------------------------

const kCFStringEncodingUTF8 = 0x08_00_01_00;
const kCFURLPOSIXPathStyle = 0n;
const CORE_FOUNDATION = {
    CFURLCreateFromFileSystemRepresentation: { arguments: ['pointer', 'buffer', 'i64', 'bool'], return: 'pointer' },
    CFBundleCreate: { arguments: ['pointer', 'pointer'], return: 'pointer' },
    CFBundleCopyExecutableURL: { arguments: ['pointer'], return: 'pointer' },
    CFURLCopyAbsoluteURL: { arguments: ['pointer'], return: 'pointer' },
    CFURLCopyFileSystemPath: { arguments: ['pointer', 'i64'], return: 'pointer' },
    CFBundleGetValueForInfoDictionaryKey: { arguments: ['pointer', 'pointer'], return: 'pointer' },
    CFStringCreateWithCString: { arguments: ['pointer', 'string', 'u32'], return: 'pointer' },
    CFStringGetTypeID: { return: 'u64' },
    CFGetTypeID: { arguments: ['pointer'], return: 'u64' },
    CFStringGetLength: { arguments: ['pointer'], return: 'i64' },
    CFStringGetMaximumSizeForEncoding: { arguments: ['i64', 'u32'], return: 'i64' },
    CFStringGetCString: { arguments: ['pointer', 'buffer', 'i64', 'u32'], return: 'bool' },
    CFRelease: { arguments: ['pointer'] },
} as const;

// --- [SERVICES] ------------------------------------------------------------------------

class ApplicationServer extends Context.Service<ApplicationServer>()('@rasm/applications/client/ApplicationServer', {
    make: Effect.fnUntraced(function* (client: RpcClient.FromGroup<typeof ApplicationRpcs, RpcClientError.RpcClientError>, exited: ChildProcessSpawner.ChildProcessHandle['exitCode'], traffic: Effect.Effect<{ readonly requests: number; readonly bytes: number }>) {
        const [first, rest] = yield* Stream.peel(client.installations({}), Sink.head());
        const installations = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before its first value')));
        const discovering = yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(installations, value)));
        return {
            ...client,
            runLaunch: Effect.fnUntraced(function* <R>(launch: Omit<Parameters<typeof client.launch>[0], 'wait'>, wait: Option.Option<Duration.Input>, launching: (launched: typeof LaunchEvent.cases.launched.Type) => Effect.Effect<void, never, R>) {
                const { launched, ...outcome } = yield* client.launch({ ...launch, wait: Option.isSome(wait) }).pipe(
                    Stream.interruptWhen(Option.match(wait, { onNone: () => Effect.never, onSome: Effect.sleep })),
                    Stream.tap(LaunchEvent.matchOrElse({ launched: launching }, () => Effect.void)),
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
            currentInstallations: Effect.map(SubscriptionRef.get(installations), Struct.get('installations')),
            searchInstallations: (name: string) => client.installations({ name }).pipe(Stream.runHead, Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`Spotlight search for applications named ${name} ended before its first listing`))), Effect.map(Struct.get('installations'))),
            exitCode: Effect.raceFirst(exited, Effect.andThen(Fiber.join(discovering), exited)),
            traffic,
        };
    }),
}) {
    static readonly fromBundle = Effect.gen(function* () {
        const [path, started, { dlopen, getRawPointer, toString }] = yield* Effect.all([Path.Path, Deferred.make<ApplicationServer['Service']>(), Effect.promise(() => import('node:ffi'))]);
        const { functions: cf } = yield* Effect.acquireDisposable(Effect.sync(() => dlopen(null, CORE_FOUNDATION)));
        const owned = (subject: string, create: () => bigint) =>
            Effect.acquireRelease(
                Effect.sync(create).pipe(
                    Effect.filterOrFail(
                        (ref) => ref !== 0n,
                        () => new Cause.NoSuchElementError(subject),
                    ),
                ),
                (ref) => Effect.sync(() => cf.CFRelease(ref)),
            );
        const stringType = cf.CFStringGetTypeID();
        const text = (ref: bigint) =>
            Option.some(ref).pipe(
                Option.filter((held) => held !== 0n && cf.CFGetTypeID(held) === stringType),
                Option.map((held) => new Uint8Array(Number.Number(cf.CFStringGetMaximumSizeForEncoding(cf.CFStringGetLength(held), kCFStringEncodingUTF8)) + 1)),
                Option.filter((written) => cf.CFStringGetCString(ref, written, BigInt(written.byteLength), kCFStringEncodingUTF8) === 1),
                Option.flatMap((written) => Option.fromNullOr(toString(getRawPointer(written)))),
            );
        const location = path.join(process.execPath, '..', '..', '..');
        const representation = new TextEncoder().encode(location);
        const url = yield* owned(`${location} is no file system path`, () => cf.CFURLCreateFromFileSystemRepresentation(null, representation, BigInt(representation.byteLength), 1));
        const bundle = yield* owned(`${location} is no application bundle`, () => cf.CFBundleCreate(null, url));
        const info = (key: string) =>
            owned(`CFString ${key}`, () => cf.CFStringCreateWithCString(null, key, kCFStringEncodingUTF8)).pipe(
                Effect.map((name) => text(cf.CFBundleGetValueForInfoDictionaryKey(bundle, name))),
                Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`Info.plist of ${location} states no ${key} string`))),
            );
        const program = owned(`Info.plist of ${location} names no executable`, () => cf.CFBundleCopyExecutableURL(bundle)).pipe(
            Effect.flatMap((relative) => owned(`Executable URL of ${location} resolves against no base`, () => cf.CFURLCopyAbsoluteURL(relative))),
            Effect.flatMap((located) => owned(`Executable of ${location} has no file system path`, () => cf.CFURLCopyFileSystemPath(located, kCFURLPOSIXPathStyle))),
            Effect.map(text),
            Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`Executable path of ${location} is no string`))),
            Effect.filterOrFail(path.isAbsolute, (relative) => new Cause.NoSuchElementError(`Executable path ${relative} of ${location} is not absolute`)),
        );
        const { identifier, version, executable } = yield* Effect.all({ identifier: info('CFBundleIdentifier'), version: info('CFBundleShortVersionString'), executable: program });
        const name = path.basename(executable);
        const served = Effect.fnUntraced(function* (forceKillAfter: Duration.Input) {
            const [handle, requests, bytes] = yield* Effect.all([spawn([executable], { stderr: 'inherit' }), Effect.sync(() => MutableRef.make(0)), Ref.make(0)]);
            const traffic = Effect.all({ requests: Effect.sync(() => MutableRef.get(requests)), bytes: Ref.get(bytes) });
            const stdout = Stream.tap(handle.stdout, (chunk) => Ref.update(bytes, Number.sum(chunk.byteLength)));
            const linked = Effect.addFinalizer(() => Effect.ignore(handle.kill({ forceKillAfter }), { log: 'Error', message: `Stopping ${name} failed` })).pipe(
                Effect.andThen(RpcClient.make(ApplicationRpcs, { generateRequestId: () => requests.pipe(MutableRef.incrementAndGet, RpcMessage.RequestId) })),
                Effect.flatMap((client) => ApplicationServer.make(client, handle.exitCode, traffic)),
            );
            return Layer.effect(ApplicationServer, linked).pipe(Layer.provide(layerProtocolChildProcess({ stdin: handle.stdin, stdout }, Effect.match(handle.exitCode, { onFailure: ({ message }) => `${name} ended: ${message}`, onSuccess: (code) => `${name} exited with code ${code}` }))));
        });
        return {
            version,
            identifier,
            layer: (forceKillAfter: Duration.Input) => Layer.tap(Layer.unwrap(served(forceKillAfter)), (context) => Deferred.succeed(started, Context.get(context, ApplicationServer))),
            ended: Deferred.await(started).pipe(
                Effect.flatMap(({ exitCode }) => exitCode),
                Effect.flatMap((code) => new Cause.UnknownError(code, `${name} exited ${code}`)),
            ),
        };
    }).pipe(Effect.scoped);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationServer };
