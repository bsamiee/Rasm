import { Array, Effect, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { Csv } from 'effect/encoding';
import type { Face } from '#/face';

// --- [CONSTANTS] -----------------------------------------------------------------------

const hexadecimal = 16;
const lastCodePoint = 0x10_ff_ff;

// --- [MODELS] --------------------------------------------------------------------------

const CodePoint = Schema.String.pipe(Schema.decodeTo(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: lastCodePoint })), SchemaTransformation.transform({ decode: (cell) => Number.parseInt(cell, hexadecimal), encode: (point) => point.toString(hexadecimal) })));
class Symbols extends Schema.Class<Symbols, { readonly brand: unique symbol }>('Symbols')({ sets: Schema.Record(Schema.String, Schema.Array(CodePoint)).annotate({ description: 'Code points by header name' }) }) {
    static readonly decode = Effect.fn('Symbols.decode')(function* (file: string) {
        const rows = yield* Stream.make(file).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: '\t', headers: true })), Stream.runCollect);
        return yield* Schema.decodeUnknownEffect(Symbols)({ sets: Record.fromIterableWith(Array.take(rows, 1).flatMap(Record.keys), (header) => [header, rows.map((row) => row[header]).filter((cell) => cell !== '')]) });
    });
    static readonly text = String.fromCodePoint;

    supported(face: Face) {
        return Symbols.make({ sets: Record.map(this.sets, (points) => points.filter((point) => face.has(point))) });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Symbols };
