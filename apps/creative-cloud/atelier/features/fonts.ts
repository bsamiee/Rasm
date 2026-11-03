import { Automation } from '@rasm/automation/automation';
import { AvailableFace } from '@rasm/automation/contract';
import { Contract } from '@rasm/indesign/host';
import { registerResource } from '@rasm/mcp/resource';
import { registerRoutedTool } from '@rasm/mcp/tool';
import { Face } from '@rasm/typography/face';
import { Justification } from '@rasm/typography/justification';
import { Settings } from '@rasm/typography/settings';
import { ShapedText } from '@rasm/typography/shaped-text';
import type { Source } from '@rasm/typography/source';
import { Array, Effect, FileSystem, Option, Path, Schema, SchemaAST, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { Rpc, RpcGroup } from 'effect/rpc';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const spacings = Struct.keys(Settings.fields.spacing.schema.fields);
const specimen = Schema.Struct({
    face: Face,
    shaped: ShapedText.annotate({ description: 'Em positions, not application composition or document coordinates' }),
    justified: Schema.OptionFromOptionalKey(
        Schema.Struct({
            text: Schema.String.annotate({ description: 'Shaped text with inserted tatweels, glyph clusters index it' }),
            slack: Schema.Finite.annotate({ description: 'Width left in em, negative when overfull' }),
            ratios: Schema.Struct(Struct.Record(spacings, Schema.Finite)).annotate({ description: 'Applied spacing ratios' }),
            amounts: Schema.Struct(Struct.Record([...spacings, 'kashida'], Schema.Finite)).annotate({ description: 'Width each mechanism added in em' }),
        }).annotate({ description: 'Absent without width' }),
    ),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const composed = Effect.fnUntraced(function* (source: typeof Source.Type, instance: typeof Face.Instance.Type, run: typeof ShapedText.Run.Type, width: Option.Option<number>, settings: Settings) {
    const face = yield* Face.open(source, instance);
    const justified = Option.map(width, (line) => Justification.justify(face, run, line, settings));
    return {
        face,
        shaped: Option.match(justified, { onNone: () => ShapedText.of(face, run), onSome: Struct.get('shaped') }).offsetMarks(settings.marks),
        justified: Option.map(justified, ({ run: { text }, slack, ratios, amounts }) => ({ text, slack, ratios, amounts })),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const faces = Automation.use((automation) => registerResource({ path: 'faces', name: 'faces', description: 'CoreText faces with file holding each', mimeType: 'application/json', schema: Schema.Array(AvailableFace), changes: Stream.empty, content: automation.faces() }));
const font = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, path, files] = yield* Effect.all([Automation, Path.Path, FileSystem.FileSystem]);
    const route = serving('font');
    const { successSchema } = yield* Effect.fromOption(Array.findFirst([...Contract.requests.values()], (rpc): rpc is Rpc.ExtractTag<RpcGroup.Rpcs<typeof Contract>, 'font'> => rpc._tag === 'font'));
    const tool = Tool.make('font', {
        description: 'Shapes text in named face or instance, with application returns its font view',
        parameters: Face.Instance.pipe(
            Schema.fieldsAssign({
                ...ShapedText.Run.fields,
                text: ShapedText.Run.fields.text.annotate({ description: 'One-script one-direction run to shape and check glyphs, no geometry when absent' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
                name: Schema.String.annotate({ description: `${SchemaAST.resolveDescription(AvailableFace.Name.ast)}, or application font name or specifier` }),
                width: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Line width in em to justify text to, natural width when absent' })),
                settings: Settings.mapFields(Struct.map(Schema.optionalKey)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }),
        ),
        success: Schema.Union([Schema.Struct({ ...AvailableFace.fields, specimen }), Schema.Struct({ ...successSchema.fields.view.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
    }).annotate(Tool.Readonly, true);
    const typeset = Effect.fnUntraced(function* ({ name, variations, size, width, settings: given, ...run }: Tool.Parameters<typeof tool>, chosen: Option.Option<Effect.Success<ReturnType<typeof route.resolve>>>) {
        const settings = Settings.make(given);
        const read = ({ file, postscriptName }: { readonly file: string; readonly postscriptName: Option.Option<string> }) => Effect.flatMap(files.readFile(file), (data) => composed({ data, postscriptName }, { variations, size }, run, width, settings));
        const native = Effect.fnUntraced(function* ({ client }: Effect.Success<ReturnType<typeof route.resolve>>) {
            const { view, source } = yield* client('font', { name, text: run.text, file: path.join(yield* files.makeTempDirectoryScoped(), 'subset.otf') });
            return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, read)) };
        }, Effect.scoped);
        const shaping = Effect.gen(function* () {
            const found = yield* automation.face({ name });
            return { ...found, specimen: yield* read({ file: yield* path.fromFileUrl(found.file), postscriptName: Option.some(found.postscriptName) }) };
        });
        return yield* Option.match(chosen, { onNone: () => shaping, onSome: native });
    });
    yield* registerRoutedTool(route.optional('CoreText'))(tool, typeset);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { faces, font };
