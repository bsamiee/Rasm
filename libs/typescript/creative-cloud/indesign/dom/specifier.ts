import type { InDesignDOMObject } from 'adobe:indesign';
import { Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cache, Effect, Equal, Option, Predicate, Record, Schema, SchemaGetter, SchemaTransformation, Struct } from 'effect';
import { Model } from '#/dom/model';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.String.check(Schema.isPattern(/(?:^|[^/])$/u));
const Separator = Schema.Literals(['//', '/']);
const { Kind, Identified, Indexed, Named } = Reference;
const Property = Schema.TemplateLiteral(['@', Schema.String]);
const Name = Schema.TemplateLiteralParser(Named.parts);
const Selector = Schema.Union([Identified, Indexed, Named]);
const Step = Schema.Struct({ container: Container, separator: Separator, kind: Schema.Union([Kind, Schema.Literal('')]), selector: Schema.OptionFromOptionalKey(Selector) });
const Written = Schema.Union([Schema.TemplateLiteralParser([Container, Separator, Kind, Selector]), Schema.TemplateLiteralParser([Container, Separator, Kind])]).pipe(
    Schema.decodeTo(
        Schema.toEncoded(Step),
        SchemaTransformation.transform({
            decode: ([container, separator, kind, selector]) => ({ container, separator, kind, ...(selector === undefined ? {} : { selector }) }),
            encode: ({ container, separator, kind, selector }) => (selector === undefined ? ([container, separator, kind] as const) : ([container, separator, kind, selector] as const)),
        }),
    ),
);
const Root = Schema.Literal('/').pipe(Schema.decodeTo(Schema.toEncoded(Step), { decode: SchemaGetter.succeed({ container: '', separator: '/', kind: '' }), encode: SchemaGetter.succeed('/') }));
class Specifier extends Schema.Class<Specifier, { readonly brand: unique symbol }>('Specifier')({ ...Step.fields, end: Schema.OptionFromOptionalKey(Step) }) {
    static readonly Path = Schema.Union([Schema.TemplateLiteralParser(['(', Written, ' to ', Written, ')']), Written, Root]).pipe(
        Schema.decodeTo(
            Specifier,
            SchemaTransformation.transform({
                decode: (written) => (Array.isArray<typeof written>(written) ? { ...written[1], end: written[3] } : written),
                encode: ({ end, ...start }: typeof Specifier.Encoded) => (end === undefined ? start : (['(', start, ' to ', end, ')'] as const)),
            }),
        ),
    );
    static readonly resolve = Effect.fn('Specifier.resolve')(function* (specifier: Specifier) {
        const model = yield* Model;
        const identified = (path: string): Option.Option<string> =>
            Option.flatMap(Schema.decodeUnknownOption(Specifier.Path)(path), ({ kind, selector, end }) =>
                Option.as(
                    Option.filter(selector, (each) => Option.isNone(end) && Schema.is(Identified)(each)),
                    kind,
                ),
            );
        const located = (path: string): Option.Option<InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[]> => {
            const requested = identified(path);
            return Option.filter(Option.liftThrowable((): unknown => model.app.doScript('resolve(arguments[0])', model.ScriptLanguage.JAVASCRIPT, [path]))(), (found): found is InDesignDOMObject<unknown> | readonly InDesignDOMObject<unknown>[] =>
                Array.ensure<unknown>(found).every((each) => model.host(each) && each.isValid && (Option.isNone(requested) || Equal.equals(identified(each.toSpecifier()), requested))),
            );
        };
        const listed = (path: string): readonly InDesignDOMObject<unknown>[] => Array.flatMap(Option.toArray(located(path)), Array.ensure);
        const nearest = Effect.fnUntraced(function* () {
            const siblings = listed(`${specifier.container}${specifier.separator}${specifier.kind}`);
            const container = Array.head(listed(specifier.container || '/'));
            const fields = yield* Effect.map(Effect.transposeOption(Option.map(container, (at) => Cache.get(model.rows, at.constructorName))), Option.match({ onNone: Array.empty, onSome: ({ fields: rows }) => Record.toEntries(rows) }));
            const value = (field: string): unknown => Option.getOrUndefined(Option.map(container, (at) => Reflect.get(at, field)));
            const sibling = Option.map(Array.head(siblings), Struct.get('constructorName'));
            const related = fields.flatMap(([field, { type, array }]) => (Option.isSome(array) && Option.isSome(sibling) && Equal.equals(type, sibling) ? Array.ensure<unknown>(value(field)) : [])).filter(model.named);
            const collections = fields
                .flatMap(([field, row]) => (model.plural(row) ? [value(field)] : []))
                .filter(model.collection)
                .flatMap((items) => Option.toArray(Option.fromUndefinedOr(items.everyItem())));
            const candidates = Array.dedupe(Array.map([...siblings, ...related.filter((each) => Option.contains(specifier.name, each.name))], (each) => each.toSpecifier()));
            return yield* Effect.fromOption(
                Option.map(
                    Array.findFirst(collections, (every) => every.toSpecifier() === specifier.path),
                    (every) => every.getElements(),
                ),
                () =>
                    Option.exists(specifier.selector, Schema.is(Indexed))
                        ? Unmatched.make({ subject: Array.isReadonlyArrayNonEmpty(siblings) ? `object among indexes 0 to ${siblings.length - 1}` : 'object', query: Option.some(specifier.path), candidates: [] })
                        : Unmatched.make({ subject: 'object', query: Option.some(specifier.path), candidates }),
            );
        });
        return yield* Option.match(located(specifier.path), {
            onSome: (found) => Effect.succeed(specifier.root || Option.isSome(specifier.selector) || Schema.is(Property)(specifier.kind) ? found : Array.ensure(found)),
            onNone: () => (Option.isSome(specifier.end) ? Effect.fail(Unmatched.make({ subject: 'range', query: Option.some(specifier.path), candidates: [] })) : nearest()),
        });
    });
    static readonly object = (specifier: Specifier) => Model.use(({ host }) => Effect.filterOrFail(Specifier.resolve(specifier), host, () => Unmatched.make({ subject: 'object', query: Option.some(specifier.path), candidates: [] })));

    get root(): boolean {
        return this.kind === '';
    }
    get path(): string {
        const steps = Array.map([this, ...Option.toArray(this.end)], ({ container, separator, kind, selector }) => `${container}${separator}${kind}${Option.getOrElse(selector, () => '')}`);
        return Option.match(this.end, { onNone: () => steps.join(''), onSome: () => `(${steps.join(' to ')})` });
    }
    get leaf(): string {
        return this.root ? this.path : Array.map([this, ...Option.toArray(this.end)], ({ kind, selector }) => `${kind}${Option.getOrElse(selector, () => '')}`).join(' to ');
    }
    get name(): Option.Option<string> {
        return Option.map(Option.flatMap(this.selector, Schema.decodeUnknownOption(Name)), ([, name]) => name);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const reference = (object: InDesignDOMObject<unknown>): Reference => {
    const valid = object.isValid;
    return Reference.make({ type: object.constructorName, specifier: object.toSpecifier(), name: valid && 'name' in object ? Option.liftPredicate(object.name, Predicate.isString) : Option.none(), valid });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { reference, Specifier };
