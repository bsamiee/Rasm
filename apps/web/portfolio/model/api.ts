import { Array, Option, Result, Schema, SchemaTransformation } from 'effect';
import { HttpApi, HttpApiClient, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { type Asset, AssetId, uploadLimit } from '#/model/asset.ts';
import { PortfolioData, StoredPortfolio } from '#/model/portfolio.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const uploadField = { asset: 'asset', renditions: 'renditions', file: 'file' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Recovery = Schema.Struct({ id: AssetId, name: Schema.NonEmptyString, status: Schema.Literals(['incomplete', 'cleanup', 'removing']) });
const Revision = Schema.TemplateLiteralParser(['"', Schema.Natural, '"']).pipe(Schema.decodeTo(Schema.Natural, SchemaTransformation.transform({ decode: ([, revision]) => revision, encode: (revision) => ['"', revision, '"'] as const })));
const draftEtag = { 'draft-etag': Revision };
const Refusal = Schema.TaggedUnion({
    Order: {},
    Metadata: Schema.StandardSchemaV1FailureResult.fields,
    Part: { index: Schema.Int, field: Schema.String, mime: Schema.String },
    Count: { expected: Schema.Int, received: Schema.Int },
    Size: { index: Schema.Int },
    Multipart: { cause: Schema.Literals(['FileTooLarge', 'FieldTooLarge', 'BodyTooLarge', 'TooManyParts', 'InternalError', 'Parse']) },
});

// --- [ERRORS] --------------------------------------------------------------------------

class PreconditionFailed extends Schema.TaggedError<PreconditionFailed, { readonly brand: unique symbol }>()('PreconditionFailed', { revision: Schema.Natural }, { httpApiStatus: 412 }) {
    override get message() {
        return `Stored draft is at revision ${this.revision}`;
    }
}
class PlacementConflict extends Schema.TaggedError<PlacementConflict, { readonly brand: unique symbol }>()('PlacementConflict', { placements: Schema.NonEmptyArray(Schema.Struct({ path: Schema.Array(Schema.Union([Schema.String, Schema.Int])), assetId: AssetId })) }, { httpApiStatus: 409 }) {
    override get message() {
        return `Draft places unavailable files ${this.placements.map(({ assetId }) => assetId).join(', ')}`;
    }
}
class UploadRefused extends Schema.TaggedError<UploadRefused, { readonly brand: unique symbol }>()('UploadRefused', { reason: Refusal }, { httpApiStatus: 400 }) {
    override get message() {
        return Refusal.match(this.reason, {
            Order: () => `First part must be field ${uploadField.asset}`,
            Metadata: ({ issues }) => `Field ${uploadField.asset} refused: ${issues.map(({ message }) => message).join('; ')}`,
            Part: ({ index, field, mime }) => `File part ${index + 1} must be ${field} of ${mime}`,
            Count: ({ expected, received }) => `Upload sent ${received} parts, asset declares ${expected}`,
            Size: ({ index }) => `File part ${index + 1} exceeds its declared size`,
            Multipart: ({ cause }) => `Multipart body refused: ${cause}`,
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Owner extends HttpApiMiddleware.Service<Owner>()('@rasm/portfolio/model/api/Owner', { error: [HttpApiError.ForbiddenNoContent, HttpApiError.ServiceUnavailableNoContent] }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const write = { headers: { 'if-match': Revision }, success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, draftEtag), error: [PreconditionFailed, PlacementConflict] };
const Api = HttpApi.make('portfolio')
    .add(
        HttpApiGroup.make('documents')
            .add(
                HttpApiEndpoint.get('draft', '/draft', { success: HttpApiSchema.WithHeaders(PortfolioData, draftEtag) }),
                HttpApiEndpoint.put('save', '/draft', { payload: StoredPortfolio, ...write }),
                HttpApiEndpoint.put('publish', '/published', {
                    payload: StoredPortfolio.check(Schema.makeFilter((document) => Array.filterMap(document.entries, ({ title }, index) => (title.trim() === '' ? Result.succeed({ path: ['entries', index, 'title'], issue: 'Title required to publish' }) : Result.failVoid)))),
                    ...write,
                }),
            )
            .middleware(Owner),
        HttpApiGroup.make('assets')
            .add(
                HttpApiEndpoint.get('recovery', '/assets/recovery', { success: Schema.Array(Recovery) }),
                HttpApiEndpoint.post('discard', '/assets/:id/discard', { params: { id: AssetId }, error: [HttpApiError.NotFoundNoContent] }),
                HttpApiEndpoint.post('upload', '/assets', {
                    payload: Schema.Unknown.pipe(HttpApiSchema.asMultipartStream({ maxFileSize: uploadLimit })),
                    success: Schema.Struct({ id: AssetId }),
                    error: [UploadRefused, HttpApiError.ConflictNoContent],
                }),
                HttpApiEndpoint.delete('delete', '/assets/:id', { params: { id: AssetId }, error: [HttpApiError.ConflictNoContent] }),
            )
            .middleware(Owner)
            .add(
                HttpApiEndpoint.get('read', '/assets/:id/:width?', {
                    params: { id: AssetId, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString.check(Schema.isInt())) },
                    error: [HttpApiError.NotFoundNoContent, HttpApiError.ServiceUnavailableNoContent],
                }),
            ),
    )
    .prefix('/api');
const urls = HttpApiClient.urlBuilder(Api);
const assetUrl = (asset: typeof Asset.Type, width?: number) => urls.assets.read({ params: { id: asset.id, width: Option.fromUndefinedOr(width) } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Api, assetUrl, Owner, PlacementConflict, PreconditionFailed, Recovery, UploadRefused, uploadField };
