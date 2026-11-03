import { Array, Result, Schema, SchemaTransformation } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { Id, uploadLimit } from '#/model/asset';
import { PortfolioData, StoredPortfolio } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const PendingUpload = Schema.Struct({ id: Id, name: Schema.NonEmptyString, status: Schema.Literals(['incomplete', 'cleanup', 'removing']) });
const Revision = Schema.TemplateLiteralParser(['"', Schema.Natural, '"']).pipe(Schema.decodeTo(Schema.Natural, SchemaTransformation.transform({ decode: ([, revision]) => revision, encode: (revision) => ['"', revision, '"'] as const })));
const draftEtag = { 'draft-etag': Revision };

// --- [ERRORS] --------------------------------------------------------------------------

class PreconditionFailed extends Schema.TaggedError<PreconditionFailed, { readonly brand: unique symbol }>()('PreconditionFailed', {}, { httpApiStatus: 412 }) {}

// --- [SERVICES] ------------------------------------------------------------------------

class Owner extends HttpApiMiddleware.Service<Owner>()('@rasm/portfolio/model/api/Owner', { error: [HttpApiError.ForbiddenNoContent, HttpApiError.ServiceUnavailableNoContent] }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const write = { headers: { 'if-match': Revision }, success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, draftEtag), error: [PreconditionFailed, HttpApiError.ConflictNoContent] };
const Api = HttpApi.make('portfolio')
    .add(
        HttpApiGroup.make('content').add(
            HttpApiEndpoint.get('draft', '/portfolio', { success: HttpApiSchema.WithHeaders(PortfolioData, draftEtag) }),
            HttpApiEndpoint.put('save', '/portfolio', { payload: StoredPortfolio, ...write }),
            HttpApiEndpoint.post('publish', '/publish', {
                payload: StoredPortfolio.check(Schema.makeFilter((document) => Array.filterMap(document.entries, ({ title }, index) => (title.trim() === '' ? Result.succeed({ path: ['entries', index, 'title'], issue: 'Title required to publish' }) : Result.failVoid)))),
                ...write,
            }),
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
