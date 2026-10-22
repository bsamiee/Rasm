import type { Story } from 'adobe:indesign';
import { Array, flow, Number, Option, Schema, Struct } from 'effect';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { StoryView };
