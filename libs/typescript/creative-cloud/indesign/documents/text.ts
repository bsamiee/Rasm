import type { InDesignDOMObject, ParagraphStyle, Story, Text, TextFrame } from 'adobe:indesign';
import { HostError, Reference } from '@rasm/automation/requests';
import { Array, Cache, Effect, Number, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';
import { reference } from '#/dom/specifier';
import { byName, encoder, picked, read, type Slice } from '#/dom/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Styled = InDesignDOMObject<unknown> & { readonly name: string; readonly basedOn: unknown; readonly nextStyle?: unknown };

// --- [MODELS] --------------------------------------------------------------------------

class ParagraphAttributes extends Schema.Class<ParagraphAttributes, { readonly brand: unique symbol }>('ParagraphAttributes')({
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'GREP styles in order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
    tabList: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String })),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Int.annotate({ description: 'Bullet character code point' }),
    numberingFormat: Schema.Json,
}) {
    static readonly of = (owner: Pick<ParagraphStyle, keyof typeof ParagraphAttributes.fields>) => {
        const { nestedGrepStyles, nestedStyles, nestedLineStyles, tabList } = ParagraphAttributes.fields;
        return {
            ...picked(ParagraphAttributes.fields)(owner),
            nestedGrepStyles: Array.map(owner.nestedGrepStyles.everyItem().getElements(), picked(nestedGrepStyles.value.fields)),
            nestedStyles: Array.map(owner.nestedStyles.everyItem().getElements(), picked(nestedStyles.value.fields)),
            nestedLineStyles: Array.map(owner.nestedLineStyles.everyItem().getElements(), picked(nestedLineStyles.value.fields)),
            tabList: Array.map(owner.tabList, picked(tabList.value.fields)),
            bulletChar: owner.bulletChar.characterValue,
        };
    };
}
class StyleView extends Schema.TaggedClass<StyleView, { readonly brand: unique symbol }>()('Style', {
    ...Reference.fields,
    group: Schema.Array(Schema.String).annotate({ description: 'Enclosing group names, nearest first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Base styles nearest first, root style excluded' }),
    nextStyle: Schema.OptionFromOptionalKey(Reference),
    own: Schema.JsonObject.annotate({ description: 'Attributes differing from base or root style' }),
    paragraphAttributes: Schema.OptionFromOptionalKey(ParagraphAttributes),
}) {
    static readonly owns = (model: typeof Model.Service) => model.owning<Styled>('basedOn');
    static readonly of = Effect.fn('StyleView.of')(function* (style: Styled) {
        const model = yield* Model;
        const enclosing = model.ancestors(style.parent);
        const refused = HostError.refused(style.toSpecifier());
        const mine = yield* Effect.try({ try: () => style.properties, catch: refused });
        const based = mine.basedOn;
        const container = Option.orElse(Array.findFirst(enclosing, model.document), () => Option.some(model.app));
        const base = Predicate.isString(based) ? yield* byName(style.constructorName, based, container) : based;
        const theirs = yield* Effect.try({ try: (): object => (model.host(base) ? base.properties : {}), catch: refused });
        const own = yield* overrides(style, mine, (name) => Reflect.get(theirs, name), { ...StyleView.fields, ...ParagraphAttributes.fields });
        return yield* read(StyleView, style.toSpecifier(), () => ({
            _tag: 'Style',
            ...reference(style),
            group: Array.flatMap(enclosing, (above) => (model.named(above) && !model.document(above) ? [above.name] : [])),
            basedOn: Array.unfold(based, (each) => (model.host(each) && Predicate.hasProperty(each, 'basedOn') ? Option.some([each, each.basedOn] as const) : Option.none())),
            nextStyle: Option.liftPredicate(mine.nextStyle, model.host),
            own,
            paragraphAttributes: Option.map(Option.liftPredicate(style, model.owning<ParagraphStyle>('nestedGrepStyles')), ParagraphAttributes.of),
        }));
    });
}
class StoryView extends Schema.TaggedClass<StoryView, { readonly brand: unique symbol }>()('Story', {
    ...Reference.fields,
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(Reference.mapFields(Struct.assign({ page: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Page name, absent on pasteboard' })), columns: Schema.OptionFromOptionalKey(Schema.Int), overflows: Schema.Boolean, characters: Schema.Int }))).annotate({
        description: 'Text frames and paths in thread order',
    }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Applied paragraph style names' }),
}) {
    static readonly owns = (model: typeof Model.Service) => model.owning<Story>('textContainers');
    static readonly of = (story: Story) =>
        read(StoryView, story.toSpecifier(), () => {
            const containers = story.textContainers;
            return {
                _tag: 'Story',
                ...reference(story),
                length: story.length,
                overflows: story.overflows,
                overset: story.length - Number.sumAll(Array.map(containers, ({ characters }) => characters.length)),
                textContainers: Array.map(containers, (container) => ({
                    ...reference(container),
                    page: 'parentPage' in container ? Option.map(Option.fromNullOr(container.parentPage), Struct.get('name')) : Option.none(),
                    columns: 'textFramePreferences' in container ? Option.some(container.textFramePreferences.textColumnCount) : Option.none(),
                    overflows: container.overflows,
                    characters: container.characters.length,
                })),
                styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, Struct.get('name'))),
            };
        });
}
const Run = Reference.mapFields(
    Struct.assign({
        contents: Schema.Json,
        length: Schema.Int,
        appliedCharacterStyle: Reference,
        appliedFont: Reference,
        pointSize: Schema.Finite,
        designAxes: Schema.Result(Schema.Array(Schema.Finite), Schema.Defect()).annotate({ description: 'Axis values in fvar order, empty for static faces, refused at some face sizes' }),
        opentypeFeatures: Schema.Array(Schema.Json).annotate({ description: 'OpenType feature tag and value pairs' }),
        kerningValue: Schema.OptionFromOptionalKey(Schema.Finite).annotate({ description: 'Manual kerning, absent under metrics or optical kerning' }),
        appliedConditions: Schema.Array(Reference),
        rubyFlag: Schema.Boolean,
        rubyString: Schema.String,
        changed: Schema.JsonObject.annotate({ description: 'Attributes differing from character style, else paragraph style' }),
    }),
);
class TextView extends Schema.TaggedClass<TextView, { readonly brand: unique symbol }>()('Text', {
    ...Reference.fields,
    parentStory: Reference,
    contents: Schema.Json.annotate({ description: 'Text, or SpecialCharacters member for one special character' }),
    length: Schema.Int,
    appliedParagraphStyle: Reference,
    appliedCharacterStyle: Reference,
    overrides: Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Text differs from paragraph or character style' }),
    ranges: Schema.Int.annotate({ description: 'Style range count' }),
    runs: Schema.Array(Run).annotate({ description: 'Style ranges of the slice in order' }),
    paragraphAttributes: ParagraphAttributes,
}) {
    static readonly owns = (model: typeof Model.Service) => Predicate.or(model.text, model.frame);
    static readonly overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
        Model.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
    static readonly of = Effect.fn('TextView.of')(function* (found: Text | TextFrame, slice: Slice) {
        const { enumerator, NothingEnum, frame } = yield* Model;
        const text = frame(found) ? found.texts.item(0) : found;
        const overridden = yield* TextView.overrides(text);
        const every = text.textStyleRanges.everyItem().getElements();
        const ranges = slice.of(every);
        const styles = yield* Effect.try({
            try: () =>
                Record.map(
                    Record.fromIterableWith(
                        Array.flatMap(ranges, (run) => [run.appliedParagraphStyle, run.appliedCharacterStyle]),
                        (style) => [style.toSpecifier(), style],
                    ),
                    Struct.get('properties'),
                ),
            catch: HostError.refused(`styles of ${text.toSpecifier()}`),
        });
        const runs = yield* Effect.forEach(
            ranges,
            Effect.fnUntraced(function* (run) {
                const [own, based, applied] = yield* Effect.try({ try: () => [run.properties, { ...styles[run.appliedParagraphStyle.toSpecifier()] }, { ...styles[run.appliedCharacterStyle.toSpecifier()] }] as const, catch: HostError.refused(run.toSpecifier()) });
                const inherited = (name: string): unknown => {
                    const set: unknown = Reflect.get(applied, name);
                    return set === null || set === '' || (enumerator(set) && NothingEnum.NOTHING.equals(set)) ? Reflect.get(based, name) : set;
                };
                return {
                    ...picked(Run.fields)(own),
                    ...reference(run),
                    designAxes: Result.try(() => run.designAxes),
                    changed: yield* overrides(run, own, inherited, { ...Run.fields, ...TextView.fields, ...ParagraphAttributes.fields }),
                };
            }),
        );
        return yield* read(TextView, text.toSpecifier(), () => ({
            _tag: 'Text',
            ...reference(text),
            ...Struct.pick(text, ['parentStory', 'contents', 'length', 'appliedParagraphStyle', 'appliedCharacterStyle']),
            overrides: overridden,
            ranges: every.length,
            runs,
            paragraphAttributes: ParagraphAttributes.of(text),
        }));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const overrides = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, own: object, base: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ rows, plural }, encoded] = yield* Effect.all([Model, encoder]);
    const { fields } = yield* Cache.get(rows, object.constructorName);
    return Record.filterMap(fields, (row, name) => (row.settable && !plural(row) && !Object.hasOwn(except, name) && encoded(Reflect.get(own, name)) !== encoded(base(name)) ? Result.succeed<unknown>(Reflect.get(own, name)) : Result.failVoid));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ParagraphAttributes, StoryView, StyleView, TextView };
