import { atomic } from '@rasm/database/transaction';
import { unavailable } from '@rasm/hosting';
import { Array, Effect, Match, Record, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';
import { SqlClient } from 'effect/sql';
import { PreconditionFailed } from '#/model/api';
import { Placement } from '#/model/placement';
import { Portfolio, PortfolioData, type StoredPortfolio } from '#/model/portfolio';
import { bookkeeping, documents, references } from '#/server/store/catalog';

// --- [CONSTANTS] -----------------------------------------------------------------------

const mimes = JSON.stringify(Record.fromIterableWith(Placement.to.members, ({ fields }) => [fields.kind.literal, fields.asset.schema.fields.mime.literals]));

// --- [MODELS] --------------------------------------------------------------------------

const Draft = Schema.Tuple([Schema.Struct({ revision: documents.schema.fields.revision, stored: Schema.BooleanFromBit })]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const empty = Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({}));
const readDocument = Effect.fn('readDocument')(
    (state: typeof documents.schema.fields.state.Type) =>
        Effect.flatten(
            Effect.zipWith(
                SqlClient.SqlClient,
                empty,
                (sql, body) => sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${body})), 'assets', (
                    SELECT json_group_object(a.id, json_set(json_remove(a.body, ${bookkeeping}), '$.id', a.id)) FROM assets a
                    WHERE a.available AND (s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${references(sql)}) WHERE state = s.state))
                )) AS data
                FROM (SELECT ${state} AS state) s LEFT JOIN documents d ON d.state = s.state`,
            ),
        ),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ revision: documents.schema.fields.revision, data: Schema.fromJsonString(PortfolioData) })]))),
    Effect.map(([row]) => row),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, revision: number) {
        const [sql, body] = yield* Effect.all([SqlClient.SqlClient, Schema.encodeEffect(documents.schema.fields.body)(portfolio)]);
        const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', n.body, ${revision + 1} FROM (SELECT ${body} AS body) n LEFT JOIN documents d ON d.state = 'draft'
            WHERE COALESCE(d.revision, 0) = ${revision} AND NOT EXISTS (
                SELECT 1 FROM (${references(sql, sql`(SELECT 'draft' AS state, n.body AS body)`)}) r
                LEFT JOIN assets a ON a.id = r.asset_id AND a.available AND (a.body ->> '$.mime') IN (SELECT value FROM json_each(${mimes}, '$.' || coalesce(r.placement ->> '$.kind', 'image')))
                    AND (r.placement ->> '$.page' IS NULL OR r.placement ->> '$.page' <= json_array_length(a.body, '$.pages'))
                WHERE a.id IS NULL
            )
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision WHERE documents.body IS DISTINCT FROM excluded.body`;
        const published = sql`INSERT INTO documents (state, body, revision) SELECT 'published', body, revision FROM documents WHERE state = 'draft' AND body = ${body}
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE (documents.body, documents.revision) IS DISTINCT FROM (excluded.body, excluded.revision)`;
        const current = sql`SELECT COALESCE(MAX(revision), 0) AS revision, COALESCE(MAX(body = ${body}), 0) AS stored FROM documents WHERE state = 'draft'`;
        const [draft] = yield* atomic([save, ...(publish ? [published] : []), current]).pipe(Effect.map(Array.lastNonEmpty), Effect.flatMap(Schema.decodeUnknownEffect(Draft)));
        return yield* Match.value(draft).pipe(
            Match.when({ stored: true }, ({ revision: stored }) => Effect.succeed(stored)),
            Match.when({ revision }, () => Effect.fail(HttpApiError.Conflict.make())),
            Match.orElse(() => Effect.fail(PreconditionFailed.make())),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { readDocument, writeDocument };
