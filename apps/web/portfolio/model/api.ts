import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { Id, uploadLimit } from './asset.ts';
import { Portfolio, PortfolioData, StoredPortfolio } from './document.ts';

// --- [MODELS] --------------------------------------------------------------------------

const PendingUpload = Schema.Struct({ id: Id, name: Schema.NonEmptyString, status: Schema.Literals(['incomplete', 'cleanup', 'removing']) });

// --- [ERRORS] --------------------------------------------------------------------------

class PreconditionFailed extends Schema.TaggedError<PreconditionFailed>()('PreconditionFailed', {}, { httpApiStatus: 412 }) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Owner extends HttpApiMiddleware.Service<Owner>()('portfolio/Owner', { error: [HttpApiError.ForbiddenNoContent, HttpApiError.ServiceUnavailableNoContent] }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

// biome-ignore lint/nursery/useExplicitReturnType: HttpApi infers endpoint types from schemas; a return annotation duplicates that graph or erases client payload types
const makeApi = <P extends typeof Portfolio | typeof StoredPortfolio>(portfolio: P) =>
    HttpApi.make('portfolio')
        .add(
            HttpApiGroup.make('content').add(
                HttpApiEndpoint.get('draft', '/portfolio', { success: HttpApiSchema.WithHeaders(PortfolioData, { 'draft-etag': Schema.String }), error: HttpApiError.ServiceUnavailableNoContent }),
                HttpApiEndpoint.put('save', '/portfolio', {
                    payload: portfolio,
                    headers: { 'if-match': Schema.NonEmptyString },
                    success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, { 'draft-etag': Schema.String }),
                    error: [PreconditionFailed, HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent],
                }),
                HttpApiEndpoint.post('publish', '/publish', {
                    payload: Schema.check<P>(Schema.makeFilter((document) => document.entries.flatMap((entry, index) => (entry.title.trim() ? [] : [{ path: ['entries', index, 'title'], issue: 'Give this entry a title before publishing' }]))))(portfolio),
                    headers: { 'if-match': Schema.NonEmptyString },
                    success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, { 'draft-etag': Schema.String }),
                    error: [PreconditionFailed, HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent],
                }),
            ),
            HttpApiGroup.make('media').add(
                HttpApiEndpoint.get('pending', '/media/pending', { success: Schema.Array(PendingUpload), error: HttpApiError.ServiceUnavailableNoContent }),
                HttpApiEndpoint.post('discard', '/media/:id/discard', { params: { id: Id }, error: [HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent] }),
                HttpApiEndpoint.post('upload', '/media', {
                    payload: Schema.Unknown.pipe(HttpApiSchema.asMultipartStream({ maxFileSize: uploadLimit })),
                    success: Schema.Struct({ id: Id }),
                    error: [HttpApiError.BadRequestNoContent, HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent],
                }),
                HttpApiEndpoint.delete('delete', '/media/:id', { params: { id: Id }, error: [HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent] }),
            ),
        )
        .prefix('/api')
        .middleware(Owner);

const Api = makeApi(StoredPortfolio);
const ClientApi = makeApi(Portfolio);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Api, ClientApi, Owner, PendingUpload, PreconditionFailed };
