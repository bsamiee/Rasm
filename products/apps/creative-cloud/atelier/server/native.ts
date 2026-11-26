import { Installation, type InstanceSnapshot } from '@rasm/applications/contract';
import { Workspace } from '@rasm/applications/workspace';
import type { Application } from '@rasm/creative-cloud-desktop/application';
import { Preferences } from '@rasm/creative-cloud-desktop/tools';
import { Link } from '@rasm/mcp/link';
import { Upstream } from '@rasm/mcp/upstream';
import { HOSTS } from '@rasm/uxp/manifest';
import { Array, Cause, Duration, Effect, Equivalence, Option, Record, Stream, SubscriptionRef } from 'effect';
import { RegistryApplications } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

interface NativeServer {
    readonly host: string;
    readonly ports: Array.NonEmptyReadonlyArray<number>;
    readonly path: string;
    readonly prefix: string;
    readonly authorization?: (row: Application) => Effect.Effect<string, Effect.Error<ReturnType<typeof Preferences.illustrator>> | Cause.NoSuchElementError, Effect.Services<ReturnType<typeof Preferences.illustrator>>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NATIVE: Record.ReadonlyRecord<string, NativeServer> = {
    [HOSTS.AI.product]: {
        host: '127.0.0.1',
        ports: Array.range(18_412, 18_422),
        path: '/v1/mcp',
        prefix: 'illustrator_',
        authorization: Effect.fnUntraced(function* (row: Application) {
            const server = Array.findFirst(yield* Preferences.illustrator(row), ({ plugin }) => Option.fromUndefinedOr(plugin.AIMCPServer));
            const { AuthToken } = yield* Effect.fromOption(server, () => new Cause.NoSuchElementError(`${row.name} preferences hold no AIMCPServer AuthToken, which its MCP server writes once enabled`));
            return `Bearer ${AuthToken}`;
        }),
    },
    [HOSTS.aftereffects.product]: { host: '127.0.0.1', ports: [3100, 3000], path: '/aftereffects/mcp', prefix: 'aftereffects_' },
    [HOSTS.premierepro.product]: { host: '127.0.0.1', ports: [3000], path: '/premiere/mcp', prefix: 'premiere_' },
    [HOSTS.ame.product]: { host: '[::1]', ports: [3001], path: '/ame/mcp', prefix: 'mediaencoder_' },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const listeningPort =
    (row: Pick<Application, 'url'>, { ports }: Pick<NativeServer, 'ports'>) =>
    (instances: readonly InstanceSnapshot[]) =>
        Option.flatMap(Array.findFirst(instances, Installation.runs(row)), ({ listening }) => Array.findFirst(ports, (port) => listening.includes(port)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const native = Stream.unwrap(
    Effect.gen(function* () {
        const [rows, { running, blocking }] = yield* Effect.all([RegistryApplications, Workspace]);
        const proxy = (row: Application, entry: NativeServer) =>
            Upstream.fromHttp({
                peer: Link.make({ application: row.url, plugin: 'mcp' }),
                prefix: entry.prefix,
                endpoint: Effect.gen(function* () {
                    const port = yield* Effect.fromOption(listeningPort(row, entry)(yield* SubscriptionRef.get(running)), () => new Cause.NoSuchElementError(`${row.name} listens on none of ${entry.ports.join(', ')}`));
                    const authorization = yield* Effect.transposeOption(Option.fromUndefinedOr(entry.authorization?.(row)));
                    return { url: new URL(`http://${entry.host}:${port}${entry.path}`), headers: Record.getSomes({ authorization }) };
                }),
                presence: Stream.map(SubscriptionRef.changes(running), (instances) => Option.isSome(listeningPort(row, entry)(instances))),
                annotations: blocking(row),
                poll: Duration.minutes(1),
            });
        return SubscriptionRef.changes(rows).pipe(
            Stream.map((held) => held.filter((row) => Record.has(NATIVE, row.product))),
            Stream.changesWith(Array.makeEquivalence(Equivalence.make((left: Application, right: Application) => left.url.href === right.url.href && left.version === right.version))),
            Stream.map(Array.flatMap((row) => Option.toArray(Option.map(Record.get(NATIVE, row.product), (entry) => proxy(row, entry))))),
            Stream.switchMap(Array.match({ onEmpty: () => Stream.succeed([]), onNonEmpty: (upstreams) => Stream.map(Stream.zipLatestAll(...upstreams), Array.flatten) })),
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { native };
