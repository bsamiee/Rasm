import photoshop, { action, app, constants, core, type Document, type ExecutionContext } from 'adobe:photoshop';
import { type File, host } from 'adobe:uxp';
import { ExecuteScript, Photoshop } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { HostError, ObjectModel, Reference } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import { parse } from 'acorn';
import { Array, Cause, Effect, Exit, flow, Layer, Match, Option, Path, Predicate, Record, Schema, String, Struct } from 'effect';

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
const classes = Record.filter({ ...app }, Predicate.isFunction);
const scope = { ...photoshop, ...classes, ...constants };

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(Struct.keys(COLLECTIONS));
const Step = Schema.TemplateLiteralParser(['/', Schema.String.check(Schema.isPattern(/^\w+$/u)), Reference.Selector, Schema.String]);
const Identity = Schema.Struct({ id: Schema.Int });
const Titled = Schema.Struct({ name: Schema.String });
const Specifiable = Schema.Struct({
    typename: Schema.String,
    id: Schema.OptionFromOptionalKey(Schema.Int),
    name: Schema.OptionFromOptionalKey(Schema.String),
    postScriptName: Schema.OptionFromOptionalKey(Schema.String),
    document: Schema.OptionFromOptionalKey(Schema.ObjectKeyword),
    parent: Schema.OptionFromOptionalNullOr(Schema.ObjectKeyword),
});
const Renderable = Schema.declare((input: unknown): input is Document | Document['layers'][number] => Predicate.hasProperty(input, 'typename') && (input.typename === 'Document' || input.typename === 'Layer'));
const Png = Schema.Struct({
    method: Schema.optionalKey(Schema.Enum(constants.PNGMethod)),
    compression: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 9 }))),
    interlaced: Schema.optionalKey(Schema.Boolean),
});
const Entry = Uxp.Entry.mapFields(Struct.assign({ isFile: Schema.Literal(true) }));
const Members = Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Finite]));
const Parameter = Schema.Struct({ type: Schema.String, start: Schema.Int, end: Schema.Int, left: Schema.OptionFromOptionalKey(Schema.Struct({ start: Schema.Int, end: Schema.Int })) });
const Declared = Schema.Struct({ body: Schema.Tuple([Schema.Struct({ expression: Schema.Struct({ body: Schema.Struct({ body: Schema.Tuple([Schema.Struct({ value: Schema.Struct({ params: Schema.Array(Parameter) }) })]) }) }) })]) });
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });
const Executable = Schema.Tuple([Schema.Union([Schema.Struct({ executablePath: Schema.Struct({ _path: Schema.NonEmptyString }) }), Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Int })])]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const elements = (container: object, kind: typeof Kind.Type): readonly object[] => {
    const items: unknown = Reflect.get(container, COLLECTIONS[kind]);
    return Array.isArray(items) ? items.filter(Predicate.isObjectKeyword) : [];
};
const flattened = (container: object, kind: typeof Kind.Type): readonly object[] => Array.flatMap(elements(container, kind), (item) => [item, ...flattened(item, kind)]);
const owner = (container: object): Option.Option<object> => (Array.isArray(container) ? Option.liftPredicate(Reflect.get(container, 'parent'), Predicate.isObjectKeyword) : Option.some(container));
const parameters = (method: { readonly length: number }) => {
    const source = `(class { ${Function.prototype.toString.call(method)} })`;
    return Option.liftThrowable(parse)(source, { ecmaVersion: 'latest' }).pipe(
        Option.flatMap(Schema.decodeUnknownOption(Declared)),
        Option.match({
            onNone: () => Array.makeBy(method.length, (at) => ({ name: `arguments[${at}]`, optional: false })),
            onSome: ({ body }) => Array.map(body[0].expression.body.body[0].value.params, ({ type, start, end, left }) => ({ name: Option.match(left, { onNone: () => source.slice(start, end), onSome: (bound) => source.slice(bound.start, bound.end) }), optional: Option.isSome(left) || type === 'RestElement' })),
        }),
    );
};
const declaration = (name: string, { prototype }: { readonly prototype: object }) => {
    const [{ fields, methods }, ...parents] = Uxp.members(prototype);
    return Effect.orDie(Schema.decodeEffect(ObjectModel.Result.cases.Class)({ _tag: 'Class', name, parents: Array.map(parents, Struct.get('name')), fields, methods: Array.map(methods, (method) => ({ name: method, parameters: parameters(Reflect.get(prototype, method)) })) }));
};
const cancelled = (context: ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const isolated = (layers: readonly Document['layers'][number][], id: number): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (flattened(layer, 'layer').some((each) => Schema.is(Identity)(each) && each.id === id) ? isolated(layer.layers ?? [], id) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
        { discard: true },
    );
const rendered = Effect.fnUntraced(function* (longestSide: number, { found, written, options }: { readonly found: typeof Renderable.Type; readonly written: File; readonly options: Option.Option<typeof Png.Type> }, { hostControl }: ExecutionContext) {
    const { document, placed } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, placed: [] }), Layer: (layer) => ({ document: layer.document, placed: [layer] }) }));
    const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
    yield* Effect.tryPromise({ try: () => hostControl.registerAutoCloseDocument(copy.id), catch: HostError.refused(`registerAutoCloseDocument of document ${copy.id}`) });
    yield* Effect.forEach(placed, ({ id, bounds }) => Effect.andThen(isolated(copy.layers, id), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
    const scale = Math.min(1, longestSide / Math.max(copy.width, copy.height));
    const [width, height] = [copy.width * scale, copy.height * scale];
    yield* scale === 1 ? Effect.void : Effect.tryPromise({ try: () => copy.resizeImage(width, height), catch: HostError.refused(`resizeImage to ${width}x${height}`) });
    yield* Effect.tryPromise({ try: () => copy.saveAs.png(written, Option.getOrUndefined(options), true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
});
const located = (container: object, kind: typeof Kind.Type, selector: typeof Reference.Selector.Type): Effect.Effect<object, Unmatched> =>
    Reference.Selector.match(selector, {
        Identified: ({ id }) => Unmatched.single(flattened(container, kind).filter(Schema.is(Identity)), `${kind} id`, Option.some(`${id}`), (item) => [`${item.id}`]),
        Indexed: ({ index }) => {
            const listed = elements(container, kind);
            return Effect.fromOption(Array.get(listed, index), () => Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(listed) ? `${kind} among indexes 0 to ${listed.length - 1}` : kind, query: Option.some(`${index}`) }));
        },
        Named: ({ name }) =>
            kind === 'textFont'
                ? Effect.fromOption(Option.fromNullOr(app.fonts.getByName(name)), () => Unmatched.make({ subject: kind, query: Option.some(name), candidates: Array.map(app.fonts, Struct.get('postScriptName')) }))
                : Unmatched.single(elements(container, kind).filter(Schema.is(Titled)), kind, Option.some(name), ({ name: title }) => [title]),
    });
const resolve = (path: string, container: object = app): Effect.Effect<object, Unmatched | Schema.SchemaError> =>
    path === ''
        ? Effect.succeed(container)
        : Effect.gen(function* () {
              const [, word, selector, rest] = yield* Schema.decodeUnknownEffect(Step)(path);
              const kind = yield* Unmatched.single(Struct.keys(COLLECTIONS), 'collection kind', Option.some(word), Array.of);
              return yield* resolve(rest, yield* located(container, kind, selector));
          });
const reference = Effect.fnUntraced(function* (value: object): Effect.fn.Return<Reference, Cause.NoSuchElementError> {
    const element = yield* Effect.fromOption(Schema.decodeUnknownOption(Specifiable)(value));
    const kind = yield* Effect.fromOption(Schema.decodeUnknownOption(Kind)(String.uncapitalize(element.typename)));
    const rooted = COLLECTIONS[kind] in app;
    const container: object = rooted ? app : yield* Effect.fromOption(Option.flatMap(Option.firstSomeOf([element.document, element.parent]), owner));
    const above = rooted ? Option.none() : Option.some((yield* reference(container)).specifier);
    const selector = yield* Effect.fromOption(
        Option.firstSomeOf([
            Option.map(element.id, (id) => Reference.Selector.cases.Identified.make({ id })),
            Option.flatMap(Option.firstSomeOf([element.postScriptName, element.name]), (name) => Reference.Selector.cases.Named.makeOption({ name })),
            Option.map(
                Array.findFirstIndex(elements(container, kind), (each) => each === value),
                (index) => Reference.Selector.cases.Indexed.make({ index }),
            ),
        ]),
    );
    const step = yield* Effect.orDie(Schema.encodeEffect(Reference.Selector)(selector));
    return Reference.make({ type: element.typename, specifier: Option.match(above, { onNone: () => `/${kind}${step}`, onSome: (parent) => `${parent}/${kind}${step}` }), name: element.name, valid: true });
});
const encoded = (value: unknown): Effect.Effect<unknown> =>
    Match.value(value).pipe(
        Match.when(Array.isArray, Effect.forEach(encoded)),
        Match.when(
            (input: unknown): input is Readonly<Record<string, unknown>> => Predicate.isObject(input) && Object.getPrototypeOf(input) === Object.prototype,
            (fields) => Effect.all(Record.map(fields, encoded)),
        ),
        Match.when(Predicate.isObjectKeyword, (object) =>
            reference(object).pipe(
                Effect.flatMap(flow(Schema.encodeEffect(Reference), Effect.orDie)),
                Effect.catchTag('NoSuchElementError', () => Effect.succeed(object)),
            ),
        ),
        Match.orElse(Effect.succeed),
    );
const modal = Effect.fnUntraced(function* <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) {
    const services = yield* Effect.context<R>();
    return yield* yield* Effect.tryPromise({
        try: (signal) => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }),
        catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.make({ reason: { _tag }, subject: commandName, cause }) }),
    });
});
const stored = (url: URL, opened: (files: Uxp['Service']['uxp']['storage']['localFileSystem'], href: string) => Promise<unknown>) =>
    Effect.flatMap(Uxp, ({ uxp: { storage } }) => Effect.tryPromise({ try: () => opened(storage.localFileSystem, url.href), catch: HostError.refused(url.href) })).pipe(
        Effect.filterOrFail(
            (entry): entry is File => Schema.is(Entry)(entry),
            () => new Cause.IllegalArgumentError(`${url.href} names no file`),
        ),
    );
const executed = (payload: ExecuteScript, { hostControl }: ExecutionContext) =>
    app.documents.reduce(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: payload.label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => (app.documents.some(({ id: openId }) => openId === id) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        Effect.gen(function* () {
            const services = yield* Effect.context<never>();
            const value = yield* yield* Uxp.evaluate({ ...scope, resolve: flow(resolve, Effect.runSyncWith(services)) }, payload.code);
            const result = yield* Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: Uxp.thrown });
            return yield* Schema.decodeUnknownEffect(ExecuteScript.Result)(Predicate.isUndefined(result) ? {} : { result: yield* encoded(result) });
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const handlers = Layer.merge(
    Photoshop.Contract.toLayer({
        execute_script: (payload) => modal(payload.label, (context) => executed(payload, context)),
        open: ({ file, label }) =>
            stored(file, (files, href) => files.getEntryWithUrl(href)).pipe(
                Effect.flatMap((entry) => modal(label, () => Effect.tryPromise({ try: () => app.open(entry), catch: HostError.refused(label) }))),
                Effect.flatMap(reference),
                Effect.catchTag('NoSuchElementError', () => new Cause.IllegalArgumentError(`${label} opened no document`)),
            ),
        render: ({ target, longestSide, options, file, label }) =>
            Effect.all({
                options: Effect.transposeOption(Option.map(options, Schema.decodeUnknownEffect(Png, { onExcessProperty: 'error' }))),
                found: resolve(target).pipe(Effect.filterOrFail(Schema.is(Renderable), () => new Cause.IllegalArgumentError(`${target} names no document, layer, or artboard`))),
                written: stored(file, (files, href) => files.createEntryWithUrl(href, { overwrite: true })),
            }).pipe(Effect.flatMap((prepared) => modal(label, (context) => rendered(longestSide, prepared, context)))),
        object_model: (payload) =>
            payload.describe(Record.keys(classes), Record.keys(constants), (name) =>
                Option.match(Record.get(classes, name), {
                    onNone: () =>
                        Effect.map(
                            Schema.decodeUnknownEffect(Members)(Reflect.get(constants, name)),
                            flow(
                                Record.collect((member, value) => ({ name: member, description: Option.some(`${value}`) })),
                                (members) => ObjectModel.Result.cases.Enumeration.make({ name, members }),
                            ),
                        ),
                    onSome: (type) => declaration(name, type),
                }),
            ),
    }),
    Layer.succeed(Photoshop.DisplayDialogs)((effect) => Effect.scoped(Effect.andThen(Uxp.preserve(app, { displayDialogs: constants.DialogModes.NONE }), effect))),
);
const link = Peer.dial(
    Photoshop.Contract,
    handlers,
    Effect.gen(function* () {
        const refused = HostError.refused(`Photoshop ${host.version} executable path`);
        const descriptors = yield* Effect.tryPromise({ try: () => action.batchPlay([{ _obj: 'get', _target: [{ _property: 'executablePath' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {}), catch: refused });
        const [reply] = yield* Schema.decodeUnknownEffect(Executable)(descriptors).pipe(Effect.mapError((cause) => refused({ cause, fields: descriptors.map(Struct.keys) })));
        if ('message' in reply) {
            return yield* refused(reply);
        }
        const path = yield* Path.Path;
        return yield* path.toFileUrl(reply.executablePath._path);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { link };
