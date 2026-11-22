import { Enrollment } from '@rasm/hosting/identity';
import { Presignable, Presigned } from '@rasm/object-storage';
import { Array, Option, Result, Schema, SchemaTransformation } from 'effect';
import { HttpApi, HttpApiClient, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { Asset, AssetId } from '#/model/asset.ts';
import { PortfolioData, StoredPortfolio } from '#/model/portfolio.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Recovery = Schema.Struct({ id: AssetId, name: Schema.NonEmptyString, status: Schema.Literals(['incomplete', 'cleanup', 'removing']) });
const Revision = Schema.TemplateLiteralParser(['"', Schema.Natural, '"']).pipe(Schema.decodeTo(Schema.Natural, SchemaTransformation.transform({ decode: ([, revision]) => revision, encode: (revision) => ['"', revision, '"'] as const })));
const draftEtag = { 'draft-etag': Revision };
const Bootstrap = Schema.Struct({ published: PortfolioData, session: Enrollment.Session });
const Upload = Schema.Struct({ attempt: Schema.String, objects: Schema.Array(Schema.Struct({ key: Schema.String, width: Schema.optionalKey(Schema.Int) })) });
const Refusal = Schema.TaggedUnion({ Unknown: { key: Schema.String }, Size: { key: Schema.String }, Missing: { keys: Schema.NonEmptyArray(Schema.String) } });

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
            Unknown: ({ key }) => `Object ${key} belongs to no open upload`,
            Size: ({ key }) => `Object ${key} differs from its declared size or type`,
            Missing: ({ keys }) => `Objects ${keys.join(', ')} are not uploaded`,
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
                    payload: StoredPortfolio.check(Schema.makeFilter(({ entries = [] }) => Array.filterMap(entries, ({ title }, index) => (title === undefined || title.trim() === '' ? Result.succeed({ path: ['entries', index, 'title'], issue: 'Title required to publish' }) : Result.failVoid)))),
                    ...write,
                }),
            )
            .middleware(Owner),
        HttpApiGroup.make('assets')
            .add(
                HttpApiEndpoint.get('recovery', '/assets/recovery', { success: Schema.Array(Recovery) }),
                HttpApiEndpoint.post('discard', '/assets/:id/discard', { params: { id: AssetId }, error: [HttpApiError.NotFoundNoContent] }),
                HttpApiEndpoint.post('begin', '/assets', { payload: Asset, success: Upload, error: [HttpApiError.ConflictNoContent] }),
                HttpApiEndpoint.post('sign', '/assets/signatures', { payload: Presignable, success: Presigned, error: [UploadRefused] }),
                HttpApiEndpoint.post('complete', '/assets/:id/uploads/:attempt', { params: { id: AssetId, attempt: Schema.String }, error: [UploadRefused, HttpApiError.ConflictNoContent] }),
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

export { Api, assetUrl, Bootstrap, Owner, PlacementConflict, PreconditionFailed, Recovery, Upload, UploadRefused };
