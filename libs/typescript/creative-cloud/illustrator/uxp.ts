import { HostError, type Render } from '@rasm/object-model';
import type { Containment } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Callable, Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Effect, Function, Match, Option, Path, Predicate, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).annotate({ description: 'Left, top, right, and bottom in points' });
const Application = Schema.Struct({
    name: Schema.NonEmptyString.annotate({ description: 'Bundle name without its extension' }),
    path: Schema.NonEmptyString.annotate({ description: 'Folder holding the running bundle' }),
    userInteractionLevel: Schema.String,
    documents: Uxp.Collection,
    getSelectedToolName: Callable,
    selectTool: Callable,
});
const Module = Schema.Struct({ app: Schema.declare(Schema.is(Application)).annotate({ description: 'Application instance require("illustrator") holds as app' }) });
const Artboards = Uxp.Collection.mapFields(Struct.assign({ add: Callable, remove: Callable, getActiveArtboardIndex: Callable, setActiveArtboardIndex: Callable }));
const Document = Schema.declare(Schema.is(Schema.Struct({ artboards: Schema.declare(Schema.is(Artboards)), layers: Uxp.Collection, pageItems: Uxp.Collection, exportFile: Callable }))).annotate({ description: 'Document by its collections and the members render calls' });
const Owned = Schema.Struct({ parentDocument: Document }).annotate({ description: 'Page item by the document holding it' });
const Bounded = Schema.Struct({ visibleBounds: Rect }).annotate({ description: 'Document or page item by its bounds' });
const Artboard = Schema.Struct({ artboardRect: Rect });
const Globals = Schema.Struct({ UserInteractionLevel: Schema.Struct({ DONTDISPLAYALERTS: Schema.String }), ExportType: Schema.Struct({ PNG24: Schema.String }), ExportOptionsPNG24: Schema.declare((value: unknown): value is new (options: object) => object => Predicate.isFunction(value)) }).annotate({
    description: 'Enumerations and creatable constructor the Illustrator module sets on globalThis',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const documentOf = Match.type<unknown>().pipe(Match.when(Schema.is(Document), Function.identity), Match.when(Schema.is(Owned), ({ parentDocument }) => parentDocument), Match.orElse(Function.constUndefined));
const hosted = Effect.all([Effect.flatMap(Uxp.require('illustrator'), Schema.decodeUnknownEffect(Module)), Schema.decodeUnknownEffect(Globals)(globalThis)]);
const application = Effect.flatMap(Effect.all([hosted, Path.Path]), ([[{ app }], path]) => path.toFileUrl(`${app.path}/${app.name}.app/`));
const containment = Effect.map(
    hosted,
    ([{ app }]): Containment => ({
        root: app,
        kinds: {
            document: { items: ({ documents }: typeof Application.Type) => documents, key: Schema.Struct({ fullName: Schema.Unknown }), is: Schema.is(Document) },
            artboard: { items: ({ artboards }: typeof Document.Type) => artboards, is: Schema.is(Artboard), owner: documentOf },
            layer: { items: ({ layers }: typeof Document.Type) => layers, is: Schema.is(Schema.Struct({ typename: Schema.Literal('Layer') })), owner: documentOf },
            pageItem: { items: ({ pageItems }: typeof Document.Type) => pageItems, key: Schema.Struct({ uuid: Schema.String }), is: Schema.is(Bounded.mapFields(Struct.assign({ layer: Schema.declare(Predicate.isObject) }))), owner: documentOf },
        },
    }),
);
const toolbar = {
    current: Effect.flatMap(hosted, ([{ app }]) => Effect.try({ try: (): unknown => app.getSelectedToolName(), catch: HostError.refused('selected tool') })).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Schema.NonEmptyString)),
        Effect.map((id) => ({ id, label: id })),
    ),
    select: (id: string) => Effect.flatMap(hosted, ([{ app }]) => Effect.try({ try: (): unknown => app.selectTool(id), catch: HostError.refused(`tool ${id}`) })),
};
const suppressDialogs = Effect.flatMap(hosted, ([{ app }, { UserInteractionLevel }]) => Uxp.preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS }));
const execute = (code: string) => Effect.flatMap(containment, (held) => Uxp.execute(held, code, (call) => Effect.scoped(Effect.andThen(suppressDialogs, call))));
const render = Effect.fnUntraced(function* ({ target }: Render, written: File) {
    const [held, [, { ExportType, ExportOptionsPNG24 }]] = yield* Effect.all([containment, hosted, suppressDialogs]);
    const [found, first] = yield* Effect.all([Uxp.locate(held, target), Uxp.locate(held, { steps: Array.take(target.steps, 1) })]);
    const bounds = Option.orElse(Option.map(Schema.decodeUnknownOption(Artboard)(found), Struct.get('artboardRect')), () => Option.map(Schema.decodeUnknownOption(Bounded)(found), Struct.get('visibleBounds')));
    const unmatched = () => Unmatched.of({ subject: 'document, artboard, or page item', query: Option.some(target.path) });
    const [document, rect] = yield* Effect.all([Effect.fromOption(Option.fromUndefinedOr(documentOf(first)), unmatched), Effect.fromOption(bounds, unmatched)]);
    const { artboards } = document;
    const active = yield* Effect.try({ try: (): unknown => artboards.getActiveArtboardIndex(), catch: HostError.refused('getActiveArtboardIndex') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Int)));
    const restored = HostError.refused(`restore of artboard ${active}`);
    const added = yield* Effect.acquireRelease(Effect.as(Effect.try({ try: (): unknown => artboards.add(rect), catch: HostError.refused(`artboards.add of ${rect.join(' ')}`) }), artboards.length), (index) =>
        Effect.orDie(Effect.andThen(Effect.try({ try: () => artboards.remove(index), catch: restored }), Effect.try({ try: () => artboards.setActiveArtboardIndex(active), catch: restored }))),
    );
    const exported = HostError.refused(`exportFile ${ExportType.PNG24} to ${written.nativePath}`);
    yield* Effect.try({ try: () => artboards.setActiveArtboardIndex(added), catch: exported });
    yield* Effect.try({ try: () => document.exportFile(written, ExportType.PNG24, new ExportOptionsPNG24({ artBoardClipping: true })), catch: exported });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, containment, execute, render, suppressDialogs, toolbar };
