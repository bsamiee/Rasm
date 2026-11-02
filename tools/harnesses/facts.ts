import { Array, Effect, FileSystem, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Key {
    readonly question: string;
    readonly application: string;
    readonly version: string;
    readonly engine: unknown;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = <E extends Schema.Top>(engine: E) => {
    const Fact = Schema.Struct({
        question: Schema.NonEmptyString.annotate({ description: 'Host behavior asked, matched exactly before any host call' }),
        application: Schema.NonEmptyString,
        version: Schema.NonEmptyString.annotate({ description: 'Version the host reports for itself' }),
        engine,
        expression: Schema.NonEmptyString,
        precondition: Schema.optionalKey(Schema.Json),
        answer: Schema.Union([Schema.Struct({ value: Schema.Json }), Schema.Struct({ thrown: Schema.String })]),
        asked: Schema.String.annotate({ description: 'ISO 8601 instant of the host call' }),
    });
    const Index = Schema.fromJsonString(Schema.Struct({ facts: Schema.Array(Fact) }), { space: 2 });
    const load = Effect.fn('load')(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        return (yield* fs.exists(file)) ? (yield* Schema.decodeEffect(Index)(yield* fs.readFileString(file))).facts : [];
    });
    const known = <F extends Key>(rows: readonly F[], key: Key) => Array.findFirst(rows, (row) => row.question === key.question && row.application === key.application && row.version === key.version && row.engine === key.engine);
    const record = Effect.fn('record')(function* (file: string, fact: typeof Fact.Type) {
        const fs = yield* FileSystem.FileSystem;
        const rows = yield* load(file);
        yield* fs.writeFileString(file, `${yield* Schema.encodeEffect(Index)({ facts: [...rows, fact] })}\n`);
    });
    return { Fact, Index, load, known, record };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { facts };
