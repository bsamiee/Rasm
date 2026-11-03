import type { CellStyle, CharacterStyle, InDesignDOMObject, ObjectStyle, ParagraphStyle, Story, TableStyle, Text, TextFrame } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Settings } from '@rasm/typography/settings';
import { Array, Cache, Effect, Number, Option, Predicate, Record, Result, type Schema, Struct } from 'effect';
import { ParagraphAttributes, StoryView, StyleView, TextView } from '#/contract';
import { HostSettings } from '#/documents/settings';
import { Model } from '#/dom/model';
import type { Slice } from '#/dom/slice';
import { Value } from '#/dom/value';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]

const paragraphAttributes = (owner: Pick<ParagraphStyle, keyof typeof ParagraphAttributes.fields & keyof ParagraphStyle>, settings: Settings) => {
    const { nestedGrepStyles, nestedStyles, nestedLineStyles, tabList } = ParagraphAttributes.fields;
    return {
        ...Value.picked(ParagraphAttributes.fields)(owner),
        nestedGrepStyles: Array.map(owner.nestedGrepStyles.everyItem().getElements(), Value.picked(nestedGrepStyles.value.fields)),
        nestedStyles: Array.map(owner.nestedStyles.everyItem().getElements(), Value.picked(nestedStyles.value.fields)),
        nestedLineStyles: Array.map(owner.nestedLineStyles.everyItem().getElements(), Value.picked(nestedLineStyles.value.fields)),
        tabList: Array.map(owner.tabList, Value.picked(tabList.value.fields)),
        bulletChar: owner.bulletChar.characterValue,
        settings,
    };
};
const overrides = (text: Pick<Text, 'textHasOverrides' | 'toSpecifier'>) =>
    Model.use(({ StyleType }) => Effect.try({ try: () => ({ paragraph: text.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }), catch: HostError.refused(`overrides of ${text.toSpecifier()}`) }));
const differing = Effect.fnUntraced(function* (object: InDesignDOMObject<unknown>, properties: object, baseValue: (name: string) => unknown, except: Schema.Struct.Fields) {
    const [{ declarations, collectionField }, encoded] = yield* Effect.all([Model, Model.encoder]);
    const { fields } = yield* Cache.get(declarations, object.constructorName);
    return Record.filterMap(fields, (row, name) => (row.settable && !collectionField(row) && !Object.hasOwn(except, name) && encoded(Reflect.get(properties, name)) !== encoded(baseValue(name)) ? Result.succeed<unknown>(Reflect.get(properties, name)) : Result.failVoid));
});

// --- [VIEWS]

const styleView = Effect.fnUntraced(function* (style: ParagraphStyle | CharacterStyle | ObjectStyle | TableStyle | CellStyle) {
    const model = yield* Model;
    const ancestors = model.ancestors(style.parent);
    const refused = HostError.refused(style.toSpecifier());
    const properties = yield* Effect.try({ try: () => style.properties, catch: refused });
    const { basedOn } = properties;
    const container = Option.orElse(Array.findFirst(ancestors, model.document), () => Option.some(model.app));
    const base = Predicate.isString(basedOn) ? yield* Model.object(style.constructorName, basedOn, container) : basedOn;
    const baseProperties = yield* Effect.try({ try: (): object => (model.domObject(base) ? base.properties : {}), catch: refused });
    const own = yield* differing(style, properties, (name) => Reflect.get(baseProperties, name), { ...StyleView.fields, ...ParagraphAttributes.fields });
    const paragraph = Option.liftPredicate(style, model.paragraphStyle);
    const settings = yield* Effect.transposeOption(Option.map(paragraph, HostSettings.read));
    return yield* Model.read(StyleView, style.toSpecifier(), () => ({
        _tag: 'Style',
        ...Model.referenceUnsafe(style),
        group: Array.filterMap(ancestors, (ancestor) => (model.named(ancestor) && !model.document(ancestor) ? Result.succeed(ancestor.name) : Result.failVoid)),
        basedOn: Array.unfold(basedOn, (above) => (model.style(above) ? Option.some([above, above.basedOn] as const) : Option.none())),
        nextStyle: 'nextStyle' in properties ? Option.liftPredicate(properties.nextStyle, model.domObject) : Option.none(),
        differing: own,
        paragraphAttributes: Option.zipWith(paragraph, settings, paragraphAttributes),
    }));
});
const storyView = (story: Story) =>
    Model.read(StoryView, story.toSpecifier(), () => {
        const containers = story.textContainers;
        return {
            _tag: 'Story',
            ...Model.referenceUnsafe(story),
            length: story.length,
            overflows: story.overflows,
            overset: story.length - Number.sumAll(Array.map(containers, ({ characters }) => characters.length)),
            textContainers: Array.map(containers, (container) => ({
                ...Model.referenceUnsafe(container),
                page: container.constructorName === 'TextPath' ? Option.none() : Option.map(Option.fromNullishOr(container.parentPage), Struct.get('name')),
                columns: container.constructorName === 'TextPath' ? Option.none() : Option.some(container.textFramePreferences.textColumnCount),
                overflows: container.overflows,
                characters: container.characters.length,
            })),
            styles: Array.dedupe(Array.map(story.paragraphs.everyItem().appliedParagraphStyle, Struct.get('name'))),
        };
    });
const textView = Effect.fnUntraced(function* (source: Text | TextFrame, slice: Slice) {
    const { nothing, frame } = yield* Model;
    const text = frame(source) ? source.texts.item(0) : source;
    const [overridden, settings] = yield* Effect.all([overrides(text), HostSettings.read(text)]);
    const allRanges = text.textStyleRanges.everyItem().getElements();
    const ranges = slice.of(allRanges);
    const { fields: runFields } = TextView.fields.runs.value;
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
            const [properties, paragraphStyle, characterStyle] = yield* Effect.try({ try: () => [run.properties, { ...styles[run.appliedParagraphStyle.toSpecifier()] }, { ...styles[run.appliedCharacterStyle.toSpecifier()] }] as const, catch: HostError.refused(run.toSpecifier()) });
            const inherited = (name: string): unknown => {
                const value: unknown = Reflect.get(characterStyle, name);
                return value === null || value === '' || nothing(value) ? Reflect.get(paragraphStyle, name) : value;
            };
            return {
                ...Value.picked(runFields)(properties),
                ...(yield* Model.reference(run)),
                designAxes: Result.try(() => run.designAxes),
                settings: yield* HostSettings.read(run),
                differing: yield* differing(run, properties, inherited, { ...runFields, ...TextView.fields, ...ParagraphAttributes.fields }),
            };
        }),
    );
    return yield* Model.read(TextView, text.toSpecifier(), () => ({
        _tag: 'Text',
        ...Model.referenceUnsafe(text),
        ...Struct.pick(text, ['parentStory', 'contents', 'length', 'appliedParagraphStyle', 'appliedCharacterStyle']),
        overrides: overridden,
        ranges: allRanges.length,
        runs,
        paragraphAttributes: paragraphAttributes(text, settings),
    }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { overrides, paragraphAttributes, storyView, styleView, textView };
