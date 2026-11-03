import type { InDesignDOMObject, MenuAction } from 'adobe:indesign';
import { HostError, ObjectModel } from '@rasm/automation/requests';
import { Array, Effect, flow, Match, Number, Option, Record, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALTERNATIVE = /\. Can also accept: |, | or /u;
const OPTIONAL = '. (Optional)';
const READER = `function declared(list, more) {
    var out = [];
    for (var index = 0; index < list.length(); index++) {
        var each = list[index], type = each.datatype;
        out.push([more(each), String(each.@name), String(each.shortdesc) || null, String(type.type) || null, type.array.length() > 0, type.min.length() > 0 ? Number(type.min) : null, type.max.length() > 0 ? Number(type.max) : null]);
    }
    return out;
}
var found = $.dictionary.getClass(arguments[0]);
found === null ? null : (function (model) {
    var members = model.elements.(@type == "instance");
    return [
        String(model.superclass) || null,
        declared(members.property, function (each) { return each.@rwaccess == "readonly"; }),
        declared(members.method, function (each) { return declared(each.parameters.parameter, function (each) { return each.@optional == "true"; }); }),
        declared(model.elements.(@type == "class").property, function () { return true; })
    ];
})(found.toXML());`;

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.TemplateLiteralParser([
    Schema.String,
    Schema.Literals(['Can return: ', 'Can accept: ']),
    Schema.String.pipe(
        Schema.decodeTo(Schema.Array(Schema.Union([Schema.TemplateLiteralParser([Schema.String, ' enumerator']).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform(([name]) => name), encode: SchemaGetter.forbiddenEncoding })), Schema.String])), {
            decode: SchemaGetter.transform((text) => text.split(ALTERNATIVE)),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.Literals(['.', OPTIONAL]),
]).pipe(Schema.decodeTo(Schema.Struct({ types: Schema.Array(Schema.String), optional: Schema.Boolean }), { decode: SchemaGetter.transform(([, , types, end]) => ({ types, optional: end === OPTIONAL })), encode: SchemaGetter.forbiddenEncoding }));
const Stated = ObjectModel.Field.mapFields(Struct.omit(['members', 'unavailable', 'settable']));
const Parameter = Stated.pipe(Schema.fieldsAssign({ optional: Schema.Boolean }));
const Declaration = Schema.Struct({
    parent: Schema.OptionFromOptionalKey(Schema.String),
    fields: Schema.Record(Schema.String, Stated.pipe(Schema.fieldsAssign({ settable: Schema.Boolean }))),
    methods: Schema.Record(Schema.String, ObjectModel.Method.mapFields(Struct.assign({ parameters: Schema.Array(Parameter) }))),
    statics: Schema.Record(Schema.String, Schema.String),
});
const Typed = [Schema.String, Schema.OptionFromNullOr(Schema.String), Schema.OptionFromNullOr(Schema.String), Schema.Boolean, Schema.OptionFromNullOr(Schema.Finite), Schema.OptionFromNullOr(Schema.Finite)] as const;
const Flagged = Schema.Tuple([Schema.Boolean, ...Typed]);
const Declared = Schema.Tuple([Schema.OptionFromNullOr(Schema.String), Schema.Array(Flagged), Schema.Array(Schema.Tuple([Schema.Array(Flagged), ...Typed])), Schema.Array(Flagged)]).pipe(
    Schema.decodeTo(Schema.toType(Declaration), {
        decode: SchemaGetter.transform(([parent, properties, methods, statics]) => {
            const stated = ([name, description, type, array, minimum, maximum]: Schema.Tuple<typeof Typed>['Type'], flagged = false) => {
                const described = Option.flatMap(description, Schema.decodeUnknownOption(Described));
                return { name, type, types: Option.orElse(Option.map(described, Struct.get('types')), () => Option.map(type, Array.of)), description, array, minimum, maximum, optional: flagged || Option.exists(described, Struct.get('optional')) };
            };
            return {
                parent,
                fields: Record.fromIterableWith(properties, ([readonly, ...each]) => [each[0], { ...stated(each), settable: !readonly }]),
                methods: Record.fromIterableWith(methods, ([parameters, name, description, type, array]) => [name, { name, description, parameters: Array.map(parameters, ([optional, ...parameter]) => stated(parameter, optional)), returns: Option.map(type, (returned) => ({ type: returned, array })) }]),
                statics: Record.getSomes(Record.fromIterableWith(statics, ([, name, description]) => [name, description])),
            };
        }),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const Dictionary = {
    aggregate: 'properties',
    read: (indesign: typeof import('adobe:indesign'), kind: string) =>
        Effect.try({ try: (): unknown => indesign.app.doScript(READER, indesign.ScriptLanguage.JAVASCRIPT, [kind]), catch: HostError.refused(`dictionary class ${kind}`) }).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.OptionFromNullOr(Declared)), Effect.orDie))),
    unavailable: (kind: string, field: string, observed: Option.Option<InDesignDOMObject<unknown>>, writing: boolean): Option.Option<string> =>
        Match.value({ kind, field, writing, stylistic: !Option.filter(observed, (each): each is MenuAction => each.constructorName === 'MenuAction').pipe(Option.exists(({ id }) => !Number.between(id, { minimum: 0x69_74, maximum: 0x69_87 }))) }).pipe(
            Match.when({ kind: Match.is('FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'), field: Match.is('angle', 'distance', 'xOffset', 'yOffset', Dictionary.aggregate) }, () => 'Shadow fields crash native Transparency provider'),
            Match.when({ kind: 'EventListener', field: Match.is('handler', Dictionary.aggregate), writing: false }, () => 'Ended UXP context handlers crash InDesign'),
            Match.when({ kind: 'MenuAction', field: Match.is('enabled', 'checked', Dictionary.aggregate), writing: false, stylistic: true }, () => 'Stylistic Set state crashes native CharacterPanel provider'),
            Match.option,
        ),
    undeclared: <S extends typeof ObjectModel.Field | typeof ObjectModel.Method>(schema: S, { name, settable = false }: { readonly name: string; readonly settable?: boolean }) => Effect.orDie(Schema.decodeEffect(schema)({ name, settable, parameters: [] })),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dictionary };
