import indesign, { type BaseCollection, type Document, type InDesignDOMObject, type ReadonlyNamedDOMObject } from 'adobe:indesign';
import type { Entry } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Effect, flow, Iterable, identity, Match, Option, Predicate, Record, Schema, Struct } from 'effect';
import { InDesignError } from '../models.ts';
import { Classes, exported, plural } from './classes.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [KINDS]
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
const record = (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype;

// --- [VALUES]
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
const reference = (object: InDesignDOMObject<unknown>) => ({ type: object.constructorName, specifier: object, name: named(object) ? object.name : undefined });
const projected: (value: unknown) => unknown = Match.type<unknown>().pipe(
    Match.when(named, reference),
    Match.when(host, (object) => object.properties),
    Match.orElse(identity),
);
const read = <S extends Schema.Top>(schema: S, value: () => unknown): Effect.Effect<S['Type'], InDesignError, S['DecodingServices']> =>
    Effect.flatMap(Effect.try({ try: () => encoded(value(), false), catch: InDesignError.refused(Option.none()) }), flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie));

// --- [LOCATIONS]
const active = Effect.try({ try: () => indesign.app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: indesign.app.documents.everyItem().name }) });
const ancestors = (object: unknown): Iterable<InDesignDOMObject<unknown>> => Iterable.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()));
const owner = (object: unknown): Option.Option<Document> => Iterable.findFirst(ancestors(object), document);
const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
    const kind = (specifier: string): string | undefined => IDENTIFIED.exec(specifier)?.groups?.kind;
    const requested = kind(path);
    return Option.filter(Option.liftThrowable((): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
        Array.ensure<unknown>(found).every((each) => host(each) && (requested === undefined || kind(each.toSpecifier()) === requested)),
    );
};
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const classes = yield* Classes;
    return yield* Option.match(located(specifier), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
            const [siblings, [container], indexed, name] = [listed(specifier.replace(SELECTOR, '')), listed(specifier.replace(LAST_STEP, '') || '/'), INDEXED.test(specifier), NAMED.exec(specifier)?.groups?.name];
            const fields =
                container === undefined
                    ? []
                    : yield* Cache.get(classes, container.constructorName).pipe(
                          Effect.map(flow(Struct.get('fields'), Record.toEntries)),
                          Effect.catchTag('Unmatched', () => Effect.succeed([])),
                      );
            const found =
                container === undefined
                    ? Option.none()
                    : Array.findFirst(
                          Iterable.filter(
                              Iterable.map(fields, ([field, row]) => (plural(row) ? Reflect.get(container, field) : undefined)),
                              collection,
                          ),
                          (value) => value.everyItem().toSpecifier() === specifier,
                      );
            const related = Array.flatMap(fields, ([field, { type, array }]) => (container !== undefined && Option.isSome(array) && Option.contains(type, siblings[0]?.constructorName) ? Array.ensure<unknown>(Reflect.get(container, field)).filter(host) : []));
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, ancestors, collection, document, encoded, host, named, owner, owning, projected, read, record, reference, resolve };
