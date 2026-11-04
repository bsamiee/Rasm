import { preprocessUploads, selectUploadState } from '@rasm/media';
import Uppy, { type Body, type Meta, type UppyFile } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, ByteSize, Cause, Data, Effect, Equal, Equivalence, Function, Match, Optic, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { HttpApiClient } from 'effect/http-api';
import { Atom } from 'effect/reactivity';
import { assets, draft, hero, recoveryRequest, updateEntry } from '#/editor/state.ts';
import { Api, UploadRefused, uploadField } from '#/model/api.ts';
import { Asset, PdfAsset, uploadLimit, VideoAsset } from '#/model/asset.ts';
import { Composition, CompositionId, Placement } from '#/model/placement.ts';
import type { EntryId } from '#/model/portfolio.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Destination = Data.TaggedEnum<{ Library: Record<never, never>; Hero: Record<never, never>; Entry: { readonly id: typeof EntryId.Type } }>;
interface UploadMeta extends Meta {
    readonly destination: Destination;
    readonly [uploadField.asset]?: string;
    readonly prepared?: { readonly asset: typeof Asset.Type; readonly compositions: readonly (typeof Composition.Type)[] };
    readonly [uploadField.renditions]?: readonly File[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const refused = Schema.decodeUnknownOption(Schema.fromJsonString(UploadRefused));

// --- [SERVICES] ------------------------------------------------------------------------

const Destination = Data.taggedEnum<Destination>();
const nextDestination = Atom.make<Destination>(Destination.Library()).pipe(Atom.keepAlive);
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
    }).use(XHRUpload, {
        endpoint: HttpApiClient.urlBuilder(Api).assets.upload(),
        fieldName: uploadField.file,
        allowedMetaFields: [uploadField.asset, uploadField.renditions],
        limit: 3,
        shouldRetry: (xhr) => xhr.status === 0 || HttpClient.isTransientResponse(HttpClientResponse.fromWeb(HttpClientRequest.post(xhr.responseURL), new Response(null, { status: xhr.status }))),
        onAfterResponse: (xhr) =>
            Option.match(refused(xhr.responseText), {
                onNone: Function.constVoid,
                onSome: (refusal) => {
                    throw refusal;
                },
            }),
    });
    uploader.on('upload-success', (file) => {
        if (file?.meta.prepared !== undefined) {
            const { asset, compositions } = file.meta.prepared;
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
            const preceding = (item: typeof Composition.Type) => !following.has(item.id);
            const stored = assets.modify(Record.set(asset.id, asset));
            get.registry.update(
                draft,
                Destination.$match(file.meta.destination, {
                    Library: () => stored,
                    Hero: () => Function.flow(stored, Optic.replace(hero, Array.headNonEmpty(Placement.fromAsset(asset)))),
                    Entry: ({ id }) => Function.flow(stored, updateEntry(id, Struct.evolve({ compositions: (current) => [...Array.takeWhile(current, preceding), ...compositions, ...Array.dropWhile(current, preceding)] }))),
                }),
            );
            uploader.setFileMeta(file.id, { ...file.meta, [uploadField.renditions]: [] });
        }
    });
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
const prepare = Effect.fnUntraced(
    function* ({ meta: { destination }, name, type }: UppyFile<UploadMeta, Body>, data: Blob) {
        const { inspectFile } = yield* Effect.tryPromise(() => import('#/editor/inspect-file.ts'));
        const random = yield* SecureRandom.SecureRandom;
        const { asset, renditions } = yield* inspectFile(new File([data], name, { type }), yield* random.uuidV7());
        const placements = Destination.$is('Entry')(destination) && !Asset.guards.pdf(asset) ? Placement.fromAsset(asset) : [];
        const compositions = yield* Effect.forEach(
            placements,
            Effect.fnUntraced(function* (placement: typeof Placement.Type) {
                const id = yield* random.uuidV7();
                return Composition.make({ id: CompositionId.make(id), items: [placement] });
            }),
        );
        return { [uploadField.asset]: yield* Schema.encodeEffect(Asset.json)(asset), prepared: { asset, compositions }, [uploadField.renditions]: renditions };
    },
    (effect, { type }) =>
        Effect.mapError(
            effect,
            (error) =>
                new Cause.UnknownError(
                    error,
                    Match.valueTags(error, {
                        MediaError: ({ reason }) =>
                            Match.valueTags(reason, {
                                Protected: () => 'PDF is password protected. Remove it and add unlocked export',
                                Unsupported: () => 'Browser cannot resize image. Retry in another browser',
                                Unreadable: () => unreadableMessage(type),
                                Missing: () => unreadableMessage(type),
                            }),
                        SchemaError: ({ message }) => `${message}. Remove it and add new export`,
                        UnknownError: () => 'Browser cannot read file. Retry or add it again',
                    }),
                ),
        ),
);
const preprocessing = Atom.runtime(SecureRandom.layerWebCrypto)
    .atom((get) => preprocessUploads(get(uppy), prepare, { concurrency: 2, message: 'Reading file' }))
    .pipe(Atom.keepAlive);
const queue = selectUploadState(uppy, Struct.get('files'));
const pendingUploads = Atom.make((get) => Array.filterMap(Object.values(get(queue)), (file) => (file.progress.uploadComplete === true ? Result.failVoid : Result.succeed(file.meta.destination)))).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Destination, nextDestination, pendingUploads, preprocessing, queue, uppy };
