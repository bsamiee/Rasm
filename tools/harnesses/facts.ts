import { ranked } from '@rasm/search/ranked';
import { Array, Effect, FileSystem, Option, Schema, String } from 'effect';
import { Answer, Completion, type Crash, type Hang } from '#/results';

// --- [MODELS] --------------------------------------------------------------------------

const Keyed = Schema.Struct({ question: Schema.String, identifier: Schema.String, version: Schema.String, engine: Schema.Unknown, host: Schema.optionalKey(Schema.Json) });
const Interrupted = Schema.TemplateLiteral([Schema.Literals(['crashed', 'hung']), ' ', Schema.String]).annotate({ description: 'Thrown answer of a case that ended or stalled its instance' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = <E extends Schema.Top>(engine: E) => {
    const Fact = Schema.Struct({
        question: Schema.NonEmptyString.annotate({ description: 'Host behavior asked, matched exactly before any host call' }),
        identifier: Schema.NonEmptyString.annotate({ description: 'Bundle identifier of the installation asked' }),
        version: Schema.NonEmptyString.annotate({ description: 'Version the installation reports' }),
        engine,
        host: Schema.optionalKey(Schema.Json.annotate({ description: 'Host state beside the version that decides answers, locale and feature set among them' })),
        expression: Schema.NonEmptyString,
        precondition: Schema.optionalKey(Schema.Json),
        answer: Answer,
        asked: Schema.String.annotate({ description: 'ISO 8601 instant of the host call' }),
    });
    interface Key {
        readonly question: string;
        readonly identifier: string;
        readonly version: string;
        readonly engine: E['Type'];
        readonly host?: Schema.Json;
        readonly expression?: string;
        readonly precondition?: Schema.Json;
    }
    type Row = Key & { readonly answer: typeof Answer.Type };
    const same = Schema.toEquivalence(Schema.UndefinedOr(Schema.Json));
    const Index = Schema.fromJsonString(Schema.Struct({ facts: Schema.Array(Fact) }), { space: 2 });
    const load = Effect.fn('load')(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        return (yield* fs.exists(file)) ? (yield* Schema.decodeEffect(Index)(yield* fs.readFileString(file))).facts : [];
    });
    const matches = (row: Key, key: Key) => row.question === key.question && row.identifier === key.identifier && row.version === key.version && row.engine === key.engine && (['host', 'expression', 'precondition'] as const).every((field) => key[field] === undefined || same(row[field], key[field]));
    const known = <F extends Row>(rows: readonly F[], key: Key) => Array.findFirst(rows, (row) => matches(row, key));
    const faulted = <F extends Row>(rows: readonly F[], key: Omit<Key, 'question'>) => Array.findFirst(rows, (row) => 'thrown' in row.answer && Schema.is(Interrupted)(row.answer.thrown) && matches(row, { ...key, question: row.question }));
    const record = Effect.fn('record')(function* (file: string, made: readonly (typeof Fact.Type & Key)[]) {
        const fs = yield* FileSystem.FileSystem;
        const rows = yield* load(file);
        const replaced = (row: Key) => made.some((next: Key) => matches(row, { question: next.question, identifier: next.identifier, version: next.version, engine: next.engine }) && same(row.host, next.host));
        yield* fs.writeFileString(file, `${yield* Schema.encodeEffect(Index)({ facts: [...rows.filter((row) => !(Schema.is(Keyed)(row) && replaced(row))), ...made] })}\n`);
    });
    return { Fact, load, known, faulted, record };
};
const answerOf = (row: { readonly completion: typeof Completion.Type; readonly crashed?: typeof Crash.Type; readonly hung?: typeof Hang.Type }, value: Schema.Json): Option.Option<typeof Answer.Type> =>
    Option.firstSomeOf([
        Option.map(Option.fromUndefinedOr(row.crashed), ({ signal, status, type, report }) => ({ thrown: `crashed ${[signal === undefined ? `status ${status}` : `signal ${signal}`, type, report].filter((part) => part !== undefined).join(' ')}` })),
        Option.map(Option.fromUndefinedOr(row.hung), ({ cpu, sample }) => ({ thrown: `hung ${cpu} ${sample}` })),
        Completion.match(row.completion, { Ran: () => Option.some({ value }), Exited: () => Option.some({ value }), Thrown: ({ message }) => Option.some({ thrown: message }), Failed: () => Option.none() }),
    ]);
const statements = Effect.fn('statements')(function* (files: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const lines = Array.flatMap(yield* Effect.forEach(files, (file) => fs.readFileString(file)), (text) => Array.fromIterable(String.linesIterator(text)).filter((line) => line.trim() !== ''));
    return (question: string) => ranked(question, lines);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { answerOf, facts, statements };
