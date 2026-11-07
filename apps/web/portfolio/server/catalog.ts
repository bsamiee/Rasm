import { Catalog, Relation } from '@rasm/database';
import { Schema } from 'effect';
import { type SqlClient, Statement } from 'effect/sql';
import { AssetId } from '#/model/asset.ts';
import { StoredPortfolio } from '#/model/portfolio.ts';

// --- [MODELS] --------------------------------------------------------------------------

const documents = Relation.make('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Natural }, { key: ['state'], defaults: { revision: 0 } });
const UploadAttempt = Schema.Struct({ id: Schema.String, discard: Schema.Boolean, objects: Schema.Array(Schema.Struct({ key: Schema.String, width: Schema.optionalKey(Schema.Int), uploadId: Schema.OptionFromOptionalKey(Schema.String) })) });
const assets = Relation.make(
    'assets',
    { id: AssetId, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.OptionFromNullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Array(UploadAttempt))), available: Schema.BooleanFromBit },
    {
        projections: {
            objectKey: { from: 'body', path: ['objectKey'] },
            removing: { from: 'body', path: ['removing'] },
            uploads: { from: 'body', path: ['uploads'] },
            available: { where: { objectKey: { not: null }, removing: { not: [1] } } },
        },
    },
);
const owner = Relation.make('owner', { email: Schema.String, userId: Schema.String }, { key: ['email'], withoutRowid: true });
const catalog = Catalog.make({ tables: [documents, assets, owner], retired: ['reconcile_revision'] });
const projectedPaths = Statement.csv(assets.paths.map((path) => Statement.fragment([Statement.parameter(path)])));

// --- [OPERATIONS] ----------------------------------------------------------------------

const references = (sql: SqlClient.SqlClient, source: Statement.Fragment = sql`${sql(documents.name)}`) => sql`SELECT d.state, j.value AS asset_id, j.fullkey, j.json -> j.path AS placement
    FROM ${source} d, json_tree(d.body) j WHERE j.key = 'assetId' AND j.type = 'text'`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, catalog, documents, owner, projectedPaths, references, UploadAttempt };
