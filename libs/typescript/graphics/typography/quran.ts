import { AYAH_MARK, ayahMark, type MarkKind, Mushaf } from '@quran.ws/text';
import { Digits, Unmatched } from '@rasm/search';
import { Array, Effect, Equivalence, Number, Option, Order, pipe, Record, Schema, Stream, Struct } from 'effect';
import { Csv } from 'effect/encoding';
import { findSurahAyahByAyahId, formatSurahAyah, generatePartBlocks, getListsOfRiwaya, isValidSurah, isValidSurahAyah, type PartBlock, partNames, type RiwayaData, type RiwayaName, type RiwayasNames, type SurahAyah, type SurahNames, surahNamesAr, surahNamesEn } from 'quran-meta';
import type { Settings } from '#/settings';
import type { Face } from '#/shaping/face';
import { TypographyError } from '#/typography-error';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PARTS = [...partNames, 'hizb'] as const;
const NAMES: readonly SurahNames[] = [surahNamesAr, surahNamesEn];

// --- [MODELS] --------------------------------------------------------------------------

const headers = <F extends Schema.Struct.Fields>(row: Schema.Struct<F>) => Schema.Struct(Struct.Record(Struct.keys(row.fields), Schema.String.annotate({ description: 'Header name' })));
const AyahRow = Schema.Struct({ surah: Digits.Numeral, number: Digits.Numeral, text: Schema.String });
const SurahRow = Schema.Struct({ number: Digits.Numeral, name: Schema.String });
const Ordinal = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1));
const Riwayah = Schema.Literals(['Bazzi', 'Douri', 'Hafs', 'Qalun', 'Qunbul', 'Shuba', 'Sousi', 'Warsh'] as const satisfies Readonly<RiwayasNames>).annotate({ description: 'Riwayah counting surah and ayah numbers and divisions' });
const Bounds = Schema.Struct({
    from: Schema.OptionFromOptionalKey(Ordinal.annotate({ description: 'First ayah counted within the division, its start when absent' })),
    to: Schema.OptionFromOptionalKey(Ordinal.annotate({ description: 'Last ayah counted within the division, its end when absent' })),
});
class Ayah extends Schema.Class<Ayah, { readonly brand: unique symbol }>('Ayah')({
    number: Schema.Int,
    text: Schema.String,
    basmalah: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Unnumbered basmalah the mushaf prints before the ayah' })),
}) {
    marked(separator: string, face: Pick<Face, 'has'>, settings: Pick<Settings, 'digits'>): Effect.Effect<string, Unmatched> {
        const mark = settings.digits(ayahMark(this.number));
        const forms = [mark, `\u{FD3F}${mark.slice(AYAH_MARK.length)}\u{FD3E}`];
        return Effect.map(
            Effect.fromOption(
                Array.findFirst(forms, (form) => Array.fromIterable(form).every((character) => face.has(character.codePointAt(0) ?? 0))),
                () => Unmatched.make({ subject: 'ayah mark the face draws', query: Option.some(String(this.number)), candidates: forms }),
            ),
            (form) =>
                Array.append(Option.toArray(this.basmalah), `${this.text}\u{A0}${form}`)
                    .map((line) => `${line}${separator}`)
                    .join(''),
        );
    }
}
class Surah extends Schema.Class<Surah, { readonly brand: unique symbol }>('Surah')({
    number: Schema.Int,
    names: Schema.NonEmptyArray(Schema.String),
    ayahs: Schema.Array(Ayah).annotate({ description: 'Sorted by number, one per number' }),
}) {}
class Quran extends Schema.Class<Quran, { readonly brand: unique symbol }>('Quran')(
    Schema.Struct({ riwayah: Schema.OptionFromOptionalKey(Riwayah), surahs: Schema.Array(Surah).annotate({ description: 'Sorted by number' }) }).check(
        Schema.makeFilter(
            ({ riwayah, surahs }) =>
                Option.match(riwayah, {
                    onNone: () => true,
                    onSome: (name) => {
                        const found = surahs.flatMap(uncounted(getListsOfRiwaya(name)));
                        return Array.isReadonlyArrayEmpty(found) || `${name} counts no ${found.join(', ')}`;
                    },
                }),
            { description: 'Surah and ayah numbers the riwayah counts' },
        ),
    ),
) {
    static readonly Columns = Schema.Struct({ ayahs: headers(AyahRow), surahs: headers(SurahRow) });
    static readonly Selection = Schema.TaggedUnion({
        Surah: { surah: Schema.String.annotate({ description: 'Surah name or number' }), ...Bounds.fields },
        Part: { part: Schema.Literals(PARTS).annotate({ description: 'Division the riwayah counts' }), number: Ordinal, ...Bounds.fields },
    }).annotate({ description: 'Ayahs of a surah or numbered division, within ayah bounds' });
    static readonly decode = Effect.fn('Quran.decode')(function* (csv: { readonly ayahs: string; readonly surahs: string }, columns: typeof Quran.Columns.Type, riwayah: Option.Option<typeof Riwayah.Type> = Option.none(), tables: readonly SurahNames[] = NAMES) {
        const rows = <F extends Schema.Struct.Fields>(row: Schema.Struct<F>, text: string, headed: { readonly [K in keyof F]: string }) => Stream.make(text).pipe(Stream.pipeThroughChannel(Csv.decodeSchemaString(row.pipe(Schema.encodeKeys(headed)))({ headers: true, ignoreEmptyLines: true })), Stream.runCollect);
        const [ayahs, surahs] = yield* Effect.all([rows(AyahRow, csv.ayahs, columns.ayahs), rows(SurahRow, csv.surahs, columns.surahs)]);
        const ayahsBySurah = Map.groupBy(ayahs, Struct.get('surah'));
        const named = Array.dedupeWith(surahs, sameNumber);
        const numbers = named.map(Struct.get('number'));
        yield* Array.match(Array.difference(Array.fromIterable(ayahsBySurah.keys()), numbers), { onEmpty: () => Effect.void, onNonEmpty: (keys) => Unmatched.make({ subject: 'surah', query: Option.some(keys.join(', ')), candidates: numbers.map(String) }) });
        return yield* Schema.decodeEffect(Quran)({
            ...Record.getSomes({ riwayah }),
            surahs: Array.sortWith(
                named.map(({ number, name }) => ({ number, names: titled(number, [name], tables), ayahs: Array.dedupeAdjacentWith(Array.sortWith(Array.map(ayahsBySurah.get(number) ?? [], Struct.omit(['surah'])), Struct.get('number'), Order.Number), sameNumber) })),
                Struct.get('number'),
                Order.Number,
            ),
        });
    });
    static readonly Render = Schema.Struct({
        marks: Schema.optionalKey(Schema.Union([Schema.Boolean, Schema.mutable(Schema.Array(Schema.Literals(['waqf', 'division', 'sajdah', 'sajdah_line', 'sah', 'raised_dot'] as const satisfies readonly MarkKind[])))]).annotate({ description: 'Signs printed against words, true for every kind' })),
        lines: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Break text where printed lines break' })),
    }).annotate({ description: 'Mushaf text rendering, ayah marks set against the face' });
    static readonly mushaf = Effect.fn('Quran.mushaf')(function* (json: Option.Option<string> = Option.none(), render: typeof Quran.Render.Type = {}, tables: readonly SurahNames[] = NAMES) {
        const mushaf = yield* Effect.tryPromise({ try: () => Promise.resolve(Option.match(json, { onNone: Mushaf.hafs, onSome: Mushaf.fromJson })), catch: (cause) => TypographyError.make({ subject: 'mushaf', cause }) });
        const keys = { bazzi: 'Bazzi', duri: 'Douri', hafs: 'Hafs', qalun: 'Qalun', shubah: 'Shuba', susi: 'Sousi', warsh: 'Warsh' } as const satisfies Record.ReadonlyRecord<string, RiwayaName>;
        const [, riwayah] = yield* Unmatched.single(Record.toEntries(keys), 'mushaf key', Option.some(mushaf.key), ([key]) => Array.of(key));
        const encoded = (surah: Mushaf['surahs'][number]) => {
            const basmalah = Option.map(Option.fromNullOr(surah.basmalah), (span) => span.render(render));
            return { number: surah.number, names: titled(surah.number, [surah.nameAr, surah.nameEn], tables), ayahs: surah.ayahs.map((ayah) => ({ number: ayah.number, text: ayah.render(render), ...Record.getSomes({ basmalah: ayah.number === 1 ? basmalah : Option.none() }) })) };
        };
        return yield* Schema.decodeEffect(Quran)({ riwayah, surahs: mushaf.surahs.map(encoded) });
    });

    surah(query: string) {
        return Unmatched.single(this.surahs, 'surah', Option.some(query), ({ names, number }) => Array.append(names, String(number)));
    }
    ayah(reference: SurahAyah): Effect.Effect<Ayah, Unmatched> {
        const [surah, number] = reference;
        return Effect.fromOption(
            Option.flatMap(
                Array.findFirst(this.surahs, (candidate) => candidate.number === surah),
                ({ ayahs }) => Array.findFirst(ayahs, (ayah) => ayah.number === number),
            ),
            () => Unmatched.make({ subject: 'ayah text', query: Option.some(formatSurahAyah(reference)) }),
        );
    }
    select(selection: typeof Quran.Selection.Type): Effect.Effect<readonly Ayah[], Unmatched> {
        return Quran.Selection.match(selection, {
            Surah: ({ surah, from, to }) => Effect.flatMap(this.surah(surah), (found) => (Option.isNone(this.riwayah) ? within(found, { from, to }) : counted(this, { _tag: 'Part', part: 'surah', number: found.number, from, to }))),
            Part: (part) => counted(this, part),
        });
    }
    text(selection: typeof Quran.Selection.Type, separator: string, face: Pick<Face, 'has'>, settings: Pick<Settings, 'digits'>) {
        return this.select(selection).pipe(Effect.flatMap(Effect.forEach((ayah) => ayah.marked(separator, face, settings))), Effect.map(Array.join('')));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const sameNumber: Equivalence.Equivalence<{ readonly number: number }> = Equivalence.mapInput(Equivalence.Number, Struct.get('number'));
const titled = (number: number, names: Array.NonEmptyReadonlyArray<string>, tables: readonly SurahNames[]) =>
    Array.dedupe(
        Array.appendAll(
            names,
            tables.flatMap((table) => Option.getOrElse(Array.get(table, number), Array.empty)),
        ),
    );
const uncounted =
    (data: RiwayaData) =>
    ({ number, ayahs }: Surah): readonly string[] =>
        isValidSurah(number, data.meta) ? ayahs.filter((ayah) => !isValidSurahAyah([number, ayah.number], data)).map((ayah) => `${number}:${ayah.number}`) : [String(number)];
const blocks = (part: (typeof PARTS)[number], data: RiwayaData): readonly PartBlock[] =>
    part === 'hizb' ? Array.chunksOf(blocks('rubAlHizb', data), data.meta.numRubAlHizbs / data.meta.numHizbs).map((hizb) => ({ startAyahId: Array.headNonEmpty(hizb).startAyahId, ayahCount: Number.sumAll(hizb.map(Struct.get('ayahCount'))) })) : (generatePartBlocks(part, data) ?? []);
const spanned = (subject: string, { from, to }: typeof Bounds.Type, numbers: readonly number[]) => {
    const end = Option.getOrElse(Array.last(numbers), () => 0);
    const range = { minimum: Option.getOrElse(from, () => 1), maximum: Option.getOrElse(to, () => end) };
    return range.maximum <= end && numbers.some(Number.between(range)) ? Effect.succeed(range) : Unmatched.make({ subject, query: Option.some(`${range.minimum}-${range.maximum}`), candidates: numbers.map(String) });
};
const within = (surah: Surah, bounds: typeof Bounds.Type) => Effect.map(spanned(`ayah in surah ${surah.number}`, bounds, surah.ayahs.map(Struct.get('number'))), (range) => surah.ayahs.filter(({ number }) => Number.between(number, range)));
const counted = Effect.fnUntraced(function* (quran: Quran, { part, number, from, to }: typeof Quran.Selection.cases.Part.Type) {
    const data = getListsOfRiwaya(yield* Effect.fromOption(quran.riwayah, () => Unmatched.make({ subject: `riwayah counting ${part}`, candidates: Riwayah.literals })));
    const listed = blocks(part, data);
    const ordinals = listed.map((_, index) => String(index + 1));
    const { startAyahId, ayahCount } = yield* Effect.fromOption(Array.get(listed, number - 1), () => Unmatched.make({ subject: `${part} in ${data.meta.riwayaName}`, query: Option.some(String(number)), candidates: ordinals }));
    const { minimum, maximum } = yield* spanned(`ayah in ${part} ${number}`, { from, to }, Array.range(1, ayahCount));
    return yield* pipe(
        Array.range(startAyahId + minimum - 1, startAyahId + maximum - 1),
        Effect.validate((id: number) => quran.ayah(findSurahAyahByAyahId(id, data))),
        Effect.mapError((missing) => Unmatched.make({ subject: 'ayah text', query: Option.some(Array.getSomes(missing.map(Struct.get('query'))).join(', ')) })),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Quran };
