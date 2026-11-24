import type { Upstream } from '@rasm/mcp';
import { HostError, type Render } from '@rasm/object-model';
import { type Containment, format, read } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import { action, app, core } from 'adobe:photoshop';
import { type File, host } from 'adobe:uxp';
import { Array, Effect, Exit, Match, Option, Predicate, Queue, Record, Schema, Stream, String, Struct, UndefinedOr } from 'effect';
import type { ExecutionContext } from 'photoshop/dom/CoreModules';
import type { Document } from 'photoshop/dom/Document';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REJECTIONS = { Cancelled: 2, Unavailable: 9 } as const;
const COLLECTIONS = {
    document: 'documents',
    textFont: 'fonts',
    layer: 'layers',
    artboard: 'artboards',
    channel: 'channels',
    colorSampler: 'colorSamplers',
    countItem: 'countItems',
    guide: 'guides',
    historyState: 'historyStates',
    layerComp: 'layerComps',
    pathItem: 'pathItems',
} as const satisfies Record<string, keyof typeof app | keyof Document>;

// --- [MODELS] --------------------------------------------------------------------------

const Typed = Schema.Struct({ typename: Schema.String });
const Exportable = Schema.declare((input: unknown): input is Document | Document['layers'][number] => Predicate.hasProperty(input, 'typename') && (input.typename === 'Document' || input.typename === 'Layer'));
const Entry = Uxp.Entry.mapFields(Struct.assign({ isFile: Schema.Literal(true) }));
const Stored = Schema.declare((input: unknown): input is globalThis.File => Schema.is(Entry)(input)).annotate({ description: 'UXP storage file, the DOM File type @types/photoshop names for file parameters' });
const Closing = Schema.Struct({ registerAutoCloseDocument: Schema.declare((input: unknown): input is (documentID: number) => Promise<void> => Predicate.isFunction(input)) }).annotate({ description: 'hostControl member Photoshop holds and @types/photoshop omits' });
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });
const Hosted = Schema.Tuple([Schema.Struct({ hostName: Schema.NonEmptyString.annotate({ description: 'Application name, the bundle name of the installation' }) })]);
const Notified = Schema.Struct({ documentID: Schema.Int }).annotate({ description: 'Descriptor of a document event' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const containment: Containment = {
    root: app,
    type: (value) => Option.match(Schema.decodeUnknownOption(Typed)(value), { onNone: () => value.constructor.name, onSome: Struct.get('typename') }),
    kinds: Record.map(COLLECTIONS, (collection: string, kind) => ({
        collection,
        nested: kind === 'layer',
        ...(kind === 'textFont' ? { keys: ['postScriptName'] } : {}),
        is: (value: object) => Option.exists(Schema.decodeUnknownOption(Typed)(value), ({ typename }) => String.uncapitalize(typename) === kind),
        ...(collection in app ? {} : { owner: (value: object) => UndefinedOr.map(read(value, 'document') ?? read(value, 'parent'), (above) => (Array.isArray(above) ? read(above, 'parent') : above)) }),
    })),
};
const holds = (layer: Document['layers'][number], id: number): boolean => (layer.layers ?? []).some((each) => each.id === id || holds(each, id));
const isolated = (layers: readonly Document['layers'][number][], id: number): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (holds(layer, id) ? isolated(layer.layers ?? [], id) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
        { discard: true },
    );
const cancelled = (context: ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const modal = Effect.fnUntraced(function* <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) {
    const services = yield* Effect.context<R>();
    const settled = yield* Effect.tryPromise({
        try: (signal): Promise<unknown> => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }),
        catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.of({ _tag }, commandName)(cause) }),
    });
    return yield* yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.declare((value: unknown): value is Exit.Exit<A, E> => Exit.isExit(value)))(settled));
});
const history = <A, E, R>(label: string, { hostControl }: ExecutionContext, body: Effect.Effect<A, E, R>) =>
    app.documents.reduce<Effect.Effect<A, E | HostError, R>>(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => (app.documents.some(({ id: openId }) => openId === id) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        body,
    );
const execute = (code: string, label: string) => modal(label, (context) => history(label, context, Uxp.executed(containment, code)));
const render = Effect.fnUntraced(function* ({ target, label }: Render, written: File) {
    const [found, stored] = yield* Effect.all([Uxp.located(containment, target).pipe(Effect.filterOrFail(Schema.is(Exportable), () => Unmatched.of({ subject: 'document, layer, or artboard', query: Option.some(target.path) }))), Effect.orDie(Schema.decodeUnknownEffect(Stored)(written))]);
    return yield* modal(
        label,
        Effect.fnUntraced(function* ({ hostControl }) {
            const { document, placed } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, placed: [] }), Layer: (layer) => ({ document: layer.document, placed: [layer] }) }));
            const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
            const refused = HostError.refused(`registerAutoCloseDocument of document ${copy.id}`);
            const closing = yield* Effect.filterOrFail(Effect.succeed(hostControl), Schema.is(Closing), refused);
            yield* Effect.tryPromise({ try: () => closing.registerAutoCloseDocument(copy.id), catch: refused });
            yield* Effect.forEach(placed, ({ id, bounds }) => Effect.andThen(isolated(copy.layers, id), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
            yield* Effect.tryPromise({ try: () => copy.saveAs.png(stored, undefined, true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
        }),
    );
});
const notices = Stream.callback<Stream.Success<Upstream['notices']>, HostError>((queue) => {
    const events = ['open', 'save', 'close'];
    const listener = (name: string, descriptor: unknown): void => {
        const target = Option.map(Schema.decodeUnknownOption(Notified)(descriptor), ({ documentID }) => format({ steps: [{ separator: '/', kind: 'document', selector: { _tag: 'Key', key: 'id', value: documentID } }] }));
        Queue.offerUnsafe(queue, { name, target, detail: Option.none() });
    };
    return Effect.acquireRelease(Effect.tryPromise({ try: () => Promise.resolve(action.addNotificationListener(events, listener)), catch: HostError.refused(`addNotificationListener of ${events.join(', ')}`) }), () =>
        Effect.orDie(Effect.tryPromise({ try: () => Promise.resolve(action.removeNotificationListener(events, listener)), catch: HostError.refused(`removeNotificationListener of ${events.join(', ')}`) })),
    );
});
const application = Effect.tryPromise({
    try: () => action.batchPlay([{ _obj: 'get', _target: [{ _property: 'hostName' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {}),
    catch: HostError.refused(`Photoshop ${host.version} host name`),
}).pipe(
    Effect.flatMap(Schema.decodeUnknownEffect(Hosted)),
    Effect.map(([{ hostName }]) => hostName),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, execute, notices, render };
