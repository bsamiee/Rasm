import type { ParagraphStyle, Story, Text, TextFrame } from 'adobe:indesign';
import { Array, Cause, Effect, flow, Number, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { Model } from './model.ts';
import { overrides, Reference, read } from './references.ts';

// --- [MODELS] --------------------------------------------------------------------------

const TabStop = Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String });
class Composition extends Schema.Class<Composition, { readonly brand: unique symbol }>('Composition')(
    {
        nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'Automatic GREP styles: character style applied wherever the expression matches, in order' }),
        nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
        nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
        tabList: Schema.Array(TabStop),
        bulletsAndNumberingListType: Schema.String,
        numberingLevel: Schema.Int,
        bulletChar: Schema.Int.annotate({ description: 'Bullet character code point' }),
        numberingFormat: Schema.Json,
    },
    { description: 'Composition rules a paragraph style, a text range, or the document text defaults own' },
) {
    static readonly of = (owner: Pick<ParagraphStyle, keyof typeof Composition.fields>) => ({
        nestedGrepStyles: Array.map(owner.nestedGrepStyles.everyItem().getElements(), ({ grepExpression, appliedCharacterStyle }) => ({ grepExpression, appliedCharacterStyle })),
        nestedStyles: Array.map(owner.nestedStyles.everyItem().getElements(), ({ appliedCharacterStyle, delimiter, repetition, inclusive }) => ({ appliedCharacterStyle, delimiter, repetition, inclusive })),
        nestedLineStyles: Array.map(owner.nestedLineStyles.everyItem().getElements(), ({ appliedCharacterStyle, lineCount, repeatLast }) => ({ appliedCharacterStyle, lineCount, repeatLast })),
        tabList: Array.map(owner.tabList, (tab) => Record.fromIterableWith(Struct.keys(TabStop.fields), (key) => [key, Reflect.get(tab, key)])),
        bulletsAndNumberingListType: owner.bulletsAndNumberingListType,
        numberingLevel: owner.numberingLevel,
        bulletChar: owner.bulletChar.characterValue,
        numberingFormat: owner.numberingFormat,
    });
}
const Overrides = Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Whether the text differs from its paragraph and character styles' });
const Run = Reference.mapFields(
    Struct.assign({
        contents: Schema.Json,
        length: Schema.Int,
        appliedCharacterStyle: Reference,
        appliedFont: Reference,
        pointSize: Schema.Finite,
        designAxes: Schema.Result(Schema.Array(Schema.Finite), Schema.Defect()).annotate({ description: 'Design axis values in fvar order, empty for a static face, or the refusal InDesign answers for some faces at some sizes' }),
        opentypeFeatures: Schema.Array(Schema.Json).annotate({ description: 'OpenType feature tag and value pairs set on the run' }),
        kerningValue: Schema.OptionFromOptionalKey(Schema.Finite).annotate({ description: 'Manual kerning, absent where InDesign kerns by metrics or optically' }),
        appliedConditions: Schema.Array(Reference),
        rubyFlag: Schema.Boolean,
        rubyString: Schema.String,
        changed: Schema.JsonObject.annotate({ description: "Other attributes the run sets differently from its styles: the character style's value where it sets one, else the paragraph style's" }),
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
    static readonly of = (story: Story) =>
        read(StoryView, story.toSpecifier(), () => {
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
    static readonly overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
        Model.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: (cause) => new Cause.UnknownError(cause, `overrides of ${text.toSpecifier()}`) }));
    static readonly of = Effect.fn('TextView.of')(function* (found: Text | TextFrame) {
        const { frame, Enumerator, NothingEnum } = yield* Model;
        const text = frame(found) ? found.texts.item(0) : found;
        const overridden = yield* TextView.overrides(text);
        const runs = yield* Effect.forEach(
            text.textStyleRanges.everyItem().getElements(),
            Effect.fnUntraced(function* (run) {
                const [own, paragraph, character] = yield* Effect.try({ try: () => [run.properties, run.appliedParagraphStyle.properties, run.appliedCharacterStyle.properties] as const, catch: (cause) => new Cause.UnknownError(cause, run.toSpecifier()) });
                const styled = { ...paragraph, ...Record.filter(character, (set: unknown) => !(set === null || set === '' || (set instanceof Enumerator && NothingEnum.NOTHING.equals(set)))) };
                return {
                    ...Reference.of(run),
                    ...Struct.pick(own, ['contents', 'length', 'appliedCharacterStyle', 'appliedFont', 'pointSize', 'opentypeFeatures', 'kerningValue', 'appliedConditions', 'rubyFlag', 'rubyString']),
                    designAxes: Result.try(() => run.designAxes),
                    changed: yield* overrides(run, own, (name) => Reflect.get(styled, name), { ...Run.fields, ...TextView.fields, ...Composition.fields }),
                };
            }),
        );
        return yield* read(TextView, text.toSpecifier(), () => ({
            _tag: 'Text',
            ...Reference.of(text),
            parentStory: text.parentStory,
            contents: text.contents,
            length: text.length,
            appliedParagraphStyle: text.appliedParagraphStyle,
            appliedCharacterStyle: text.appliedCharacterStyle,
            overrides: overridden,
            runs,
            composition: Composition.of(text),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composition, Overrides, StoryView, TextView };
