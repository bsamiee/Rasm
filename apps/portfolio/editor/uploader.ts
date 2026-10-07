import { BrowserCrypto } from '@effect/platform-browser';
import Uppy, { type Body, type Meta, type UppyFile } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, Crypto, Effect, Match, Struct } from 'effect';
import { AsyncResult, Atom } from 'effect/reactivity';
import { inspectFile } from '../media/inspect.ts';
import { type Asset, mediaTypes, uploadLimit } from '../model/asset.ts';
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
        onBeforeFileAdded: (file, files) => {
            const target = get.registry.get(destination);
            if (target.kind === 'hero' && Object.values(files).some((item) => item.meta.destination.kind === 'hero' && !item.progress.uploadComplete)) {
                uploader.info('Finish or remove the queued hero file before adding another.', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    }).use(XHRUpload, { endpoint: '/api/media', allowedMetaFields: ['asset'], limit: 3 });
    uploader.addPreProcessor((ids) => Effect.forEach(ids, (id) => prepare(uploader, id), { concurrency: 2, discard: true }).pipe(Effect.provide(BrowserCrypto.layer), Effect.runPromise));
    uploader.on('upload-success', (file) => {
        const prepared = file?.meta.prepared;
        const queue = uploader.getFiles();
        const following = new Set(queue.slice(queue.findIndex((item) => item.id === file?.id) + 1).flatMap((item) => item.meta.prepared?.compositions.map(Struct.get('id')) ?? []));
        if (file && prepared) {
            get.registry.update(
                draft,
                AsyncResult.map((data) => receive(data, { ...prepared, destination: file.meta.destination, following })),
            );
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);

// --- [OPERATIONS] ----------------------------------------------------------------------

const removed = (uploader: Uppy<UploadMeta, Body>, id: string): Effect.Effect<void> =>
    Effect.callback((resume) => {
        const listener = (file: UppyFile<UploadMeta, Body>): void => {
            if (file.id === id) {
                resume(Effect.void);
            }
        };
        uploader.on('file-removed', listener);
        return Effect.sync(() => uploader.off('file-removed', listener));
    });
const prepare = Effect.fnUntraced(function* (uploader: Uppy<UploadMeta, Body>, id: string) {
    const file = uploader.getFile(id);
    const { data } = file;
    const complete = (): void => uploader.emit('preprocess-complete', uploader.getFile(id));
    if (file.meta.prepared || !(data instanceof Blob)) {
        return complete();
    }
    const crypto = yield* Crypto.Crypto;
    const inspected = Effect.gen(function* () {
        const asset = yield* inspectFile(data, yield* crypto.randomUUIDv4, file.name);
        const compositions = yield* Effect.forEach(placementsFor(asset), (placement) => Effect.map(crypto.randomUUIDv4, (key) => createComposition(key, placement)));
        return { asset, compositions } satisfies Prepared;
    });
    uploader.emit('preprocess-progress', file, { mode: 'indeterminate', message: 'Reading file' });
    yield* inspected.pipe(
        Effect.match({
            onSuccess: (prepared) => {
                uploader.setFileMeta(id, { destination: file.meta.destination, asset: JSON.stringify(prepared.asset), prepared });
                complete();
            },
            onFailure: (error) => {
                uploader.emit('upload-error', file, error);
                complete();
            },
        }),
        Effect.race(removed(uploader, id)),
    );
});
const place = (current: readonly (typeof Composition.Type)[], { compositions, following }: Uploaded): readonly (typeof Composition.Type)[] => {
    const [before, after] = Array.span(current, (item) => !following.has(item.id));
    return [...before, ...compositions, ...after];
};
const receive = (data: typeof PortfolioData.Type, uploaded: Uploaded): typeof PortfolioData.Type => ({
    assets: { ...data.assets, [uploaded.asset.id]: uploaded.asset },
    portfolio: Match.value(uploaded.destination).pipe(
        Match.when({ kind: 'library' }, () => data.portfolio),
        Match.when({ kind: 'hero' }, () => ({ ...data.portfolio, hero: Array.headNonEmpty(placementsFor(uploaded.asset)) })),
        Match.when({ kind: 'entry' }, ({ id }) => ({ ...data.portfolio, entries: data.portfolio.entries.map((entry) => (entry.id === id ? { ...entry, compositions: place(entry.compositions, uploaded) } : entry)) })),
        Match.exhaustive,
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Destination, destination, type UploadMeta, uploads };
