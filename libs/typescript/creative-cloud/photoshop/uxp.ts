import { Declaration, HostError, Reference, Specifier } from '@rasm/object-model';
import { type Containment, encode, read, reference, walk } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import * as photoshop from 'adobe:photoshop';
import { type File, host } from 'adobe:uxp';
import { Array, Cause, Effect, Exit, flow, Match, Option, Predicate, Record, Schema, String, Struct } from 'effect';

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
} as const satisfies Record<string, keyof typeof photoshop.app | keyof photoshop.Document>;
const owned = (value: object) => Record.fromIterableWith(Object.getOwnPropertyNames(value), (key): readonly [string, unknown] => [key, Reflect.get(value, key)]);
const [modules, classes, enumerations] = [owned(photoshop), Record.filter(owned(photoshop.app), Predicate.isFunction), owned(photoshop.constants)];
const Typed = Schema.Struct({ typename: Schema.String });
const containment: Containment = {
    root: photoshop.app,
    type: (value) => Option.match(Schema.decodeUnknownOption(Typed)(value), { onNone: () => value.constructor.name, onSome: Struct.get('typename') }),
    kinds: Record.map(COLLECTIONS, (collection: string, kind) => ({
        collection,
        nested: kind === 'layer',
        ...(kind === 'textFont' ? { keys: ['postScriptName'] } : {}),
        is: (value: object) => Option.exists(Schema.decodeUnknownOption(Typed)(value), ({ typename }) => String.uncapitalize(typename) === kind),
        ...(collection in photoshop.app
            ? {}
            : {
                  owner: (value: object) => {
                      const above = read(value, 'document') ?? read(value, 'parent');
                      return Array.isArray(above) ? read(above, 'parent') : above;
                  },
              }),
    })),
};

// --- [MODELS] --------------------------------------------------------------------------

const Exportable = Schema.declare((input: unknown): input is photoshop.Document | photoshop.Document['layers'][number] => Predicate.hasProperty(input, 'typename') && (input.typename === 'Document' || input.typename === 'Layer'));
const Png = Schema.Struct({
    method: Schema.optionalKey(Schema.Enum(photoshop.constants.PNGMethod)),
    compression: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 9 }))),
    interlaced: Schema.optionalKey(Schema.Boolean),
});
const Entry = Uxp.Entry.mapFields(Struct.assign({ isFile: Schema.Literal(true) }));
const Members = Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Finite]));
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });
const Hosted = Schema.Tuple([Schema.Struct({ hostName: Schema.NonEmptyString.annotate({ description: 'Application name, the bundle name of the installation' }) })]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (target: string) =>
    Effect.flatMap(Schema.decodeEffect(Specifier.Path)(target), (specifier) => {
        const found = walk(containment, specifier);
        return Option.match(Schema.decodeUnknownOption(Unmatched)(found), { onNone: () => Effect.succeed(found), onSome: Effect.fail });
    });
const referenced = (value: unknown) => Effect.flatMap(Effect.fromNullishOr(Predicate.isObject(value) ? reference(containment, value) : undefined), (row) => Effect.orDie(Schema.decodeEffect(Reference)(row)));
const encoded = (value: unknown): unknown => encode(value, { reference: (each) => reference(containment, each) });
const holds = (layer: photoshop.Document['layers'][number], id: number): boolean => (layer.layers ?? []).some((each) => each.id === id || holds(each, id));
const isolated = (layers: readonly photoshop.Document['layers'][number][], id: number): Effect.Effect<void, HostError> =>
    Effect.forEach(
        layers.filter((layer) => layer.id !== id),
        (layer) => (holds(layer, id) ? isolated(layer.layers ?? [], id) : Effect.try({ try: () => Object.assign(layer, { visible: false }), catch: HostError.refused(`visible of layer ${layer.id}`) })),
        { discard: true },
    );
const cancelled = (context: photoshop.ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const modal = Effect.fnUntraced(function* <A, E, R>(commandName: string, body: (context: photoshop.ExecutionContext) => Effect.Effect<A, E, R>) {
    const services = yield* Effect.context<R>();
    return yield* yield* Effect.tryPromise({
        try: (signal) => photoshop.core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }),
        catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.make({ reason: { _tag }, subject: commandName, cause }) }),
    });
});
const history = <A, E, R>(label: string, { hostControl }: photoshop.ExecutionContext, body: Effect.Effect<A, E, R>) =>
    photoshop.app.documents.reduce<Effect.Effect<A, E | HostError, R>>(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => (photoshop.app.documents.some(({ id: openId }) => openId === id) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        body,
    );
const stored = (url: URL, opened: (files: Uxp['Service']['uxp']['storage']['localFileSystem'], href: string) => Promise<unknown>) =>
    Effect.flatMap(Uxp, ({ uxp: { storage } }) => Effect.tryPromise({ try: () => opened(storage.localFileSystem, url.href), catch: HostError.refused(url.href) })).pipe(
        Effect.filterOrFail(
            (entry): entry is File => Schema.is(Entry)(entry),
            () => new Cause.IllegalArgumentError(`${url.href} names no file`),
        ),
    );
const session = Uxp.preserve(photoshop.app, { displayDialogs: photoshop.constants.DialogModes.NONE });
const execute = (code: string, label: string) =>
    modal(label, (context) =>
        history(
            label,
            context,
            Effect.gen(function* () {
                const services = yield* Effect.context<never>();
                const resolve = flow(located, Effect.runSyncWith(services));
                const value = yield* yield* Uxp.evaluate({ ...modules, ...classes, ...enumerations, resolve }, code);
                const result = yield* Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: Uxp.thrown });
                return Option.map(Option.fromUndefinedOr(result), encoded);
            }),
        ),
    );
const open = (file: URL, label: string) =>
    stored(file, (files, href) => files.getEntryWithUrl(href)).pipe(
        Effect.flatMap((entry) => modal(label, () => Effect.tryPromise({ try: () => photoshop.app.open(entry), catch: HostError.refused(label) }))),
        Effect.flatMap(referenced),
        Effect.catchTag('NoSuchElementError', () => new Cause.IllegalArgumentError(`${label} opened no document`)),
    );
const rasterize = Effect.fnUntraced(function* ({ target, longestSide, options, file }: { readonly target: string; readonly longestSide: number; readonly options: Option.Option<unknown>; readonly file: URL }, label: string) {
    const [png, found, written] = yield* Effect.all([
        Effect.transposeOption(Option.map(options, Schema.decodeUnknownEffect(Png, { onExcessProperty: 'error' }))),
        located(target).pipe(Effect.filterOrFail(Schema.is(Exportable), () => new Cause.IllegalArgumentError(`${target} names no document, layer, or artboard`))),
        stored(file, (files, href) => files.createEntryWithUrl(href, { overwrite: true })),
    ]);
    return yield* modal(
        label,
        Effect.fnUntraced(function* ({ hostControl }) {
            const { document, placed } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, placed: [] }), Layer: (layer) => ({ document: layer.document, placed: [layer] }) }));
            const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
            yield* Effect.tryPromise({ try: () => hostControl.registerAutoCloseDocument(copy.id), catch: HostError.refused(`registerAutoCloseDocument of document ${copy.id}`) });
            yield* Effect.forEach(placed, ({ id, bounds }) => Effect.andThen(isolated(copy.layers, id), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
            const scale = Math.min(1, longestSide / Math.max(copy.width, copy.height));
            const [width, height] = [copy.width * scale, copy.height * scale];
            yield* scale === 1 ? Effect.void : Effect.tryPromise({ try: () => copy.resizeImage(width, height), catch: HostError.refused(`resizeImage to ${width}x${height}`) });
            yield* Effect.tryPromise({ try: () => copy.saveAs.png(written, Option.getOrUndefined(png), true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
        }),
    );
});
const objectModel = (name: Option.Option<string>) =>
    Declaration.describe(name, Record.keys(classes), Record.keys(enumerations), (named) =>
        Option.match(Record.get(classes, named), {
            onNone: () => Effect.map(Schema.decodeUnknownEffect(Members)(Record.get(enumerations, named).pipe(Option.getOrUndefined)), (members) => Declaration.cases.Enumeration.make({ name: named, members: Record.collect(members, (member, value) => ({ name: member, description: Option.some(`${value}`) })) })),
            onSome: ({ prototype }: { readonly prototype?: unknown }) => {
                const [{ fields, methods } = { fields: [], methods: [] }, ...parents] = Uxp.members(prototype);
                return Effect.orDie(Schema.decodeEffect(Declaration.cases.Class)({ _tag: 'Class', name: named, parents: Array.map(parents, Struct.get('name')), fields, methods }));
            },
        }),
    );
const application = Effect.tryPromise({
    try: () => photoshop.action.batchPlay([{ _obj: 'get', _target: [{ _property: 'hostName' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {}),
    catch: HostError.refused(`Photoshop ${host.version} host name`),
}).pipe(
    Effect.flatMap(Schema.decodeUnknownEffect(Hosted)),
    Effect.map(([{ hostName }]) => hostName),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, containment, execute, located, objectModel, open, rasterize, session };
