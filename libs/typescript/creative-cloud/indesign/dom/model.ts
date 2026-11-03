import type {
    Application,
    BaseCollection,
    CellStyle,
    CharacterStyle,
    Document,
    Enumeration,
    Enumerator,
    Font,
    Gradient,
    InDesignDOMObject,
    Layer,
    Link,
    MasterSpread,
    MixedInk,
    MixedInkGroup,
    ObjectStyle,
    Page,
    PageItem,
    ParagraphStyle,
    PlainColor,
    PlainSwatch,
    PreflightProcess,
    ReadonlyNamedDOMObject,
    Spread,
    Story,
    TableStyle,
    Text,
    TextDefault,
    TextFrame,
    Tint,
} from 'adobe:indesign';
import { HostError, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Cause, Context, Duration, Effect, Equal, Equivalence, Exit, flow, identity, Match, Option, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import { Dictionary } from '#/dom/dictionary';
import { Specifier } from '#/dom/specifier';
import { Value } from '#/dom/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Typed {
    readonly types: Option.Option<readonly string[]>;
    readonly type: Option.Option<string>;
}
type Failure = HostError | Unmatched | Schema.SchemaError;

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/dom/model', {
    make: Effect.fnUntraced(function* (indesign: typeof import('adobe:indesign')) {
        const model: typeof indesign = Object.create(indesign);
        const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
        const classes = Array.filterMap(exported, ([name, value]) => (Predicate.isFunction(value) ? Result.succeed(name) : Result.failVoid));
        const enumerations = Array.filterMap(exported, ([name, value]) => (value instanceof indesign.Enumeration ? Result.succeed(name) : Result.failVoid));
        const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
        const enumerator = (value: unknown): value is Enumerator => value instanceof indesign.Enumerator;
        const members = <E extends Enumeration>(found: E) => {
            const keyed = (member: unknown, key: string): member is Extract<E[keyof E], Enumerator> => enumerator(member) && String(member) === key;
            return Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
                const member: unknown = Reflect.get(found, key);
                return keyed(member, key) ? [member] : [];
            });
        };
        const owning = <A>(member: keyof A & string) => {
            const owners = new Set(Array.filterMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? Result.succeed(type.prototype) : Result.failVoid)));
            return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
        };
        const domObject = owning<InDesignDOMObject<unknown>>('toSpecifier');
        const document = owning<Document>('createTOC');
        const ancestors = (object: unknown): readonly InDesignDOMObject<unknown>[] => Array.unfold(object, (above: unknown) => (domObject(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()));
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
                const { fields, methods, ...declared } = yield* Effect.fromOption(yield* Dictionary.read(indesign, kind), () => Unmatched.make({ subject: 'class', query: Option.some(kind), candidates: classes }));
                return {
                    ...declared,
                    fields: Record.map(fields, (row) => ({ ...filled(row), unavailable: Dictionary.unavailable(kind, row.name, Option.none(), false) })),
                    methods: Record.map(methods, (method) => ({ ...method, parameters: Array.map(method.parameters, filled) })),
                };
            }),
            retried,
        );
        const parents = (name: string): Effect.Effect<readonly string[], Unmatched | HostError> => Effect.flatMap(Cache.get(declarations, name), ({ parent }) => Option.match(parent, { onNone: () => Effect.succeed([]), onSome: (above) => Effect.map(parents(above), (rest) => [above, ...rest]) }));
        const element = (kind: string) => Effect.map(Cache.get(declarations, kind), ({ methods }) => Option.flatMap(Record.get(methods, 'item'), ({ returns }) => Option.map(returns, Struct.get('type'))));
        const documentField = yield* Cache.makeWith(
            Effect.fnUntraced(function* (kind: string) {
                const { fields } = yield* Cache.get(declarations, 'Document');
                const listed = Array.findFirst(Record.toEntries(fields), ([, { array, type }]) => array && Option.contains(type, kind));
                const held = Effect.findFirst(Record.toEntries(Record.filter(fields, collectionField)), ([, { type }]) => Effect.map(Effect.transposeOption(Option.map(type, element)), flow(Option.flatten, Option.contains(kind))));
                return Option.map(Option.isSome(listed) ? listed : yield* held, ([field]) => field);
            }),
            retried,
        );
        return Object.assign(model, {
            exported,
            classes,
            enumerations,
            owning,
            domObject,
            collection: owning<BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }>>('everyItem'),
            named: owning<ReadonlyNamedDOMObject<unknown>>('name'),
            application: owning<Application>('menuActions'),
            document,
            page: owning<Page>('documentOffset'),
            spread: owning<Spread>('spreadHidden'),
            masterSpread: owning<MasterSpread>('namePrefix'),
            layer: owning<Layer>('layerColor'),
            link: owning<Link>('linkResourceURI'),
            swatch: owning<PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup>('parentColorGroup'),
            item: owning<PageItem>('itemLayer'),
            story: owning<Story>('textContainers'),
            textContainer: owning<Story>('texts'),
            text: owning<Text>('parentTextFrames'),
            textDefault: (value: unknown): value is TextDefault => domObject(value) && value.constructorName === 'TextDefault',
            frame: owning<TextFrame>('textFrameIndex'),
            style: owning<ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle>('basedOn'),
            paragraphStyle: owning<ParagraphStyle>('nestedGrepStyles'),
            overridable: owning<Text>('textHasOverrides'),
            storied: owning<Text>('parentStory'),
            font: owning<Font>('postscriptName'),
            preflightProcess: owning<PreflightProcess>('aggregatedResults'),
            ancestors,
            owner: (object: unknown): Option.Option<Document> => Array.findFirst(ancestors(object), document),
            enumeration,
            enumerator,
            nothing: (value: unknown): boolean => enumerator(value) && indesign.NothingEnum.NOTHING.equals(value),
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
        return pipe(
            Array.head(open),
            Option.filter(() => open.length === 1),
            Option.orElse(() => shown),
            Effect.fromOption(() => Unmatched.make({ subject: 'document', candidates: Array.map(open, (each) => each.toSpecifier()) })),
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
        return yield* Option.match(Option.all([scope, field]), {
            onNone: () => Effect.succeed<unknown>(name),
            onSome: ([document, member]) => {
                const listed: unknown = Reflect.get(document, member);
                return Unmatched.single((model.collection(listed) ? listed.everyItem().getElements() : Array.ensure(listed)).filter(model.named), type, Option.some(name), (each) => [each.toSpecifier(), each.name]);
            },
        });
    });
    static readonly fonts = Model.use(({ app, FontStatus }) =>
        Effect.try({
            try: () => {
                const every = app.fonts.everyItem();
                const faces = Array.map(Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))), ([[label, postscript], [full, [family, style]]], index) => ({ label, postscript, full, family, style, index }));
                const regular = Record.map(Array.groupBy(faces, Struct.get('family')), (listed) => Array.findFirst(listed, ({ style }) => style === 'Regular').pipe(Option.getOrElse(() => Array.headNonEmpty(listed))).index);
                const installed = Array.map(Array.dedupeWith(faces, Equivalence.mapInput(Equivalence.String, Struct.get('postscript'))), ({ label, postscript, full, family, index }) => ({
                    names: Array.make(label, postscript, full, ...(Option.contains(Record.get(regular, family), index) ? [family] : [])),
                    font: (): Font => app.fonts.item(index),
                }));
                const missing = Array.flatMap(app.documents.everyItem().getElements(), (open) => {
                    const used = open.fonts.everyItem();
                    return Array.filterMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? Result.failVoid : Result.succeed({ names: Array.of(label), font: (): Font => open.fonts.item(index) })));
                });
                const candidates = [...installed, ...missing];
                return (name: string): Effect.Effect<Font, Unmatched> => Effect.map(Unmatched.single(candidates, 'font', Option.some(name), Struct.get('names')), ({ font }) => font());
            },
            catch: HostError.refused('fonts'),
        }),
    );
    static readonly resolve = Effect.fn('Model.resolve')(function* (specifier: Specifier) {
        const [model, written] = yield* Effect.all([Model, specifier.path]);
        const identified = (path: string): Option.Option<string> =>
            Option.flatMap(Schema.decodeUnknownOption(Specifier.Path)(path), ({ kind, selector, end }) =>
                Option.as(
                    Option.filter(selector, (each) => Option.isNone(end) && Reference.Selector.guards.Identified(each)),
                    kind,
                ),
            );
        const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = identified(path);
            return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
                Array.ensure<unknown>(found).every((each) => model.domObject(each) && each.isValid && (Option.isNone(requested) || Equal.equals(identified(each.toSpecifier()), requested))),
            );
        };
        const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
        const nearest = Effect.fnUntraced(function* () {
            const siblings = listed(`${specifier.container}${specifier.separator}${specifier.kind}`);
            const container = Array.head(listed(specifier.container || '/'));
            const fields = (yield* Effect.forEach(Option.toArray(container), (at) => Cache.get(model.declarations, at.constructorName))).flatMap(({ fields: declared }) => Record.toEntries(declared));
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
        });
        return yield* Option.match(located(written), {
            onSome: (found) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) || specifier.property ? found : Array.ensure(found)),
            onNone: () => (Option.isSome(specifier.end) ? Effect.fail(Unmatched.make({ subject: 'range', query: Option.some(written) })) : nearest()),
        });
    });
    static readonly located = (specifier: Specifier) => Model.use(({ domObject }) => Effect.filterOrElse(Model.resolve(specifier), domObject, () => Effect.flatMap(specifier.path, (written) => Unmatched.make({ subject: 'object', query: Option.some(written) }))));
    static readonly referenceUnsafe = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid ? Option.liftPredicate(Reflect.get(object, 'name'), Predicate.isString) : Option.none(), valid });
    };
    static readonly reference = (object: InDesignDOMObject<unknown>): Effect.Effect<Reference, HostError> => Effect.try({ try: () => Model.referenceUnsafe(object), catch: HostError.refused('object reference') });
    static readonly encoder = Effect.map(
        Model,
        ({ enumerator, collection, domObject }) =>
            (value: unknown): string | undefined =>
                JSON.stringify(value, function (this: object, key: string, part: unknown) {
                    const held: unknown = Reflect.get(this, key);
                    return Match.value(held).pipe(
                        Match.when(Option.isOption, Option.getOrUndefined),
                        Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                        Match.orElse(() => part),
                        Match.value,
                        Match.when(enumerator, String),
                        Match.when(collection, (items) => ({ type: items.constructor.name, specifier: items.everyItem().toSpecifier(), length: items.length })),
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
                }),
    );
    static readonly decoder = (target: unknown, fonts: Effect.Effect<(name: string) => Effect.Effect<Font, Unmatched>, HostError, Model>) => {
        const joined = <E>(failed: E | Unmatched, later: E | Unmatched): E | Unmatched => {
            if (!Schema.is(Unmatched)(later)) {
                return later;
            }
            return Schema.is(Unmatched)(failed) ? Unmatched.make({ subject: `${failed.subject} or ${later.subject}`, query: failed.query, candidates: [...failed.candidates, ...later.candidates] }) : failed;
        };
        const firstOf = <A, E, R>([head, ...rest]: Array.NonEmptyReadonlyArray<Effect.Effect<A, E, R>>) => rest.reduce<Effect.Effect<A, E | Unmatched, R>>((chosen, next) => Effect.catch(chosen, (failed) => Effect.mapError(next, (later) => joined(failed, later))), head);
        const typed = (row: Option.Option<Typed>) =>
            Effect.fnUntraced(function* (text: string) {
                const { enumeration, classes, owner } = yield* Model;
                const types = Option.getOrElse(Option.flatMap(row, Struct.get('types')), Array.empty<string>);
                const scope = Option.match(owner(target), { onSome: (document) => Effect.succeedSome<object>(document), onNone: () => Effect.option(Model.current) });
                const named = types.includes('String')
                    ? [Effect.succeed<unknown>(text)]
                    : Array.map(
                          types.filter((type) => type === 'Font' || classes.includes(type)),
                          (type): Effect.Effect<unknown, Failure, Model> => (type === 'Font' ? Effect.flatMap(fonts, (font) => font(text)) : Effect.flatMap(scope, (found) => Model.object(type, text, found))),
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
                    Match.when(Array.isArray, (items) => Effect.forEach(items, decode(row, within))),
                    Match.when(
                        Value.plainObject,
                        Effect.fnUntraced(function* (properties: Record.ReadonlyRecord<string, unknown>) {
                            const { declarations } = yield* Model;
                            const fields: Record.ReadonlyRecord<string, Typed> = Option.isSome(within) ? (yield* Cache.get(declarations, within.value)).fields : {};
                            return yield* Effect.all(Record.map(properties, (each, key) => decode(Record.get(fields, key), Option.flatMap(Record.get(fields, key), Struct.get('type')))(each)));
                        }),
                    ),
                    Match.orElse(Effect.succeed),
                );
        return decode;
    };
    static readonly read = <S extends Schema.Top>(schema: S, subject: string, value: () => unknown) =>
        Model.encoder.pipe(
            Effect.flatMap((encoded) => Effect.try({ try: () => encoded(value()), catch: (cause) => (Cause.isIllegalArgumentError(cause) ? cause : HostError.refused(subject)(cause)) })),
            Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(schema))), Effect.orDie)),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Model };
