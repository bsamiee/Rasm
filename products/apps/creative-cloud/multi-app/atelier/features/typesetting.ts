import { indesign } from '@rasm/atelier-indesign-plugin/tools';
import { Route, Server } from '@rasm/mcp';
import { Quran } from '@rasm/quran';
import { Settings } from '@rasm/typography';
import { Face, Symbols } from '@rasm/typography/shaping';
import { Effect, FileSystem, Option, Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { AbsolutePath } from '#/absolute-path';
import { applicationFont, AvailableFace, Fonts } from '#/features/fonts';
import type { routes } from '#/routes';

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

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [helper, files] = yield* Effect.all([Fonts, FileSystem.FileSystem]);
    const faceOf = (name: typeof AvailableFace.Name.Type) => helper.face({ name }).pipe(Effect.flatMap(Struct.get('source')), Effect.flatMap(Face.open));
    const read = QuranSource.match({
        Csv: ({ ayahs, surahs, columns, riwayah }) => Effect.flatMap(Effect.all({ ayahs: files.readFileString(ayahs), surahs: files.readFileString(surahs) }), (csv) => Quran.decode(csv, columns, riwayah)),
        Mushaf: ({ file, render }) => Effect.flatMap(Effect.transposeOption(Option.map(file, (path) => files.readFileString(path))), (json) => Quran.mushaf(json, render)),
    });
    yield* Route(serving(indesign.kerning.tag))(indesign.kerning.tool, (parameters, { client }) => indesign.kerning.run(client, applicationFont(client))(parameters));
    yield* Route(serving(indesign.typeset.tag))(indesign.typeset.tool, (parameters, { client }) => indesign.typeset.run(client, applicationFont(client))(parameters));
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
