import { Containment } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import type { Render } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { Callable, HostObject, Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Context, Effect, Layer, Option, Path, Predicate, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]);
const Application = HostObject({
    name: Schema.NonEmptyString,
    path: Schema.NonEmptyString,
    userInteractionLevel: Schema.String,
    documents: Uxp.Collection,
    getSelectedToolName: Callable,
    selectTool: Callable,
});
const Module = Schema.Struct({ app: Application });
const Filed = Schema.Struct({ fullName: Schema.Unknown });
const Document = HostObject({ ...Filed.fields, artboards: Uxp.Collection, layers: Uxp.Collection, pageItems: Uxp.Collection, getPageItemFromUuid: Callable, imageCapture: Callable });
const Identified = Schema.Struct({ uuid: Schema.String });
const PageItem = Identified.mapFields(Struct.assign({ parentDocument: Schema.Unknown }));
const Bounded = Schema.Struct({ visibleBounds: Rect });
const Artboard = Schema.Struct({ artboardRect: Rect });
const Globals = Schema.Struct({ UserInteractionLevel: Schema.Struct({ DONTDISPLAYALERTS: Schema.String }), ImageCaptureOptions: Schema.declare((value: unknown): value is new (options: object) => object => Predicate.isFunction(value)) });

// --- [SERVICES] ------------------------------------------------------------------------

class Illustrator extends Context.Service<Illustrator>()('@rasm/illustrator/uxp/Illustrator', {
    make: Effect.map(Effect.all([Uxp.require('illustrator', Module), Effect.suspend(() => Schema.decodeUnknownEffect(Globals)(globalThis))]), ([{ app }, globals]) => ({
        ...globals,
        app,
        containment: {
            root: app,
            kinds: {
                document: Containment.kind({ items: ({ documents }: typeof Application.Type) => documents, key: Filed, is: Schema.is(Document)<object> }),
                artboard: Containment.kind({ items: ({ artboards }: typeof Document.Type) => artboards }),
                layer: Containment.kind({ items: ({ layers }: typeof Document.Type) => layers }),
                pageItem: Containment.kind({ items: ({ pageItems }: typeof Document.Type) => pageItems, key: Identified, by: { uuid: (document, uuid) => document.getPageItemFromUuid(uuid) }, is: Schema.is(PageItem)<object>, owner: ({ parentDocument }) => parentDocument }),
            },
        } satisfies Containment,
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const application = Illustrator.use(({ app }) => Path.Path.use((path) => path.toFileUrl(`${app.path}/${app.name}.app/`)));
const toolbar = {
    current: Illustrator.use(({ app }) => Effect.try({ try: (): unknown => app.getSelectedToolName(), catch: HostError.refused('selected tool') })).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Schema.NonEmptyString)),
        Effect.map((id) => ({ id, label: id })),
    ),
    select: (id: string) => Illustrator.use(({ app }) => Effect.try({ try: (): unknown => app.selectTool(id), catch: HostError.refused(`tool ${id}`) })),
    changes: Uxp.ticks,
};
const suppressDialogs = Illustrator.use(({ app, UserInteractionLevel }) => Uxp.preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS }));
const execute = (code: string) => Illustrator.use(({ containment }) => Uxp.execute(containment, code));
const render = Effect.fnUntraced(function* ({ target }: Render, written: File) {
    const { containment, ImageCaptureOptions } = yield* Illustrator;
    const [found, first] = yield* Effect.all([Uxp.locate(containment, target), Uxp.locate(containment, { steps: Array.take(target.steps, 1) })]);
    const bounds = Option.orElse(Option.map(Schema.decodeUnknownOption(Artboard)(found), Struct.get('artboardRect')), () => Option.map(Schema.decodeUnknownOption(Bounded)(found), Struct.get('visibleBounds')));
    const unmatched = () => Unmatched.of({ subject: 'document, artboard, or page item', query: Option.some(target.path) });
    const [document, clipBounds] = yield* Effect.all([Effect.fromOption(Schema.decodeUnknownOption(Document)(first), unmatched), Effect.fromOption(bounds, unmatched)]);
    yield* Effect.try({
        try: () => document.imageCapture(written, clipBounds, new ImageCaptureOptions({ resolution: 72, transparency: true, antiAliasing: true })),
        catch: HostError.refused(`imageCapture of ${clipBounds.join(' ')} to ${written.nativePath}`),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, execute, Illustrator, render, suppressDialogs, toolbar };
