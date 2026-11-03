import { Array, Effect, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { Csv } from 'effect/encoding';
import type { Face } from '#/face';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEXADECIMAL = 16;
const MAX_CODE_POINT = 0x10_ff_ff;

// --- [MODELS] --------------------------------------------------------------------------

const CodePoint = Schema.String.pipe(Schema.decodeTo(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: MAX_CODE_POINT })), SchemaTransformation.transform({ decode: (cell) => Number.parseInt(cell, HEXADECIMAL), encode: (point) => point.toString(HEXADECIMAL) })));
class Symbols extends Schema.Class<Symbols, { readonly brand: unique symbol }>('Symbols')({ sets: Schema.Record(Schema.String, Schema.Array(CodePoint)).annotate({ description: 'Code points by header name' }) }) {
    static readonly decode = Effect.fn('Symbols.decode')(function* (file: string) {
        const rows = yield* Stream.make(file).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: '\t', headers: true, ignoreEmptyLines: true })), Stream.runCollect);
        const columns = Array.groupBy(
            rows.flatMap((row) => Record.toEntries(row)),
            ([header]) => header,
        );
        return yield* Schema.decodeEffect(Symbols)({ sets: Record.map(columns, (cells) => cells.map(([, cell]) => cell).filter((cell) => cell !== '')) });
    });

    supported(face: Face) {
        return Symbols.make({ sets: Record.map(this.sets, (points) => points.filter((point) => face.has(point))) });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Symbols };
