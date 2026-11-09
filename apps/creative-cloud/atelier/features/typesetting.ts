import { Kerns, type Runs, Typeset } from '@rasm/atelier-protocol/indesign';
import { Route, Server } from '@rasm/mcp';
import { Quran } from '@rasm/quran';
import { Unmatched } from '@rasm/search';
import { Settings } from '@rasm/typography';
import { Face, Kerning, Symbols, Word } from '@rasm/typography/shaping';
import { Array, Cause, Effect, Equal, FileSystem, Number, Option, Order, Predicate, Record, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { AbsolutePath } from '#/absolute-path';
import { Adobe } from '#/adobe';
import { AvailableFace } from '#/available-face';
import { applicationFont } from '#/features/fonts';
import type { routes } from '#/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Client = Parameters<typeof applicationFont>[0];
type Span = (typeof Runs.Result.Type)['runs'][number] & { readonly start: number; readonly end: number };

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODES = {
    kern: { restores: false, shapes: true },
    replace: { restores: true, shapes: true },
    restore: { restores: true, shapes: false },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const QuranSource = Schema.TaggedUnion({
    Csv: {
        ayahs: AbsolutePath.annotate({ description: 'Ayah CSV path' }),
        surahs: AbsolutePath.annotate({ description: 'Surah CSV path' }),
        columns: Quran.Columns,
        riwayah: Quran.fields.riwayah,
    },
    Mushaf: {
        file: Schema.OptionFromOptionalKey(AbsolutePath.annotate({ description: 'Mushaf JSON path of any riwayah, bundled Hafs when absent' })),
        render: Schema.optionalKey(Quran.Render),
    },
}).annotate({ description: 'Caller CSV files, or mushaf JSON from @quran.ws/text' });
const KerningTool = Tool.make('kerning', {
    description: 'Computes pair kerning and baseline shift from glyph outlines across text, story or frame widened to whole words, and writes them in one undo step',
    parameters: Schema.Struct({
        target: Kerns.fields.target,
        rules: Kerning.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Kerning.make({})))),
        mode: Schema.OptionFromOptionalKey(
            Schema.Literals(Record.keys(MODES)).annotate({
                description: 'kern keeps manual kerning. replace restores metrics kerning across the words, then kerns. restore writes metrics kerning alone. Absent, a whole story kerns and text inside one replaces',
            }),
        ),
    }),
    success: Kerns.Result,
});
const TypesetTool = Tool.make('typeset', {
    description: 'Reads settings of text, story, frame or text defaults, or writes given settings in one undo step, turning off stylistic sets rival to one turned on and refusing kashida in a face with no tatweel advance and no jalt',
    parameters: Schema.Struct(Typeset.fields),
    success: Typeset.Result,
});
const QuranTool = Tool.make('quran', {
    description: 'Reads ayahs of surah or division from CSV files or mushaf, its basmalah a line before ayah 1, each ayah closed by a number mark the named CoreText face draws and each line by separator, change inserts them',
    parameters: Schema.Struct({
        source: QuranSource,
        selection: Quran.Selection,
        face: AvailableFace.Name,
        numberingSystem: Settings.fields.numberingSystem,
        separator: Schema.String.annotate({ description: 'Text after each line' }),
    }),
    success: Schema.Struct({ text: Schema.String }),
})
    .annotate(Tool.Readonly, true)
    .annotate(Tool.OpenWorld, false);
const SymbolsTool = Tool.make('symbols', {
    description: 'Lists symbols of TSV file that named CoreText face covers, by set',
    parameters: Schema.Struct({
        file: AbsolutePath.annotate({ description: 'Symbol TSV path, one column of hexadecimal code points per set' }),
        face: AvailableFace.Name,
    }),
    success: Schema.Struct({ sets: Schema.Record(Schema.String, Schema.Array(Schema.String)) }),
})
    .annotate(Tool.Readonly, true)
    .annotate(Tool.OpenWorld, false);

// --- [OPERATIONS] ----------------------------------------------------------------------

const sameFace = (left: Span, right: Span) => left.font === right.font && Equal.equals(left.axes, right.axes);
const spans = ({ runs }: typeof Runs.Result.Type, [first, last]: readonly [number, number]): readonly Span[] => {
    const [, placed] = Array.mapAccum(runs, 0, (at, run) => [at + run.text.length, { ...run, start: at, end: at + run.text.length }] as const);
    return Array.match(placed, { onEmpty: Array.empty<Array.NonEmptyReadonlyArray<Span>>, onNonEmpty: Array.groupWith(sameFace) })
        .map((group): Span => ({ ...Array.headNonEmpty(group), end: Array.lastNonEmpty(group).end, text: group.map(Struct.get('text')).join('') }))
        .filter((span) => (span.start < last && span.end > first) || (span.start === first && span.end === last))
        .map((span): Span => {
            const [from, to] = [Math.max(span.start, first), Math.min(span.end, last)];
            return { ...span, start: from, end: to, text: span.text.slice(from - span.start, to - span.start) };
        });
};
const faced = (client: Client, located: readonly Span[], extra: readonly string[]) =>
    Effect.forEach(
        Array.dedupeWith(located, sameFace),
        Effect.fnUntraced(function* (first: Span) {
            const { font, axes } = first;
            const group = located.filter((span) => sameFace(first, span));
            const { source } = yield* applicationFont(client, font, [...group.map(Struct.get('text')), ...extra]);
            const subset = yield* Effect.fromOption(source, () => Unmatched.make({ subject: 'font file', query: Option.some(font) }));
            const named = yield* Face.open(subset);
            const face = Array.isReadonlyArrayEmpty(axes) ? named : yield* Face.open(subset, Face.Instance.make({ variations: Record.fromEntries(Array.zip(named.axes.map(Struct.get('tag')), axes)) }));
            return { font, face, placed: group.map((span) => ({ span, face })) };
        }),
    );
const kerned = Effect.fnUntraced(function* (client: Client, { target, rules, mode }: Tool.Parameters<typeof KerningTool>) {
    const selection = yield* client('runs', { target });
    const text = selection.runs.map(Struct.get('text')).join('');
    const [first, last] = Word.expand(text, selection.start, selection.start + selection.length);
    const { restores, shapes } = MODES[Option.getOrElse(mode, () => (selection.length === text.length ? 'kern' : 'replace'))];
    const restore = Option.liftPredicate({ index: first, length: last - first }, ({ length }) => restores && length > 0);
    const faces = !shapes || first === last ? [] : yield* faced(client, spans(selection, [first, last]), []);
    const ordered = Array.sort(
        faces.flatMap(Struct.get('placed')),
        Order.mapInput(Number.Order, ({ span }: { readonly span: Span }) => span.start),
    );
    const kerns = yield* Stream.fromIterable(ordered).pipe(
        Stream.map(({ span, face }) => ({ font: span.font, kerns: rules.kerns(face, span.text).map(Struct.evolve({ index: Number.sum(span.start) })) })),
        Server.progress(({ font }, at) => ({ progress: at + 1, total: ordered.length, message: font })),
        Stream.runCollect,
    );
    return yield* client('kerns', { target: selection.specifier, restore, kerns: kerns.flatMap(Struct.get('kerns')) });
});
const typeset = Effect.fnUntraced(function* (client: Client, { target, settings }: Tool.Parameters<typeof TypesetTool>) {
    const elongating = Option.exists(Option.flatMapNullishOr(settings, Struct.get('kashida')), (kashida) => !Predicate.isTagged(kashida, 'None'));
    const tatweel = String.fromCodePoint(Face.TATWEEL);
    const faces = elongating || Option.isSome(Option.flatMapNullishOr(settings, Struct.get('features'))) ? yield* Effect.flatMap(client('runs', { target }), (selection) => faced(client, spans(selection, [selection.start, selection.start + selection.length]), elongating ? [tatweel] : [])) : [];
    const broken = elongating ? faces.filter(({ face }) => Option.isNone(face.tatweel) && !face.layout.substitutions.has('jalt')) : [];
    yield* Array.isReadonlyArrayEmpty(broken) ? Effect.void : new Cause.IllegalArgumentError(`${broken.map(Struct.get('font')).join(', ')} draw no ${tatweel} with positive advance and hold no jalt, so kashida breaks their joins`);
    return yield* client('typeset', { target, settings: Option.map(settings, Struct.evolve({ features: (features = {}) => faces.reduce((exclusive, { face }) => face.layout.exclusive(exclusive), features) })) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [adobe, files] = yield* Effect.all([Adobe, FileSystem.FileSystem]);
    const faceOf = (name: typeof AvailableFace.Name.Type) => adobe.face({ name }).pipe(Effect.flatMap(Struct.get('source')), Effect.flatMap(Face.open));
    const read = QuranSource.match({
        Csv: ({ ayahs, surahs, columns, riwayah }) => Effect.flatMap(Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), (csv) => Quran.decode(csv, columns, riwayah)),
        Mushaf: ({ file, render }) => Effect.flatMap(Effect.transposeOption(Option.map(file, (path) => files.readFileString(path))), (json) => Quran.mushaf(json, render)),
    });
    yield* Route(serving('kerns'))(KerningTool, (parameters, { client }) => kerned(client, parameters));
    yield* Route(serving('typeset'))(TypesetTool, (parameters, { client }) => typeset(client, parameters));
    yield* Server.registerTool(
        QuranTool,
        Effect.fnUntraced(function* ({ source, selection, face: name, numberingSystem, separator }) {
            const [quran, face] = yield* Effect.all([read(source), faceOf(name)]);
            return { text: yield* quran.text(selection, separator, face, Settings.make({ numberingSystem })) };
        }),
    );
    yield* Server.registerTool(
        SymbolsTool,
        Effect.fnUntraced(function* ({ file, face: name }) {
            const [symbols, face] = yield* Effect.all([Effect.flatMap(files.readFileString(file), Symbols.decode), faceOf(name)]);
            return { sets: symbols.supported(face).sets };
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
