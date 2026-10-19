import type { InDesignDOMObject, Link, ParagraphStyle, Swatch } from 'adobe:indesign';
import { Array, Cache, Cause, Effect, Iterable, Option, Predicate, Record, Result, Schema } from 'effect';
import { hostOwned } from './documents.ts';
import { Classes } from './objects/classes.ts';
import { Reference, read } from './objects/specifiers.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Styled = InDesignDOMObject<unknown> & { readonly name: string; readonly basedOn: unknown };
type Definition = Styled | Swatch | Link;

// --- [MODELS] --------------------------------------------------------------------------

const Rule = Schema.Struct({ style: Schema.OptionFromOptionalKey(Reference) });
class DefinitionView extends Schema.TaggedClass<DefinitionView, { readonly brand: unique symbol }>()('Definition', {
    ...Reference.fields,
    hostOwned: Schema.Boolean.annotate({ description: 'A bracketed name marks a definition InDesign owns and refuses to remove' }),
    group: Schema.Array(Schema.String).annotate({ description: 'Enclosing style or color group names, outermost first' }),
    basedOn: Schema.Array(Reference).annotate({ description: "A style's inheritance chain nearest first, or a tint's base color" }),
    own: Schema.JsonObject.annotate({ description: 'What the definition states itself: attributes a style sets differently from its base, a color or tint value, a link source and status' }),
    rules: Schema.OptionFromOptionalKey(
        Schema.Struct({
            nextStyle: Schema.OptionFromOptionalKey(Reference),
            grep: Schema.Array(Rule.pipe(Schema.fieldsAssign({ expression: Schema.String }))).annotate({ description: 'Automatic GREP styles: character style applied wherever the expression matches, in order' }),
            nested: Schema.Array(Rule.pipe(Schema.fieldsAssign({ delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean }))),
            lines: Schema.Array(Rule.pipe(Schema.fieldsAssign({ lineCount: Schema.Int, repeatLast: Schema.Int }))),
            tabs: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String })),
            list: Schema.Struct({ type: Schema.String, level: Schema.Int, bullet: Schema.Int.annotate({ description: 'Bullet character code point' }), numberingFormat: Schema.Json }),
        }),
    ).annotate({ description: 'Composition rules a paragraph style owns' }),
}) {
    static readonly guard = ({ owning }: Classes['Service']) => {
        const guards = [owning<Styled>('basedOn'), owning<Swatch>('parentColorGroup'), owning<Link>('linkResourceURI')];
        return (value: unknown): value is Definition => guards.some((guard) => guard(value));
    };
    static readonly of = Effect.fn('of')(function* (found: Definition) {
        const { rows, owning, host, named, document, plural, encoded, projected, reference, ancestors } = yield* Classes;
        const { fields } = yield* Cache.get(rows, found.constructorName);
        const styled = Option.liftPredicate(found, owning<Styled>('basedOn'));
        const [mine, theirs] = yield* Effect.try({
            try: () => Option.match(styled, { onNone: () => [{}, {}] as const, onSome: (style) => [style.properties, host(style.basedOn) ? style.basedOn.properties : {}] as const }),
            catch: (cause) => new Cause.UnknownError(cause, found.toSpecifier()),
        });
        const attributes = Record.filter(fields, (row, name) => row.settable && !['basedOn', 'nextStyle', 'name', 'label'].includes(name) && !plural(row));
        const own = Option.isSome(styled)
            ? Record.filterMap(attributes, (_, name) => (encoded(Reflect.get(mine, name), false) === encoded(Reflect.get(theirs, name), false) ? Result.failVoid : Result.succeed<unknown>(Reflect.get(mine, name))))
            : Record.fromIterableWith('linkResourceURI' in found ? ['status', 'filePath', 'linkType', 'needed', 'edited', 'size', 'date'] : ['space', 'model', 'colorValue', 'tintValue', 'gradientType'], (name) => [name, projected(Reflect.get(found, name))]);
        const applied = (rule: { readonly appliedCharacterStyle: unknown }) => (host(rule.appliedCharacterStyle) ? reference(rule.appliedCharacterStyle) : undefined);
        const rules = (style: ParagraphStyle) => ({
            nextStyle: host(style.nextStyle) ? reference(style.nextStyle) : undefined,
            grep: style.nestedGrepStyles
                .everyItem()
                .getElements()
                .map((rule) => ({ expression: rule.grepExpression, style: applied(rule) })),
            nested: style.nestedStyles
                .everyItem()
                .getElements()
                .map((rule) => ({ style: applied(rule), delimiter: rule.delimiter, repetition: rule.repetition, inclusive: rule.inclusive })),
            lines: style.nestedLineStyles
                .everyItem()
                .getElements()
                .map((rule) => ({ style: applied(rule), lineCount: rule.lineCount, repeatLast: rule.repeatLast })),
            tabs: style.tabStops
                .everyItem()
                .getElements()
                .map((stop) => ({ position: stop.position, alignment: stop.alignment, leader: stop.leader, alignmentCharacter: stop.alignmentCharacter })),
            list: { type: style.bulletsAndNumberingListType, level: style.numberingLevel, bullet: style.bulletChar.characterValue, numberingFormat: style.numberingFormat },
        });
        return yield* read(DefinitionView, () => ({
            _tag: 'Definition',
            ...reference(found),
            hostOwned: hostOwned(found.name),
            group: [...Array.flatMap(Array.fromIterable(ancestors(found.parent)), (above) => (named(above) && !document(above) ? [above.name] : [])).toReversed(), ...('parentColorGroup' in found && host(found.parentColorGroup) && named(found.parentColorGroup) ? [found.parentColorGroup.name] : [])],
            basedOn: Array.map(
                Option.match(styled, {
                    onNone: () => ['baseColor' in found ? found.baseColor : undefined].filter(host),
                    onSome: (style) => Array.fromIterable(Iterable.unfold(style.basedOn, (each) => (host(each) && Predicate.hasProperty(each, 'basedOn') ? Option.some([each, each.basedOn] as const) : Option.none()))),
                }),
                reference,
            ),
            own,
            rules: Option.getOrUndefined(Option.map(Option.liftPredicate(found, owning<ParagraphStyle>('nestedGrepStyles')), rules)),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { DefinitionView };
