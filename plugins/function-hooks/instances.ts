import { fileURLToPath } from 'node:url';
import { Launched, State } from '@rasm/applications';
import { Array, Effect, Match, Option, Record, Schema, Struct } from 'effect';
import { Host } from '#/host';
import { installed, WATCHER } from '#/policies/apps';

// --- [TYPES] ---------------------------------------------------------------------------

type Said = readonly [condition: string, action?: string];

// --- [MODELS] --------------------------------------------------------------------------

const Instance = Schema.Struct({ pid: Schema.Int, bundle: Schema.String.check(Schema.isPattern(/^[\w.-]+$/u)) });
type Instance = typeof Instance.Type;

const Change = Schema.Struct({ created: Schema.Array(Instance), reused: Schema.Array(Instance), exited: Schema.Array(Schema.Int) });
type Change = typeof Change.Type;

const Watched = Instance.mapFields(Struct.assign({ ended: Schema.Boolean, conditions: Schema.Array(Schema.String), notes: Schema.Array(Schema.String) }));

const Outline = Schema.Struct({
    process: State.Running.schema.fields.process,
    asking: Schema.Array(Schema.Struct({ kind: State.Window.schema.fields.kind, number: State.Window.schema.fields.number, title: Schema.optionalKey(Schema.String) })),
});
const Printed = Schema.Struct({ launched: Schema.optionalKey(Schema.Struct({ created: Schema.Boolean })), running: Schema.optionalKey(Outline), exited: Schema.optionalKey(State.Exited) });
const Line = Schema.fromJsonString(Schema.Record(Instance.fields.bundle, Schema.Record(Schema.String.check(Schema.isPattern(/^\d+$/u)), Printed)));
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ launched: Launched }), Launched]));

// --- [OPERATIONS] ----------------------------------------------------------------------

const statesOf = (text: string) => {
    const bundles = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Line)(line))).flatMap(Record.toEntries);
    return bundles.flatMap(([bundle, pids]) => Record.collect(pids, (pid, state) => ({ pid: Number(pid), bundle, state })));
};

const exitedOf = (text: string): Change['exited'] =>
    statesOf(text)
        .filter(({ state }) => state.exited !== undefined)
        .map(({ pid }) => pid);

const observed = (text: string) => {
    const answered = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Answer)(line))).map((answer) => ('launched' in answer ? answer.launched : answer));
    const launched = statesOf(text).map(({ pid, bundle, state }) => ({ pid, bundle, created: state.launched?.created }));
    const claimed = [...launched, ...answered.map(({ pid, identifier, created }) => ({ pid, bundle: identifier, created }))];
    const holding = (created: boolean) => claimed.filter((claim) => claim.created === created).map(({ pid, bundle }): Instance => ({ pid, bundle }));
    return Effect.succeed({ created: holding(true), reused: holding(false), exited: exitedOf(text) } satisfies Change);
};

const runningAs = (pid: number, { process, asking }: typeof Outline.Type) => {
    const asked = asking.map(({ kind, number, title }): Said => [title === undefined ? `shows ${kind} window` : `shows ${kind} window "${title}"`, `run ${WATCHER} ${kind === 'blocking' ? 'capture' : 'read'} ${pid} --window ${number}`]);
    return Match.value({ process, asks: Array.isReadonlyArrayNonEmpty(asked) }).pipe(
        Match.withReturnType<readonly Said[]>(),
        Match.when({ asks: true }, () => asked),
        Match.when({ process: 'stopped' }, () => [['is stopped', `run kill -CONT ${pid}`]]),
        Match.when({ process: 'sleeping' }, () => [['is blocked', `run ${WATCHER} capture ${pid}`]]),
        Match.when({ process: 'running' }, () => [['is busy']]),
        Match.orElse(() => []),
    );
};

const exitedAs = Match.type<typeof State.Exited.Type>().pipe(
    Match.withReturnType<readonly Said[]>(),
    Match.when({ report: Match.defined }, ({ report: { url } }) => [['crashed', `read ${Option.getOrElse(Option.liftThrowable(fileURLToPath)(url), () => url)} before a launch`]]),
    Match.when({ reportFailure: Match.defined }, ({ reportFailure }) => [[`crashed with report failure ${reportFailure}`]]),
    Match.when({ reporting: true }, () => [['crashed', 'wait for its report before a launch']]),
    Match.when({ signal: Match.defined }, ({ signal, detail = [] }) => [[['ended on signal', signal, ...detail].join(' ')]]),
    Match.when({ status: 0 }, () => []),
    Match.when({ status: Match.number }, ({ status }) => [[`exited ${status}`]]),
    Match.orElse(() => []),
);

const watched = (text: string) =>
    Effect.succeed(
        statesOf(text).map(({ pid, bundle, state: { running, exited } }): typeof Watched.Type => {
            const said = [...(running === undefined ? [] : runningAs(pid, running)), ...(exited === undefined ? [] : exitedAs(exited))];
            const ended = exited !== undefined && exited.reporting !== true;
            return {
                pid,
                bundle,
                ended,
                conditions: ended ? [] : said.map(([condition]) => `${bundle} ${pid} ${condition}`),
                notes: said.map((parts) => `${bundle} ${pid} ${parts.join(', ')}`),
            };
        }),
    );

const quit = Effect.fnUntraced(function* (instances: readonly Instance[]) {
    const host = yield* Host;
    const running = yield* installed(instances.map(({ pid }) => pid));
    const live = instances.filter(({ pid, bundle }) => (running[pid] ?? []).some(({ identifier }) => identifier === bundle));
    const answers = yield* Effect.forEach(live, ({ pid }) => host.output([WATCHER, 'quit', `${pid}`]), { concurrency: 'unbounded' });
    return { created: [], reused: [], exited: [...Array.difference(instances, live).map(({ pid }) => pid), ...exitedOf(answers.join('\n'))] } satisfies Change;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, Instance, observed, quit, Watched, watched };
