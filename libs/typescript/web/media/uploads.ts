import type { Body, Meta, Uppy, UppyFile } from '@uppy/core';
import { Effect, Formatter, Option, Schema } from 'effect';
import { Atom } from 'effect/reactivity';

// --- [TYPES] ---------------------------------------------------------------------------

interface Preparation<M extends Meta, B extends Body, E extends Error> {
    readonly message: string;
    readonly prepared: (file: UppyFile<M, B>) => boolean;
    readonly prepare: (file: UppyFile<M, B>, data: Blob) => Effect.Effect<M, E>;
    readonly reject: (error: E) => Option.Option<string>;
}

// --- [ERRORS] --------------------------------------------------------------------------

const PrepareReason = Schema.TaggedUnion({ Unloaded: { cause: Schema.Defect() } });
class PrepareError extends Schema.TaggedError<PrepareError, { readonly brand: unique symbol }>()('PrepareError', { reason: PrepareReason }) {
    override get message(): string {
        return PrepareReason.match(this.reason, { Unloaded: ({ cause }) => `the upload preparation code could not load: ${Formatter.format(cause)}` });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const removed = <M extends Meta, B extends Body>(uploader: Uppy<M, B>, id: string): Effect.Effect<void> =>
    Effect.callback((resume) => {
        const listener = (file: UppyFile<M, B>): void => {
            if (file.id === id) {
                resume(Effect.void);
            }
        };
        uploader.on('file-removed', listener);
        return Effect.sync(() => uploader.off('file-removed', listener));
    });
const prepareFile = <M extends Meta, B extends Body, E extends Error>(uploader: Uppy<M, B>, id: string, { message, prepared, prepare, reject }: Preparation<M, B, E>): Effect.Effect<void> =>
    Option.match(Option.fromNullishOr(uploader.getFile(id)), {
        onNone: () => Effect.void,
        onSome: (file) => {
            const complete = (): void => uploader.emit('preprocess-complete', uploader.getFile(id));
            if (prepared(file) || !(file.data instanceof Blob)) {
                return Effect.sync(complete);
            }
            return Effect.andThen(
                Effect.sync(() => uploader.emit('preprocess-progress', file, { mode: 'indeterminate', message })),
                prepare(file, file.data),
            ).pipe(
                Effect.matchEffect({
                    onSuccess: (meta) =>
                        Effect.sync(() => {
                            uploader.setFileMeta(id, meta);
                            complete();
                        }),
                    onFailure: (error) =>
                        Effect.sync(() =>
                            Option.match(reject(error), {
                                onNone: () => {
                                    uploader.emit('upload-error', file, error);
                                    complete();
                                },
                                onSome: (reason) => {
                                    uploader.info(`${file.name}: ${reason}`, 'error', uploader.opts.infoTimeout);
                                    uploader.removeFile(id);
                                },
                            }),
                        ),
                }),
                Effect.race(removed(uploader, id)),
            );
        },
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const queuedFiles = <M extends Meta, B extends Body>(uploads: Atom.Atom<Uppy<M, B>>): Atom.Atom<readonly UppyFile<M, B>[]> =>
    Atom.make((get): readonly UppyFile<M, B>[] => {
        const uploader = get(uploads);
        const update = (): void => get.setSelf(uploader.getFiles());
        (['files-added', 'file-removed', 'upload-start', 'upload-success'] as const).forEach((event) => {
            uploader.on(event, update);
            get.addFinalizer(() => uploader.off(event, update));
        });
        return uploader.getFiles();
    });
const preprocess = <M extends Meta, B extends Body, E extends Error>(uploader: Uppy<M, B>, { concurrency, ...preparation }: Preparation<M, B, E> & { readonly concurrency: number }): void =>
    uploader.addPreProcessor((ids) => Effect.runPromise(Effect.forEach(ids, (id) => prepareFile(uploader, id, preparation), { concurrency, discard: true })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { PrepareError, preprocess, queuedFiles };
