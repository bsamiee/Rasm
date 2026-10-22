import { type Catalog, literal, table } from '@rasm/database';
import { Option, Schema } from 'effect';
import type { SqlClient, Statement } from 'effect/sql';
import { StoredPortfolio } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const documents = table('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Natural }, { key: ['state'], defaults: { revision: 0 } });
const UploadAttempt = Schema.Struct({ id: Schema.String, discard: Schema.Boolean, objects: Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromOptionalKey(Schema.String) })) });
const assets = table(
    'assets',
    { id: Schema.String, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.OptionFromNullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.NullOr(Schema.fromJsonString(Schema.Array(UploadAttempt))), available: Schema.BooleanFromBit },
    {
        projections: {
            objectKey: { from: 'body', path: ['objectKey'] },
            removing: { from: 'body', path: ['removing'] },
            uploads: { from: 'body', path: ['uploads'] },
            available: { where: { objectKey: { not: null }, removing: { not: [1] } } },
        },
    },
);
const bookkeeping = Object.values(assets.definition.columns)
    .flatMap(({ projection }) => Option.toArray(projection))
    .flatMap((projected) => (projected._tag === 'Path' ? [literal(['$', ...projected.path].join('.'))] : []))
    .join(', ');
const owner = table('owner', { email: Schema.String, userId: Schema.String }, { key: ['email'], withoutRowid: true });
const catalog = { tables: [documents, assets, owner], retired: [] } as const satisfies Catalog;

// --- [OPERATIONS] ----------------------------------------------------------------------

const references = (sql: SqlClient.SqlClient, source: Statement.Fragment) => sql`SELECT d.state, j.value AS asset_id, j.json -> j.path AS placement
    FROM ${source} d, json_tree(d.body) j WHERE j.key = 'assetId' AND j.type = 'text'`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, bookkeeping, catalog, documents, owner, references, UploadAttempt };
