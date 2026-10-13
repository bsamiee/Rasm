import type { Enumeration, Enumerator } from 'adobe:indesign';
import indesign from 'adobe:indesign';
import { storage } from 'adobe:uxp';
import { Array, Cache, Context, Effect, flow, identity, Layer, Match, Option, Predicate, pipe, Record, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { isNonEmpty } from 'effect/String';
import { GalleyError, Inspect, type Refusal } from './contract.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Untranslatable = Pick<(typeof Refusal.cases.Untranslatable)['Type'], 'pattern' | 'constructs'>;
type Row = readonly [path: readonly string[], field: string, name: string, value: unknown, candidates: readonly (readonly [enumeration: string, member: string])[]];
type Checks = Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (value: unknown) => Effect.Effect<readonly [value: unknown, notes: readonly string[]], Untranslatable>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEX = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const LAST_STEP = /\/+[^/]+$/u;
const SEPARATORS = /[^a-z0-9]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Callable = Schema.declare(Predicate.isFunction);
const Hosted = Schema.StructWithRest(Schema.Struct({ constructorName: Schema.String, toSpecifier: Callable }), [Schema.Record(Schema.String, Schema.Unknown)]);
const Collection = Schema.Struct({ constructorName: Schema.String, length: Schema.Int, item: Callable, itemByRange: Callable, everyItem: Callable });
const Typed = [Schema.String, Schema.String, Schema.String, Schema.Boolean, Schema.Array(Schema.Finite)] as const;
const Parameter = Schema.Tuple([...Typed, Schema.Boolean]);
const Rows = Schema.NullOr(Schema.Tuple([Schema.String, Schema.Array(Parameter), Schema.Array(Schema.Tuple([...Typed, Schema.Array(Parameter)])), Schema.Array(Parameter)]));

type Host = typeof Hosted.Type;

// --- [SERVICES] ------------------------------------------------------------------------

class Temporary extends Context.Service<Temporary, string>()('@rasm/galley/Temporary') {
    static readonly layer = Layer.effect(this, Effect.promise(() => storage.localFileSystem.getTemporaryFolder()).pipe(Effect.map(Struct.get('nativePath'))));
}
class Model extends Context.Service<Model>()('@rasm/galley/Model', {
    make: Effect.gen(function* () {
        const { app, ScriptLanguage } = indesign;
        const reader =
            'function rows(list, more) { var out = []; for (var index = 0; index < list.length(); index++) { var row = list[index]; out.push([String(row.@name), String(row.shortdesc), String(row.datatype.type), row.datatype.array.length() > 0, row.datatype.min.length() > 0 ? [Number(row.datatype.min), Number(row.datatype.max)] : [], more(row)]); } return out; } var found = $.dictionary.getClass(arguments[0]); found === null ? null : (function (model) { var members = model.elements.(@type == "instance"); return [String(model.superclass), rows(members.property, function (row) { return row.@rwaccess == "readonly"; }), rows(members.method, function (row) { return rows(row.parameters.parameter, function (row) { return row.@optional == "true"; }); }), rows(model.elements.(@type == "class").property, function () { return true; })]; })(found.toXML());';
        const { Class } = Inspect.successSchema.cases;
        const [Field, Method] = [Class.fields.fields.value, Class.fields.methods.value];
        const stated = ([name, description, type, array, [minimum, maximum]]: readonly [string, string, string, boolean, readonly number[], unknown]): Omit<typeof Field.Type, 'settable'> => {
            const choices: unknown = Reflect.get(indesign, type);
            return {
                name,
                description: Option.liftPredicate(description, isNonEmpty),
                type: Option.liftPredicate(type, isNonEmpty),
                array: Option.liftPredicate(true as const, () => array),
                minimum: Option.fromUndefinedOr(minimum),
                maximum: Option.fromUndefinedOr(maximum),
                members: Option.liftPredicate(choices instanceof indesign.Enumeration ? Array.map(members(choices), String) : Array.empty<string>(), Array.isArrayNonEmpty),
            };
        };
        return yield* Cache.make({
            capacity: classes.size,
            lookup: (kind: string) =>
                read(Rows, () => app.doScript(reader, ScriptLanguage.JAVASCRIPT, [kind])).pipe(
                    Effect.flatMap(Effect.fromNullishOr),
                    Effect.mapError(() => missing(`class ${kind}`, kind, [...classes])),
                    Effect.map(([parent, properties, methods, statics]) => ({
                        parent,
                        fields: Record.fromIterableWith(properties, (row) => [row[0], Schema.encodeSync(Field)({ ...stated(row), settable: Option.some(!row[5]) })]),
                        methods: Record.fromIterableWith(methods, ([name, description, type, array, , parameters]) => [
                            name,
                            Schema.encodeSync(Method)({
                                name,
                                description: Option.liftPredicate(description, isNonEmpty),
                                parameters: Option.some(Array.map(parameters, (row) => ({ ...stated(row), optional: row[5] || row[1].endsWith('(Optional)') }))),
                                returns: Option.map(Option.liftPredicate(type, isNonEmpty), (returned) => ({ type: Option.some(returned), array: Option.liftPredicate(true as const, () => array) })),
                            }),
                        ]),
                        statics: Record.fromIterableWith(statics, ([name, description]) => [name, description]),
                    })),
                ),
        });
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
const owning = (member: string): readonly (readonly [string, { readonly prototype: unknown }])[] => Array.flatMap(exported, ([name, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [[name, type] as const] : []));
const host = (
    (held: ReadonlySet<unknown>) =>
    (value: unknown): value is Host =>
        Predicate.isObject(value) && held.has(Object.getPrototypeOf(value))
)(new Set(Array.map(owning('toSpecifier'), ([, type]) => type.prototype)));
const classes = new Set(Array.flatMap(exported, ([name, type]) => (Predicate.isFunction(type) ? [name] : [])));
const collected = owning('everyItem');
const plurals = new Set(Array.map(collected, ([name]) => name));
const collection = (
    (held: ReadonlySet<unknown>) =>
    (value: unknown): value is typeof Collection.Type =>
        Predicate.isObject(value) && held.has(Object.getPrototypeOf(value))
)(new Set(Array.map(collected, ([, type]) => type.prototype)));
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const thrown = (cause: unknown): GalleyError => new GalleyError({ reason: { _tag: 'Thrown', cause, line: Option.none(), source: Option.none(), corrections: Option.none(), kept: false } });
const missing = (subject: string, query: string, candidates: readonly string[]): GalleyError => new GalleyError({ reason: { _tag: 'Missing', subject, query, candidates, range: Option.none() } });
const members = (enumeration: Enumeration): readonly Enumerator[] =>
    Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (key) => {
        const member: unknown = Reflect.get(enumeration, key);
        return member instanceof indesign.Enumerator && String(member) === key ? [member] : [];
    });
const named = ((): ((value: string) => readonly (readonly [enumeration: string, member: Enumerator])[]) => {
    const plain = (text: string): string => text.toLowerCase().replaceAll(SEPARATORS, '');
    const shared = (lists: readonly (readonly string[])[], word: (words: readonly string[], depth: number) => string | undefined, kept: number, depth: number): number =>
        lists.length > 1 && Array.every(lists, (words) => words.length > depth + kept + 1) && new Set(Array.map(lists, (words) => word(words, depth))).size === 1 ? shared(lists, word, kept, depth + 1) : depth;
    const keyed = Array.groupBy(
        Array.flatMap(exported, ([name, enumeration]) => {
            const held = enumeration instanceof indesign.Enumeration ? members(enumeration) : [];
            const lists = Array.map(held, (member) => String(member).split('_'));
            const suffix = shared(lists, (words, depth) => words.at(-1 - depth), 0, 0);
            const prefix = shared(lists, (words, depth) => words[depth], suffix, 0);
            return Array.flatMap(Array.zip(lists, held), ([words, member]) => [[plain(words.join('')), [name, member] as const] as const, [plain(words.slice(prefix, words.length - suffix).join('')), [name, member] as const] as const]);
        }),
        ([key]) => key,
    );
    return (value) =>
        pipe(
            Record.get(keyed, plain(value)),
            Option.getOrElse(Array.empty),
            Array.map(([, pair]) => pair),
            Array.dedupeWith(([, left], [, right]) => left.equals(right)),
        );
})();
const encoded = (value: unknown, labeled: boolean): string | undefined =>
    JSON.stringify(value, (_key, part: unknown) =>
        Match.value(part).pipe(
            Match.when(Match.instanceOf(indesign.Enumerator), String),
            Match.when(collection, (items) => (labeled ? { type: items.constructorName, length: items.length } : Option.getOrNull(Option.map(Option.fromNullishOr(items.everyItem()), (every) => every.toSpecifier())))),
            Match.when(host, (object) => (labeled ? { type: object.constructorName || undefined, specifier: object.toSpecifier(), invalid: object.isValid === false || undefined } : object.toSpecifier())),
            Match.when(Predicate.isMap, Object.fromEntries),
            Match.when(Predicate.isSet, (entries) => [...entries]),
            Match.when(Predicate.isBigInt, String),
            Match.orElse(identity),
        ),
    );
const read = <S extends Schema.Top>(schema: S, value: () => unknown): Effect.Effect<S['Type'], GalleyError, S['DecodingServices']> => Effect.flatMap(Effect.try({ try: () => encoded(value(), false), catch: thrown }), (text) => Effect.orDie(Schema.decodeUnknownEffect(Schema.fromJsonString(schema))(text)));
const active = Effect.try({ try: () => indesign.app.activeDocument, catch: () => missing('document', '', indesign.app.documents.everyItem().name) });
const located = (path: string): Effect.Effect<Host | readonly Host[], unknown> =>
    Effect.try({ try: (): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]), catch: identity }).pipe(Effect.filterOrFail((found): found is Host | readonly Host[] => host(found) || (Array.isArray(found) && Array.every(found, host)), identity));
const corrections = (value: unknown): Effect.Effect<readonly unknown[]> => (Predicate.isString(value) ? Effect.map(Effect.option(located(value)), (object) => [...Option.toArray(object), ...Array.map(named(value), ([, member]) => member)]) : Effect.succeed([]));
const wired = (value: unknown): unknown =>
    Match.value(value).pipe(
        Match.when(Schema.is(Schema.Array(Schema.Unknown)), (items) => ['[', ...Array.map(items, wired)]),
        Match.when(record, (fields) => ['{', ...Array.flatMap(Object.entries(fields), ([field, held]) => [field, wired(held)])]),
        Match.orElse(identity),
    );
const unwired = (value: unknown): unknown =>
    Match.value(value).pipe(
        Match.when(Schema.is(Schema.Array(Schema.Unknown)), ([tag, ...items]) => (tag === '[' ? Array.map(items, unwired) : Object.fromEntries(Array.map(Array.chunksOf(items, 2), ([field, held]) => [String(field), unwired(held)])))),
        Match.orElse(identity),
    );

const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    const listed = (path: string): Effect.Effect<readonly Host[]> => Effect.map(Effect.option(located(path)), flow(Option.toArray, Array.flatMap(Array.ensure)));
    const kin = (container: Host, sibling: Host, name: string): Effect.Effect<readonly Host[]> =>
        Effect.map(Effect.option(Cache.get(model, container.constructorName)), (shape) =>
            Array.flatMap(Option.toArray(shape), ({ fields }) => Array.flatMap(Record.toEntries(fields), ([field, { type, array }]) => (type === sibling.constructorName && array === true ? Array.filter(Array.filter(Array.ensure<unknown>(container[field]), host), (each) => each.name === name) : []))),
        );
    return yield* Effect.catch(located(specifier), () =>
        Effect.gen(function* () {
            const [siblings, [container]] = yield* Effect.all([listed(specifier.replace(SELECTOR, '')), listed(specifier.replace(LAST_STEP, '') || '/')]);
            const indexed = INDEX.test(specifier);
            const sought = Option.flatMapNullishOr(Option.fromNullishOr(NAMED.exec(specifier)), ({ groups }) => groups?.name);
            const related = yield* Option.match(Option.all([Option.fromUndefinedOr(container), Array.head(siblings), sought]), { onNone: () => Effect.succeed([]), onSome: ([owner, sibling, name]) => kin(owner, sibling, name) });
            return yield* Effect.fail(
                new GalleyError({
                    reason: {
                        _tag: 'Missing',
                        subject: specifier,
                        query: '',
                        candidates: indexed ? [] : Array.dedupe(Array.map([...siblings, ...related], (each): string => each.toSpecifier())),
                        range: Option.liftPredicate({ first: 0, last: siblings.length - 1 }, () => indexed && siblings.length > 0),
                    },
                }),
            );
        }),
    );
});
const leaves = (owner: object, fields: Record.ReadonlyRecord<string, unknown>, path: readonly string[]): readonly { readonly owner: object; readonly path: readonly string[]; readonly type: string; readonly field: string; readonly name: string; readonly value: unknown }[] =>
    Array.flatMap(Object.entries(fields), ([field, value]) => {
        const inner: unknown = record(value) ? Reflect.get(owner, field) : undefined;
        return record(value) && host(inner) ? leaves(inner, value, [...path, field]) : [{ owner, path, type: host(owner) ? owner.constructorName : '', field, name: Array.join([...path, field], '.'), value }];
    });
const screened =
    (checks: Checks, at: (typeof Refusal.Type)['target']) =>
    ({ type, field, name, value }: { readonly type: string; readonly field: string; readonly name: string; readonly value: unknown }): Effect.Effect<readonly [unknown, readonly string[]], typeof Refusal.Type> =>
        Option.match(Option.flatMap(Record.get(checks, type), Record.get(field)), {
            onNone: () => Effect.succeed([value, []] as const),
            onSome: (check) =>
                Effect.mapBoth(check(value), {
                    onFailure: (refused): typeof Refusal.Type => ({ _tag: 'Untranslatable', target: at, field: name, ...refused }),
                    onSuccess: ([given, notes]) => [given, Array.map(notes, (text) => `${name}: ${text}`)] as const,
                }),
        });
const prepared = (owner: object | string, values: Record.ReadonlyRecord<string, unknown>, checks: Checks): Effect.Effect<{ readonly rows: readonly Row[]; readonly refusals: readonly (typeof Refusal.Type)[]; readonly notes: readonly string[] }> => {
    const listed: readonly { readonly path: readonly string[]; readonly type: string; readonly field: string; readonly name: string; readonly value: unknown }[] = Predicate.isString(owner)
        ? Array.map(Object.entries(values), ([field, value]) => ({ path: [], type: owner, field, name: field, value }))
        : leaves(owner, values, []);
    return Effect.map(
        Effect.partition(listed, (leaf) =>
            Effect.map(
                screened(
                    checks,
                    Option.map(Option.liftPredicate(owner, host), (root) => root.toSpecifier()),
                )(leaf),
                ([value, notes]) => ({ notes, row: [leaf.path, leaf.field, leaf.name, value, Predicate.isString(value) ? Array.map(named(value), ([enumeration, member]) => [enumeration, String(member)] as const) : []] as const }),
            ),
        ),
        ([kept, refusals]) => ({ refusals, notes: Array.flatMap(kept, Struct.get('notes')), rows: Array.map(kept, Struct.get('row')) }),
    );
};
const write = Effect.fn('write')(function* (target: object, values: Record.ReadonlyRecord<string, unknown>, checks: Checks) {
    const at = Option.map(Option.liftPredicate(target, host), (owner) => owner.toSpecifier());
    const written = yield* Effect.validate(leaves(target, values, []), (leaf) => {
        const { owner, field, name } = leaf;
        const put = (candidate: unknown): Effect.Effect<object, unknown> => Effect.try({ try: () => Object.assign(owner, { [field]: candidate }), catch: identity });
        const placed = (given: unknown): Effect.Effect<Option.Option<string>, typeof Refusal.Type> =>
            Effect.catch(Effect.as(put(given), Option.none<string>()), (cause) =>
                Effect.flatMap(
                    Effect.flatMap(corrections(given), Effect.findFirst(flow(put, Effect.isSuccess))),
                    Option.match({
                        onNone: () => Effect.fail<typeof Refusal.Type>({ _tag: 'Refused', target: at, field: name, cause }),
                        onSome: (accepted) => Effect.succeed(Option.liftPredicate(`${name} = ${JSON.stringify(given)} assigned ${String(accepted)}`, () => accepted instanceof indesign.Enumerator)),
                    }),
                ),
            );
        return Effect.flatMap(screened(checks, at)(leaf), ([given, noted]) => Effect.map(placed(given), (assigned) => [...noted, ...Option.toArray(assigned)]));
    }).pipe(Effect.mapError((fields) => new GalleyError({ reason: { _tag: 'Rejected', fields } })));
    return Array.flatten(written);
});
const scope = Effect.acquireRelease(
    Effect.sync(() => {
        const settings = indesign.app.scriptPreferences;
        const { userInteractionLevel, enableRedraw, measurementUnit } = settings;
        Object.assign(settings, { userInteractionLevel: indesign.UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: indesign.MeasurementUnits.POINTS });
        return { settings, saved: { userInteractionLevel, enableRedraw, measurementUnit } };
    }),
    ({ settings, saved }) => Effect.sync(() => Object.assign(settings, saved)),
);
const inspect = Effect.fn('inspect')(function* (target: string, fields: Rpc.Payload<typeof Inspect>['fields']) {
    const { app, ScriptLanguage } = indesign;
    const model = yield* Model;
    const styling =
        'var target = resolve(arguments[0]), names = [], kept = [], done = false, methods = ["clearOverrides", "clearObjectStyleOverrides", "clearCellStyleOverrides", "clearTableStyleOverrides"]; for (var at = 1; at < arguments.length; at++) names.push(arguments[at]); try { app.doScript(function () { var cleared = 0; for (var each = 0; each < methods.length; each++) if (typeof target[methods[each]] === "function") { target[methods[each]](); cleared++; } kept = cleared === 0 ? null : []; for (var name = 0; kept !== null && name < names.length; name++) kept.push(target[names[name]]); done = true; throw 0; }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT); } catch (error) { if (!done) throw error; } kept';
    const names = Array.map(exported, ([name]) => name);
    const wanted = Option.getOrElse(fields, (): readonly string[] => []);
    const typed = (type: string | undefined): Option.Option<string> => Option.filter(Option.fromUndefinedOr(type), (name) => classes.has(name));
    const document = (value: unknown): Option.Option<Host> => Option.flatMap(Option.liftPredicate(value, host), (owner) => (owner.constructorName === 'Document' ? Option.some(owner) : document(owner.parent)));
    const parents = (name: string): Effect.Effect<readonly string[]> => Effect.flatMap(Effect.option(Cache.get(model, name)), Option.match({ onNone: () => Effect.succeed([]), onSome: ({ parent }) => (isNonEmpty(parent) ? Effect.map(parents(parent), (above) => [parent, ...above]) : Effect.succeed([])) }));
    const expanded = (value: unknown): Effect.Effect<unknown> => (host(value) ? Effect.map(Effect.option(Cache.get(model, value.constructorName)), (shape) => (Option.exists(shape, ({ parent }) => parent === 'Preference') ? value.properties : value)) : Effect.succeed(value));
    const taken = (owner: Host): Effect.Effect<readonly (readonly [string, unknown])[], GalleyError> => Effect.forEach(wanted, (name) => Effect.flatMap(Effect.try({ try: (): unknown => owner[name], catch: thrown }), (value) => Effect.map(expanded(value), (shown) => [name, shown] as const)));
    const items = (found: readonly Host[]): Effect.Effect<() => unknown, GalleyError> =>
        Effect.map(Effect.forEach(found, taken), (values) => () => ({
            _tag: 'Items',
            specifier: target,
            total: found.length,
            items: Array.zipWith(found, values, (item, entries) => ({ type: item.constructorName, specifier: item, name: item.name, values: Object.fromEntries(entries) })),
        }));
    const object = Effect.fnUntraced(function* (found: Host) {
        const entries = yield* Option.match(fields, { onSome: () => taken(found), onNone: () => Effect.try({ try: () => Object.entries(record(found.properties) ? found.properties : {}), catch: thrown }) });
        const own = Option.map(yield* Effect.option(Cache.get(model, found.constructorName)), Struct.get('fields'));
        const row = (name: string): (typeof Inspect.successSchema.cases.Class.fields.fields.value)['Encoded'] => Option.getOrElse(Option.flatMap(own, Record.get(name)), () => ({ name }));
        const cleared = yield* read(Schema.NullOr(Schema.Array(Schema.Json)), () => app.doScript(styling, ScriptLanguage.JAVASCRIPT, [found.toSpecifier(), ...Array.map(entries, ([name]) => name)]));
        const styled = Record.fromIterableWith(Array.zip(entries, cleared ?? []), ([[name], value]) => [name, value]);
        const container = document(found);
        const accessors = Option.isSome(container) ? Option.match(yield* Effect.option(Cache.get(model, 'Document')), { onNone: () => [], onSome: ({ fields: shape }) => Record.toEntries(shape) }) : [];
        const held = Record.fromEntries(
            Array.getSomes(
                yield* Effect.forEach(accessors, ([accessor, { type }]) =>
                    Effect.map(Effect.transposeOption(Option.map(typed(type), (plural) => Effect.option(Cache.get(model, plural)))), (shape) =>
                        Option.map(
                            Option.flatMap(Option.flatten(shape), ({ methods }) => Option.flatMap(Record.get(methods, 'item'), ({ returns }) => Option.fromUndefinedOr(returns?.type))),
                            (element) => [element, accessor] as const,
                        ),
                    ),
                ),
            ),
        );
        return (): unknown => {
            const rest = Option.match(fields, {
                onSome: () => [],
                onNone: () =>
                    Array.difference(
                        Object.getOwnPropertyNames(Object.getPrototypeOf(found)),
                        Array.map(entries, ([name]) => name),
                    ),
            });
            const runs = Array.filter([found.textStyleRanges], collection);
            const columns = Array.flatMap(runs, (ranges) =>
                Array.flatMap(Array.fromNullishOr(ranges.everyItem()), (every) =>
                    Option.match(fields, {
                        onSome: Array.map((name): readonly [string, unknown] => [name, every[name]]),
                        onNone: () => {
                            const listed = Array.filter(Array.ensure<unknown>(every.properties), record);
                            return Array.map(Object.keys(listed[0] ?? {}), (name): readonly [string, unknown] => [name, Array.map(listed, (each) => each[name])]);
                        },
                    }),
                ),
            );
            const mixed = Record.filter(
                Record.fromIterableWith(columns, ([name, column]) => [name, [...new Map(Array.map(Array.ensure(column), (each) => [encoded(each, false), each])).values()]]),
                (distinct) => distinct.length > 1,
            );
            const listing = (name: string, value: unknown): readonly unknown[] => (collection(value) && value.length > 0 ? [{ name, count: value.length, specifier: value }] : []);
            return {
                _tag: 'Object',
                type: found.constructorName,
                specifier: found,
                fields: Array.flatMap(entries, ([name, value]) =>
                    collection(value)
                        ? []
                        : [
                              {
                                  ...row(name),
                                  value: value ?? null,
                                  mixed: Record.get(mixed, name).pipe(Option.getOrUndefined),
                                  style: Option.getOrUndefined(Option.filter(Record.get(styled, name), (base) => JSON.stringify(base) !== encoded(value, false))),
                                  source: pipe(
                                      typed(row(name).type),
                                      Option.orElse(() => (host(value) ? Option.some(value.constructorName) : Option.none())),
                                      Option.flatMap((type) => Record.get(held, type)),
                                      Option.flatMap((accessor) => Option.map(container, (owner) => owner[accessor])),
                                      Option.getOrUndefined,
                                  ),
                              },
                          ],
                ),
                collections: [...Array.flatMap(entries, ([name, value]) => listing(name, value)), ...Array.flatMap(rest, (name) => Option.getOrElse(Option.liftThrowable(() => listing(name, found[name]))(), () => []))],
            };
        };
    });
    const member = Option.flatMap(
        Array.findFirst(names, (name) => name.toLowerCase() === target.toLowerCase()),
        (name) =>
            Match.value<unknown>(Reflect.get(indesign, name)).pipe(
                Match.when(Match.instanceOf(indesign.Enumeration), (enumeration) =>
                    Effect.map(Effect.option(Cache.get(model, name)), (shape) => (): unknown => ({
                        _tag: 'Enumeration',
                        name,
                        members: Array.map(members(enumeration), (each) => ({
                            name: String(each),
                            description: Option.getOrUndefined(
                                Option.filter(
                                    Option.flatMap(shape, ({ statics }) => Record.get(statics, String(each))),
                                    isNonEmpty,
                                ),
                            ),
                        })),
                    })),
                ),
                Match.when(Predicate.isFunction, (type) =>
                    Effect.map(Effect.all([Cache.get(model, name), parents(name)]), ([{ fields: held, methods: calls }, lineage]) => (): unknown => {
                        const live = Object.entries(Object.getOwnPropertyDescriptors(type.prototype));
                        return {
                            _tag: 'Class',
                            name,
                            parents: lineage,
                            fields: Array.flatMap(live, ([key, { get }]) => (get === undefined ? [] : [Option.getOrElse(Record.get(held, key), () => ({ name: key }))])),
                            methods: Array.flatMap(live, ([key, { value }]) => (Predicate.isFunction(value) && value !== type ? [Option.getOrElse(Record.get(calls, key), () => ({ name: key }))] : [])),
                        };
                    }),
                ),
                Match.option,
            ),
    );
    const raw = yield* Option.match(member, {
        onSome: identity,
        onNone: () =>
            resolve(target).pipe(
                Effect.mapError((error) => (error.reason._tag === 'Missing' && Array.isReadonlyArrayEmpty(error.reason.candidates) && Option.isNone(error.reason.range) ? missing(target, target, names) : error)),
                Effect.flatMap((found) =>
                    host(found)
                        ? Option.match(
                              Array.findFirst(wanted, (name) => !(name in found)),
                              {
                                  onSome: (name) => Effect.fail(missing(`${name} of ${target}`, name, Object.getOwnPropertyNames(Object.getPrototypeOf(found)))),
                                  onNone: () => object(found),
                              },
                          )
                        : items(found),
                ),
            ),
    });
    return yield* read(Inspect.successSchema, raw);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, Callable, type Checks, collection, encoded, type Host, host, inspect, LAST_STEP, Model, missing, plurals, prepared, read, record, resolve, scope, Temporary, thrown, type Untranslatable, unwired, wired, write };
