// --- [IMPORTS] -------------------------------------------------------------------------

import { Clock, Crypto, Duration, Effect, Layer, Match, Option, Path, Queue, Record, Ref, Schema, Struct } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError } from './errors.ts';
import { Link } from './frames.ts';
import { Hosts, Resolved } from './hosts.ts';
import { InFlight, Jobs, liveness, Process, probe } from './jobs.ts';
import { read } from './osascript.ts';
import { dispatch, Links, linkState } from './socket.ts';
import { HostId, JobId, PROBE_MS } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Server {
    readonly name: string;
    readonly version: string;
}

// --- [TOOL] ----------------------------------------------------------------------------

const _health = Tool.make('health', {
    description: 'One row per host: the resolved host table row, the link state, the process and its load, a live probe over its channel, the queue depth, and the in-flight job',
    parameters: Schema.Struct({ host: Schema.OptionFromOptionalKey(HostId) }),
    success: Schema.Struct({
        server: Schema.Struct({ name: Schema.String, version: Schema.String, uptimeSeconds: Schema.Number }),
        hosts: Schema.Array(
            Schema.Struct({
                host: Resolved,
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
    }),
    dependencies: [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, Path.Path],
}).annotate(Tool.Readonly, true);

const _toolkit = Toolkit.make(_health);

// --- [ROWS] ----------------------------------------------------------------------------

const _row = Effect.fnUntraced(function* (links: Links, jobs: Jobs, host: Resolved, ago: (at: number) => number) {
    const fields = Match.value(host).pipe(
        Match.discriminatorsExhaustive('channel')({
            osascript: (row) => ({ probe: probe(jobs[row.id], () => read(row.id, row.bundleId, PROBE_MS, 'get version', Option.none())), link: Effect.succeedNone }),
            socket: (row) => ({
                probe: probe(jobs[row.id], (jobId) =>
                    Effect.map(dispatch(links[row.id], { jobId, kind: 'execute', body: '1', suspendHistory: Option.none(), commandName: Option.none() }), Struct.get('value')),
                ),
                link: Effect.map(linkState(links[row.id]), Option.some),
            }),
        }),
    );
    const process = yield* Effect.result(liveness(jobs[host.id]));
    const probed = yield* fields.probe;
    const [link, queueDepth, activity] = yield* Effect.all([fields.link, Queue.size(jobs[host.id].queue), Ref.get(jobs[host.id].activity)]);
    return {
        host,
        link,
        process,
        probe: probed,
        queueDepth,
        inFlight: activity.inFlight,
        wedged: Option.map(activity.wedged, ({ jobId, at }) => ({ jobId, secondsAgo: ago(at) })),
        lastSuccessSecondsAgo: Option.map(activity.lastSuccessAt, ago),
        lastError: Option.map(activity.lastError, ({ error, count, at }) => ({ error, count, secondsAgo: ago(at) })),
    };
});

const _answer = Effect.fnUntraced(function* (server: Server, startedAt: number, hosts: Hosts, links: Links, jobs: Jobs, selection: Option.Option<HostId>) {
    const now = yield* Clock.currentTimeMillis;
    const ago = (at: number): number => Duration.toSeconds(Duration.millis(now - at));
    const rows = yield* Effect.forEach(Option.match(selection, { onNone: () => Record.values(hosts), onSome: (id) => [hosts[id]] }), (host) => _row(links, jobs, host, ago), {
        concurrency: 'unbounded',
    });
    return { server: { ...server, uptimeSeconds: ago(startedAt) }, hosts: rows };
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer = (server: Server): Layer.Layer<never, never, Hosts | Links | Jobs | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | Path.Path> =>
    Layer.provide(
        McpServer.toolkit(_toolkit),
        _toolkit.toLayer(
            Effect.map(Effect.all([Clock.currentTimeMillis, Hosts, Links, Jobs]), ([startedAt, hosts, links, jobs]) =>
                _toolkit.of({ health: (params) => _answer(server, startedAt, hosts, links, jobs, params.host) }),
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
