import { Effect } from 'effect';
import { HttpApiBuilder, HttpApiSchema } from 'effect/http-api';
import { Api } from '#/model/api';
import { readDocument, writeDocument } from '#/server/store/documents';

// --- [COMPOSITION] ---------------------------------------------------------------------

const content = HttpApiBuilder.group(Api, 'content', (handlers) => {
    const etagged = <A>(body: A) => (revision: number) => HttpApiSchema.withHeaders({ body, headers: { 'draft-etag': revision } });
    return handlers.handleAll({
        draft: () => Effect.map(readDocument('draft'), ({ data, revision }) => etagged(data)(revision)),
        save: ({ payload, headers }) => Effect.map(writeDocument(payload, false, headers['if-match']), etagged(undefined)),
        publish: ({ payload, headers }) => Effect.map(writeDocument(payload, true, headers['if-match']), etagged(undefined)),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { content };
