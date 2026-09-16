// --- [IMPORTS] -------------------------------------------------------------------------

import { Clock, Crypto, Duration, Effect, Layer, Match, Option, Record, Ref, Schema } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { BridgeError } from './errors.ts';
import { Link } from './frames.ts';
import { Hosts, Resolved } from './hosts.ts';
import { InFlight, Jobs } from './jobs.ts';
import { probe as osascript } from './osascript.ts';
import manifest from './package.json' with { type: 'json' };
import { Links, linkState, probe as socket } from './socket.ts';
import { HostId } from './values.ts';

// --- [TOOL] ----------------------------------------------------------------------------

const _health = Tool.make('health', {
    description: 'One row per host: the resolved host table row, the link state, a live probe over its channel, and the in-flight job',
    parameters: Schema.Struct({ host: Schema.OptionFromOptionalKey(HostId) }),
    success: Schema.Struct({
        server: Schema.Struct({ name: Schema.String, version: Schema.String, uptimeSeconds: Schema.Number }),
        hosts: Schema.Array(
            Schema.Struct({
                host: Resolved,
                link: Schema.OptionFromNullOr(Link),
                probe: Schema.toCodecJson(Schema.Result(Schema.Json, BridgeError)),
                inFlight: Schema.OptionFromNullOr(InFlight),
                lastSuccessAt: Schema.OptionFromNullOr(Schema.Number),
                lastError: Schema.OptionFromNullOr(Schema.Struct({ error: BridgeError, count: Schema.Int, secondsAgo: Schema.Number })),
            }),
        ),
    }),
    dependencies: [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto],
});

const _toolkit = Toolkit.make(_health);

// --- [ROWS] ----------------------------------------------------------------------------

const _row = Effect.fnUntraced(function* (links: Links, jobs: Jobs, host: Resolved, now: number) {
    const fields = Match.value(host).pipe(
        Match.discriminatorsExhaustive('channel')({
            osascript: (row) => ({ probe: osascript(row.id, row.bundleId, jobs[row.id]), link: Effect.succeedNone }),
            socket: (row) => ({ probe: socket(links[row.id], jobs[row.id]), link: Effect.map(linkState(links[row.id]), Option.some) }),
        }),
    );
    const probe = yield* fields.probe;
    const [link, activity] = yield* Effect.all([fields.link, Ref.get(jobs[host.id])]);
    return {
        host,
        link,
        probe,
        inFlight: activity.inFlight,
        lastSuccessAt: activity.lastSuccessAt,
        lastError: Option.map(activity.lastError, ({ error, count, at }) => ({ error, count, secondsAgo: Duration.toSeconds(Duration.millis(now - at)) })),
    };
});

const _answer = Effect.fnUntraced(function* (startedAt: number, hosts: Hosts, links: Links, jobs: Jobs, selection: Option.Option<HostId>) {
    const now = yield* Clock.currentTimeMillis;
    const rows = yield* Effect.forEach(Option.match(selection, { onNone: () => Record.values(hosts), onSome: (id) => [hosts[id]] }), (host) => _row(links, jobs, host, now), {
        concurrency: 'unbounded',
    });
    return { server: { name: manifest.name, version: manifest.version, uptimeSeconds: Duration.toSeconds(Duration.millis(now - startedAt)) }, hosts: rows };
});

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Hosts | Links | Jobs | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto> = Layer.provide(
    McpServer.toolkit(_toolkit),
    _toolkit.toLayer(
        Effect.map(Effect.all([Clock.currentTimeMillis, Hosts, Links, Jobs]), ([startedAt, hosts, links, jobs]) =>
            _toolkit.of({ health: (params) => _answer(startedAt, hosts, links, jobs, params.host) }),
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
