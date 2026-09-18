// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Clock, Crypto, Duration, Effect, Layer, Match, Option, Path, Queue, Ref, Result, Schema, Struct } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError } from './errors.ts';
import { Link } from './frames.ts';
import { Bundle, Hosts, installed } from './hosts.ts';
import { InFlight, Jobs, liveness, Process, probe } from './jobs.ts';
import { read } from './osascript.ts';
import manifest from './package.json' with { type: 'json' };
import { Links, linkState, probing } from './socket.ts';
import { HOSTS, HostId, JobId, PROBE_MS } from './values.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER: Pick<typeof manifest, 'name' | 'version'> = Struct.pick(manifest, ['name', 'version']);

// --- [MODELS] --------------------------------------------------------------------------

const Health: Schema.Struct<{
    readonly server: Schema.Struct<{ readonly name: Schema.String; readonly version: Schema.String; readonly uptimeSeconds: Schema.Number }>;
    readonly hosts: Schema.$Array<
        Schema.Struct<{
            readonly host: Schema.toCodecJson<Schema.Result<typeof Bundle, typeof BridgeError>>;
            readonly link: Schema.OptionFromNullOr<typeof Link>;
            readonly process: Schema.toCodecJson<Schema.Result<typeof Process, typeof BridgeError>>;
            readonly probe: Schema.toCodecJson<Schema.Result<Schema.Codec<Schema.Json>, typeof BridgeError>>;
            readonly queueDepth: Schema.Int;
            readonly inFlight: Schema.OptionFromNullOr<typeof InFlight>;
            readonly wedged: Schema.OptionFromNullOr<Schema.Struct<{ readonly jobId: typeof JobId; readonly secondsAgo: Schema.Number }>>;
            readonly lastSuccessSecondsAgo: Schema.OptionFromNullOr<Schema.Number>;
            readonly lastError: Schema.OptionFromNullOr<Schema.Struct<{ readonly error: typeof BridgeError; readonly count: Schema.Int; readonly secondsAgo: Schema.Number }>>;
        }>
    >;
}> = Schema.Struct({
    server: Schema.Struct({ name: Schema.String, version: Schema.String, uptimeSeconds: Schema.Number }),
    hosts: Schema.Array(
        Schema.Struct({
            host: Schema.toCodecJson(Schema.Result(Bundle, BridgeError)),
            link: Schema.OptionFromNullOr(Link),
            process: Schema.toCodecJson(Schema.Result(Process, BridgeError)),
            probe: Schema.toCodecJson(Schema.Result(Schema.Json, BridgeError)),
            queueDepth: Schema.Int,
            inFlight: Schema.OptionFromNullOr(InFlight),
            wedged: Schema.OptionFromNullOr(Schema.Struct({ jobId: JobId, secondsAgo: Schema.Number })),
            lastSuccessSecondsAgo: Schema.OptionFromNullOr(Schema.Number),
            lastError: Schema.OptionFromNullOr(Schema.Struct({ error: BridgeError, count: Schema.Int, secondsAgo: Schema.Number })),
        }),
    ),
});

// --- [TOOL] ----------------------------------------------------------------------------

const _toolkit = Toolkit.make(
    Tool.make('health', {
        description: 'Returns link, process, queue, and live probe status for every host, or for `host` alone',
        parameters: Schema.Struct({ host: Schema.OptionFromOptionalKey(HostId) }),
        success: Health,
        dependencies: [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, Path.Path, Hosts, Links, Jobs],
    }).annotate(Tool.Readonly, true),
);

// --- [ROWS] ----------------------------------------------------------------------------

const _row = Effect.fnUntraced(function* (id: HostId, ago: (at: number) => number) {
    const [hosts, links, jobs] = yield* Effect.all([Hosts, Links, Jobs]);
    const host = jobs[id];
    const fields = Match.value(HOSTS[id]).pipe(
        Match.discriminatorsExhaustive('transport')({
            osascript: (row) => ({
                probe: probe(host, () => Effect.flatMap(installed(row.id, hosts[row.id]), (found) => read(row.id, found.bundleId, PROBE_MS, 'get version', Option.none()))),
                link: Effect.succeedNone,
            }),
            socket: (row) => ({ probe: probing({ link: links[row.id], host }), link: Effect.map(linkState(links[row.id]), Option.some) }),
        }),
    );
    const [resolved, process] = yield* Effect.all([Effect.result(installed(id, hosts[id])), Effect.result(liveness(host))]);
    const probed = yield* fields.probe;
    const [link, queueDepth, activity] = yield* Effect.all([fields.link, Queue.size(host.queue), Ref.get(host.activity)]);
    return {
        host: Result.map(resolved, Struct.pick(Struct.keys(Bundle.fields))),
        link,
        process,
        probe: probed,
        queueDepth,
        inFlight: activity.inFlight,
        wedged: Option.map(activity.wedged, ({ jobId, startedAt }) => ({ jobId, secondsAgo: ago(startedAt) })),
        lastSuccessSecondsAgo: Option.map(activity.lastSuccessAt, ago),
        lastError: Option.map(activity.lastError, ({ error, count, at }) => ({ error, count, secondsAgo: ago(at) })),
    };
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Hosts | Links | Jobs | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | Path.Path> = Layer.provide(
    McpServer.toolkit(_toolkit),
    _toolkit.toLayer(
        Effect.map(Clock.currentTimeMillis, (startedAt) =>
            _toolkit.of({
                health: Effect.fnUntraced(function* ({ host: selection }) {
                    const now = yield* Clock.currentTimeMillis;
                    const ago = (at: number): number => Duration.toSeconds(Duration.millis(now - at));
                    const rows = yield* Effect.forEach(Option.match(selection, { onNone: () => HostId.literals, onSome: Array.of }), (id) => _row(id, ago), { concurrency: 'unbounded' });
                    return { server: { ...SERVER, uptimeSeconds: ago(startedAt) }, hosts: rows };
                }),
            }),
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Health, layer, SERVER };
