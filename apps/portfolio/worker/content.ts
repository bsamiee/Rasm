import { HttpApiBuilder } from 'effect/http-api';
import { Api } from '../model/api.ts';
import { readDocument, writeDocument } from './database.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const content = HttpApiBuilder.group(Api, 'content', (handlers) =>
    handlers.handleAll({
        draft: () => readDocument('draft'),
        save: ({ payload }) => writeDocument(payload, ['draft']),
        publish: ({ payload }) => writeDocument(payload, ['draft', 'published']),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { content };
