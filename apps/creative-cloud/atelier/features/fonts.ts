import { Automation } from '@rasm/automation/automation';
import { AvailableFace } from '@rasm/automation/available-face';
import { Font } from '@rasm/indesign/contract';
import { Resource } from '@rasm/mcp/resource';
import { Route } from '@rasm/mcp/route';
import { Face } from '@rasm/typography/face';
import { Justification } from '@rasm/typography/justification';
import { Settings } from '@rasm/typography/settings';
import { ShapedText } from '@rasm/typography/shaped-text';
import type { Source } from '@rasm/typography/source';
import { Effect, FileSystem, Option, Schema, SchemaAST, Stream } from 'effect';
import { Tool } from 'effect/ai';
import type { listening } from '#/features/plugins';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Specimen = Schema.Struct({
    face: Face,
    shaped: Schema.Union([ShapedText, Justification]).annotate({ description: 'Em positions, not application composition or document coordinates, justified with width' }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const applicationFont = Effect.fnUntraced(function* (client: Effect.Success<ReturnType<Effect.Success<typeof listening>['call']>>, name: string, text: string) {
    const files = yield* FileSystem.FileSystem;
    const { font, source } = yield* client('font', { name, text, file: yield* files.makeTempFileScoped({ suffix: '.otf' }) });
    const read = Option.map(source, ({ file, postscriptName }) => Effect.map(files.readFile(file), (data): typeof Source.Type => ({ data, postscriptName })));
    return { font, source: yield* Effect.transposeOption(read) };
}, Effect.scoped);
const composed = Effect.fnUntraced(function* (source: typeof Source.Type, instance: typeof Face.Instance.Type, run: typeof ShapedText.Run.Type, width: Option.Option<number>, settings: Settings) {
    const face = yield* Face.open(source, instance);
    return { face, shaped: Option.match(width, { onNone: () => ShapedText.of(face, run, settings), onSome: (line) => Justification.of(face, run, line, settings) }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const faces = Automation.use((automation) => Resource.register({ name: 'faces', description: 'CoreText faces with file holding each', schema: Schema.Array(AvailableFace), changes: Stream.empty, content: automation.faces() }));
const font = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, route] = [yield* Automation, serving('font')];
    const tool = Tool.make(route.tag, {
        description: 'Shapes text in CoreText face or instance by name, or returns application font with its specimen when application is given',
        parameters: Face.Instance.pipe(
            Schema.fieldsAssign({
                ...ShapedText.Run.fields,
                text: ShapedText.Run.fields.text.annotate({ description: 'One-script one-direction run to shape and check glyphs, no geometry when absent' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
                name: AvailableFace.Name.annotate({ description: `${SchemaAST.resolveDescription(AvailableFace.Name.ast)}, or application font name or specifier` }),
                width: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Line width in em to justify text to, natural width when absent' })),
                settings: Settings.Partial.pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }),
        ),
        success: Schema.Union([Schema.Struct({ ...AvailableFace.fields, specimen: Specimen }), Schema.Struct({ ...Font.fields, specimen: Schema.OptionFromOptionalKey(Specimen) })]),
    }).annotate(Tool.Readonly, true);
    const typeset = Effect.fnUntraced(function* ({ name, variations, size, width, settings: given, ...run }: Tool.Parameters<typeof tool>, chosen: Option.Option<Effect.Success<ReturnType<typeof route.resolve>>>) {
        const settings = yield* Settings.merge(Settings.make({}), given);
        const shaped = (source: typeof Source.Type) => composed(source, { variations, size }, run, width, settings);
        const native = Effect.fnUntraced(function* ({ client }: Effect.Success<ReturnType<typeof route.resolve>>) {
            const applied = yield* applicationFont(client, name, run.text);
            return { ...applied.font, specimen: yield* Effect.transposeOption(Option.map(applied.source, shaped)) };
        });
        const shaping = Effect.gen(function* () {
            const found = yield* automation.face({ name });
            return { ...found, specimen: yield* Effect.flatMap(AvailableFace.source(found), shaped) };
        });
        return yield* Option.match(chosen, { onNone: () => shaping, onSome: native });
    });
    yield* Route.register(route.optional('CoreText'))(tool, typeset);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applicationFont, faces, font };
