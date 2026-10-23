import { Schema, SchemaTransformation } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { Id, uploadLimit } from '#/model/asset';
import { PortfolioData, StoredPortfolio } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const PendingUpload = Schema.Struct({ id: Id, name: Schema.NonEmptyString, status: Schema.Literals(['incomplete', 'cleanup', 'removing']) });
const Revision = Schema.TemplateLiteralParser(['"', Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)), '"']).pipe(Schema.decodeTo(Schema.Int, SchemaTransformation.transform({ decode: ([, revision]) => revision, encode: (revision) => ['"', revision, '"'] as const })));

// --- [ERRORS] --------------------------------------------------------------------------

class PreconditionFailed extends Schema.TaggedError<PreconditionFailed, { readonly brand: unique symbol }>()('PreconditionFailed', {}, { httpApiStatus: 412 }) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Owner extends HttpApiMiddleware.Service<Owner>()('@rasm/portfolio/model/api/Owner', { error: [HttpApiError.ForbiddenNoContent, HttpApiError.ServiceUnavailableNoContent] }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const write = { headers: { 'if-match': Revision }, success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, { 'draft-etag': Revision }), error: [PreconditionFailed, HttpApiError.ConflictNoContent] };
const Api = HttpApi.make('portfolio')
    .add(
        HttpApiGroup.make('content').add(
            HttpApiEndpoint.get('draft', '/portfolio', { success: HttpApiSchema.WithHeaders(PortfolioData, { 'draft-etag': Revision }) }),
            HttpApiEndpoint.put('save', '/portfolio', { payload: StoredPortfolio, ...write }),
            HttpApiEndpoint.post('publish', '/publish', { payload: StoredPortfolio.check(Schema.makeFilter((document) => document.entries.flatMap(({ title }, index) => (title.trim() === '' ? [{ path: ['entries', index, 'title'], issue: 'Title required to publish' }] : [])))), ...write }),
        ),
        HttpApiGroup.make('media').add(
            HttpApiEndpoint.get('pending', '/media/pending', { success: Schema.Array(PendingUpload) }),
            HttpApiEndpoint.post('discard', '/media/:id/discard', { params: { id: Id }, error: [HttpApiError.ConflictNoContent] }),
            HttpApiEndpoint.post('upload', '/media', {
                payload: Schema.Unknown.pipe(HttpApiSchema.asMultipartStream({ maxFileSize: uploadLimit })),
                success: Schema.Struct({ id: Id }),
                error: [HttpApiError.BadRequestNoContent, HttpApiError.ConflictNoContent],
            }),
            HttpApiEndpoint.delete('delete', '/media/:id', { params: { id: Id }, error: [HttpApiError.ConflictNoContent] }),
        ),
    )
    .prefix('/api')
    .middleware(Owner);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Api, Owner, PendingUpload, PreconditionFailed };
