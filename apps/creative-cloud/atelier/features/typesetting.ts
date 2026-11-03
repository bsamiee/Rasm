import { Automation } from '@rasm/automation/automation';
import { AvailableFace } from '@rasm/automation/available-face';
import { Kerns, type Runs } from '@rasm/automation/typesetting';
import { progress } from '@rasm/mcp/progress';
import { Route } from '@rasm/mcp/route';
import { registerTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Face } from '@rasm/typography/face';
import { Kerning } from '@rasm/typography/kerning';
import { Quran } from '@rasm/typography/quran';
import { Symbols } from '@rasm/typography/symbols';
import { Word } from '@rasm/typography/word';
import { Array, Effect, Equal, FileSystem, Number, Option, Order, Record, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { AbsolutePath } from '#/absolute-path';
import { applicationFont } from '#/features/fonts';
import type { routes } from '#/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Client = Parameters<typeof applicationFont>[0];
type Span = (typeof Runs.Result.Type)['runs'][number] & { readonly start: number; readonly end: number };

// --- [OPERATIONS] ----------------------------------------------------------------------

const sameFace = (left: Span, right: Span) => left.font === right.font && Equal.equals(left.axes, right.axes);
const spans = ({ start, length, runs }: typeof Runs.Result.Type): readonly Span[] => {
    const [, placed] = Array.mapAccum(runs, 0, (at, run) => [at + run.text.length, { ...run, start: at, end: at + run.text.length }] as const);
    const [first, last] = Word.expand(runs.map(Struct.get('text')).join(''), start, start + length);
    return Array.match(placed, { onEmpty: Array.empty<Array.NonEmptyReadonlyArray<Span>>, onNonEmpty: Array.groupWith(sameFace) })
        .map((group): Span => ({ ...Array.headNonEmpty(group), end: Array.lastNonEmpty(group).end, text: group.map(Struct.get('text')).join('') }))
        .filter((span) => span.start < last && span.end > first)
        .map((span): Span => {
            const [from, to] = [Math.max(span.start, first), Math.min(span.end, last)];
            return { ...span, start: from, end: to, text: span.text.slice(from - span.start, to - span.start) };
        });
};
const opened = Effect.fnUntraced(function* (client: Client, { font, axes }: Span, group: readonly Span[]) {
    const { source } = yield* applicationFont(client, font, group.map(Struct.get('text')).join(''));
    const subset = yield* Effect.fromOption(source, () => Unmatched.make({ subject: 'font file', query: Option.some(font) }));
    const named = yield* Face.open(subset);
    const face = Array.isReadonlyArrayEmpty(axes) ? named : yield* Face.open(subset, Face.Instance.make({ variations: Record.fromEntries(Array.zip(named.axes.map(Struct.get('tag')), axes)) }));
    return group.map((span) => ({ span, face }));
});
const kerned = Effect.fnUntraced(function* (client: Client, target: string, kerning: Kerning) {
    const found = yield* client('runs', { target });
    const located = spans(found);
    const faces = yield* Effect.forEach(Array.dedupeWith(located, sameFace), (face) =>
        opened(
            client,
            face,
            located.filter((span) => sameFace(face, span)),
        ),
    );
    const ordered = Array.sort(
        faces.flat(),
        Order.mapInput(Number.Order, ({ span }: (typeof faces)[number][number]) => span.start),
    );
    const kerns = yield* Stream.fromIterable(ordered).pipe(
        Stream.map(({ span, face }) => ({ font: span.font, kerns: kerning.kerns(face, span.text).map(({ index, kerning: value }) => ({ index: span.start + index, kerning: value })) })),
        progress(({ font }, at) => ({ progress: at + 1, total: ordered.length, message: font })),
        Stream.runCollect,
    );
    return yield* client('kerns', { target: found.specifier, kerns: kerns.flatMap(Struct.get('kerns')) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, files] = yield* Effect.all([Automation, FileSystem.FileSystem]);
    yield* Route.register(serving('kerns'))(
        Tool.make('kerning', {
            description: 'Computes pair kerning from glyph outlines across text, story or frame and writes it in one undo step, widening text to whole words and keeping manual kerning',
            parameters: Schema.Struct({
                target: Kerns.fields.target,
                rules: Kerning.pipe(Schema.withDecodingDefaultKey(Schema.encodeEffect(Kerning)(Kerning.make({})))),
            }),
            success: Kerns.Result,
        }),
        ({ target, rules }, { client }) => kerned(client, target, rules),
    );
    yield* registerTool(
        Tool.make('quran', {
            description: 'Reads surah text from CSV files, change inserts it',
            parameters: Schema.Struct({
                ayahs: AbsolutePath.annotate({ description: 'Ayah CSV path' }),
                surahs: AbsolutePath.annotate({ description: 'Surah CSV path' }),
                columns: Quran.Columns,
                surah: Schema.String.annotate({ description: 'Surah name or number' }),
                ayah: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Ayah number, every ayah when absent' })),
                separator: Schema.String.annotate({ description: 'Text after each ayah' }),
            }),
            success: Schema.Struct({ text: Schema.String }),
        })
            .annotate(Tool.Readonly, true)
            .annotate(Tool.OpenWorld, false),
        Effect.fnUntraced(function* ({ ayahs, surahs, columns, surah, ayah, separator }) {
            const quran = yield* Quran.decode(yield* Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), columns);
            return { text: yield* (yield* quran.surah(surah)).text(ayah, separator) };
        }),
    );
    yield* registerTool(
        Tool.make('symbols', {
            description: 'Lists symbols of TSV file that named CoreText face covers, by set',
            parameters: Schema.Struct({
                file: AbsolutePath.annotate({ description: 'Symbol TSV path, one column of hexadecimal code points per set' }),
                face: AvailableFace.Name,
            }),
            success: Schema.Struct({ sets: Schema.Record(Schema.String, Schema.Array(Schema.String)) }),
        })
            .annotate(Tool.Readonly, true)
            .annotate(Tool.OpenWorld, false),
        Effect.fnUntraced(function* ({ file, face: name }) {
            const [symbols, found] = yield* Effect.all([Effect.flatMap(files.readFileString(file), Symbols.decode), automation.face({ name })]);
            const face = yield* Effect.flatMap(AvailableFace.source(found), Face.open);
            return { sets: symbols.supported(face).sets };
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
