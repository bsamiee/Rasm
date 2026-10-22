import { Effect } from 'effect';
import { HttpApiBuilder, HttpApiSchema } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { readDocument, writeDocument } from './store/documents.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const content = HttpApiBuilder.group(Api, 'content', (handlers) => {
    const withEtag = Effect.map((revision: number) => HttpApiSchema.withHeaders({ body: undefined, headers: { 'draft-etag': revision } }));
    return handlers.handleAll({
        draft: () => readDocument('draft').pipe(Effect.map(({ data, revision }) => HttpApiSchema.withHeaders({ body: data, headers: { 'draft-etag': revision } }))),
        save: ({ payload, headers }) => withEtag(writeDocument(payload, false, headers['if-match'])),
        publish: ({ payload, headers }) => withEtag(writeDocument(payload, true, headers['if-match'])),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { content };
