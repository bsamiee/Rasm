import { createServer } from 'node:http';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { Automation } from '@rasm/applications/node';
import { ExecuteScript, HostError, Requests } from '@rasm/automation';
import { Peer } from '@rasm/automation/peer';
import { UxpManifest } from '@rasm/uxp';
import { Array, Context, Deferred, Effect, Equal, FileSystem, Layer, Option, Path, Record, Result, Schema, Stream, Struct } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { NetAddress } from 'effect/net';
import vite from 'vite/package.json' with { type: 'json' };
import type { Outcome } from '#/cases/results';
import { JsonObject } from '#/cases/values';
import { HarnessError } from '#/harness-error';
import { type Cased, LABEL, type Live, type Services } from '#/session/harness';
import { checked } from '#/session/instance';
import { record } from '#/trace/records';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel = Extract<Cased, { readonly _tag: 'Panel' }>;
interface Route {
    readonly load: (live: Live, folder: string) => Effect.Effect<void, Error, Services>;
    readonly show: (live: Live, plugin: string, panel: string) => Effect.Effect<void, Error, Services>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOOPBACK = NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, 0);
const VITE = fileURLToPath(new URL(vite.bin.vite, import.meta.resolve('vite/package.json')));
const LISTENING = Peer.listen(Requests, { path: '*', middleware: Layer.empty });

// --- [MODELS] --------------------------------------------------------------------------

const Read = Schema.Struct({ read: JsonObject, failed: Schema.Record(Schema.String, Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = Effect.fn('built')(function* (config: string, port: number, scratch: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const project = path.dirname(config);
    const folder = path.join(scratch, `plugin-${port}`, path.basename(project));
    yield* checked(process.execPath, [VITE, 'build', project, '--configLoader', 'native', '--config', config, '--outDir', folder, '--logLevel', 'warn'], { PRODUCT_BUNDLE_IDENTIFIER: `${LABEL}.${port}`, LINK_PORT: `${port}` });
    const { id } = yield* Effect.flatMap(fs.readFileString(path.join(folder, UxpManifest.file)), Schema.decodeEffect(UxpManifest.json));
    return { config, folder, id };
});
const reading = (reads: Panel['read']) => ['const read = {}, failed = {};', ...Record.collect(reads, (key, expression) => `try { read[${JSON.stringify(key)}] = (${expression}); } catch (error) { failed[${JSON.stringify(key)}] = String(error?.message ?? error); }`), 'return { read, failed };'].join('\n');
const completed = (error: Error): (typeof Outcome.Type)['completion'] =>
    Schema.is(HostError)(error) && HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? { _tag: 'Thrown', message: error.subject, ...Record.getSomes({ line: Option.map(error.reason.line, (line) => line - 1) }) } : { _tag: 'Failed', reason: error.message };

const linked = Effect.fn('linked')(function* (live: Live, configs: Array.NonEmptyReadonlyArray<string>, route: Route) {
    const [path, automation, listened] = yield* Effect.all([Path.Path, Automation, Deferred.make<Effect.Success<typeof LISTENING>>()]);
    const served = HttpRouter.serve(Layer.effectDiscard(Effect.flatMap(LISTENING, (held) => Deferred.succeed(listened, held))), { disableLogger: true, disableListenLog: true });
    const context = yield* Layer.build(Layer.provideMerge(served, layerServer(createServer, { host: NetAddress.formatHost(LOOPBACK), port: LOOPBACK.port })));
    const { address } = Context.get(context, HttpServer.HttpServer);
    const { port } = yield* Effect.fromOption(Option.liftPredicate(address, NetAddress.isInetAddress), () => HarnessError.unavailable('link server')(`listens on ${NetAddress.formatUrlUnsafe(address)}`));
    const links = yield* Deferred.await(listened);
    const application = yield* Effect.flatMap(path.toFileUrl(live.chosen.app), (url) => Schema.decodeEffect(Peer.fields.application)(url.href));
    const plugins = yield* Effect.forEach(configs, (config) => built(config, port, live.scratch));
    const ids = new Set(plugins.map(Struct.get('id')));
    yield* Effect.forEach(plugins, ({ folder, id }) => Effect.andThen(route.load(live, folder), record('Info', 'plugin loaded', { plugin: id, folder, port })), { discard: true });
    const client = yield* Effect.cached(
        Stream.runHead(Stream.filter(links.changes, (peers) => new Set(peers.filter(Peer.serves(application)).map(Struct.get('extension'))).isSupersetOf(ids))).pipe(
            Effect.andThen(Effect.forEach(plugins, ({ id }) => record('Info', 'plugin linked', { plugin: id }), { discard: true })),
            Effect.andThen(links.call(application)),
        ),
    );
    const called = (body: string) =>
        client.pipe(
            Effect.flatMap((invoke) => invoke('execute_script', ExecuteScript.make({ code: ExecuteScript.expression(body, false) }))),
            Effect.map(({ result }) => Option.getOrNull(result)),
        );
    const reads = (item: Panel) => Effect.flatMap(called(reading(item.read)), Schema.decodeUnknownEffect(Read));
    const panelled = Effect.fn('panelled')(function* (item: Panel, opened: readonly Schema.Json[]) {
        const plugin = yield* Effect.fromOption(
            Array.findFirst(plugins, ({ config }) => config === item.plugin),
            () => HarnessError.unavailable(item.name)(`plugin ${item.plugin} holds no build`),
        );
        yield* route.show(live, plugin.id, item.panel);
        const before = yield* reads(item);
        const evaluated = yield* Effect.result(called(`return (${item.evaluate});`));
        yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(item.until), (until) => Effect.repeat(called(`return Boolean(${until});`), { until: Equal.equals(true) })));
        const after = yield* reads(item);
        const failed = item.capture === true ? Record.getFailures(Record.mapKeys(yield* automation.screenshot({ pid: live.pid, directory: yield* path.toFileUrl(item.folder), window: Option.none(), longestSide: Option.none() }), (window) => `${window}.png`)) : {};
        return {
            name: item.name,
            opened,
            before: before.read,
            beforeFailed: before.failed,
            read: after.read,
            readFailed: { ...after.failed, ...Record.map(failed, Struct.get('message')) },
            ...Result.match(evaluated, { onSuccess: (value) => ({ completion: { _tag: 'Ran' as const }, value }), onFailure: (error) => ({ completion: completed(error) }) }),
        };
    });
    return { panelled };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { linked, type Panel, type Route };
