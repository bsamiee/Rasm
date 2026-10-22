import { Automation, Exited, type Installation, Pid, type RunningInstance } from '@rasm/automation';
import { Instructions } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Context, Effect, Function, Option, type Predicate, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { InDesignScript } from '#/indesign-script';
import type { status } from '#/status';

// --- [OPERATIONS] ----------------------------------------------------------------------

const runs = (installation: Installation) => (row: typeof RunningInstance.Type) => row.url.href === installation.url.href && !Schema.is(Exited)(row.state);
const instance = Effect.fnUntraced(function* <E, R>(found: Effect.Effect<Installation, E, R>, pid: Option.Option<number>) {
    const [installation, rows] = yield* Effect.all([found, Automation.use((automation) => SubscriptionRef.get(automation.instances))]);
    return { installation, pid: yield* Unmatched.single(`running instance of ${installation.name}`, Option.map(pid, String))(rows.filter(runs(installation)).map(Struct.get('pid')), (each) => [String(each)]) };
});
const candidates = (hosts: Predicate.Predicate<Installation>) => (given: Option.Option<string>) =>
    Automation.use((automation) =>
        Effect.zipWith(
            automation.installed,
            Effect.map(
                SubscriptionRef.get(automation.instances),
                Array.filter((row) => row.activationPolicy === 'regular'),
            ),
            (installed, docked) => installed.filter((row) => hosts(row) && (Option.isSome(given) || docked.some(runs(row)))),
        ),
    );
const lookup = (hosts: Predicate.Predicate<Installation>) => (given: Option.Option<string>) =>
    Effect.flatMap(candidates(hosts)(given), (rows) => Unmatched.single(Option.match(given, { onSome: () => 'installed Adobe application', onNone: () => 'running Adobe application in the Dock' }), given)(rows, (row) => [row.name]));
const routes = Effect.fnUntraced(function* (names: readonly string[], { refused }: Effect.Success<typeof status>) {
    const [installed, hosts] = yield* Effect.all([Automation.use(Struct.get('installed')), InDesignScript.use(Struct.get('hosts'))]);
    const listed = (holds: Predicate.Predicate<Installation>) => Schema.Literals(Array.intersection(names, installed.filter(holds).map(Struct.get('name')))).annotate({ description: 'Installed Adobe application' });
    const route = (holds: Predicate.Predicate<Installation>) => ({
        fields: {
            application: Schema.OptionFromOptionalKey(listed(holds).annotate({ description: 'Installed Adobe application, absent for the one running in the Dock' })),
            pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Running instance, required when its bundle runs several' })),
        },
        resolve: ({ application: given, pid }: { readonly application: Option.Option<string>; readonly pid: Option.Option<number> }) => instance(lookup(holds)(given), pid),
        depends: ({ pid }: { readonly pid: number }) => Effect.succeed([refused(pid)]),
        annotations: Context.make(Tool.OpenWorld, false).pipe(Context.add(Instructions, ['A call that changes state returns once its application shows a dialog requiring an answer. No tool changes the frontmost application.'])),
        completion: { application: () => Effect.map(candidates(holds)(Option.none()), Array.map(Struct.get('name'))) },
    });
    return { application: route(Function.constTrue), indesign: route(hosts), listed };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { instance, lookup, routes };
