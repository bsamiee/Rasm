import { Array, Cause, Config, Console, Context, DateTime, Duration, Effect, FileSystem, Filter, Formatter, Layer, Logger, Match, MutableRef, Option, Path, Record, References, Schema, Stdio, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import type { Live } from '#/session/harness';
import { sampled } from '#/session/instance';
import { since } from '#/trace/query';
import { declared, Field, Kind, messageOf, partition, RECORDS, record, Signed, tagOf } from '#/trace/records';

// --- [TYPES] ---------------------------------------------------------------------------

type Phase = 'run' | 'prove' | 'compare' | 'plan' | 'launch' | 'ready' | 'open' | 'survey' | 'batch' | 'quit' | 'log';
interface Tally {
    readonly cases: { readonly ran: number; readonly failed: number; readonly notRun: number };
    readonly signatures: readonly (typeof Signed.Type)[];
    readonly instances: readonly { readonly pid: number; readonly started: string; readonly state: string; readonly reports: readonly (typeof Kind.Type)[] }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STAMPED = /(?<high>[\da-f]{8})-(?<low>[\da-f]{4})-7[\da-f]{3}-[\da-f]{4}-[\da-f]{12}$/u;
const RETAINED = { runs: 200, age: Duration.weeks(1) } as const;
const EMPTY: Tally = { cases: { ran: 0, failed: 0, notRun: 0 }, signatures: [], instances: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Counted = Schema.Struct({ level: Schema.String, phase: Schema.String, ...Struct.pick(Field, ['pid', 'started', 'version', 'proof', 'case', 'tag', 'completion', 'cases', 'kind', 'state']) }).annotate({ description: 'Record fields the closing summary tallies' });
const signedAlike = Schema.toEquivalence(Signed);
const session = Config.option(Config.String('CLAUDE_CODE_SESSION_ID'));

// --- [SERVICES] ------------------------------------------------------------------------

class Run extends Context.Service<Run, { readonly commit: string; readonly folder: string; readonly phase: MutableRef.MutableRef<Phase> }>()('@rasm/harnesses/session/run') {
    static readonly Instance = Context.Reference<Option.Option<Pick<Live, 'pid' | 'started'>>>('@rasm/harnesses/session/run/Run/Instance', { defaultValue: Option.none });
    static readonly layer = Effect.fnUntraced(function* (host: string, logs: string, commit: string) {
        const { run, folder, file } = yield* begun(host, logs);
        const held = { commit, folder, phase: MutableRef.make<Phase>('run') };
        const tally = MutableRef.make(EMPTY);
        const summary = Effect.gen(function* () {
            const { cases, signatures: signed, instances } = MutableRef.get(tally);
            const signatures = yield* Array.match(signed, { onEmpty: () => Effect.succeedNone, onNonEmpty: (listed) => Effect.asSome(since(logs, host, listed)) });
            yield* Console.log(Formatter.formatJson({ run, log: file, cases, ...Record.getSomes({ signatures }), instances }));
        });
        yield* Effect.addFinalizer(() => Effect.orDie(summary));
        return Layer.mergeAll(Layer.succeed(Run, held), Logger.layer([Logger.toFile(Logger.make(written(held.phase, tally)), file)]));
    }, Layer.unwrap);
}
class Signing extends Context.Service<Signing, true>()('@rasm/harnesses/session/run/Signing') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PHASES]
const failed = (label: Phase, cause: Cause.Cause<unknown>) =>
    Match.value(cause).pipe(
        Match.when(
            (held) => Option.isSome(Context.getOption(Cause.annotations(held), Signing)),
            () => Effect.void,
        ),
        Match.when(Cause.hasInterruptsOnly, () => record('Warn', 'failed', { tag: 'Interrupted', error: `${label} interrupted` })),
        Match.orElse((held) => {
            const error = Cause.squash(held);
            return record('Error', 'failed', { tag: tagOf(error), error: Cause.hasDies(held) ? Cause.pretty(held) : messageOf(error) });
        }),
    );
const signing = <E>(cause: Cause.Cause<E>) => Effect.failCause(Cause.annotate(cause, Context.make(Signing, true)));
const phase =
    (label: Phase) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(Run, ({ phase: current }) => {
            const outer = MutableRef.get(current);
            return Effect.sync(() => MutableRef.set(current, label)).pipe(
                Effect.andThen(self),
                Effect.onError((cause) => failed(label, cause)),
                Effect.catchCause(signing),
                Effect.ensuring(Effect.sync(() => MutableRef.set(current, outer))),
            );
        });
const sample = (pid: number) => Effect.flatMap(Effect.all([Run, Path.Path]), ([{ folder }, path]) => sampled(pid, path.join(folder, `${pid}.sample.txt`)));

// --- [SUMMARY]
const counted =
    (entry: unknown) =>
    (tally: Tally): Tally =>
        Option.match(Schema.decodeUnknownOption(Counted)(entry), {
            onNone: () => tally,
            onSome: ({ level, phase: at, pid, started, version, proof, case: name, tag, completion, cases, kind, state }) => {
                const finished = completion !== undefined;
                const signed = Option.fromUndefinedOr(tag ?? (level === 'Info' ? undefined : completion));
                const ours = (held: Tally['instances'][number]) => held.pid === pid && held.started === started;
                const [mine, others] = Array.partition(tally.instances, Filter.fromPredicate(ours));
                const prior = Array.head(mine);
                const seen = Option.flatMap(Option.all({ pid: Option.fromUndefinedOr(pid), started: Option.fromUndefinedOr(started) }), (held) =>
                    Option.map(Option.firstSomeOf([Option.fromUndefinedOr(state), Option.map(prior, Struct.get('state'))]), (current) => ({
                        ...held,
                        state: current,
                        reports: Array.union(Option.match(prior, { onNone: () => [], onSome: Struct.get('reports') }), Schema.is(Kind)(kind) && level === 'Warn' ? [kind] : []),
                    })),
                );
                return {
                    cases: { ran: tally.cases.ran + (finished ? 1 : 0), failed: tally.cases.failed + (finished && level === 'Error' ? 1 : 0), notRun: tally.cases.notRun + (cases ?? []).length },
                    signatures: Option.match(signed, {
                        onNone: () => tally.signatures,
                        onSome: (held) => Array.unionWith(tally.signatures, [{ ...Record.getSomes({ version: Option.fromUndefinedOr(version), proof: Option.fromUndefinedOr(proof), case: Option.fromUndefinedOr(name) }), phase: at, tag: held }], signedAlike),
                    }),
                    instances: Option.match(seen, { onNone: () => tally.instances, onSome: (held) => [...others, held] }),
                };
            },
        });
const written =
    (current: MutableRef.MutableRef<Phase>, tally: MutableRef.MutableRef<Tally>) =>
    ({ message, logLevel, fiber, date }: Logger.Options<unknown>) => {
        const stamp = Option.match(fiber.getRef(Run.Instance), { onNone: () => ({}), onSome: ({ pid, started }) => ({ pid, started: DateTime.formatIso(started) }) });
        const text = Array.ensure(message).join(' ');
        const entry = { at: date.toISOString(), level: logLevel, message: text, phase: MutableRef.get(current), ...declared(text, { ...stamp, ...fiber.getRef(References.CurrentLogAnnotations) }) };
        const json = Formatter.formatJson(entry);
        MutableRef.update(tally, counted(entry));
        fiber.getRef(Console.Console).log(json);
        return json;
    };

// --- [LIFECYCLE]
const begun = Effect.fn('begun')(function* (host: string, logs: string) {
    const [fs, path, run, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom.use((random) => random.uuidV7()), DateTime.now]);
    const folder = partition(path, logs, host, run);
    yield* fs.makeDirectory(folder, { recursive: true });
    const runs = yield* fs.glob(partition(path, logs, host, '*'));
    const cutoff = DateTime.subtractDuration(now, RETAINED.age);
    const born = (held: string) => Option.flatMap(Option.fromNullishOr(STAMPED.exec(held)?.groups), ({ high, low }) => DateTime.make(Number.parseInt(`${high}${low}`, 16)));
    const stale = runs.filter((held, index) => index < runs.length - RETAINED.runs || Option.exists(born(held), DateTime.isLessThan(cutoff)));
    yield* Effect.forEach(stale, (held) => fs.remove(held, { recursive: true, force: true }), { discard: true });
    return { run, folder, file: path.join(folder, RECORDS) };
});
const starting = Effect.flatMap(Effect.all([Run, Stdio.Stdio.use((stdio) => stdio.args), session]), ([{ commit }, args, owner]) => record('Info', 'run started', { commit, args, session: Option.getOrNull(owner) }));
const ran = <A, E, R>(self: Effect.Effect<A, E, R>) => starting.pipe(Effect.andThen(self), Effect.tap(record('Info', 'run ended', {})), phase('run'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { phase, Run, ran, sample, signing };
