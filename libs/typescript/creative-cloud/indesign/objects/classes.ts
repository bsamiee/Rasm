import type { BaseCollection, Document, Enumeration, Enumerator, InDesignDOMObject, MenuAction, ReadonlyNamedDOMObject } from 'adobe:indesign';
import type { Entry } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Array, Cache, type Cause, Context, Effect, flow, Iterable, identity, Layer, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Host } from '../host.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Member = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Schema.OptionFromOptionalKey(Schema.Literal(true)),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
    unavailable: Schema.OptionFromOptionalKey(Schema.String),
}).annotate({ identifier: 'Member' });
const Field = Member.pipe(Schema.fieldsAssign({ settable: Schema.Boolean })).annotate({ identifier: 'Field' });
const Method = Member.mapFields(Struct.pick(['name', 'description']))
    .pipe(
        Schema.fieldsAssign({
            parameters: Schema.Array(Member.mapFields(Struct.omit(['unavailable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean }))),
            returns: Schema.OptionFromOptionalKey(Member.mapFields(Struct.pick(['type', 'array']))),
        }),
    )
    .annotate({ identifier: 'Method' });
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;

// --- [SERVICES] ------------------------------------------------------------------------

class Classes extends Context.Service<Classes>()('@rasm/indesign/objects/classes', {
    make: Effect.gen(function* () {
        const { indesign } = yield* Host;
        const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
        const kinds = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) || value instanceof indesign.Enumeration ? [name] : []));
        const owning = <A>(member: keyof A): (<T>(value: T) => value is T & A) => {
            const owners = new Set(Array.flatMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [])));
            return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
        };
        const [host, collection, named, document] = [
            owning<InDesignDOMObject<unknown>>('toSpecifier'),
            owning<BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }>>('everyItem'),
            owning<ReadonlyNamedDOMObject<unknown>>('name'),
            owning<Document>('createTOC'),
        ];
        const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
        const members = (found: Enumeration): readonly Enumerator[] =>
            Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
                const member: unknown = Reflect.get(found, key);
                return member instanceof indesign.Enumerator && String(member) === key ? [member] : [];
            });
        const plural = ({ type }: Pick<typeof Member.Type, 'type'>): boolean =>
            Option.exists(type, (kind) => {
                const declared: unknown = Reflect.get(indesign, kind);
                return Predicate.isFunction(declared) && Object.hasOwn(declared.prototype, 'everyItem');
            });
        const unavailable = (kind: string, field: string, observed: Option.Option<Pick<InDesignDOMObject<unknown>, 'constructorName'>>, writing: boolean): Option.Option<string> =>
            Match.value({ kind, field, writing }).pipe(
                Match.when(
                    (member) => ['FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'].includes(member.kind) && ['angle', 'distance', 'xOffset', 'yOffset', 'properties'].includes(member.field),
                    () => Option.some('The native Transparency provider crashes for this Find/Change shadow accessor'),
                ),
                Match.when({ kind: 'EventListener', writing: false }, (member) => (['handler', 'properties'].includes(member.field) ? Option.some('Reading an event handler whose UXP context has ended crashes InDesign; the DOM exposes no context lifetime') : Option.none())),
                Match.when({ kind: 'MenuAction', writing: false }, (member) => {
                    const id = Option.map(
                        Option.filter(observed, (each): each is MenuAction => each.constructorName === 'MenuAction'),
                        Struct.get('id'),
                    );
                    const first = 0x69_74;
                    const last = 0x69_87;
                    return ['enabled', 'checked', 'properties'].includes(member.field) && (Option.isNone(id) || (id.value >= first && id.value <= last)) ? Option.some('The native CharacterPanel provider crashes when reading Stylistic Set action state') : Option.none();
                }),
                Match.option,
                Option.flatten,
                Option.filter(() => indesign.app.version === '22.0.0.84'),
            );
        const rows = yield* Cache.make({
            capacity: exported.length,
            lookup: Effect.fnUntraced(function* (kind: string) {
                const reader = `function rows(list, more) {
    var out = [];
    for (var index = 0; index < list.length(); index++) {
        var row = list[index];
        out.push([String(row.@name), String(row.shortdesc), String(row.datatype.type), row.datatype.array.length() > 0, row.datatype.min.length() > 0 ? [Number(row.datatype.min), Number(row.datatype.max)] : [], more(row)]);
    }
    return out;
}
var found = $.dictionary.getClass(arguments[0]);
found === null ? null : (function (model) {
    var members = model.elements.(@type == "instance");
    return [
        String(model.superclass),
        rows(members.property, function (row) { return row.@rwaccess == "readonly"; }),
        rows(members.method, function (row) { return rows(row.parameters.parameter, function (row) { return row.@optional == "true"; }); }),
        rows(model.elements.(@type == "class").property, function () { return true; })
    ];
})(found.toXML());`;
                const Typed = [Schema.String, Schema.String, Schema.String, Schema.Boolean, Schema.Array(Schema.Finite)] as const;
                const Flagged = Schema.Tuple([...Typed, Schema.Boolean]);
                const Rows = Schema.NullOr(Schema.Tuple([Schema.String, Schema.Array(Flagged), Schema.Array(Schema.Tuple([...Typed, Schema.Array(Flagged)])), Schema.Array(Flagged)]));
                const [parent, properties, methods, statics] = yield* Effect.try(() => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind])).pipe(
                    Effect.flatMap(flow(Schema.decodeUnknownEffect(Rows), Effect.orDie)),
                    Effect.filterOrFail(Predicate.isNotNull, () => Unmatched.make({ subject: 'class', query: kind, candidates: kinds })),
                );
                const stated = ([name, description, type, array, [minimum, maximum]]: readonly [string, string, string, boolean, readonly number[], unknown]): typeof Member.Type => ({
                    name,
                    description: Option.liftPredicate(description, Predicate.isTruthy),
                    type: Option.liftPredicate(type, Predicate.isTruthy),
                    array: array ? Option.some(true as const) : Option.none(),
                    minimum: Option.fromUndefinedOr(minimum),
                    maximum: Option.fromUndefinedOr(maximum),
                    members: Option.flatMap(enumeration(type), (found) => Array.match(Array.map(members(found), String), { onEmpty: Option.none, onNonEmpty: Option.some })),
                    unavailable: unavailable(kind, name, Option.none(), false),
                });
                const parameter = (each: typeof Flagged.Type) => ({ ...Struct.omit(stated(each), ['unavailable']), optional: each[5] || each[1].endsWith('(Optional)') });
                return {
                    parent: Option.liftPredicate(parent, Predicate.isTruthy),
                    fields: Record.fromIterableWith(properties, (row) => [row[0], { ...stated(row), settable: !row[5] }]),
                    methods: Record.fromIterableWith(methods, (row) => {
                        const { name, description, type, array } = stated(row);
                        return [name, { name, description, parameters: Array.map(row[5], parameter), returns: Option.as(type, { type, array }) }];
                    }),
                    statics: Record.fromIterableWith(statics, ([name, description]) => [name, description]),
                } satisfies { readonly parent: Option.Option<string>; readonly fields: Record.ReadonlyRecord<string, typeof Field.Type>; readonly methods: Record.ReadonlyRecord<string, typeof Method.Type>; readonly statics: Record.ReadonlyRecord<string, string> };
            }),
        });
        const lineage: (name: string) => Effect.Effect<readonly string[], Cause.UnknownError> = Effect.fnUntraced(function* (name: string) {
            const parent = yield* Cache.get(rows, name).pipe(
                Effect.map(Struct.get('parent')),
                Effect.catchTag('Unmatched', () => Effect.succeedNone),
            );
            return Option.isSome(parent) ? [parent.value, ...(yield* lineage(parent.value))] : [];
        });
        const element = (kind: string): Effect.Effect<Option.Option<string>> =>
            Cache.get(rows, kind).pipe(
                Effect.map((model) => Option.flatMap(Record.get(model.methods, 'item'), flow(Struct.get('returns'), Option.flatMap(Struct.get('type'))))),
                Effect.catch(() => Effect.succeedNone),
            );
        const reference = (object: InDesignDOMObject<unknown>) => ({ type: object.constructorName, specifier: object, name: named(object) ? object.name : undefined });
        const projected: (value: unknown) => unknown = Match.type<unknown>().pipe(
            Match.when(named, reference),
            Match.when(host, (object) => object.properties),
            Match.orElse(identity),
        );
        const encoded = (value: unknown, labeled: boolean): string | undefined =>
            JSON.stringify(value, function (this: object, key: string, part: unknown) {
                const held: unknown = Reflect.get(this, key);
                return Match.value(Option.isOption(held) ? Option.getOrUndefined(held) : part).pipe(
                    Match.when(Match.instanceOf(indesign.Enumerator), String),
                    Match.when(collection, (items) => (labeled ? { type: items.constructor.name, length: items.length } : items.everyItem().toSpecifier())),
                    Match.when(host, (object) => (labeled ? { type: object.constructorName || undefined, specifier: object.toSpecifier(), invalid: object.isValid === false || undefined } : object.toSpecifier())),
                    Match.when(
                        (entry: unknown): entry is Entry => !record(entry) && Predicate.hasProperty(entry, 'isEntry') && entry.isEntry === true,
                        (entry) => String(entry.url),
                    ),
                    Match.when(Predicate.isMap, Object.fromEntries),
                    Match.when(Predicate.isSet, (entries) => [...entries]),
                    Match.when(Predicate.isBigInt, String),
                    Match.orElse(identity),
                );
            });
        const ancestors = (object: unknown): Iterable<InDesignDOMObject<unknown>> => Iterable.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()));
        const owner = (object: unknown): Option.Option<Document> => Iterable.findFirst(ancestors(object), document);
        return { exported, kinds, owning, host, collection, named, document, enumeration, members, plural, unavailable, rows, lineage, element, reference, projected, encoded, ancestors, owner };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Classes, Field, Member, Method, record };
