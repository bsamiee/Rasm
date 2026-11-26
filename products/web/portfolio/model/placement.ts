import { Array, Effect, Match, Option, Record, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, String } from 'effect';
import { Asset, AssetId, Assets, Sheet } from '#/model/asset';

// --- [CONSTANTS] -----------------------------------------------------------------------

const focus = { minimum: 0, maximum: 100 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const defaulted = <S extends Schema.Top & Schema.WithoutConstructorDefault>(schema: S, value: S['Type'] & S['Encoded']) => schema.pipe(Schema.withDecodingDefaultKey(Effect.succeed(value)), Schema.withConstructorDefault(Effect.succeed(value)));
const reference = <K extends Assets['Service'][keyof Assets['Service']]['kind']>(kind: K) =>
    AssetId.pipe(
        Schema.decodeTo(Schema.toType(Asset.cases[kind]), {
            decode: SchemaGetter.transformEffect((assetId, options) =>
                Assets.pipe(
                    Effect.flatMap((assets) => Effect.fromOption(Record.get(assets, assetId), () => new SchemaIssue.InvalidValue({ message: `File ${assetId} missing from library` }, assetId, options))),
                    Effect.filterOrFail(Asset.isAnyOf([kind]), (other) => new SchemaIssue.InvalidValue({ message: `${other.name} is ${other.kind}, placement needs ${kind}` }, assetId, options)),
                ),
            ),
            encode: SchemaGetter.transform(({ id }) => id),
        }),
    );
const stated = (given: Option.Option<string>) => Option.filter(Option.map(given, String.trim), String.isNonEmpty);
const prose = <S extends Schema.Codec<string, string>>(kept: S) => Schema.optionalKey(Schema.String).pipe(Schema.decodeTo(Schema.optionalKey(kept), SchemaTransformation.transformOptional({ decode: stated, encode: stated })));
const Prose = Object.assign(prose(Schema.NonEmptyString), { of: prose });
const text = { caption: Prose, alt: Prose, description: Prose };
const placed = <K extends Assets['Service'][keyof Assets['Service']]['kind'], F extends Schema.Struct.Fields>(kind: K, fields: F) => Schema.Struct({ ...text, kind: Schema.Literal(kind), asset: reference(kind), ...fields }).pipe(Schema.encodeKeys({ asset: 'assetId' }));
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
const Scale = Schema.Literals(['full', 'medium', 'small']);
const Alignment = Schema.Literals(['left', 'center', 'right']);
class Composition extends Schema.Opaque<Composition>()(
    Schema.Struct({
        id: Schema.String.check(Schema.isUUID()).pipe(Schema.brand('CompositionId')),
        items: Schema.Union([Schema.Tuple([placements]), Schema.Tuple([placements, placements])]),
        scale: defaulted(Scale, 'full'),
        align: defaulted(Alignment, 'center'),
    }),
) {}
const Placement = Object.assign(placements, {
    focus,
    fromAsset: (asset: Composition['items'][number]['asset']): Array.NonEmptyReadonlyArray<Composition['items'][number]> =>
        Asset.match(asset, {
            pdf: (document) => Array.map(document.sheets, (page) => ({ kind: 'pdf', asset: document, page }) as const),
            video: (video) => [{ kind: 'video', asset: video }] as const,
            image: (image) => [{ kind: 'image', asset: image, role: 'drawing' }] as const,
        }),
    ratio: (placement: Composition['items'][number]) => {
        const { width, height } = Match.value(placement).pipe(
            Match.when({ kind: 'image', role: 'photograph', framing: { mode: 'cover' } }, () => ({ width: 4, height: 3 })),
            Match.when({ kind: 'pdf' }, ({ page }) => page),
            Match.orElse(({ asset }) => asset),
        );
        return width / height;
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Alignment, Composition, defaulted, Placement, Prose, Scale };
