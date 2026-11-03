import { Digits } from '@rasm/search/digits';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Order, Record, Schema, Stream, Struct } from 'effect';
import { Csv } from 'effect/encoding';

// --- [MODELS] --------------------------------------------------------------------------

const Ayah = Schema.Struct({ number: Schema.Int, text: Schema.String });
const Surah = Schema.Struct({ number: Schema.Int, name: Schema.String, ayahs: Schema.Array(Ayah) });
const AyahRow = Schema.Struct({ surah: Digits.Numeral, number: Digits.Numeral, text: Schema.String });
const SurahRow = Schema.Struct({ number: Digits.Numeral, name: Schema.String });
const Header = Schema.String.annotate({ description: 'Header name' });
class Quran extends Schema.Class<Quran, { readonly brand: unique symbol }>('Quran')({ surahs: Schema.Array(Surah).annotate({ description: 'Sorted by number' }) }) {
    static readonly Surah = Surah;
    static readonly Columns = Schema.Struct({ ayahs: Schema.Struct(Struct.Record(Struct.keys(AyahRow.fields), Header)), surahs: Schema.Struct(Struct.Record(Struct.keys(SurahRow.fields), Header)) });
    static readonly decode = Effect.fn('Quran.decode')(function* (files: { readonly ayahs: string; readonly surahs: string }, columns: typeof Quran.Columns.Type) {
        const [ayahs, surahs] = yield* Effect.all([rows(AyahRow, files.ayahs, columns.ayahs), rows(SurahRow, files.surahs, columns.surahs)]);
        const ayahsBySurah = Array.groupBy(ayahs, ({ surah }) => String(surah));
        const uniqueSurahs = Array.dedupeWith(surahs, (left, right) => left.number === right.number);
        yield* Effect.forEach(Record.keys(ayahsBySurah), (surah) => Unmatched.single(uniqueSurahs, 'surah', Option.some(surah), ({ number }) => [String(number)]), { discard: true });
        return Quran.make({
            surahs: Array.sortWith(
                uniqueSurahs.map(({ number, name }) => ({ number, name, ayahs: Array.map(ayahsBySurah[String(number)] ?? [], Struct.omit(['surah'])) })),
                Struct.get('number'),
                Order.Number,
            ),
        });
    });
    static readonly text = (surah: typeof Surah.Type, ayah: number | 'all', separator: string): Effect.Effect<string, Unmatched> =>
        Effect.map(
            Unmatched.matching(surah.ayahs, `ayah in ${surah.name}`, ayah === 'all' ? Option.none() : Option.some(String(ayah)), ({ number }) => [String(number)]),
            (selected) => selected.map(({ text }) => `${text}${separator}`).join(''),
        );

    surah(query: string) {
        return Unmatched.single(this.surahs, 'surah', Option.some(query), ({ name, number }) => [name, String(number)]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rows = <F extends Schema.Struct.Fields>(row: Schema.Struct<F>, file: string, columns: { readonly [K in keyof F]: string }) => Stream.make(file).pipe(Stream.pipeThroughChannel(Csv.decodeSchemaString(row.pipe(Schema.encodeKeys(columns)))({ headers: true, ignoreEmptyLines: true })), Stream.runCollect);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Quran };
