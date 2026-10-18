import indesign, { type BaseCollection, type Enumeration, type Enumerator, type InDesignDOMObject, type MenuAction, type ReadonlyNamedDOMObject } from 'adobe:indesign';
import type { Entry } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Context, Deferred, Effect, flow, Iterable, identity, Layer, Match, Option, Predicate, Record, Ref, Schema, Struct } from 'effect';
import { InDesignError, type Phase } from '../models.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;

// --- [SERVICES] ------------------------------------------------------------------------

class Model extends Context.Service<Model>()('@rasm/indesign/document/objects/Model', {
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
            const [parent, properties, methods, statics] = yield* read(Rows, () => indesign.app.doScript(reader, indesign.ScriptLanguage.JAVASCRIPT, [kind])).pipe(Effect.filterOrFail(Predicate.isNotNull, () => Unmatched.make({ subject: 'class', query: kind, candidates: kinds })));
            const stated = ([name, description, type, array, [minimum, maximum]]: readonly [string, string, string, boolean, readonly number[], unknown]) => {
                const choices: unknown = Reflect.get(indesign, type);
                const listed = choices instanceof indesign.Enumeration ? Array.map(members(choices), String) : [];
                return {
                    name,
                    description: description || undefined,
                    type: type || undefined,
                    array: array || undefined,
                    minimum,
                    maximum,
                    members: Array.isReadonlyArrayNonEmpty(listed) ? listed : undefined,
                    unavailable: Option.getOrUndefined(unavailable(kind, name, Option.none(), false)),
                };
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
class Handoff extends Context.Service<Handoff>()('@rasm/indesign/document/objects/Handoff', {
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
const exported = Array.map(Object.getOwnPropertyNames(indesign), (name) => [name, Reflect.get(indesign, name)] as const);
const kinds = Array.flatMap(exported, ([name, value]) => (Predicate.isFunction(value) || value instanceof indesign.Enumeration ? [name] : []));
const owning = <A>(member: keyof A): (<T>(value: T) => value is T & A) => {
    const owners = new Set(Array.flatMap(exported, ([, type]) => (Predicate.isFunction(type) && Object.hasOwn(type.prototype, member) ? [type.prototype] : [])));
    return <T>(value: T): value is T & A => Predicate.isObject(value) && owners.has(Object.getPrototypeOf(value));
};
const [host, collection] = [owning<InDesignDOMObject<unknown>>('toSpecifier'), owning<BaseCollection<unknown, unknown, InDesignDOMObject<unknown, 'plural'>>>('everyItem')];
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
        ),
    );
const read = <S extends Schema.Top>(schema: S, value: () => unknown): Effect.Effect<S['Type'], InDesignError, S['DecodingServices']> =>
    Effect.flatMap(Effect.try({ try: () => encoded(value(), false), catch: InDesignError.refused(Option.none()) }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie));
const active = Effect.try({ try: () => indesign.app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: indesign.app.documents.everyItem().name }) });

// --- [OBJECTS]
const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
    const kind = (specifier: string): string | undefined => IDENTIFIED.exec(specifier)?.groups?.kind;
    const requested = kind(path);
    return Option.filter(Option.liftThrowable((): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
        Array.ensure<unknown>(found).every((each) => host(each) && (requested === undefined || kind(each.toSpecifier()) === requested)),
    );
};
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    const named = owning<ReadonlyNamedDOMObject<unknown>>('name');
    return yield* Option.match(located(specifier), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
            const [siblings, [container], indexed, name] = [listed(specifier.replace(SELECTOR, '')), listed(specifier.replace(LAST_STEP, '') || '/'), INDEXED.test(specifier), NAMED.exec(specifier)?.groups?.name];
            const fields =
                container === undefined
                    ? []
                    : yield* Cache.get(model, container.constructorName).pipe(
                          Effect.map(flow(Struct.get('fields'), Record.toEntries)),
                          Effect.catchTag('Unmatched', () => Effect.succeed([])),
                      );
            const found =
                container === undefined
                    ? Option.none()
                    : Array.findFirst(
                          Iterable.filter(
                              Iterable.map(fields, ([field, { type }]) => {
                                  const declared: unknown = type === undefined ? undefined : Reflect.get(indesign, type);
                                  return Predicate.isFunction(declared) && Object.hasOwn(declared.prototype, 'everyItem') ? Reflect.get(container, field) : undefined;
                              }),
                              collection,
                          ),
                          (value) => value.everyItem().toSpecifier() === specifier,
                      );
            const related = Array.flatMap(fields, ([field, { type, array }]) => (container !== undefined && array === true && type === siblings[0]?.constructorName ? Array.ensure<unknown>(Reflect.get(container, field)).filter(host) : []));
            return yield* Option.match(found, {
                onSome: (value) => Effect.succeed(value.everyItem().getElements()),
                onNone: () =>
                    Unmatched.make({
                        subject: indexed && Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object',
                        query: specifier,
                        candidates: indexed ? [] : Array.dedupe(Array.map([...siblings, ...related.filter((each) => named(each) && each.name === name)], (each) => each.toSpecifier())),
                    }),
            });
        }),
    });
});

// --- [UNITS]
const transacted = Effect.fn('transacted')(function* <A, E, R>(name: string, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    yield* app.modalState ? Effect.fail(InDesignError.make({ reasons: [{ _tag: 'Modal' }] })) : Effect.void;
    const { handed, entered } = yield* Handoff;
    const [done, run, opened] = [yield* Deferred.make<A, E | InDesignError>(), Effect.runSyncWith(yield* Effect.context<R>()), app.documents.everyItem().id];
    const recorded = Effect.flatMap(Effect.exit(body), (exit) => Effect.andThen(Deferred.done(done, exit), exit));
    const close = Effect.sync(() => {
        Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
            app.documents.itemByID(id).close(SaveOptions.NO);
        });
    });
    const wait = Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: InDesignError.refused(Option.some(name)) });
    const step = Effect.catch(Effect.try({ try: (): unknown => app.doScript(() => run(recorded), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name), catch: InDesignError.refused(Option.some(name)) }), (refused) => Deferred.fail(done, refused));
    return yield* handed(wait, Effect.andThen(entered, Effect.andThen(step, Deferred.await(done)))).pipe(Effect.onError(() => close));
});

// --- [WRITES]
const preserve = <T extends object>(owner: T, values: Partial<T>) => {
    const refused = InDesignError.refused(Option.none());
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(Struct.keys(values), (key) => [key, owner[key]]), catch: refused }), (saved) => Effect.try({ try: () => Object.assign(owner, saved), catch: refused }).pipe(Effect.orDie)).pipe(
        Effect.tap(() => Effect.try({ try: () => Object.assign(owner, values), catch: refused })),
    );
};
const write = Effect.fn('write')(function* (target: Pick<InDesignDOMObject<unknown>, 'constructorName' | 'toSpecifier'>, values: Record.ReadonlyRecord<string, unknown>) {
    const decoded: (value: unknown) => Effect.Effect<unknown, InDesignError | Unmatched, Model> = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ type: Schema.String, specifier: Schema.String })), (reference) => resolve(reference.specifier)),
        Match.when(
            Schema.is(Schema.Struct({ type: Schema.String, member: Schema.String })),
            Effect.fnUntraced(function* ({ type, member }) {
                const refused = InDesignError.refused(Option.some(target.toSpecifier()));
                const enumeration = yield* Effect.try({ try: (): unknown => Reflect.get(indesign, type), catch: refused }).pipe(
                    Effect.filterOrFail(
                        (value): value is Enumeration => value instanceof indesign.Enumeration,
                        () => Unmatched.make({ subject: 'enumeration', query: type, candidates: Array.map(exported.filter(flow(Struct.get(1), Schema.is(Schema.instanceOf(indesign.Enumeration)))), Struct.get(0)) }),
                    ),
                );
                return yield* Effect.try({ try: (): unknown => Reflect.get(enumeration, member), catch: refused }).pipe(
                    Effect.filterOrFail(
                        (value): value is Enumerator => value instanceof indesign.Enumerator,
                        () => Unmatched.make({ subject: `${type} member`, query: member, candidates: Array.map(members(enumeration), String) }),
                    ),
                );
            }),
        ),
        Match.when(Array.isArray, (items) => Effect.forEach(items, decoded)),
        Match.when(record, (properties) => Effect.all(Record.map(properties, decoded))),
        Match.orElse(Effect.succeed),
    );
    const assigned = Effect.fnUntraced(function* (owner: typeof target, properties: typeof values): Effect.fn.Return<void, InDesignError, Model> {
        const specifier = owner.toSpecifier();
        const { fields } = yield* Cache.get(yield* Model, owner.constructorName).pipe(Effect.mapError(InDesignError.refused(Option.some(specifier))));
        yield* Effect.validate(
            Object.entries(properties),
            Effect.fnUntraced(function* ([field, value]) {
                const refused = InDesignError.refused(Option.some(`${specifier} ${field}`));
                const nested = record(value) && Option.contains(Option.map(Record.get(fields, field), Struct.get('settable')), false);
                const blocked = unavailable(owner.constructorName, field, Option.some(owner), !nested);
                yield* Option.isNone(blocked) ? Effect.void : Effect.fail(refused(blocked.value));
                const inner: unknown = nested ? yield* Effect.try({ try: () => Reflect.get(owner, field), catch: refused }) : undefined;
                if (record(value) && host(inner)) {
                    return yield* assigned(inner, value);
                }
                yield* Effect.try({ try: () => Object.assign(owner, { [field]: value }), catch: refused });
            }),
            { discard: true },
        ).pipe(Effect.mapError((errors) => InDesignError.make({ reasons: Array.flatMap(errors, Struct.get('reasons')) })));
    });
    yield* assigned(target, yield* Effect.all(Record.map(values, decoded)));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, collection, encoded, exported, Handoff, host, kinds, Model, members, owning, preserve, read, record, resolve, transacted, unavailable, write };
