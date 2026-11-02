import { Installation, Pid } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Script } from '@rasm/indesign/script';
import { Instructions } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Effect, Function, Option, type Predicate, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { status } from '#/status';

// --- [OPERATIONS] ----------------------------------------------------------------------

const candidates = (hosts: Predicate.Predicate<Installation>, given: Option.Option<string>, pid: Option.Option<number>) =>
    Automation.use((automation) =>
        Effect.zipWith(automation.installed, SubscriptionRef.get(automation.instances), (installed, rows) => {
            const selecting = rows.filter((row) => Option.match(pid, { onNone: () => row.activationPolicy === 'regular', onSome: (wanted) => row.pid === wanted }));
            return installed.filter((row) => hosts(row) && (Option.isSome(given) || selecting.some(Installation.runs(row))));
        }),
    );
const lookup =
    (hosts: Predicate.Predicate<Installation>) =>
    (given: Option.Option<string>, pid: Option.Option<number> = Option.none()) =>
        Effect.flatMap(
            candidates(hosts, given, pid),
            Unmatched.single(Option.match(given, { onSome: () => 'installed Adobe application', onNone: () => Option.match(pid, { onNone: () => 'running Adobe application in Dock', onSome: (each) => `Adobe application running process ${each}` }) }), given, (row) => [row.name]),
        );
const instance = Effect.fnUntraced(function* (hosts: Predicate.Predicate<Installation>, given: Option.Option<string>, pid: Option.Option<number>) {
    const [installation, rows] = yield* Effect.all([lookup(hosts)(given, pid), Automation.use((automation) => SubscriptionRef.get(automation.instances))]);
    return { installation, pid: yield* Unmatched.single(rows.filter(Installation.runs(installation)).map(Struct.get('pid')), `running instance of ${installation.name}`, Option.map(pid, String), (each) => [String(each)]) };
});
const routes = Effect.fnUntraced(function* ({ refused }: Effect.Success<typeof status>) {
    const [installed, hosts] = yield* Effect.all([Automation.use(Struct.get('installed')), Script.use(Struct.get('hosts'))]);
    const listed = (holds: Predicate.Predicate<Installation>) => Schema.Literals(Array.dedupe(installed.filter(holds).map(Struct.get('name')))).annotate({ description: 'Installed Adobe application' });
    const route = (holds: Predicate.Predicate<Installation>) => {
        const fields = {
            application: Schema.OptionFromOptionalKey(listed(holds).annotate({ description: 'Application running pid, or running in Dock, when absent' })),
            pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Selects application when application is absent, required when bundle runs more than one instance' })),
        };
        return {
            fields,
            resolve: ({ application: given, pid }: Schema.Struct.Type<typeof fields>) => instance(holds, given, pid),
            depends: ({ pid }: { readonly pid: number }) => Effect.succeed([refused(pid)]),
            annotations: Context.make(Tool.OpenWorld, false).pipe(Context.add(Instructions, ['State-changing calls return early when a dialog requires an answer. No tool changes frontmost application'])),
            completion: { application: () => Effect.map(candidates(holds, Option.none(), Option.none()), Array.map(Struct.get('name'))) },
        };
    };
    return { application: route(Function.constTrue), indesign: route(hosts), listed };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { instance, lookup, routes };
