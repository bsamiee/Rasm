import { Digits } from '@rasm/search/digits';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Order, Record, Schema, SchemaGetter, Stream, Struct } from 'effect';
import { Csv } from 'effect/encoding';

// --- [MODELS] --------------------------------------------------------------------------

const Ayah = Schema.Struct({ number: Schema.Int, text: Schema.String });
const Surah = Schema.Struct({ number: Schema.Int, name: Schema.String, ayahs: Schema.Array(Ayah) });
const Numeral = Schema.String.pipe(Schema.decodeTo(Schema.NumberFromString.check(Schema.isInt()), { decode: SchemaGetter.transform(Digits.fold), encode: SchemaGetter.passthrough() }));
const AyahRows = Schema.Array(Schema.Struct({ surah: Numeral, number: Numeral, text: Schema.String }));
const SurahRows = Schema.Array(Schema.Struct({ number: Numeral, name: Schema.String }));
class Quran extends Schema.Class<Quran, { readonly brand: unique symbol }>('Quran')({ surahs: Schema.Array(Surah).annotate({ description: 'Sorted by number' }) }) {
    static readonly Surah = Surah;
    static readonly Columns = Schema.Struct({
        ayahs: Schema.Struct(Struct.Record(['surah', 'number', 'text'], Schema.String.annotate({ description: 'Header name' }))),
        surahs: Schema.Struct(Struct.Record(['number', 'name'], Schema.String.annotate({ description: 'Header name' }))),
    });
    static readonly decode = Effect.fn('Quran.decode')(function* (files: { readonly ayahs: string; readonly surahs: string }, columns: typeof Quran.Columns.Type) {
        const [ayahs, surahs] = yield* Effect.all([rows('ayahs', files.ayahs, columns.ayahs).pipe(Effect.flatMap(Schema.decodeUnknownEffect(AyahRows))), rows('surahs', files.surahs, columns.surahs).pipe(Effect.flatMap(Schema.decodeUnknownEffect(SurahRows)))]);
        const groups = Array.groupBy(ayahs, ({ surah }) => String(surah));
        const named = Array.dedupeWith(surahs, (left, right) => left.number === right.number);
        yield* Effect.forEach(Record.keys(groups), (surah) => Unmatched.single(named, 'surah', Option.some(surah), ({ number }) => [String(number)]));
        return Quran.make({
            surahs: Array.sort(
                named.map(({ number, name }) => ({ number, name, ayahs: Array.map(groups[String(number)] ?? [], Struct.omit(['surah'])) })),
                Order.mapInput(Order.Number, ({ number }: typeof Surah.Type) => number),
            ),
        });
    });
    static readonly text = (surah: typeof Surah.Type, ayah: number | 'all', separator: string): Effect.Effect<string, Unmatched> =>
        Effect.map(
            ayah === 'all'
                ? Effect.succeed(surah.ayahs)
                : Effect.map(
                      Unmatched.single(surah.ayahs, `ayah in ${surah.name}`, Option.some(String(ayah)), ({ number }) => [String(number)]),
                      Array.of,
                  ),
            (selected) => selected.map(({ text }) => `${text}${separator}`).join(''),
        );

    surah(query: string) {
        return Unmatched.single(this.surahs, 'surah', Option.some(query), ({ name, number }) => [name, String(number)]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rows = Effect.fnUntraced(function* (subject: string, file: string, columns: Record.ReadonlyRecord<string, string>) {
    const records = yield* Stream.make(file).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: ',' })), Stream.runCollect);
    const headers = Array.map(
        Option.getOrElse(Array.head(records), () => []),
        (name, index) => ({ name, index }),
    );
    const cells = yield* Effect.all(Record.map(columns, (header) => Unmatched.single(headers, `${subject} header`, Option.some(header), ({ name }) => [name])));
    return records.slice(1).map((record) => Record.map(cells, ({ index }) => record[index]));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Quran };
