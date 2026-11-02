import { Catalog } from '@rasm/database/catalog';
import { Column } from '@rasm/database/column';
import { Relation } from '@rasm/database/relation';
import { Option, Schema } from 'effect';
import { type SqlClient, Statement } from 'effect/sql';
import { StoredPortfolio } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const documents = Relation.make('documents', { state: Schema.Literals(['draft', 'published']), body: Schema.fromJsonString(StoredPortfolio), revision: Schema.Natural }, { key: ['state'], defaults: { revision: 0 } });
const UploadAttempt = Schema.Struct({ id: Schema.String, discard: Schema.Boolean, objects: Schema.Array(Schema.Struct({ key: Schema.String, uploadId: Schema.OptionFromOptionalKey(Schema.String) })) });
const assets = Relation.make(
    'assets',
    { id: Schema.String, body: Schema.fromJsonString(Schema.Unknown), objectKey: Schema.OptionFromNullOr(Schema.String), removing: Schema.NullOr(Schema.BooleanFromBit), uploads: Schema.OptionFromNullOr(Schema.fromJsonString(Schema.Array(UploadAttempt))), available: Schema.BooleanFromBit },
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
const catalog = new Catalog({ tables: [documents, assets, owner], retired: ['reconcile_revision'], excluded: ['_cf_'] });
const bookkeeping = Statement.csv(
    Object.values(assets.columns)
        .flatMap(({ projection }) => Option.toArray(Option.filter(projection, Column.Projection.$is('Path'))))
        .map(({ path }) => Statement.fragment([Statement.parameter(['$', ...path].join('.'))])),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const references = (sql: SqlClient.SqlClient, source: Statement.Fragment = sql`${sql(documents.name)}`) => sql`SELECT d.state, j.value AS asset_id, j.json -> j.path AS placement
    FROM ${source} d, json_tree(d.body) j WHERE j.key = 'assetId' AND j.type = 'text'`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { assets, bookkeeping, catalog, documents, owner, references, UploadAttempt };
