import photoshop, { type Document, type ExecutionContext } from 'adobe:photoshop';
import type { File } from 'adobe:uxp';
import { ExecuteScript, HostError, ObjectModel, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { Uxp } from '@rasm/uxp/runtime';
import { parse } from 'acorn';
import { Array, Cause, Effect, Exit, flow, Layer, Match, Option, Path, Predicate, Record, Result, Schema, SchemaTransformation, Scope, String, Struct } from 'effect';
import { Photoshop } from '#/contract';

// --- [CONSTANTS] -----------------------------------------------------------------------

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
} as const satisfies Record<string, keyof typeof photoshop.app | keyof Document>;

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(Struct.keys(COLLECTIONS));
const Name = Schema.String.check(Schema.isPattern(/^(?![\s\S]*"\])/u));
const Selector = Schema.TaggedUnion({ Identified: { id: Schema.Int }, Indexed: { index: Schema.Int }, Named: { name: Name } });
const Written = Schema.Union([
    Schema.TemplateLiteralParser(Reference.Identified.parts).pipe(Schema.decodeTo(Selector.cases.Identified, SchemaTransformation.transform({ decode: ([, id]) => ({ _tag: 'Identified', id }) as const, encode: ({ id }) => [Reference.Identified.parts[0], id, Reference.Identified.parts[2]] as const }))),
    Schema.TemplateLiteralParser(Reference.Indexed.parts).pipe(Schema.decodeTo(Selector.cases.Indexed, SchemaTransformation.transform({ decode: ([, index]) => ({ _tag: 'Indexed', index }) as const, encode: ({ index }) => [Reference.Indexed.parts[0], index, Reference.Indexed.parts[2]] as const }))),
    Schema.TemplateLiteralParser([Reference.Named.parts[0], Name, Reference.Named.parts[2]]).pipe(
        Schema.decodeTo(Selector.cases.Named, SchemaTransformation.transform({ decode: ([, name]) => ({ _tag: 'Named', name }) as const, encode: ({ name }) => [Reference.Named.parts[0], name, Reference.Named.parts[2]] as const })),
    ),
]);
const Step = Schema.TemplateLiteralParser(['/', Schema.String.check(Schema.isPattern(/^\w+$/u)), Written, Schema.String]);
const Identity = Schema.Struct({ id: Schema.Int });
const Titled = Schema.Struct({ name: Schema.String });
const Instance = Schema.declare(Predicate.isObjectKeyword);
const Element = Schema.Struct({
    typename: Schema.String,
    id: Schema.OptionFromOptionalKey(Schema.Int),
    name: Schema.OptionFromOptionalKey(Schema.String),
    postScriptName: Schema.OptionFromOptionalKey(Schema.String),
    document: Schema.OptionFromOptionalKey(Instance),
    parent: Schema.OptionFromOptionalKey(Schema.NullOr(Instance)),
});
const Rendered = Schema.Union([Schema.declare((input: unknown): input is Document => Predicate.hasProperty(input, 'typename') && input.typename === 'Document'), Schema.declare((input: unknown): input is Document['layers'][number] => Predicate.hasProperty(input, 'typename') && input.typename === 'Layer')]);
const Png = Schema.Struct({
    method: Schema.optionalKey(Schema.Enum(photoshop.constants.PNGMethod)),
    compression: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 9 }))),
    interlaced: Schema.optionalKey(Schema.Boolean),
});
const Entry = Uxp.Entry.mapFields(Struct.assign({ isFile: Schema.Literal(true) }));
const Members = Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Finite]));
const Parameter = Schema.Struct({ type: Schema.String, start: Schema.Int, end: Schema.Int, left: Schema.OptionFromOptionalKey(Schema.Struct({ start: Schema.Int, end: Schema.Int })) });
const Declared = Schema.Struct({ body: Schema.Tuple([Schema.Struct({ expression: Schema.Struct({ body: Schema.Struct({ body: Schema.Tuple([Schema.Struct({ value: Schema.Struct({ params: Schema.Array(Parameter) }) })]) }) }) })]) });
const Executable = Schema.Tuple([Schema.Struct({ executablePath: Schema.Struct({ _path: Schema.String }) })]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const elements = (container: object, kind: typeof Kind.Type): readonly object[] => {
    const items: unknown = Reflect.get(container, COLLECTIONS[kind]);
    return Array.isArray(items) ? items.filter(Predicate.isObjectKeyword) : [];
};
const searched =
    (kind: typeof Kind.Type, found: (item: object) => boolean) =>
    (container: object): Option.Option<object> =>
        elements(container, kind).reduce<Option.Option<object>>((held, item) => Option.orElse(held, () => (found(item) ? Option.some(item) : searched(kind, found)(item))), Option.none<object>());
const flattened =
    (kind: typeof Kind.Type) =>
    (container: object): readonly object[] =>
        Array.flatMap(elements(container, kind), (item) => [item, ...flattened(kind)(item)]);
const identified = (id: number) => (item: object) => Option.contains(Option.map(Schema.decodeUnknownOption(Identity)(item), Struct.get('id')), id);
const owner = (container: object): Option.Option<object> => (Array.isArray(container) ? Option.liftPredicate(Reflect.get(container, 'parent'), Predicate.isObjectKeyword) : Option.some(container));
const parameters = (method: { readonly length: number }) => {
    const source = `(class { ${Function.prototype.toString.call(method)} })`;
    return Option.match(Option.flatMap(Option.liftThrowable(parse)(source, { ecmaVersion: 'latest' }), Schema.decodeUnknownOption(Declared)), {
        onNone: () => Array.unfold(0, (at) => (at < method.length ? Option.some([{ name: `arguments[${at}]`, optional: false }, at + 1] as const) : Option.none())),
        onSome: ({
            body: [
                {
                    expression: {
                        body: {
                            body: [{ value }],
                        },
                    },
                },
            ],
        }) => Array.map(value.params, ({ type, start, end, left }) => ({ name: Option.match(left, { onNone: () => source.slice(start, end), onSome: (bound) => source.slice(bound.start, bound.end) }), optional: Option.isSome(left) || type === 'RestElement' })),
    });
};
const declaration = (name: string, { prototype }: { readonly prototype: object }) => {
    const [{ fields, methods }, ...parents] = Uxp.members(prototype);
    return Effect.orDie(
        Schema.decodeEffect(ObjectModel.Result.cases.Class)({
            _tag: 'Class',
            name,
            parents: Array.map(parents, Struct.get('name')),
            fields,
            methods: Array.map(methods, (method) => ({ name: method, parameters: Option.match(Option.liftPredicate(Reflect.get(prototype, method), Predicate.isFunction), { onNone: Array.empty, onSome: parameters }) })),
        }),
    );
};
const cancelled = (context: ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const isolated = (layers: readonly Document['layers'][number][], id: number): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (Option.isSome(searched('layer', identified(id))(layer)) ? isolated(layer.layers ?? [], id) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
        { discard: true },
    );
const rendered = Effect.fnUntraced(function* (longestSide: number, { found, written, options }: { readonly found: typeof Rendered.Type; readonly written: File; readonly options: Option.Option<typeof Png.Type> }, { hostControl }: ExecutionContext) {
    const { document, layer } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, layer: Option.none() }), Layer: (placed) => ({ document: placed.document, layer: Option.some(placed) }) }));
    const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
    yield* Effect.tryPromise({ try: () => hostControl.registerAutoCloseDocument(copy.id), catch: HostError.refused(`registerAutoCloseDocument of document ${copy.id}`) });
    yield* Option.match(layer, {
        onNone: () => Effect.void,
        onSome: ({ id, bounds }) => Effect.andThen(isolated(copy.layers, id), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })),
    });
    const [width, height] = [copy.width, copy.height];
    const scale = Math.min(1, longestSide / Math.max(width, height));
    const [scaledWidth, scaledHeight] = [width * scale, height * scale];
    yield* scale === 1 ? Effect.void : Effect.tryPromise({ try: () => copy.resizeImage(scaledWidth, scaledHeight), catch: HostError.refused(`resizeImage to ${scaledWidth}x${scaledHeight}`) });
    yield* Effect.tryPromise({ try: () => copy.saveAs.png(written, Option.getOrUndefined(options), true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { app, core, constants, action } = photoshop;
const classes = Record.filter(Object.fromEntries(Object.entries(app)), Predicate.isFunction);
const scope = { ...photoshop, ...classes, ...constants };
const font = (name: string) => Effect.fromOption(Option.fromNullOr(app.fonts.getByName(name)), () => Unmatched.make({ subject: 'textFont', query: Option.some(name), candidates: Array.map(app.fonts, Struct.get('postScriptName')) }));
const located = (container: object, kind: typeof Kind.Type, selector: typeof Selector.Type): Effect.Effect<object, Unmatched> =>
    Selector.match(selector, {
        Identified: ({ id }) => Effect.fromOption(searched(kind, identified(id))(container), () => Unmatched.make({ subject: `${kind} id`, query: Option.some(`${id}`), candidates: Array.filterMap(flattened(kind)(container), (item) => (Schema.is(Identity)(item) ? Result.succeed(`${item.id}`) : Result.failVoid)) })),
        Indexed: ({ index }) => {
            const listed = elements(container, kind);
            return Effect.fromOption(Array.get(listed, index), () => Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(listed) ? `${kind} among indexes 0 to ${listed.length - 1}` : kind, query: Option.some(`${index}`) }));
        },
        Named: ({ name }) =>
            kind === 'textFont'
                ? font(name)
                : Effect.map(
                      Unmatched.single(Array.getSomes(Array.map(elements(container, kind), (item) => Option.map(Schema.decodeUnknownOption(Titled)(item), ({ name: title }) => ({ item, title })))), kind, Option.some(name), ({ title }) => [title]),
                      Struct.get('item'),
                  ),
    });
const walked = (container: object, path: string): Effect.Effect<object, Unmatched | Schema.SchemaError> =>
    path === ''
        ? Effect.succeed(container)
        : Effect.gen(function* () {
              const [, word, selector, rest] = yield* Schema.decodeUnknownEffect(Step)(path);
              const kind = yield* Unmatched.single(Struct.keys(COLLECTIONS), 'collection kind', Option.some(word), Array.of);
              return yield* walked(yield* located(container, kind, selector), rest);
          });
const resolve = (specifier: string) => walked(app, specifier);
const traced = (value: object): Option.Option<{ readonly element: typeof Element.Type; readonly path: readonly (readonly [typeof Kind.Type, typeof Selector.Type])[] }> =>
    Option.gen(function* () {
        const element = yield* Schema.decodeUnknownOption(Element)(value);
        const kind = yield* Schema.decodeUnknownOption(Kind)(String.uncapitalize(element.typename));
        const rooted = COLLECTIONS[kind] in app;
        const container: object = rooted
            ? app
            : yield* Option.flatMap(
                  Option.orElse(element.document, () => Option.flatMap(element.parent, Option.fromNullOr)),
                  owner,
              );
        const above = rooted ? [] : (yield* traced(container)).path;
        const selector = yield* Option.map(element.id, (id) => Selector.cases.Identified.make({ id })).pipe(
            Option.orElse(() =>
                Option.map(
                    Option.filter(
                        Option.orElse(element.postScriptName, () => element.name),
                        Schema.is(Name),
                    ),
                    (name) => Selector.cases.Named.make({ name }),
                ),
            ),
            Option.orElse(() =>
                Option.map(
                    Array.findFirstIndex(elements(container, kind), (each) => each === value),
                    (index) => Selector.cases.Indexed.make({ index }),
                ),
            ),
        );
        return { element, path: [...above, [kind, selector] as const] };
    });
const reference = (value: object): Option.Option<Reference> => Option.map(traced(value), ({ element, path }) => Reference.make({ type: element.typename, specifier: path.reduceRight((rest, [kind, selector]) => Schema.encodeSync(Step)(['/', kind, selector, rest]), ''), name: element.name, valid: true }));
const encoded = (value: unknown): unknown => {
    if (Array.isArray(value)) {
        return Array.map(value, encoded);
    }
    if (Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype) {
        return Record.map(value, encoded);
    }
    return Predicate.isObjectKeyword(value) ? Option.getOrElse(Option.map(reference(value), Schema.encodeSync(Reference)), () => value) : value;
};
const modal = <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) =>
    Effect.flatMap(Effect.context<R>(), (services) => Effect.flatten(Effect.tryPromise({ try: (signal) => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }), catch: HostError.refused(commandName) })));
const stored = (url: URL, opened: (files: Uxp['Service']['uxp']['storage']['localFileSystem'], href: string) => Promise<unknown>) =>
    Effect.flatMap(Uxp, ({ uxp: { storage } }) => Effect.tryPromise({ try: () => opened(storage.localFileSystem, url.href), catch: HostError.refused(url.href) })).pipe(
        Effect.filterOrFail(
            (found): found is File => Schema.is(Entry)(found),
            () => new Cause.IllegalArgumentError(`${url.href} names no file`),
        ),
    );
const executed = (payload: ExecuteScript, { hostControl }: ExecutionContext) =>
    app.documents.filter(Predicate.isObjectKeyword).reduce(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: payload.label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }),
            ),
        Effect.flatten(Uxp.evaluate({ ...scope, resolve: flow(resolve, Effect.runSync) }, payload.code)).pipe(
            Effect.flatMap((value) => Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: Uxp.thrown })),
            Effect.flatMap((result) => Schema.decodeUnknownEffect(ExecuteScript.Result)(Predicate.isUndefined(result) ? {} : { result: encoded(result) })),
        ),
    );
const model = (payload: ObjectModel) =>
    payload.describe(Record.keys(classes), Record.keys(constants), (match) =>
        Option.match(Record.get(classes, match), {
            onNone: () => Effect.map(Schema.decodeUnknownEffect(Members)(Reflect.get(constants, match)), (members) => ObjectModel.Result.cases.Enumeration.make({ name: match, members: Record.collect(members, (member, value) => ({ name: member, description: Option.some(`${value}`) })) })),
            onSome: (type) => declaration(match, type),
        }),
    );
const server = Layer.merge(
    Photoshop.Contract.toLayer({
        execute_script: (payload) => modal(payload.label, (context) => executed(payload, context)),
        open: ({ file, label }) =>
            stored(file, (files, href) => files.getEntryWithUrl(href)).pipe(
                Effect.flatMap((found) => modal(label, () => Effect.tryPromise({ try: () => app.open(found), catch: HostError.refused(label) }))),
                Effect.flatMap((document) => Effect.fromOption(reference(document), () => new Cause.IllegalArgumentError(`${label} opened no document`))),
            ),
        render: ({ target, longestSide, options, file, label }) =>
            Effect.all({
                options: Effect.transposeOption(Option.map(options, Schema.decodeUnknownEffect(Png, { onExcessProperty: 'error' }))),
                found: resolve(target).pipe(Effect.filterOrFail(Schema.is(Rendered), () => new Cause.IllegalArgumentError(`${target} names no document, layer, or artboard`))),
                written: stored(file, (files, href) => files.createEntryWithUrl(href, { overwrite: true })),
            }).pipe(Effect.flatMap((prepared) => modal(label, (context) => rendered(longestSide, prepared, context)))),
        object_model: model,
    }),
    Layer.succeed(Photoshop.DisplayDialogs)((effect) => Effect.scopedWith((held) => Effect.andThen(Scope.provide(Uxp.preserve(app, { displayDialogs: constants.DialogModes.NONE }), held), effect))),
);
const linked = Link.dial(Photoshop.Contract, server, {
    application: Effect.tryPromise({ try: () => action.batchPlay([{ _obj: 'get', _target: [{ _property: 'executablePath' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {}), catch: HostError.refused('executablePath') }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Executable)),
        Effect.zip(Path.Path),
        Effect.flatMap(([[{ executablePath }], path]) => path.toFileUrl(executablePath._path)),
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { linked };
