import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Cache, Effect, Equal, Iterable, Option, Record, Struct } from 'effect';
import { Model } from './model.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;
const COLLECTION = /\/[\w-]+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const ancestors = (object: unknown) => Model.useSync(({ host }) => Array.fromIterable(Iterable.unfold(object, (above: unknown) => (host(above) && above.constructorName !== 'Application' ? Option.some([above, above.parent] as const) : Option.none()))));
const owner = (object: unknown) => Model.use(({ document }) => Effect.map(ancestors(object), Array.findFirst(document)));
const active = Model.use(({ app }) => Effect.try({ try: () => app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: Option.none(), candidates: app.documents.everyItem().name }) }));
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const model = yield* Model;
    const kind = (path: string): string | undefined => IDENTIFIED.exec(path)?.groups?.kind;
    const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
        const requested = kind(path);
        return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
            Array.ensure<unknown>(found).every((each) => model.host(each) && each.isValid && (requested === undefined || kind(each.toSpecifier()) === requested)),
        );
    };
    const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
    return yield* Option.match(located(specifier), {
        onSome: (found) => Effect.succeed(COLLECTION.test(specifier) ? Array.ensure(found) : found),
        onNone: Effect.fnUntraced(function* () {
            const siblings = listed(specifier.replace(SELECTOR, ''));
            const container = Array.head(listed(specifier.replace(LAST_STEP, '') || '/'));
            const fields = yield* Effect.map(Effect.transposeOption(Option.map(container, (at) => Cache.get(model.rows, at.constructorName))), Option.match({ onNone: Array.empty, onSome: ({ fields: rows }) => Record.toEntries(rows) }));
            const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at) => Reflect.get(at, field)));
            const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
            const related = fields.flatMap(([field, { type, array }]) => (Option.isSome(array) && Option.isSome(sibling) && Equal.equals(type, sibling) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
            const collections = fields
                .flatMap(([field, row]) => (model.plural(row) ? [value(field)] : []))
                .filter(model.collection)
                .flatMap((items) => Option.toArray(Option.fromUndefinedOr(items.everyItem())));
            const name = NAMED.exec(specifier)?.groups?.name;
            const candidates = Array.dedupe(Array.map([...siblings, ...related.filter((each) => each.name === name)], (each) => each.toSpecifier()));
            return yield* Effect.fromOption(
                Option.map(
                    Array.findFirst(collections, (every) => every.toSpecifier() === specifier),
                    (every) => every.getElements(),
                ),
                () => (INDEXED.test(specifier) ? Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: Option.some(specifier), candidates: [] }) : Unmatched.make({ subject: 'object', query: Option.some(specifier), candidates })),
            );
        }),
    });
});
const resolveObject = (specifier: string) => Model.use(({ host }) => Effect.filterOrFail(resolve(specifier), host, () => Unmatched.make({ subject: 'object', query: Option.some(specifier), candidates: [] })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, ancestors, owner, resolve, resolveObject };
