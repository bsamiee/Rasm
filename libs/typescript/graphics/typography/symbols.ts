import { Array, Effect, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { Csv } from 'effect/encoding';
import type { Face } from '#/face';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEXADECIMAL = 16;
const HEX_DIGITS = /^[\dA-Fa-f]{1,6}$/u;
const MAX_CODE_POINT = 0x10_ff_ff;
const SURROGATES = { minimum: 0xd8_00, maximum: 0xdf_ff };

// --- [MODELS] --------------------------------------------------------------------------

const Character = Schema.String.check(Schema.isPattern(HEX_DIGITS)).pipe(
    Schema.decodeTo(
        Schema.Int.check(
            Schema.isBetween({ minimum: 0, maximum: MAX_CODE_POINT }),
            Schema.makeFilter((point) => point < SURROGATES.minimum || point > SURROGATES.maximum, { description: 'Unicode scalar value, surrogates excluded' }),
        ),
        SchemaTransformation.transform({ decode: (cell) => Number.parseInt(cell, HEXADECIMAL), encode: (point) => point.toString(HEXADECIMAL) }),
    ),
    Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: String.fromCodePoint, encode: (character) => character.codePointAt(0) ?? 0 })),
);
class Symbols extends Schema.Class<Symbols, { readonly brand: unique symbol }>('Symbols')({ sets: Schema.Record(Schema.String, Schema.Array(Character)).annotate({ description: 'Characters by header name, cells hexadecimal code points' }) }) {
    static readonly decode = Effect.fn('Symbols.decode')(function* (tsv: string) {
        const rows = yield* Stream.make(tsv).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: '\t', headers: true, ignoreEmptyLines: true })), Stream.runCollect);
        const columns = Array.groupBy(
            rows.flatMap((row) => Record.toEntries(row)),
            ([header]) => header,
        );
        return yield* Schema.decodeEffect(Symbols)({ sets: Record.map(columns, (cells) => cells.map(([, cell]) => cell).filter((cell) => cell !== '')) });
    });

    supported(face: Face) {
        return Symbols.make({ sets: Record.map(this.sets, (characters) => characters.filter((character) => face.has(character.codePointAt(0) ?? 0))) });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Symbols };
