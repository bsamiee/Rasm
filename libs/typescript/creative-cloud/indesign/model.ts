import type { BaseCollection, Document, Enumeration, Enumerator, InDesignDOMObject, MenuAction, Page, PageItem, ReadonlyNamedDOMObject, Spread, Text, TextFrame } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Cause, Context, Effect, flow, Match, Number, Option, Predicate, pipe, Random, Record, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECLARED = /Can (?:return|accept): (?<types>.+?)\.(?: \(Optional\))?$/u;
const ALTERNATIVE = /\. Can also accept: |, | or /u;
const ENUMERATOR = / enumerator$/u;

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
const Class = Schema.Struct({ parent: Schema.OptionFromOptionalKey(Schema.String), fields: Schema.Record(Schema.String, Field), methods: Schema.Record(Schema.String, Method), statics: Schema.Record(Schema.String, Schema.String) });

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/model', {
    make: Effect.fnUntraced(function* (indesign: typeof import('adobe:indesign')) {
        const model: typeof indesign = Object.create(indesign);
        const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
        const classes = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) ? [name] : []));
        const enumerations = Array.flatMap(exported, ([name, value]) => (value instanceof indesign.Enumeration ? [name] : []));
        const prototypes = Record.map(
            Array.groupBy(
                Array.flatMap(exported, ([, type]) => (Predicate.isFunction(type) ? Array.map(Object.getOwnPropertyNames(type.prototype), (member) => [member, type.prototype] as const) : [])),
                ([member]) => member,
            ),
            (owners) => new Set(Array.map(owners, ([, prototype]) => prototype)),
        );
        const owning = <A>(member: keyof A & string): (<T>(value: T) => value is T & A) => {
            const owners = Option.getOrElse(Record.get(prototypes, member), () => new Set<unknown>());
            return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
        };
        const [host, collection, named, document, page, spread, item, text, frame] = [
            owning<InDesignDOMObject<unknown>>('toSpecifier'),
            owning<BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'> & { readonly getElements: () => InDesignDOMObject<unknown>[] }>>('everyItem'),
            owning<ReadonlyNamedDOMObject<unknown>>('name'),
            owning<Document>('createTOC'),
            owning<Page>('documentOffset'),
            owning<Spread>('spreadHidden'),
            owning<PageItem>('itemLayer'),
            owning<Text>('parentTextFrames'),
            owning<TextFrame>('textFrameIndex'),
        ];
        const enumeration = (name: string): Option.Option<Enumeration> => Option.liftPredicate(Reflect.get(indesign, name), (value): value is Enumeration => value instanceof indesign.Enumeration);
        const members = <E extends Enumeration>(found: E) => {
            const keyed = (member: unknown, key: string): member is Extract<E[keyof E], Enumerator> => member instanceof indesign.Enumerator && String(member) === key;
            return Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(found)), (key) => {
                const member: unknown = Reflect.get(found, key);
                return keyed(member, key) ? [member] : [];
            });
        };
        const declared = ({ type, description }: Pick<typeof Member.Type, 'type' | 'description'>) =>
            Option.match(
                Option.flatMapNullishOr(description, (written) => DECLARED.exec(written)?.groups?.types),
                {
                    onNone: () => Option.toArray(type),
                    onSome: (types) => Array.map(types.split(ALTERNATIVE), (each) => each.replace(ENUMERATOR, '')),
                },
            );
        const listing = (kind: string): boolean => {
            const found: unknown = Reflect.get(indesign, kind);
            return Predicate.isFunction(found) && Object.hasOwn(found.prototype, 'everyItem');
        };
        const plural = ({ type }: Pick<typeof Member.Type, 'type'>): boolean => Option.exists(type, listing);
        const unavailable = (kind: string, field: string, observed: Option.Option<InDesignDOMObject<unknown>>, writing: boolean): Option.Option<string> => {
            const first = 0x69_74;
            const last = 0x69_87;
            const id = Option.map(
                Option.filter(observed, (each): each is MenuAction => each.constructorName === 'MenuAction'),
                Struct.get('id'),
            );
            return Match.value({ kind, field, writing, stylistic: !Option.exists(id, Predicate.not(Number.between({ minimum: first, maximum: last }))) }).pipe(
                Match.when({ kind: Match.is('FindChangeDropShadowSetting', 'FindChangeInnerShadowSetting'), field: Match.is('angle', 'distance', 'xOffset', 'yOffset', 'properties') }, () => 'The native Transparency provider crashes for this Find/Change shadow accessor'),
                Match.when({ kind: 'EventListener', field: Match.is('handler', 'properties'), writing: false }, () => 'Reading an event handler whose UXP context has ended crashes InDesign; the DOM exposes no context lifetime'),
                Match.when({ kind: 'MenuAction', field: Match.is('enabled', 'checked', 'properties'), writing: false, stylistic: true }, () => 'The native CharacterPanel provider crashes when reading Stylistic Set action state'),
                Match.option,
            );
        };
        const rows = yield* Cache.make({
            capacity: exported.length,
            lookup: (kind: string) => {
                const reader = `function rows(list, more) {
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
                const Typed = [Schema.String, Schema.OptionFromNullOr(Schema.String), Schema.OptionFromNullOr(Schema.String), Schema.OptionFromNullOr(Schema.Literal(true)), Schema.OptionFromNullOr(Schema.Finite), Schema.OptionFromNullOr(Schema.Finite)] as const;
                const Flagged = Schema.Tuple([Schema.Boolean, ...Typed]);
                const stated = ([name, description, type, array, minimum, maximum]: Schema.Tuple<typeof Typed>['Type']) => ({
                    name,
                    description,
                    type,
                    array,
                    minimum,
                    maximum,
                    members: pipe(
                        declared({ type, description }),
                        Array.map(enumeration),
                        Array.getSomes,
                        Array.flatMap((found) => Array.map(members(found), String)),
                        Array.dedupe,
                        Option.liftPredicate(Array.isArrayNonEmpty),
                    ),
                    unavailable: unavailable(kind, name, Option.none(), false),
                });
                const parameter = ([optional, ...row]: typeof Flagged.Type) => ({ ...Struct.omit(stated(row), ['unavailable']), optional: optional || Option.exists(row[1], (description) => description.endsWith('(Optional)')) });
                const Reader = Schema.Tuple([Schema.OptionFromNullOr(Schema.String), Schema.Array(Flagged), Schema.Array(Schema.Tuple([Schema.Array(Flagged), ...Typed])), Schema.Array(Flagged)]).pipe(
                    Schema.decodeTo(Schema.toType(Class), {
                        decode: SchemaGetter.transform(([parent, properties, methods, statics]) => ({
                            parent,
                            fields: Record.fromIterableWith(properties, ([readonly, ...row]) => [row[0], { ...stated(row), settable: !readonly }]),
                            methods: Record.fromIterableWith(methods, ([parameters, ...row]) => {
                                const { name, description, type, array } = stated(row);
                                return [name, { name, description, parameters: Array.map(parameters, parameter), returns: Option.as(type, { type, array }) }];
                            }),
                            statics: Record.getSomes(Record.fromIterableWith(statics, ([, name, description]) => [name, description])),
                        })),
                        encode: SchemaGetter.forbiddenEncoding,
                    }),
                );
                return Effect.try({ try: () => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind]), catch: (cause) => new Cause.UnknownError(cause, `dictionary class ${kind}`) }).pipe(
                    Effect.flatMap(flow(Schema.decodeUnknownEffect(Schema.NullOr(Reader)), Effect.orDie)),
                    Effect.filterOrFail(Predicate.isNotNull, () => Unmatched.make({ subject: 'class', query: Option.some(kind), candidates: classes })),
                );
            },
        });
        const lineage = (name: string): Effect.Effect<readonly string[], Unmatched | Cause.UnknownError> => Effect.flatMap(Cache.get(rows, name), ({ parent }) => Option.match(parent, { onNone: () => Effect.succeed([]), onSome: (above) => Effect.map(lineage(above), (rest) => [above, ...rest]) }));
        const element = (kind: string) => Effect.map(Cache.get(rows, kind), ({ methods }) => Option.flatMap(Record.get(methods, 'item'), ({ returns }) => Option.flatMap(returns, Struct.get('type'))));
        const catalog = yield* Cache.make({
            capacity: exported.length,
            lookup: Effect.fnUntraced(function* (kind: string) {
                const { fields } = yield* Cache.get(rows, 'Document');
                const listed = Array.findFirst(Record.toEntries(fields), ([, { array, type }]) => Option.isSome(array) && Option.contains(type, kind));
                const held = pipe(
                    Record.map(fields, ({ type }) => Option.filter(type, listing)),
                    Record.getSomes,
                    Record.toEntries,
                    Effect.findFirst(([, each]) => Effect.map(element(each), Option.contains(kind))),
                    Effect.map(Option.map(([field]) => field)),
                );
                return yield* Option.match(listed, { onSome: ([field]) => Effect.succeedSome(field), onNone: () => held });
            }),
        });
        const extended = <S extends Schema.Top>(schema: S, at: string, source: string, ...given: readonly string[]) =>
            Effect.try({ try: (): unknown => indesign.app.doScript(source, indesign.ScriptLanguage.JAVASCRIPT, [...given]), catch: (cause) => new Cause.UnknownError(cause, at) }).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(schema), Effect.orDie)));
        const folders = Effect.map(extended(Schema.Tuple([Schema.String, Schema.String, Schema.String]), 'settings and presets folders', '[app.scriptPreferences.scriptsFolder.parent.parent.fsName, app.filePath.fsName + "/Presets", $.locale]'), ([settings, presets, locale]) => ({ settings, presets, locale }));
        const file = (owner: InDesignDOMObject<unknown>, field: string) => Effect.map(extended(Schema.NullOr(Schema.String), `${owner.toSpecifier()} ${field}`, 'var value = resolve(arguments[0])[arguments[1]]; value === null ? null : value.fsName', owner.toSpecifier(), field), Option.fromNullOr);
        const temporary = (name: string) =>
            Effect.acquireRelease(
                Effect.zipWith(extended(Schema.String, `temporary ${name}`, 'Folder.temp.fsName'), Random.nextInt, (folder, unique) => `${folder}/${unique}-${name}`),
                (path) => Effect.orDie(extended(Schema.Boolean, `removal of ${path}`, 'File(arguments[0]).remove()', path)),
            );
        return Object.assign(model, { exported, classes, enumerations, owning, host, collection, named, document, page, spread, item, text, frame, enumeration, members, declared, plural, unavailable, rows, lineage, catalog, folders, file, temporary });
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Field, Member, Method, Model };
