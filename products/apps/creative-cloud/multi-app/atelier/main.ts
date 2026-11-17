import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Application } from '@rasm/creative-cloud-desktop';
import { Typesetting } from '@rasm/indesign';
import { Link } from '@rasm/mcp';
import { serve } from '@rasm/mcp/node';
import { Script } from '@rasm/object-model';
import { teardown } from '@rasm/telemetry/node';
import { Array, Cause, Config, Duration, Effect, Equivalence, Layer, Option, Order, type Record, Ref, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { Flag } from 'effect/cli';
import { Host } from 'effect/net';
import { applications } from '#/applications';
import manifest from '#/package.json' with { type: 'json' };
import { type Runtime, scripts } from '#/scripts';
import { status } from '#/status';
import { typography } from '#/typography';

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.gen(function* () {
    const [server, answering, installed] = yield* Effect.all([ApplicationServer, status, Application.installed]);
    const catalog = yield* SubscriptionRef.make<readonly Application[]>(installed);
    const named = (peer: Link) =>
        Effect.map(SubscriptionRef.get(catalog), (rows) =>
            Option.getOrElse(
                Option.map(
                    Array.findFirst(rows, (row) => Link.hosted(row)(peer)),
                    Struct.get('name'),
                ),
                () => String(peer.application),
            ),
        );
    const links = yield* Link.listen(Script.merge(Typesetting));
    yield* Link.host(links, { name: named });
    yield* typography(links);
    const dictionaries = yield* Ref.make<Record.ReadonlyRecord<string, Option.Option<Runtime>>>({});
    const round = Effect.flatMap(SubscriptionRef.get(catalog), (rows) => Effect.all([applications(catalog), scripts(answering, links, dictionaries, rows)], { discard: true })).pipe(Effect.catchCause((cause) => Effect.logError('Tool registration failed, tools keep their last registration', cause)));
    const refreshed = Application.installed.pipe(
        Effect.flatMap((rows) => SubscriptionRef.set(catalog, rows)),
        Effect.catchCause((cause) => Effect.logError('Adobe application listing failed, tools keep the last listing', cause)),
    );
    yield* Effect.forkScoped(Stream.runForEach(Stream.drop(SubscriptionRef.changes(server.installations), 1), () => refreshed));
    const running = Stream.changesWith(
        Stream.map(SubscriptionRef.changes(server.instances), (rows) =>
            Array.sort(
                Array.map(
                    Array.filter(rows, ({ state }) => !State.isAnyOf(['exited'])(state)),
                    ({ url }) => url.href,
                ),
                Order.String,
            ),
        ),
        Array.makeEquivalence(Equivalence.String),
    );
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(Stream.merge(Stream.changesWith(SubscriptionRef.changes(catalog), Array.makeEquivalence(Schema.toEquivalence(Application))), links.changes), running), () => round));
});
Effect.gen(function* () {
    const { version, layer: helper } = yield* ApplicationServer.bundled;
    return yield* serve(
        { name: manifest.name, description: manifest.description, version },
        {
            listen: Flag.String('listen').pipe(Flag.withSchema(Schema.HostPortFromString), Flag.withDefault(Host.hostPortFromStringUnsafe('localhost:7490')), Flag.map(Option.some), Flag.withDescription('Endpoint as host:port or [IPv6]:port, a host name binds every address it resolves to')),
            hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
            target: 'file',
            executions: { name: 'executions', description: 'Refused state-changing calls that keep running', retention: Duration.hours(1) },
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
            instructions: ['Specifiers come from results', 'Linked extensions add tools while their application runs'],
        },
        Layer.effectDiscard(tools),
        ApplicationServer.pipe(
            Effect.flatMap(({ exited }) => exited),
            Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `Atelier helper exited ${code}`))),
        ),
    ).pipe(Effect.provide(helper));
}).pipe(Effect.provide(layer), runMain({ teardown }));
