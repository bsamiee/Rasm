import { type Encoder, encode, isPlain } from '@rasm/object-model/host';
import { Declaration, HostError, Reference, Selector, Specifier } from '@rasm/object-model/model';
import { Unmatched } from '@rasm/search/unmatched';
import { Settings } from '@rasm/typography/settings';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import indesign, {
    type AnySwatch,
    type Application,
    type BaseCollection,
    type CellStyle,
    type CharacterStyle,
    type Document,
    type Enumeration,
    type Enumerator,
    type Event,
    type EventListener,
    type EventTargetDOMObject,
    type FindChangeDropShadowSetting,
    type FindChangeInnerShadowSetting,
    type Font,
    type InDesignDOMObject,
    type InDesignEventMap,
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
import { Array, Cause, Context, Deferred, Effect, Equal, Equivalence, Function, flow, identity, Match, Number, Option, Predicate, PubSub, Record, Result, Schema, SchemaAST, Stream, Struct } from 'effect';
import { Value } from '#/value';
import { Extent, type FindChange } from '#/views';

// --- [TYPES] ---------------------------------------------------------------------------

type Failure = HostError | Unmatched | Schema.SchemaError;
type Member<E extends Enumeration> = [Extract<E[keyof E], Enumerator>] extends [never] ? Enumerator : Extract<E[keyof E], Enumerator>;
type Owned = {
    readonly toSpecifier: InDesignDOMObject<unknown>;
    readonly createTOC: Document;
    readonly everyItem: BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }> & Pick<InDesignDOMObject<unknown>, 'constructorName'>;
    readonly name: ReadonlyNamedDOMObject<unknown>;
    readonly menuActions: Application;
    readonly invoke: MenuAction;
    readonly handler: EventListener;
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

const PROPERTIES = 'properties' satisfies keyof InDesignDOMObject<unknown>;
const STYLISTIC_SETS = 20;
const STYLISTIC_SET_ACTIONS = { minimum: 26_996, maximum: 26_996 + STYLISTIC_SETS - 1 };

// --- [MODELS] --------------------------------------------------------------------------

const DeclarationJson = Schema.fromJsonString(Declaration.Model);

// --- [SERVICES] ------------------------------------------------------------------------

class Dom extends Context.Service<Dom>()('@rasm/indesign/uxp/dom', {
    make: (text: string) =>
        Effect.zipWith(Schema.decodeEffect(DeclarationJson)(text), PubSub.unbounded<readonly [string, Event]>(), (declared, hub) => {
            const inherit: <T extends object>(prototype: T) => T = Object.create;
            const domObject = owning('toSpecifier');
            const document = owning('createTOC');
            const application = owning('menuActions');
            const ancestors = (object: unknown, link: keyof Pick<Owned['basedOn'], 'parent' | 'basedOn'> = 'parent'): readonly InDesignDOMObject<unknown>[] => Array.unfold(object, (above: unknown) => (domObject(above) && !application(above) ? Option.some([above, Reflect.get(above, link)] as const) : Option.none()));
            const fits =
                (value: unknown) =>
                (member: SchemaAST.AST): boolean =>
                    SchemaAST.isObjects(member) ? Predicate.isObject(value) && !Array.isArray(value) && Option.match(readTag(member), { onNone: () => true, onSome: (tag) => domObject(value) && value.constructorName === tag }) : SchemaAST.isArrays(member) && (Array.isArray(value) || collection(value));
            const record = (node: SchemaAST.Objects, value: object, skipped: readonly PropertyKey[]) => {
                const isReferenced = domObject(value) && node.propertySignatures.some(({ name }) => name === ('specifier' satisfies keyof typeof Reference.fields));
                const read = ({ name, type }: SchemaAST.PropertySignature) =>
                    name === '_tag' && SchemaAST.isLiteral(type)
                        ? Option.some([name, type.literal] as const)
                        : Option.liftPredicate(name, (field) => !(skipped.includes(field) || (isReferenced && Object.hasOwn(Reference.fields, field)))).pipe(
                              Option.flatMap(Option.liftThrowable((field): unknown => Reflect.get(value, field))),
                              Option.filter((property) => !(Predicate.isNullish(property) || nothing(property))),
                              Option.map((property) => [name, pickValue(type, property)] as const),
                          );
                return Object.fromEntries([...(isReferenced ? Object.entries(Dom.referenceUnsafe(value)) : []), ...Array.getSomes(Array.map(node.propertySignatures, read))]);
            };
            const list = (node: SchemaAST.Arrays, value: unknown) => {
                const items: unknown = collection(value) ? value.everyItem().getElements() : value;
                return Array.isArray(items) ? items.map((item: unknown, index) => pickValue(entry(node, index), item)) : value;
            };
            const pickValue = (ast: SchemaAST.AST, value: unknown, skipped: readonly PropertyKey[] = []): unknown =>
                Match.value(ast).pipe(
                    Match.when(SchemaAST.isSuspend, (node) => pickValue(node.thunk(), value)),
                    Match.when(SchemaAST.isUnion, (node) => Option.match(Array.findFirst(node.types, fits(value)), { onNone: () => value, onSome: (member) => pickValue(member, value) })),
                    Match.when(SchemaAST.isArrays, (node) => list(node, value)),
                    Match.when(SchemaAST.isObjects, (node) => (Predicate.isObjectOrArray(value) && !collection(value) && Array.isReadonlyArrayNonEmpty(node.propertySignatures) ? record(node, value, skipped) : value)),
                    Match.orElse(() => value),
                );
            const encoding: Encoder = {
                enumerator: (value) => (enumerator(value) ? String(value) : undefined),
                collection: (value) =>
                    Option.liftPredicate(value, collection).pipe(
                        Option.map(Dom.collect),
                        Option.map(({ specifier, ...held }) => ({ ...held, ...Record.getSomes({ specifier }) })),
                        Option.getOrUndefined,
                    ),
                reference: flow(Option.liftPredicate(domObject), Option.map(Dom.referenceUnsafe), Option.flatMap(Schema.encodeOption(Reference)), Option.getOrUndefined),
                replace: Match.type<unknown>().pipe(
                    Match.when(Option.isOption, Option.getOrUndefined),
                    Match.when(Result.isResult, Result.match({ onSuccess: (success) => ({ _tag: 'Success', success }), onFailure: (failure) => ({ _tag: 'Failure', failure }) })),
                    Match.when(Predicate.isPromise, () => {
                        throw new Cause.IllegalArgumentError('Value is a promise, and a read answers before it settles');
                    }),
                    Match.when(Schema.is(Uxp.Entry), ({ url }) => String(url)),
                    Match.when(Schema.is(Settings), flow(Schema.encodeOption(Schema.toCodecJson(Settings)), Option.getOrUndefined)),
                    Match.when(Predicate.isError, flow(Schema.encodeOption(Schema.Defect()), Option.getOrUndefined)),
                    Match.when(
                        Predicate.isMap,
                        Record.fromIterableWith(([name, value]) => [String(name), value] as const),
                    ),
                    Match.when(Predicate.isSet, Array.fromIterable),
                    Match.orElse(identity),
                ),
            };
            const attached = new Set<string>();
            return Object.assign(inherit(indesign), {
                encoding,
                subscribe: PubSub.subscribe(hub),
                listen: (target: EventTargetDOMObject<unknown>, type: keyof InDesignEventMap, plugin: string) => {
                    const name = `${plugin}:${type}`;
                    const key = `${target.toSpecifier()} ${name}`;
                    if (attached.has(key)) {
                        return key;
                    }
                    const stale = target.eventListeners.itemByName(name);
                    if (stale.isValid) {
                        stale.remove();
                    }
                    target.eventListeners.add(
                        type,
                        (event: Event) => {
                            PubSub.publishUnsafe(hub, [key, event]);
                        },
                        false,
                        { name },
                    );
                    attached.add(key);
                    return key;
                },
                owning,
                domObject,
                collection,
                picked: (schema: Schema.Top, value: object, skipped: readonly PropertyKey[] = []): Record.ReadonlyRecord<string, unknown> => Option.getOrElse(Option.liftPredicate(pickValue(SchemaAST.toEncoded(schema.ast), value, skipped), isPlain), Record.empty),
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
                textDefault: (value: unknown): value is TextDefault => domObject(value) && value.constructorName === ('TextDefault' satisfies TextDefault['constructorName']),
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
    static readonly current = Dom.use(({ app, document }) => {
        const open = app.documents.everyItem().getElements();
        return Option.filter(Array.head(open), () => open.length === 1).pipe(
            Option.orElse(() => Option.flatMap(Option.liftThrowable((): unknown => app.activeWindow.parent)(), Option.liftPredicate(document))),
            Effect.fromOption(() => Unmatched.of({ subject: 'document', candidates: Array.map(open, (each) => each.toSpecifier()) })),
        );
    });
    static readonly selection = Dom.use(({ app }) => Effect.try({ try: () => (app.windows.length === 0 ? [] : app.selection), catch: HostError.refused('selection') }).pipe(Effect.flatMap(Unmatched.single('selection', Option.none(), (item) => [item.toSpecifier()]))));
    static readonly file = (owner: InDesignDOMObject<unknown>, field: string) =>
        Dom.use(({ app, ScriptLanguage }) => Effect.try({ try: (): unknown => app.doScript('var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', ScriptLanguage.JAVASCRIPT, [owner.toSpecifier(), field]), catch: HostError.refused(`${owner.toSpecifier()} ${field}`) })).pipe(
            Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.OptionFromNullOr(Schema.String)), Effect.orDie)),
        );
    static readonly member = Effect.fnUntraced(function* (kind: string, name: string) {
        const { enumeration, members, declared } = yield* Dom;
        const found = yield* Effect.fromOption(enumeration(kind), () => Unmatched.of({ subject: 'enumeration', query: Option.some(kind), candidates: declared.enumerations }));
        return yield* Unmatched.single(members(found), `${kind} member`, Option.some(name), (member) => [member.toString()]);
    });
    static readonly object = Effect.fnUntraced(function* (kind: string, name: string, target: unknown) {
        const dom = yield* Dom;
        const document = yield* Option.match(dom.owner(target), { onSome: Effect.succeedSome, onNone: () => Effect.option(Dom.current) });
        const fields = Option.match(document, { onNone: Record.empty, onSome: ({ constructorName }) => dom.declaration(constructorName).fields });
        const field = Record.findFirst(fields, ({ array }) => Option.contains(array, kind)).pipe(Option.orElse(() => Record.findFirst(fields, ({ collection }) => Option.contains(collection, kind))));
        const held = Option.zipWith(document, field, (owner, [named]): unknown => Reflect.get(owner, named));
        const listed = Option.map(held, (members): readonly unknown[] => (dom.collection(members) ? members.everyItem().getElements() : Array.ensure(members)));
        return yield* Option.match(listed, { onNone: () => Effect.succeed<unknown>(name), onSome: (members) => Unmatched.single(members.filter(dom.named), kind, Option.some(name), (each) => [each.toSpecifier(), each.name]) });
    });
    static readonly unavailable = (object: InDesignDOMObject<unknown>, field: string, writing: boolean): Option.Option<string> =>
        Match.value({ object, field, writing }).pipe(
            Match.when(
                {
                    object: { constructorName: Match.is(...(['FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'] as const satisfies readonly (FindChangeDropShadowSetting | FindChangeInnerShadowSetting)['constructorName'][])) },
                    field: Match.is(...(['angle', 'distance', 'xOffset', 'yOffset', PROPERTIES] as const satisfies readonly (keyof FindChangeDropShadowSetting & keyof FindChangeInnerShadowSetting)[])),
                },
                () => 'InDesign crashes on any read or write of it in a Find/Change shadow setting',
            ),
            Match.when({ object: owning('handler'), field: Match.is('handler' satisfies keyof EventListener, PROPERTIES), writing: false }, () => 'InDesign crashes reading it once the script that added the listener ended'),
            Match.when(
                { object: (each: InDesignDOMObject<unknown>) => owning('invoke')(each) && Number.between(each.id, STYLISTIC_SET_ACTIONS), field: Match.is(...(['enabled', 'checked', PROPERTIES] as const satisfies readonly (keyof MenuAction)[])), writing: false },
                () => 'InDesign crashes reading it on a Stylistic Set menu action',
            ),
            Match.option,
        );
    static readonly properties = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>) {
        const [{ fields }, specifier] = [(yield* Dom).declaration(object.constructorName), object.toSpecifier()];
        return yield* Option.match(Dom.unavailable(object, PROPERTIES, false), {
            onNone: () => Effect.try({ try: (): Record.ReadonlyRecord<string, unknown> => object.properties, catch: HostError.refused(`${specifier} ${PROPERTIES}`) }),
            onSome: () =>
                Effect.all(Record.filterMap(fields, ({ collection }, name) => (Option.isSome(collection) || Option.isSome(Dom.unavailable(object, name, false)) ? Result.failVoid : Result.succeed(Effect.try({ try: (): unknown => Reflect.get(object, name), catch: HostError.refused(`${specifier} ${name}`) }))))),
        });
    });
    static readonly saved = (object: InDesignDOMObject<unknown>) =>
        Dom.useSync(({ declaration }) =>
            Record.filterMap(declaration(object.constructorName).fields, ({ settable }, name) =>
                settable && name !== PROPERTIES && Option.isNone(Dom.unavailable(object, name, false)) ? Result.fromOption(Option.filter(Option.liftThrowable((): unknown => Reflect.get(object, name))(), Predicate.not(nothing)), Function.constVoid) : Result.failVoid,
            ),
        );
    static readonly fonts = Dom.use(({ app, FontStatus }) => {
        const listMissing = (open: Document) => {
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
                const candidates = [...installed, ...Array.flatMap(app.documents.everyItem().getElements(), listMissing)];
                return (name: string): Effect.Effect<Font, Unmatched> => Effect.map(Unmatched.single(candidates, 'font', Option.some(name), Struct.get('names')), ({ font }) => font());
            },
            catch: HostError.refused('fonts'),
        });
    });
    static readonly resolve = Effect.fn('Dom.resolve')(function* (specifier: Specifier) {
        const [dom, text, query] = [yield* Dom, specifier.path, Option.some(specifier.path)];
        const resolvePath = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = readIdKind(path);
            const valid = (each: unknown): each is InDesignDOMObject<unknown> => dom.domObject(each) && each.isValid && (Option.isNone(requested) || Equal.equals(readIdKind(each.toSpecifier()), requested));
            return Option.filter(Option.liftThrowable((): unknown => dom.app.doScript('resolve(arguments[0])', dom.ScriptLanguage.JAVASCRIPT, [path]))(), (resolved): resolved is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] => (Array.isArray(resolved) ? resolved.every(valid) : valid(resolved)));
        };
        const listResolved = flow(resolvePath, Option.toArray, Array.flatMap(Array.ensure));
        const missed = (): Effect.Effect<readonly InDesignDOMObject<unknown>[], Unmatched> => {
            const container = Array.head(listResolved(specifier.parent));
            const fields = Option.match(container, { onNone: Array.empty, onSome: (at) => Record.toEntries(dom.declaration(at.constructorName).fields) });
            const read = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at): unknown => Reflect.get(at, field)));
            return Option.match(specifier.selector, {
                onNone: () => {
                    const held = Array.filterMap(fields, ([field, { collection }]) => (Option.isSome(collection) ? Result.succeed(read(field)) : Result.failVoid)).filter(dom.collection);
                    const collections = Array.flatMapNullishOr(held, (items) => items.everyItem());
                    return Array.findFirst(collections, (every) => (every.toSpecifier() === text ? Option.some(every.getElements()) : Option.none())).pipe(Effect.fromOption(() => Unmatched.of({ subject: 'object', query, candidates: specifier.isProperty ? [] : Array.map(collections, (every) => every.toSpecifier()) })));
                },
                onSome: (selector) => {
                    const siblings = listResolved(specifier.withoutSelector);
                    const sibling = Option.map(Array.head(siblings), (each) => each.constructorName);
                    return Selector.match(selector, {
                        Index: () => Unmatched.of({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query }),
                        Key: () => {
                            const related = fields.flatMap(([field, { array }]) => (Option.isSome(sibling) && Equal.equals(array, sibling) ? Array.ensure<unknown>(read(field)) : [])).filter(dom.named);
                            return Unmatched.of({ subject: 'object', query, candidates: Array.dedupe(Array.map([...siblings, ...related.filter((each) => Option.contains(specifier.name, each.name))], (each) => each.toSpecifier())) });
                        },
                    });
                },
            });
        };
        return yield* Option.match(resolvePath(text), {
            onSome: (resolved) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) || specifier.isProperty ? resolved : Array.ensure(resolved)),
            onNone: () => (specifier.end === undefined ? missed() : Unmatched.of({ subject: 'range', query })),
        });
    });
    static readonly locate = Effect.fnUntraced(function* (specifier: Specifier) {
        const [dom, found] = yield* Effect.all([Dom, Dom.resolve(specifier)]);
        return dom.domObject(found) ? found : yield* Unmatched.of({ subject: 'object', query: Option.some(specifier.path) });
    });
    static readonly referenceUnsafe = (object: InDesignDOMObject<unknown>): Reference => {
        const valid = object.isValid;
        const name: unknown = valid && owning('name')(object) ? object.name : undefined;
        return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: Option.liftPredicate(name, Predicate.isString), valid });
    };
    static readonly collect = (items: Owned['everyItem']) => ({ type: items.constructorName, length: items.length, specifier: Option.map(Option.fromNullishOr(items.everyItem()), (every) => every.toSpecifier()) });
    static readonly reference = (object: InDesignDOMObject<unknown>): Effect.Effect<Reference, HostError> => Effect.try({ try: () => Dom.referenceUnsafe(object), catch: HostError.refused('object reference') });
    static readonly encoder = Dom.useSync(
        ({ encoding }) =>
            (value: unknown): string | undefined =>
                JSON.stringify(encode(value, encoding)),
    );
    static readonly decode =
        (target: unknown, named: readonly string[]) =>
        (value: unknown): Effect.Effect<unknown, Failure, Dom> =>
            Match.value(value).pipe(
                Match.when(Schema.is(Value.Located), ({ specifier }) => Dom.resolve(specifier)),
                Match.when(Schema.is(Value.Enumerated), ({ type, member }) => Dom.member(type, member)),
                Match.when(Predicate.isString, (text) =>
                    Array.match(named, {
                        onEmpty: () => Effect.succeed<unknown>(text),
                        onNonEmpty: (kinds) =>
                            Effect.forEach(kinds, (kind) => Effect.flip(Effect.map(Option.isSome(enumeration(kind)) ? Dom.member(kind, text) : Dom.object(kind, text, target), Array.of))).pipe(
                                Effect.map((missed) => Unmatched.of({ subject: missed.map(({ subject }) => subject).join(' or '), query: Option.some(text), candidates: missed.flatMap(({ candidates }) => candidates) })),
                                Effect.flip,
                                Effect.map(Array.headNonEmpty),
                            ),
                    }),
                ),
                Match.when(Array.isArray, Effect.forEach(Dom.decode(target, named))),
                Match.when(isPlain, (properties) => Effect.all(Record.map(properties, Dom.decode(target, [])))),
                Match.orElse(Effect.succeed),
            );
    static readonly write: {
        (values: Record.ReadonlyRecord<string, unknown>): (target: InDesignDOMObject<unknown>) => Effect.Effect<Effect.Effect<readonly string[], HostError>, Failure, Dom>;
        (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>): Effect.Effect<Effect.Effect<readonly string[], HostError>, Failure, Dom>;
    } = Function.dual(2, (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) => Effect.map(decodeWrites(target, values), (writes) => Effect.validate(writes, ([name, write]) => Effect.sandbox(Effect.as(write, name))).pipe(Effect.catch(failAll))));
    static readonly events = (select: (dom: Dom['Service']) => readonly [EventTargetDOMObject<unknown>, readonly (keyof InDesignEventMap)[]]) =>
        Stream.unwrap(
            Effect.gen(function* () {
                const [dom, { id }] = yield* Effect.all([Dom, Plugin]);
                const [target, types] = select(dom);
                const subscription = yield* dom.subscribe;
                const keys = new Set(yield* Effect.try({ try: () => Array.map(types, (type) => dom.listen(target, type, id)), catch: HostError.refused(`listeners of ${types.join(', ')}`) }));
                return Stream.fromSubscription(subscription).pipe(Stream.filterMap(([key, event]) => (keys.has(key) ? Result.succeed(event) : Result.failVoid)));
            }),
        );
    static readonly extent = Effect.fn('Dom.extent')(function* (...placed: readonly (Page | PageItem)[]) {
        const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Dom;
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
        Dom.use(({ encoding }) => Effect.try({ try: () => encode(value(), encoding), catch: (cause) => (Cause.isIllegalArgumentError(cause) ? cause : HostError.refused(subject)(cause)) })).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.toCodecJson(schema)), Effect.orDie)));
    static readonly pick = <S extends Schema.Top>(schema: S, object: InDesignDOMObject<unknown>, supplied: () => Record.ReadonlyRecord<string, unknown> = Record.empty) =>
        Effect.flatMap(Dom, ({ picked }) =>
            Dom.read(schema, object.toSpecifier(), () => {
                const given = supplied();
                return { ...picked(schema, object, Record.keys(given)), ...given };
            }),
        );
    static readonly commit = Effect.fnUntraced(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
        const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Dom;
        const [opened, outcome, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
        const close = (id: number) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
        yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, outcome), Deferred.await(outcome))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(outcome, error)));
        return yield* Deferred.await(outcome).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), close, { discard: true })));
    });
    static readonly commitWrite = <W, E, R>(label: string, target: InDesignDOMObject<unknown>, body: Effect.Effect<W, E, R>): Effect.Effect<Omit<Reference, 'brand'> & { readonly written: W }, E | HostError, R | Dom> =>
        Dom.commit(
            label,
            Effect.zipWith(body, Dom.reference(target), (members, reference) => ({ ...reference, written: members })),
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
const readTag = ({ propertySignatures }: SchemaAST.Objects) => Array.findFirst(propertySignatures, ({ name, type }) => (name === '_tag' && SchemaAST.isLiteral(type) ? Option.some(type.literal) : Option.none()));
const entry = (node: SchemaAST.Arrays, index: number): SchemaAST.AST =>
    Array.get(node.elements, index).pipe(
        Option.orElse(() => Array.head(node.rest)),
        Option.getOrElse(() => Schema.Unknown.ast),
    );
const readIdKind = (path: string) =>
    Schema.decodeOption(Specifier.FromString)(path).pipe(
        Option.filter(({ end, selector }) => end === undefined && Option.exists(selector, (step) => Selector.guards.Key(step) && step.key === 'id')),
        Option.flatMap(({ steps }) => Array.last(steps)),
        Option.map(Struct.get('kind')),
    );
const failAll = <E>(causes: readonly Cause.Cause<E>[]) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))));
const decodeWrites = Effect.fnUntraced(function* (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>): Effect.fn.Return<readonly (readonly [string, Effect.Effect<unknown, HostError>])[], Failure, Dom> {
    const [{ declaration, domObject }, specifier] = [yield* Dom, target.toSpecifier()];
    const { fields } = declaration(target.constructorName);
    return yield* Effect.validate(
        Object.entries(values),
        Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
            const refused = HostError.refused(`${specifier} ${field}`);
            yield* Predicate.hasProperty(target, field) ? Effect.void : Unmatched.of({ subject: `field of ${specifier}`, query: Option.some(field), candidates: Record.keys(fields) });
            yield* Effect.transposeOption(Option.map(Dom.unavailable(target, field, !isPlain(value)), refused));
            const inner: unknown = isPlain(value) ? yield* Effect.try({ try: (): unknown => Reflect.get(target, field), catch: refused }) : undefined;
            if (isPlain(value) && domObject(inner)) {
                return Array.map(yield* decodeWrites(inner, value), ([name, write]) => [`${field}.${name}`, write] as const);
            }
            const native = yield* Dom.decode(target, Option.match(Record.get(fields, field), { onNone: Array.empty<string>, onSome: Struct.get('classes') }))(value);
            return [[field, Effect.try({ try: () => Object.assign(target, { [field]: native }), catch: refused })] as const];
        }, Effect.sandbox),
    ).pipe(Effect.map(Array.flatten), Effect.catch(failAll));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dom, STYLISTIC_SETS };
