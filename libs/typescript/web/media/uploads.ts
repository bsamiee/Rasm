import type { Body, Meta, Uppy, UppyFile } from '@uppy/core';
import { Array, Deferred, Effect } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [COMPOSITION] ---------------------------------------------------------------------

const files = <M extends Meta, B extends Body>(uploads: Atom.Atom<Uppy<M, B>>) =>
    Atom.make((get) => {
        const uploader = get(uploads);
        get.addFinalizer(uploader.store.subscribe((_, next) => get.setSelf(next.files)));
        return uploader.getState().files;
    });
const preprocess = <M extends Meta, B extends Body, E extends { readonly name: string; readonly message: string }>(uploader: Uppy<M, B>, prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<Partial<M>, E>, { concurrency, message }: { readonly concurrency: number; readonly message: string }) =>
    uploader.addPreProcessor((ids) =>
        Effect.runPromise(
            Effect.forEach(
                Array.flatMap(uploader.getFilesByIds(ids), (file) => (file.data instanceof Blob && file.progress.uploadStarted === null ? [[file, file.data] as const] : [])),
                Effect.fnUntraced(function* ([file, data]) {
                    const removed = yield* Deferred.make<void>();
                    const listener = (item: UppyFile<M, B>) => item.id === file.id && Deferred.doneUnsafe(removed, Effect.void);
                    uploader.on('file-removed', listener).emit('preprocess-progress', file, { mode: 'indeterminate', message });
                    yield* prepare(file, data).pipe(
                        Effect.match({ onSuccess: (meta) => uploader.setFileMeta(file.id, { ...file.meta, ...meta }), onFailure: (error) => uploader.emit('upload-error', file, error) }),
                        Effect.raceFirst(Deferred.await(removed)),
                        Effect.ensuring(Effect.sync(() => uploader.off('file-removed', listener).emit('preprocess-complete', file))),
                    );
                }),
                { concurrency, discard: true },
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { files, preprocess };
