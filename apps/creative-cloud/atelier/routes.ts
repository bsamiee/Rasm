import { Installation, Pid } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Route } from '@rasm/mcp/route';
import { registerTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { Array, Context, Effect, Match, Option, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcGroup } from 'effect/rpc';
import type { Libraries, listening } from '#/features/plugins';
import type { status } from '#/status';

// --- [TYPES] ---------------------------------------------------------------------------

type Links = Effect.Success<typeof listening>;
type Tag = RpcGroup.Rpcs<typeof Libraries>['_tag'];
interface Pair {
    readonly library: Links['libraries'][number];
    readonly peer: Link;
}
type Holds = (installation: Installation, peers: readonly Pair[]) => boolean;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXECUTE_SCRIPT = 'execute_script' as const satisfies Tag;

// --- [OPERATIONS] ----------------------------------------------------------------------

const routes = Effect.fnUntraced(function* ({ answering }: Effect.Success<typeof status>, { libraries, call }: Links) {
    const automation = yield* Automation;
    const current = Effect.all({
        installed: automation.installed,
        instances: SubscriptionRef.get(automation.instances),
        peers: Effect.map(
            Effect.forEach(libraries, (library) =>
                Effect.map(
                    library.peers,
                    Array.map((peer): Pair => ({ library, peer })),
                ),
            ),
            Array.flatten,
        ),
    });
    const registered = yield* current;
    const linking =
        (installation: Installation) =>
        ({ peer }: Pair) =>
            Link.serves(installation.url)(peer);
    const serves = (tag: Tag, installation: Installation) => (pair: Pair) => pair.library.contract.requests.has(tag) && linking(installation)(pair);
    const connected =
        (installation: Installation) =>
        ({ library }: Pair) =>
            Effect.map(call(installation.url), (client) => ({ library, client }));
    const route = <Chosen extends { readonly pid: number }>(holds: Holds, kind: string, chosen: (found: { readonly installation: Installation; readonly pid: number; readonly peers: readonly Pair[] }) => Effect.Effect<Chosen, Error>) => {
        const listed = Schema.Literals(Array.dedupe(registered.installed.filter((row) => holds(row, registered.peers)).map(Struct.get('name')))).annotate({ description: `Installed ${kind}` });
        const located = (absent: string) => ({
            application: Schema.OptionFromOptionalKey(listed.annotate({ description: `Application running pid, ${absent} when absent` })),
            pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Selects application when application is absent, required when bundle runs more than one instance' })),
        });
        const fields = located('or running in Dock,');
        const holding = Effect.map(current, ({ installed, peers }) => installed.filter((row) => holds(row, peers)));
        const resolve = Effect.fnUntraced(function* ({ application: given, pid: wanted }: Schema.Struct.Type<typeof fields>) {
            const { installed, instances, peers } = yield* current;
            const selecting = instances.filter((row) => Option.match(wanted, { onNone: () => row.activationPolicy === 'regular', onSome: (each) => row.pid === each }));
            const subject = Match.value({ given, wanted }).pipe(
                Match.when({ given: Option.isSome }, () => `installed ${kind}`),
                Match.when({ wanted: Option.isSome }, ({ wanted: { value } }) => `${kind} running process ${value}`),
                Match.orElse(() => `${kind} running in Dock`),
            );
            const installation = yield* Unmatched.single(
                installed.filter((row) => holds(row, peers) && (Option.isSome(given) || selecting.some(Installation.runs(row)))),
                subject,
                given,
                (row) => [row.name],
            );
            const running = yield* Unmatched.single(instances.filter(Installation.runs(installation)).map(Struct.get('pid')), `running instance of ${installation.name}`, Option.map(wanted, String), (each) => [String(each)]);
            return yield* chosen({ installation, pid: running, peers });
        });
        const depends = ({ pid }: Chosen) => Effect.succeed([answering(pid)]);
        const annotations = Context.make(Tool.OpenWorld, false).pipe(
            Context.add(
                registerTool.Visible,
                Effect.map(current, ({ installed, instances, peers }) => installed.some((row) => holds(row, peers) && instances.some(Installation.runs(row)))),
            ),
        );
        return {
            fields,
            resolve,
            depends,
            annotations,
            listed,
            named: (given: string) =>
                Effect.flatMap(
                    holding,
                    Unmatched.single(`installed ${kind}`, Option.some(given), (row) => [row.name]),
                ),
            completion: { application: () => Effect.map(holding, Array.map(Struct.get('name'))) },
            optional: (fallback: string) => Route.optional({ fields: located(`${fallback} without pid`), resolve, depends, annotations }),
        };
    };
    const serving = <Served extends Tag>(tag: Served) => ({
        ...route(
            (installation, peers) => peers.some(serves(tag, installation)),
            `Adobe application serving ${tag}`,
            ({ installation, pid, peers }) =>
                Effect.fromOption(Array.findFirst(peers, serves(tag, installation))).pipe(
                    Effect.flatMap(connected(installation)),
                    Effect.map((link) => ({ installation, pid, ...link })),
                ),
        ),
        tag,
        serves: ({ library }: { readonly library: Pair['library'] }) => library.contract.requests.has(tag),
    });
    return {
        application: route(
            () => true,
            'Adobe application',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        dictionary: route(
            (installation, peers) => installation.scriptable && !peers.some(linking(installation)),
            'scriptable Adobe application without open link',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        script: {
            ...route(
                (installation, peers) => installation.scriptable || peers.some(serves(EXECUTE_SCRIPT, installation)),
                'Adobe application running scripts',
                ({ installation, pid, peers }) =>
                    Array.findFirst(peers, serves(EXECUTE_SCRIPT, installation)).pipe(
                        Option.map(connected(installation)),
                        Effect.transposeOption,
                        Effect.map((linked) => ({ installation, pid, linked })),
                    ),
            ),
            tag: EXECUTE_SCRIPT,
        },
        serving,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { routes };
