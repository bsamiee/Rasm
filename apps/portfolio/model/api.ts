import { Schema } from 'effect';
import { HttpApi, HttpApiEndpoint, HttpApiError, HttpApiGroup, HttpApiMiddleware, HttpApiSchema } from 'effect/http-api';
import { Id, uploadLimit } from './asset.ts';
import { PortfolioData, StoredPortfolio } from './document.ts';

// --- [MODELS] --------------------------------------------------------------------------

const PublishedPortfolio = StoredPortfolio.check(Schema.makeFilter((portfolio) => portfolio.entries.flatMap((entry, index) => (entry.title.trim() ? [] : [{ path: ['entries', index, 'title'], issue: 'Give this entry a title before publishing' }]))));

// --- [SERVICES] ------------------------------------------------------------------------

class Owner extends HttpApiMiddleware.Service<Owner>()('portfolio/Owner', { error: [HttpApiError.ForbiddenNoContent, HttpApiError.ServiceUnavailableNoContent] }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Api = HttpApi.make('portfolio')
    .add(
        HttpApiGroup.make('content').add(
            HttpApiEndpoint.get('draft', '/portfolio', { success: PortfolioData, error: HttpApiError.ServiceUnavailableNoContent }),
            HttpApiEndpoint.put('save', '/portfolio', { payload: StoredPortfolio, error: [HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent] }),
            HttpApiEndpoint.post('publish', '/publish', { payload: PublishedPortfolio, error: [HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent] }),
        ),
        HttpApiGroup.make('media').add(
            HttpApiEndpoint.post('upload', '/media', {
                payload: Schema.Unknown.pipe(HttpApiSchema.asMultipartStream({ maxParts: 2, maxFileSize: uploadLimit })),
                success: Schema.Struct({ id: Id }),
                error: [HttpApiError.BadRequestNoContent, HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent],
            }),
            HttpApiEndpoint.delete('delete', '/media/:id', { params: { id: Id }, error: [HttpApiError.ConflictNoContent, HttpApiError.ServiceUnavailableNoContent] }),
        ),
    )
    .prefix('/api')
    .middleware(Owner);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Api, Owner };
