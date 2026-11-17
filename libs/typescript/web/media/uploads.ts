import type { Body, Meta, State, Uppy, UppyEventMap, UppyFile } from '@uppy/core';
import { Cause, Effect, FiberSet, type Types } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const selectUploadState = <M extends Meta, B extends Body, A>(uploader: Atom.Atom<Uppy<M, B>>, select: (state: State<M, B>) => A) =>
    Atom.make((get) => {
        const current = get(uploader);
        get.addFinalizer(current.store.subscribe((_, next) => get.setSelf(select(next))));
        return select(current.getState());
    });
const processUploads = Effect.fn('processUploads')(function* <M extends Meta, B extends Body, E extends Parameters<UppyEventMap<M, B>['upload-error']>[1], R>(
    uploader: Uppy<M, B>,
    { prepare, finish }: { readonly prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<Partial<M>, E, R>; readonly finish: (file: UppyFile<M, B>) => Effect.Effect<void, E, R> },
    { concurrency, message }: { readonly concurrency: Types.Concurrency; readonly message: string },
) {
    const run = yield* FiberSet.makeRuntimePromise<R>();
    const removed = (file: UppyFile<M, B>) =>
        Effect.callback<void>((resume) => {
            const listener = (item: UppyFile<M, B>) => {
                if (item.id === file.id) {
                    resume(unsubscribe);
                }
            };
            const unsubscribe = Effect.sync(() => {
                uploader.off('file-removed', listener);
            });
            uploader.on('file-removed', listener);
            return unsubscribe;
        });
    const prepared = (file: UppyFile<M, B> | undefined) =>
        file?.data instanceof Blob && file.progress.uploadStarted === null
            ? Effect.sync(() => uploader.emit('preprocess-progress', file, { mode: 'indeterminate', message })).pipe(
                  Effect.andThen(prepare(file, file.data)),
                  Effect.catchDefect((defect) => new Cause.UnknownError(defect)),
                  Effect.matchEffect({
                      onSuccess: (meta) => Effect.sync(() => uploader.setFileMeta(file.id, { ...file.meta, ...meta })),
                      onFailure: (error) => Effect.sync(() => uploader.emit('upload-error', file, error)),
                  }),
                  Effect.raceFirst(removed(file)),
                  Effect.ensuring(Effect.sync(() => uploader.emit('preprocess-complete', file))),
              )
            : Effect.void;
    const finished = (file: UppyFile<M, B> | undefined) =>
        file?.progress.uploadComplete === true && file.error === undefined
            ? Effect.sync(() => uploader.emit('postprocess-progress', file, { mode: 'indeterminate', message })).pipe(
                  Effect.andThen(finish(file)),
                  Effect.catchDefect((defect) => new Cause.UnknownError(defect)),
                  Effect.catch((error) => Effect.sync(() => uploader.emit('upload-error', file, error))),
                  Effect.ensuring(Effect.sync(() => uploader.emit('postprocess-complete', file))),
              )
            : Effect.void;
    const across = (each: typeof prepared) => (ids: string[]) => run(Effect.forEach(ids, (id) => each(uploader.getState().files[id]), { concurrency, discard: true }));
    const processors = [
        [uploader.addPreProcessor, uploader.removePreProcessor, across(prepared)],
        [uploader.addPostProcessor, uploader.removePostProcessor, across(finished)],
    ] as const;
    yield* Effect.forEach(processors, ([add, remove, processor]) => Effect.acquireRelease(Effect.sync(add.bind(uploader, processor)), () => Effect.sync(remove.bind(uploader, processor))));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { processUploads, selectUploadState };
