import { Inspection, Kerns, Typeset, Typesetting } from '@rasm/indesign';
import { Link, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Settings, type Source } from '@rasm/typography';
import { drawsKashida, Kerning, Quran, Symbols, TATWEEL } from '@rasm/typography/arabic';
import { expandToWords, Face, Justification, ShapedText, Span } from '@rasm/typography/shaping';
import { Array, Cause, Context, Effect, Equivalence, FileSystem, Number, Option, Order, Predicate, Record, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SAME_FACE = Equivalence.Struct({ font: Equivalence.String, axes: Array.makeEquivalence(Equivalence.Number) });
const MODES = { kern: { restores: false, shapes: true }, replace: { restores: true, shapes: true }, restore: { restores: true, shapes: false } } as const;
const FONT = Schema.String.annotate({ description: 'Application font, PostScript, or full name, family name selects its Regular or first face' });
const PATH = Schema.String.check(Schema.isStartingWith('/'));

// --- [MODELS] --------------------------------------------------------------------------

const Specimen = Schema.Struct({
    face: Face,
    shaped: Schema.OptionFromOptionalKey(Schema.Union([ShapedText, Justification]).annotate({ description: 'Em positions, not application composition or document coordinates, justified with width, absent with no text' })),
    choices: Schema.Array(Face.fields.layout.Choice).annotate({ description: 'aalt, salt and cvNN alternates of each shaped glyph' }),
});
const QuranSource = Schema.TaggedUnion({
    Csv: { ayahs: PATH.annotate({ description: 'Ayah CSV path' }), surahs: PATH.annotate({ description: 'Surah CSV path' }), columns: Quran.Columns, riwayah: Quran.fields.riwayah },
    Mushaf: { file: Schema.OptionFromOptionalKey(PATH.annotate({ description: 'Mushaf JSON path of any riwayah, bundled Hafs when absent' })), render: Schema.optionalKey(Quran.Render) },
}).annotate({ description: 'Caller CSV files, or mushaf JSON from @quran.ws/text' });
const Tools = {
    font: Tool.make('font', {
        description: 'Returns application font with its face, and shapes text in it, justified to width when given',
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
    }).annotate(Tool.Readonly, true),
    kerning: Tool.make('kerning', {
        description: 'Computes pair kerning and baseline shift from glyph outlines across text, story or frame widened to whole words, and writes them in one undo step',
        parameters: Schema.Struct({
            target: Kerns.fields.target,
            rules: Kerning.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Kerning.make({})))),
            mode: Schema.OptionFromOptionalKey(Schema.Literals(Record.keys(MODES)).annotate({ description: 'kern keeps manual kerning. replace restores metrics kerning across the words, then kerns. restore writes metrics kerning alone. Absent, a whole story kerns and text inside one replaces' })),
        }),
        success: Kerns.Result,
    }),
    typeset: Tool.make('typeset', {
        description: 'Reads settings of text, story, frame or text defaults, or writes given settings in one undo step, turning off stylistic sets rival to one turned on and refusing kashida in a face with no tatweel advance and no jalt',
        parameters: Schema.Struct(Typeset.fields),
        success: Typeset.Result,
    }),
    quran: Tool.make('quran', {
        description: 'Reads ayahs of surah or division from CSV files or mushaf, its basmalah a line before ayah 1, each ayah closed by a number mark the named application font draws and each line by separator',
        parameters: Schema.Struct({ source: QuranSource, selection: Quran.Selection, face: FONT, numberingSystem: Settings.fields.numberingSystem, separator: Schema.String.annotate({ description: 'Text after each line' }) }),
        success: Schema.Struct({ text: Schema.String }),
    }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false))),
    symbols: Tool.make('symbols', {
        description: 'Lists symbols of TSV file that named application font covers, by set',
        parameters: Schema.Struct({ file: PATH.annotate({ description: 'Symbol TSV path, one column of hexadecimal code points per set' }), face: FONT }),
        success: Schema.Struct({ sets: Schema.Record(Schema.String, Schema.Array(Schema.String)) }),
    }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false))),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const grouped = (runs: readonly { readonly text: string; readonly font: string; readonly axes: readonly number[] }[], first: number, last: number) => {
    const placed = Span.styled(runs, first, last);
    return Array.map(Array.dedupeWith(placed, SAME_FACE), (span) => ({ span, group: placed.filter((each) => SAME_FACE(span, each)) }));
};
const placements = (faces: readonly { readonly face: Face; readonly group: readonly { readonly text: string; readonly start: number }[] }[]) =>
    Array.sort(
        Array.flatMap(faces, ({ face, group }) => Array.map(group, (span) => ({ span, face }))),
        Order.mapInput(Number.Order, ({ span }: { readonly span: { readonly start: number } }) => span.start),
    );
const specimen = Effect.fnUntraced(function* (data: Source, { coordinates, variations, size, width, settings: given, text, ...run }: Tool.Parameters<typeof Tools.font>) {
    const face = yield* Face.open(data, { coordinates, variations, size });
    const settings = yield* Settings.merge(Settings.make({}), Struct.evolve(given, { features: (features = {}) => face.layout.exclusive(features) }));
    const natural = Option.map(text, (body) => ShapedText.of(face, { ...run, text: body }, settings));
    const justified = Option.map(Option.all([text, width]), ([body, line]) => Justification.of([{ face, run: { ...run, text: body }, settings }], line));
    return { face, shaped: Option.orElse(justified, () => natural), choices: Option.match(natural, { onNone: () => [], onSome: (shaped) => face.layout.choices(shaped) }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typography = Effect.fnUntraced(function* (links: { readonly peers: Effect.Effect<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof Typesetting>, RpcClientError.RpcClientError>, Unmatched> }) {
    const files = yield* FileSystem.FileSystem;
    const serving = Link.serves(Typesetting.requests.keys());
    const client = links.call(serving, 'linked application serving typesetting');
    const visible = Context.make(Server.Visible, Effect.map(links.peers, Array.some(serving)));
    const read = ({ file, postscriptName }: { readonly file: string; readonly postscriptName: Option.Option<string> }) => Effect.map(files.readFile(file), (data) => ({ data, postscriptName }));
    const sourced = Effect.fnUntraced(function* (name: string, characters: string) {
        const file = yield* files.makeTempFileScoped({ suffix: '.otf' });
        const { font, source } = yield* Effect.flatMap(client, (linked) => linked('font', { name, text: characters, file }));
        return { font, source: yield* Effect.transposeOption(Option.map(source, read)) };
    }, Effect.scoped);
    const faced = Effect.fnUntraced(function* (name: string, characters: string, instance?: Parameters<typeof Face.open>[1]) {
        const { source } = yield* sourced(name, characters);
        return yield* Face.open(yield* Effect.fromOption(source, () => Unmatched.of({ subject: 'font file', query: Option.some(name) })), instance);
    });
    const styled = (runs: Parameters<typeof grouped>[0], first: number, last: number, extra: string) =>
        Effect.forEach(
            grouped(runs, first, last),
            Effect.fnUntraced(function* ({ span, group }) {
                const face = yield* faced(span.font, [...group.map(Struct.get('text')), extra].join(''), Face.Instance.make({ coordinates: span.axes }));
                return { font: span.font, face, group };
            }),
        );
    yield* Server.registerTool(
        Tools.font.annotateMerge(visible),
        Effect.fnUntraced(function* (given) {
            const { font, source } = yield* sourced(
                given.name,
                Option.getOrElse(given.text, () => ''),
            );
            return { ...font, specimen: yield* Effect.transposeOption(Option.map(source, (data) => specimen(data, given))) };
        }),
    );
    yield* Server.registerTool(
        Tools.kerning.annotateMerge(visible),
        Effect.fnUntraced(function* ({ target, rules, mode }) {
            const linked = yield* client;
            const selection = yield* linked('runs', { target });
            const text = selection.runs.map(Struct.get('text')).join('');
            const [first, last] = expandToWords(text, selection.start, selection.start + selection.length);
            const { restores, shapes } = MODES[Option.getOrElse(mode, () => (selection.length === text.length ? 'kern' : 'replace'))];
            const restore = Option.liftPredicate({ index: first, length: last - first }, ({ length }) => restores && length > 0);
            const ordered = placements(!shapes || first === last ? [] : yield* styled(selection.runs, first, last, ''));
            const rows = yield* Stream.fromIterable(ordered).pipe(
                Stream.map(({ span, face }) => Array.map(rules.kerns(face, span.text), Struct.evolve({ index: Number.sum(span.start) }))),
                Server.progress((_, at) => ({ progress: at + 1, total: ordered.length })),
                Stream.runCollect,
            );
            return yield* linked('kerns', { target: selection.specifier, restore, kerns: rows.flat() });
        }),
    );
    yield* Server.registerTool(
        Tools.typeset.annotateMerge(visible),
        Effect.fnUntraced(function* ({ target, settings }) {
            const linked = yield* client;
            const elongating = Option.exists(Option.flatMapNullishOr(settings, Struct.get('kashida')), Predicate.not(Predicate.isTagged('None')));
            const tatweel = String.fromCodePoint(TATWEEL);
            const selection = yield* linked('runs', { target });
            const faces = elongating || Option.isSome(Option.flatMapNullishOr(settings, Struct.get('features'))) ? yield* styled(selection.runs, selection.start, selection.start + selection.length, elongating ? tatweel : '') : [];
            const broken = elongating ? faces.filter(({ face }) => !drawsKashida(face)) : [];
            yield* Array.isReadonlyArrayEmpty(broken) ? Effect.void : new Cause.IllegalArgumentError(`${broken.map(Struct.get('font')).join(', ')} draw no ${tatweel} with positive advance and hold no jalt, so kashida breaks their joins`);
            return yield* linked('typeset', { target, settings: Option.map(settings, Struct.evolve({ features: (features = {}) => faces.reduce((exclusive, { face }) => face.layout.exclusive(exclusive), features) })) });
        }),
    );
    yield* Server.registerTool(
        Tools.quran.annotateMerge(visible),
        Effect.fnUntraced(function* ({ source, selection, face: name, numberingSystem, separator }) {
            const text = QuranSource.match(source, {
                Csv: ({ ayahs, surahs, columns, riwayah }) => Effect.flatMap(Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), (csv) => Quran.decode(csv, columns, riwayah)),
                Mushaf: ({ file, render }) => Effect.flatMap(Effect.transposeOption(Option.map(file, files.readFileString)), (json) => Quran.mushaf(json, render)),
            });
            const [quran, face] = yield* Effect.all([text, faced(name, '')]);
            return { text: yield* Quran.text(quran, separator, face, Settings.make({ numberingSystem }))(selection) };
        }),
    );
    yield* Server.registerTool(
        Tools.symbols.annotateMerge(visible),
        Effect.fnUntraced(function* ({ file, face: name }) {
            const symbols = yield* Effect.flatMap(files.readFileString(file), Symbols.decode);
            const face = yield* faced(name, Object.values(symbols.sets).flat().join(''));
            return { sets: symbols.supported(face).sets };
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typography };
