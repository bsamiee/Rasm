import { Array, Effect, Match, Record, Schema, SchemaGetter, SchemaIssue, SchemaTransformation } from 'effect';
import { Asset, AssetId, Assets, defaulted, Sheet } from '#/model/asset.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const focus = { minimum: 0, maximum: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const CompositionId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('CompositionId'));
const reference = <K extends (typeof Asset.Type)['kind']>(kind: K) => {
    const target = Schema.toType(Asset.cases[kind]);
    return AssetId.pipe(
        Schema.decodeTo(target, {
            decode: SchemaGetter.transformEffect((assetId, options) =>
                Assets.use((assets) => Effect.fromOption(Record.get(assets, assetId), () => new SchemaIssue.InvalidValue({ message: `File ${assetId} missing from library` }, assetId, options))).pipe(
                    Effect.filterOrFail(Schema.is(target), (asset) => new SchemaIssue.InvalidValue({ message: `${asset.name} is ${asset.kind}, placement needs ${kind}` }, assetId, options)),
                ),
            ),
            encode: SchemaGetter.transform(({ id }) => id),
        }),
    );
};
const text = { caption: Schema.String, alt: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String) };
const placed = <K extends (typeof Asset.Type)['kind'], F extends Schema.Struct.Fields>(kind: K, fields: F) => Schema.Struct({ ...text, kind: Schema.Literal(kind), asset: reference(kind), ...fields }).pipe(Schema.encodeKeys({ asset: 'assetId' }));
const percentage = Schema.Finite.check(Schema.isBetween(focus));
const placements = Schema.Union([
    placed('image', { role: Schema.Literal('drawing') }),
    placed('image', { role: Schema.Literal('photograph'), framing: Schema.Union([Schema.Struct({ mode: Schema.Literal('contain') }), Schema.Struct({ mode: Schema.Literal('cover'), x: percentage, y: percentage })]) }),
    placed('pdf', { page: Sheet.fields.number }).pipe(
        Schema.decodeTo(Schema.Struct({ ...text, kind: Schema.Literal('pdf'), asset: Schema.toType(Asset.cases.pdf), page: Sheet }), {
            decode: SchemaGetter.transformEffect((placement, options) =>
                Array.get(placement.asset.sheets, placement.page - 1).pipe(
                    Effect.fromOption,
                    Effect.mapBoth({ onFailure: () => new SchemaIssue.InvalidValue({ message: `${placement.asset.name} has ${placement.asset.pages.length} sheets, placement needs sheet ${placement.page}` }, placement, options), onSuccess: (page) => ({ ...placement, page }) }),
                ),
            ),
            encode: SchemaGetter.transform((placement) => ({ ...placement, page: placement.page.number })),
        }),
    ),
    placed('video', {
        poster: Schema.optionalKey(Schema.Struct({ assetId: reference('image') }).pipe(Schema.decodeTo(Schema.toType(Asset.cases.image), SchemaTransformation.transform({ decode: ({ assetId }) => assetId, encode: (assetId) => ({ assetId }) })))),
    }),
]);
const Placement = Object.assign(placements, {
    focus,
    fromAsset: (asset: typeof Asset.Type): Array.NonEmptyReadonlyArray<typeof placements.Type> =>
        Asset.match(asset, {
            pdf: (document) => Array.map(document.sheets, (page) => ({ kind: 'pdf', asset: document, page, caption: '' }) as const),
            video: (video) => [{ kind: 'video', asset: video, caption: '' }] as const,
            image: (image) => [{ kind: 'image', asset: image, caption: '', role: 'drawing' }] as const,
        }),
    ratio: (placement: typeof placements.Type) => {
        const { width, height } = Match.value(placement).pipe(
            Match.when({ kind: 'image', role: 'photograph', framing: { mode: 'cover' } }, () => ({ width: 4, height: 3 })),
            Match.when({ kind: 'pdf' }, ({ page }) => page),
            Match.orElse(({ asset }) => asset),
        );
        return width / height;
    },
});
const Composition = Schema.Struct({
    id: CompositionId,
    items: Schema.Union([Schema.Tuple([Placement]), Schema.Tuple([Placement, Placement])]),
    scale: defaulted(Schema.Literals(['full', 'medium', 'small']), 'full'),
    align: defaulted(Schema.Literals(['left', 'center', 'right']), 'center'),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Composition, CompositionId, Placement };
