import indesign, { type Enumeration, type Enumerator, type InDesignDOMObject, type MenuAction } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Context, Effect, flow, Layer, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { InDesignError } from '../models.ts';

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
// --- [SERVICES] ------------------------------------------------------------------------

class Classes extends Context.Service<Classes>()('@rasm/indesign/objects/classes', {
    make: Cache.make({
        capacity: Object.getOwnPropertyNames(indesign).length,
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
            const [parent, properties, methods, statics] = yield* Effect.try({ try: () => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind]), catch: InDesignError.refused(Option.some(kind)) }).pipe(
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
            return {
                parent: Option.liftPredicate(parent, Predicate.isTruthy),
                fields: Record.fromIterableWith(properties, (row) => [row[0], { ...stated(row), settable: !row[5] }]),
                methods: Record.fromIterableWith(methods, (row) => {
                    const { name, description, type, array } = stated(row);
                    return [name, { name, description, parameters: Array.map(row[5], (each) => ({ ...Struct.omit(stated(each), ['unavailable']), optional: each[5] || each[1].endsWith('(Optional)') })), returns: Option.map(type, (declared) => ({ type: Option.some(declared), array })) }];
                }),
                statics: Record.fromIterableWith(statics, ([name, description]) => [name, description]),
            } satisfies { readonly parent: Option.Option<string>; readonly fields: Record.ReadonlyRecord<string, typeof Field.Type>; readonly methods: Record.ReadonlyRecord<string, typeof Method.Type>; readonly statics: Record.ReadonlyRecord<string, string> };
        }),
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [MODULE]
const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
const kinds = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) || value instanceof indesign.Enumeration ? [name] : []));
const members = (found: Enumeration): readonly Enumerator[] =>
    Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
        const member: unknown = Reflect.get(found, key);
        return member instanceof indesign.Enumerator && String(member) === key ? [member] : [];
    });
const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
const plural = ({ type }: Pick<typeof Member.Type, 'type'>): boolean =>
    Option.exists(type, (kind) => {
        const declared: unknown = Reflect.get(indesign, kind);
        return Predicate.isFunction(declared) && Object.hasOwn(declared.prototype, 'everyItem');
    });

// --- [EXCLUSIONS]
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

// --- [LINEAGE]
const lineage: (name: string) => Effect.Effect<readonly string[], InDesignError, Classes> = Effect.fnUntraced(function* (name: string) {
    const parent = yield* Cache.get(yield* Classes, name).pipe(
        Effect.map(Struct.get('parent')),
        Effect.catchTag('Unmatched', () => Effect.succeedNone),
    );
    return Option.isSome(parent) ? [parent.value, ...(yield* lineage(parent.value))] : [];
});
const element = (collection: string): Effect.Effect<Option.Option<string>, never, Classes> =>
    Effect.flatMap(Classes, (classes) =>
        Cache.get(classes, collection).pipe(
            Effect.map((rows) => Option.flatMap(Record.get(rows.methods, 'item'), flow(Struct.get('returns'), Option.flatMap(Struct.get('type'))))),
            Effect.catch(() => Effect.succeedNone),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Classes, element, enumeration, exported, Field, kinds, lineage, Member, Method, members, plural, unavailable };
