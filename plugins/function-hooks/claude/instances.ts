import { fileURLToPath } from 'node:url';
import { Launched, State } from '@rasm/applications';
import { Array, Effect, Match, Option, Record, Result, Schema } from 'effect';
import { Instance } from '#/claude/routes';
import { Host } from '#/host';
import { WATCH_APPS } from '#/shell/programs';

// --- [TYPES] ---------------------------------------------------------------------------

type Note = readonly [condition: string, action?: string];

// --- [MODELS] --------------------------------------------------------------------------

const Outline = Schema.Struct({
    status: State.Running.schema.fields.status,
    process: State.Running.schema.fields.process,
    asking: Schema.Array(Schema.Struct({ kind: State.Window.schema.fields.kind, number: State.Window.schema.fields.number, title: Schema.optionalKey(Schema.String) })),
});
const Printed = Schema.Struct({ instance: Instance.fields.instance, launched: Schema.optionalKey(Schema.Struct({ created: Schema.Boolean })), running: Schema.optionalKey(Outline), exited: Schema.optionalKey(State.Exited) });
const Line = Schema.fromJsonString(Schema.Record(Instance.fields.bundle, Schema.Record(Schema.String, Printed)));
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ launched: Launched }), Launched]));
const Missing = Schema.fromJsonString(Schema.Record(Instance.fields.instance, Schema.Record(Schema.String, Schema.Never)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linesOf = (text: string) => Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Line)(line))).flatMap(Record.toEntries);

const statesOf = (lines: ReturnType<typeof linesOf>) => lines.flatMap(([bundle, pids], at) => Record.collect(pids, (pid, state) => ({ at, pid: Number(pid), instance: state.instance, bundle, state })));

const exitedOf = (text: string) =>
    statesOf(linesOf(text))
        .filter(({ state }) => state.exited !== undefined)
        .map(({ instance }) => instance);

const observed = (text: string) => {
    const answered = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Answer)(line))).map((answer) => ('launched' in answer ? answer.launched : answer));
    const launched = statesOf(linesOf(text)).map(({ pid, instance, bundle, state }) => ({ pid, instance, bundle, created: state.launched?.created }));
    const claimed = [...launched, ...answered.map(({ pid, instance, identifier, created }) => ({ pid, instance, bundle: identifier, created }))];
    const holding = (created: boolean) => claimed.filter((claim) => claim.created === created).map(({ pid, instance, bundle }) => ({ pid, instance, bundle }));
    return { created: holding(true), reused: holding(false), exited: exitedOf(text) };
};

const runningAs = ({ pid, instance }: Pick<typeof Instance.Type, 'pid' | 'instance'>, { process, asking }: typeof Outline.Type) => {
    const asked = asking.map(({ kind, number, title }): Note => [title === undefined ? `shows ${kind} window` : `shows ${kind} window "${title}"`, `run ${WATCH_APPS} ${kind === 'blocking' ? 'capture' : 'read'} ${instance} --window ${number}`]);
    return Match.value({ process, asks: Array.isReadonlyArrayNonEmpty(asked) }).pipe(
        Match.withReturnType<readonly Note[]>(),
        Match.when({ asks: true }, () => asked),
        Match.when({ process: 'stopped' }, () => [['is stopped', `run kill -CONT ${pid}`]]),
        Match.when({ process: 'sleeping' }, () => [['is blocked', `run ${WATCH_APPS} capture ${instance}`]]),
        Match.when({ process: 'running' }, () => [['is busy']]),
        Match.orElse(() => []),
    );
};

const exitedAs = Match.type<typeof State.Exited.Type>().pipe(
    Match.withReturnType<readonly Note[]>(),
    Match.when({ report: Match.defined }, ({ report: { url } }) => [['crashed', `read ${fileURLToPath(url)} before a launch`]]),
    Match.when({ reportFailure: Match.defined }, ({ reportFailure }) => [[`crashed with report failure ${reportFailure}`]]),
    Match.when({ reporting: true }, () => [['crashed', 'wait for its report before a launch']]),
    Match.when({ _tag: 'Signal' }, ({ signal, detail = [] }) => [[['ended on signal', signal, ...detail].join(' ')]]),
    Match.when({ _tag: 'Status', status: 0 }, () => []),
    Match.when({ _tag: 'Status' }, ({ status }) => [[`exited ${status}`]]),
    Match.when({ _tag: 'Unread' }, () => []),
    Match.exhaustive,
);

const watched = (text: string) => {
    const lines = linesOf(text);
    const emptied = Record.fromEntries(Array.filterMap(lines, ([bundle, pids], at) => (Record.isEmptyRecord(pids) ? Result.succeed([bundle, at] as const) : Result.failVoid)));
    const states = Array.filterMap(statesOf(lines), ({ at, pid, instance, bundle, state: { running, exited } }) => {
        const notes = [...(running === undefined ? [] : runningAs({ pid, instance }, running)), ...(exited === undefined ? [] : exitedAs(exited))];
        const ended = exited !== undefined && exited.reporting !== true;
        const standing = {
            pid,
            instance,
            bundle,
            ended,
            ...(running !== undefined && { status: running.status }),
            asks: running !== undefined && Array.isReadonlyArrayNonEmpty(running.asking),
            conditions: ended ? [] : notes.map(([condition]) => `${bundle} ${pid} ${condition}`),
            notes: notes.map((parts) => `${bundle} ${pid} ${parts.join(', ')}`),
        };
        return !ended && Option.exists(Record.get(emptied, bundle), (last) => at < last) ? Result.failVoid : Result.succeed(standing);
    });
    return { absent: Record.keys(emptied), states };
};

const quit = (instances: readonly (typeof Instance.Type)[]) =>
    Effect.map(
        Host.use((host) => host.output([WATCH_APPS, 'quit', ...instances.map(({ instance }) => instance)])),
        (text) => [...text.split('\n').flatMap((line) => Option.match(Schema.decodeOption(Missing)(line), { onNone: () => [], onSome: Record.keys })), ...exitedOf(text)],
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { observed, quit, watched };
