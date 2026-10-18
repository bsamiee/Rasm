import type { Body, Meta, Uppy, UppyFile } from '@uppy/core';
import { Deferred, Effect, Option } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [COMPOSITION] ---------------------------------------------------------------------

const files = <M extends Meta, B extends Body>(uploads: Atom.Atom<Uppy<M, B>>) =>
    Atom.make((get) => {
        const uploader = get(uploads);
        get.addFinalizer(uploader.store.subscribe((_, next) => get.setSelf(next.files)));
        return uploader.getState().files;
    });
const preprocess = <M extends Meta, B extends Body, E extends Error>(
    uploader: Uppy<M, B>,
    prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<M, E>,
    { concurrency, message, prepared, reject }: { readonly concurrency: number; readonly message: string; readonly prepared: (file: UppyFile<M, B>) => boolean; readonly reject: (error: E) => Option.Option<string> },
) =>
    uploader.addPreProcessor((ids) =>
        Effect.runPromise(
            Effect.forEach(
                ids,
                Effect.fnUntraced(function* (id: string) {
                    const file: UppyFile<M, B> | undefined = uploader.getFile(id);
                    if (file?.data instanceof Blob && !prepared(file)) {
                        const removed = yield* Deferred.make<void>();
                        const listener = (item: UppyFile<M, B>) => item.id === id && Deferred.doneUnsafe(removed, Effect.void);
                        uploader.on('file-removed', listener).emit('preprocess-progress', file, { mode: 'indeterminate', message });
                        yield* prepare(file, file.data).pipe(
                            Effect.matchEffect({
                                onSuccess: (meta) => Effect.sync(() => uploader.setFileMeta(id, meta)),
                                onFailure: (error) =>
                                    Effect.sync(() =>
                                        Option.match(reject(error), {
                                            onNone: () => uploader.emit('upload-error', file, error),
                                            onSome: (reason) => {
                                                uploader.info(`${file.name}: ${reason}`, 'error', uploader.opts.infoTimeout);
                                                uploader.removeFile(id);
                                            },
                                        }),
                                    ),
                            }),
                            Effect.raceFirst(Deferred.await(removed)),
                            Effect.ensuring(Effect.sync(() => uploader.off('file-removed', listener).emit('preprocess-complete', file))),
                        );
                    }
                }),
                { concurrency, discard: true },
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { files, preprocess };
