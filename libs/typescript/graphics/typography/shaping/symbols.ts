import { Array, Effect, Predicate, Record, Schema, SchemaTransformation, Stream, Tuple } from 'effect';
import { Csv } from 'effect/encoding';
import type { Face } from '#/shaping/face';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEXADECIMAL = 16;
const HEX_CELL = /^(?:0[Xx])?[\dA-Fa-f]{1,6}$/u;
const MAX_CODE_POINT = 0x10_ff_ff;
const SCALAR_VALUE = /^\P{Cs}$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Character = Schema.String.check(Schema.isPattern(HEX_CELL)).pipe(
    Schema.decodeTo(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: MAX_CODE_POINT })), SchemaTransformation.transform({ decode: (cell) => Number.parseInt(cell, HEXADECIMAL), encode: (point) => point.toString(HEXADECIMAL) })),
    Schema.decodeTo(Schema.String.check(Schema.isPattern(SCALAR_VALUE)), SchemaTransformation.transform({ decode: String.fromCodePoint, encode: (character) => character.codePointAt(0) ?? 0 })),
);
class Symbols extends Schema.Class<Symbols, { readonly brand: unique symbol }>('Symbols')({ sets: Schema.Record(Schema.String, Schema.Array(Character)).annotate({ description: 'Characters by header name, cells hexadecimal code points after an optional 0x' }) }) {
    static readonly decode = Effect.fn('Symbols.decode')(function* (tsv: string) {
        const rows = yield* Stream.make(tsv).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: '\t', headers: true, ignoreEmptyLines: true })), Stream.runCollect);
        const columns = Array.groupBy(rows.flatMap(Record.toEntries), Tuple.get(0));
        return yield* Schema.decodeEffect(Symbols)({ sets: Record.map(columns, (entries) => Array.map(entries, Tuple.get(1)).filter(Predicate.isTruthy)) });
    });

    supported(face: Pick<Face, 'draws'>) {
        return Symbols.make({ sets: Record.map(this.sets, (characters) => characters.filter((character) => face.draws(character))) });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Symbols };
