import indesign, { type Enumeration, type Enumerator } from 'adobe:indesign';
import { Unmatched } from '@rasm/mcp/server';
import { Array, Cache, Context, Deferred, Effect, flow, identity, Layer, Match, Option, Predicate, Record, Ref, Schema, Struct } from 'effect';
import { InDesignError, type Phase } from './contract.ts';
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
    readonly add: () => Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/dom/Model', {
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
            const [parent, properties, methods, statics] = yield* read(Rows, () => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind])).pipe(
                Effect.flatMap(Effect.fromNullishOr),
                Effect.mapError(() => Unmatched.make({ subject: 'class', query: kind, candidates: kinds })),
            );
            const stated = ([name, description, type, array, [minimum, maximum]]: readonly [string, string, string, boolean, readonly number[], unknown]) => {
                const choices: unknown = Reflect.get(indesign, type);
                const listed = choices instanceof indesign.Enumeration ? Array.map(members(choices), String) : [];
                return { name, description: description || undefined, type: type || undefined, array: array || undefined, minimum, maximum, members: Array.isReadonlyArrayNonEmpty(listed) ? listed : undefined };
            };
            return {
                parent,
                fields: Record.fromIterableWith(properties, (row) => [row[0], { ...stated(row), settable: !row[5] }]),
                methods: Record.fromIterableWith(methods, (row) => {
                    const { name, description, type, array } = stated(row);
                    return [name, { name, description, parameters: Array.map(row[5], (each) => ({ ...stated(each), optional: each[5] || each[1].endsWith('(Optional)') })), returns: type === undefined ? undefined : { type, array } }];
                }),
                statics: Record.fromIterableWith(statics, ([name, description]) => [name, description]),
            };
        }),
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}
class Handoff extends Context.Service<Handoff>()('@rasm/indesign/dom/Handoff', {
    make: Effect.map(Ref.make<typeof Phase.Type | Effect.Effect<void>>('Idle'), (state) => ({
        enter: Effect.uninterruptible(Effect.flatten(Ref.modify(state, (now) => (Effect.isEffect(now) ? [Effect.as(now, 'Entered' as const), 'Held' as const] : [Effect.succeed(now), now])))),
        entered: Ref.set(state, 'Entered'),
        handed: Effect.fnUntraced(
            function* <A, E, R, W>(wait: Effect.Effect<unknown, W>, step: Effect.Effect<A, E, R>) {
                const [done, services] = yield* Effect.all([Deferred.make<A, E>(), Effect.context<R>()]);
                yield* Ref.set(state, Effect.asVoid(Deferred.into(Effect.provideContext(step, services), done)));
                yield* wait.pipe(Effect.repeat({ until: () => Deferred.isDone(done) }));
                return yield* Deferred.await(done);
            },
            Effect.ensuring(Ref.set(state, 'Idle')),
        ),
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VALUES]
const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
const kinds = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) || value instanceof indesign.Enumeration ? [name] : []));
const enumerations = Array.flatMap(exported, ([, value]) => (value instanceof indesign.Enumeration ? [value] : []));
const owning = <A>(member: string): (<T>(value: T) => value is T & A) => {
    const owners = new Set(Array.flatMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [])));
    return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
};
const [host, collection] = [owning<Host>('toSpecifier'), owning<Collection>('everyItem')];
const plural = (type: string): boolean => {
    const owner: unknown = Reflect.get(indesign, type);
    return Predicate.isFunction(owner) && Object.hasOwn(owner.prototype, 'everyItem');
};
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;
const members = (enumeration: Enumeration): readonly Enumerator[] =>
    Array.flatMap(Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)), (key) => {
        const member: unknown = Reflect.get(enumeration, key);
        return member instanceof indesign.Enumerator && String(member) === key ? [member] : [];
    });
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
const read = <S extends Schema.Top>(schema: S, value: () => unknown): Effect.Effect<S['Type'], InDesignError, S['DecodingServices']> =>
    Effect.flatMap(Effect.try({ try: () => encoded(value(), false), catch: InDesignError.refused(Option.none()) }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie));
const active = Effect.try({ try: () => indesign.app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: indesign.app.documents.everyItem().name }) });

// --- [OBJECTS]
const located = (path: string): Option.Option<Host | readonly Host[]> => {
    const kind = (specifier: string): string | undefined => IDENTIFIED.exec(specifier)?.groups?.kind;
    return Option.filter(Option.liftThrowable((): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is Host | readonly Host[] =>
        Array.ensure<unknown>(found).every((each) => host(each) && (kind(path) === undefined || kind(each.toSpecifier()) === kind(path))),
    );
};
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    return yield* Option.match(located(specifier), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const listed = (path: string): readonly Host[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
            const [siblings, [container], indexed] = [listed(specifier.replace(SELECTOR, '')), listed(specifier.replace(LAST_STEP, '') || '/'), INDEXED.test(specifier)];
            const fields = container === undefined ? [] : Record.toEntries(Option.match(yield* Effect.option(Cache.get(model, container.constructorName)), { onNone: () => ({}), onSome: Struct.get('fields') }));
            const related = Array.flatMap(fields, ([field, { type, array }]) => (container !== undefined && array === true && type === siblings[0]?.constructorName ? Array.ensure<unknown>(container[field]).filter(host) : []));
            return yield* Unmatched.make({
                subject: indexed && Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object',
                query: specifier,
                candidates: indexed ? [] : Array.dedupe(Array.map([...siblings, ...related.filter((each) => each.name === NAMED.exec(specifier)?.groups?.name)], (each) => each.toSpecifier())),
            });
        }),
    });
});

// --- [UNITS]
const stepped = Effect.fnUntraced(function* <A, E, R>(name: string, keep: boolean, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    const [done, run, opened] = [yield* Deferred.make<A, E | InDesignError>(), Effect.runSyncWith(yield* Effect.context<R>()), app.documents.everyItem().id];
    const recorded = Effect.flatMap(Effect.exit(body), (exit) => Effect.andThen(Deferred.done(done, exit), keep ? exit : Effect.interrupt));
    const close = Effect.sync(() => {
        Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
            app.documents.itemByID(id).close(SaveOptions.NO);
        });
    });
    yield* Effect.catch(Effect.try({ try: (): unknown => app.doScript(() => run(recorded), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name), catch: InDesignError.refused(Option.some(name)) }), (refused) => Deferred.fail(done, refused));
    return yield* Deferred.await(done).pipe(keep ? Effect.onError(() => close) : Effect.ensuring(close));
});
const transacted = Effect.fn('transacted')(function* <A, E, R>(name: string, keep: boolean, body: Effect.Effect<A, E, R>) {
    const { app, ScriptLanguage } = indesign;
    yield* app.modalState ? Effect.fail(InDesignError.make({ reasons: [{ _tag: 'Modal' }] })) : Effect.void;
    const { handed, entered } = yield* Handoff;
    const wait = Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: InDesignError.refused(Option.some(name)) });
    return yield* handed(wait, stepped(name, keep, Effect.andThen(entered, body)));
});

// --- [WRITES]
const write = Effect.fn('write')(function* (target: Pick<Host, 'constructorName' | 'toSpecifier'>, values: Record.ReadonlyRecord<string, unknown>) {
    const { app } = indesign;
    const leaves = (owner: Pick<Host, 'constructorName'>, fields: Record.ReadonlyRecord<string, unknown>, path: readonly string[]): readonly (readonly [owner: Pick<Host, 'constructorName'>, field: string, path: string, value: unknown])[] =>
        Array.flatMap(Object.entries(fields), ([field, value]) => {
            const inner: unknown = Reflect.get(owner, field);
            return record(value) && host(inner) ? leaves(inner, value, [...path, field]) : [[owner, field, [...path, field].join('.'), value] as const];
        });
    const sound = <F>(pattern: string, text: string, unread: F) =>
        stepped(
            'Probe',
            false,
            Effect.sync(() => {
                const scratch = app.documents.add(false);
                const [marked, styled, frame] = [scratch.characterStyles.add(), scratch.paragraphStyles.add(), scratch.textFrames.add()];
                const nested = styled.nestedGrepStyles.add();
                marked.underline = true;
                nested.appliedCharacterStyle = marked;
                nested.grepExpression = pattern;
                frame.contents = text;
                frame.parentStory.appliedParagraphStyle = styled;
                return frame.parentStory.characters.item(0).underline;
            }),
        ).pipe(
            Effect.orDie,
            Effect.filterOrFail(identity, () => unread),
        );
    const notes = yield* Effect.validate(
        leaves(target, values, []),
        Effect.fnUntraced(function* ([owner, field, path, value]) {
            const at = `${target.toSpecifier()} ${path}`;
            const untranslatable = (constructs: Array.NonEmptyReadonlyArray<{ readonly construct: string; readonly alternative: Option.Option<string> }>) => ({ _tag: 'Untranslatable', at, constructs }) as const;
            const [given, rewritten, probe] = yield* Effect.mapError(Effect.fromResult(translated(owner.constructorName, field, value, Option.liftPredicate(app.findGrepPreferences.findWhat, Predicate.isString))), untranslatable);
            yield* Effect.transposeOption(Option.map(probe, ([pattern, text]) => sound(pattern, text, untranslatable([{ construct: String(given), alternative: Option.none() }]))));
            const put = (candidate: unknown) => Effect.try({ try: () => Object.assign(owner, { [field]: candidate }), catch: (cause) => ({ _tag: 'Refused', at: Option.some(at), cause }) as const }).pipe(Effect.as(candidate));
            const [specified, named] = Predicate.isString(given)
                ? [
                      Option.toArray(located(given)),
                      enumerations.flatMap((enumeration) => {
                          const member: unknown = Reflect.get(enumeration, given);
                          return member instanceof indesign.Enumerator ? [member] : [];
                      }),
                  ]
                : [[], []];
            const assigned = yield* Effect.firstSuccessOf(Array.map([...specified, given], put)).pipe(Effect.catch((refused) => Effect.firstSuccessOf([...Array.map(named, put), Effect.fail(refused)])));
            return Array.map([...rewritten, ...(assigned instanceof indesign.Enumerator && Predicate.isString(given) ? [`'${given}' assigned ${String(assigned)}`] : [])], (note) => `${path}: ${note}`);
        }),
    ).pipe(Effect.mapError((reasons) => InDesignError.make({ reasons })));
    return Array.flatten(notes);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, collection, encoded, exported, Handoff, type Host, host, kinds, Model, members, owning, plural, read, record, resolve, transacted, write };
