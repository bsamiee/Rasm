import { atomic } from '@rasm/database';
import { unavailable } from '@rasm/hosting';
import { ObjectStorage } from '@rasm/hosting/server';
import { Array, ByteSize, Cause, Effect, Exit, Function, Option, Record, Schema, SchemaIssue, Sink, Stream, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { SqlClient, type SqlError, SqlSchema, type Statement } from 'effect/sql';
import { Api, Recovery, UploadRefused, uploadField } from '#/model/api.ts';
import { Asset, type AssetId } from '#/model/asset.ts';
import { assets, projectedPaths, references, UploadAttempt } from '#/server/catalog.ts';
import { OwnerAccount } from '#/server/owner.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Release = Schema.Struct({ assetId: assets.schema.fields.id, action: Schema.TaggedUnion({ Abandon: { attemptId: Schema.String }, Discard: {}, Delete: {} }) });
const AssetRead = Schema.Struct({ id: assets.schema.fields.id, width: Schema.OptionFromNullOr(Schema.Int) });
const AssetObject = Schema.Struct({ mime: Schema.String, name: Schema.OptionFromNullOr(Schema.String), published: Schema.BooleanFromBit, objectKey: Schema.String });
const Released = Schema.Struct({ objects: Schema.fromJsonString(UploadAttempt.fields.objects) });

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
                    JOIN json_each(a.uploads) u ON u.value ->> '$.id' = a.${sql('objectKey')} JOIN json_each(u.value, '$.objects') o ON o.value ->> '$.width' IS q.width
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
const objects = (asset: typeof Asset.Type, attempt: string) => [
    ...Asset.matchOrElse(asset, { image: ({ renditions = [] }) => renditions }, () => []).map(({ width, size, mime }) => ({ key: `${attempt}/${width}`, width: Option.some(width), size: ByteSize.bytes(size), mime, field: uploadField.renditions })),
    { key: attempt, width: Option.none<number>(), size: ByteSize.bytes(asset.size), mime: asset.mime, field: uploadField.file },
];
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
    (assetId: typeof AssetId.Type, key: string, uploadId: string) =>
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
    (assetId: typeof AssetId.Type, attempt: string) =>
        SqlClient.SqlClient.use(
            (sql) => sql`UPDATE assets SET body = json_insert(body, '$.objectKey', ${attempt}) WHERE id = ${assetId} AND removing IS NOT TRUE AND EXISTS (
                SELECT 1 FROM json_each(uploads) u WHERE u.value ->> '$.id' = ${attempt} AND (u.value ->> '$.discard' = 0 OR ${sql('objectKey')} IS NOT NULL)
            ) RETURNING id`,
        ),
    claimed,
);
const receive = Effect.fnUntraced(function* (assetId: typeof AssetId.Type, stored: boolean, expected: ReturnType<typeof objects>, index: number, part: Multipart.Part) {
    const object = yield* Effect.fromOption(Array.get(expected, index), () => UploadRefused.make({ reason: { _tag: 'Count', expected: expected.length, received: index + 1 } }));
    const file = yield* Option.liftPredicate(part, Multipart.isFile).pipe(
        Option.filter((sent) => sent.key === object.field && sent.contentType === object.mime && Option.match(object.width, { onNone: () => true, onSome: (width) => sent.name === String(width) })),
        Effect.fromOption(() => UploadRefused.make({ reason: { _tag: 'Part', index, field: object.field, mime: object.mime } })),
    );
    yield* stored
        ? Stream.runDrain(file.content)
        : ObjectStorage.upload(object).pipe(
              Effect.tap(({ uploadToken }) => attachUpload(assetId, object.key, uploadToken)),
              Effect.flatMap(({ write }) => write(file.content)),
              Effect.catchTag('BadRequest', () => UploadRefused.make({ reason: { _tag: 'Size', index } })),
          );
});

// --- [REMOVALS]
const releaseUploads = Effect.fn('releaseUploads')(
    Function.flow(
        SqlSchema.findOne({
            Request: Release,
            Result: Released,
            execute: ({ assetId, action }) =>
                SqlClient.SqlClient.use((sql) => {
                    const pending = sql`(removing IS TRUE OR ${sql('objectKey')} IS NOT u.value ->> '$.id')`;
                    const discard = (...discarded: readonly Statement.Fragment[]) => sql`UPDATE assets SET body = json_set(body, '$.uploads', json((
                        SELECT json_group_array(json(CASE WHEN ${sql.and(discarded)} THEN json_set(u.value, '$.discard', json('true')) ELSE u.value END)) FROM json_each(uploads) u
                    ))) WHERE id = ${assetId}`;
                    const released = (...conditions: readonly Statement.Fragment[]) => sql`SELECT (
                        SELECT json_group_array(json(o.value)) FROM json_each(a.uploads) u, json_each(u.value, '$.objects') o WHERE ${sql.and([sql`u.value ->> '$.discard' = 1`, ...conditions])}
                    ) AS objects FROM assets a WHERE a.id = ${assetId}`;
                    const statements = Release.fields.action.match(action, {
                        Abandon: ({ attemptId }) => {
                            const attempt = sql`u.value ->> '$.id' = ${attemptId}`;
                            return [discard(attempt, pending), released(attempt)] as const;
                        },
                        Discard: () => [discard(pending), released()] as const,
                        Delete: () => [sql`UPDATE assets SET body = json_set(body, '$.removing', json('true')) WHERE id = ${assetId} AND NOT EXISTS (SELECT 1 FROM (${references(sql)}) WHERE asset_id = assets.id)`, discard(sql`removing IS TRUE`), sql`${released()} AND a.removing IS TRUE`] as const,
                    });
                    return Effect.map(atomic(statements), Array.lastNonEmpty);
                }),
        }),
        Effect.map(Struct.get('objects')),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
    ),
);
const finishRelease = Effect.fn('finishRelease')(
    (assetId: typeof AssetId.Type, keys: Array.NonEmptyReadonlyArray<string>) =>
        SqlClient.SqlClient.use((sql) =>
            atomic([
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
const releaseObjects = Effect.fnUntraced(function* (release: typeof Release.Type) {
    const [released, failures] = yield* Effect.partition(yield* releaseUploads(release), ({ key, uploadId }) => Effect.as(ObjectStorage.remove(key, uploadId), key), { concurrency: 'unbounded' });
    yield* Array.isReadonlyArrayNonEmpty(released) ? finishRelease(release.assetId, released) : Effect.void;
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError('object delete failed', ...failures.map(Cause.fail));
        return yield* HttpApiError.ServiceUnavailable.make();
    }
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const upload = HttpApiBuilder.handler(
    Api,
    'assets',
    'upload',
    Effect.fn('upload')(
        function* ({ payload }) {
            const [head, parts] = yield* Stream.peel(payload, Sink.head());
            const asset = yield* head.pipe(
                Option.filter(Multipart.isField),
                Option.filter(({ key }) => key === uploadField.asset),
                Effect.fromOption(() => UploadRefused.make({ reason: { _tag: 'Order' } })),
                Effect.flatMap(({ value }) => Schema.decodeEffect(Asset.json)(value)),
                Effect.catchTag('SchemaError', ({ issue }) => UploadRefused.make({ reason: { _tag: 'Metadata', ...SchemaIssue.makeFormatterStandardSchemaV1()(issue) } })),
            );
            const attempt = yield* SecureRandom.SecureRandom.use((random) => random.uuidV7());
            const expected = objects(asset, attempt);
            const { id, ...metadata } = asset;
            const stored = yield* Effect.acquireRelease(beginUpload({ id, body: { ...metadata, uploads: [{ id: attempt, discard: false, objects: expected.map(({ key, width }) => ({ key, ...Record.getSomes({ width }) })) } satisfies typeof UploadAttempt.Encoded] } }), (_, exit) =>
                Exit.isFailure(exit) ? releaseObjects({ assetId: id, action: { _tag: 'Abandon', attemptId: attempt } }).pipe(Effect.ignore({ log: 'Error', message: 'abandoned upload release failed' })) : Effect.void,
            );
            yield* parts.pipe(
                Stream.mapEffect((part, index) => receive(asset.id, stored, expected, index, part)),
                Stream.runCount,
                Effect.filterOrFail(
                    (received) => received === expected.length,
                    (received) => UploadRefused.make({ reason: { _tag: 'Count', expected: expected.length, received } }),
                ),
            );
            yield* stored ? Effect.void : completeUpload(asset.id, attempt);
            return { id: asset.id };
        },
        Effect.scoped,
        Effect.catchTags({ MultipartError: ({ reason }) => UploadRefused.make({ reason: { _tag: 'Multipart', cause: reason._tag } }), HostingError: unavailable }),
    ),
);
const read = HttpApiBuilder.handler(
    Api,
    'assets',
    'read',
    Effect.fn('read')(function* ({ params }) {
        const { objectKey, mime, name, published } = yield* readAsset(params);
        if (!published) {
            yield* Effect.catchTag(OwnerAccount.authorize, 'Forbidden', () => HttpApiError.NotFound.make());
        }
        return yield* ObjectStorage.serve({ key: objectKey, mime, ...Record.getSomes({ filename: name }) }, { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) });
    }),
);
const assetGroup = HttpApiBuilder.group(Api, 'assets', (handlers) =>
    handlers.handleAll({
        upload,
        recovery: () => readRecovery,
        discard: ({ params }) => releaseObjects({ assetId: params.id, action: { _tag: 'Discard' } }).pipe(Effect.catchTag('NoSuchElementError', () => HttpApiError.NotFound.make())),
        delete: ({ params }) => releaseObjects({ assetId: params.id, action: { _tag: 'Delete' } }).pipe(Effect.catchTag('NoSuchElementError', () => HttpApiError.Conflict.make())),
        read,
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { assetGroup };
