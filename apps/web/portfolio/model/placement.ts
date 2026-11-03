import { Array, Effect, Match, Option, Record, Schema, type SchemaAST, SchemaGetter, SchemaIssue } from 'effect';
import { Asset, AssetId, Assets, ImageAsset, PdfAsset, Sheet, VideoAsset } from '#/model/asset.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const percentBounds = { minimum: 0, maximum: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const CompositionId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('CompositionId'));
const text = { caption: Schema.String, alt: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String) };
const percentage = Schema.Finite.check(Schema.isBetween(percentBounds));
const drawing = Schema.Struct({ ...text, assetId: AssetId, kind: Schema.Literal('image'), role: Schema.Literal('drawing') });
const photograph = Schema.Struct({
    ...text,
    assetId: AssetId,
    kind: Schema.Literal('image'),
    role: Schema.Literal('photograph'),
    framing: Schema.Union([Schema.Struct({ mode: Schema.Literal('contain') }), Schema.Struct({ mode: Schema.Literal('cover'), x: percentage, y: percentage })]),
});
const sheet = Schema.Struct({ ...text, assetId: AssetId, kind: Schema.Literal('pdf'), page: Sheet.fields.number });
const resolve = <K extends (typeof Asset.Type)['kind']>(kind: K, assetId: typeof AssetId.Type, input: unknown, options: SchemaAST.ParseOptions) =>
    Assets.use((assets) =>
        Option.match(Record.get(assets, assetId), {
            onNone: () => Effect.fail(new SchemaIssue.InvalidValue({ message: `File ${assetId} missing from library` }, input, options)),
            onSome: (asset) => (Asset.isAnyOf([kind])(asset) ? Effect.succeed(asset) : Effect.fail(new SchemaIssue.InvalidValue({ message: `${asset.name} is ${asset.kind}, placement needs ${kind}` }, input, options))),
        }),
    );
const film = Schema.Struct({
    ...text,
    assetId: AssetId,
    kind: Schema.Literal('video'),
    poster: Schema.optionalKey(
        Schema.Struct({ assetId: AssetId }).pipe(
            Schema.decodeTo(Schema.toType(ImageAsset), {
                decode: SchemaGetter.transformEffect((input, options) => resolve('image', input.assetId, input, options)),
                encode: SchemaGetter.transform((asset) => ({ assetId: asset.id })),
            }),
        ),
    ),
});
const StoredPlacement = Schema.Union([drawing, photograph, sheet, film]);
const ResolvedPlacement = Schema.Union([
    drawing.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(ImageAsset) })),
    photograph.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(ImageAsset) })),
    sheet.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, asset: Schema.toType(PdfAsset), page: Sheet })),
    film.mapFields(({ assetId: _assetId, ...fields }) => ({ ...fields, poster: Schema.toType(fields.poster), asset: Schema.toType(VideoAsset) })),
]);
const Placement = StoredPlacement.pipe(
    Schema.decodeTo(ResolvedPlacement, {
        decode: SchemaGetter.transformEffect((input: typeof StoredPlacement.Type, options: SchemaAST.ParseOptions) => {
            const { assetId, ...stored } = input;
            return Match.value(stored).pipe(
                Match.discriminatorsExhaustive('kind')({
                    pdf: (placement) =>
                        Effect.flatMap(resolve('pdf', assetId, input, options), (asset) =>
                            Option.match(Array.get(asset.pages, placement.page - 1), {
                                onNone: () => Effect.fail(new SchemaIssue.InvalidValue({ message: `${asset.name} has ${asset.pages.length} sheets, placement needs sheet ${placement.page}` }, input, options)),
                                onSome: (page) => Effect.succeed({ ...placement, asset, page: { ...page, number: placement.page } }),
                            }),
                        ),
                    video: (placement) => Effect.map(resolve('video', assetId, input, options), (asset) => ({ ...placement, asset })),
                    image: (placement) => Effect.map(resolve('image', assetId, input, options), (asset) => ({ ...placement, asset })),
                }),
            );
        }),
        encode: SchemaGetter.transform(({ asset, ...placement }: typeof ResolvedPlacement.Type) => (placement.kind === 'pdf' ? { ...placement, assetId: asset.id, page: placement.page.number } : { ...placement, assetId: asset.id })),
    }),
);
const Composition = Schema.Struct({
    id: CompositionId,
    items: Schema.Union([Schema.Tuple([Placement]), Schema.Tuple([Placement, Placement])]),
    scale: Schema.Literals(['full', 'medium', 'small']).pipe(Schema.withConstructorDefault(Effect.succeed('full'))),
    align: Schema.Literals(['left', 'center', 'right']).pipe(Schema.withConstructorDefault(Effect.succeed('center'))),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const placementsFor = (asset: typeof Asset.Type): Array.NonEmptyReadonlyArray<typeof Placement.Type> =>
    Asset.match(asset, {
        pdf: (document) => Array.map(document.sheets, (page) => ({ kind: 'pdf', asset: document, page, caption: '' }) as const),
        video: (video) => [{ kind: 'video', asset: video, caption: '' }] as const,
        image: (image) => [{ kind: 'image', asset: image, caption: '', role: 'drawing' }] as const,
    });
const placementDimensions = (placement: typeof Placement.Type) => (placement.kind === 'pdf' ? placement.page : placement.asset);
const placementRatio = (placement: typeof Placement.Type) => {
    const { width, height } = placement.kind === 'image' && placement.role === 'photograph' && placement.framing.mode === 'cover' ? { width: 4, height: 3 } : placementDimensions(placement);
    return width / height;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composition, CompositionId, Placement, percentBounds, placementDimensions, placementRatio, placementsFor };
