import { files, preprocess } from '@rasm/media';
import Uppy, { type Body, type Meta } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, Cause, Effect, Equivalence, Function, Match, Optic, Record, Schema, Struct } from 'effect';
import { HttpApiClient } from 'effect/http-api';
import { AsyncResult, Atom } from 'effect/reactivity';
import { assets, draft, hero, identifier, pendingRequest, updateEntry } from '#/editor/state';
import { Api } from '#/model/api';
import { Asset, mediaTypes, PdfAsset, uploadLimit } from '#/model/asset';
import { Composition, placementsFor } from '#/model/placement';

// --- [TYPES] ---------------------------------------------------------------------------

type Destination = { readonly kind: 'library' } | { readonly kind: 'hero' } | { readonly kind: 'entry'; readonly id: string };
interface UploadMeta extends Meta {
    destination: Destination;
    asset?: string;
    prepared?: { readonly asset: typeof Asset.Type; readonly compositions: readonly (typeof Composition.Type)[] };
    renditions?: readonly File[];
}

// --- [SERVICES] ------------------------------------------------------------------------

const destination = Atom.make<Destination>({ kind: 'library' }).pipe(Atom.keepAlive);
const uppy = Atom.make((get) => {
    const uploader: Uppy<UploadMeta, Body> = new Uppy<UploadMeta, Body>({
        restrictions: { maxFileSize: uploadLimit, allowedFileTypes: [...mediaTypes] },
        onBeforeFileAdded: (file, queued) => {
            const target = get.registry.get(destination);
            if (target.kind === 'hero' && Object.values(queued).some((item) => item.meta.destination.kind === 'hero' && item.progress.uploadComplete !== true)) {
                uploader.info('Finish or remove queued hero file first', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    }).use(XHRUpload, { endpoint: HttpApiClient.urlBuilder(Api).media.upload(), allowedMetaFields: ['asset', 'renditions'], limit: 3 });
    preprocess(
        uploader,
        Effect.fnUntraced(function* ({ meta: { destination: target }, name, type }, data) {
            const { inspectFile } = yield* Effect.tryPromise({ try: () => import('#/editor/inspect-file'), catch: (cause) => new Cause.UnknownError(cause, 'File reader failed to load. Retry file') });
            const { asset, renditions } = yield* inspectFile({ data, id: yield* identifier, name, mime: type });
            const compositions = yield* Effect.forEach(
                target.kind === 'entry' && !Schema.is(PdfAsset)(asset) ? placementsFor(asset) : [],
                Effect.fnUntraced(function* (placement) {
                    return Composition.make({ id: yield* identifier, items: [placement] });
                }),
            );
            return { asset: yield* Schema.encodeEffect(Schema.fromJsonString(Asset))(asset), prepared: { asset, compositions }, renditions };
        }),
        { concurrency: 2, message: 'Reading file' },
    );
    uploader.on('upload-success', (file) => {
        if (file?.meta.prepared !== undefined) {
            const { asset, compositions } = file.meta.prepared;
            const queued = uploader.getFiles();
            const following = new Set(
                queued
                    .slice(queued.findIndex(({ id }) => id === file.id) + 1)
                    .flatMap((item) => item.meta.prepared?.compositions ?? [])
                    .map(Struct.get('id')),
            );
            const preceding = (item: typeof Composition.Type) => !following.has(item.id);
            const stored = assets.modify(Record.set(asset.id, asset));
            get.registry.update(
                draft,
                AsyncResult.map(
                    Match.value(file.meta.destination).pipe(
                        Match.discriminatorsExhaustive('kind')({
                            library: () => stored,
                            hero: () => Function.flow(stored, Optic.replace(hero, Array.headNonEmpty(placementsFor(asset)))),
                            entry: ({ id }) => Function.flow(stored, updateEntry(id, Struct.evolve({ compositions: (current) => [...Array.takeWhile(current, preceding), ...compositions, ...Array.dropWhile(current, preceding)] }))),
                        }),
                    ),
                ),
            );
            uploader.setFileMeta(file.id, { ...file.meta, renditions: [] });
        }
    });
    uploader.on('complete', () => get.registry.refresh(pendingRequest));
    uploader.on('file-removed', (file) => {
        if (file.meta.prepared !== undefined && file.progress.uploadComplete !== true) {
            get.registry.refresh(pendingRequest);
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);
const queue = files(uppy);
const pendingUploads = Atom.make((get) => Object.values(get(queue)).flatMap((file) => (file.progress.uploadComplete === true ? [] : [file.meta.destination]))).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Destination, destination, pendingUploads, queue, uppy };
