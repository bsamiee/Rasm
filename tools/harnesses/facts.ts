import { rank } from '@rasm/search/rank';
import { Array, Effect, FileSystem, flow, Option, Schema, String } from 'effect';
import { Answer, Common, Completion, deciding, type Outcome } from '#/results';
import type { Any } from '#/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MACHINE = /^(?:Security|TCC|LaunchServices) /u;
const RUNTIME = /^\S+\.dylib /u;

// --- [MODELS] --------------------------------------------------------------------------

const Interrupted = Schema.TemplateLiteral([Schema.Literals(['crashed', 'hung']), ' ', Schema.String]).annotate({ description: 'Thrown answer of a case that ended or stalled its instance' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = (engine: Any) => {
    const Fact = Schema.Struct({
        question: Schema.NonEmptyString.annotate({ description: 'Host behavior asked, matched exactly before any host call' }),
        ...deciding(engine),
        expression: Schema.NonEmptyString,
        args: Schema.optionalKey(Schema.Array(Schema.Json)),
        precondition: Schema.optionalKey(Schema.Json),
        answer: Answer,
        asked: Schema.String.annotate({ description: 'ISO 8601 instant of the host call' }),
    });
    type Key = Pick<typeof Fact.Type, 'question' | 'identifier' | 'version' | 'engine'> & Partial<Pick<typeof Fact.Type, 'host' | 'expression' | 'args' | 'precondition'>>;
    const same = Schema.toEquivalence(Schema.UndefinedOr(Schema.Json));
    const Index = Schema.fromJsonString(Schema.Struct({ facts: Schema.Array(Fact) }), { space: 2 });
    const load = Effect.fn('load')(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        return (yield* fs.exists(file)) ? (yield* Schema.decodeEffect(Index)(yield* fs.readFileString(file))).facts : [];
    });
    const matches = (row: Key, key: Key) => row.question === key.question && row.identifier === key.identifier && row.version === key.version && row.engine === key.engine && (['host', 'expression', 'args', 'precondition'] as const).every((field) => key[field] === undefined || same(row[field], key[field]));
    const known = (rows: readonly (typeof Fact.Type)[], key: Key) => Array.findFirst(rows, (row) => matches(row, key));
    const faulted = (rows: readonly (typeof Fact.Type)[], key: Omit<Key, 'question'>) => Array.findFirst(rows, (row) => 'thrown' in row.answer && Schema.is(Interrupted)(row.answer.thrown) && matches(row, { ...key, question: row.question }));
    const record = Effect.fn('record')(function* (file: string, made: readonly (typeof Fact.Type)[]) {
        const fs = yield* FileSystem.FileSystem;
        const rows = yield* load(file);
        const replaced = (row: Key) => made.some((next) => matches(row, { question: next.question, identifier: next.identifier, version: next.version, engine: next.engine }) && same(row.host, next.host));
        yield* fs.writeFileString(file, `${yield* Schema.encodeEffect(Index)({ facts: [...rows.filter((row) => !replaced(row)), ...made] })}\n`);
    });
    return { Fact, load, known, faulted, record };
};
const answerOf = (row: typeof Outcome.Type): Option.Option<typeof Answer.Type> => {
    const held: Schema.Json = row.value ?? null;
    const value = Option.match(
        Option.filter(Schema.decodeUnknownOption(Common.fields.alerts)(row.alerts), (alerts) => alerts.length > 0),
        { onNone: (): Schema.Json => held, onSome: (alerts): Schema.Json => ({ value: held, alerts: alerts.map(({ text }) => text) }) },
    );
    return Option.firstSomeOf([
        Option.map(Option.fromUndefinedOr(row.crashed), ({ signal, status, type, report }) => ({ thrown: `crashed ${[signal === undefined ? `status ${status}` : `signal ${signal}`, type, report].filter((part) => part !== undefined).join(' ')}` })),
        Option.map(
            Option.filter(
                Option.fromUndefinedOr(row.hung),
                ({ frames }) =>
                    !Option.exists(
                        Array.findFirst(frames, (frame) => !RUNTIME.test(frame)),
                        flow(String.match(MACHINE), Option.isSome),
                    ),
            ),
            ({ cpu, sample }) => ({ thrown: `hung ${cpu} ${sample}` }),
        ),
        Completion.match(row.completion, { Ran: () => Option.some({ value }), Exited: () => Option.some({ value }), Thrown: ({ message }) => Option.some({ thrown: message }), Failed: () => Option.none() }),
    ]);
};
const statements = Effect.fn('statements')(function* (files: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const lines = Array.flatMap(yield* Effect.forEach(files, (file) => fs.readFileString(file)), (text) => Array.fromIterable(String.linesIterator(text)).filter((line) => line.trim() !== ''));
    return (question: string) => rank(question, lines);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { answerOf, facts, statements };
