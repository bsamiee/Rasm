import { BrowserCrypto } from '@effect/platform-browser';
import { PrepareError, preprocess, queuedFiles as queue } from '@rasm/media/uploads.ts';
import Uppy, { type Body, type Meta } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, Crypto, Effect, Equivalence, Function, Match, Optic, Record, Schema, Struct } from 'effect';
import { AsyncResult, Atom } from 'effect/reactivity';
import { Asset, mediaTypes, PdfAsset, uploadLimit } from '../model/asset.ts';
import type { PortfolioData } from '../model/document.ts';
import { type Composition, createComposition, placementsFor } from '../model/placement.ts';
import { draft } from './services.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Destination = { readonly kind: 'library' } | { readonly kind: 'hero' } | { readonly kind: 'entry'; readonly id: string };
interface Prepared {
    readonly asset: typeof Asset.Type;
    readonly compositions: readonly (typeof Composition.Type)[];
}
interface UploadMeta extends Meta {
    destination: Destination;
    asset?: string;
    prepared?: Prepared;
    renditions?: readonly File[];
}
interface Uploaded extends Prepared {
    readonly destination: Destination;
    readonly following: ReadonlySet<string>;
}

// --- [SERVICES] ------------------------------------------------------------------------

const destination = Atom.make<Destination>({ kind: 'library' }).pipe(Atom.keepAlive);
const uploads = Atom.make((get): Uppy<UploadMeta, Body> => {
    const uploader = new Uppy<UploadMeta, Body>({
        restrictions: { maxFileSize: uploadLimit, allowedFileTypes: [...mediaTypes] },
    }).use(XHRUpload, { endpoint: '/api/media', allowedMetaFields: ['asset', 'renditions'], limit: 3 });
    const beforeFileAdded = uploader.opts.onBeforeFileAdded;
    uploader.setOptions({
        onBeforeFileAdded: (file, files) => {
            if (beforeFileAdded(file, files) === false) {
                return false;
            }
            const target = get.registry.get(destination);
            if (target.kind === 'hero' && Object.values(files).some((item) => item.meta.destination.kind === 'hero' && item.progress.uploadComplete !== true)) {
                uploader.info('Finish or remove the queued hero file before adding another.', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    });
    preprocess(uploader, {
        concurrency: 2,
        message: 'Reading file',
        prepared: (file) => file.meta.prepared !== undefined,
        prepare: (file, data) => prepare(file.meta.destination, data, file.name, file.type).pipe(Effect.provide(BrowserCrypto.layer)),
        reject: (error) =>
            Match.value(error).pipe(
                Match.when({ _tag: 'PdfError', reason: { _tag: 'Protected' } }, () => 'This PDF is password protected. Add an unlocked export.'),
                Match.when({ _tag: 'PdfError', reason: { _tag: 'Invalid' } }, () => 'This PDF could not be read. Add a fresh PDF export.'),
                Match.when({ _tag: 'InspectError', reason: { _tag: 'Unplayable' } }, () => 'This browser cannot play the video. Export it as H.264 MP4 or VP9 WebM.'),
                Match.option,
            ),
    });
    uploader.on('upload-success', (file) => {
        if (file?.meta.prepared !== undefined) {
            const order = file.meta.prepared.compositions.length > 0 ? uploader.getFiles() : [];
            const following = new Set(order.slice(order.findIndex((item) => item.id === file.id) + 1).flatMap((item) => item.meta.prepared?.compositions.map(Struct.get('id')) ?? []));
            get.registry.update(draft, AsyncResult.map(receive({ ...file.meta.prepared, destination: file.meta.destination, following })));
            uploader.setFileMeta(file.id, { ...file.meta, renditions: [] });
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);
const queuedFiles = queue(uploads);
const pendingUploads = Atom.make((get): readonly Destination[] =>
    get(queuedFiles)
        .filter((file) => file.progress.uploadComplete !== true)
        .map((file) => file.meta.destination),
).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

// --- [OPERATIONS] ----------------------------------------------------------------------

const prepare = Effect.fnUntraced(function* (target: Destination, data: Blob, name: string, mime: string) {
    const crypto = yield* Crypto.Crypto;
    const { inspectFile } = yield* Effect.tryPromise({ try: () => import('../media/inspect.ts'), catch: (cause) => PrepareError.make({ reason: { _tag: 'Unloaded', cause } }) });
    const { asset, renditions } = yield* inspectFile(data, yield* crypto.randomUUIDv4(), name, mime);
    const compositions = yield* target.kind === 'entry' && !Schema.is(PdfAsset)(asset) ? Effect.forEach(placementsFor(asset, true), (placement) => Effect.map(crypto.randomUUIDv4(), (key) => createComposition(key, placement))) : Effect.succeed([]);
    const prepared: Prepared = { asset, compositions };
    return { destination: target, asset: yield* Schema.encodeEffect(Schema.fromJsonString(Asset))(asset), prepared, renditions } satisfies UploadMeta;
});
const place = (current: readonly (typeof Composition.Type)[], { compositions, following }: Pick<Uploaded, 'compositions' | 'following'>): readonly (typeof Composition.Type)[] => {
    const [before, after] = Array.span(current, (item) => !following.has(item.id));
    return [...before, ...compositions, ...after];
};
const receive = ({ asset, destination: target, ...uploaded }: Uploaded): ((data: typeof PortfolioData.Type) => typeof PortfolioData.Type) => {
    const data = Optic.id<typeof PortfolioData.Type>();
    const stored = data.key('assets').modify(Record.set(asset.id, asset));
    if (target.kind === 'hero') {
        return Function.flow(stored, Optic.replace(data.key('portfolio').optionalKey('hero'), Array.headNonEmpty(placementsFor(asset, false))));
    }
    if (target.kind === 'entry' && !Schema.is(PdfAsset)(asset)) {
        return Function.flow(
            stored,
            data
                .key('portfolio')
                .key('entries')
                .modify(Array.map((entry) => (entry.id === target.id ? { ...entry, compositions: place(entry.compositions, uploaded) } : entry))),
        );
    }
    return stored;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Destination, destination, pendingUploads, queuedFiles, uploads };
