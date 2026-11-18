import { HostError, Reference, Specifier } from '@rasm/object-model';
import { type Containment, encode, read, reference, walk } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import * as photoshop from 'adobe:photoshop';
import { type File, host } from 'adobe:uxp';
import { Array, Cause, Effect, Exit, flow, Match, Option, Path, Predicate, Record, Schema, SchemaTransformation, String, Struct } from 'effect';
import type { ExecutionContext } from 'photoshop/dom/CoreModules';
import type { Document } from 'photoshop/dom/Document';
import type { PNGSaveOptions } from 'photoshop/dom/objects/SaveOptions';

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
} as const satisfies Record<string, keyof typeof photoshop.app | keyof Document>;
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

const Exportable = Schema.declare((input: unknown): input is Document | Document['layers'][number] => Predicate.hasProperty(input, 'typename') && (input.typename === 'Document' || input.typename === 'Layer'));
const Png = Schema.Struct({
    method: Schema.optionalKey(Schema.Enum(photoshop.constants.PNGMethod)),
    compression: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 9 }))),
    interlaced: Schema.optionalKey(Schema.Boolean),
});
const Saved = Png.pipe(
    Schema.decodeTo(
        Schema.declare((input: unknown): input is PNGSaveOptions => Predicate.isObject(input)),
        SchemaTransformation.passthrough({ strict: false }),
    ),
).annotate({ description: 'PNG options Photoshop reads from a plain object, where @types/photoshop names a class' });
const Entry = Uxp.Entry.mapFields(Struct.assign({ isFile: Schema.Literal(true) }));
const Stored = Schema.declare((input: unknown): input is globalThis.File => Schema.is(Entry)(input)).annotate({ description: 'UXP storage file, the DOM File type @types/photoshop names for file parameters' });
const Closing = Schema.Struct({ registerAutoCloseDocument: Schema.declare((input: unknown): input is (documentID: number) => Promise<void> => Predicate.isFunction(input)) }).annotate({ description: 'hostControl member Photoshop holds and @types/photoshop omits' });
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
        try: (signal): Promise<unknown> => photoshop.core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }),
        catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.of({ _tag }, commandName)(cause) }),
    });
    return yield* yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.declare((value: unknown): value is Exit.Exit<A, E> => Exit.isExit(value)))(settled));
});
const history = <A, E, R>(label: string, { hostControl }: ExecutionContext, body: Effect.Effect<A, E, R>) =>
    photoshop.app.documents.reduce<Effect.Effect<A, E | HostError, R>>(
        (inner, { id }) =>
            Effect.acquireUseRelease(
                Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name: label }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                () => inner,
                (suspension, exit) => (photoshop.app.documents.some(({ id: openId }) => openId === id) ? Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }) : Effect.void),
            ),
        body,
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
                const value = yield* yield* Uxp.evaluate({ resolve }, code);
                const result = yield* Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: Uxp.thrown });
                return yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(result), flow(encoded, Schema.decodeUnknownEffect(Schema.Json))));
            }),
        ),
    );
const open = Effect.fnUntraced(function* (file: string, label: string) {
    const [{ uxp }, path] = yield* Effect.all([Uxp, Path.Path]);
    const url = yield* path.toFileUrl(file);
    const entry = yield* Effect.tryPromise({ try: () => uxp.storage.localFileSystem.getEntryWithUrl(url.href), catch: HostError.refused(file) }).pipe(Effect.flatMap((each) => Effect.mapError(Schema.decodeUnknownEffect(Stored)(each), () => new Cause.IllegalArgumentError(`${file} names no file`))));
    const opened = yield* modal(label, () => Effect.tryPromise({ try: () => photoshop.app.open(entry), catch: HostError.refused(label) }));
    return yield* Effect.catchTag(referenced(opened), 'NoSuchElementError', () => new Cause.IllegalArgumentError(`${label} opened no document`));
});
const rasterize = Effect.fnUntraced(function* (target: string, longestSide: number, options: Option.Option<unknown>, written: File) {
    const [png, found, stored] = yield* Effect.all([
        Effect.transposeOption(Option.map(options, Schema.decodeUnknownEffect(Saved, { onExcessProperty: 'error' }))),
        located(target).pipe(Effect.filterOrFail(Schema.is(Exportable), () => new Cause.IllegalArgumentError(`${target} names no document, layer, or artboard`))),
        Effect.orDie(Schema.decodeUnknownEffect(Stored)(written)),
    ]);
    return yield* modal(
        `Rasterize ${target}`,
        Effect.fnUntraced(function* ({ hostControl }) {
            const { document, placed } = Match.value(found).pipe(Match.discriminatorsExhaustive('typename')({ Document: (opened) => ({ document: opened, placed: [] }), Layer: (layer) => ({ document: layer.document, placed: [layer] }) }));
            const copy = yield* Effect.tryPromise({ try: () => document.duplicate(), catch: HostError.refused(`duplicate of document ${document.id}`) });
            const refused = HostError.refused(`registerAutoCloseDocument of document ${copy.id}`);
            const closing = yield* Effect.filterOrFail(Effect.succeed(hostControl), Schema.is(Closing), refused);
            yield* Effect.tryPromise({ try: () => closing.registerAutoCloseDocument(copy.id), catch: refused });
            yield* Effect.forEach(placed, ({ id, bounds }) => Effect.andThen(isolated(copy.layers, id), Effect.tryPromise({ try: () => copy.crop(bounds), catch: HostError.refused(`crop to ${bounds.left} ${bounds.top} ${bounds.right} ${bounds.bottom}`) })), { discard: true });
            const scale = Math.min(1, longestSide / Math.max(copy.width, copy.height));
            const [width, height] = [copy.width * scale, copy.height * scale];
            yield* scale === 1 ? Effect.void : Effect.tryPromise({ try: () => copy.resizeImage(width, height), catch: HostError.refused(`resizeImage to ${width}x${height}`) });
            yield* Effect.tryPromise({ try: () => copy.saveAs.png(stored, Option.getOrUndefined(png), true), catch: HostError.refused(`saveAs.png to ${written.nativePath}`) });
        }),
    );
});
const application = Effect.tryPromise({
    try: () => photoshop.action.batchPlay([{ _obj: 'get', _target: [{ _property: 'hostName' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {}),
    catch: HostError.refused(`Photoshop ${host.version} host name`),
}).pipe(
    Effect.flatMap(Schema.decodeUnknownEffect(Hosted)),
    Effect.map(([{ hostName }]) => hostName),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { application, containment, execute, located, open, rasterize, session };
