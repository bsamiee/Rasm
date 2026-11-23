import { MediaError, processUploads, selectUploadState } from '@rasm/media';
import { Presignable } from '@rasm/object-storage';
import AwsS3 from '@uppy/aws-s3';
import Uppy, { type Body, type Meta, type UppyEventMap, type UppyFile } from '@uppy/core';
import { Array, ByteSize, Data, Effect, Equal, Equivalence, Function, Layer, Match, Optic, Predicate, Record, Result, Schema, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { HttpApiClient } from 'effect/http-api';
import { Atom, AtomRegistry } from 'effect/reactivity';
import { assets, draft, hero, PortfolioClient, recoveryRequest, updateEntry } from '#/editor/state';
import { Api, type UploadRefused } from '#/model/api';
import { Asset, PdfAsset, uploadLimit, VideoAsset } from '#/model/asset';
import { Composition, Placement } from '#/model/placement';
import type { Entry } from '#/model/portfolio';

// --- [TYPES] ---------------------------------------------------------------------------

type Destination = Data.TaggedEnum<{ Library: object; Hero: object; Entry: { readonly id: Entry['id'] } }>;
interface UploadMeta extends Meta {
    readonly destination: Destination;
    readonly prepared?: { readonly asset: Composition['items'][number]['asset']; readonly compositions: readonly Composition[]; readonly attempt: string; readonly key: string };
}
type UploadFailure = Parameters<UppyEventMap<UploadMeta, Body>['upload-error']>[1];

// --- [SERVICES] ------------------------------------------------------------------------

const Destination = Data.taggedEnum<Destination>();
const nextDestination = Atom.make<Destination>(Destination.Library()).pipe(Atom.keepAlive);
const signed = Effect.fn('signed')(function* (request: unknown) {
    const client = yield* HttpApiClient.make(Api);
    return yield* client.assets.sign({ payload: yield* Schema.decodeUnknownEffect(Presignable)(request) });
}, Effect.provide(FetchHttpClient.layer));
const uppy = Atom.make((get) => {
    const uploader: Uppy<UploadMeta, Body> = new Uppy<UploadMeta, Body>({
        restrictions: { maxFileSize: ByteSize.toNumberUnsafe(uploadLimit), allowedFileTypes: [...Asset.mediaTypes] },
        onBeforeFileAdded: (file, queued) => {
            const target = get.registry.get(nextDestination);
            if (Destination.$is('Hero')(target) && Object.values(queued).some((item) => Destination.$is('Hero')(item.meta.destination) && item.progress.uploadComplete !== true)) {
                uploader.info('Finish or remove queued hero file first', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    }).use(AwsS3<UploadMeta, Body>, { shouldUseMultipart: true, limit: 3, generateObjectKey: (file: UppyFile<UploadMeta, Body>) => file.meta.prepared?.key ?? file.id, signRequest: (request) => Effect.runPromise(signed(request)) });
    uploader.on('complete', () => get.registry.refresh(recoveryRequest));
    uploader.on('file-removed', (file) => {
        if (file.meta.prepared !== undefined && file.progress.uploadComplete !== true) {
            get.registry.refresh(recoveryRequest);
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);
const unreadableMessage = Match.type<string>().pipe(
    Match.when(Match.is(...PdfAsset.fields.mime.literals), () => 'Browser cannot read PDF. Remove it and add new export'),
    Match.when(Match.is(...VideoAsset.fields.mime.literals), () => 'Browser cannot play video. Remove it and add H.264 MP4 or VP9 WebM export'),
    Match.orElse(() => 'Browser cannot read image. Remove it and add new export'),
);
const refused = {
    UploadRefused: ({ message }: UploadRefused) => message,
    HttpClientError: () => 'Upload could not reach storage. Retry',
    Conflict: () => 'File already uploaded with other details. Remove it and add it again',
    Forbidden: () => 'Session ended. Sign in again and retry',
    ServiceUnavailable: () => 'Server could not record upload. Retry',
} as const;
const prepare = Effect.fnUntraced(
    function* ({ meta: { destination }, name, type }: UppyFile<UploadMeta, Body>, data: Blob) {
        const { inspectFile } = yield* Effect.tryPromise({ try: () => import('#/editor/inspect-file'), catch: MediaError.of('Unavailable', name) });
        const [random, api, http] = yield* Effect.all([SecureRandom.SecureRandom, PortfolioClient, HttpClient.HttpClient]);
        const { asset, renditions } = yield* inspectFile(new File([data], name, { type }), yield* random.uuidV7());
        const { attempt, objects } = yield* api.assets.begin({ payload: asset });
        yield* Effect.forEach(
            Array.zip(objects, renditions),
            Effect.fnUntraced(function* ([{ key }, rendition]) {
                const { url, headers } = yield* api.assets.sign({ payload: { method: 'PUT', key } });
                const body = yield* Effect.tryPromise({ try: () => rendition.bytes(), catch: MediaError.of('Unreadable', `${name} rendition ${rendition.name}`) });
                yield* Effect.flatMap(http.execute(HttpClientRequest.put(url, { headers }).pipe(HttpClientRequest.bodyUint8Array(body, rendition.type))), HttpClientResponse.filterStatusOk).pipe(Effect.provideService(HttpClient.TracerPropagationEnabled, false));
            }),
            { concurrency: 'unbounded', discard: true },
        );
        const placements = Destination.$is('Entry')(destination) && !Asset.guards.pdf(asset) ? Placement.fromAsset(asset) : [];
        const compositions = yield* Effect.forEach(
            placements,
            Effect.fnUntraced(function* (placement) {
                const id = yield* random.uuidV7();
                return Composition.make({ id: Composition.fields.id.make(id), items: [placement] });
            }),
        );
        return { prepared: { asset, compositions, attempt, key: attempt } };
    },
    (effect, { type }) =>
        Effect.mapError(effect, (error): UploadFailure => ({
            name: error._tag,
            message: Match.valueTags(error, {
                MediaError: ({ reason }) =>
                    Match.valueTags(reason, {
                        Protected: () => 'PDF is password protected. Remove it and add unlocked export',
                        Unsupported: () => 'Browser cannot resize image. Retry in another browser',
                        Unreadable: () => unreadableMessage(type),
                        Missing: () => unreadableMessage(type),
                        Unavailable: () => 'Browser cannot read file. Retry or add it again',
                    }),
                SchemaError: ({ message }) => `${message}. Remove it and add new export`,
                ...refused,
            }),
        })),
);
const placed = Effect.fnUntraced(
    function* (file: UppyFile<UploadMeta, Body>) {
        const { asset, compositions, attempt } = yield* Effect.fromNullishOr(file.meta.prepared);
        const [api, registry] = yield* Effect.all([PortfolioClient, AtomRegistry.AtomRegistry]);
        yield* api.assets.complete({ params: { id: asset.id, attempt } });
        const uploader = registry.get(uppy);
        const queued = uploader.getFiles();
        const following = new Set(
            queued
                .values()
                .drop(queued.findIndex(Function.flow(Struct.get('id'), Equal.equals(file.id))) + 1)
                .map(Struct.get('meta'))
                .map(Struct.get('prepared'))
                .filter(Predicate.isNotUndefined)
                .flatMap(Struct.get('compositions'))
                .map(Struct.get('id')),
        );
        const preceding = ({ id }: Composition) => !following.has(id);
        const stored = assets.modify(Record.set(asset.id, asset));
        registry.update(
            draft,
            Destination.$match(file.meta.destination, {
                Library: () => stored,
                Hero: () => Function.flow(stored, Optic.replace(hero, Array.headNonEmpty(Placement.fromAsset(asset)))),
                Entry: ({ id }) => Function.flow(stored, updateEntry(id, Struct.evolve({ compositions: (current) => Array.flatten(Array.intersperse(Array.span(current, preceding), compositions)) }))),
            }),
        );
    },
    Effect.mapError((error): UploadFailure => ({ name: error._tag, message: Match.valueTags(error, { ...refused, NoSuchElementError: () => 'File lost its prepared details. Remove it and add it again', SchemaError: () => 'Server answered in an unknown form. Retry' }) })),
);
const preprocessing = PortfolioClient.runtime.atom((get) => processUploads(get(uppy), { prepare, finish: placed }, { concurrency: 2, message: 'Reading file' }).pipe(Effect.provide(Layer.merge(SecureRandom.layerWebCrypto, FetchHttpClient.layer)))).pipe(Atom.keepAlive);
const queue = selectUploadState(uppy, Struct.get('files'));
const pendingUploads = Atom.make((get) => Array.filterMap(Object.values(get(queue)), (file) => (file.progress.uploadComplete === true ? Result.failVoid : Result.succeed(file.meta.destination)))).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Destination, nextDestination, pendingUploads, preprocessing, queue, uppy };
