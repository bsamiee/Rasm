import { Installation, Pid } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Instructions, Visible } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { Array, Context, Effect, Match, Option, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { Library } from '#/features/plugins';
import type { status } from '#/status';

// --- [TYPES] ---------------------------------------------------------------------------

interface Pair {
    readonly library: Library;
    readonly peer: Link;
}
type Holds = (installation: Installation, peers: readonly Pair[]) => boolean;

// --- [OPERATIONS] ----------------------------------------------------------------------

const routes = Effect.fnUntraced(function* ({ dialog }: Effect.Success<typeof status>, libraries: readonly Library[]) {
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
    const listed = (holds: Holds) => Schema.Literals(Array.dedupe(registered.installed.filter((row) => holds(row, registered.peers)).map(Struct.get('name')))).annotate({ description: 'Installed Adobe application' });
    const serves =
        (tag: string, installation: Installation) =>
        ({ library, peer }: Pair) =>
            library.Contract.requests.has(tag) && Link.serves(installation.url)(peer);
    const connected =
        (installation: Installation) =>
        ({ library }: Pair) =>
            Effect.map(library.call(Link.serves(installation.url)), (client) => ({ library, client }));
    const route = <Chosen extends { readonly pid: number }>(holds: Holds, kind: string, chosen: (found: { readonly installation: Installation; readonly pid: number; readonly peers: readonly Pair[] }) => Effect.Effect<Chosen, Error>, served: (chosen: Chosen) => boolean = () => true) => {
        const pid = Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Selects application when application is absent, required when bundle runs more than one instance' }));
        const fields = { application: Schema.OptionFromOptionalKey(listed(holds).annotate({ description: 'Application running pid, or running in Dock, when absent' })), pid };
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
        const depends = ({ pid: running }: Chosen) => Effect.succeed([dialog(running)]);
        const annotations = Context.make(Tool.OpenWorld, false).pipe(
            Context.add(Instructions, ['Specifiers come from results', 'State-changing calls return early when a dialog requires an answer. No tool changes frontmost application']),
            Context.add(
                Visible,
                Effect.map(current, ({ installed, instances, peers }) => installed.some((row) => holds(row, peers) && instances.some(Installation.runs(row)))),
            ),
        );
        const completion = { application: () => Effect.map(holding, Array.map(Struct.get('name'))) };
        return {
            fields,
            holding,
            resolve,
            depends,
            annotations,
            completion,
            serves: served,
            optional: (fallback: string) => {
                const optional = { application: Schema.OptionFromOptionalKey(listed(holds).annotate({ description: `Application running pid, ${fallback} when absent without pid` })), pid };
                return {
                    fields: optional,
                    resolve: (given: Schema.Struct.Type<typeof optional>) => (Option.isNone(given.application) && Option.isNone(given.pid) ? Effect.succeedNone : Effect.asSome(resolve(given))),
                    depends: Option.match({ onNone: () => Effect.succeed([]), onSome: depends }),
                    annotations: Context.add(annotations, Visible, Effect.succeed(true)),
                    completion,
                    serves: (picked: Option.Option<Chosen>) => Option.exists(picked, served),
                };
            },
        };
    };
    const serving = (tag: string) =>
        route(
            (installation, peers) => peers.some(serves(tag, installation)),
            `Adobe application serving ${tag}`,
            Effect.fnUntraced(function* ({ installation, pid, peers }) {
                return { installation, pid, ...(yield* Effect.flatMap(Effect.fromOption(Array.findFirst(peers, serves(tag, installation))), connected(installation))) };
            }),
            ({ library }) => library.Contract.requests.has(tag),
        );
    const scripting = route(
        (installation, peers) => installation.scriptable || peers.some(serves('execute_script', installation)),
        'Adobe application running scripts',
        Effect.fnUntraced(function* ({ installation, pid, peers }) {
            const linked = yield* Effect.transposeOption(Option.map(Array.findFirst(peers, serves('execute_script', installation)), connected(installation)));
            return { installation, pid, linked };
        }),
    );
    return {
        application: route(
            () => true,
            'Adobe application',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        scriptable: route(
            (installation) => installation.scriptable,
            'scriptable Adobe application',
            ({ installation, pid }) => Effect.succeed({ installation, pid }),
        ),
        scripting,
        serving,
        listed,
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { routes };
