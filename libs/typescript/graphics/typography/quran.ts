import { Digits } from '@rasm/search/digits';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Order, Record, Schema, Stream, Struct } from 'effect';
import { Csv } from 'effect/encoding';

// --- [MODELS] --------------------------------------------------------------------------

const headers = <F extends Schema.Struct.Fields>(row: Schema.Struct<F>) => Schema.Struct(Struct.Record(Struct.keys(row.fields), Schema.String.annotate({ description: 'Header name' })));
const Ayah = Schema.Struct({ number: Schema.Int, text: Schema.String });
const AyahRow = Schema.Struct({ surah: Digits.Numeral, number: Digits.Numeral, text: Schema.String });
const SurahRow = Schema.Struct({ number: Digits.Numeral, name: Schema.String });
class Surah extends Schema.Class<Surah, { readonly brand: unique symbol }>('Surah')({ number: Schema.Int, name: Schema.String, ayahs: Schema.Array(Ayah).annotate({ description: 'Sorted by number, one per number' }) }) {
    text(ayah: Option.Option<number>, separator: string): Effect.Effect<string, Unmatched> {
        return Effect.map(
            Unmatched.matching(this.ayahs, `ayah in ${this.name}`, Option.map(ayah, String), ({ number }) => [String(number)]),
            (selected) => selected.map(({ text }) => `${text}${separator}`).join(''),
        );
    }
}
class Quran extends Schema.Class<Quran, { readonly brand: unique symbol }>('Quran')({ surahs: Schema.Array(Surah).annotate({ description: 'Sorted by number' }) }) {
    static readonly Columns = Schema.Struct({ ayahs: headers(AyahRow), surahs: headers(SurahRow) });
    static readonly decode = Effect.fn('Quran.decode')(function* (csv: { readonly ayahs: string; readonly surahs: string }, columns: typeof Quran.Columns.Type) {
        const [ayahs, surahs] = yield* Effect.all([rows(AyahRow, csv.ayahs, columns.ayahs), rows(SurahRow, csv.surahs, columns.surahs)]);
        const ayahsBySurah = Array.groupBy(ayahs, ({ surah }) => String(surah));
        const uniqueSurahs = Array.dedupeWith(surahs, (left, right) => left.number === right.number);
        const missing = Array.difference(Record.keys(ayahsBySurah), uniqueSurahs.map(({ number }) => String(number)));
        yield* Array.match(missing, { onEmpty: () => Effect.void, onNonEmpty: (keys) => Unmatched.make({ subject: 'surah', query: Option.some(keys.join(', ')), candidates: uniqueSurahs.map(({ number }) => String(number)) }) });
        return Quran.make({
            surahs: Array.sortWith(
                uniqueSurahs.map(({ number, name }) => Surah.make({ number, name, ayahs: Array.dedupeWith(Array.sortWith(Array.map(ayahsBySurah[String(number)] ?? [], Struct.omit(['surah'])), Struct.get('number'), Order.Number), (left, right) => left.number === right.number) })),
                Struct.get('number'),
                Order.Number,
            ),
        });
    });

    surah(query: string) {
        return Unmatched.single(this.surahs, 'surah', Option.some(query), ({ name, number }) => [name, String(number)]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rows = <F extends Schema.Struct.Fields>(row: Schema.Struct<F>, csv: string, columns: { readonly [K in keyof F]: string }) => Stream.make(csv).pipe(Stream.pipeThroughChannel(Csv.decodeSchemaString(row.pipe(Schema.encodeKeys(columns)))({ headers: true, ignoreEmptyLines: true })), Stream.runCollect);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Quran };
