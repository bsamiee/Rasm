import { HostError, type Render } from '@rasm/object-model';
import { type Containment, read } from '@rasm/object-model/host';
import { Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Cause, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Typed = Schema.Struct({ typename: Schema.String });
const Method = Schema.declare((value: unknown): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const Rect = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).annotate({ description: 'Left, top, right, and bottom in points' });
const Application = Schema.Struct({ userInteractionLevel: Schema.Literals(['DISPLAYALERTS', 'DONTDISPLAYALERTS']) });
const Module = Schema.Struct({ app: Schema.declare(Schema.is(Application)).annotate({ description: 'Application instance require("illustrator") holds as app' }) });
const Artboards = Schema.Struct({ length: Schema.Int, add: Method, getActiveArtboardIndex: Method, setActiveArtboardIndex: Method });
const Document = Schema.declare(Schema.is(Schema.Struct({ artboards: Schema.declare(Schema.is(Artboards)), exportFile: Method }))).annotate({ description: 'Document by the members render calls' });
const Bounded = Schema.Struct({ visibleBounds: Rect }).annotate({ description: 'Document or page item by its bounds' });
const Artboard = Schema.Struct({ artboardRect: Rect });
const OWNED = {
    artboard: { collection: 'artboards', keys: [], is: Schema.is(Artboard) },
    layer: { collection: 'layers', keys: [], is: Schema.is(Schema.Struct({ typename: Schema.Literal('Layer') })) },
    pageItem: { collection: 'pageItems', keys: ['uuid'], is: Schema.is(Bounded.mapFields(Struct.assign({ layer: Schema.declare(Predicate.isObject) }))) },
} as const;
const Removable = Schema.declare(Schema.is(Schema.Struct({ remove: Method }))).annotate({ description: 'Artboard artboards.add returns' });
const Options = Schema.Struct({ ExportOptionsPNG24: Schema.declare((value: unknown): value is new (props: Readonly<Record<string, unknown>>) => object => Predicate.isFunction(value)) }).annotate({ description: 'Creatable constructor the Illustrator module sets on globalThis' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const type = (value: object): string => Option.match(Schema.decodeUnknownOption(Typed)(value), { onNone: () => value.constructor.name, onSome: Struct.get('typename') });
const listed = (collection: unknown) => {
    const length = read(collection, 'length');
    return Array.makeBy(Predicate.isNumber(length) ? length : 0, (at) => read(collection, `${at}`));
};
const opened =
    (app: object) =>
    (value: unknown): Option.Option<typeof Document.Type> => {
        const above = Array.unfold(value, (each) => (Predicate.isObject(each) ? Option.some([each, Schema.is(Document)(each) || read(each, 'parent') === each ? undefined : read(each, 'parent')] as const) : Option.none()));
        return Option.filter(
            Option.orElse(Array.findFirst(above, Schema.is(Document)), () => Array.findFirst(listed(read(app, 'documents')), (document) => Record.some(OWNED, ({ collection }) => listed(read(document, collection)).includes(value)))),
            Schema.is(Document),
        );
    };
const hosted = Effect.flatMap(Uxp.required('illustrator'), Schema.decodeUnknownEffect(Module));
const containment = Effect.map(hosted, ({ app }): Containment => {
    const owner = (value: object): unknown => Option.getOrUndefined(opened(app)(value));
    return {
        root: app,
        type,
        kinds: { document: { collection: 'documents', keys: ['fullName'], is: Schema.is(Document) }, ...Record.map(OWNED, Struct.assign({ owner })) },
    };
});
const session = Effect.flatMap(hosted, ({ app }) => Uxp.preserve(app, { userInteractionLevel: 'DONTDISPLAYALERTS' }));
const execute = (code: string) => Effect.flatMap(containment, (held) => Uxp.executed(held, code, (call) => Effect.scoped(Effect.andThen(session, call))));
const render = Effect.fnUntraced(function* ({ target }: Render, written: File) {
    const [held] = yield* Effect.all([containment, session]);
    const [found, { ExportOptionsPNG24 }] = yield* Effect.all([Uxp.located(held, target), Schema.decodeUnknownEffect(Options)(globalThis)]);
    const bounds = Option.orElse(Option.map(Schema.decodeUnknownOption(Artboard)(found), Struct.get('artboardRect')), () => Option.map(Schema.decodeUnknownOption(Bounded)(found), Struct.get('visibleBounds')));
    const [document, rect] = yield* Effect.all([
        Effect.fromOption(opened(held.root)(found), () => new Cause.IllegalArgumentError(`${target.path} sits in no open document, render takes a Document, Artboard, or page item of one`)),
        Effect.fromOption(bounds, () => new Cause.IllegalArgumentError(`${Predicate.isObject(found) ? type(found) : typeof found} has no bounds, render takes a Document, Artboard, or page item`)),
    ]);
    const { artboards } = document;
    const active = yield* Effect.try({ try: (): unknown => artboards.getActiveArtboardIndex(), catch: HostError.refused('getActiveArtboardIndex') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Int)));
    yield* Effect.acquireRelease(Effect.try({ try: (): unknown => artboards.add(rect), catch: HostError.refused(`artboards.add of ${rect.join(' ')}`) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Removable))), (added) =>
        Effect.orDie(Effect.try({ try: () => [added.remove(), artboards.setActiveArtboardIndex(active)], catch: HostError.refused(`restore of artboard ${active}`) })),
    );
    const last = yield* Schema.decodeEffect(Schema.Int)(artboards.length);
    yield* Effect.try({
        try: () => [artboards.setActiveArtboardIndex(last - 1), document.exportFile(written.nativePath, 'PNG24', new ExportOptionsPNG24({ artBoardClipping: true }))],
        catch: HostError.refused(`exportFile PNG24 to ${written.nativePath}`),
    });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { containment, execute, render, session };
