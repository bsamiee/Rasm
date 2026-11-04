import { Installation, Pid, type RunningInstance } from '@rasm/automation';
import { Automation } from '@rasm/automation/node';
import { Route, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Peer } from '@rasm/uxp/peer';
import { Array, Context, Effect, Function, flow, Match, Option, Predicate, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcGroup } from 'effect/rpc';
import { type Libraries, library, type listening } from '#/features/plugins';
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
    const automation = yield* Automation;
    const current = Effect.all({ installed: automation.installed, instances: SubscriptionRef.get(automation.instances), peers: links.peers });
    const registered = yield* current;
    const linked = (tag: Tag) => (installation: Installation, peers: readonly Peer[]) => Option.filter(library(installation), ({ Contract }) => Contract.requests.has(tag) && peers.some(Peer.serves(installation.url)));
    const connected =
        (installation: Installation) =>
        <Owner>(owner: Owner) =>
            Effect.map(links.call(installation.url), (client) => ({ library: owner, client }));
    const route = <Chosen extends { readonly pid: number }>(holds: Holds, kind: string, chosen: (found: { readonly installation: Installation; readonly pid: number; readonly peers: readonly Peer[] }) => Effect.Effect<Chosen, Error>) => {
        const listed = Schema.Literals(Array.dedupe(registered.installed.filter(holds(registered.peers)).map(Struct.get('name')))).annotate({ description: `Installed ${kind}` });
        const located = (absent: string) => ({
            application: Schema.OptionFromOptionalKey(listed.annotate({ description: `Application running pid, ${absent}` })),
            pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Selects application when application is absent, required when bundle runs more than one instance' })),
        });
        const fields = located('or running in Dock, when absent');
        const holding = Effect.map(current, ({ installed, peers }) => installed.filter(holds(peers)));
        const subject = Match.type<Schema.Struct.Type<typeof fields>>().pipe(
            Match.when({ application: Option.isSome }, () => `installed ${kind}`),
            Match.when({ pid: Option.isSome }, ({ pid: { value } }) => `${kind} running process ${value}`),
            Match.orElse(() => `${kind} running in Dock`),
        );
        const resolve = Effect.fnUntraced(function* (selection: Schema.Struct.Type<typeof fields>) {
            const { application: given, pid: wanted } = selection;
            const { installed, instances, peers } = yield* current;
            const selecting = instances.filter(Option.match(wanted, { onNone: () => (row: RunningInstance) => row.activationPolicy === 'regular', onSome: (pid) => (row: RunningInstance) => row.pid === pid }));
            const installation = yield* Unmatched.single(installed.filter(Predicate.and(holds(peers), Option.isSome(given) ? Function.constTrue : runningIn(selecting))), subject(selection), given, names);
            const running = yield* Unmatched.single(instances.filter(Installation.runs(installation)).map(Struct.get('pid')), `running instance of ${installation.name}`, Option.map(wanted, String), flow(String, Array.of));
            return yield* chosen({ installation, pid: running, peers });
        });
        const depends = ({ pid }: Chosen) => Effect.succeed([answering(pid)]);
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
            optional: (fallback: string) => Route.optional({ fields: located(`${fallback} when absent without pid`), resolve, depends, annotations }),
        };
    };
    const serving = <Served extends Tag>(tag: Served) => ({
        ...route(
            (peers) => (installation) => Option.isSome(linked(tag)(installation, peers)),
            `Adobe application serving ${tag}`,
            ({ installation, pid, peers }) => Effect.fromOption(linked(tag)(installation, peers)).pipe(Effect.flatMap(connected(installation)), Effect.map(Struct.assign({ installation, pid }))),
        ),
        tag,
        serves: ({ library: { Contract } }: { readonly library: Option.Option.Value<ReturnType<typeof library>> }) => Contract.requests.has(tag),
    });
    return {
        application: route(
            () => Function.constTrue,
            'Adobe application',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        dictionary: route(
            (peers) => (installation) => installation.scriptable && !peers.some(Peer.serves(installation.url)),
            'scriptable Adobe application without open link',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        script: {
            ...route(
                (peers) => (installation) => installation.scriptable || Option.isSome(linked(EXECUTE_SCRIPT)(installation, peers)),
                'Adobe application running scripts',
                ({ installation, pid, peers }) => Effect.transposeOption(Option.map(linked(EXECUTE_SCRIPT)(installation, peers), connected(installation))).pipe(Effect.bindTo('linked'), Effect.map(Struct.assign({ installation, pid }))),
            ),
            tag: EXECUTE_SCRIPT,
        },
        serving,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { routes };
