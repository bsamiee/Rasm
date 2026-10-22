import { atomic } from '@rasm/database';
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const readDocument = Effect.fn('readDocument')(
    (state: typeof documents.fields.state.Type) =>
        Effect.flatten(
            Effect.zipWith(
                SqlClient.SqlClient,
                Schema.encodeEffect(Schema.fromJsonString(Portfolio))(Portfolio.make({})),
                (sql, empty) => sql`SELECT COALESCE(d.revision, 0) AS revision, json_object('portfolio', json(COALESCE(d.body, ${empty})), 'assets', (
                    SELECT json_group_object(a.id, json_set(json_remove(a.body, ${sql.literal(bookkeeping)}), '$.id', a.id)) FROM assets a
                    WHERE a.available AND (s.state = 'draft' OR a.id IN (SELECT asset_id FROM (${references(sql, sql.literal('documents'))}) WHERE state = s.state))
                )) AS data
                FROM (SELECT ${state} AS state) s LEFT JOIN documents d ON d.state = s.state`,
            ),
        ),
    Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ revision: documents.fields.revision, data: Schema.fromJsonString(PortfolioData) })]))),
    Effect.map(([row]) => row),
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);
const writeDocument = Effect.fn('writeDocument')(
    function* (portfolio: typeof StoredPortfolio.Type, publish: boolean, revision: number) {
        const [sql, body] = yield* Effect.all([SqlClient.SqlClient, Schema.encodeEffect(documents.fields.body)(portfolio)]);
        const save = sql`INSERT INTO documents (state, body, revision) SELECT 'draft', n.body, ${revision + 1} FROM (SELECT ${body} AS body) n WHERE NOT EXISTS (
                SELECT 1 FROM (${references(sql, sql`(SELECT 'draft' AS state, n.body AS body)`)}) r
                LEFT JOIN assets a ON a.id = r.asset_id AND a.available AND (a.body ->> '$.mime') IN (SELECT value FROM json_each(${mimes}, '$.' || coalesce(r.placement ->> '$.kind', 'image')))
                    AND (r.placement ->> '$.page' IS NULL OR r.placement ->> '$.page' <= json_array_length(a.body, '$.pages'))
                WHERE a.id IS NULL
            )
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE documents.revision = ${revision} AND documents.body IS DISTINCT FROM excluded.body
            RETURNING revision`;
        const state = sql`SELECT COALESCE(MAX(revision), 0) AS revision, COALESCE(MAX(body = ${body}), 0) AS unchanged FROM documents WHERE state = 'draft'`;
        const published = sql`INSERT INTO documents (state, body, revision) SELECT 'published', body, revision FROM documents WHERE state = 'draft' AND body = ${body} AND revision IN (${revision}, ${revision + 1})
            ON CONFLICT (state) DO UPDATE SET body = excluded.body, revision = excluded.revision
            WHERE (documents.body, documents.revision) IS DISTINCT FROM (excluded.body, excluded.revision)`;
        const [saved, [current]] = yield* Effect.flatMap(
            atomic([save, state, ...(publish ? [published] : [])]),
            Schema.decodeUnknownEffect(Schema.TupleWithRest(Schema.Tuple([Schema.Array(Schema.Unknown), Schema.Tuple([Schema.Struct({ revision: documents.fields.revision, unchanged: Schema.BooleanFromBit })])]), [Schema.Unknown])),
        );
        return yield* Match.value({ written: Array.isReadonlyArrayNonEmpty(saved), ...current }).pipe(
            Match.whenOr({ written: true }, { revision, unchanged: true }, ({ revision: stored }) => Effect.succeed(stored)),
            Match.when({ revision }, () => Effect.fail(HttpApiError.Conflict.make())),
            Match.orElse(() => Effect.fail(PreconditionFailed.make())),
        );
    },
    Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { readDocument, writeDocument };
