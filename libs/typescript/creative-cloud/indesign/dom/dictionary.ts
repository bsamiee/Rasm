import type { InDesignDOMObject, MenuAction } from 'adobe:indesign';
import { Array, Effect, Match, Number, Option, Predicate, Record, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALTERNATIVE = /\. Can also accept: |, | or /u;
const READER = `function rows(list, more) {
    var out = [];
    for (var index = 0; index < list.length(); index++) {
        var row = list[index], type = row.datatype;
        out.push([more(row), String(row.@name), String(row.shortdesc) || null, String(type.type) || null, type.array.length() > 0 || null, type.min.length() > 0 ? Number(type.min) : null, type.max.length() > 0 ? Number(type.max) : null]);
    }
    return out;
}
var found = $.dictionary.getClass(arguments[0]);
found === null ? null : (function (model) {
    var members = model.elements.(@type == "instance");
    return [
        String(model.superclass) || null,
        rows(members.property, function (row) { return row.@rwaccess == "readonly"; }),
        rows(members.method, function (row) { return rows(row.parameters.parameter, function (row) { return row.@optional == "true"; }); }),
        rows(model.elements.(@type == "class").property, function () { return true; })
    ];
})(found.toXML());`;

// --- [MODELS] --------------------------------------------------------------------------

const Declared = Schema.TemplateLiteralParser([
    Schema.String,
    Schema.Literals(['Can return: ', 'Can accept: ']),
    Schema.String.pipe(
        Schema.decodeTo(Schema.Array(Schema.Union([Schema.TemplateLiteralParser([Schema.String, ' enumerator']).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform(([name]) => name), encode: SchemaGetter.forbiddenEncoding })), Schema.String])), {
            decode: SchemaGetter.transform((text) => text.split(ALTERNATIVE)),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.Literals(['.', '. (Optional)']),
]).pipe(Schema.decodeTo(Schema.Struct({ types: Schema.Array(Schema.String) }), { decode: SchemaGetter.transform(([, , types]) => ({ types })), encode: SchemaGetter.forbiddenEncoding }));
const Optional = Schema.TemplateLiteral([Schema.String, '(Optional)']);
const Member = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    types: Schema.Array(Schema.String),
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
const Class = Schema.Struct({ parent: Schema.OptionFromOptionalKey(Schema.String), fields: Schema.Record(Schema.String, Field), methods: Schema.Record(Schema.String, Method), statics: Schema.Record(Schema.String, Schema.String) });
const Typed = [Schema.String, Schema.OptionFromNullOr(Schema.String), Schema.OptionFromNullOr(Schema.String), Schema.OptionFromNullOr(Schema.Literal(true)), Schema.OptionFromNullOr(Schema.Finite), Schema.OptionFromNullOr(Schema.Finite)] as const;
const Flagged = Schema.Tuple([Schema.Boolean, ...Typed]);
const Rows = Schema.Tuple([Schema.OptionFromNullOr(Schema.String), Schema.Array(Flagged), Schema.Array(Schema.Tuple([Schema.Array(Flagged), ...Typed])), Schema.Array(Flagged)]).pipe(
    Schema.decodeTo(Schema.toType(Class), {
        decode: SchemaGetter.transform(([parent, properties, methods, statics]) => {
            const stated = ([name, description, type, array, minimum, maximum]: Schema.Tuple<typeof Typed>['Type']) => ({
                name,
                type,
                types: Option.match(Option.flatMap(description, Schema.decodeUnknownOption(Declared)), { onNone: () => Option.toArray(type), onSome: Struct.get('types') }),
                description,
                array,
                minimum,
                maximum,
                members: Option.none(),
                unavailable: Option.none(),
            });
            return {
                parent,
                fields: Record.fromIterableWith(properties, ([readonly, ...row]) => [row[0], { ...stated(row), settable: !readonly }]),
                methods: Record.fromIterableWith(methods, ([parameters, ...row]) => {
                    const { name, description, type, array } = stated(row);
                    return [name, { name, description, parameters: Array.map(parameters, ([optional, ...each]) => ({ ...Struct.omit(stated(each), ['unavailable']), optional: optional || Option.exists(each[1], Schema.is(Optional)) })), returns: Option.as(type, { type, array }) }];
                }),
                statics: Record.getSomes(Record.fromIterableWith(statics, ([, name, description]) => [name, description])),
            };
        }),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const unavailable = (kind: string, field: string, observed: Option.Option<InDesignDOMObject<unknown>>, writing: boolean): Option.Option<string> => {
    const first = 0x69_74;
    const last = 0x69_87;
    const id = Option.map(
        Option.filter(observed, (each): each is MenuAction => each.constructorName === 'MenuAction'),
        Struct.get('id'),
    );
    return Match.value({ kind, field, writing, stylistic: !Option.exists(id, Predicate.not(Number.between({ minimum: first, maximum: last }))) }).pipe(
        Match.when({ kind: Match.is('FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'), field: Match.is('angle', 'distance', 'xOffset', 'yOffset', 'properties') }, () => 'Shadow fields crash native Transparency provider'),
        Match.when({ kind: 'EventListener', field: Match.is('handler', 'properties'), writing: false }, () => 'Ended UXP context handlers crash InDesign'),
        Match.when({ kind: 'MenuAction', field: Match.is('enabled', 'checked', 'properties'), writing: false, stylistic: true }, () => 'Stylistic Set state crashes native CharacterPanel provider'),
        Match.option,
    );
};
const undeclared = <S extends typeof Field | typeof Method>(schema: S, name: string) => Effect.orDie(Schema.decodeEffect(schema)({ name, types: [], settable: false, parameters: [] }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Field, Member, Method, READER, Rows, unavailable, undeclared };
