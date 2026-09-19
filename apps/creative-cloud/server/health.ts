// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Clock, Crypto, Duration, Effect, type FileSystem, Layer, Match, Option, Path, Result, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { register } from './contract.ts';
import { BridgeError } from './errors.ts';
import { InFlight, Link, Outcome } from './frames.ts';
import { Bundle, Hosts, installed } from './hosts.ts';
import { Jobs, liveness, Process, probe, request } from './jobs.ts';
import { read } from './osascript.ts';
import manifest from './package.json' with { type: 'json' };
import { activity, Links, linkState, outcome, probing } from './socket.ts';
import { HOSTS, HostId, JobId, PROBE_MS, SOCKETS } from './values.ts';

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
        description:
            'Returns link, process, queue, and live probe status for every host, or for `host` alone. With both `host` and `jobId`, reads the retained native operation outcome without probing or dispatching work. Outcomes remain available within the authority process and its bounded recent-operation window; unknown means no retained result, and pluginDetached means native completion is indeterminate',
        parameters: Schema.Union([
            Schema.Struct({ _tag: Schema.tagDefaultOmit('outcome'), host: Schema.Literals(Struct.keys(SOCKETS)), jobId: JobId }),
            Schema.Struct({ _tag: Schema.tagDefaultOmit('health'), host: Schema.OptionFromOptionalKey(HostId), jobId: Schema.optionalKey(Schema.Never) }),
        ]),
        success: Schema.Union([Health, Schema.toCodecJson(Outcome)]),
        failure: Schema.toCodecJson(BridgeError),
        dependencies: [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, Path.Path, Hosts, Links, Jobs],
    }).annotate(Tool.Readonly, true),
);

// --- [ROWS] ----------------------------------------------------------------------------

const _row = Effect.fnUntraced(function* (id: HostId) {
    const [hosts, links, jobs] = yield* Effect.all([Hosts, Links, Jobs]);
    const host = jobs[id];
    const [resolved, process] = yield* Effect.all([Effect.result(installed(id, hosts[id])), Effect.result(liveness(host))]);
    const entry = yield* request(PROBE_MS);
    const fields = Match.value(HOSTS[id]).pipe(
        Match.discriminatorsExhaustive('transport')({
            osascript: (row) => ({
                probe: probe(
                    host,
                    entry,
                    Effect.flatMap(Effect.fromResult(resolved), (found) => read(row.id, found.bundlePath, PROBE_MS, 'get version', Option.none())),
                ),
                link: Effect.succeedNone,
                activity: SubscriptionRef.get(host.activity),
            }),
            socket: (row) => ({
                probe: probing({ link: links[row.id], host }, entry, Option.none()),
                link: Effect.map(linkState(links[row.id]), Option.some),
                activity: activity({ link: links[row.id], host }),
            }),
        }),
    );
    const probed = yield* fields.probe;
    const [link, state] = yield* Effect.all([fields.link, fields.activity]);
    const now = yield* Clock.currentTimeMillis;
    const ago = (at: number): number => Duration.toSeconds(Duration.millis(now - at));
    return {
        host: Result.map(resolved, Struct.pick(Struct.keys(Bundle.fields))),
        link,
        process,
        probe: probed,
        queueDepth: state.queueDepth,
        inFlight: state.inFlight,
        wedged: Option.map(state.wedged, ({ jobId, startedAt }) => ({ jobId, secondsAgo: ago(startedAt) })),
        lastSuccessSecondsAgo: Option.map(state.lastSuccessAt, ago),
        lastError: Option.map(state.lastError, ({ error, count, at }) => ({ error, count, secondsAgo: ago(at) })),
    };
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Hosts | Links | Jobs | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path> = Layer.provide(
    register(_toolkit),
    _toolkit.toLayer(
        Effect.map(Clock.currentTimeMillis, (startedAt) =>
            _toolkit.of({
                health: Effect.fnUntraced(function* (input) {
                    if (input._tag === 'outcome') {
                        return yield* Links.use((links) => outcome(links[input.host], input.jobId));
                    }
                    const rows = yield* Effect.forEach(Option.match(input.host, { onNone: () => HostId.literals, onSome: Array.of }), _row, { concurrency: 'unbounded' });
                    return { server: { ...SERVER, uptimeSeconds: Duration.toSeconds(Duration.millis((yield* Clock.currentTimeMillis) - startedAt)) }, hosts: rows };
                }),
            }),
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Health, layer, SERVER };
