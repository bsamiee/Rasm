import type { Story, Text, TextFrame } from 'adobe:indesign';
import { Array, Cache, Cause, Effect, identity, Match, Option, Record, Schema, Struct } from 'effect';
import { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { Reference, read } from './objects/specifiers.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Threaded = Story | TextFrame | Text;

// --- [MODELS] --------------------------------------------------------------------------

class ThreadView extends Schema.TaggedClass<ThreadView, { readonly brand: unique symbol }>()('Thread', {
    story: Reference,
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    containers: Schema.Array(Reference.pipe(Schema.fieldsAssign({ page: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Page name, absent on the pasteboard' })), columns: Schema.OptionFromOptionalKey(Schema.Int), overflows: Schema.Boolean, characters: Schema.Int }))).annotate({
        description: 'Text frames and paths in thread order',
    }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Paragraph style names the story applies' }),
    range: Schema.OptionFromOptionalKey(
        Reference.pipe(
            Schema.fieldsAssign({
                contents: Schema.String,
                length: Schema.Int,
                paragraphStyle: Reference,
                characterStyle: Reference,
                overrides: Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)),
                runs: Schema.Array(Reference.pipe(Schema.fieldsAssign({ length: Schema.Int, contents: Schema.String, characterStyle: Reference, changed: Schema.JsonObject.annotate({ description: 'Attributes the run sets differently from its paragraph and character styles' }) }))).annotate({
                    description: 'Style ranges of the range in order',
                }),
            }),
        ),
    ).annotate({ description: 'The text the target names: a range itself, or the text a frame holds; absent for a whole story' }),
}) {
    static readonly guard = ({ owning }: Classes['Service']) => {
        const guards = [owning<Story>('textContainers'), owning<TextFrame>('textFrameIndex'), owning<Text>('parentTextFrames')];
        return (value: unknown): value is Threaded => guards.some((guard) => guard(value));
    };
    static readonly of = Effect.fn('of')(function* (found: Threaded) {
        const [{ indesign }, { rows, owning, named, plural, encoded, reference }] = yield* Effect.all([Host, Classes]);
        const { fields } = yield* Cache.get(rows, 'TextStyleRange');
        const attributes = Record.keys(Record.filter(fields, (row, name) => row.settable && !['contents', 'appliedParagraphStyle', 'appliedCharacterStyle', 'label', 'name'].includes(name) && !plural(row)));
        const story = 'parentStory' in found ? found.parentStory : found;
        const range = Match.value(found).pipe(
            Match.when(owning<Text>('parentTextFrames'), identity<Text>),
            Match.when(owning<TextFrame>('textFrameIndex'), (frame) => frame.texts.item(0)),
            Match.option,
        );
        const described = (text: Text) =>
            Effect.try({
                try: () => ({
                    ...reference(text),
                    contents: text.contents,
                    length: text.length,
                    paragraphStyle: reference(text.appliedParagraphStyle),
                    characterStyle: reference(text.appliedCharacterStyle),
                    overrides: { paragraph: text.textHasOverrides(indesign.StyleType.PARAGRAPH_STYLE_TYPE), character: text.textHasOverrides(indesign.StyleType.CHARACTER_STYLE_TYPE) },
                    runs: text.textStyleRanges
                        .everyItem()
                        .getElements()
                        .map((run) => {
                            const [own, paragraph, character] = [run.properties, run.appliedParagraphStyle.properties, run.appliedCharacterStyle.properties];
                            const set = (holder: object, name: string) => encoded(Reflect.get(holder, name), false);
                            return {
                                ...reference(run),
                                length: run.length,
                                contents: run.contents,
                                characterStyle: reference(run.appliedCharacterStyle),
                                changed: Record.fromIterableWith(
                                    attributes.filter((name) => set(own, name) !== set(paragraph, name) && (Reflect.get(character, name) === undefined || set(own, name) !== set(character, name))),
                                    (name) => [name, Reflect.get(own, name)],
                                ),
                            };
                        }),
                }),
                catch: (cause) => new Cause.UnknownError(cause, text.toSpecifier()),
            });
        const selected = yield* Effect.transposeOption(Option.map(range, described));
        const containers = story.textContainers;
        return yield* read(ThreadView, () => ({
            _tag: 'Thread',
            story: reference(story),
            length: story.length,
            overflows: story.overflows,
            overset: story.length - containers.reduce((sum, container) => sum + container.characters.length, 0),
            containers: containers.map((container) => ({
                ...reference(container),
                page: 'parentPage' in container ? Option.getOrUndefined(Option.map(Option.liftPredicate(container.parentPage, named), Struct.get('name'))) : undefined,
                columns: 'textFramePreferences' in container ? container.textFramePreferences.textColumnCount : undefined,
                overflows: container.overflows,
                characters: container.characters.length,
            })),
            styles: Array.dedupe(story.paragraphs.everyItem().appliedParagraphStyle.map(Struct.get('name'))),
            range: Option.getOrUndefined(selected),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ThreadView };
