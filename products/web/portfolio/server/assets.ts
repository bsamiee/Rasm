import { Rows } from '@rasm/database/engine';
import { ObjectStorage } from '@rasm/object-storage/storage';
import { Array, ByteSize, Cause, Duration, Effect, Function, Option, Record, Schema, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { SqlClient, type SqlError, SqlSchema, type Statement } from 'effect/sql';
import { Api, Recovery, UploadRefused } from '#/model/api';
import { Asset, type Assets } from '#/model/asset';
import { assets, projectedPaths, references, UploadAttempt, unavailable } from '#/server/catalog';
import { OwnerAccount } from '#/server/owner';

// --- [MODELS] --------------------------------------------------------------------------

class Release extends Schema.Opaque<Release>()(Schema.Struct({ assetId: assets.schema.fields.id, action: Schema.Literals(['discard', 'delete']) })) {}
const AssetRead = Schema.Struct({ id: assets.schema.fields.id, width: Schema.OptionFromNullOr(Schema.Int) });
const AssetObject = Schema.Struct({ mime: Schema.String, name: Schema.OptionFromNullOr(Schema.String), published: Schema.BooleanFromBit, objectKey: Schema.String });
const Released = Schema.Struct({ objects: Schema.fromJsonString(UploadAttempt.fields.objects) });
const Declared = Schema.Struct({ key: Schema.NullOr(Schema.String), attempt: Schema.NullOr(Schema.String) });
const DeclaredObject = Schema.Struct({ assetId: assets.schema.fields.id, attempt: Schema.String, key: Schema.String, size: Schema.Int, mime: Schema.String, uploadId: Schema.OptionFromNullOr(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]

const readAsset = Effect.fn('readAsset')(
    Function.flow(
        SqlSchema.findOne({
            Request: AssetRead,
            Result: AssetObject,
            execute: ({ id, width }) =>
                SqlClient.SqlClient.use(
                    (sql) => sql`SELECT COALESCE(r.value ->> '$.mime', a.body ->> '$.mime') AS mime, CASE WHEN r.value IS NULL THEN a.body ->> '$.name' END AS name, o.value ->> '$.key' AS ${sql('objectKey')},
                    EXISTS (SELECT 1 FROM (${references(sql)}) WHERE state = 'published' AND asset_id = a.id) AS published
                    FROM (SELECT ${id} AS id, ${width} AS width) q JOIN assets a ON a.id = q.id
                    JOIN json_each(a.uploads) u ON u.value ->> '$.id' = a.${sql('objectKey')} JOIN json_each(u.value, '$.objects') o ON o.value ->> '$.width' IS NOT DISTINCT FROM q.width
                    LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = q.width
                    WHERE a.available`,
                ),
        }),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => HttpApiError.NotFound.make()),
    ),
);
const readRecovery = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Recovery,
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT id, body ->> '$.name' AS name,
            CASE WHEN removing IS TRUE THEN 'removing' WHEN ${sql('objectKey')} IS NULL THEN 'incomplete' ELSE 'cleanup' END AS status
            FROM assets WHERE NOT available OR EXISTS (SELECT 1 FROM json_each(uploads) WHERE value ->> '$.id' <> ${sql('objectKey')})`,
        ),
})(undefined).pipe(Effect.catchTag(['SqlError', 'SchemaError'], unavailable), Effect.withSpan('readRecovery'));

// --- [UPLOADS]

const declared = SqlSchema.findAll({
    Request: Declared,
    Result: DeclaredObject,
    execute: ({ key, attempt }) =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT a.id AS ${sql('assetId')}, u.value ->> '$.id' AS attempt, o.value ->> '$.key' AS key, COALESCE(r.value ->> '$.size', a.body ->> '$.size') AS size,
            COALESCE(r.value ->> '$.mime', a.body ->> '$.mime') AS mime, o.value ->> '$.uploadId' AS ${sql('uploadId')}
            FROM assets a, json_each(a.uploads) u, json_each(u.value, '$.objects') o LEFT JOIN json_each(a.body, '$.renditions') r ON r.value ->> '$.width' = o.value ->> '$.width'
            WHERE u.value ->> '$.discard' = 0 AND a.removing IS NOT TRUE AND a.${sql('objectKey')} IS NULL
            AND (${key} IS NULL OR o.value ->> '$.key' = ${key}) AND (${attempt} IS NULL OR u.value ->> '$.id' = ${attempt})`,
        ),
});
const beginUpload = Effect.fn('beginUpload')(
    Function.flow(
        SqlSchema.findOne({
            Request: assets.schema.mapFields(Struct.pick(['id', 'body'])),
            Result: assets.schema.mapFields(Struct.pick(['objectKey'])),
            execute: (row) =>
                SqlClient.SqlClient.use(
                    (sql) => sql`INSERT INTO assets (id, body) VALUES (${row.id}, ${row.body})
                    ON CONFLICT (id) DO UPDATE SET body = CASE WHEN assets.${sql('objectKey')} IS NULL THEN json_insert(assets.body, '$.uploads[#]', excluded.body -> '$.uploads[0]') ELSE assets.body END
                    WHERE json_remove(assets.body, ${projectedPaths}) = json_remove(excluded.body, ${projectedPaths}) AND assets.removing IS NOT TRUE
                    RETURNING ${sql('objectKey')}`,
                ),
        }),
        Effect.map(Function.flow(Struct.get('objectKey'), Option.isSome)),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => HttpApiError.Conflict.make()),
    ),
);
const claimed = <A, R>(statement: Effect.Effect<readonly A[], SqlError.SqlError, R>) =>
    statement.pipe(
        Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => HttpApiError.Conflict.make()),
        Effect.asVoid,
        Effect.catchTag('SqlError', unavailable),
    );
const attachUpload = Effect.fn('attachUpload')(
    (assetId: keyof Assets['Service'], key: string, uploadId: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_set(body, (
                SELECT o.fullkey || '.uploadId' FROM json_each(body, '$.uploads') u, json_each(body, u.fullkey || '.objects') o WHERE o.value ->> '$.key' = ${key}
            ), ${uploadId}) WHERE id = ${assetId} AND EXISTS (
                SELECT 1 FROM json_each(uploads) u, json_each(u.value, '$.objects') o WHERE u.value ->> '$.discard' = 0 AND o.value ->> '$.key' = ${key}
            ) RETURNING id`,
        ),
    claimed,
);
const completeUpload = Effect.fn('completeUpload')(
    (assetId: keyof Assets['Service'], attempt: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_insert(body, '$.objectKey', ${attempt}) WHERE id = ${assetId} AND removing IS NOT TRUE AND EXISTS (
                SELECT 1 FROM json_each(uploads) u WHERE u.value ->> '$.id' = ${attempt} AND (u.value ->> '$.discard' = 0 OR ${sql('objectKey')} IS NOT NULL)
            ) RETURNING id`,
        ),
    claimed,
);

// --- [REMOVALS]

const releaseUploads = Effect.fn('releaseUploads')(
    Function.flow(
        SqlSchema.findOne({
            Request: Release,
            Result: Released,
            execute: ({ assetId, action }) =>
                SqlClient.SqlClient.use((sql) => {
                    const pending = sql`(removing IS TRUE OR ${sql('objectKey')} IS DISTINCT FROM u.value ->> '$.id')`;
                    const discard = (discarded: Statement.Fragment) => sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                        SELECT json_group_array(json(CASE WHEN ${discarded} THEN json_set(u.value, '$.discard', json('true')) ELSE u.value END)) FROM json_each(uploads) u
                    ))) WHERE id = ${assetId}`;
                    const released = sql`SELECT (
                        SELECT json_group_array(json(o.value)) FROM json_each(a.uploads) u, json_each(u.value, '$.objects') o WHERE u.value ->> '$.discard' = 1
                    ) AS objects FROM assets a WHERE a.id = ${assetId}`;
                    const statements = {
                        discard: [discard(pending), released],
                        delete: [sql`UPDATE assets SET body = json_set(body, '$.removing', json('true')) WHERE id = ${assetId} AND NOT EXISTS (SELECT 1 FROM (${references(sql)}) WHERE asset_id = assets.id)`, discard(sql`removing IS TRUE`), sql`${released} AND a.removing IS TRUE`],
                    } as const;
                    return Effect.map(Rows.atomic(statements[action]), Array.lastNonEmpty);
                }),
        }),
        Effect.map(Struct.get('objects')),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
    ),
);
const finishRelease = Effect.fn('finishRelease')(
    (assetId: keyof Assets['Service'], keys: Array.NonEmptyReadonlyArray<string>) =>
        SqlClient.SqlClient.use((sql) =>
            Rows.atomic([
                sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                    SELECT json_group_array(json(value)) FROM (
                        SELECT CASE WHEN u.value ->> '$.discard' = 1
                            THEN json_set(u.value, '$.objects', json((SELECT json_group_array(json(o.value)) FROM json_each(u.value, '$.objects') o LEFT JOIN json_each(${JSON.stringify(keys)}) k ON k.value = o.value ->> '$.key' WHERE k.value IS NULL)))
                            ELSE u.value END AS value FROM json_each(uploads) u
                    ) WHERE value ->> '$.discard' = 0 OR json_array_length(value, '$.objects') > 0
                ))) WHERE id = ${assetId}`,
                sql`DELETE FROM assets WHERE id = ${assetId} AND NOT available AND json_array_length(uploads) = 0`,
            ]),
        ),
    Effect.asVoid,
    Effect.catchTag('SqlError', unavailable),
);
const releaseObjects = Effect.fnUntraced(function* (release: Release) {
    const [released, failures] = yield* Effect.partition(yield* releaseUploads(release), ({ key, uploadId }) => Effect.as(ObjectStorage.remove(key, uploadId), key), { concurrency: 'unbounded' });
    yield* Array.isReadonlyArrayNonEmpty(released) ? finishRelease(release.assetId, released) : Effect.void;
    return yield* Array.isReadonlyArrayNonEmpty(failures) ? Effect.andThen(Effect.logError('object delete failed', ...failures.map(Cause.fail)), HttpApiError.ServiceUnavailable.make()) : Effect.void;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const begin = HttpApiBuilder.handler(
    Api,
    'assets',
    'begin',
    Effect.fn('begin')(function* ({ payload }) {
        const attempt = yield* SecureRandom.SecureRandom.use((random) => random.uuidV7());
        const { id, ...metadata } = payload;
        const expected = [...Asset.matchOrElse(payload, { image: ({ renditions = [] }) => renditions }, () => []).map(({ width }) => ({ key: `${attempt}/${width}`, width })), { key: attempt }];
        const stored = yield* beginUpload({ id, body: { ...metadata, uploads: [Schema.toEncoded(UploadAttempt).make({ id: attempt, discard: false, objects: expected })] } });
        return { attempt, objects: stored ? [] : expected };
    }),
);
const sign = HttpApiBuilder.handler(
    Api,
    'assets',
    'sign',
    Effect.fn('sign')(
        function* ({ payload }) {
            const unknown = () => UploadRefused.make({ reason: { _tag: 'Unknown', key: payload.key } });
            const object = yield* Effect.flatMap(declared({ key: payload.key, attempt: null }), (found) => Effect.fromOption(Array.head(found), unknown));
            yield* 'uploadId' in payload && Option.isNone(object.uploadId) ? Effect.catchTag(attachUpload(object.assetId, payload.key, payload.uploadId), 'Conflict', unknown) : Effect.void;
            return yield* ObjectStorage.use((storage) => storage.sign({ size: ByteSize.bytes(object.size), mime: object.mime })(payload));
        },
        Effect.catchTag(['SqlError', 'SchemaError', 'ObjectStorageError'], unavailable),
    ),
);
const complete = HttpApiBuilder.handler(
    Api,
    'assets',
    'complete',
    Effect.fn('complete')(
        function* ({ params: { id, attempt } }) {
            const expected = (yield* declared({ key: null, attempt })).filter(({ assetId }) => assetId === id);
            const held = yield* Effect.forEach(expected, (object) => ObjectStorage.use((storage) => storage.stat(object.key)).pipe(Effect.map((stat) => ({ object, stat }))), { concurrency: 'unbounded' });
            const missing = held.filter(({ stat }) => Option.isNone(stat)).map(({ object }) => object.key);
            yield* Array.isReadonlyArrayNonEmpty(missing) ? UploadRefused.make({ reason: { _tag: 'Missing', keys: missing } }) : Effect.void;
            const differing = held.find(({ object, stat }) => Option.exists(stat, ({ size, mime }) => ByteSize.toNumberUnsafe(size) !== object.size || mime !== object.mime));
            yield* differing === undefined ? Effect.void : UploadRefused.make({ reason: { _tag: 'Size', key: differing.object.key } });
            yield* completeUpload(id, attempt);
        },
        Effect.catchTag(['SqlError', 'SchemaError', 'ObjectStorageError'], unavailable),
    ),
);
const read = HttpApiBuilder.handler(
    Api,
    'assets',
    'read',
    Effect.fn('read')(function* ({ params }) {
        const { objectKey, mime, name, published } = yield* readAsset(params);
        yield* published ? Effect.void : Effect.catchTag(OwnerAccount.authorize, 'Forbidden', () => HttpApiError.NotFound.make());
        return yield* ObjectStorage.serve({ key: objectKey, mime, ...Record.getSomes({ filename: name }), ...(published && { cacheControl: `private, max-age=${Duration.toSeconds(Duration.days(1))}` }) }, { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) });
    }),
);
const assetGroup = HttpApiBuilder.group(Api, 'assets', (handlers) =>
    handlers.handleAll({
        begin,
        sign,
        complete,
        recovery: () => readRecovery,
        discard: ({ params }) => releaseObjects({ assetId: params.id, action: 'discard' }).pipe(Effect.catchTag('NoSuchElementError', () => HttpApiError.NotFound.make())),
        delete: ({ params }) => releaseObjects({ assetId: params.id, action: 'delete' }).pipe(Effect.catchTag('NoSuchElementError', () => HttpApiError.Conflict.make())),
        read,
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { assetGroup };
