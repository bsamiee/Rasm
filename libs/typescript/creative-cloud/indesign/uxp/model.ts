import indesign, {
    type AnySwatch,
    type Application,
    type BaseCollection,
    type CellStyle,
    type CharacterStyle,
    type Document,
    type Enumeration,
    type Enumerator,
    type Font,
    type InDesignDOMObject,
    type Layer,
    type Link,
    type MasterSpread,
    type ObjectStyle,
    type Page,
    type PageItem,
    type ParagraphStyle,
    type PreflightProcess,
    type ReadonlyNamedDOMObject,
    type Spread,
    type Story,
    type TableStyle,
    type Text,
    type TextDefault,
    type TextFrame,
    type TextPath,
    type XmlStory,
} from 'adobe:indesign';
import { HostError, type ObjectModel, Reference } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import { Array, Cache, Cause, Context, Deferred, Duration, Effect, Equal, Equivalence, Exit, Function, flow, identity, Match, Number, Option, Predicate, pipe, Record, Result, Schema, SchemaAST, Struct, Tuple } from 'effect';
import { Extent, type Search } from '#/index';
import { Specifier } from '#/specifier';
import { readDeclaration, unavailableReason } from '#/uxp/dictionary';
import { Value } from '#/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Typed {
    readonly types: Option.Option<readonly string[]>;
    readonly type: Option.Option<string>;
}
type Failure = HostError | Unmatched | Schema.SchemaError;
type Owned = {
    readonly toSpecifier: InDesignDOMObject<unknown>;
    readonly createTOC: Document;
    readonly everyItem: BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }>;
    readonly name: ReadonlyNamedDOMObject<unknown>;
    readonly menuActions: Application;
    readonly documentOffset: Page;
    readonly spreadHidden: Spread;
    readonly namePrefix: MasterSpread;
    readonly layerColor: Layer;
    readonly linkResourceURI: Link;
    readonly parentColorGroup: AnySwatch;
    readonly itemLayer: PageItem;
    readonly textContainers: Story | XmlStory;
    readonly texts: InDesignDOMObject<unknown> & Pick<Text, 'texts'>;
    readonly parentTextFrames: Text;
    readonly textFrameIndex: TextFrame | TextPath;
    readonly basedOn: ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle;
    readonly nestedGrepStyles: ParagraphStyle;
    readonly textHasOverrides: InDesignDOMObject<unknown> & Pick<Text, 'textHasOverrides'>;
    readonly parentStory: InDesignDOMObject<unknown> & Pick<Text, 'parentStory'>;
    readonly textStyleRanges: InDesignDOMObject<unknown> & Pick<Text, 'textStyleRanges'>;
    readonly postscriptName: Font;
    readonly aggregatedResults: PreflightProcess;
} & {
    readonly [K in `change${typeof Search.Type}`]: InDesignDOMObject<unknown> & Pick<Document, `${'find' | 'change'}${typeof Search.Type}`>;
};

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/uxp/model', {
    make: Effect.gen(function* () {
        const model: typeof indesign = Object.create(indesign);
        const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
        const classes = Array.filterMap(exported, ([name, value]) => (Predicate.isFunction(value) ? Result.succeed(name) : Result.failVoid));
        const enumerations = Array.filterMap(exported, ([name, value]) => (value instanceof indesign.Enumeration ? Result.succeed(name) : Result.failVoid));
        const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
        const enumerator = (value: unknown): value is Enumerator => value instanceof indesign.Enumerator;
        const members = <E extends Enumeration>(found: E) => {
            const keyed = (member: unknown, key: string): member is Extract<E[keyof E], Enumerator> => enumerator(member) && String(member) === key;
            return Array.filterMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
                const member: unknown = Reflect.get(found, key);
                return keyed(member, key) ? Result.succeed(member) : Result.failVoid;
            });
        };
        const owning = <K extends keyof Owned>(member: K) => {
            const owners = new Set(Array.filterMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? Result.succeed(type.prototype) : Result.failVoid)));
            return (value: unknown): value is Owned[K] => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
        };
        const domObject = owning('toSpecifier');
        const document = owning('createTOC');
        const ancestors = (object: unknown, link: 'parent' | 'basedOn' = 'parent'): readonly InDesignDOMObject<unknown>[] => Array.unfold(object, (above: unknown) => (domObject(above) && above.constructorName !== 'Application' ? Option.some([above, Reflect.get(above, link)] as const) : Option.none()));
        const collection = owning('everyItem');
        const nothing = (value: unknown): boolean => enumerator(value) && indesign.NothingEnum.NOTHING.equals(value);
        const tagged = ({ propertySignatures }: SchemaAST.Objects) =>
            Option.flatMap(
                Array.findFirst(propertySignatures, ({ name }) => name === '_tag'),
                ({ type }) => (SchemaAST.isLiteral(type) ? Option.some(type.literal) : Option.none()),
            );
        const fits =
            (value: unknown) =>
            (member: SchemaAST.AST): boolean =>
                SchemaAST.isObjects(member) ? Predicate.isObject(value) && !Array.isArray(value) && Option.match(tagged(member), { onNone: () => true, onSome: (tag) => domObject(value) && value.constructorName === tag }) : SchemaAST.isArrays(member) && (Array.isArray(value) || collection(value));
        const entry = (node: SchemaAST.Arrays, index: number): SchemaAST.AST =>
            Option.getOrElse(
                Option.orElse(Array.get(node.elements, index), () => Array.head(node.rest)),
                () => Schema.Unknown.ast,
            );
        const record = (node: SchemaAST.Objects, value: object, skipped: readonly PropertyKey[]) => {
            const referenced = domObject(value) && node.propertySignatures.some(({ name }) => name === 'specifier');
            const read = ({ name, type }: SchemaAST.PropertySignature) => {
                if (name === '_tag' && SchemaAST.isLiteral(type)) {
                    return Option.some([name, type.literal] as const);
                }
                if (skipped.includes(name) || (referenced && Object.hasOwn(Reference.fields, name))) {
                    return Option.none();
                }
                return Option.liftThrowable((): unknown => Reflect.get(value, name))().pipe(
                    Option.filter((held) => !(Predicate.isNullish(held) || nothing(held))),
                    Option.map((held) => [name, picking(type, held)] as const),
                );
            };
            return Object.fromEntries([...(referenced ? Object.entries(Model.referenceUnsafe(value)) : []), ...Array.getSomes(Array.map(node.propertySignatures, read))]);
        };
        const list = (node: SchemaAST.Arrays, value: unknown) => {
            const items: unknown = collection(value) ? value.everyItem().getElements() : value;
            return Array.isArray(items) ? items.map((item: unknown, index) => picking(entry(node, index), item)) : value;
        };
        const picking = (ast: SchemaAST.AST, value: unknown, skipped: readonly PropertyKey[] = []): unknown =>
            Match.value(ast).pipe(
                Match.when(SchemaAST.isSuspend, (node) => picking(node.thunk(), value)),
                Match.when(SchemaAST.isUnion, (node) => Option.match(Array.findFirst(node.types, fits(value)), { onNone: () => value, onSome: (member) => picking(member, value) })),
                Match.when(SchemaAST.isArrays, (node) => list(node, value)),
                Match.when(SchemaAST.isObjects, (node) =>
                    Option.match(Option.all([Option.filter(Option.liftPredicate(value, Predicate.isObject), Predicate.not(collection)), Option.liftPredicate(node.propertySignatures, Array.isReadonlyArrayNonEmpty)]), { onNone: () => value, onSome: ([held]) => record(node, held, skipped) }),
                ),
                Match.orElse(() => value),
            );
        const plural = new Set(Array.filterMap(exported, ([name, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, 'everyItem') ? Result.succeed(name) : Result.failVoid)));
        const collectionField = ({ type }: { readonly type: Option.Option<string> }): boolean => Option.exists(type, (kind) => plural.has(kind));
        const filled = <R extends { readonly types: Option.Option<readonly string[]> }>(row: R) => ({
            ...row,
            members: pipe(
                Option.getOrElse(row.types, Array.empty<string>),
                Array.map(enumeration),
                Array.getSomes,
                Array.flatMap((found) => Array.map(members(found), String)),
                Array.dedupe,
                Option.liftPredicate(Array.isArrayNonEmpty),
            ),
        });
        const retried = { capacity: exported.length, timeToLive: (exit: Exit.Exit<unknown, unknown>) => (Exit.isFailure(exit) && Option.exists(Cause.findErrorOption(exit.cause), Schema.is(HostError)) ? Duration.zero : Duration.infinity) };
        const declarations = yield* Cache.makeWith(
            Effect.fnUntraced(function* (kind: string) {
                const { fields, methods, ...declared } = yield* readDeclaration(kind, classes);
                return {
                    ...declared,
                    fields: Record.map(fields, filled),
                    methods: Record.map(methods, Struct.evolve({ parameters: Array.map(filled) })),
                };
            }),
            retried,
        );
        const parents = (name: string): Effect.Effect<readonly string[], Unmatched | HostError> =>
            Cache.get(declarations, name).pipe(Effect.map(Struct.get('parent')), Effect.flatMap(Option.match({ onNone: () => Effect.succeed(Array.empty<string>()), onSome: (above) => Effect.map(parents(above), Array.prepend(above)) })));
        const element = (kind: string) => Effect.map(Cache.get(declarations, kind), flow(Struct.get('methods'), Record.get('item'), Option.flatMap(Struct.get('returns')), Option.map(Struct.get('type'))));
        const documentField = yield* Cache.makeWith(
            Effect.fnUntraced(function* (kind: string) {
                const { fields } = yield* Cache.get(declarations, 'Document');
                const listed = Array.findFirst(Record.toEntries(fields), flow(Tuple.get(1), Predicate.Struct({ array: Predicate.isTruthy, type: Option.contains(kind) })));
                const held = Effect.findFirst(Record.toEntries(Record.filter(fields, collectionField)), flow(Tuple.get(1), Struct.get('type'), Option.map(element), Effect.transposeOption, Effect.map(flow(Option.flatten, Option.contains(kind)))));
                return Option.map(yield* Option.match(listed, { onNone: () => held, onSome: Effect.succeedSome }), Tuple.get(0));
            }),
            retried,
        );
        return Object.assign(model, {
            exported,
            classes,
            enumerations,
            owning,
            domObject,
            collection,
            picked: (schema: Schema.Top, value: object, skipped: readonly PropertyKey[] = []): Record.ReadonlyRecord<string, unknown> => Option.getOrElse(Option.liftPredicate(picking(SchemaAST.toEncoded(schema.ast), value, skipped), Value.plainObject), Record.empty),
            named: owning('name'),
            application: owning('menuActions'),
            document,
            page: owning('documentOffset'),
            spread: owning('spreadHidden'),
            masterSpread: owning('namePrefix'),
            layer: owning('layerColor'),
            link: owning('linkResourceURI'),
            swatch: owning('parentColorGroup'),
            item: owning('itemLayer'),
            story: owning('textContainers'),
            textContainer: owning('texts'),
            text: owning('parentTextFrames'),
            textDefault: (value: unknown): value is TextDefault => domObject(value) && value.constructorName === 'TextDefault',
            frame: owning('textFrameIndex'),
            style: owning('basedOn'),
            paragraphStyle: owning('nestedGrepStyles'),
            overridable: owning('textHasOverrides'),
            storied: owning('parentStory'),
            font: owning('postscriptName'),
            preflightProcess: owning('aggregatedResults'),
            ancestors,
            owner: (object: unknown): Option.Option<Document> => Array.findFirst(ancestors(object), document),
            enumeration,
            enumerator,
            nothing,
            members,
            collectionField,
            declarations,
            parents,
            documentField,
        });
    }),
}) {
    static readonly current = Model.use(({ app, document }) => {
        const open = app.documents.everyItem().getElements();
        const shown = Option.flatMap(Option.liftThrowable((): unknown => app.activeWindow.parent)(), Option.liftPredicate(document));
        const candidates = Array.map(open, (each) => each.toSpecifier());
        return pipe(
            Array.head(open),
            Option.filter(() => open.length === 1),
            Option.orElse(() => shown),
            Effect.fromOption(() => Unmatched.make({ subject: 'document', candidates })),
        );
    });
    static readonly selection = Model.use(({ app }) => Effect.try({ try: () => app.selection, catch: HostError.refused('selection') }).pipe(Effect.flatMap(Unmatched.single('selection', Option.none(), (item) => [item.toSpecifier()]))));
    static readonly file = (owner: InDesignDOMObject<unknown>, field: string) =>
        Model.use(({ app, ScriptLanguage }) => Effect.try({ try: (): unknown => app.doScript('var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', ScriptLanguage.JAVASCRIPT, [owner.toSpecifier(), field]), catch: HostError.refused(`${owner.toSpecifier()} ${field}`) })).pipe(
            Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.OptionFromNullOr(Schema.String)), Effect.orDie)),
        );
    static readonly member = Effect.fnUntraced(function* (kind: string, name: string) {
        const { enumeration, members, enumerations } = yield* Model;
        const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.make({ subject: 'enumeration', query: Option.some(kind), candidates: enumerations }));
        return yield* Unmatched.single(members(found), `${kind} member`, Option.some(name), (each) => [String(each)]);
    });
    static readonly object = Effect.fnUntraced(function* (type: string, name: string, scope: Option.Option<object>) {
        const model = yield* Model;
        const { fields } = yield* Cache.get(model.declarations, type);
        const field = Record.has(fields, 'name') ? yield* Cache.get(model.documentField, type) : Option.none();
        const candidates = Option.zipWith(scope, field, (document, member) => {
            const listed: unknown = Reflect.get(document, member);
            return (model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named);
        });
        return yield* Option.match(candidates, { onNone: () => Effect.succeed<unknown>(name), onSome: Unmatched.single(type, Option.some(name), (each) => [each.toSpecifier(), each.name]) });
    });
    static readonly fonts = Model.use(({ app, FontStatus }) =>
        Effect.try({
            try: () => {
                const every = app.fonts.everyItem();
                const faces = Array.map(Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))), ([[label, postscript], [full, [family, style]]], index) => ({ label, postscript, full, family, style, index }));
                const regular = Record.map(Array.groupBy(faces, Struct.get('family')), (listed) => Array.findFirst(listed, Predicate.Struct({ style: Equal.equals('Regular') })).pipe(Option.getOrElse(Function.constant(Array.headNonEmpty(listed)))).index);
                const installed = Array.map(Array.dedupeWith(faces, Equivalence.mapInput(Equivalence.String, Struct.get('postscript'))), ({ label, postscript, full, family, index }) => ({
                    names: Array.make(label, postscript, full, ...(Option.contains(Record.get(regular, family), index) ? [family] : [])),
                    font: (): Font => app.fonts.item(index),
                }));
                const missing = (open: Document) => {
                    const used = open.fonts.everyItem();
                    return Array.filterMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? Result.failVoid : Result.succeed({ names: Array.of(label), font: (): Font => open.fonts.item(index) })));
                };
                const candidates = [...installed, ...Array.flatMap(app.documents.everyItem().getElements(), missing)];
                return (name: string): Effect.Effect<Font, Unmatched> => Effect.map(Unmatched.single(candidates, 'font', Option.some(name), Struct.get('names')), ({ font }) => font());
            },
            catch: HostError.refused('fonts'),
        }),
    );
    static readonly resolve = Effect.fn('Model.resolve')(function* (specifier: Specifier) {
        const [model, written] = yield* Effect.all([Model, specifier.path]);
        const identified = (path: string) =>
            Option.map(
                Option.filter(Schema.decodeUnknownOption(Specifier.Path)(path), ({ end, selector }) => Option.isNone(end) && Option.exists(selector, Reference.Selector.guards.Identified)),
                Struct.get('kind'),
            );
        const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = identified(path);
            const valid = (each: unknown): each is InDesignDOMObject<unknown> => model.domObject(each) && each.isValid && (Option.isNone(requested) || Equal.equals(identified(each.toSpecifier()), requested));
            return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] => (Array.isArray(found) ? found.every(valid) : valid(found)));
        };
        const nearest = function* (): Effect.fn.Return<readonly InDesignDOMObject<unknown>[], HostError | Unmatched> {
            const listed = flow(located, Option.toArray, Array.flatMap(Array.ensure));
            const siblings = listed(`${specifier.container}${specifier.separator}${specifier.kind}`);
            const container = Array.head(listed(specifier.container || '/'));
            const fields = Option.toArray(yield* Effect.transposeOption(Option.map(container, (at) => Cache.get(model.declarations, at.constructorName)))).flatMap(flow(Struct.get('fields'), Record.toEntries));
            const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at) => Reflect.get(at, field)));
            const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
            const related = fields.flatMap(([field, { type, array }]) => (array && Option.isSome(sibling) && Equal.equals(type, sibling) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
            const collections = Array.getSomes(
                fields
                    .filter(([, row]) => model.collectionField(row))
                    .map(([field]) => value(field))
                    .filter(model.collection)
                    .map((items) => Option.fromNullishOr(items.everyItem())),
            );
            const candidates = Array.dedupe(Array.map([...siblings, ...related.filter((each) => Option.contains(specifier.name, each.name))], (each) => each.toSpecifier()));
            return yield* Effect.fromOption(
                Option.map(
                    Array.findFirst(collections, (every) => every.toSpecifier() === written),
                    (every) => every.getElements(),
                ),
                () =>
                    Option.exists(specifier.selector, Reference.Selector.guards.Indexed)
                        ? Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: Option.some(written) })
                        : Unmatched.make({ subject: 'object', query: Option.some(written), candidates }),
            );
        };
        return yield* Option.match(located(written), {
            onSome: (found) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) || specifier.property ? found : Array.ensure(found)),
            onNone: () => Option.match(specifier.end, { onNone: () => Effect.gen(nearest), onSome: () => Effect.fail(Unmatched.make({ subject: 'range', query: Option.some(written) })) }),
        });
    });
    static readonly located = (specifier: Specifier) =>
        Model.use(
            Effect.fnUntraced(function* (model: Context.Service.Shape<typeof Model>) {
                const found = yield* Model.resolve(specifier);
                return model.domObject(found) ? found : yield* Unmatched.make({ subject: 'object', query: Option.some(yield* specifier.path) });
            }),
        );
    static readonly referenceUnsafe = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid ? Option.liftPredicate(Reflect.get(object, 'name'), Predicate.isString) : Option.none(), valid });
    };
    static readonly collected = (items: Owned['everyItem']) => ({ type: items.constructor.name, length: items.length, specifier: Option.map(Option.fromNullishOr(items.everyItem()), (every) => every.toSpecifier()) });
    static readonly reference = (object: InDesignDOMObject<unknown>): Effect.Effect<Reference, HostError> => Effect.try({ try: () => Model.referenceUnsafe(object), catch: HostError.refused('object reference') });
    static readonly encoder = Effect.map(Model, ({ enumerator, collection, domObject }) => {
        const replacer = function (this: object, key: string, part: unknown) {
            const held: unknown = Reflect.get(this, key);
            return Match.value(held).pipe(
                Match.when(Option.isOption, Option.getOrUndefined),
                Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                Match.orElse(() => part),
                Match.value,
                Match.when(enumerator, String),
                Match.when(collection, Model.collected),
                Match.when(domObject, Model.referenceUnsafe),
                Match.when(Predicate.isPromise, () => {
                    throw new Cause.IllegalArgumentError(`${key} holds a promise, scripts answer before it settles`);
                }),
                Match.when(Value.plainObject, identity),
                Match.when(Schema.is(Uxp.Entry), ({ url }) => String(url)),
                Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                Match.when(Predicate.isMap, Object.fromEntries),
                Match.when(Predicate.isSet, (entries) => [...entries]),
                Match.when(Predicate.isBigInt, String),
                Match.orElse(identity),
            );
        };
        return (value: unknown): string | undefined => JSON.stringify(value, replacer);
    });
    static readonly decoder = (target: unknown, fonts: Effect.Effect<(name: string) => Effect.Effect<Font, Unmatched>, HostError, Model>) => {
        const joined =
            <E>(failed: E | Unmatched) =>
            (later: E | Unmatched): E | Unmatched => {
                if (!Schema.is(Unmatched)(later)) {
                    return later;
                }
                return Schema.is(Unmatched)(failed) ? Unmatched.make({ subject: `${failed.subject} or ${later.subject}`, query: failed.query, candidates: [...failed.candidates, ...later.candidates] }) : failed;
            };
        const firstOf = <A, E, R>([head, ...rest]: Array.NonEmptyReadonlyArray<Effect.Effect<A, E, R>>) => rest.reduce<Effect.Effect<A, E | Unmatched, R>>((chosen, next) => Effect.catch(chosen, (failed) => Effect.mapError(next, joined(failed))), head);
        const typed = (row: Option.Option<Typed>) =>
            Effect.fnUntraced(function* (text: string) {
                const { enumeration, classes, owner } = yield* Model;
                const types = Option.getOrElse(Option.flatMap(row, Struct.get('types')), Array.empty<string>);
                const scope = Option.match(owner(target), { onSome: (document) => Effect.succeedSome<object>(document), onNone: () => Effect.option(Model.current) });
                const named = types.includes('String')
                    ? [Effect.succeed<unknown>(text)]
                    : Array.map(
                          types.filter((type) => type === 'Font' || classes.includes(type)),
                          Effect.fnUntraced(function* (type) {
                              return type === 'Font' ? yield* (yield* fonts)(text) : yield* Model.object(type, text, yield* scope);
                          }),
                      );
                const enumerated = Array.map(types.filter(flow(enumeration, Option.isSome)), (type): Effect.Effect<unknown, Failure, Model> => Model.member(type, text));
                return yield* firstOf(Array.isReadonlyArrayNonEmpty(named) ? Array.prependAll(named, enumerated) : Array.append(enumerated, Effect.succeed<unknown>(text)));
            });
        const decode =
            (row: Option.Option<Typed>, within: Option.Option<string>) =>
            (value: unknown): Effect.Effect<unknown, Failure, Model> =>
                Match.value(value).pipe(
                    Match.when(Schema.is(Value.Located), ({ specifier }) => Model.resolve(specifier)),
                    Match.when(Schema.is(Value.Enumerated), ({ type, member }) => Model.member(type, member)),
                    Match.when(Predicate.isString, typed(row)),
                    Match.when(Array.isArray, Effect.forEach(decode(row, within))),
                    Match.when(
                        Value.plainObject,
                        Effect.fnUntraced(function* (properties: Record.ReadonlyRecord<string, unknown>) {
                            const { declarations } = yield* Model;
                            const fields: Record.ReadonlyRecord<string, Typed> = yield* Option.match(within, { onNone: () => Effect.succeed(Record.empty<string, Typed>()), onSome: (kind) => Effect.map(Cache.get(declarations, kind), Struct.get('fields')) });
                            return yield* Effect.all(
                                Record.map(properties, (each, key) => {
                                    const field = Record.get(fields, key);
                                    return decode(field, Option.flatMap(field, Struct.get('type')))(each);
                                }),
                            );
                        }),
                    ),
                    Match.orElse(Effect.succeed),
                );
        return decode;
    };
    static readonly write: {
        (values: Record.ReadonlyRecord<string, unknown>): (target: InDesignDOMObject<unknown>) => Effect.Effect<readonly string[], Failure, Model>;
        (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>): Effect.Effect<readonly string[], Failure, Model>;
    } = Function.dual(
        2,
        Effect.fnUntraced(function* (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) {
            const specifier = target.toSpecifier();
            const write = writer(target, specifier, yield* Effect.cached(Model.fonts));
            const { declarations, collectionField } = yield* Model;
            const { fields } = yield* Cache.get(declarations, target.constructorName);
            const writable = Record.toEntries(Record.filter(fields, Predicate.not(collectionField)));
            return yield* Effect.validate(
                Object.entries(values),
                Effect.fnUntraced(function* ([wanted, value]: readonly [string, unknown]) {
                    const [, row] = yield* Unmatched.single(writable, `field of ${specifier}`, Option.some(wanted), flow(Tuple.get(0), Array.of));
                    return yield* write(row, value);
                }, Effect.sandbox),
            ).pipe(
                Effect.map(Array.flatten),
                Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
            );
        }),
    );
    static readonly extent = Effect.fn('Model.extent')(function* (...placed: readonly (Page | PageItem)[]) {
        const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
        return yield* Effect.try({
            try: () => {
                const corners: ReadonlyArray<readonly [number, number]> = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]) =>
                    each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
                );
                return Extent.make(Struct.makeReducer<typeof Extent.Encoded>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))));
            },
            catch: HostError.refused(`extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
        });
    });
    static readonly read = <S extends Schema.Top>(schema: S, subject: string, value: () => unknown) =>
        Model.encoder.pipe(
            Effect.flatMap((encoded) => Effect.try({ try: () => encoded(value()), catch: (cause) => (Cause.isIllegalArgumentError(cause) ? cause : HostError.refused(subject)(cause)) })),
            Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(schema))), Effect.orDie)),
        );
    static readonly picked = <S extends Schema.Top>(schema: S, object: InDesignDOMObject<unknown>, supplied: () => Record.ReadonlyRecord<string, unknown> = Record.empty) =>
        Effect.flatMap(Model, ({ picked }) =>
            Model.read(schema, object.toSpecifier(), () => {
                const given = supplied();
                return { ...picked(schema, object, Record.keys(given)), ...given };
            }),
        );
    static readonly commit = Effect.fnUntraced(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
        const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Model;
        const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
        const closed = (id: number) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
        yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(done, error)));
        return yield* Deferred.await(done).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), closed, { discard: true })));
    });
    static readonly written = <W, E, R>(label: string, target: InDesignDOMObject<unknown>, body: Effect.Effect<W, E, R>) => Model.commit(label, Effect.all([body, Model.reference(target)]).pipe(Effect.map(([members, reference]) => ({ ...reference, written: members }))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const writer = (target: InDesignDOMObject<unknown>, specifier: string, fonts: Parameters<typeof Model.decoder>[1]) => {
    const decode = Model.decoder(target, fonts);
    return Effect.fnUntraced(function* (row: typeof ObjectModel.Field.Type, value: unknown) {
        const field = row.name;
        const refused = HostError.refused(`${specifier} ${field}`);
        const nested = Value.plainObject(value) && !row.settable;
        const { domObject } = yield* Model;
        yield* Effect.transposeOption(Option.map(unavailableReason(target.constructorName, field, Option.some(target), !nested), refused));
        const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
        if (Value.plainObject(value) && domObject(inner)) {
            return Array.map(yield* Model.write(inner, value), (name) => `${field}.${name}`);
        }
        const native = yield* decode(Option.some(row), row.type)(value);
        yield* Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused });
        return [field];
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Model };
