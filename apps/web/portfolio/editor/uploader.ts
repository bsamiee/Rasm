import { files, preprocess } from '@rasm/media/uploads.ts';
import Uppy, { type Body, type Meta } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, Effect, Equivalence, Function, Match, Optic, Record, Schema, Struct } from 'effect';
import { AsyncResult, Atom } from 'effect/reactivity';
import { Asset, mediaTypes, PdfAsset, uploadLimit } from '../model/asset.ts';
import type { PortfolioData } from '../model/document.ts';
import { type Composition, createComposition, placementsFor } from '../model/placement.ts';
import { draft, identifier } from './services.ts';

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
const uploads = Atom.make((get) => {
    const uploader: Uppy<UploadMeta, Body> = new Uppy<UploadMeta, Body>({
        restrictions: { maxFileSize: uploadLimit, allowedFileTypes: [...mediaTypes] },
        onBeforeFileAdded: (file, queued) => {
            const target = get.registry.get(destination);
            if (target.kind === 'hero' && Object.values(queued).some((item) => item.meta.destination.kind === 'hero' && item.progress.uploadComplete !== true)) {
                uploader.info('Finish or remove the queued hero file before adding another.', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    }).use(XHRUpload, { endpoint: '/api/media', allowedMetaFields: ['asset', 'renditions'], limit: 3 });
    preprocess(
        uploader,
        Effect.fnUntraced(function* ({ meta: { destination: target }, name, type }, data) {
            const { inspectFile } = yield* Effect.tryPromise(() => import('../media/inspect.ts'));
            const { asset, renditions } = yield* inspectFile(data, yield* identifier, name, type);
            const compositions = yield* Effect.forEach(target.kind === 'entry' && !Schema.is(PdfAsset)(asset) ? placementsFor(asset) : [], (placement) => Effect.zipWith(identifier, Effect.succeed(placement), createComposition));
            return { destination: target, asset: yield* Schema.encodeEffect(Schema.fromJsonString(Asset))(asset), prepared: { asset, compositions }, renditions };
        }),
        {
            concurrency: 2,
            message: 'Reading file',
            prepared: (file) => file.meta.prepared !== undefined,
            reject: (error) =>
                Match.value(error).pipe(
                    Match.tag('InspectError', ({ reason }) =>
                        Match.valueTags(reason, {
                            Protected: () => 'This PDF is password protected. Add an unlocked export.',
                            Invalid: () => 'This PDF could not be read. Add a fresh PDF export.',
                            Unplayable: () => 'This browser cannot play the video. Export it as H.264 MP4 or VP9 WebM.',
                        }),
                    ),
                    Match.option,
                ),
        },
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
            const data = Optic.id<typeof PortfolioData.Type>();
            const stored = data.key('assets').modify(Record.set(asset.id, asset));
            get.registry.update(
                draft,
                AsyncResult.map(
                    Match.value(file.meta.destination).pipe(
                        Match.discriminatorsExhaustive('kind')({
                            library: () => stored,
                            hero: () => Function.flow(stored, Optic.replace(data.key('portfolio').optionalKey('hero'), Array.headNonEmpty(placementsFor(asset)))),
                            entry: ({ id }) =>
                                Function.flow(
                                    stored,
                                    data
                                        .key('portfolio')
                                        .key('entries')
                                        .modify(Array.map((entry) => (entry.id === id ? { ...entry, compositions: [...Array.takeWhile(entry.compositions, preceding), ...compositions, ...Array.dropWhile(entry.compositions, preceding)] } : entry))),
                                ),
                        }),
                    ),
                ),
            );
            uploader.setFileMeta(file.id, { ...file.meta, renditions: [] });
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);
const queue = files(uploads);
const pendingUploads = Atom.make((get) => Object.values(get(queue)).flatMap((file) => (file.progress.uploadComplete === true ? [] : [file.meta.destination]))).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Destination, destination, pendingUploads, queue, uploads };
