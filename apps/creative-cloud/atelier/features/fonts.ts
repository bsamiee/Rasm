import { Inspect } from '@rasm/atelier-protocol/indesign';
import { Resource, Route } from '@rasm/mcp';
import { Settings, type Source } from '@rasm/typography';
import { Face, Justification, ShapedText } from '@rasm/typography/shaping';
import { Effect, FileSystem, Option, Schema, SchemaAST, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Adobe } from '#/adobe';
import { AvailableFace } from '#/available-face';
import type { listening } from '#/libraries';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Specimen = Schema.Struct({
    face: Face,
    shaped: Schema.OptionFromOptionalKey(Schema.Union([ShapedText, Justification]).annotate({ description: 'Em positions, not application composition or document coordinates, justified with width, absent with no text' })),
    choices: Schema.Array(Face.fields.layout.Choice).annotate({ description: 'aalt, salt and cvNN alternates of each shaped glyph' }),
});
const FontRequest = Face.Instance.pipe(
    Schema.fieldsAssign({
        ...ShapedText.Run.fields,
        text: Schema.OptionFromOptionalKey(ShapedText.Run.fields.text.annotate({ description: 'One-script one-direction run to shape and check glyphs, no geometry when absent' })),
        name: AvailableFace.Name.annotate({ description: `${SchemaAST.resolveDescription(AvailableFace.Name.ast)}, or application font name or specifier` }),
        width: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Line width in em to justify text to, natural width when absent' })),
        settings: Settings.Partial.pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
);
const FontResult = Schema.Union([AvailableFace.mapFields(Struct.assign({ specimen: Specimen })), Inspect.Font.mapFields(Struct.assign({ specimen: Schema.OptionFromOptionalKey(Specimen) }))]);
const Faces = Schema.Array(AvailableFace);

// --- [OPERATIONS] ----------------------------------------------------------------------

const applicationFont = Effect.fnUntraced(function* (client: Effect.Success<ReturnType<Effect.Success<typeof listening>['call']>>, name: string, characters: readonly string[]) {
    const files = yield* FileSystem.FileSystem;
    const { font, source } = yield* client('font', { name, text: characters.join(''), file: yield* files.makeTempFileScoped({ suffix: '.otf' }) });
    const read = Option.map(source, ({ file, postscriptName }) => Effect.all({ data: files.readFile(file), postscriptName: Effect.succeed(postscriptName) }));
    return { font, source: yield* Effect.transposeOption(read) };
}, Effect.scoped);
const composed = Effect.fnUntraced(function* (source: typeof Source.Type, instance: typeof Face.Instance.Type, run: Option.Option<typeof ShapedText.Run.Type>, width: Option.Option<number>, given: typeof Settings.Partial.Type) {
    const face = yield* Face.open(source, instance);
    const settings = yield* Settings.merge(Settings.make({}), Struct.evolve(given, { features: (features = {}) => face.layout.exclusive(features) }));
    const specimen = Option.map(run, (text) => {
        const natural = ShapedText.of(face, text, settings);
        return { shaped: Option.match(width, { onNone: () => natural, onSome: (line) => Justification.of([{ face, run: text, settings }], line) }), choices: face.layout.choices(natural) };
    });
    return { face, shaped: Option.map(specimen, Struct.get('shaped')), choices: Option.match(specimen, { onNone: () => [], onSome: Struct.get('choices') }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const faces = Effect.flatMap(Adobe, (adobe) => Resource.register({ name: 'faces', description: 'CoreText faces with file holding each', schema: Faces, changes: Stream.empty, content: adobe.faces() }));
const font = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [adobe, route] = [yield* Adobe, serving('font')];
    const tool = Tool.make(route.tag, {
        description: 'Shapes text in CoreText face or instance by name, or returns application font with its specimen when application is given',
        parameters: FontRequest,
        success: FontResult,
    }).annotate(Tool.Readonly, true);
    const typeset = ({ name, variations, size, width, settings, text, ...run }: Tool.Parameters<typeof tool>, chosen: Option.Option<Effect.Success<ReturnType<typeof route.resolve>>>) => {
        const shaped = (source: typeof Source.Type) =>
            composed(
                source,
                { variations, size },
                Option.map(text, (given) => ({ ...run, text: given })),
                width,
                settings,
            );
        const native = Effect.fnUntraced(function* ({ client }: Effect.Success<ReturnType<typeof route.resolve>>) {
            const applied = yield* applicationFont(client, name, Option.toArray(text));
            return { ...applied.font, specimen: yield* Effect.transposeOption(Option.map(applied.source, shaped)) };
        });
        const shaping = Effect.gen(function* () {
            const available = yield* adobe.face({ name });
            return { ...available, specimen: yield* Effect.flatMap(available.source, shaped) };
        });
        return Option.match(chosen, { onNone: () => shaping, onSome: native });
    };
    yield* Route(route.optional('CoreText'))(tool, typeset);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applicationFont, faces, font };
