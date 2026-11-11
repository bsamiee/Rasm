import { Installation, ProcessInstance, type RunningInstance } from '@rasm/applications';
import { Peer } from '@rasm/atelier-protocol/peer';
import { Route, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Context, Effect, Function, Match, Option, Predicate, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcGroup } from 'effect/rpc';
import { Adobe } from '#/adobe';
import { type Libraries, library, type listening } from '#/libraries';
import type { status } from '#/status';

// --- [TYPES] ---------------------------------------------------------------------------

type Tag = RpcGroup.Rpcs<typeof Libraries>['_tag'];
type Holds = (peers: readonly Peer[]) => (installation: Installation) => boolean;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXECUTE_SCRIPT = 'execute_script' as const satisfies Tag;

// --- [OPERATIONS] ----------------------------------------------------------------------

const names = (installation: Installation) => Array.of(installation.name);
const runningIn = (instances: readonly RunningInstance[]) => (installation: Installation) => instances.some(Installation.runs(installation));
const routes = Effect.fnUntraced(function* ({ answering }: Effect.Success<typeof status>, links: Effect.Success<typeof listening>) {
    const { installed: listing, instances: tracked } = yield* Adobe;
    const current = Effect.all({ installed: listing, instances: SubscriptionRef.get(tracked), peers: links.peers });
    const registered = yield* current;
    const linked = (tag: Tag) => (installation: Installation, peers: readonly Peer[]) => Option.filter(library(installation), ({ Contract }) => Contract.requests.has(tag) && peers.some(Peer.serves(installation.url)));
    const connected =
        (installation: Installation) =>
        <Owner>(owner: Owner) =>
            Effect.map(links.call(installation.url), (client) => ({ library: owner, client }));
    const route = <Chosen extends Pick<RunningInstance, 'instance'>>(holds: Holds, kind: string, chosen: (running: Pick<RunningInstance, 'instance'> & { readonly installation: Installation }, peers: readonly Peer[]) => Effect.Effect<Chosen, Error>) => {
        const listed = Schema.Literals(Array.dedupe(registered.installed.filter(holds(registered.peers)).map(Struct.get('name')))).annotate({ description: `Installed ${kind}` });
        const located = (absent: string) => ({
            application: Schema.OptionFromOptionalKey(listed.annotate({ description: `Application running instance, ${absent}` })),
            instance: Schema.OptionFromOptionalKey(ProcessInstance.annotate({ description: 'Instance value of a launch result or status completion, selects application when application is absent, required when bundle runs more than one instance' })),
        });
        const fields = located('or running in Dock, when absent');
        const holding = Effect.map(current, ({ installed, peers }) => installed.filter(holds(peers)));
        const subject = Match.type<Schema.Struct.Type<typeof fields>>().pipe(
            Match.when({ application: Option.isSome }, () => `installed ${kind}`),
            Match.when({ instance: Option.isSome }, ({ instance: { value } }) => `${kind} running instance ${value}`),
            Match.orElse(() => `${kind} running in Dock`),
        );
        const resolve = Effect.fnUntraced(function* (selection: Schema.Struct.Type<typeof fields>) {
            const { application: given, instance: wanted } = selection;
            const { installed, instances, peers } = yield* current;
            const selecting = instances.filter(Option.match(wanted, { onNone: () => (row: RunningInstance) => row.activationPolicy === 'regular', onSome: (named) => (row: RunningInstance) => row.instance === named }));
            const installation = yield* Unmatched.single(installed.filter(Predicate.and(holds(peers), Option.isSome(given) ? Function.constTrue : runningIn(selecting))), subject(selection), given, names);
            const { instance } = yield* Unmatched.single(instances.filter(Installation.runs(installation)), `running instance of ${installation.name}`, wanted, (row) => Array.of(row.instance));
            return yield* chosen({ installation, instance }, peers);
        });
        const depends = ({ instance }: Chosen) => Effect.succeed([answering(instance)]);
        const annotations = Context.make(Tool.OpenWorld, false).pipe(
            Context.add(
                Server.Visible,
                Effect.map(current, ({ installed, instances, peers }) => installed.some(Predicate.and(holds(peers), runningIn(instances)))),
            ),
        );
        return {
            fields,
            resolve,
            depends,
            annotations,
            listed,
            named: (given: string) => Effect.flatMap(holding, Unmatched.single(`installed ${kind}`, Option.some(given), names)),
            completion: { application: () => Effect.map(holding, Array.map(Struct.get('name'))) },
            optional: (fallback: string) => Route.optional({ fields: located(`${fallback} when absent without instance`), resolve, depends, annotations }),
        };
    };
    const serving = <Served extends Tag>(tag: Served) => ({
        ...route(
            (peers) => (installation) => Option.isSome(linked(tag)(installation, peers)),
            `Adobe application serving ${tag}`,
            (running, peers) => Effect.fromOption(linked(tag)(running.installation, peers)).pipe(Effect.flatMap(connected(running.installation)), Effect.map(Struct.assign(running))),
        ),
        tag,
        serves: ({ library: { Contract } }: { readonly library: Option.Option.Value<ReturnType<typeof library>> }) => Contract.requests.has(tag),
    });
    return {
        application: route(() => Function.constTrue, 'Adobe application', Effect.succeed),
        dictionary: route((peers) => (installation) => installation.scriptable && !peers.some(Peer.serves(installation.url)), 'scriptable Adobe application without open link', Effect.succeed),
        script: {
            ...route(
                (peers) => (installation) => installation.scriptable || Option.isSome(linked(EXECUTE_SCRIPT)(installation, peers)),
                'Adobe application running scripts',
                (running, peers) => Effect.transposeOption(Option.map(linked(EXECUTE_SCRIPT)(running.installation, peers), connected(running.installation))).pipe(Effect.bindTo('linked'), Effect.map(Struct.assign(running))),
            ),
            tag: EXECUTE_SCRIPT,
        },
        serving,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { routes };
