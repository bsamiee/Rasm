import { Array, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { type Change, Instance } from '#/decision';
import { Host } from '#/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCHER = 'watch-apps';
const BUNDLE_ID = /bundleID="(?<bundle>[\w.-]+)"/u;
const BACKGROUND_ONLY = /\btype="BackgroundOnly"/u;

// --- [MODELS] --------------------------------------------------------------------------

const Asking = Schema.Struct({ kind: Schema.String, title: Schema.optionalKey(Schema.NullOr(Schema.String)) });
const Exited = Schema.Struct({ status: Schema.optionalKey(Schema.Int), reporting: Schema.optionalKey(Schema.Boolean), report: Schema.optionalKey(Schema.Unknown), reportFailure: Schema.optionalKey(Schema.String) });
const State = Schema.Struct({
    launched: Schema.optionalKey(Schema.Struct({ created: Schema.Boolean })),
    running: Schema.optionalKey(Schema.Struct({ status: Schema.String, asking: Schema.Array(Asking) })),
    exited: Schema.optionalKey(Exited),
});
const Line = Schema.fromJsonString(Schema.Record(Instance.fields.bundle, Schema.Record(Schema.String.check(Schema.isPattern(/^\d+$/u)), State)));
const Launched = Schema.Struct({ pid: Schema.Int, created: Schema.Boolean });
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

const appOf = (pid: number | string) =>
    Effect.map(
        Host.use((host) => host.output(['lsappinfo', 'info', '-only', 'bundleID,ApplicationType', `${pid}`])),
        (text) => Option.map(Option.fromNullishOr(BUNDLE_ID.exec(text)?.groups?.bundle), (bundle) => ({ bundle, foreground: !BACKGROUND_ONLY.test(text) })),
    );

const instanceOf = (pid: number) =>
    Effect.map(
        appOf(pid),
        Option.map(({ bundle }): Instance => ({ pid, bundle })),
    );

const observed = Effect.fnUntraced(function* (text: string) {
    const answers = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Answer)(line))).map((answer) => ('launched' in answer ? answer.launched : answer));
    const answered = yield* Effect.forEach(answers, ({ pid, created }) => Effect.map(instanceOf(pid), Option.map(Struct.assign({ created }))), { concurrency: 'unbounded' });
    const launched = statesOf(text).map(({ pid, bundle, state }) => ({ pid, bundle, created: state.launched?.created }));
    const claimed = [...launched, ...Array.getSomes(answered)];
    const holding = (created: boolean) => claimed.filter((claim) => claim.created === created).map(({ pid, bundle }): Instance => ({ pid, bundle }));
    return { created: holding(true), reused: holding(false), exited: exitedOf(text) } satisfies Change;
});

const exitNote = ({ status = 0, reporting, report, reportFailure }: typeof Exited.Type): readonly string[] => {
    const crashed = [reporting, report, reportFailure].some(Predicate.isNotUndefined);
    return crashed || status !== 0 ? [`${crashed ? 'crashed, read its report' : `exited ${status}, read it`} with ${WATCHER} log`] : [];
};

const watched = (text: string) =>
    Effect.succeed(
        statesOf(text).map(({ pid, bundle, state: { running, exited } }) => ({
            pid,
            bundle,
            ended: exited !== undefined,
            notes: [
                ...(running?.status === 'unresponsive' ? [`stopped responding, read it with ${WATCHER} read`] : []),
                ...(running?.asking ?? []).map(({ kind, title }) => `${['shows', kind, title].filter(Predicate.isNotNullish).join(' ')}, answer it with ${WATCHER} perform`),
                ...(exited === undefined ? [] : exitNote(exited)),
            ].map((note) => `${bundle} ${pid} ${note}`),
        })),
    );

const quit = Effect.fnUntraced(function* (instances: readonly Instance[]) {
    const host = yield* Host;
    const live = yield* Effect.filter(instances, (instance) => Effect.map(instanceOf(instance.pid), Option.contains(instance)), { concurrency: 'unbounded' });
    const answers = yield* Effect.forEach(live, ({ pid }) => host.output([WATCHER, 'quit', '--pid', `${pid}`]), { concurrency: 'unbounded' });
    return { created: [], reused: [], exited: [...Array.difference(instances, live).map(({ pid }) => pid), ...exitedOf(answers.join('\n'))] } satisfies Change;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { appOf, observed, quit, WATCHER, watched };
