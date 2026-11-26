import { type Description, FacetControl } from '@rasm/applications/source';
import type { Upstream } from '@rasm/mcp/upstream';
import { Containment, format } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import type { Render } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/plugin';
import { action, app, constants, core } from 'adobe:photoshop';
import type { File } from 'adobe:uxp';
import { Array, Cause, Context, Effect, Exit, Function, flow, Layer, Match, Option, Predicate, Queue, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import type { ActionDescriptor, ExecutionContext } from 'photoshop/dom/CoreModules';
import type { Document } from 'photoshop/dom/Document';

// --- [TYPES] ---------------------------------------------------------------------------

type Leaf = Pick<FacetControl, 'kind' | 'role' | 'value' | 'selected'> & { readonly write: (to: unknown) => Option.Option<unknown> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const REJECTIONS = { Cancelled: 2, Unavailable: 9 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identified = Schema.Struct({ id: Schema.Int }).annotate({ description: 'Object keyed by its id' });
const Exportable = Schema.declare((input: unknown) => input instanceof app.Document || input instanceof app.Layer).annotate({ description: 'Document or layer, the objects render exports' });
const Stored = Schema.declare((input: unknown): input is globalThis.File => Schema.is(Uxp.Entry)(input)).annotate({ description: 'UXP storage file, the DOM File type @types/photoshop names for file parameters' });
const Closing = Schema.Struct({ registerAutoCloseDocument: Schema.declare((input: unknown): input is (documentID: number) => Promise<void> => Predicate.isFunction(input)) }).annotate({ description: 'hostControl member Photoshop holds and @types/photoshop omits' });
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });
const Refusal = Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Int }).annotate({ description: 'Error descriptor a refused command replies with' });
const Hosted = Schema.Struct({ hostName: Schema.NonEmptyString.annotate({ description: 'Application name, the bundle name of the installation' }) });
const ActionObject = Schema.StructWithRest(Schema.Struct({ _obj: Schema.String }), [Schema.Record(Schema.String, Schema.Unknown)]).annotate({ description: 'Descriptor of an object, its keys the string or character IDs Photoshop names them by' });
const ToolOptions = Schema.Struct({ currentToolOptions: ActionObject }).annotate({ description: 'Options of the current tool, the application property holding them' });
const DocumentEvent = Schema.Struct({ documentID: Schema.Int }).annotate({ description: 'Descriptor of a document event' });
const Enumerated = Schema.Struct({ _enum: Schema.String, _value: Schema.String }).annotate({ description: 'Enumeration value with its type' });
const Unit = Schema.Struct({ _unit: Schema.String, _value: Schema.Finite }).annotate({ description: 'Number with its unit' });
const ModalState = Schema.Struct({
    level: Schema.Int.annotate({ description: 'Depth of the modal state, 0 once it exits' }),
    kind: Schema.optionalKey(Enumerated.annotate({ description: 'Kind of a tool modal state, as toolModalStateChanged names it' })),
    title: Schema.optionalKey(Schema.String.annotate({ description: 'Title of a modal dialog, as modalStateChanged names it' })),
}).annotate({ description: 'Descriptor of toolModalStateChanged or modalStateChanged' });

// --- [SERVICES] ------------------------------------------------------------------------

class Notifications extends Context.Service<Notifications, { readonly events: Stream.Stream<Stream.Success<Upstream['events']>, HostError>; readonly changes: Stream.Stream<unknown, HostError>; readonly operations: Effect.Effect<readonly string[]> }>()('@rasm/photoshop/uxp/Notifications') {
    static readonly layer = Layer.effect(Notifications)(
        Effect.gen(function* () {
            const entered = yield* SubscriptionRef.make<Record.ReadonlyRecord<string, string>>({});
            const track = (name: string, descriptor: unknown) =>
                Option.match(
                    Option.flatMap(Schema.decodeUnknownOption(ModalState)(descriptor), ({ level, kind, title }) => Option.map(Option.fromUndefinedOr(UndefinedOr.map(kind, Struct.get('_value')) ?? title), (operation) => ({ level, operation }))),
                    { onNone: () => Effect.succeedNone, onSome: ({ level, operation }) => Effect.as(SubscriptionRef.update(entered, level > 0 ? Record.set(name, operation) : Record.remove(name)), Option.none()) },
                );
            const handlers: Record.ReadonlyRecord<string, (name: string, descriptor: unknown) => Effect.Effect<Option.Option<Stream.Success<Upstream['events']>>>> = {
                open: toEvent,
                save: toEvent,
                close: toEvent,
                select: () => Effect.succeedNone,
                toolModalStateChanged: track,
                modalStateChanged: track,
            };
            const names = Record.keys(handlers);
            const heard = Stream.callback<readonly [string, unknown], HostError>((queue) => {
                const listener = (name: string, descriptor: unknown): void => {
                    Queue.offerUnsafe(queue, [name, descriptor]);
                };
                return Effect.acquireRelease(Effect.tryPromise({ try: () => Promise.resolve(action.addNotificationListener(names, listener)), catch: HostError.refused(`notifications ${names.join(', ')}`) }), () =>
                    Effect.orDie(Effect.tryPromise({ try: () => Promise.resolve(action.removeNotificationListener(names, listener)), catch: HostError.refused(`notifications ${names.join(', ')}`) })),
                );
            });
            const changes = yield* heard.pipe(
                Stream.mapEffect(([name, descriptor]) => Option.match(Record.get(handlers, name), { onNone: () => Effect.succeedNone, onSome: (handle) => handle(name, descriptor) })),
                Stream.share({ capacity: 'unbounded' }),
            );
            yield* Effect.forkScoped(Effect.tapCause(Stream.runDrain(changes), Effect.logError));
            return { events: Stream.filterMap(changes, (event) => Result.fromOption(event, Function.constVoid)), changes, operations: Effect.map(SubscriptionRef.get(entered), Record.values) };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const documentOf = ({ parent }: { readonly parent: Document }) => parent;
const channel = Containment.kind({ items: (document: Document) => document.channels, key: Schema.Struct({ name: Schema.String }), by: { name: (document, name) => document.channels.getByName(String(name)) }, owner: documentOf });
const containment: Containment = {
    root: app,
    kinds: {
        document: Containment.kind({ items: (photoshop: typeof app) => photoshop.documents, key: Identified }),
        textFont: Containment.kind({ items: (photoshop: typeof app) => photoshop.fonts, key: Schema.Struct({ postScriptName: Schema.String }), by: { postScriptName: (photoshop, name) => photoshop.fonts.getByName(String(name)) } }),
        layer: Containment.kind({ items: (owner: Document | Document['layers'][number]) => owner.layers, key: Identified, nested: true, owner: ({ document }) => document }),
        artboard: Containment.kind({ items: (document: Document) => document.artboards, key: Identified }),
        componentChannel: channel,
        alphaChannel: channel,
        colorSampler: Containment.kind({ items: (document: Document) => document.colorSamplers, owner: documentOf }),
        countItem: Containment.kind({ items: (document: Document) => document.countItems, owner: ({ parent }) => parent.parent }),
        guide: Containment.kind({ items: (document: Document) => document.guides, key: Identified, owner: documentOf }),
        historyState: Containment.kind({ items: (document: Document) => document.historyStates, key: Identified, owner: documentOf }),
        layerComp: Containment.kind({ items: (document: Document) => document.layerComps, key: Identified, owner: documentOf }),
        pathItem: Containment.kind({ items: (document: Document) => document.pathItems, key: Identified, owner: documentOf }),
    },
};
const toEvent = (name: string, descriptor: unknown) => Effect.succeedSome({ name, target: Option.map(Schema.decodeUnknownOption(DocumentEvent)(descriptor), ({ documentID }) => format({ steps: [{ separator: '/', kind: 'document', selector: { _tag: 'Key', key: 'id', value: documentID } }] })), detail: Option.none() });
const lineage = (layer: Document['layers'][number] | null): readonly number[] => (layer === null ? [] : [layer.id, ...lineage(layer.parent)]);
const isolate = (layers: readonly Document['layers'][number][], id: number, above: readonly number[]): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (above.includes(layer.id) ? isolate(layer.layers ?? [], id, above) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
        { discard: true },
    );
const interruptOnCancel = (context: ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const modal = Effect.fnUntraced(function* <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) {
    const services = yield* Effect.context<R>();
    const settled = yield* Effect.tryPromise({
        try: (signal): Promise<unknown> => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), interruptOnCancel(context)), { signal }), { commandName }),
        catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.of({ _tag }, commandName)(cause) }),
    });
    return yield* yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.declare((value: unknown): value is Exit.Exit<A, E> => Exit.isExit(value)))(settled));
});
const play = (commands: ActionDescriptor[]) =>
    Effect.flatMap(Effect.tryPromise({ try: () => action.batchPlay(commands, {}), catch: HostError.refused(Array.map(commands, Struct.get('_obj')).join(', ')) }), (replies) =>
        Option.match(
            Array.findFirst(Array.zip(commands, replies), ([{ _obj: command }, reply]) => Option.map(Schema.decodeUnknownOption(Refusal)(reply), HostError.refused(command))),
            { onNone: () => Effect.succeed(replies), onSome: Effect.fail },
        ),
    );
const history = <A, E, R>(label: string, { hostControl }: ExecutionContext, body: Effect.Effect<A, E, R>) =>
    app.documents.reduce<Effect.Effect<A, E | HostError, R>>(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => (action.validateReference({ _ref: 'document', _id: id }) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        body,
    );
const suppressDialogs = Uxp.preserve(app, { displayDialogs: constants.DialogModes.ERROR });
const execute = (code: string, label: string) => modal(label, (context) => history(label, context, Uxp.execute(containment, code)));
const render = Effect.fnUntraced(function* ({ target, label }: Render, file: File) {
    const [found, stored] = yield* Effect.all([Uxp.locate(containment, target).pipe(Effect.filterOrFail(Schema.is(Exportable), () => Unmatched.of({ subject: 'document, layer, or artboard', query: Option.some(target.path) }))), Effect.orDie(Schema.decodeUnknownEffect(Stored)(file))]);
    return yield* modal(
        label,
        Effect.fnUntraced(function* ({ hostControl }) {
            const { document, placed } = found instanceof app.Layer ? { document: found.document, placed: [found] } : { document: found, placed: [] };
            const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
            const refused = HostError.refused(`registerAutoCloseDocument of document ${copy.id}`);
            const closing = yield* Effect.fromResult(Result.liftPredicate(hostControl, Schema.is(Closing), refused));
            yield* Effect.tryPromise({ try: () => closing.registerAutoCloseDocument(copy.id), catch: refused });
            yield* Effect.forEach(placed, ({ id, bounds, parent }) => Effect.andThen(isolate(copy.layers, id, lineage(parent)), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
            yield* Effect.tryPromise({ try: () => copy.saveAs.png(stored, undefined, true), catch: HostError.refused(`saveAs.png to ${file.nativePath}`) });
        }),
    );
});
const property = <const Fields extends Schema.Struct.Fields>(reply: Schema.Struct<Fields>) =>
    play(Array.map(Struct.keys(reply.fields), (_property) => ({ _obj: 'get', _target: [{ _property }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }))).pipe(Effect.flatMap((replies) => Schema.decodeUnknownEffect(reply)(Object.assign({}, ...replies))));
const application = Effect.map(property(Hosted), Struct.get('hostName'));
const leaf = Match.type<unknown>().pipe(
    Match.withReturnType<Leaf>(),
    Match.when(Match.boolean, (value) => ({ kind: 'toggle', role: typeof value, value: { _tag: 'bool', value }, write: Option.liftPredicate(Predicate.isBoolean) })),
    Match.when(Match.number, (value) => ({ kind: 'number', role: typeof value, value: { _tag: 'real', value }, write: Option.liftPredicate(Predicate.isNumber) })),
    Match.when(Match.string, (value) => ({ kind: 'text', role: typeof value, value: { _tag: 'text', value }, write: Option.liftPredicate(Predicate.isString) })),
    Match.when(Schema.is(Unit), ({ _unit: role, _value: value }) => ({
        kind: 'measure',
        role,
        value: { _tag: 'real', value },
        write: flow(
            Option.liftPredicate(Predicate.isNumber),
            Option.map((number) => ({ _unit: role, _value: number })),
        ),
    })),
    Match.when(Schema.is(Enumerated), ({ _enum: role, _value: value }) => ({
        kind: 'choice',
        role,
        value: { _tag: 'text', value },
        selected: 'attribute',
        write: flow(
            Option.liftPredicate(Predicate.isString),
            Option.map((text) => ({ _enum: role, _value: text })),
        ),
    })),
    Match.option,
);
const controls = ({ _obj: owner, ...held }: typeof ActionObject.Type, path: typeof Description.Locator.Type): readonly FacetControl[] =>
    Array.flatMap(Record.toEntries(held), ([key, value]) => {
        const locator = Array.append(path, [owner, key] as const);
        return Option.match(leaf(value), {
            onSome: (stated) => [FacetControl.make({ ...Struct.omit(stated, ['write']), locator, label: { text: key, attribute: owner } })],
            onNone: () => (Schema.is(ActionObject)(value) ? controls(value, locator) : []),
        });
    });
const update = ({ _obj: owner, ...held }: typeof ActionObject.Type, [key, ...below]: Array.NonEmptyReadonlyArray<string>, to: unknown): Result.Result<unknown, Unmatched | Cause.IllegalArgumentError> => {
    const value = held[key];
    const unmatched = Result.fail(Unmatched.of({ subject: `option of ${owner}`, query: Option.some(key), candidates: Record.keys(held) }));
    return Array.match(below, {
        onEmpty: () =>
            Object.hasOwn(held, key)
                ? Result.fromOption(
                      Option.flatMap(leaf(value), ({ write }): Option.Option<unknown> => write(to)),
                      () => new Cause.IllegalArgumentError(`option ${key} of ${owner} takes a value of the kind it holds, received ${typeof to}`),
                  )
                : unmatched,
        onNonEmpty: (deeper) => (Schema.is(ActionObject)(value) ? Result.map(update(value, deeper, to), (inner) => ({ ...value, [Array.headNonEmpty(deeper)]: inner })) : unmatched),
    });
};
const toolbar = {
    current: Effect.map(Effect.tryPromise({ try: () => core.getActiveTool(), catch: HostError.refused('active tool') }), ({ key: id, title: label }) => ({ id, label })),
    select: (id: string) => modal(`tool ${id}`, () => play([{ _obj: 'select', _target: [{ _ref: id }] }])),
    changes: Stream.unwrap(Notifications.useSync(Struct.get('changes'))),
    options: {
        describe: (at: typeof Description.Locator.Type) => Effect.map(property(ToolOptions), ({ currentToolOptions }) => controls(currentToolOptions, at)),
        operate: (tool: string, path: Array.NonEmptyReadonlyArray<string>, to: unknown) =>
            property(ToolOptions).pipe(
                Effect.flatMap(({ currentToolOptions, currentToolOptions: { _obj: owner } }) => Effect.map(Effect.fromResult(update(currentToolOptions, path, to)), (value) => ({ _obj: owner, [Array.headNonEmpty(path)]: value }))),
                Effect.flatMap((options) => modal(`option ${path.join(' ')} of tool ${tool}`, () => play([{ _obj: 'set', _target: [{ _ref: tool }], to: options }]))),
            ),
    },
    pointer: Effect.map(Notifications.use(Struct.get('operations')), (operations) => ({ operations })),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, containment, execute, Notifications, render, suppressDialogs, toolbar };
