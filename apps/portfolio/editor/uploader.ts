import { BrowserCrypto } from '@effect/platform-browser';
import Uppy, { type Body, type Meta, type UppyFile } from '@uppy/core';
import XHRUpload from '@uppy/xhr-upload';
import { Array, Crypto, Effect, Equivalence, Function, Match, Struct } from 'effect';
import { AsyncResult, Atom } from 'effect/reactivity';
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
    renditions?: File[];
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
            if (target.kind === 'hero' && Object.values(files).some((item) => item.meta.destination.kind === 'hero' && !item.progress.uploadComplete)) {
                uploader.info('Finish or remove the queued hero file before adding another.', 'info');
                return false;
            }
            return { ...file, meta: { ...file.meta, destination: target } };
        },
    });
    uploader.addPreProcessor((ids) => Effect.forEach(ids, (id) => prepare(uploader, id), { concurrency: 2, discard: true }).pipe(Effect.provide(BrowserCrypto.layer), Effect.runPromise));
    uploader.on('upload-success', (file) => {
        const prepared = file?.meta.prepared;
        if (file && prepared) {
            const queue = prepared.compositions.length > 0 ? uploader.getFiles() : [];
            const following = new Set(queue.slice(queue.findIndex((item) => item.id === file.id) + 1).flatMap((item) => item.meta.prepared?.compositions.map(Struct.get('id')) ?? []));
            get.registry.update(
                draft,
                AsyncResult.map((data) => receive(data, { ...prepared, destination: file.meta.destination, following })),
            );
            uploader.setFileMeta(file.id, { ...file.meta, renditions: [] });
        }
    });
    get.addFinalizer(() => uploader.destroy());
    return uploader;
}).pipe(Atom.keepAlive);
const queuedFiles = Atom.make((get): readonly UppyFile<UploadMeta, Body>[] => {
    const uploader = get(uploads);
    const update = (): void => get.setSelf(uploader.getFiles());
    (['files-added', 'file-removed', 'upload-start', 'upload-success'] as const).forEach((event) => {
        uploader.on(event, update);
        get.addFinalizer(() => uploader.off(event, update));
    });
    return uploader.getFiles();
});
const pendingUploads = Atom.make((get): readonly Destination[] =>
    get(queuedFiles)
        .filter((file) => !file.progress.uploadComplete)
        .map((file) => file.meta.destination),
).pipe(Atom.withEquality(Equivalence.Array(Equivalence.strictEqual<Destination>())));

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
    if (!file) {
        return;
    }
    const { data } = file;
    const complete = (): void => uploader.emit('preprocess-complete', uploader.getFile(id));
    if (file.meta.prepared || !(data instanceof Blob)) {
        return complete();
    }
    const crypto = yield* Crypto.Crypto;
    const inspected = Effect.gen(function* () {
        const { inspectFile } = yield* Effect.tryPromise(() => import('../media/inspect.ts'));
        const { asset, renditions } = yield* inspectFile(data, yield* crypto.randomUUIDv4, file.name, file.type);
        const compositions = yield* file.meta.destination.kind === 'entry' && asset.mime !== 'application/pdf' ? Effect.forEach(placementsFor(asset, true), (placement) => Effect.map(crypto.randomUUIDv4, (key) => createComposition(key, placement))) : Effect.succeed([]);
        return { prepared: { asset, compositions } satisfies Prepared, renditions };
    });
    uploader.emit('preprocess-progress', file, { mode: 'indeterminate', message: 'Reading file' });
    yield* inspected.pipe(
        Effect.matchEffect({
            onSuccess: ({ prepared, renditions }) =>
                Effect.sync(() => {
                    uploader.setFileMeta(id, { destination: file.meta.destination, asset: JSON.stringify(prepared.asset), prepared, renditions });
                    complete();
                }),
            onFailure: (error) =>
                Effect.promise(async () => {
                    const { PasswordException, InvalidPDFException } = await import('pdfjs-dist');
                    uploader.emit('upload-error', file, {
                        name: error.name,
                        message: Match.value(error.cause).pipe(
                            Match.when(Match.instanceOf(PasswordException), Function.constant('This PDF is password protected. Remove it and add an unlocked export.')),
                            Match.when(Match.instanceOf(InvalidPDFException), Function.constant('This PDF could not be read. Remove it and add a fresh PDF export.')),
                            // ast-grep-ignore: no-rendered-cause-tsx -- Uppy upload-error requires the final display message at this boundary.
                            Match.orElse(Function.constant(error.message)),
                        ),
                    });
                    complete();
                }),
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
        Match.when({ kind: 'hero' }, () => ({ ...data.portfolio, hero: Array.headNonEmpty(placementsFor(uploaded.asset, false)) })),
        Match.when({ kind: 'entry' }, ({ id }) => (uploaded.asset.mime === 'application/pdf' ? data.portfolio : { ...data.portfolio, entries: data.portfolio.entries.map((entry) => (entry.id === id ? { ...entry, compositions: place(entry.compositions, uploaded) } : entry)) })),
        Match.exhaustive,
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Destination, destination, pendingUploads, queuedFiles, uploads };
