import type { ParagraphStyle, Story, Text, TextFrame } from 'adobe:indesign';
import { Array, Cache, Cause, Effect, flow, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Model } from './model.ts';
import { encoder, Reference, read } from './references.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Composition = Schema.Struct({
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'Automatic GREP styles: character style applied wherever the expression matches, in order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
    tabStops: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String })),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Int.annotate({ description: 'Bullet character code point' }),
    numberingFormat: Schema.Json,
}).annotate({ description: 'Composition rules a paragraph style, a text range, or the document text defaults own' });
const Overrides = Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Whether the text differs from its paragraph and character styles' });
const Run = Reference.mapFields(
    Struct.assign({
        contents: Schema.Json,
        length: Schema.Int,
        appliedCharacterStyle: Reference,
        appliedFont: Reference,
        pointSize: Schema.Finite,
        designAxes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Finite).annotate({ description: 'Design axis values in fvar order, absent for a static face' })),
        opentypeFeatures: Schema.Array(Schema.Json).annotate({ description: 'OpenType feature tag and value pairs set on the run' }),
        changed: Schema.JsonObject.annotate({ description: 'Other attributes the run sets differently from its paragraph and character styles' }),
    }),
);
class StoryView extends Schema.TaggedClass<StoryView, { readonly brand: unique symbol }>()('Story', {
    ...Reference.fields,
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(Reference.mapFields(Struct.assign({ page: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Page name, absent on the pasteboard' })), columns: Schema.OptionFromOptionalKey(Schema.Int), overflows: Schema.Boolean, characters: Schema.Int }))).annotate({
        description: 'Text frames and paths in thread order',
    }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Paragraph style names the story applies' }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<Story>('textContainers')), Option.map(StoryView.of));
    static readonly of = Effect.fn('of')(function* (story: Story) {
        return yield* read(StoryView, () => {
            const containers = story.textContainers;
            return {
                _tag: 'Story',
                ...Reference.of(story),
                length: story.length,
                overflows: story.overflows,
                overset: story.length - Number.sumAll(Array.map(containers, ({ characters }) => characters.length)),
                textContainers: Array.map(containers, (container) => ({
                    ...Reference.of(container),
                    page: 'parentPage' in container ? Option.map(Option.fromNullOr(container.parentPage), Struct.get('name')) : Option.none(),
                    columns: 'textFramePreferences' in container ? Option.some(container.textFramePreferences.textColumnCount) : Option.none(),
                    overflows: container.overflows,
                    characters: container.characters.length,
                })),
                styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, Struct.get('name'))),
            };
        });
    });
}
class TextView extends Schema.TaggedClass<TextView, { readonly brand: unique symbol }>()('Text', {
    ...Reference.fields,
    parentStory: Reference,
    contents: Schema.Json.annotate({ description: 'Text, or the SpecialCharacters member of a range holding one special character' }),
    length: Schema.Int,
    appliedParagraphStyle: Reference,
    appliedCharacterStyle: Reference,
    overrides: Overrides,
    runs: Schema.Array(Run).annotate({ description: 'Style ranges in order' }),
    composition: Composition,
}) {
    static readonly viewed = ({ text, frame }: Model['Service']) => flow(Option.liftPredicate(Predicate.or(text, frame)), Option.map(TextView.of));
    static readonly overrides = Effect.fn('overrides')(function* (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) {
        const { StyleType } = yield* Model;
        return yield* Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: (cause) => new Cause.UnknownError(cause, `overrides of ${text.toSpecifier()}`) });
    });
    static readonly of = Effect.fn('of')(function* (found: Text | TextFrame) {
        const [model, encoded] = yield* Effect.all([Model, encoder]);
        const text = model.frame(found) ? found.texts.item(0) : found;
        const { fields } = yield* Cache.get(model.rows, 'TextStyleRange');
        const attributes = Record.keys(Record.filter(fields, (row, name) => row.settable && !model.plural(row) && !Object.hasOwn(Run.fields, name) && !Object.hasOwn(TextView.fields, name)));
        const overrides = yield* TextView.overrides(text);
        const runs = yield* Effect.forEach(
            text.textStyleRanges.everyItem().getElements(),
            Effect.fnUntraced(function* (run) {
                const [own, paragraph, character] = yield* Effect.try({ try: () => [run.properties, run.appliedParagraphStyle.properties, run.appliedCharacterStyle.properties] as const, catch: (cause) => new Cause.UnknownError(cause, run.toSpecifier()) });
                const set = (holder: object, name: string) => encoded(Reflect.get(holder, name));
                return {
                    ...Reference.of(run),
                    contents: run.contents,
                    length: run.length,
                    appliedCharacterStyle: run.appliedCharacterStyle,
                    appliedFont: run.appliedFont,
                    pointSize: run.pointSize,
                    designAxes: Option.liftThrowable(() => run.designAxes)(),
                    opentypeFeatures: run.opentypeFeatures,
                    changed: Record.fromIterableWith(
                        attributes.filter((name) => set(own, name) !== set(paragraph, name) && set(own, name) !== set(character, name)),
                        (name) => [name, Reflect.get(own, name)],
                    ),
                };
            }),
        );
        return yield* read(TextView, () => ({
            _tag: 'Text',
            ...Reference.of(text),
            parentStory: text.parentStory,
            contents: text.contents,
            length: text.length,
            appliedParagraphStyle: text.appliedParagraphStyle,
            appliedCharacterStyle: text.appliedCharacterStyle,
            overrides,
            runs,
            composition: composition(text),
        }));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const composition = (owner: Pick<ParagraphStyle, keyof typeof Composition.fields>) => ({
    nestedGrepStyles: Array.map(owner.nestedGrepStyles.everyItem().getElements(), ({ grepExpression, appliedCharacterStyle }) => ({ grepExpression, appliedCharacterStyle })),
    nestedStyles: Array.map(owner.nestedStyles.everyItem().getElements(), ({ appliedCharacterStyle, delimiter, repetition, inclusive }) => ({ appliedCharacterStyle, delimiter, repetition, inclusive })),
    nestedLineStyles: Array.map(owner.nestedLineStyles.everyItem().getElements(), ({ appliedCharacterStyle, lineCount, repeatLast }) => ({ appliedCharacterStyle, lineCount, repeatLast })),
    tabStops: Array.map(owner.tabStops.everyItem().getElements(), ({ position, alignment, leader, alignmentCharacter }) => ({ position, alignment, leader, alignmentCharacter })),
    bulletsAndNumberingListType: owner.bulletsAndNumberingListType,
    numberingLevel: owner.numberingLevel,
    bulletChar: owner.bulletChar.characterValue,
    numberingFormat: owner.numberingFormat,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composition, composition, Overrides, StoryView, TextView };
