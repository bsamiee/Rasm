import { Effect } from 'effect';
import { HttpApiBuilder, HttpApiSchema } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { readDocument, writeDocument } from './database.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const content = HttpApiBuilder.group(Api, 'content', (handlers) =>
    handlers.handleAll({
        draft: () => readDocument('draft').pipe(Effect.map(({ data, etag }) => HttpApiSchema.withHeaders({ body: data, headers: { 'draft-etag': etag } }))),
        save: ({ payload, headers }) => writeDocument(payload, ['draft'], headers['if-match']).pipe(Effect.map((etag) => HttpApiSchema.withHeaders({ body: undefined, headers: { 'draft-etag': etag } }))),
        publish: ({ payload, headers }) => writeDocument(payload, ['draft', 'published'], headers['if-match']).pipe(Effect.map((etag) => HttpApiSchema.withHeaders({ body: undefined, headers: { 'draft-etag': etag } }))),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { content };
