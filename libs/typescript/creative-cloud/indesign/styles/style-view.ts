import type { InDesignDOMObject, ParagraphStyle } from 'adobe:indesign';
import { Array, Cause, Effect, flow, Iterable, Option, Predicate, Schema } from 'effect';
import { byName } from '../dom/decoding.ts';
import { read } from '../dom/encoding.ts';
import { ancestors } from '../dom/location.ts';
import { Model } from '../dom/model.ts';
import { Reference } from '../dom/reference.ts';
import { overrides } from './overrides.ts';
import { ParagraphAttributes } from './paragraph-attributes.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Styled = InDesignDOMObject<unknown> & { readonly name: string; readonly basedOn: unknown; readonly nextStyle?: unknown };

// --- [MODELS] --------------------------------------------------------------------------

class StyleView extends Schema.TaggedClass<StyleView, { readonly brand: unique symbol }>()('Style', {
    ...Reference.fields,
    group: Schema.Array(Schema.String).annotate({ description: 'Enclosing style group names, nearest first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Inheritance chain, nearest first, ending before the root style' }),
    nextStyle: Schema.OptionFromOptionalKey(Reference),
    own: Schema.JsonObject.annotate({ description: 'Attributes the style sets differently from its base, the root style for a style based on it' }),
    paragraphAttributes: Schema.OptionFromOptionalKey(ParagraphAttributes),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Styled>('basedOn')), Option.map(StyleView.of));
    static readonly of = Effect.fn('StyleView.of')(function* (style: Styled) {
        const [{ app, owning, host, named, document }, enclosing] = yield* Effect.all([Model, ancestors(style.parent)]);
        const refused = (cause: unknown) => new Cause.UnknownError(cause, style.toSpecifier());
        const mine = yield* Effect.try({ try: () => style.properties, catch: refused });
        const based = mine.basedOn;
        const container = Option.orElse(Array.findFirst(enclosing, document), () => Option.some(app));
        const base = Predicate.isString(based) ? yield* byName(style.constructorName, based, container) : based;
        const theirs = yield* Effect.try({ try: (): object => (host(base) ? base.properties : {}), catch: refused });
        const own = yield* overrides(style, mine, (name) => Reflect.get(theirs, name), { ...StyleView.fields, ...ParagraphAttributes.fields });
        return yield* read(StyleView, style.toSpecifier(), () => ({
            _tag: 'Style',
            ...Reference.of(style),
            group: Array.flatMap(enclosing, (above) => (named(above) && !document(above) ? [above.name] : [])),
            basedOn: Array.fromIterable(Iterable.unfold(based, (each) => (host(each) && Predicate.hasProperty(each, 'basedOn') ? Option.some([each, each.basedOn] as const) : Option.none()))),
            nextStyle: Option.liftPredicate(mine.nextStyle, host),
            own,
            paragraphAttributes: Option.map(Option.liftPredicate(style, owning<ParagraphStyle>('nestedGrepStyles')), ParagraphAttributes.of),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { StyleView };
