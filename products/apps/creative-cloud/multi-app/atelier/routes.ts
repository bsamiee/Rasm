import { Installation, ProcessInstance } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { Route, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Context, Effect, Function, Option, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcGroup } from 'effect/rpc';
import { type Libraries, library, type listening } from '#/libraries';
import type { status } from '#/status';

// --- [TYPES] ---------------------------------------------------------------------------

type Tag = RpcGroup.Rpcs<typeof Libraries>['_tag'];
interface Running {
    readonly installation: Installation;
    readonly instance: typeof ProcessInstance.Type;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXECUTE_SCRIPT = 'execute_script' as const satisfies Tag;

// --- [OPERATIONS] ----------------------------------------------------------------------

const serves = (peers: readonly Peer[]) => (installation: Installation) => peers.some(Peer.serves(installation));
const linked = (tag: Tag) => (installation: Installation, peers: readonly Peer[]) => Option.filter(library(installation), ({ Contract }) => Contract.requests.has(tag) && serves(peers)(installation));

// --- [COMPOSITION] ---------------------------------------------------------------------

const routes = Effect.fnUntraced(function* ({ answering }: Effect.Success<typeof status>, links: Effect.Success<typeof listening>) {
    const server = yield* ApplicationServer;
    const current = Effect.all({ installed: server.installed, instances: SubscriptionRef.get(server.instances), peers: links.peers });
    const registered = yield* current;
    const connected = (installation: Installation) => Effect.map(links.call(Peer.serves(installation), `linked plugin of ${installation.name}`), (client) => ({ client }));
    const route = <Chosen extends Running>(holds: (peers: readonly Peer[]) => (installation: Installation) => boolean, kind: string, chosen: (running: Running, peers: readonly Peer[]) => Effect.Effect<Chosen, Error>) => {
        const listed = Schema.Literals(Array.dedupe(registered.installed.filter(holds(registered.peers)).map(Struct.get('name')))).annotate({ description: `Installed ${kind}` });
        const located = (absent: string) => ({
            application: Schema.OptionFromOptionalKey(listed.annotate({ description: `Application running instance, ${absent}` })),
            instance: Schema.OptionFromOptionalKey(ProcessInstance.annotate({ description: 'Instance value of a launch result or status completion, selects application when application is absent, required when bundle runs more than one instance' })),
        });
        const resolve = Effect.fnUntraced(function* (given: { readonly application: Option.Option<string>; readonly instance: Option.Option<typeof ProcessInstance.Type> }) {
            const { peers } = yield* current;
            const { installation, instance } = yield* server.select(holds(peers), kind)(given);
            return yield* chosen({ installation, instance }, peers);
        });
        const holding = Effect.map(current, ({ installed, peers }) => installed.filter(holds(peers)));
        const annotations = Context.make(Tool.OpenWorld, false).pipe(
            Context.add(
                Server.Visible,
                Effect.map(current, ({ installed, instances, peers }) => installed.some((installation) => holds(peers)(installation) && instances.some(Installation.runs(installation)))),
            ),
        );
        const depends = ({ instance }: Chosen) => Effect.succeed([answering(instance)]);
        return {
            listed,
            fields: located('or running in Dock, when absent'),
            resolve,
            depends,
            annotations,
            named: (given: string) =>
                Effect.flatMap(
                    holding,
                    Unmatched.single(`installed ${kind}`, Option.some(given), ({ name }) => [name]),
                ),
            completion: { application: () => Effect.map(holding, Array.map(Struct.get('name'))) },
            optional: (fallback: string) => Route.optional({ fields: located(`${fallback} when absent without instance`), resolve, depends, annotations }),
        };
    };
    return {
        application: route(() => Function.constTrue, 'Adobe application', Effect.succeed),
        dictionary: route((peers) => (installation) => installation.scriptable && !serves(peers)(installation), 'scriptable Adobe application without open link', Effect.succeed),
        script: {
            ...route(
                (peers) => (installation) => installation.scriptable || Option.isSome(linked(EXECUTE_SCRIPT)(installation, peers)),
                'Adobe application running scripts',
                (running, peers) => Effect.transposeOption(Option.map(linked(EXECUTE_SCRIPT)(running.installation, peers), (served) => Effect.map(connected(running.installation), Struct.assign({ library: served })))).pipe(Effect.bindTo('linked'), Effect.map(Struct.assign(running))),
            ),
            tag: EXECUTE_SCRIPT,
        },
        serving: <Served extends Tag>(tag: Served) => ({
            ...route(
                (peers) => (installation) => Option.isSome(linked(tag)(installation, peers)),
                `Adobe application serving ${tag}`,
                (running, peers) => Effect.fromOption(linked(tag)(running.installation, peers)).pipe(Effect.andThen(connected(running.installation)), Effect.map(Struct.assign(running))),
            ),
            tag,
            serves: ({ installation }: Pick<Running, 'installation'>) => Option.exists(library(installation), ({ Contract }) => Contract.requests.has(tag)),
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { routes };
