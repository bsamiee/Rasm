import { Array, Effect, identity, Match, Option, Record, Result, Schema, type SchemaAST, SchemaGetter, SchemaIssue, SchemaTransformation, String } from 'effect';
import { Asset, AssetId, Assets, Sheet } from '#/model/asset';
import { defaulted } from '#/model/defaulted';

// --- [CONSTANTS] -----------------------------------------------------------------------

const focus = { minimum: 0, maximum: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const CompositionId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('CompositionId'));
const reference = <K extends (typeof Asset.Type)['kind']>(kind: K) => {
    const target = Schema.toType(Asset.cases[kind]);
    return AssetId.pipe(
        Schema.decodeTo(target, {
            decode: SchemaGetter.transformEffect(
                Effect.fnUntraced(function* (assetId: typeof AssetId.Type, options: SchemaAST.ParseOptions) {
                    const asset = yield* Effect.fromOption(Record.get(yield* Assets, assetId), () => new SchemaIssue.InvalidValue({ message: `File ${assetId} missing from library` }, assetId, options));
                    return yield* Effect.fromResult(Result.liftPredicate(asset, Asset.isAnyOf([kind]), (other) => new SchemaIssue.InvalidValue({ message: `${other.name} is ${other.kind}, placement needs ${kind}` }, assetId, options)));
                }),
            ),
            encode: SchemaGetter.transform(({ id }) => id),
        }),
    );
};
const Prose = Schema.optionalKey(Schema.String).pipe(Schema.decodeTo(Schema.optionalKey(Schema.Trimmed.check(Schema.isNonEmpty())), SchemaTransformation.transformOptional({ decode: (given) => Option.filter(Option.map(given, String.trim), String.isNonEmpty), encode: identity })));
const text = { caption: Prose, alt: Prose, description: Prose };
const placed = <K extends (typeof Asset.Type)['kind'], F extends Schema.Struct.Fields>(kind: K, fields: F) => Schema.Struct({ ...text, kind: Schema.Literal(kind), asset: reference(kind), ...fields }).pipe(Schema.encodeKeys({ asset: 'assetId' }));
const percentage = Schema.Finite.check(Schema.isBetween(focus));
const placements = Schema.Union([
    placed('image', { role: Schema.Literal('drawing') }),
    placed('image', { role: Schema.Literal('photograph'), framing: Schema.Union([Schema.Struct({ mode: Schema.Literal('contain') }), Schema.Struct({ mode: Schema.Literal('cover'), x: percentage, y: percentage })]) }),
    placed('pdf', { page: Sheet.fields.number }).pipe(
        Schema.decodeTo(Schema.Struct({ ...text, kind: Schema.Literal('pdf'), asset: Schema.toType(Asset.cases.pdf), page: Sheet }), {
            decode: SchemaGetter.transformEffect((placement, options) =>
                Array.get(placement.asset.sheets, placement.page - 1).pipe(
                    Effect.fromOption(() => new SchemaIssue.InvalidValue({ message: `${placement.asset.name} sheet ${placement.page} is outside 1–${placement.asset.pages.length}` }, placement, options)),
                    Effect.map((page) => ({ ...placement, page })),
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
            pdf: (document) => Array.map(document.sheets, (page) => ({ kind: 'pdf', asset: document, page }) as const),
            video: (video) => [{ kind: 'video', asset: video }] as const,
            image: (image) => [{ kind: 'image', asset: image, role: 'drawing' }] as const,
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
const Scale = Schema.Literals(['full', 'medium', 'small']);
const Alignment = Schema.Literals(['left', 'center', 'right']);
const Composition = Schema.Struct({
    id: CompositionId,
    items: Schema.Union([Schema.Tuple([Placement]), Schema.Tuple([Placement, Placement])]),
    scale: defaulted(Scale, 'full'),
    align: defaulted(Alignment, 'center'),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Alignment, Composition, CompositionId, Placement, Scale };
