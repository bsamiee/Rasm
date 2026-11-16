import { Inspection } from '@rasm/indesign';
import { Resource, Route } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Settings, type Source } from '@rasm/typography';
import { Face, Justification, ShapedText } from '@rasm/typography/shaping';
import { Context, Effect, FileSystem, Layer, Option, Path, Schema, SchemaAST, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Rpc, RpcClient, RpcGroup } from 'effect/rpc';
import type { listening } from '#/libraries';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

class AvailableFace extends Schema.Class<AvailableFace, { readonly brand: unique symbol }>('AvailableFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.URL }) {
    static readonly Name = Schema.String.annotate({ description: 'PostScript name, full name, or family for default face, any case' });

    get source() {
        const { file, postscriptName } = this;
        return Effect.gen(function* () {
            const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            return { data: yield* files.readFile(yield* path.fromFileUrl(file)), postscriptName: Option.some(postscriptName) } satisfies typeof Source.Type;
        });
    }
}
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
const Faces = Schema.Array(AvailableFace);
const FontResult = Schema.Union([AvailableFace.mapFields(Struct.assign({ specimen: Specimen })), Inspection.View.cases.Font.mapFields(Struct.assign({ specimen: Schema.OptionFromOptionalKey(Specimen) }))]);

// --- [SERVICES] ------------------------------------------------------------------------

class Fonts extends Context.Service<Fonts>()('@rasm/atelier/features/fonts', {
    make: RpcClient.make(RpcGroup.make(Rpc.make('faces', { success: Schema.Array(AvailableFace) }), Rpc.make('face', { payload: { name: AvailableFace.Name }, success: AvailableFace, error: Unmatched }))),
}) {
    static readonly layer = Layer.effect(Fonts, Fonts.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const applicationFont = (client: Effect.Success<ReturnType<Effect.Success<typeof listening>['call']>>) =>
    Effect.fnUntraced(function* (name: string, characters: readonly string[]) {
        const files = yield* FileSystem.FileSystem;
        const { font, source } = yield* client('font', { name, text: characters.join(''), file: yield* files.makeTempFileScoped({ suffix: '.otf' }) });
        return { font, source: yield* Effect.transposeOption(Option.map(source, ({ file, postscriptName }) => Effect.map(files.readFile(file), (data) => ({ data, postscriptName })))) };
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

const fonts = Effect.fnUntraced(function* ({ serving }: Effect.Success<ReturnType<typeof routes>>) {
    const [helper, route] = [yield* Fonts, serving('font')];
    yield* Resource.register({ name: 'faces', description: 'CoreText faces with file holding each', schema: Faces, changes: Stream.empty, content: helper.faces() });
    const tool = Tool.make(route.tag, { description: 'Shapes text in CoreText face or instance by name, or returns application font with its specimen when application is given', parameters: FontRequest, success: FontResult }).annotate(Tool.Readonly, true);
    yield* Route(route.optional('CoreText'))(tool, ({ name, coordinates, variations, size, width, settings, text, ...run }, chosen) => {
        const shaped = (source: typeof Source.Type) =>
            composed(
                source,
                { coordinates, variations, size },
                Option.map(text, (given) => ({ ...run, text: given })),
                width,
                settings,
            );
        const native = Effect.fnUntraced(function* ({ client }: Option.Option.Value<typeof chosen>) {
            const applied = yield* applicationFont(client)(name, Option.toArray(text));
            return { ...applied.font, specimen: yield* Effect.transposeOption(Option.map(applied.source, shaped)) };
        });
        const coreText = Effect.gen(function* () {
            const available = yield* helper.face({ name });
            return { ...available, specimen: yield* Effect.flatMap(available.source, shaped) };
        });
        return Option.match(chosen, { onNone: () => coreText, onSome: native });
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applicationFont, AvailableFace, Fonts, fonts };
