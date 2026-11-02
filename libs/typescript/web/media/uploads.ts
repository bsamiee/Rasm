import type { Body, Meta, State, Uppy, UppyEventMap, UppyFile } from '@uppy/core';
import { Cause, Effect, Option } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const uppyState = <M extends Meta, B extends Body, A>(uploads: Atom.Atom<Uppy<M, B>>, select: (state: State<M, B>) => A) =>
    Atom.make((get) => {
        const uploader = get(uploads);
        get.addFinalizer(uploader.store.subscribe((_, next) => get.setSelf(select(next))));
        return select(uploader.getState());
    });
const preprocess = <M extends Meta, B extends Body, E extends Parameters<UppyEventMap<M, B>['upload-error']>[1]>(uploader: Uppy<M, B>, prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<Partial<M>, E>, { concurrency, message }: { readonly concurrency: number; readonly message: string }) => {
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
                  Effect.match({ onSuccess: (meta) => Option.map(Option.fromNullishOr(uploader.getState().files[file.id]), (current) => uploader.setFileMeta(file.id, { ...current.meta, ...meta })), onFailure: (error) => uploader.emit('upload-error', file, error) }),
                  Effect.raceFirst(removed(file)),
                  Effect.ensuring(Effect.sync(() => uploader.emit('preprocess-complete', file))),
              )
            : Effect.void;
    uploader.addPreProcessor((ids) => Effect.runPromise(Effect.forEach(ids, (id) => Effect.suspend(() => prepared(uploader.getState().files[id])), { concurrency, discard: true })));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { preprocess, uppyState };
