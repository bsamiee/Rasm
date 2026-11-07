import { Reference } from '@rasm/automation/requests';
import { Array, Effect, Option, Schema, SchemaGetter, SchemaTransformation, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.String.check(Schema.isPattern(/(?:^|[^/])$/u));
const Separator = Schema.Literals(['//', '/']);
const Kind = Schema.String.check(Schema.isPattern(/^@?[a-z][\w-]*$/u));
const Property = Schema.TemplateLiteral(['@', Schema.String]);
const Step = Schema.Struct({ container: Container, separator: Separator, kind: Schema.Union([Kind, Schema.Literal('')]), selector: Schema.OptionFromOptionalKey(Reference.Selector) });
const Written = Schema.Union([Schema.TemplateLiteralParser([Container, Separator, Kind, Schema.toEncoded(Reference.Selector)]), Schema.TemplateLiteralParser([Container, Separator, Kind])]).pipe(
    Schema.decodeTo(
        Schema.toEncoded(Step),
        SchemaTransformation.transform({
            decode: ([container, separator, kind, selector]) => ({ container, separator, kind, ...(selector === undefined ? {} : { selector }) }),
            encode: ({ container, separator, kind, selector }) => (selector === undefined ? ([container, separator, kind] as const) : ([container, separator, kind, selector] as const)),
        }),
    ),
);
const Root = Schema.Literal('/').pipe(Schema.decodeTo(Schema.toEncoded(Step), { decode: SchemaGetter.succeed({ container: '', separator: '/', kind: '' }), encode: SchemaGetter.succeed('/') }));
class Specifier extends Schema.Class<Specifier, { readonly brand: unique symbol }>('Specifier')(Step.mapFields(Struct.assign({ end: Schema.OptionFromOptionalKey(Step) }))) {
    static readonly Path = Schema.Union([Schema.TemplateLiteralParser(['(', Written, ' to ', Written, ')']), Written, Root]).pipe(
        Schema.decodeTo(
            Specifier,
            SchemaTransformation.transform({
                decode: (written) => (Array.isArray<typeof written>(written) ? { ...written[1], end: written[3] } : written),
                encode: ({ end, ...start }: typeof Specifier.Encoded) => (end === undefined ? start : (['(', start, ' to ', end, ')'] as const)),
            }),
        ),
    );

    get root(): boolean {
        return this.kind === '';
    }
    get property(): boolean {
        return Schema.is(Property)(this.kind);
    }
    get path(): Effect.Effect<string> {
        return Effect.orDie(Schema.encodeEffect(Specifier.Path)(this));
    }
    get leaf(): string {
        return this.root ? Root.from.literal : Array.map([this, ...Option.toArray(this.end)], ({ kind, selector }) => `${kind}${Option.getOrElse(Option.flatMap(selector, Schema.encodeOption(Reference.Selector)), () => '')}`).join(' to ');
    }
    get name(): Option.Option<string> {
        return Option.map(Option.filter(this.selector, Reference.Selector.guards.Named), Struct.get('name'));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Specifier };
