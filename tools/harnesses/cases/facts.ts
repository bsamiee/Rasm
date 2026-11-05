import { Array, Effect, FileSystem, flow, Option, Path, Schema, String, Struct } from 'effect';
import { Answer, deciding, Halted, Outcome } from '#/cases/results';
import type { Harness } from '#/session/harness';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = ReturnType<typeof fact>['Type'];
type Key = Pick<Fact, 'question' | 'identifier' | 'version' | 'engine' | 'environment'> & Partial<Pick<Fact, 'host' | 'expression' | 'args' | 'precondition'>>;
type Asking<T> = Pick<Fact, 'identifier' | 'version' | 'environment'> & { readonly engine: T };

// --- [CONSTANTS] -----------------------------------------------------------------------

const MACHINE = /^(?:Security|TCC|LaunchServices) /u;
const RUNTIME = /^\S+\.dylib /u;

// --- [MODELS] --------------------------------------------------------------------------

const Alerted = Schema.Struct({ alerts: Outcome.common.alerts });
const Interrupted = Schema.TemplateLiteral([Schema.Literals(['crashed', 'hung']), ' ', Schema.String]).annotate({ description: 'Thrown answer of a case that ended or stalled its instance' });
const fact = (engine: Harness['Engine']) =>
    Schema.Struct({
        question: Schema.NonEmptyString.annotate({ description: 'Host behavior asked, matched exactly before any host call' }),
        ...deciding(engine),
        expression: Schema.NonEmptyString,
        args: Schema.optionalKey(Schema.Array(Schema.Json)),
        precondition: Schema.optionalKey(Schema.Json),
        answer: Answer,
        asked: Schema.String.annotate({ description: 'ISO 8601 instant of the host call' }),
    });
const Index = (engine: Harness['Engine']) => Schema.fromJsonString(Schema.Struct({ facts: Schema.Array(fact(engine)) }), { space: 2 }).annotate({ description: 'Facts index of one host under its application folder' });
const same = Schema.toEquivalence(Schema.UndefinedOr(Schema.Json));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INDEX]
const matches = (row: Key, key: Key) =>
    row.question === key.question && row.identifier === key.identifier && row.version === key.version && row.engine === key.engine && same(row.environment, key.environment) && (['host', 'expression', 'args', 'precondition'] as const).every((field) => key[field] === undefined || same(row[field], key[field]));
const known = (rows: readonly Fact[], key: Key) => Array.findFirst(rows, (row) => matches(row, key));
const faulted = (rows: readonly Fact[], key: Omit<Key, 'question'>) => Array.findFirst(rows, (row) => 'thrown' in row.answer && Schema.is(Interrupted)(row.answer.thrown) && matches(row, { ...key, question: row.question }));
const load = Effect.fn('load')(function* (engine: Harness['Engine'], file: string) {
    const fs = yield* FileSystem.FileSystem;
    return yield* fs.readFileString(file).pipe(
        Effect.flatMap(Schema.decodeEffect(Index(engine))),
        Effect.map(({ facts }) => facts),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
    );
});
const record = Effect.fn('record')(function* (engine: Harness['Engine'], file: string, made: readonly Fact[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const rows = yield* load(engine, file);
    const replaced = (row: Key) => made.some((next) => matches(row, Struct.pick(next, ['question', 'identifier', 'version', 'engine', 'environment'])) && same(row.host, next.host));
    yield* fs.makeDirectory(path.dirname(file), { recursive: true });
    yield* fs.writeFileString(file, `${yield* Schema.encodeEffect(Index(engine))({ facts: [...rows.filter((row) => !replaced(row)), ...made] })}\n`);
});
const Fact = Object.assign(fact, { known, faulted, load, record });

// --- [ANSWERS]
const answerOf = (row: typeof Outcome.Type): Option.Option<typeof Answer.Type> => {
    const held: Schema.Json = row.value ?? null;
    const value = Option.match(
        Option.filter(Schema.decodeUnknownOption(Alerted)(row), ({ alerts }) => alerts.length > 0),
        { onNone: (): Schema.Json => held, onSome: ({ alerts }): Schema.Json => ({ value: held, alerts: alerts.map(({ text }) => text) }) },
    );
    const halted = Option.flatMap(
        Option.fromUndefinedOr(row.halted),
        Halted.match({
            Crashed: ({ signal, status, exception, report }) => Option.some({ thrown: `crashed ${[signal === undefined ? `status ${status}` : `signal ${signal}`, exception, report].filter((part) => part !== undefined).join(' ')}` }),
            Hung: ({ process, sample, frames }) =>
                Option.liftPredicate(
                    { thrown: ['hung', process, sample].filter((part) => part !== undefined).join(' ') },
                    () =>
                        !Option.exists(
                            Array.findFirst(frames, (frame) => !RUNTIME.test(frame)),
                            flow(String.match(MACHINE), Option.isSome),
                        ),
                ),
            Dialog: () => Option.none(),
        }),
    );
    return Option.firstSomeOf([halted, Outcome.fields.completion.match(row.completion, { Ran: () => Option.some({ value }), Exited: () => Option.some({ value }), Thrown: ({ message }) => Option.some({ thrown: message }), Failed: () => Option.none() })]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Asking, answerOf, Fact };
