import type { Page } from 'adobe:indesign';
import { Array, Effect, flow, Option, Schema, Struct } from 'effect';
import { Extent } from '#/documents/extent';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

class PageView extends Schema.TaggedClass<PageView, { readonly brand: unique symbol }>()('Page', {
    ...Reference.fields,
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread holding the page' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    margins: Schema.Struct({
        ...Struct.Record(['top', 'bottom', 'inside', 'outside'], Schema.Finite),
        columns: Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Left and right edge of each column from the left margin' }),
    }),
    items: Schema.Struct({ own: Schema.Int, inherited: Schema.Int }).annotate({ description: 'Page items held, and parent items shown' }),
}) {
    static readonly viewed = ({ page }: Model['Service']) => flow(Option.liftPredicate(page), Option.map(PageView.of));
    static readonly of = (page: Page) =>
        Effect.flatMap(Extent.of(page), (box) =>
            read(PageView, page.toSpecifier(), () => {
                const { marginPreferences: margins } = page;
                return {
                    _tag: 'Page',
                    ...Reference.of(page),
                    documentOffset: page.documentOffset,
                    side: page.side,
                    extent: box,
                    parent: page.parent,
                    appliedMaster: Option.fromNullOr(page.appliedMaster),
                    appliedSection: page.appliedSection,
                    margins: { top: margins.top, bottom: margins.bottom, inside: margins.left, outside: margins.right, columns: Array.chunksOf(margins.columnsPositions, 2) },
                    items: { own: page.pageItems.length, inherited: page.masterPageItems.length },
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PageView };
