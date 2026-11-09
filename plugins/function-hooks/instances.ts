import { Installation, Launched } from '@rasm/applications';
import { Array, Effect, Predicate, Record, Schema } from 'effect';
import { type Change, Instance } from '#/decision';
import { Host } from '#/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCHER = 'watch-apps';

// --- [MODELS] --------------------------------------------------------------------------

const Asking = Schema.Struct({ kind: Schema.String, title: Schema.optionalKey(Schema.NullOr(Schema.String)) });
const Exited = Schema.Struct({ status: Schema.optionalKey(Schema.Int), reporting: Schema.optionalKey(Schema.Boolean), report: Schema.optionalKey(Schema.Unknown), reportFailure: Schema.optionalKey(Schema.String) });
const State = Schema.Struct({
    launched: Schema.optionalKey(Schema.Struct({ created: Schema.Boolean })),
    running: Schema.optionalKey(Schema.Struct({ status: Schema.String, asking: Schema.Array(Asking) })),
    exited: Schema.optionalKey(Exited),
});
const Line = Schema.fromJsonString(Schema.Record(Instance.fields.bundle, Schema.Record(Schema.String.check(Schema.isPattern(/^\d+$/u)), State)));
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ launched: Launched }), Launched]));
const Found = Schema.fromJsonString(Schema.toCodecJson(Schema.Record(Schema.String, Schema.Array(Installation))));

// --- [OPERATIONS] ----------------------------------------------------------------------

const statesOf = (text: string) => {
    const bundles = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Line)(line))).flatMap(Record.toEntries);
    return bundles.flatMap(([bundle, pids]) => Record.collect(pids, (pid, state) => ({ pid: Number(pid), bundle, state })));
};

const exitedOf = (text: string): Change['exited'] =>
    statesOf(text)
        .filter(({ state }) => state.exited !== undefined)
        .map(({ pid }) => pid);

const installed = (names: readonly (number | string)[]): Effect.Effect<Record<string, readonly Installation[]>, never, Host> =>
    Array.isReadonlyArrayNonEmpty(names)
        ? Effect.map(
              Host.use((host) => host.output([WATCHER, 'find', ...Array.map(names, String)])),
              (text) => Record.fromEntries(Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Found)(line))).flatMap(Record.toEntries)),
          )
        : Effect.succeed({});

const observed = (text: string) => {
    const answered = Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Answer)(line))).map((answer) => ('launched' in answer ? answer.launched : answer));
    const launched = statesOf(text).map(({ pid, bundle, state }) => ({ pid, bundle, created: state.launched?.created }));
    const claimed = [...launched, ...answered.map(({ pid, identifier, created }) => ({ pid, bundle: identifier, created }))];
    const holding = (created: boolean) => claimed.filter((claim) => claim.created === created).map(({ pid, bundle }): Instance => ({ pid, bundle }));
    return Effect.succeed({ created: holding(true), reused: holding(false), exited: exitedOf(text) } satisfies Change);
};

const exitNote = ({ status = 0, reporting, report, reportFailure }: typeof Exited.Type): readonly string[] => {
    const crashed = [reporting, report, reportFailure].some(Predicate.isNotUndefined);
    return crashed || status !== 0 ? [crashed ? `crashed, read its report with ${WATCHER} log` : `exited ${status}, read it with ${WATCHER} log`] : [];
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
    const running = yield* installed(instances.map(({ pid }) => pid));
    const live = instances.filter(({ pid, bundle }) => (running[pid] ?? []).some(({ identifier }) => identifier === bundle));
    const answers = yield* Effect.forEach(live, ({ pid }) => host.output([WATCHER, 'quit', `${pid}`]), { concurrency: 'unbounded' });
    return { created: [], reused: [], exited: [...Array.difference(instances, live).map(({ pid }) => pid), ...exitedOf(answers.join('\n'))] } satisfies Change;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { installed, observed, quit, WATCHER, watched };
