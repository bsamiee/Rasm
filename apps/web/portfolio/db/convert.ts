// biome-ignore lint/correctness/noNodejsModules: Node.js runs storage conversion with its native argument parser
import { parseArgs } from 'node:util';
import type { D1Database } from '@cloudflare/workers-types';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { D1Client } from '@effect/sql-d1';
import { Console, Effect, Match, Record, Schema, Stdio, String } from 'effect';
import { SqlSchema } from 'effect/sql';
import { getPlatformProxy } from 'wrangler';
import hosting from '../.openai/hosting.json' with { type: 'json' };
import { contentEtag } from './digest.ts';
import { schema } from './schema.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ORIGINAL]
const convertOriginal = Effect.fn('convertOriginal')(function* () {
    const sql = yield* D1Client.D1Client;
    const rows = yield* SqlSchema.findAll({
        Request: Schema.Void,
        Result: Schema.Struct({ kind: Schema.Literals(['document', 'asset']), key: Schema.String, body: Schema.String }),
        execute: () => sql`SELECT 'document' AS kind, state AS key, body FROM documents
            UNION ALL SELECT 'asset' AS kind, id AS key, body FROM assets`,
    })(undefined);
    const hashed = yield* Effect.forEach(rows, (row) => contentEtag(row.body).pipe(Effect.map((digest) => ({ ...row, digest }))));
    yield* sql.batch([
        sql`ALTER TABLE documents RENAME TO source_documents`,
        sql`ALTER TABLE assets RENAME TO source_assets`,
        ...schema(sql),
        ...hashed.flatMap((row) =>
            row.kind === 'document'
                ? [sql`INSERT INTO portfolios(digest,body) VALUES (${row.digest},${row.body}) ON CONFLICT(digest) DO NOTHING`, sql`INSERT INTO documents(state,digest) VALUES (${row.key},${row.digest})`]
                : [sql`INSERT INTO assets(digest,body,object_key) VALUES (${row.digest},${row.body},${row.key})`, sql`INSERT INTO upload_attempts(id,asset_id) VALUES (${row.key},${row.key})`, sql`INSERT INTO upload_objects(key,attempt_id) VALUES (${row.key},${row.key})`],
        ),
        sql`DROP TABLE source_documents`,
        sql`DROP TABLE source_assets`,
    ]);
});

// --- [SPLIT]
const convertSplit = Effect.fn('convertSplit')(function* () {
    const sql = yield* D1Client.D1Client;
    yield* sql.batch([
        sql`DROP VIEW portfolio_references`,
        sql`UPDATE portfolios SET body = json_set(body, '$.entries', json((
            SELECT json_group_array(json_set(e.body, '$.id', e.id, '$.compositions', json((
                SELECT json_group_array(json(c.body) ORDER BY c.position)
                FROM compositions c WHERE c.digest = e.digest AND c.entry_id = e.id
            ))) ORDER BY e.position)
            FROM entries e WHERE e.digest = portfolios.digest
        )))`,
        sql`ALTER TABLE assets RENAME TO source_assets`,
        sql`ALTER TABLE upload_attempts RENAME TO source_upload_attempts`,
        sql`ALTER TABLE upload_objects RENAME TO source_upload_objects`,
        sql`DROP INDEX upload_attempts_asset`,
        sql`DROP INDEX upload_objects_attempt`,
        ...schema(sql),
        sql`INSERT INTO assets(digest,body,object_key,removing)
            SELECT digest, json_patch(json_patch(json_set(body, '$.id', id),
                CASE WHEN EXISTS (SELECT 1 FROM pages WHERE asset_id = a.id)
                    THEN json_object('pages', json((SELECT json_group_array(json(body) ORDER BY position) FROM pages WHERE asset_id = a.id)))
                    ELSE '{}' END),
                CASE WHEN EXISTS (SELECT 1 FROM renditions WHERE asset_id = a.id)
                    THEN json_object('renditions', json((SELECT json_group_array(json(body) ORDER BY position) FROM renditions WHERE asset_id = a.id)))
                    ELSE '{}' END),
                object_key, removing FROM source_assets a`,
        sql`INSERT INTO upload_attempts(id,asset_id,discard) SELECT id,asset_id,discard FROM source_upload_attempts`,
        sql`INSERT INTO upload_objects(key,attempt_id,upload_id) SELECT key,attempt_id,upload_id FROM source_upload_objects`,
        sql`DROP TABLE source_upload_objects`,
        sql`DROP TABLE source_upload_attempts`,
        sql`DROP TABLE renditions`,
        sql`DROP TABLE pages`,
        sql`DROP TABLE source_assets`,
        sql`DROP TABLE compositions`,
        sql`DROP TABLE entries`,
    ]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const input = Schema.Struct({ source: Schema.Literals(['original', 'split']), config: Schema.String, persist: Schema.String });
    const args = yield* (yield* Stdio.Stdio).args;
    const { values } = yield* Effect.try(() => parseArgs({ args, options: { ...Record.map(input.fields, () => ({ type: 'string' as const })), help: { type: 'boolean', short: 'h' } } }));
    if (values.help) {
        return yield* Console.log(
            `Usage: ${Record.keys(input.fields)
                .map((name) => `--${name} <value>`)
                .join(' ')}\nsource: ${input.fields.source.literals.join(' | ')}`,
        );
    }
    const { source, config, persist } = yield* Schema.decodeUnknownEffect(input)(values);
    const proxy = yield* Effect.acquireRelease(
        Effect.tryPromise(() => getPlatformProxy<Record.ReadonlyRecord<typeof hosting.d1, D1Database>>({ configPath: config, persist: { path: persist }, remoteBindings: false })),
        (platform) => Effect.promise(() => platform.dispose()),
    );
    const db = yield* Effect.fromNullishOr(proxy.env[hosting.d1]);
    yield* Match.value(source).pipe(
        Match.when('original', () => convertOriginal()),
        Match.when('split', () => convertSplit()),
        Match.exhaustive,
        Effect.provide(D1Client.layer({ db, transformResultNames: String.snakeToCamel })),
    );
    yield* Effect.logInfo('Portfolio storage converted');
}).pipe(Effect.withSpan('convertStorage'), Effect.scoped, Effect.provide(NodeServices.layer), NodeRuntime.runMain);
