import { once } from 'node:events';
import { createServer } from 'node:net';
import { pathToFileURL } from 'node:url';
import { layer as sockets } from '@effect/platform-node/NodeSocketServer';
import { ApplicationRpcs, Event, Installation, LaunchEvent, InstanceSnapshot } from '@rasm/applications';
import { routed } from '@rasm/testing/playwright';
import { Array, Cause, Config, Deferred, Effect, FileSystem, Layer, MutableRef, Path, Schedule, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpClient, McpProtocol } from 'effect/ai';
import { HttpClient } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcSerialization, RpcServer } from 'effect/rpc';
import { build } from 'plist';

// --- [TYPES] ---------------------------------------------------------------------------

interface Bundle {
    readonly name: string;
    readonly identifier: string;
    readonly window?: string;
    readonly declared?: Readonly<Record<string, Schema.Json>>;
}
interface Logged {
    readonly identifier: string;
    readonly event: Event;
    readonly taken: Deferred.Deferred<void>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Bound = Schema.Struct({ port: Schema.Int });
const Installations = Schema.Array(Schema.toCodecJson(Installation));
const Instance = Schema.toCodecJson(InstanceSnapshot);
const Happened = Schema.toCodecJson(Event);
const Launch = Schema.toCodecJson(LaunchEvent);
const Text = Schema.Struct({ text: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const free = Effect.gen(function* () {
    const probe = yield* Effect.sync(() => createServer().listen(0, '127.0.0.1'));
    yield* Effect.promise(() => once(probe, 'listening'));
    const { port } = yield* Schema.decodeUnknownEffect(Bound)(probe.address());
    yield* Effect.callback<void>((resume) => {
        probe.close(() => resume(Effect.void));
    });
    return port;
});
const text = ({ content }: { readonly content: readonly unknown[] }) => Array.map(Array.getSomes(Array.map(content, (block) => Schema.decodeUnknownOption(Text)(block))), Struct.get('text')).join('\n');
const served = Effect.fnUntraced(function* (product: string, executable: string, bundles: readonly Bundle[], settings: readonly string[]) {
    const [fs, path, spawner, http, searched] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, HttpClient.HttpClient, Config.String('PATH')]);
    const built = yield* Effect.fromOption(routed('@rasm/nx-vite', product), () => new Cause.NoSuchElementError(`nx.json states no Vite outputPath to run ${product} from`));
    const [root, port] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'rasad-' }), free]);
    const [contents, socket] = [path.join(root, `${executable}.app`, 'Contents'), path.join(root, 'server.sock')];
    const located = (name: string) => pathToFileURL(path.join(root, 'Applications', `${name}.app/`)).href;
    const installations = yield* Schema.decodeEffect(Installations)(bundles.map(({ name, identifier, declared }) => ({ url: located(name), identifier, name, scriptable: false, activationPolicy: 'regular', ...declared })));
    const [running, log, launches] = [yield* SubscriptionRef.make<readonly InstanceSnapshot[]>([]), yield* SubscriptionRef.make<readonly Logged[]>([]), MutableRef.make<readonly string[]>([])];
    const reported = Effect.fnUntraced(function* (identifier: string, pid: number, title: string) {
        const [event, taken] = yield* Effect.all([Effect.orDie(Schema.decodeEffect(Happened)({ instance: `${pid}@1`, sequence: 1, at: '2026-11-25T09:30:00Z', change: { opened: { container: 7000 + pid, kind: 'window', title } } })), Deferred.make<void>()]);
        yield* SubscriptionRef.update(log, Array.append({ identifier, event, taken }));
        yield* Deferred.await(taken);
    });
    const launch = Effect.fnUntraced(function* ({ url }: { readonly url: URL }) {
        const bundle = yield* Effect.orDie(Effect.fromNullishOr(bundles.find(({ name }) => located(name) === url.href)));
        const pid = 400 + MutableRef.get(launches).length;
        yield* Effect.sync(() => MutableRef.update(launches, Array.append(url.href)));
        const row = yield* Effect.orDie(Schema.decodeEffect(Instance)({ pid, instance: `${pid}@1`, identifier: bundle.identifier, url: url.href, activationPolicy: 'regular', state: { running: { active: false, status: 'responding', windows: [] } }, listening: [] }));
        yield* SubscriptionRef.update(running, Array.append(row));
        yield* Effect.forEach(Array.fromNullishOr(bundle.window), (title) => reported(bundle.identifier, pid, title), { discard: true });
        return yield* Effect.orDie(Schema.decodeEffect(Launch)({ launched: { pid, instance: row.instance, identifier: bundle.identifier, created: true } }));
    });
    const fake = ApplicationRpcs.omit('pasteboard', 'model', 'intern', 'handlers', 'act', 'quit', 'dictionary', 'execute', 'capture', 'record', 'scale', 'diagnostics');
    yield* Layer.build(
        RpcServer.layer(fake).pipe(
            Layer.provide([
                fake.toLayer({
                    installations: ({ identifiers, prefix }) =>
                        Stream.concat(Stream.succeed({ unreadable: [], installations: installations.filter((each) => (identifiers === undefined && prefix === undefined) || identifiers?.includes(each.identifier) === true || (prefix !== undefined && each.identifier.startsWith(prefix))) }), Stream.never),
                    instances: ({ identifiers }) =>
                        Stream.map(
                            SubscriptionRef.changes(running),
                            Array.filter((row) => identifiers?.includes(row.identifier) !== false),
                        ),
                    events: ({ identifiers }) =>
                        SubscriptionRef.changes(log).pipe(
                            Stream.mapAccum(
                                () => 0,
                                (seen, entries) => [entries.length, entries.slice(seen)],
                            ),
                            Stream.filter(({ identifier }) => identifiers?.includes(identifier) !== false),
                            Stream.tap(({ taken }) => Deferred.succeed(taken, undefined)),
                            Stream.map(Struct.get('event')),
                        ),
                    observations: () => Stream.never,
                    pressure: () => Stream.never,
                    watch: ({ instance }) => Effect.flatMap(SubscriptionRef.get(running), (rows) => Effect.orDie(Effect.fromOption(Array.findFirst(rows, (row) => row.instance === instance)))),
                    launch: (payload) => Stream.fromEffect(launch(payload)),
                }),
                RpcServer.layerProtocolSocketServer,
            ]),
            Layer.provide([sockets({ path: socket }), RpcSerialization.layerNdJsonRpc()]),
        ),
    );
    const files = {
        [path.join(contents, 'Info.plist')]: build({ CFBundleExecutable: executable, CFBundleIdentifier: `app.rasm.${executable.toLowerCase()}.suite`, CFBundleShortVersionString: '0.0.0' }),
        [path.join(contents, 'MacOS', executable)]: `#!/bin/sh\nexec /usr/bin/nc -U '${socket}'\n`,
    };
    yield* Effect.forEach(Object.entries(files), ([file, body]) => Effect.andThen(fs.makeDirectory(path.dirname(file), { recursive: true }), fs.writeFileString(file, body)), { discard: true });
    yield* Effect.all([fs.chmod(path.join(contents, 'MacOS', executable), 0o755), fs.makeDirectory(path.join(contents, 'Helpers'))], { discard: true });
    yield* fs.link(path.join(built, 'server'), path.join(contents, 'Helpers', 'server'));
    const helper = yield* spawner.spawn(ChildProcess.make(path.join(contents, 'Helpers', 'server'), ['--listen', `127.0.0.1:${port}`, '--output-dir', path.join(root, 'output'), ...settings], { env: { HOME: path.join(root, 'home'), PATH: searched }, extendEnv: true, stdout: 'ignore', stderr: 'inherit' }));
    const url = `http://127.0.0.1:${port}/`;
    const connect = Effect.raceFirst(
        McpClient.make({ name: `${executable}-suite`, version: '1.0.0' }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url, protocol: McpProtocol.v2025_11_25 })), Effect.retry({ schedule: Schedule.spaced('100 millis') }), Effect.timeout('20 seconds')),
        Effect.flatMap(helper.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${product} exits ${code} before it answers MCP`))),
    ).pipe(Effect.provideService(HttpClient.HttpClient, http));
    return { url, connect, launches: Effect.sync(() => MutableRef.get(launches)) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Bundle, served, text };
