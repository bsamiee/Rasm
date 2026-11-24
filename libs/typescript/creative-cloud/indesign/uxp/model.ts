import { Declaration, HostError, Reference, Selector, Specifier } from '@rasm/object-model';
import { encode, isPlain } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Settings } from '@rasm/typography';
import { Uxp } from '@rasm/uxp/plugin';
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
    type MenuAction,
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
import { Array, Cause, Context, Deferred, Effect, Equal, Equivalence, Function, flow, identity, Match, Number, Option, Predicate, Record, Result, Schema, SchemaAST, Struct } from 'effect';
import { Value } from '#/value';
import { Extent, type FindChange } from '#/views';

// --- [TYPES] ---------------------------------------------------------------------------

type Failure = HostError | Unmatched | Schema.SchemaError;
type Member<E extends Enumeration> = [Extract<E[keyof E], Enumerator>] extends [never] ? Enumerator : Extract<E[keyof E], Enumerator>;
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
} & Readonly<Record<`change${FindChange['mode']}`, InDesignDOMObject<unknown> & Pick<Document, `${'find' | 'change'}${FindChange['mode']}`>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const AGGREGATE_FIELD = 'properties';
const STYLISTIC_SETS = 20;
const STYLISTIC_SET_ACTION = 0x69_74;
const STYLISTIC_SET_ACTIONS = { minimum: STYLISTIC_SET_ACTION, maximum: STYLISTIC_SET_ACTION + STYLISTIC_SETS - 1 };

// --- [MODELS] --------------------------------------------------------------------------

const Declared = Schema.fromJsonString(Declaration.Model);

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/uxp/model', {
    make: (text: string) =>
        Effect.map(Schema.decodeEffect(Declared)(text), (declared) => {
            const inheriting: <T extends object>(prototype: T) => T = Object.create;
            const domObject = owning('toSpecifier');
            const document = owning('createTOC');
            const application = owning('menuActions');
            const ancestors = (object: unknown, link: 'parent' | 'basedOn' = 'parent'): readonly InDesignDOMObject<unknown>[] => Array.unfold(object, (above: unknown) => (domObject(above) && !application(above) ? Option.some([above, Reflect.get(above, link)] as const) : Option.none()));
            const fits =
                (value: unknown) =>
                (member: SchemaAST.AST): boolean =>
                    SchemaAST.isObjects(member) ? Predicate.isObject(value) && !Array.isArray(value) && Option.match(tagged(member), { onNone: () => true, onSome: (tag) => domObject(value) && value.constructorName === tag }) : SchemaAST.isArrays(member) && (Array.isArray(value) || collection(value));
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
                        Option.filter((property) => !(Predicate.isNullish(property) || nothing(property))),
                        Option.map((property) => [name, picking(type, property)] as const),
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
                    Match.when(SchemaAST.isObjects, (node) => (Predicate.isObjectOrArray(value) && !collection(value) && Array.isReadonlyArrayNonEmpty(node.propertySignatures) ? record(node, value, skipped) : value)),
                    Match.orElse(() => value),
                );
            return Object.assign(inheriting(indesign), {
                owning,
                domObject,
                collection,
                picked: (schema: Schema.Top, value: object, skipped: readonly PropertyKey[] = []): Record.ReadonlyRecord<string, unknown> => Option.getOrElse(Option.liftPredicate(picking(SchemaAST.toEncoded(schema.ast), value, skipped), isPlain), Record.empty),
                named: owning('name'),
                application,
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
                declared,
                declaration: (kind: string) => Option.getOrElse(Record.get(declared.classes, kind), () => Declaration.Model.fields.classes.value.make({ fields: {}, methods: {} })),
                enumeration,
                enumerator,
                nothing,
                members,
            });
        }),
}) {
    static readonly current = Model.use(({ app, document }) => {
        const open = app.documents.everyItem().getElements();
        return Option.filter(Array.head(open), () => open.length === 1).pipe(
            Option.orElse(() => Option.flatMap(Option.liftThrowable((): unknown => app.activeWindow.parent)(), Option.liftPredicate(document))),
            Effect.fromOption(() => Unmatched.of({ subject: 'document', candidates: Array.map(open, (each) => each.toSpecifier()) })),
        );
    });
    static readonly selection = Model.use(({ app }) => Effect.try({ try: () => (app.windows.length === 0 ? [] : app.selection), catch: HostError.refused('selection') }).pipe(Effect.flatMap(Unmatched.single('selection', Option.none(), (item) => [item.toSpecifier()]))));
    static readonly file = (owner: InDesignDOMObject<unknown>, field: string) =>
        Model.use(({ app, ScriptLanguage }) => Effect.try({ try: (): unknown => app.doScript('var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', ScriptLanguage.JAVASCRIPT, [owner.toSpecifier(), field]), catch: HostError.refused(`${owner.toSpecifier()} ${field}`) })).pipe(
            Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.OptionFromNullOr(Schema.String)), Effect.orDie)),
        );
    static readonly member = Effect.fnUntraced(function* (kind: string, name: string) {
        const { enumeration, members, declared } = yield* Model;
        const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.of({ subject: 'enumeration', query: Option.some(kind), candidates: declared.enumerations }));
        return yield* Unmatched.single(members(found), `${kind} member`, Option.some(name), (member) => [member.toString()]);
    });
    static readonly object = Effect.fnUntraced(function* (kind: string, name: string, target: unknown) {
        const model = yield* Model;
        const document = yield* Option.match(model.owner(target), { onSome: Effect.succeedSome, onNone: () => Effect.option(Model.current) });
        const fields = Option.match(document, { onNone: Record.empty, onSome: ({ constructorName }) => model.declaration(constructorName).fields });
        const field = Record.findFirst(fields, ({ array }) => Option.contains(array, kind)).pipe(Option.orElse(() => Record.findFirst(fields, ({ collection }) => Option.contains(collection, kind))));
        const held = Option.zipWith(document, field, (owner, [named]): unknown => Reflect.get(owner, named));
        const listed = Option.map(held, (members): readonly unknown[] => (model.collection(members) ? members.everyItem().getElements() : Array.ensure(members)));
        return yield* Option.match(listed, { onNone: () => Effect.succeed<unknown>(name), onSome: (members) => Unmatched.single(members.filter(model.named), kind, Option.some(name), (each) => [each.toSpecifier(), each.name]) });
    });
    static readonly unavailable = (object: InDesignDOMObject<unknown>, field: string, writing: boolean): Option.Option<string> =>
        Match.value({ kind: object.constructorName, field, writing, stylistic: Option.liftPredicate(object, (each): each is MenuAction => each.constructorName === 'MenuAction').pipe(Option.exists(({ id }) => Number.between(id, STYLISTIC_SET_ACTIONS))) }).pipe(
            Match.when({ kind: Match.is('FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'), field: Match.is('angle', 'distance', 'xOffset', 'yOffset', AGGREGATE_FIELD) }, () => 'Shadow fields crash native Transparency provider'),
            Match.when({ kind: 'EventListener', field: Match.is('handler', AGGREGATE_FIELD), writing: false }, () => 'Ended UXP context handlers crash InDesign'),
            Match.when({ kind: 'MenuAction', field: Match.is('enabled', 'checked', AGGREGATE_FIELD), writing: false, stylistic: true }, () => 'Stylistic Set state crashes native CharacterPanel provider'),
            Match.option,
        );
    static readonly properties = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>) {
        const [{ fields }, specifier] = [(yield* Model).declaration(object.constructorName), object.toSpecifier()];
        return yield* Option.match(Model.unavailable(object, AGGREGATE_FIELD, false), {
            onNone: () => Effect.try({ try: (): Record.ReadonlyRecord<string, unknown> => object.properties, catch: HostError.refused(`${specifier} ${AGGREGATE_FIELD}`) }),
            onSome: () =>
                Effect.all(Record.filterMap(fields, ({ collection }, name) => (Option.isSome(collection) || Option.isSome(Model.unavailable(object, name, false)) ? Result.failVoid : Result.succeed(Effect.try({ try: (): unknown => Reflect.get(object, name), catch: HostError.refused(`${specifier} ${name}`) }))))),
        });
    });
    static readonly saved = (object: InDesignDOMObject<unknown>) =>
        Model.useSync(({ declaration }) =>
            Record.filterMap(declaration(object.constructorName).fields, ({ settable }, name) =>
                settable && name !== AGGREGATE_FIELD && Option.isNone(Model.unavailable(object, name, false)) ? Result.fromOption(Option.filter(Option.liftThrowable((): unknown => Reflect.get(object, name))(), Predicate.not(nothing)), Function.constVoid) : Result.failVoid,
            ),
        );
    static readonly fonts = Model.use(({ app, FontStatus }) => {
        const missing = (open: Document) => {
            const used = open.fonts.everyItem();
            return Array.filterMap(Array.zip(used.name, used.status), ([label, status], index) => (FontStatus.INSTALLED.equals(status) ? Result.failVoid : Result.succeed({ names: Array.of(label), font: (): Font => open.fonts.item(index) })));
        };
        return Effect.try({
            try: () => {
                const every = app.fonts.everyItem();
                const faces = Array.map(Array.zip(Array.zip(every.name, every.postscriptName), Array.zip(every.fullName, Array.zip(every.fontFamily, every.fontStyleName))), ([[label, postscript], [full, [family, style]]], index) => ({ label, postscript, full, family, style, index }));
                const regular = Record.map(Array.groupBy(faces, Struct.get('family')), (listed) => Array.findFirst(listed, Predicate.Struct({ style: Equal.equals('Regular') })).pipe(Option.getOrElse(Function.constant(Array.headNonEmpty(listed)))).index);
                const installed = Array.map(Array.dedupeWith(faces, Equivalence.mapInput(Equivalence.String, Struct.get('postscript'))), ({ label, postscript, full, family, index }) => ({
                    names: Array.make(label, postscript, full, ...(Option.contains(Record.get(regular, family), index) ? [family] : [])),
                    font: (): Font => app.fonts.item(index),
                }));
                const candidates = [...installed, ...Array.flatMap(app.documents.everyItem().getElements(), missing)];
                return (name: string): Effect.Effect<Font, Unmatched> => Effect.map(Unmatched.single(candidates, 'font', Option.some(name), Struct.get('names')), ({ font }) => font());
            },
            catch: HostError.refused('fonts'),
        });
    });
    static readonly resolve = Effect.fn('Model.resolve')(function* (specifier: Specifier) {
        const [model, written] = [yield* Model, specifier.path];
        const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = identified(path);
            const valid = (each: unknown): each is InDesignDOMObject<unknown> => model.domObject(each) && each.isValid && (Option.isNone(requested) || Equal.equals(identified(each.toSpecifier()), requested));
            return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (resolved): resolved is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
                Array.isArray(resolved) ? resolved.every(valid) : valid(resolved),
            );
        };
        const nearest = Effect.suspend(() => {
            const listed = flow(located, Option.toArray, Array.flatMap(Array.ensure));
            const siblings = listed(specifier.unselected);
            const container = Array.head(listed(specifier.parent));
            const fields = Option.match(container, { onNone: Array.empty, onSome: (at) => Record.toEntries(model.declaration(at.constructorName).fields) });
            const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at): unknown => Reflect.get(at, field)));
            const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
            const related = fields.flatMap(([field, { array }]) => (Option.isSome(sibling) && Equal.equals(array, sibling) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
            const held = Array.filterMap(fields, ([field, { collection }]) => (Option.isSome(collection) ? Result.succeed(value(field)) : Result.failVoid)).filter(model.collection);
            const collections = Array.flatMapNullishOr(held, (items) => items.everyItem());
            const candidates = Array.dedupe(Array.map([...siblings, ...related.filter((each) => Option.contains(specifier.name, each.name))], (each) => each.toSpecifier()));
            const unmatched = Option.exists(specifier.selector, Selector.guards.Index) ? { subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: Option.some(written) } : { subject: 'object', query: Option.some(written), candidates };
            return Array.findFirst(collections, (every) => (every.toSpecifier() === written ? Option.some(every.getElements()) : Option.none())).pipe(Effect.fromOption(() => Unmatched.of(unmatched)));
        });
        const absent = specifier.end === undefined ? nearest : Effect.fail(Unmatched.of({ subject: 'range', query: Option.some(written) }));
        return yield* Option.match(located(written), { onSome: (resolved) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) || specifier.property ? resolved : Array.ensure(resolved)), onNone: () => absent });
    });
    static readonly located = Effect.fnUntraced(function* (specifier: Specifier) {
        const [model, found] = yield* Effect.all([Model, Model.resolve(specifier)]);
        return model.domObject(found) ? found : yield* Unmatched.of({ subject: 'object', query: Option.some(specifier.path) });
    });
    static readonly referenceUnsafe = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid ? Option.liftPredicate(Reflect.get(object, 'name'), Predicate.isString) : Option.none(), valid });
    };
    static readonly collected = (items: Owned['everyItem']) => ({ type: items.constructor.name, length: items.length, specifier: Option.map(Option.fromNullishOr(items.everyItem()), (every) => every.toSpecifier()) });
    static readonly reference = (object: InDesignDOMObject<unknown>): Effect.Effect<Reference, HostError> => Effect.try({ try: () => Model.referenceUnsafe(object), catch: HostError.refused('object reference') });
    static readonly encoder = Effect.map(Model, ({ enumerator, collection, domObject }) => {
        const host = {
            enumerator: (value: object) => (enumerator(value) ? String(value) : undefined),
            collection: (value: object) =>
                Option.liftPredicate(value, collection).pipe(
                    Option.map(Model.collected),
                    Option.map(({ specifier, ...held }) => ({ ...held, ...Record.getSomes({ specifier }) })),
                    Option.getOrUndefined,
                ),
            reference: (value: object) =>
                Option.liftPredicate(value, domObject).pipe(
                    Option.map(Model.referenceUnsafe),
                    Option.map(({ name, ...held }) => ({ ...held, ...Record.getSomes({ name }) })),
                    Option.getOrUndefined,
                ),
            replace: Match.type<unknown>().pipe(
                Match.when(Option.isOption, Option.getOrUndefined),
                Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                Match.when(Predicate.isPromise, () => {
                    throw new Cause.IllegalArgumentError('Result holds a promise, scripts answer before it settles');
                }),
                Match.when(Schema.is(Uxp.Entry), ({ url }) => String(url)),
                Match.when(Schema.is(Settings), flow(Schema.encodeOption(Schema.toCodecJson(Settings)), Option.getOrUndefined)),
                Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                Match.when(
                    Predicate.isMap,
                    Record.fromIterableWith(([name, value]) => [String(name), value] as const),
                ),
                Match.when(Predicate.isSet, (entries) => [...entries]),
                Match.when(Predicate.isBigInt, String),
                Match.orElse(identity),
            ),
        };
        return (value: unknown): string | undefined => JSON.stringify(encode(value, host));
    });
    static readonly decode =
        (target: unknown, named: readonly string[]) =>
        (value: unknown): Effect.Effect<unknown, Failure, Model> =>
            Match.value(value).pipe(
                Match.when(Schema.is(Value.Located), ({ specifier }) => Model.resolve(specifier)),
                Match.when(Schema.is(Value.Enumerated), ({ type, member }) => Model.member(type, member)),
                Match.when(Predicate.isString, (text) =>
                    Array.match(named, { onEmpty: () => Effect.succeed<unknown>(text), onNonEmpty: (kinds) => firstOf(Array.map(kinds, (kind): Effect.Effect<unknown, Failure, Model> => (Option.isSome(enumeration(kind)) ? Model.member(kind, text) : Model.object(kind, text, target)))) }),
                ),
                Match.when(Array.isArray, Effect.forEach(Model.decode(target, named))),
                Match.when(isPlain, (properties) => Effect.all(Record.map(properties, Model.decode(target, [])))),
                Match.orElse(Effect.succeed),
            );
    static readonly write: {
        (values: Record.ReadonlyRecord<string, unknown>): (target: InDesignDOMObject<unknown>) => Effect.Effect<Effect.Effect<readonly string[], HostError>, Failure, Model>;
        (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>): Effect.Effect<Effect.Effect<readonly string[], HostError>, Failure, Model>;
    } = Function.dual(2, (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.map(decoded(target, values), (writes) => Effect.validate(writes, ([name, write]) => Effect.sandbox(Effect.as(write, name))).pipe(Effect.catch(accumulated))));
    static readonly extent = Effect.fn('Model.extent')(function* (...placed: readonly (Page | PageItem)[]) {
        const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
        return yield* Effect.try({
            try: () => {
                const corners: readonly (readonly [number, number])[] = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]: readonly [Page | PageItem, (typeof AnchorPoint)['TOP_LEFT_ANCHOR' | 'BOTTOM_RIGHT_ANCHOR']]) =>
                    each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
                );
                return Extent.make(Struct.makeReducer({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))));
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
        const [opened, outcome, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
        const closed = (id: number) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
        yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, outcome), Deferred.await(outcome))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(outcome, error)));
        return yield* Deferred.await(outcome).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), closed, { discard: true })));
    });
    static readonly written = <W, E, R>(label: string, target: InDesignDOMObject<unknown>, body: Effect.Effect<W, E, R>): Effect.Effect<Omit<Reference, 'brand'> & { readonly written: W }, E | HostError, R | Model> =>
        Model.commit(
            label,
            Effect.zipWith(body, Model.reference(target), (members, reference) => ({ ...reference, written: members })),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const owning =
    <K extends keyof Owned>(member: K) =>
    (value: unknown): value is Owned[K] =>
        Predicate.hasProperty(value, 'toSpecifier') && Predicate.hasProperty(value, member);
const collection = (value: unknown): value is Owned['everyItem'] => Predicate.hasProperty(value, 'everyItem');
const enumerator = (value: unknown): value is Enumerator => value instanceof indesign.Enumerator;
const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
const members = <E extends Enumeration>(found: E) => Array.filterMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => Result.liftPredicate<unknown, void, Member<E>>(Reflect.get(found, key), (member): member is Member<E> => enumerator(member) && member.toString() === key, Function.constVoid));
const nothing = (value: unknown): boolean => enumerator(value) && indesign.NothingEnum.NOTHING.equals(value);
const tagged = ({ propertySignatures }: SchemaAST.Objects) => Array.findFirst(propertySignatures, ({ name, type }) => (name === '_tag' && SchemaAST.isLiteral(type) ? Option.some(type.literal) : Option.none()));
const entry = (node: SchemaAST.Arrays, index: number): SchemaAST.AST =>
    Array.get(node.elements, index).pipe(
        Option.orElse(() => Array.head(node.rest)),
        Option.getOrElse(() => Schema.Unknown.ast),
    );
const identified = (path: string) =>
    Schema.decodeOption(Specifier.Path)(path).pipe(
        Option.filter(({ end, selector }) => end === undefined && Option.exists(selector, (step) => Selector.guards.Key(step) && step.key === 'id')),
        Option.flatMap(({ steps }) => Array.last(steps)),
        Option.map(Struct.get('kind')),
    );
const joined =
    <E>(failed: E | Unmatched) =>
    (later: E | Unmatched): E | Unmatched => {
        if (!Schema.is(Unmatched)(later)) {
            return later;
        }
        return Schema.is(Unmatched)(failed) ? Unmatched.of({ subject: `${failed.subject} or ${later.subject}`, query: failed.query, candidates: [...failed.candidates, ...later.candidates] }) : failed;
    };
const accumulated = <E>(causes: readonly Cause.Cause<E>[]) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))));
const decoded = Effect.fnUntraced(function* (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>): Effect.fn.Return<readonly (readonly [string, Effect.Effect<unknown, HostError>])[], Failure, Model> {
    const [{ declaration, domObject }, specifier] = [yield* Model, target.toSpecifier()];
    const { fields } = declaration(target.constructorName);
    return yield* Effect.validate(
        Object.entries(values),
        Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
            const refused = HostError.refused(`${specifier} ${field}`);
            yield* Predicate.hasProperty(target, field) ? Effect.void : Unmatched.of({ subject: `field of ${specifier}`, query: Option.some(field), candidates: Record.keys(fields) });
            yield* Effect.transposeOption(Option.map(Model.unavailable(target, field, !isPlain(value)), refused));
            const inner: unknown = isPlain(value) ? yield* Effect.try({ try: (): unknown => Reflect.get(target, field), catch: refused }) : undefined;
            if (isPlain(value) && domObject(inner)) {
                return Array.map(yield* decoded(inner, value), ([name, write]) => [`${field}.${name}`, write] as const);
            }
            const native = yield* Model.decode(target, Option.match(Record.get(fields, field), { onNone: Array.empty<string>, onSome: Struct.get('classes') }))(value);
            return [[field, Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused })] as const];
        }, Effect.sandbox),
    ).pipe(Effect.map(Array.flatten), Effect.catch(accumulated));
});
const firstOf = <A, E, R>([head, ...rest]: Array.NonEmptyReadonlyArray<Effect.Effect<A, E, R>>) => rest.reduce<Effect.Effect<A, E | Unmatched, R>>((chosen, fallback) => Effect.catch(chosen, (failed) => Effect.mapError(fallback, joined(failed))), head);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Model, STYLISTIC_SETS };
