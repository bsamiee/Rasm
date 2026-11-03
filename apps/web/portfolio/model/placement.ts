import { Array, Effect, Match, Record, Schema, type SchemaAST, SchemaGetter, SchemaIssue } from 'effect';
import { Asset, AssetId, Assets, ImageAsset, PdfAsset, Sheet, VideoAsset } from '#/model/asset.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const percentBounds = { minimum: 0, maximum: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const CompositionId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('CompositionId'));
const text = { caption: Schema.String, alt: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String) };
const percentage = Schema.Finite.check(Schema.isBetween(percentBounds));
const drawing = Schema.Struct({ ...text, assetId: AssetId, kind: Schema.Literal('image'), role: Schema.Literal('drawing') });
const photograph = Schema.Struct({ ...text, assetId: AssetId, kind: Schema.Literal('image'), role: Schema.Literal('photograph'), framing: Schema.Union([Schema.Struct({ mode: Schema.Literal('contain') }), Schema.Struct({ mode: Schema.Literal('cover'), x: percentage, y: percentage })]) });
const sheet = Schema.Struct({ ...text, assetId: AssetId, kind: Schema.Literal('pdf'), page: Sheet.fields.number });
const resolve = <K extends (typeof Asset.Type)['kind']>(kind: K, assetId: typeof AssetId.Type, input: unknown, options: SchemaAST.ParseOptions) =>
    Assets.use((assets) => Effect.fromOption(Record.get(assets, assetId), () => new SchemaIssue.InvalidValue({ message: `File ${assetId} missing from library` }, input, options))).pipe(
        Effect.filterOrFail(Asset.isAnyOf([kind]), (asset) => new SchemaIssue.InvalidValue({ message: `${asset.name} is ${asset.kind}, placement needs ${kind}` }, input, options)),
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
                        Effect.gen(function* () {
                            const asset = yield* resolve('pdf', assetId, input, options);
                            const page = yield* Effect.fromOption(Array.get(asset.sheets, placement.page - 1), () => new SchemaIssue.InvalidValue({ message: `${asset.name} has ${asset.pages.length} sheets, placement needs sheet ${placement.page}` }, input, options));
                            return { ...placement, asset, page };
                        }),
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
    scale: Schema.Literals(['full', 'medium', 'small']).pipe(Schema.withDecodingDefaultKey(Effect.succeed('full')), Schema.withConstructorDefault(Effect.succeed('full' as const))),
    align: Schema.Literals(['left', 'center', 'right']).pipe(Schema.withDecodingDefaultKey(Effect.succeed('center')), Schema.withConstructorDefault(Effect.succeed('center' as const))),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const placementsFor = (asset: typeof Asset.Type): Array.NonEmptyReadonlyArray<typeof Placement.Type> =>
    Asset.match(asset, {
        pdf: (document) => Array.map(document.sheets, (page) => ({ kind: 'pdf', asset: document, page, caption: '' }) as const),
        video: (video) => [{ kind: 'video', asset: video, caption: '' }] as const,
        image: (image) => [{ kind: 'image', asset: image, caption: '', role: 'drawing' }] as const,
    });
const placementRatio = (placement: typeof Placement.Type) => {
    const { width, height } = Match.value(placement).pipe(
        Match.when({ kind: 'image', role: 'photograph', framing: { mode: 'cover' } }, () => ({ width: 4, height: 3 })),
        Match.when({ kind: 'pdf' }, ({ page }) => page),
        Match.orElse(({ asset }) => asset),
    );
    return width / height;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composition, CompositionId, Placement, percentBounds, placementRatio, placementsFor };
