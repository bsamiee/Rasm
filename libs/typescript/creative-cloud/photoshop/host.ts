import type { Document, ExecutionContext } from 'adobe:photoshop';
import type { File } from 'adobe:uxp';
import { ExecuteScript, HostError, ObjectModel, Reference, Requests } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cause, Effect, Exit, flow, Layer, Option, Predicate, Record, Schema, SchemaGetter, Struct } from 'effect';
import { Url } from 'effect/http';
import { RpcMiddleware } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Photoshop = typeof import('adobe:photoshop').default;

// --- [CONSTANTS] -----------------------------------------------------------------------

const COLLECTIONS = {
    document: 'documents',
    font: 'fonts',
    layer: 'layers',
    artboard: 'artboards',
    channel: 'channels',
    colorSampler: 'colorSamplers',
    countItem: 'countItems',
    guide: 'guides',
    historyState: 'historyStates',
    layerComp: 'layerComps',
    pathItem: 'pathItems',
} as const satisfies Record<string, keyof Photoshop['app'] | keyof Document>;

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(Struct.keys(COLLECTIONS));
const Step = Schema.Union([Schema.TemplateLiteralParser([Kind, ...Reference.Identified.parts]), Schema.TemplateLiteralParser([Kind, ...Reference.Named.parts]), Schema.TemplateLiteralParser([Kind, ...Reference.Indexed.parts])]);
const Listed = Schema.Struct({ length: Schema.Int });
const Identified = Schema.Struct({ id: Schema.Int });
const Named = Schema.Union([Schema.Struct({ name: Schema.String }), Schema.Struct({ title: Schema.String }).pipe(Schema.decodeTo(Schema.Struct({ name: Schema.String }), { decode: SchemaGetter.transform(({ title }) => ({ name: title })), encode: SchemaGetter.transform(({ name }) => ({ title: name })) }))]);
const Members = Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Finite]));
const Opened = Schema.Struct({ typename: Schema.Literal('Document'), id: Schema.Int, title: Schema.String });
const Placed = Schema.Struct({ typename: Schema.Literal('Layer'), id: Schema.Int, name: Schema.String, document: Schema.Struct({ id: Schema.Int }), bounds: Schema.Struct({ left: Schema.Finite, top: Schema.Finite, right: Schema.Finite, bottom: Schema.Finite }) });
const Executable = Schema.Tuple([Schema.Struct({ executablePath: Schema.Struct({ _path: Schema.String }) })]);

// --- [SERVICES] ------------------------------------------------------------------------

class DisplayDialogs extends RpcMiddleware.Service<DisplayDialogs>()('@rasm/photoshop/host/DisplayDialogs') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const members = (container: object, kind: typeof Kind.Type): readonly object[] => {
    const items: unknown = Reflect.get(container, COLLECTIONS[kind]);
    return Predicate.isObject(items) ? Option.match(Schema.decodeUnknownOption(Listed)(items), { onNone: Array.empty, onSome: ({ length }) => Array.makeBy(length, (at): unknown => Reflect.get(items, at)).filter(Predicate.isObject) }) : [];
};
const tree =
    (kind: typeof Kind.Type) =>
    (item: object): readonly object[] => [item, ...Array.flatMap(members(item, kind), tree(kind))];
const named = (item: object): Option.Option<string> => Option.map(Schema.decodeUnknownOption(Named)(item), ({ name }) => name);
const located = (container: object, [kind, form, value]: typeof Step.Type): Option.Option<object> => {
    if (form === '[@id=') {
        return Array.findFirst(Array.flatMap(members(container, kind), tree(kind)), (item) => Option.exists(Schema.decodeUnknownOption(Identified)(item), ({ id }) => id === value));
    }
    return form === '[' ? Array.get(members(container, kind), value) : Array.findFirst(members(container, kind), (item) => Option.contains(named(item), value));
};
const resolve = (app: object) => (specifier: string) =>
    Effect.forEach(
        specifier.split('/').filter((step) => step !== ''),
        (step) => Schema.decodeUnknownEffect(Step)(step),
    ).pipe(
        Effect.flatMap(
            Effect.reduce(
                (): object => app,
                (container, step) => Effect.fromOption(located(container, step), () => Unmatched.make({ subject: step[0], query: Option.some(specifier), candidates: Array.getSomes(Array.map(members(container, step[0]), named)) })),
            ),
        ),
    );
const encoded = (value: unknown): unknown =>
    Option.getOrElse(
        Option.firstSomeOf<typeof Reference.Encoded>([
            Option.map(Schema.decodeUnknownOption(Opened)(value), ({ typename, id, title }) => ({ type: typename, specifier: `/document[@id=${id}]`, name: title, valid: true })),
            Option.map(Schema.decodeUnknownOption(Placed)(value), ({ typename, id, name, document }) => ({ type: typename, specifier: `/document[@id=${document.id}]/layer[@id=${id}]`, name, valid: true })),
        ]),
        (): unknown => {
            if (Array.isArray(value)) {
                return Array.map(value, encoded);
            }
            return Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype ? Record.map(Object.fromEntries(Object.entries(value)), encoded) : value;
        },
    );
const declaration = (name: string, type: { readonly prototype: object }) => {
    const { getters, methods } = Uxp.accessors(type.prototype);
    const parent = (prototype: unknown) =>
        Option.map(
            Option.liftPredicate(prototype, (each): each is object => Predicate.isObject(each) && each !== Object.prototype),
            (each) => [each.constructor.name, Object.getPrototypeOf(each)] as const,
        );
    return { _tag: 'Class', name, parents: Array.unfold(Object.getPrototypeOf(type.prototype), parent), fields: Array.map(getters, ({ name: field, settable }) => ({ name: field, types: [], settable })), methods: Array.map(methods, (method) => ({ name: method, parameters: [] })) } as const;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = Requests.middleware(DisplayDialogs).annotate(ExecuteScript.Awaits, true);
const host = (photoshop: Photoshop) => {
    const { app, core, constants, action } = photoshop;
    const resolved = resolve(app);
    const classes = Record.filter(Object.fromEntries(Object.entries(app)), Predicate.isFunction);
    const modal =
        (commandName: string) =>
        <A, E>(body: (context: ExecutionContext) => Effect.Effect<A, E>) =>
            Effect.flatten(Effect.tryPromise({ try: (signal) => core.executeAsModal((context) => Effect.runPromiseExit(body(context), { signal }), { commandName }), catch: HostError.refused(commandName) }));
    const entry = (url: URL, create: boolean) =>
        Effect.flatMap(Uxp, ({ uxp: { storage } }) =>
            Effect.tryPromise({ try: (): Promise<unknown> => (create ? storage.localFileSystem.createEntryWithUrl(url.href, { overwrite: true }) : storage.localFileSystem.getEntryWithUrl(url.href)), catch: HostError.refused(url.href) }).pipe(
                Effect.filterOrFail(
                    (found): found is File => Predicate.hasProperty(found, 'isFile') && found.isFile === true,
                    () => new Cause.IllegalArgumentError(`${url.href} names no file`),
                ),
            ),
        );
    const executed = (payload: ExecuteScript, history: Option.Option<ExecutionContext['hostControl']>) => {
        const scope = { ...photoshop, ...classes, ...constants, resolve: flow(resolved, Effect.runSync) };
        const run = Effect.gen(function* () {
            const value = yield* yield* Uxp.evaluate(scope, payload.code);
            return yield* Effect.tryPromise({ try: (): Promise<unknown> => Promise.resolve(value), catch: Uxp.thrown });
        });
        return Option.match(history, {
            onNone: () => run,
            onSome: (hostControl) =>
                Effect.acquireUseRelease(
                    Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: app.activeDocument.id, name: payload.label }), catch: HostError.refused(payload.label) }),
                    () => run,
                    (suspension, exit) => Effect.promise(() => hostControl.resumeHistory(suspension, Exit.isSuccess(exit))),
                ),
        }).pipe(Effect.flatMap((result) => Schema.decodeUnknownEffect(ExecuteScript.Result)(Predicate.isUndefined(result) ? {} : { result: encoded(result) })));
    };
    const target = (found: object, subject: string) =>
        Option.match(
            Array.findFirst(app.documents, (document) => document === found),
            {
                onSome: (document) => Effect.succeed({ document, bounds: Option.none<typeof Placed.Type.bounds>() }),
                onNone: () =>
                    Effect.fromOption(
                        Option.flatMap(Schema.decodeUnknownOption(Placed)(found), ({ document: { id }, bounds }) =>
                            Option.map(
                                Array.findFirst(app.documents, (document) => document.id === id),
                                (document) => ({ document, bounds: Option.some(bounds) }),
                            ),
                        ),
                        () => new Cause.IllegalArgumentError(`${subject} names no document, layer, or artboard`),
                    ),
            },
        );
    const rendered = ({ document, bounds }: Effect.Success<ReturnType<typeof target>>, longestSide: number, options: Option.Option<object>, written: File, refused: (cause: unknown) => HostError) =>
        Effect.acquireUseRelease(
            Effect.tryPromise({ try: () => document.duplicate(), catch: refused }),
            (copy: Document) =>
                Effect.gen(function* () {
                    yield* Option.match(bounds, { onNone: () => Effect.void, onSome: (cropped) => Effect.tryPromise({ try: () => copy.crop(cropped), catch: refused }) });
                    const scale = longestSide / Math.max(copy.width, copy.height);
                    yield* Effect.tryPromise({ try: () => copy.resizeImage(copy.width * scale, copy.height * scale), catch: refused });
                    yield* Effect.tryPromise({ try: () => copy.saveAs.png(written, Option.getOrUndefined(options), true), catch: refused });
                }),
            (copy) => Effect.sync(() => copy.closeWithoutSaving()),
        );
    const model = (name: Option.Option<string>) => {
        const enumerations = Record.keys(constants);
        return Effect.flatMap(Effect.transposeOption(Option.map(name, (query) => Unmatched.single([...Record.keys(classes), ...enumerations], 'class or enumeration', Option.some(query), Array.of))), (found) =>
            Schema.decodeEffect(ObjectModel.Result)(
                Option.match(found, {
                    onNone: () => ({ _tag: 'Index', classes: Record.keys(classes), enumerations }) as const,
                    onSome: (match) =>
                        Option.match(Record.get(classes, match), {
                            onNone: () => ({ _tag: 'Enumeration', name: match, members: Record.collect(Option.getOrElse(Schema.decodeUnknownOption(Members)(Reflect.get(constants, match)), Record.empty), (member, value) => ({ name: member, description: String(value) })) }) as const,
                            onSome: (type) => declaration(match, type),
                        }),
                }),
            ),
        );
    };
    const server = Layer.merge(
        Contract.toLayer({
            execute_script: (payload) => modal(payload.label)(({ hostControl }) => executed(payload, Option.as(Array.head(app.documents), hostControl))),
            open: ({ file, label }) =>
                Effect.flatMap(entry(file, false), (found) => modal(label)(() => Effect.tryPromise({ try: () => app.open(found), catch: HostError.refused(label) }))).pipe(
                    Effect.map((document) => Reference.make({ type: document.typename, specifier: `/document[@id=${document.id}]`, name: Option.some(document.title), valid: true })),
                ),
            render: ({ target: specifier, longestSide, options, file, label }) =>
                Effect.all([Effect.flatMap(resolved(specifier), (found) => target(found, specifier)), entry(file, true)]).pipe(Effect.flatMap(([found, written]) => modal(label)(() => rendered(found, longestSide, options, written, HostError.refused(label))))),
            object_model: ({ name }) => model(name),
        }),
        Layer.succeed(DisplayDialogs)((effect) =>
            Effect.andThen(
                Effect.acquireRelease(
                    Effect.sync(() => {
                        const held = app.displayDialogs;
                        app.displayDialogs = constants.DialogModes.NONE;
                        return held;
                    }),
                    (held) =>
                        Effect.sync(() => {
                            app.displayDialogs = held;
                        }),
                ),
                effect,
            ),
        ),
    );
    const application = Effect.tryPromise(() => action.batchPlay([{ _obj: 'get', _target: [{ _property: 'executablePath' }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] }], {})).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Executable)),
        Effect.flatMap(([{ executablePath }]) => Effect.fromResult(Url.fromString(encodeURI(`file://${executablePath._path}`)))),
    );
    return { server, application };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, host };
