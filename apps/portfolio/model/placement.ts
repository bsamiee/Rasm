import { Array, Context, Effect, Match, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { type Asset, type AssetCollection, type Dimensions, Id, ImageAsset, Page, PdfAsset, VideoAsset } from './asset.ts';

// --- [MODELS] --------------------------------------------------------------------------

const text = { caption: Schema.String, alt: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String) };
const percentage = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 100 }));
const contain = Schema.Struct({ mode: Schema.Literal('contain') });
const cover = Schema.Struct({ mode: Schema.Literal('cover'), x: percentage, y: percentage });
const pageNumber = Schema.Int.check(Schema.isGreaterThan(0));
const drawing = Schema.Struct({ ...text, assetId: Id, kind: Schema.Literal('image'), role: Schema.Literal('drawing'), framing: contain });
const photograph = Schema.Struct({ ...text, assetId: Id, kind: Schema.Literal('image'), role: Schema.Literal('photograph'), framing: Schema.Union([contain, cover]) });
const sheet = Schema.Struct({ ...text, assetId: Id, kind: Schema.Literal('pdf'), page: pageNumber });
const film = Schema.Struct({
    ...text,
    assetId: Id,
    kind: Schema.Literal('video'),
    poster: Schema.optionalKey(
        Schema.Struct({ assetId: Id }).pipe(
            Schema.decodeTo(Schema.toType(ImageAsset), {
                decode: SchemaGetter.transformEffect(({ assetId }) =>
                    Assets.use((assets) =>
                        Match.value(assets[assetId]).pipe(
                            Match.when({ mime: Match.is(...ImageAsset.fields.mime.literals) }, Effect.succeed<ImageAsset>),
                            Match.orElse(() => Effect.fail(new SchemaIssue.InvalidValue({ message: 'The poster image is missing or is not an image' }))),
                        ),
                    ),
                ),
                encode: SchemaGetter.transform((asset) => ({ assetId: asset.id })),
            }),
        ),
    ),
});
const StoredPlacement = Schema.Union([drawing, photograph, sheet, film]);
const ResolvedPlacement = Schema.Union([
    drawing.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(ImageAsset) })),
    photograph.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(ImageAsset) })),
    sheet.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(PdfAsset), page: Schema.Struct({ ...Page.fields, number: pageNumber }) })),
    film.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, poster: Schema.toType(fields.poster), asset: Schema.toType(VideoAsset) })),
]);
const Placement = StoredPlacement.pipe(
    Schema.decodeTo(ResolvedPlacement, {
        decode: SchemaGetter.transformEffect(
            Effect.fnUntraced(function* ({ assetId, ...stored }: typeof StoredPlacement.Type): Effect.fn.Return<typeof ResolvedPlacement.Type, SchemaIssue.Issue, Assets> {
                return yield* Match.value({ placement: stored, asset: (yield* Assets)[assetId] }).pipe(
                    Match.when({ placement: { kind: 'pdf' }, asset: { mime: 'application/pdf' } }, ({ placement, asset }) =>
                        Effect.fromNullishOr(asset.pages[placement.page - 1]).pipe(
                            Effect.map((page) => ({ ...placement, asset, page: { ...page, number: placement.page } })),
                            Effect.mapError(() => new SchemaIssue.InvalidValue({ message: 'The PDF does not contain this sheet' })),
                        ),
                    ),
                    Match.when({ placement: { kind: 'video' }, asset: { mime: Match.is(...VideoAsset.fields.mime.literals) } }, ({ placement, asset }) => Effect.succeed({ ...placement, asset })),
                    Match.when({ placement: { kind: 'image' }, asset: { mime: Match.is(...ImageAsset.fields.mime.literals) } }, ({ placement, asset }) => Effect.succeed({ ...placement, asset })),
                    Match.orElse(() => Effect.fail(new SchemaIssue.InvalidValue({ message: 'The referenced file is missing or has a different media type' }))),
                );
            }),
        ),
        encode: SchemaGetter.transform(({ asset, ...placement }: typeof ResolvedPlacement.Type): typeof StoredPlacement.Type => (placement.kind === 'pdf' ? { ...placement, assetId: asset.id, page: placement.page.number } : { ...placement, assetId: asset.id })),
    }),
);
const Composition = Schema.Struct({ id: Id, items: Schema.Union([Schema.Tuple([Placement]), Schema.Tuple([Placement, Placement])]), scale: Schema.Literals(['full', 'medium', 'small']), align: Schema.Literals(['left', 'center', 'right']) });

// --- [SERVICES] ------------------------------------------------------------------------

class Assets extends Context.Service<Assets, typeof AssetCollection.Type>()('portfolio/Assets') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const placementsFor = (asset: typeof Asset.Type, allPages: boolean): Array.NonEmptyReadonlyArray<typeof Placement.Type> =>
    Match.value(asset).pipe(
        Match.when({ mime: 'application/pdf' }, (document) => Array.map(allPages ? document.pages : Array.of(Array.headNonEmpty(document.pages)), (page, index): typeof Placement.Type => ({ kind: 'pdf', asset: document, page: { ...page, number: index + 1 }, caption: '' }))),
        Match.when({ mime: Match.is(...VideoAsset.fields.mime.literals) }, (video) => [{ kind: 'video', asset: video, caption: '' }] as const),
        Match.orElse((image) => [{ kind: 'image', asset: image, caption: '', role: 'drawing', framing: { mode: 'contain' } }] as const),
    );
const placementDimensions = (placement: typeof Placement.Type): typeof Dimensions.Type => (placement.kind === 'pdf' ? placement.page : placement.asset);
const createComposition = (id: string, ...items: typeof Composition.Type.items): typeof Composition.Type => ({ id, items, scale: 'full', align: 'center' });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Assets, Composition, createComposition, Placement, placementDimensions, placementsFor };
