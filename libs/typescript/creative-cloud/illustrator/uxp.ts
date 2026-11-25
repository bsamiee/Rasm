import { HostError, type Render } from '@rasm/object-model';
import { type Containment, read } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Effect, Option, Path, Predicate, Record, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Typed = Schema.Struct({ typename: Schema.String });
const Method = Schema.declare((value: unknown): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).annotate({ description: 'Left, top, right, and bottom in points' });
const Application = Schema.Struct({
    name: Schema.NonEmptyString.annotate({ description: 'Bundle name without its extension' }),
    path: Schema.NonEmptyString.annotate({ description: 'Folder holding the running bundle' }),
    userInteractionLevel: Schema.String,
});
const Module = Schema.Struct({ app: Schema.declare(Schema.is(Application)).annotate({ description: 'Application instance require("illustrator") holds as app' }) });
const Artboards = Schema.Struct({ length: Schema.Int, add: Method, remove: Method, getActiveArtboardIndex: Method, setActiveArtboardIndex: Method });
const Document = Schema.declare(Schema.is(Schema.Struct({ artboards: Schema.declare(Schema.is(Artboards)), exportFile: Method }))).annotate({ description: 'Document by the members render calls' });
const Bounded = Schema.Struct({ visibleBounds: Rect }).annotate({ description: 'Document or page item by its bounds' });
const Artboard = Schema.Struct({ artboardRect: Rect });
const OWNED = {
    artboard: { collection: 'artboards', keys: [], is: Schema.is(Artboard) },
    layer: { collection: 'layers', keys: [], is: Schema.is(Schema.Struct({ typename: Schema.Literal('Layer') })) },
    pageItem: { collection: 'pageItems', keys: ['uuid'], is: Schema.is(Bounded.mapFields(Struct.assign({ layer: Schema.declare(Predicate.isObject) }))) },
} as const;
const Globals = Schema.Struct({ UserInteractionLevel: Schema.Struct({ DONTDISPLAYALERTS: Schema.String }), ExportType: Schema.Struct({ PNG24: Schema.String }), ExportOptionsPNG24: Schema.declare((value: unknown): value is new (options: object) => object => Predicate.isFunction(value)) }).annotate({
    description: 'Enumerations and creatable constructor the Illustrator module sets on globalThis',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = (value: unknown) => Array.findFirst([value, read(value, 'parentDocument')], Schema.is(Document));
const hosted = Effect.all([Effect.flatMap(Uxp.require('illustrator'), Schema.decodeUnknownEffect(Module)), Schema.decodeUnknownEffect(Globals)(globalThis)]);
const application = Effect.flatMap(Effect.all([hosted, Path.Path]), ([[{ app }], path]) => path.toFileUrl(`${app.path}/${app.name}.app/`));
const containment = Effect.map(hosted, ([{ app }]): Containment => ({
    root: app,
    type: (value) => Option.match(Schema.decodeUnknownOption(Typed)(value), { onNone: () => value.constructor.name, onSome: Struct.get('typename') }),
    kinds: { document: { collection: 'documents', keys: ['fullName'], is: Schema.is(Document) }, ...Record.map(OWNED, Struct.assign({ owner: (value: object): unknown => Option.getOrUndefined(opened(value)) })) },
}));
const suppressDialogs = Effect.flatMap(hosted, ([{ app }, { UserInteractionLevel }]) => Uxp.preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS }));
const execute = (code: string) => Effect.flatMap(containment, (held) => Uxp.execute(held, code, (call) => Effect.scoped(Effect.andThen(suppressDialogs, call))));
const render = Effect.fnUntraced(function* ({ target }: Render, written: File) {
    const [held, [, { ExportType, ExportOptionsPNG24 }]] = yield* Effect.all([containment, hosted, suppressDialogs]);
    const [found, first] = yield* Effect.all([Uxp.locate(held, target), Uxp.locate(held, { steps: Array.take(target.steps, 1) })]);
    const bounds = Option.orElse(Option.map(Schema.decodeUnknownOption(Artboard)(found), Struct.get('artboardRect')), () => Option.map(Schema.decodeUnknownOption(Bounded)(found), Struct.get('visibleBounds')));
    const unmatched = () => Unmatched.of({ subject: 'document, artboard, or page item', query: Option.some(target.path) });
    const [document, rect] = yield* Effect.all([Effect.fromOption(opened(first), unmatched), Effect.fromOption(bounds, unmatched)]);
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

export { application, containment, execute, render, suppressDialogs };
