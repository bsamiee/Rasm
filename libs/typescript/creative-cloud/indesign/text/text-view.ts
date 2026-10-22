import type { Text, TextFrame } from 'adobe:indesign';
import { Cause, Effect, flow, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { read } from '#/dom/encoding';
import { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';
import { overrides } from '#/styles/overrides';
import { ParagraphAttributes } from '#/styles/paragraph-attributes';

// --- [MODELS] --------------------------------------------------------------------------

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
class TextView extends Schema.TaggedClass<TextView, { readonly brand: unique symbol }>()('Text', {
    ...Reference.fields,
    parentStory: Reference,
    contents: Schema.Json.annotate({ description: 'Text, or the SpecialCharacters member of a range holding one special character' }),
    length: Schema.Int,
    appliedParagraphStyle: Reference,
    appliedCharacterStyle: Reference,
    overrides: Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Whether the text differs from its paragraph and character styles' }),
    runs: Schema.Array(Run).annotate({ description: 'Style ranges in order' }),
    paragraphAttributes: ParagraphAttributes,
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
                    changed: yield* overrides(run, own, (name) => Reflect.get(styled, name), { ...Run.fields, ...TextView.fields, ...ParagraphAttributes.fields }),
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
            paragraphAttributes: ParagraphAttributes.of(text),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TextView };
