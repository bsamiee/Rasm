import type { BaseCollection, Document, Enumeration, Enumerator, InDesignDOMObject, Page, PageItem, ReadonlyNamedDOMObject, Spread, Text, TextFrame } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Cache, Cause, Context, Effect, flow, Option, Predicate, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import { Class, declared, type Member, unavailable } from './dictionary.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/dom/model', {
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
        const listing = (kind: string): boolean => {
            const found: unknown = Reflect.get(indesign, kind);
            return Predicate.isFunction(found) && Object.hasOwn(found.prototype, 'everyItem');
        };
        const plural = ({ type }: Pick<typeof Member.Type, 'type'>): boolean => Option.exists(type, listing);
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
        return Object.assign(model, { exported, classes, enumerations, owning, host, collection, named, document, page, spread, item, text, frame, enumeration, members, plural, rows, lineage, catalog });
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Model };
