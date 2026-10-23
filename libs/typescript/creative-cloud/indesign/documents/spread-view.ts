import type { MasterSpread, Spread } from 'adobe:indesign';
import { Effect, flow, Option, Predicate, Schema, Struct } from 'effect';
import { Extent } from '#/documents/extent';
import { PageView } from '#/documents/page-view';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

class SpreadView extends Schema.TaggedClass<SpreadView, { readonly brand: unique symbol }>()('Spread', {
    ...Reference.fields,
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent, or base parent of a parent spread' }),
    kind: Schema.TaggedUnion({ Spread: { allowPageShuffle: Schema.Boolean }, MasterSpread: { namePrefix: Schema.String, baseName: Schema.String } }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(PageView.fields, ['side', 'extent'])))).annotate({ description: 'Pages in order' }),
    items: Schema.Int,
}) {
    static readonly viewed = ({ owning, spread }: Model['Service']) => flow(Option.liftPredicate(Predicate.or(spread, owning<MasterSpread>('namePrefix'))), Option.map(SpreadView.of));
    static readonly of = (found: Spread | MasterSpread) =>
        Effect.flatMap(
            Effect.forEach(found.pages.everyItem().getElements(), (page) => Effect.map(Extent.of(page), (box) => ({ ...Reference.of(page), side: page.side, extent: box }))),
            (pages) =>
                read(SpreadView, found.toSpecifier(), () => ({
                    _tag: 'Spread',
                    ...Reference.of(found),
                    index: found.index,
                    showMasterItems: found.showMasterItems,
                    appliedMaster: Option.fromNullOr(found.appliedMaster),
                    kind: found.constructorName === 'Spread' ? { _tag: 'Spread', allowPageShuffle: found.allowPageShuffle } : { _tag: 'MasterSpread', namePrefix: found.namePrefix, baseName: found.baseName },
                    pages,
                    items: found.pageItems.length,
                })),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { SpreadView };
