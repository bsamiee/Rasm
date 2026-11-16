import type { Body, Meta, State, Uppy, UppyEventMap, UppyFile } from '@uppy/core';
import { Cause, Effect, FiberSet, type Types } from 'effect';
import { HttpClient, HttpClientError, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { Atom } from 'effect/reactivity';

// --- [OPERATIONS] ----------------------------------------------------------------------

const selectUploadState = <M extends Meta, B extends Body, A>(uploader: Atom.Atom<Uppy<M, B>>, select: (state: State<M, B>) => A) =>
    Atom.make((get) => {
        const current = get(uploader);
        get.addFinalizer(current.store.subscribe((_, next) => get.setSelf(select(next))));
        return select(current.getState());
    });
const isTransientUpload = (xhr: XMLHttpRequest) => {
    const request = HttpClientRequest.post(xhr.responseURL);
    const reason = xhr.status === 0 ? new HttpClientError.TransportError({ request }) : new HttpClientError.StatusCodeError({ request, response: HttpClientResponse.fromWeb(request, new Response(null, { status: xhr.status, statusText: xhr.statusText })) });
    return HttpClient.isTransientError(new HttpClientError.HttpClientError({ reason }));
};
const preprocessUploads = Effect.fn('preprocessUploads')(function* <M extends Meta, B extends Body, E extends Parameters<UppyEventMap<M, B>['upload-error']>[1], R>(
    uploader: Uppy<M, B>,
    prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<Partial<M>, E, R>,
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
    const processor = (ids: string[]) => run(Effect.forEach(ids, (id) => prepared(uploader.getState().files[id]), { concurrency, discard: true }));
    yield* Effect.acquireRelease(
        Effect.sync(() => uploader.addPreProcessor(processor)),
        () => Effect.sync(uploader.removePreProcessor.bind(uploader, processor)),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { isTransientUpload, preprocessUploads, selectUploadState };
