import type { Value } from '@rasm/applications';
import { Description } from '@rasm/applications/source';
import type { Upstream } from '@rasm/mcp';
import { HostError, type Render } from '@rasm/object-model';
import { type Containment, format, read } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import { action, app, core } from 'adobe:photoshop';
import type { File } from 'adobe:uxp';
import { Array, Cause, Context, Effect, Exit, Formatter, flow, Layer, Match, Option, Predicate, Queue, Record, Result, Schema, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import type { ActionDescriptor, ExecutionContext } from 'photoshop/dom/CoreModules';
import type { Document } from 'photoshop/dom/Document';
import type { Tool } from 'photoshop/dom/objects/Tool';

// --- [TYPES] ---------------------------------------------------------------------------

type Path = typeof Description.Locator.Type;
type Event = Stream.Success<Upstream['events']>;
type Leaf = Pick<typeof Description.Control.Type, 'kind' | 'role' | 'value' | 'selected'> & { readonly write: (to: unknown) => Option.Option<unknown> };

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
const TOOL = 'currentTool' satisfies keyof typeof app;
const ID = 'id' satisfies keyof Tool;
const OPTIONS = 'currentToolOptions';

// --- [MODELS] --------------------------------------------------------------------------

const Typed = Schema.Struct({ typename: Schema.String });
const Exportable = Schema.declare((input: unknown): input is Document | Document['layers'][number] => Predicate.hasProperty(input, 'typename') && (input.typename === 'Document' || input.typename === 'Layer'));
const Stored = Schema.declare((input: unknown): input is globalThis.File => Schema.is(Uxp.Entry)(input)).annotate({ description: 'UXP storage file, the DOM File type @types/photoshop names for file parameters' });
const Closing = Schema.Struct({ registerAutoCloseDocument: Schema.declare((input: unknown): input is (documentID: number) => Promise<void> => Predicate.isFunction(input)) }).annotate({ description: 'hostControl member Photoshop holds and @types/photoshop omits' });
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });
const Refusal = Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Int }).annotate({ description: 'Error descriptor a refused command replies with' });
const Hosted = Schema.Tuple([Schema.Struct({ hostName: Schema.NonEmptyString.annotate({ description: 'Application name, the bundle name of the installation' }) })]);
const ActionObject = Schema.StructWithRest(Schema.Struct({ _obj: Schema.String }), [Schema.Record(Schema.String, Schema.Unknown)]).annotate({ description: 'Descriptor of an object, its keys the string or character IDs Photoshop names them by' });
const ToolOptions = Schema.Tuple([Schema.Struct({ currentToolOptions: ActionObject })]).annotate({ description: 'Reply to a get of the options of the current tool' });
const DocumentEvent = Schema.Struct({ documentID: Schema.Int }).annotate({ description: 'Descriptor of a document event' });
const Selected = Schema.Struct({ _target: Schema.Tuple([Schema.Struct({ _ref: Schema.String })]) }).annotate({ description: 'Descriptor of a select, a tool switch where its reference names the current tool' });
const Enumerated = Schema.Struct({ _enum: Schema.String, _value: Schema.String }).annotate({ description: 'Enumeration value with its type' });
const Unit = Schema.Struct({ _unit: Schema.String, _value: Schema.Finite }).annotate({ description: 'Number with its unit' });
const ModalState = Schema.Struct({
    level: Schema.Int.annotate({ description: 'Depth of the modal state, 0 once it exits' }),
    state: Enumerated,
    kind: Schema.optionalKey(Enumerated.annotate({ description: 'Kind of a tool modal state, as toolModalStateChanged names it' })),
    title: Schema.optionalKey(Schema.String.annotate({ description: 'Title of a modal dialog, as modalStateChanged names it' })),
}).annotate({ description: 'Descriptor of toolModalStateChanged or modalStateChanged' });

// --- [SERVICES] ------------------------------------------------------------------------

class Notifications extends Context.Service<Notifications, { readonly events: Stream.Stream<Event, HostError>; readonly operations: Effect.Effect<readonly string[]> }>()('@rasm/photoshop/uxp/Notifications') {
    static readonly layer = Layer.effect(Notifications)(
        Effect.gen(function* () {
            const entered = yield* SubscriptionRef.make<Record.ReadonlyRecord<string, string>>({});
            const moded = (name: string, descriptor: unknown) =>
                Option.match(
                    Option.flatMap(Schema.decodeUnknownOption(ModalState)(descriptor), ({ level, state: { _value: state }, kind, title }) => Option.map(Option.fromUndefinedOr(UndefinedOr.map(kind, Struct.get('_value')) ?? title), (operation) => ({ level, state, operation }))),
                    {
                        onNone: () => Effect.succeed(Result.failVoid),
                        onSome: (detail) => Effect.as(SubscriptionRef.update(entered, detail.level > 0 ? Record.set(name, detail.operation) : Record.remove(name)), Result.succeed<Event>({ name, target: Option.none(), detail: Option.some(detail) })),
                    },
                );
            const handlers: Record.ReadonlyRecord<string, (name: string, descriptor: unknown) => Effect.Effect<Result.Result<Event, void>>> = { open: documented, save: documented, close: documented, select: selected, toolModalStateChanged: moded, modalStateChanged: moded };
            const names = Record.keys(handlers);
            const heard = Stream.callback<readonly [string, unknown], HostError>((queue) => {
                const listener = (name: string, descriptor: unknown): void => {
                    Queue.offerUnsafe(queue, [name, descriptor]);
                };
                return Effect.acquireRelease(Effect.tryPromise({ try: () => Promise.resolve(action.addNotificationListener(names, listener)), catch: HostError.refused(`addNotificationListener of ${names.join(', ')}`) }), () =>
                    Effect.orDie(Effect.tryPromise({ try: () => Promise.resolve(action.removeNotificationListener(names, listener)), catch: HostError.refused(`removeNotificationListener of ${names.join(', ')}`) })),
                );
            });
            const events = yield* heard.pipe(
                Stream.filterMapEffect(([name, descriptor]) => Option.match(Record.get(handlers, name), { onNone: () => Effect.succeed(Result.failVoid), onSome: (handle) => handle(name, descriptor) })),
                Stream.share({ capacity: 'unbounded' }),
            );
            yield* Effect.forkScoped(Effect.tapCause(Stream.runDrain(events), Effect.logError));
            return { events, operations: Effect.map(SubscriptionRef.get(entered), Record.values) };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const containment: Containment = {
    root: app,
    type: (value) => Option.match(Schema.decodeUnknownOption(Typed)(value), { onNone: () => value.constructor.name, onSome: Struct.get('typename') }),
    kinds: Record.map(COLLECTIONS, (collection: string, kind) => ({
        collection,
        nested: true,
        keys: ['id', 'postScriptName', 'name'] satisfies (keyof Document | keyof (typeof app.fonts)[number])[],
        is: (value: object) => Option.exists(Schema.decodeUnknownOption(Typed)(value), ({ typename }) => String.uncapitalize(typename) === kind),
        ...(collection in app ? {} : { owner: (value: object) => UndefinedOr.map(read(value, 'document') ?? read(value, 'parent'), (above) => (Array.isArray(above) ? read(above, 'parent') : above)) }),
    })),
};
const lineage = (layer: Document['layers'][number] | null): readonly number[] => (layer === null ? [] : [layer.id, ...lineage(layer.parent)]);
const isolated = (layers: readonly Document['layers'][number][], id: number, above: readonly number[]): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (above.includes(layer.id) ? isolated(layer.layers ?? [], id, above) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
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
const played = (commands: ActionDescriptor[]) =>
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
                (suspension, exit) => (app.documents.some(({ id: openId }) => openId === id) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        body,
    );
const execute = (code: string, label: string) => modal(label, (context) => history(label, context, Uxp.execute(containment, code)));
const render = Effect.fnUntraced(function* ({ target, label }: Render, written: File) {
    const [found, stored] = yield* Effect.all([Uxp.locate(containment, target).pipe(Effect.filterOrFail(Schema.is(Exportable), () => Unmatched.of({ subject: 'document, layer, or artboard', query: Option.some(target.path) }))), Effect.orDie(Schema.decodeUnknownEffect(Stored)(written))]);
    return yield* modal(
        label,
        Effect.fnUntraced(function* ({ hostControl }) {
            const { document, placed } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, placed: [] }), Layer: (layer) => ({ document: layer.document, placed: [layer] }) }));
            const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
            const refused = HostError.refused(`registerAutoCloseDocument of document ${copy.id}`);
            const closing = yield* Effect.filterOrFail(Effect.succeed(hostControl), Schema.is(Closing), refused);
            yield* Effect.tryPromise({ try: () => closing.registerAutoCloseDocument(copy.id), catch: refused });
            yield* Effect.forEach(placed, ({ id, bounds, parent }) => Effect.andThen(isolated(copy.layers, id, lineage(parent)), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
            yield* Effect.tryPromise({ try: () => copy.saveAs.png(stored, undefined, true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
        }),
    );
});
const property = <S extends Schema.Top>(name: string, reply: S) => played([{ _obj: 'get', _target: [{ _property: name }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }]).pipe(Effect.flatMap(Schema.decodeUnknownEffect(reply)));
const application = Effect.map(property('hostName', Hosted), ([{ hostName }]) => hostName);
const options = property(OPTIONS, ToolOptions);
const documented = (name: string, descriptor: unknown) =>
    Effect.succeed(Result.succeed<Event>({ name, target: Option.map(Schema.decodeUnknownOption(DocumentEvent)(descriptor), ({ documentID }) => format({ steps: [{ separator: '/', kind: 'document', selector: { _tag: 'Key', key: 'id', value: documentID } }] })), detail: Option.none() }));
const selected = (name: string, descriptor: unknown) =>
    Effect.sync(() =>
        Option.match(
            Option.filter(Schema.decodeUnknownOption(Selected)(descriptor), ({ _target: [{ _ref: reference }] }) => reference === app.currentTool.id),
            { onNone: () => Result.failVoid, onSome: ({ _target: [{ _ref: currentTool }] }) => Result.succeed<Event>({ name, target: Option.none(), detail: Option.some({ currentTool }) }) },
        ),
    );
const same = Schema.toEquivalence(Description.Locator);
const leaf = Match.type<unknown>().pipe(
    Match.withReturnType<Leaf>(),
    Match.when(Match.boolean, (value) => ({ kind: 'toggle', role: typeof value, value: { bool: { value } }, write: Option.liftPredicate(Predicate.isBoolean) })),
    Match.when(Match.number, (value) => ({ kind: 'number', role: typeof value, value: { real: { value } }, write: Option.liftPredicate(Predicate.isNumber) })),
    Match.when(Match.string, (value) => ({ kind: 'text', role: typeof value, value: { text: { value } }, write: Option.liftPredicate(Predicate.isString) })),
    Match.when(Schema.is(Unit), ({ _unit: role, _value: value }) => ({
        kind: 'measure',
        role,
        value: { real: { value } },
        write: flow(
            Option.liftPredicate(Predicate.isNumber),
            Option.map((number) => ({ _unit: role, _value: number })),
        ),
    })),
    Match.when(Schema.is(Enumerated), ({ _enum: role, _value: value }) => ({
        kind: 'choice',
        role,
        value: { text: { value } },
        selected: 'attribute',
        write: flow(
            Option.liftPredicate(Predicate.isString),
            Option.map((text) => ({ _enum: role, _value: text })),
        ),
    })),
    Match.option,
);
const controls = ({ _obj: owner, ...held }: typeof ActionObject.Type, path: Path): readonly (typeof Description.Control.Type)[] =>
    Array.flatMap(Record.toEntries(held), ([key, value]) => {
        const locator = Array.append(path, [owner, key] as const);
        return Option.match(leaf(value), {
            onSome: (stated) => [Description.Control.make({ ...Struct.omit(stated, ['write']), locator, label: { text: key, attribute: owner } })],
            onNone: () => (Schema.is(ActionObject)(value) ? controls(value, locator) : []),
        });
    });
const written = ({ _obj: owner, ...held }: typeof ActionObject.Type, [key, ...below]: Array.NonEmptyReadonlyArray<string>, to: unknown): Result.Result<unknown, Unmatched | Cause.IllegalArgumentError> => {
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
        onNonEmpty: (deeper) => (Schema.is(ActionObject)(value) ? Result.map(written(value, deeper, to), (inner) => ({ ...value, [Array.headNonEmpty(deeper)]: inner })) : unmatched),
    });
};
const describe = Effect.fn('describe')(function* ({ within }: { readonly within?: Path }) {
    const toolbar: Path = [[app.typename, TOOL]];
    yield* within === undefined || same(within, toolbar) ? Effect.void : Unmatched.of({ subject: 'facet item', query: Option.some(Formatter.format(within)), candidates: [Formatter.format(toolbar)] });
    const refused = HostError.refused(TOOL);
    const [operations, [{ currentToolOptions }], { title }, tool] = yield* Effect.all([Notifications.use(Struct.get('operations')), options, Effect.tryPromise({ try: () => core.getActiveTool(), catch: refused }), Effect.try({ try: () => app.currentTool, catch: refused })]);
    return Description.make({
        containers: [
            {
                locator: toolbar,
                kind: 'toolbar',
                title,
                controls: [
                    Description.Control.make({ locator: Array.append(toolbar, [tool.typename, ID] as const), kind: 'choice', role: tool.typename, label: { text: TOOL, attribute: ID }, value: { text: { value: tool.id } }, selected: 'attribute' }),
                    ...controls(currentToolOptions, Array.append(toolbar, [tool.typename, tool.id] as const)),
                ],
            },
        ],
        pointer: { tool: tool.id, operations },
    });
});
const operate = Effect.fn('operate')(function* ({ locator, to }: { readonly locator: Path; readonly to?: typeof Value.Type }) {
    const text = locator.map((step) => step.join(' ')).join(' > ');
    const tool = yield* Effect.try({ try: () => app.currentTool.id, catch: HostError.refused(TOOL) });
    const [field, member, ...path] = Array.map(locator, ([, identifier]) => identifier);
    yield* field === TOOL ? Effect.void : Unmatched.of({ subject: 'facet control', query: Option.some(text) });
    if (Array.isReadonlyArrayNonEmpty(path)) {
        yield* member === tool ? Effect.void : Unmatched.of({ subject: 'option of the current tool', query: Option.some(text), candidates: [tool] });
        const [{ currentToolOptions }] = yield* options;
        const value = yield* Effect.fromResult(written(currentToolOptions, path, to));
        yield* modal(text, () => played([{ _obj: 'set', _target: [{ _ref: tool }], to: { _obj: OPTIONS, [Array.headNonEmpty(path)]: value } }]));
        return;
    }
    yield* member === ID ? Effect.void : Unmatched.of({ subject: 'facet control', query: Option.some(text) });
    const chosen = yield* Effect.filterOrFail(Effect.succeed(to), Predicate.isString, () => new Cause.IllegalArgumentError(`${text} takes the id of a tool, received ${typeof to}`));
    yield* modal(text, () => played([{ _obj: 'select', _target: [{ _ref: chosen }] }]));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, containment, describe, execute, Notifications, operate, render };
