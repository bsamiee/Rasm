import { Automation, AvailableFace, FaceName } from '@rasm/automation';
import { FontView } from '@rasm/indesign';
import { READ_ONLY, registerResource, registerRoutedTool } from '@rasm/mcp';
import { Face, Instance, Run, Shaped, type Source } from '@rasm/typography';
import { Effect, FileSystem, Option, Path, Schema, SchemaAST, Stream } from 'effect';
import { Tool } from 'effect/ai';
import { InDesignScript } from '#/indesign-script';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const specimen = Schema.Struct({ face: Face, shaped: Shaped.annotate({ description: 'Em positions, not InDesign composition or document coordinates' }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const faces = Automation.use((automation) => registerResource({ uri: 'atelier://faces', name: 'faces', description: 'CoreText faces with file holding each', mimeType: 'application/json', schema: Schema.Array(AvailableFace), changes: Stream.empty, content: automation.faces() }));
const font = Effect.fnUntraced(function* ({ indesign, listed }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, indesignScript, path, files] = yield* Effect.all([Automation, InDesignScript, Path.Path, FileSystem.FileSystem]);
    const hosts = yield* indesignScript.hosts;
    const tool = Tool.make('font', {
        description: 'Shapes text in named face or instance. With application, returns InDesign font view',
        parameters: Instance.pipe(
            Schema.fieldsAssign({
                ...Run.fields,
                text: Run.fields.text.annotate({ description: 'One-script one-direction run to shape and check glyphs, no geometry when absent' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
                name: Schema.String.annotate({ description: `${SchemaAST.resolveDescription(FaceName.ast)}, or InDesign font name or specifier` }),
            }),
        ),
        success: Schema.Union([Schema.Struct({ ...AvailableFace.fields, specimen }), Schema.Struct({ ...FontView.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
    }).annotateMerge(READ_ONLY);
    const typeset = Effect.fnUntraced(function* ({ name, variations, size, ...run }: Tool.Parameters<typeof tool>, chosen: Option.Option<{ readonly pid: number }>) {
        const shaped = (source: typeof Source.Type) => Effect.map(Face.open(source, { variations, size }), (face) => ({ face, shaped: Shaped.of(face, run) }));
        const native = Effect.fnUntraced(function* ({ pid }: { readonly pid: number }) {
            const call = yield* indesignScript.client(pid);
            const { view, source } = yield* call('font', { name, text: run.text });
            return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, shaped)) };
        }, Effect.scoped);
        const shaping = Effect.gen(function* () {
            const found = yield* automation.face({ name });
            return { ...found, specimen: yield* shaped({ data: yield* files.readFile(yield* path.fromFileUrl(found.file)), postscriptName: Option.some(found.postscriptName) }) };
        });
        return yield* Option.match(chosen, { onNone: () => shaping, onSome: native });
    });
    yield* registerRoutedTool({
        ...indesign,
        fields: { application: Schema.OptionFromOptionalKey(listed(hosts).annotate({ description: 'InDesign that answers, CoreText when absent' })) },
        resolve: ({ application }) => Effect.transposeOption(Option.map(application, (name) => indesign.resolve({ application: Option.some(name), pid: Option.none() }))),
        depends: Option.match({ onNone: () => Effect.succeed([]), onSome: indesign.depends }),
    })(tool, typeset);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { faces, font };
