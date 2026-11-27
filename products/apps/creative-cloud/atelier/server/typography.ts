import { Target } from '@rasm/application-tools/handle/target';
import { Catalog } from '@rasm/applications/catalog';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Kerns, Typeset, TypesettingRpcs } from '@rasm/indesign/typesetting';
import { Inspection } from '@rasm/indesign/views';
import { Link } from '@rasm/mcp/link';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { drawsKashida, TATWEEL } from '@rasm/typography/arabic/kashida';
import { Kerning } from '@rasm/typography/arabic/kerning';
import { Symbols } from '@rasm/typography/arabic/quran/symbols';
import { Quran } from '@rasm/typography/arabic/quran/text';
import { Settings } from '@rasm/typography/settings';
import { Face } from '@rasm/typography/shaping/face';
import { Justification } from '@rasm/typography/shaping/justification';
import { ShapedText } from '@rasm/typography/shaping/shaped-text';
import { expandToWords, Span } from '@rasm/typography/shaping/word';
import type { Source } from '@rasm/typography/source';
import { Array, Cause, Context, Effect, FileSystem, Layer, Number, Option, Predicate, Record, Schema, Stream, Struct } from 'effect';
import { Tool, Toolkit } from 'effect/ai';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODES = { kern: { restores: false, shapes: true }, replace: { restores: true, shapes: true }, restore: { restores: true, shapes: false } } as const;
const FONTS = new Intl.ListFormat('en');

// --- [MODELS] --------------------------------------------------------------------------

const FONT = Schema.String.annotate({ description: 'Application font, PostScript, or full name, family name selects its Regular or first face' });
const PATH = Schema.String.check(Schema.isStartingWith('/'));
const Specimen = Schema.Struct({ face: Face, shaped: Schema.OptionFromOptionalKey(Schema.Union([ShapedText, Justification])), choices: Schema.Array(Face.fields.layout.Choice) });
const QuranSource = Schema.TaggedUnion({
    Csv: { ayahs: PATH.annotate({ description: 'Ayah CSV path' }), surahs: PATH.annotate({ description: 'Surah CSV path' }), columns: Quran.Columns, riwayah: Quran.fields.riwayah },
    Mushaf: { file: Schema.OptionFromOptionalKey(PATH.annotate({ description: 'Mushaf JSON path of any riwayah, bundled Hafs when absent' })), render: Schema.optionalKey(Quran.Render) },
}).annotate({ description: 'Caller CSV files, or mushaf JSON from @quran.ws/text' });
const Tools = Toolkit.make(
    Tool.make('font', {
        description: 'Returns application font with its face, and shapes text in it at em positions, justified to width when given',
        parameters: Face.Instance.pipe(
            Schema.fieldsAssign({
                ...ShapedText.Run.fields,
                name: FONT,
                text: Schema.OptionFromOptionalKey(ShapedText.Run.fields.text.annotate({ description: 'One-script one-direction run to shape and check glyphs, no geometry when absent' })),
                width: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Line width in em to justify text to, natural width when absent' })),
                settings: Settings.Partial.pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }),
        ),
        success: Inspection.View.cases.Font.mapFields(Struct.assign({ specimen: Schema.OptionFromOptionalKey(Specimen) })),
        failure: Schema.ErrorInstance(),
        dependencies: [Handle],
    }).annotate(Tool.Readonly, true),
    Tool.make('kerning', {
        description: 'Computes pair kerning and baseline shift from glyph outlines across text, story or frame widened to whole words, and writes them in one undo step',
        parameters: Schema.Struct({
            target: Kerns.fields.target,
            rules: Kerning.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Kerning.make({})))),
            mode: Schema.OptionFromOptionalKey(Schema.Literals(Record.keys(MODES)).annotate({ description: 'kern keeps manual kerning. replace restores metrics kerning across the words, then kerns. restore writes metrics kerning alone. Absent, a whole story kerns and text inside one replaces' })),
        }),
        success: Report.schema,
        failure: Schema.ErrorInstance(),
        dependencies: [Handle],
    }),
    Tool.make('typeset', {
        description: 'Reads settings of text, story, frame or text defaults, or writes given settings in one undo step, turning off stylistic sets rival to one turned on and refusing kashida in a face with no tatweel advance and no jalt',
        parameters: Schema.Struct(Typeset.fields),
        success: Typeset.Result,
        failure: Schema.ErrorInstance(),
        dependencies: [Handle],
    }),
    Tool.make('quran', {
        description: 'Returns the text of the ayahs of a surah or division from CSV files or a mushaf, its basmalah a line before ayah 1, each ayah closed by a number mark the named application font draws and each line by separator',
        parameters: Schema.Struct({ source: QuranSource, selection: Quran.Selection, face: FONT, numberingSystem: Settings.fields.numberingSystem, separator: Schema.String.annotate({ description: 'Text after each line' }) }),
        success: Report.schema,
        failure: Schema.ErrorInstance(),
        dependencies: [Handle],
    }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false))),
    Tool.make('symbols', {
        description: 'Lists the symbols of a TSV file that the named application font draws, one line per set',
        parameters: Schema.Struct({ file: PATH.annotate({ description: 'Symbol TSV path, one column of hexadecimal code points per set' }), face: FONT }),
        success: Report.schema,
        failure: Schema.ErrorInstance(),
        dependencies: [Handle],
    }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false))),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const specimen = Effect.fnUntraced(function* (data: Source, given: Tool.Parameters<typeof Tools.tools.font>) {
    const face = yield* Face.open(data, Struct.pick(given, Struct.keys(Face.Instance.fields)));
    const settings = yield* Settings.merge(Settings.make({}), Struct.evolve(given.settings, { features: (features = {}) => face.layout.exclusive(features) }));
    const run = Option.map(given.text, (text) => ({ ...Struct.pick(given, Struct.keys(ShapedText.Run.fields)), text }));
    const natural = Option.map(run, (shaping) => ShapedText.of(face, shaping, settings));
    const justified = Option.map(Option.all([run, given.width]), ([shaping, width]) => Justification.of([{ face, run: shaping, settings }], width));
    return { face, shaped: Option.orElse(justified, () => natural), choices: Option.match(natural, { onNone: () => [], onSome: (shaped) => face.layout.choices(shaped) }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typography = Effect.fnUntraced(function* (links: { readonly peers: Effect.Effect<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof TypesettingRpcs>, RpcClientError.RpcClientError>, Unmatched> }) {
    const [files, built] = yield* Effect.all([FileSystem.FileSystem, Effect.map(Effect.context<ApplicationServer | Catalog>(), Context.pick(ApplicationServer, Catalog))]);
    const serving = Link.serves(TypesettingRpcs);
    const client = Target.current.pipe(
        Effect.flatMap(({ row }) => links.call(Predicate.and(Link.runsIn(row), serving), `linked plugin of ${row.name} serving typesetting`)),
        Effect.provide(built),
    );
    const readSource = Effect.fnUntraced(function* (linked: Effect.Success<typeof client>, name: string, characters: string) {
        const file = yield* files.makeTempFileScoped({ suffix: '.otf' });
        const found = yield* linked('fontSource', { name, text: characters, file });
        return yield* Effect.transposeOption(Option.map(found, ({ file: path, postscriptName }) => files.readFile(path).pipe(Effect.bindTo('data'), Effect.map(Struct.assign({ postscriptName })))));
    }, Effect.scoped);
    const openFace = Effect.fnUntraced(function* (linked: Effect.Success<typeof client>, name: string, characters: string, instance?: typeof Face.Instance.Type) {
        const source = yield* readSource(linked, name, characters);
        return yield* Face.open(yield* Effect.fromOption(source, () => Unmatched.of({ subject: 'installed font', query: Option.some(name) })), instance);
    });
    const styleRuns = (linked: Effect.Success<typeof client>, runs: Parameters<typeof Span.styled>[0], first: number, last: number, extra: string) =>
        Effect.forEach(Record.values(Array.groupBy(Span.styled(runs, first, last), ({ font, axes }) => `${font}\t${axes.join(' ')}`)), (group) => {
            const { font, axes } = Array.headNonEmpty(group);
            return Effect.map(openFace(linked, font, [...group.map(Struct.get('text')), extra].join(''), Face.Instance.make({ coordinates: axes })), (face) => ({ font, face, group }));
        });
    const handlers = Tools.toLayer({
        font: Effect.fnUntraced(function* (given) {
            const linked = yield* client;
            const [font, source] = yield* Effect.all([
                linked('font', { name: given.name }),
                readSource(
                    linked,
                    given.name,
                    Option.getOrElse(given.text, () => ''),
                ),
            ]);
            return { ...font, specimen: yield* Effect.transposeOption(Option.map(source, (data) => specimen(data, given))) };
        }, Effect.scoped),
        kerning: Effect.fnUntraced(function* ({ target, rules, mode }) {
            const linked = yield* client;
            const selection = yield* linked('runs', { target });
            const text = selection.runs.map(Struct.get('text')).join('');
            const [first, last] = expandToWords(text, selection.start, selection.start + selection.length);
            const { restores, shapes } = MODES[Option.getOrElse(mode, () => (selection.length === text.length ? 'kern' : 'replace'))];
            const restore = Option.liftPredicate({ index: first, length: last - first }, ({ length }) => restores && length > 0);
            const faces = shapes && first < last ? yield* styleRuns(linked, selection.runs, first, last, '') : [];
            const spans = Array.sortWith(
                faces.flatMap(({ face, group }) => Array.cartesian([face], group)),
                ([, span]) => span.start,
                Number.Order,
            );
            const rows = yield* Stream.fromIterable(spans).pipe(
                Stream.map(([face, span]) => Array.map(rules.kerns(face, span.text), Struct.evolve({ index: Number.sum(span.start) }))),
                Server.progress((_, at) => ({ progress: at + 1, total: spans.length })),
                Stream.runCollect,
            );
            const kerns = rows.flat();
            const { specifier, written } = yield* linked('kerns', { target: selection.specifier, restore, kerns });
            return new Report({
                lines: [
                    new Line({ head: [specifier] }),
                    ...Option.toArray(restore).map(({ length }) => new Line({ head: ['restored'], value: `metrics kerning across ${Format.count(length, { one: 'character', other: 'characters' })}` })),
                    new Line({ head: ['written'], value: `${Format.count(written)} of ${Format.count(kerns.length, { one: 'kerning row', other: 'kerning rows' })}` }),
                ],
            });
        }, Effect.scoped),
        typeset: Effect.fnUntraced(function* ({ target, settings }) {
            const linked = yield* client;
            const elongating = Option.exists(Option.flatMapNullishOr(settings, Struct.get('kashida')), Predicate.not(Predicate.isTagged('None')));
            const featured = Option.isSome(Option.flatMapNullishOr(settings, Struct.get('features')));
            const faces = elongating || featured ? yield* Effect.flatMap(linked('runs', { target }), ({ runs, start, length }) => styleRuns(linked, runs, start, start + length, elongating ? String.fromCodePoint(TATWEEL) : '')) : [];
            const broken = elongating ? faces.filter(({ face }) => !drawsKashida(face)) : [];
            yield* Array.match(broken, {
                onEmpty: () => Effect.void,
                onNonEmpty: (fonts) => new Cause.IllegalArgumentError(`Kashida breaks the joins of ${FONTS.format(fonts.map(Struct.get('font')))}, as they draw no tatweel with a positive advance and hold no jalt, so set kashida to None or apply a face that has either`),
            });
            return yield* linked('typeset', { target, settings: Option.map(settings, Struct.evolve({ features: (features = {}) => faces.reduce((exclusive, { face }) => face.layout.exclusive(exclusive), features) })) });
        }, Effect.scoped),
        quran: Effect.fnUntraced(function* ({ source, selection, face: name, numberingSystem, separator }) {
            const linked = yield* client;
            const quran = yield* QuranSource.match(source, {
                Csv: ({ ayahs, surahs, columns, riwayah }) => Effect.flatMap(Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), (csv) => Quran.decode(csv, columns, riwayah)),
                Mushaf: ({ file, render }) => Effect.flatMap(Effect.transposeOption(Option.map(file, files.readFileString)), (json) => Quran.mushaf(json, render)),
            });
            const text = yield* Quran.text(quran, separator, (marks) => openFace(linked, name, marks), Settings.make({ numberingSystem }))(selection);
            return new Report({ lines: Report.of(text).lines, budget: globalThis.Number.POSITIVE_INFINITY });
        }, Effect.scoped),
        symbols: Effect.fnUntraced(function* ({ file, face: name }) {
            const [linked, symbols] = yield* Effect.all([client, Effect.flatMap(files.readFileString(file), Symbols.decode)]);
            const face = yield* openFace(linked, name, Record.values(symbols.sets).flat().join(''));
            return new Report({ lines: Record.collect(symbols.supported(face).sets, (set, characters) => new Line({ head: [set], value: Array.match(characters, { onEmpty: () => 'none drawn', onNonEmpty: Array.join(' ') }) })) });
        }, Effect.scoped),
    });
    return Layer.provide(Server.toolkit(Tools, { annotations: Context.make(Server.Visibility, Effect.map(links.peers, Array.some(serving))) }), handlers);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typography };
