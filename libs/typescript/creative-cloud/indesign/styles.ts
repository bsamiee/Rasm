import type { InDesignDOMObject, ParagraphStyle } from 'adobe:indesign';
import { Array, Cache, Cause, Effect, flow, Iterable, Option, Predicate, Record, Result, Schema } from 'effect';
import { Model } from './model.ts';
import { ancestors, encoder, Reference, read } from './references.ts';
import { Composition } from './text.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Styled = InDesignDOMObject<unknown> & { readonly name: string; readonly basedOn: unknown };

// --- [MODELS] --------------------------------------------------------------------------

class StyleView extends Schema.TaggedClass<StyleView, { readonly brand: unique symbol }>()('Style', {
    ...Reference.fields,
    hostOwned: Schema.Boolean.annotate({ description: 'A bracketed name marks a style InDesign owns and refuses to remove' }),
    group: Schema.Array(Schema.String).annotate({ description: 'Enclosing style group names, outermost first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Inheritance chain, nearest first' }),
    nextStyle: Schema.OptionFromOptionalKey(Reference),
    own: Schema.JsonObject.annotate({ description: 'Attributes the style sets differently from its base' }),
    composition: Schema.OptionFromOptionalKey(Composition),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Styled>('basedOn')), Option.map(StyleView.of));
    static readonly of = Effect.fn('of')(function* (style: Styled) {
        const [{ rows, owning, host, named, document, plural }, encoded, enclosing] = yield* Effect.all([Model, encoder, ancestors(style.parent)]);
        const { fields } = yield* Cache.get(rows, style.constructorName);
        const [mine, theirs] = yield* Effect.try({ try: () => [style.properties, host(style.basedOn) ? style.basedOn.properties : {}] as const, catch: (cause) => new Cause.UnknownError(cause, style.toSpecifier()) });
        const attributes = Record.filter(fields, (row, name) => row.settable && !plural(row) && !Object.hasOwn(StyleView.fields, name) && !Object.hasOwn(Composition.fields, name));
        const reference = Reference.of(style);
        return yield* read(StyleView, () => ({
            _tag: 'Style',
            ...reference,
            hostOwned: reference.hostOwned,
            group: Array.flatMap(enclosing, (above) => (named(above) && !document(above) ? [above.name] : [])).toReversed(),
            basedOn: Array.fromIterable(Iterable.unfold(style.basedOn, (each) => (host(each) && Predicate.hasProperty(each, 'basedOn') ? Option.some([each, each.basedOn] as const) : Option.none()))),
            nextStyle: Option.flatMap(Option.liftPredicate(style, owning<ParagraphStyle>('nextStyle')), ({ nextStyle }) => Option.liftPredicate(nextStyle, host)),
            own: Record.filterMap(attributes, (_, name) => (encoded(Reflect.get(mine, name)) === encoded(Reflect.get(theirs, name)) ? Result.failVoid : Result.succeed<unknown>(Reflect.get(mine, name)))),
            composition: Option.map(Option.liftPredicate(style, owning<ParagraphStyle>('nestedGrepStyles')), Composition.of),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { StyleView };
