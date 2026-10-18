import indesign, { type Enumeration, type Enumerator } from 'adobe:indesign';
import { Unmatched } from '@rasm/mcp/link';
import { Handoff } from '@rasm/uxp/runtime';
import { Array, Cache, Context, Deferred, Effect, Exit, flow, identity, Layer, Match, Option, Predicate, pipe, Record, Schema, type Scope } from 'effect';
import { InDesignError, type Refusal, Specifier } from './contract.ts';
import { translated } from './grep.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Host {
    readonly constructorName: string;
    readonly toSpecifier: () => string;
    readonly [field: string]: unknown;
}
interface Collection {
    readonly constructorName: string;
    readonly length: number;
    readonly item: (key: unknown) => unknown;
    readonly everyItem: () => Host | undefined;
    readonly add: () => unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;
const SEPARATORS = /[^a-z0-9]/gu;

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/dom/Model', {
    make: Cache.make({
        capacity: Object.getOwnPropertyNames(indesign).length,
        lookup: Effect.fnUntraced(function* (kind: string) {
            const reader =
                'function rows(list, more) { var out = []; for (var index = 0; index < list.length(); index++) { var row = list[index]; out.push([String(row.@name), String(row.shortdesc), String(row.datatype.type), row.datatype.array.length() > 0, row.datatype.min.length() > 0 ? [Number(row.datatype.min), Number(row.datatype.max)] : [], more(row)]); } return out; } var found = $.dictionary.getClass(arguments[0]); found === null ? null : (function (model) { var members = model.elements.(@type == "instance"); return [String(model.superclass), rows(members.property, function (row) { return row.@rwaccess == "readonly"; }), rows(members.method, function (row) { return rows(row.parameters.parameter, function (row) { return row.@optional == "true"; }); }), rows(model.elements.(@type == "class").property, function () { return true; })]; })(found.toXML());';
            const typed = [Schema.String, Schema.String, Schema.String, Schema.Boolean, Schema.Array(Schema.Finite)] as const;
            const parameter = Schema.Tuple([...typed, Schema.Boolean]);
            const Rows = Schema.NullOr(Schema.Tuple([Schema.String, Schema.Array(parameter), Schema.Array(Schema.Tuple([...typed, Schema.Array(parameter)])), Schema.Array(parameter)]));
            const stated = ([name, description, type, array, [minimum, maximum]]: readonly [string, string, string, boolean, readonly number[], unknown]) => {
                const choices: unknown = Reflect.get(indesign, type);
                const listed = choices instanceof indesign.Enumeration ? Array.map(members(choices), String) : [];
                return { name, description: description || undefined, type: type || undefined, array: array || undefined, minimum, maximum, members: Array.isArrayNonEmpty(listed) ? listed : undefined };
            };
            const [parent, properties, methods, statics] = yield* read(Rows, () => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind])).pipe(
                Effect.flatMap(Effect.fromNullishOr),
                Effect.mapError(() => Unmatched.make({ subject: 'class', query: kind, candidates: classes })),
            );
            const signatures = Array.map(methods, ([, , , , , parameters]) => Array.map(parameters, (each) => ({ ...stated(each), optional: each[5] || each[1].endsWith('(Optional)') })));
            return {
                parent,
                fields: Record.fromIterableWith(properties, (row) => [row[0], { ...stated(row), settable: !row[5] }]),
                methods: Record.fromIterableWith(Array.zip(methods, signatures), ([row, parameters]) => {
                    const { name, description, type, array } = stated(row);
                    return [name, { name, description, parameters, returns: type === undefined ? undefined : { type, array } }];
                }),
                statics: Record.fromIterableWith(statics, ([name, description]) => [name, description]),
            };
        }),
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VALUES]
const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
const owning = <A>(member: string): readonly [names: ReadonlySet<string>, owns: <T>(value: T) => value is T & A] => {
    const owners = Array.flatMap(exported, ([name, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [[name, type.prototype] as const] : []));
    const prototypes = new Set(Array.map(owners, ([, prototype]) => prototype));
    return [new Set(Array.map(owners, ([name]) => name)), <T>(value: T): value is T & A => Predicate.isObject(value) && prototypes.has(Object.getPrototypeOf(value))];
};
const [[, host], [plurals, collection]] = [owning<Host>('toSpecifier'), owning<Collection>('everyItem')];
const classes = Array.flatMap(exported, ([name, type]) => (Predicate.isFunction(type) ? [name] : []));
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const members = (enumeration: Enumeration): readonly Enumerator[] =>
    Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (key) => {
        const member: unknown = Reflect.get(enumeration, key);
        return member instanceof indesign.Enumerator && String(member) === key ? [member] : [];
    });
const named = ((): ((value: string) => readonly Enumerator[]) => {
    const plain = (text: string): string => text.toLowerCase().replaceAll(SEPARATORS, '');
    const shared = (lists: readonly (readonly string[])[], word: (words: readonly string[], depth: number) => string | undefined, kept: number, depth: number): number =>
        lists.length > 1 && lists.every((words) => words.length > depth + kept + 1) && new Set(Array.map(lists, (words) => word(words, depth))).size === 1 ? shared(lists, word, kept, depth + 1) : depth;
    const keyed = Array.groupBy(
        Array.flatMap(exported, ([, enumeration]) => {
            const held = enumeration instanceof indesign.Enumeration ? members(enumeration) : [];
            const lists = Array.map(held, (member) => String(member).split('_'));
            const suffix = shared(lists, (words, depth) => words.at(-1 - depth), 0, 0);
            const prefix = shared(lists, (words, depth) => words[depth], suffix, 0);
            return Array.flatMap(Array.zip(lists, held), ([words, member]) => [[plain(words.join('')), member] as const, [plain(words.slice(prefix, words.length - suffix).join('')), member] as const]);
        }),
        ([key]) => key,
    );
    return (value) =>
        pipe(
            Record.get(keyed, plain(value)),
            Option.getOrElse(Array.empty),
            Array.map(([, member]) => member),
            Array.dedupeWith((left, right) => left.equals(right)),
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
const read = <S extends Schema.Top>(schema: S, value: () => unknown): Effect.Effect<S['Type'], InDesignError, S['DecodingServices']> => Effect.flatMap(Effect.try({ try: () => encoded(value(), false), catch: InDesignError.thrown }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie));
const active = Effect.try({ try: () => indesign.app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: indesign.app.documents.everyItem().name }) });
const located = (path: string): Option.Option<Host | readonly Host[]> => {
    const kind = (specifier: string): string | undefined => IDENTIFIED.exec(specifier)?.groups?.kind;
    return Option.filter(Option.liftThrowable((): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is Host | readonly Host[] =>
        (Array.isArray(found) ? found : [found]).every((each) => host(each) && (kind(path) === undefined || kind(each.toSpecifier()) === kind(path))),
    );
};

// --- [OBJECTS]
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const [model, found] = [yield* Model, located(specifier)];
    const listing = (value: unknown): readonly Host[] => (Array.isArray(value) ? value : [value]).filter(host);
    return yield* Option.match(found, {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const [siblings, [container]] = [Array.flatMap(Option.toArray(located(specifier.replace(SELECTOR, ''))), listing), Array.flatMap(Option.toArray(located(specifier.replace(LAST_STEP, '') || '/')), listing)];
            const [indexed, name, sibling] = [INDEXED.test(specifier), NAMED.exec(specifier)?.groups?.name, siblings[0]];
            const shape = container === undefined ? Option.none() : yield* Effect.option(Cache.get(model, container.constructorName));
            const holding = Option.match(shape, { onNone: () => [], onSome: ({ fields }) => Record.toEntries(fields) });
            const related = container === undefined || sibling === undefined ? [] : Array.flatMap(holding, ([field, { type, array }]) => (type === sibling.constructorName && array === true ? listing(container[field]) : []));
            return yield* Unmatched.make({
                subject: indexed && Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object',
                query: specifier,
                candidates: indexed ? [] : Array.dedupe(Array.map([...siblings, ...related.filter((each) => each.name === name)], (each) => each.toSpecifier())),
            });
        }),
    });
});
const held = (owner: object, values: Record.ReadonlyRecord<string, unknown>): Effect.Effect<void, never, Scope.Scope> =>
    Effect.asVoid(
        Effect.acquireRelease(
            Effect.sync(() => {
                const saved = Record.map(values, (_, key) => Reflect.get(owner, key));
                Object.assign(owner, values);
                return saved;
            }),
            (saved) => Effect.sync(() => Object.assign(owner, saved)),
        ),
    );

// --- [STEPS]
const stepped = Effect.fnUntraced(function* <A, E, R>(name: string, keep: boolean, body: Effect.Effect<A, E, R>) {
    const { app, ScriptLanguage, UndoModes } = indesign;
    const [done, run] = [yield* Deferred.make<A, E>(), Effect.runSyncWith(yield* Effect.context<R>())];
    const recorded = Effect.flatMap(Effect.exit(body), (exit) => Effect.andThen(Deferred.done(done, exit), keep ? exit : Effect.interrupt));
    const refused = yield* Effect.exit(Effect.try({ try: (): unknown => app.doScript(() => run(recorded), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name), catch: InDesignError.thrown }));
    yield* Exit.isFailure(refused) && !(yield* Deferred.isDone(done)) ? refused : Effect.void;
    return yield* Deferred.await(done);
});
const transacted = Effect.fn('transacted')(function* <A, E, R>(name: string, keep: boolean, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage } = indesign;
    yield* app.modalState ? Effect.fail(InDesignError.make({ reason: { _tag: 'Blocked' } })) : Effect.void;
    const [{ handed, entered }, opened] = [yield* Handoff, app.documents.everyItem().id];
    const wait = Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: InDesignError.thrown });
    const close = (id: number): Effect.Effect<void> => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
    return yield* handed(Effect.asVoid(wait), stepped(name, keep, Effect.andThen(entered, body))).pipe(Effect.onExit((exit) => Effect.forEach(keep && Exit.isSuccess(exit) ? [] : Array.difference(app.documents.everyItem().id, opened), close, { discard: true })));
});

// --- [WRITES]
const write = Effect.fn('write')(function* (target: Pick<Host, 'constructorName' | 'toSpecifier'>, values: Record.ReadonlyRecord<string, unknown>) {
    const { app, NothingEnum, SaveOptions } = indesign;
    const at = Option.some(Specifier.make(target.toSpecifier()));
    const leaves = (owner: Pick<Host, 'constructorName' | 'toSpecifier'>, fields: Record.ReadonlyRecord<string, unknown>, path: readonly string[]): readonly { readonly owner: Pick<Host, 'constructorName'>; readonly field: string; readonly name: string; readonly value: unknown }[] =>
        Array.flatMap(Object.entries(fields), ([field, value]) => {
            const inner: unknown = record(value) ? Reflect.get(owner, field) : undefined;
            return record(value) && host(inner) ? leaves(inner, value, [...path, field]) : [{ owner, field, name: [...path, field].join('.'), value }];
        });
    const sound = ([pattern, text]: readonly [string, string]): Effect.Effect<number, InDesignError> =>
        stepped(
            'Probe',
            false,
            Effect.sync(() => {
                const scratch = app.documents.add(false);
                try {
                    const frame = scratch.textFrames.add();
                    frame.contents = text;
                    app.findGrepPreferences = NothingEnum.NOTHING;
                    app.findChangeGrepOptions = NothingEnum.NOTHING;
                    app.findGrepPreferences.findWhat = pattern;
                    return frame.parentStory.findGrep().length;
                } finally {
                    scratch.close(SaveOptions.NO);
                }
            }),
        );
    const notes = yield* Effect.validate(
        leaves(target, values, []),
        Effect.fnUntraced(function* ({ owner, field, name, value }) {
            const untranslatable = (constructs: Array.NonEmptyReadonlyArray<{ readonly construct: string; readonly alternative: Option.Option<string> }>): typeof Refusal.Type => ({ _tag: 'Untranslatable', target: at, field: name, pattern: String(value), constructs });
            const [given, rewritten, probe] = yield* Effect.mapError(Effect.fromResult(translated(owner.constructorName, field, value, Option.liftPredicate(app.findGrepPreferences.findWhat, Predicate.isString))), untranslatable);
            yield* Effect.transposeOption(
                Option.map(
                    probe,
                    flow(
                        sound,
                        Effect.orDie,
                        Effect.filterOrFail(
                            (count) => count > 0,
                            () => untranslatable([{ construct: String(given), alternative: Option.none() }]),
                        ),
                    ),
                ),
            );
            const put = (candidate: unknown): Effect.Effect<object, typeof Refusal.Type> => Effect.try({ try: () => Object.assign(owner, { [field]: candidate }), catch: (cause) => ({ _tag: 'Refused', target: at, field: name, cause }) });
            const accepted = yield* Effect.findFirst(Predicate.isString(given) ? [...Option.toArray(located(given)), given, ...named(given)] : [given], flow(put, Effect.isSuccess));
            yield* Option.isNone(accepted) ? put(given) : Effect.void;
            const assigned = Option.filter(accepted, (each) => each instanceof indesign.Enumerator);
            return Array.map([...rewritten, ...Option.toArray(Option.map(assigned, (each) => `${JSON.stringify(given)} assigned ${String(each)}`))], (text) => `${name}: ${text}`);
        }),
    ).pipe(Effect.mapError((fields) => InDesignError.make({ reason: { _tag: 'Rejected', fields } })));
    return Array.flatten(notes);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, classes, collection, encoded, exported, type Host, held, host, Model, members, owning, plurals, read, record, resolve, stepped, transacted, write };
