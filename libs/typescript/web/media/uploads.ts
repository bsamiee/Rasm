import type { Body, Meta, State, Uppy, UppyEventMap, UppyFile } from '@uppy/core';
import { Cause, Effect, FiberSet, Option, type Types } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const state = <M extends Meta, B extends Body, A>(uploader: Atom.Atom<Uppy<M, B>>, select: (state: State<M, B>) => A) =>
    Atom.make((get) => {
        const current = get(uploader);
        get.addFinalizer(current.store.subscribe((_, next) => get.setSelf(select(next))));
        return select(current.getState());
    });
const preprocess = Effect.fn('preprocess')(function* <M extends Meta, B extends Body, E extends Parameters<UppyEventMap<M, B>['upload-error']>[1], R>(
    uploader: Uppy<M, B>,
    prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<Partial<M>, E, R>,
    { concurrency, message }: { readonly concurrency: Types.Concurrency; readonly message: string },
) {
    const run = yield* FiberSet.makeRuntimePromise<R>();
    const removed = (file: UppyFile<M, B>) =>
        Effect.callback<void>((resume) => {
            const listener = (item: UppyFile<M, B>) => item.id === file.id && resume(Effect.void);
            uploader.on('file-removed', listener);
            return Effect.sync(() => uploader.off('file-removed', listener));
        });
    const prepared = (file: UppyFile<M, B> | undefined) =>
        file?.data instanceof Blob && file.progress.uploadStarted === null
            ? Effect.sync(() => uploader.emit('preprocess-progress', file, { mode: 'indeterminate', message })).pipe(
                  Effect.andThen(prepare(file, file.data)),
                  Effect.catchDefect((defect) => Effect.fail(new Cause.UnknownError(defect))),
                  Effect.matchEffect({
                      onSuccess: (meta) => Effect.sync(() => Option.map(Option.fromNullishOr(uploader.getState().files[file.id]), (current) => uploader.setFileMeta(file.id, { ...current.meta, ...meta }))),
                      onFailure: (error) => Effect.sync(() => uploader.emit('upload-error', file, error)),
                  }),
                  Effect.raceFirst(removed(file)),
                  Effect.ensuring(Effect.sync(() => uploader.emit('preprocess-complete', file))),
              )
            : Effect.void;
    const processor = (ids: string[]) => run(Effect.forEach(ids, (id) => Effect.suspend(() => prepared(uploader.getState().files[id])), { concurrency, discard: true }));
    yield* Effect.acquireRelease(
        Effect.sync(() => uploader.addPreProcessor(processor)),
        () => Effect.sync(() => uploader.removePreProcessor(processor)),
    );
});
const Uploads = { state, preprocess };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
