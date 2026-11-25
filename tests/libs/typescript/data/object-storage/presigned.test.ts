import { CreateBucketCommand, S3Client } from '@aws-sdk/client-s3';
import { assert, layer } from '@effect/vitest';
import { ObjectStorage, Presignable } from '@rasm/object-storage';
import { s3Storage } from '@rasm/object-storage/s3';
import { ByteSize, ConfigProvider, Effect, Layer, Option, Record, Schema } from 'effect';
import { FetchHttpClient, Headers, HttpClient, HttpClientRequest } from 'effect/http';
import { GenericContainer, Wait } from 'testcontainers';

// --- [CONSTANTS] -----------------------------------------------------------------------

const credentials = { accessKeyId: 'storage-access', secretAccessKey: 'storage-secret' } as const;
const [bucket, region, port, mime] = ['objects', 'us-east-1', 9000, 'video/mp4'] as const;
const encoder = new TextEncoder();
const uploaded = /<UploadId>(?<id>[^<]+)<\/UploadId>/u;
const listed = /<PartNumber>(?<part>\d+)<\/PartNumber>/gu;
const failure = /<Code>(?<code>[^<]+)<\/Code>/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const body = (size: number) => Uint8Array.from({ length: size }, (_, index) => (index * 31) % 253);
const sent = Effect.fnUntraced(function* (request: typeof Presignable.Encoded, size: number, bytes?: Uint8Array) {
    const decoded = yield* Schema.decodeEffect(Presignable)(request);
    const { url, headers } = yield* ObjectStorage.use(({ sign }) => sign({ size: ByteSize.bytes(size), mime })(decoded));
    const made = HttpClientRequest.make(request.method)(url, { headers });
    const response = yield* HttpClient.execute(bytes === undefined ? made : HttpClientRequest.bodyUint8Array(made, bytes, Option.getOrUndefined(Headers.get(Headers.fromInput(headers), 'content-type'))));
    const text = yield* response.text;
    return { status: response.status, code: Option.fromNullishOr(failure.exec(text)?.groups?.['code']), etag: Headers.get(response.headers, 'etag'), text };
});
const stat = (key: string) => ObjectStorage.use((storage) => storage.stat(key)).pipe(Effect.map(Option.map(({ size, mime: stated }) => ({ size: ByteSize.toNumberUnsafe(size), mime: stated }))));

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Layer.unwrap(
    Effect.gen(function* () {
        const container = yield* Effect.acquireDisposable(
            Effect.promise(() => new GenericContainer('rustfs/rustfs:1.0.6').withEnvironment({ RUSTFS_ACCESS_KEY: credentials.accessKeyId, RUSTFS_SECRET_KEY: credentials.secretAccessKey, RUSTFS_CONSOLE_ENABLE: 'false' }).withExposedPorts(port).withWaitStrategy(Wait.forHttp('/health/ready', port)).start()),
        );
        const endpoint = `http://${container.getHost()}:${container.getMappedPort(port)}`;
        yield* Effect.acquireUseRelease(
            Effect.sync(() => new S3Client({ endpoint, region, credentials, forcePathStyle: true })),
            (client) => Effect.promise(() => client.send(new CreateBucketCommand({ Bucket: bucket }))),
            (client) => Effect.sync(() => client.destroy()),
        );
        return Layer.merge(s3Storage(bucket).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown({ AWS_ENDPOINT_URL_S3: endpoint, AWS_REGION: region, AWS_ACCESS_KEY_ID: credentials.accessKeyId, AWS_SECRET_ACCESS_KEY: credentials.secretAccessKey })))), FetchHttpClient.layer);
    }),
);

layer(served, { timeout: '2 minutes', excludeTestServices: true })('presigned requests to S3', (it) => {
    it.effect(
        'stores the body of a presigned upload sent with its stated headers, and refuses a second upload to its key and a body of another size',
        Effect.fnUntraced(function* () {
            const [first, second] = [
                { method: 'PUT', key: 'media/first.mp4' },
                { method: 'PUT', key: 'media/second.mp4' },
            ] as const;
            const outcomes = {
                accepted: yield* sent(first, 21, body(21)),
                repeated: yield* sent(first, 21, body(21).toReversed()),
                resized: yield* sent(second, 21, body(22)),
            };
            assert.deepStrictEqual(
                { ...Record.map(outcomes, ({ status, code }) => [status, code]), first: yield* stat(first.key), second: yield* stat(second.key) },
                { accepted: [200, Option.none()], repeated: [412, Option.some('PreconditionFailed')], resized: [403, Option.some('SignatureDoesNotMatch')], first: Option.some({ size: 21, mime }), second: Option.none() },
            );
        }),
    );
    it.effect(
        'assembles an object from presigned create, part, list, and complete requests',
        Effect.fnUntraced(function* () {
            const key = 'media/parted.mp4';
            const uploadId = yield* Effect.fromNullishOr(uploaded.exec((yield* sent({ method: 'POST', key }, 0)).text)?.groups?.['id']);
            const etag = yield* Effect.fromOption((yield* sent({ method: 'PUT', key, uploadId, partNumber: 1 }, 34, body(34))).etag);
            const parts = yield* sent({ method: 'GET', key, uploadId }, 0);
            const completed = yield* sent({ method: 'POST', key, uploadId }, 0, encoder.encode(`<CompleteMultipartUpload><Part><PartNumber>1</PartNumber><ETag>${etag}</ETag></Part></CompleteMultipartUpload>`));
            assert.deepStrictEqual({ listed: Array.from(parts.text.matchAll(listed), (match) => match.groups?.['part']), completed: [completed.status, completed.code], assembled: yield* stat(key) }, { listed: ['1'], completed: [200, Option.none()], assembled: Option.some({ size: 34, mime }) });
        }),
    );
    it.effect(
        'removes a multipart upload with the parts it holds',
        Effect.fnUntraced(function* () {
            const key = 'media/abandoned.mp4';
            const uploadId = yield* Effect.fromNullishOr(uploaded.exec((yield* sent({ method: 'POST', key }, 0)).text)?.groups?.['id']);
            yield* sent({ method: 'PUT', key, uploadId, partNumber: 1 }, 5, body(5));
            const held = yield* sent({ method: 'GET', key, uploadId }, 0);
            yield* ObjectStorage.remove(key, Option.some(uploadId));
            const gone = yield* sent({ method: 'GET', key, uploadId }, 0);
            assert.deepStrictEqual({ held: Array.from(held.text.matchAll(listed), (match) => match.groups?.['part']), gone: [gone.status, gone.code], stored: yield* stat(key) }, { held: ['1'], gone: [404, Option.some('NoSuchUpload')], stored: Option.none() });
        }),
    );
});
