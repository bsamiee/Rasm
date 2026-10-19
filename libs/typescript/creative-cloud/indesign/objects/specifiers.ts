import type { InDesignDOMObject } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cache, Effect, flow, Iterable, Option, Record, Schema, Struct } from 'effect';
import { Host } from '../host.ts';
import { Classes } from './classes.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Specifier = Schema.String.annotate({ identifier: 'Specifier', description: "InDesign's path to an object, as results return it" });
const Reference = Schema.Struct({ type: Schema.String, specifier: Specifier, name: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ identifier: 'Reference' });

// --- [CONSTANTS] -----------------------------------------------------------------------

const SELECTOR = /\[(?:"[^"]*"|[^\]"])*\]$/u;
const INDEXED = /\[-?\d+\]$/u;
const NAMED = /\[@name="(?<name>[^"]*)"\]$/u;
const IDENTIFIED = /(?<kind>[\w-]+)\[@id=\d+\]$/u;
const LAST_STEP = /\/+[^/]+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VALUES]
const leaf = (specifier: string): string => specifier.slice(specifier.lastIndexOf('/') + 1);
const read = <S extends Schema.Top>(schema: S, value: () => unknown) =>
    Effect.flatMap(Classes, ({ encoded }) =>
        Effect.flatMap(
            Effect.try(() => encoded(value(), false)),
            flow(Schema.decodeUnknownEffect(Schema.fromJsonString(schema)), Effect.orDie),
        ),
    );

// --- [LOCATIONS]
const active = Effect.flatMap(Host, ({ indesign }) => Effect.try({ try: () => indesign.app.activeDocument, catch: () => Unmatched.make({ subject: 'active document', query: '', candidates: indesign.app.documents.everyItem().name }) }));
const resolve = Effect.fn('resolve')(function* (specifier: string) {
    const [{ indesign }, classes] = yield* Effect.all([Host, Classes]);
    const kind = (path: string): string | undefined => IDENTIFIED.exec(path)?.groups?.kind;
    const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
        const requested = kind(path);
        return Option.filter(Option.liftThrowable((): unknown => indesign.app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
            Array.ensure<unknown>(found).every((each) => classes.host(each) && (requested === undefined || kind(each.toSpecifier()) === requested)),
        );
    };
    return yield* Option.match(located(specifier), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
            const [siblings, [container], indexed, name] = [listed(specifier.replace(SELECTOR, '')), listed(specifier.replace(LAST_STEP, '') || '/'), INDEXED.test(specifier), NAMED.exec(specifier)?.groups?.name];
            const fields =
                container === undefined
                    ? []
                    : yield* Cache.get(classes.rows, container.constructorName).pipe(
                          Effect.map(flow(Struct.get('fields'), Record.toEntries)),
                          Effect.catchTag('Unmatched', () => Effect.succeed([])),
                      );
            const found =
                container === undefined
                    ? Option.none()
                    : Array.findFirst(
                          Iterable.filter(
                              Iterable.map(fields, ([field, row]) => (classes.plural(row) ? Reflect.get(container, field) : undefined)),
                              classes.collection,
                          ),
                          (value) => value.everyItem().toSpecifier() === specifier,
                      );
            const related = Array.flatMap(fields, ([field, { type, array }]) => (container !== undefined && Option.isSome(array) && Option.contains(type, siblings[0]?.constructorName) ? Array.ensure<unknown>(Reflect.get(container, field)).filter(classes.host) : []));
            return yield* Option.match(found, {
                onSome: (value) => Effect.succeed(value.everyItem().getElements()),
                onNone: () =>
                    Unmatched.make({
                        subject: indexed && Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object',
                        query: specifier,
                        candidates: indexed ? [] : Array.dedupe(Array.map([...siblings, ...related.filter((each) => classes.named(each) && each.name === name)], (each) => each.toSpecifier())),
                    }),
            });
        }),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { active, leaf, Reference, read, resolve, Specifier };
