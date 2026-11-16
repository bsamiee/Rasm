import { useAtom, useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { ModalDialog } from '@rasm/ui';
import { Array, Result } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { RequestAlert } from '#/editor/request-alert.tsx';
import { discardRequest, recoveryRequest } from '#/editor/state';
import { queue } from '#/editor/uploader';
import { confirmation, joinDetails } from '#/media/labels';
import type { Recovery } from '#/model/api';

// --- [COMPOSITION] ---------------------------------------------------------------------

const UploadRecovery = () => {
    const files = Object.values(useAtomValue(queue));
    const recovery = useAtomValue(recoveryRequest);
    const refreshRecovery = useAtomRefresh(recoveryRequest);
    const [discarded, discard] = useAtom(discardRequest);
    const actions = {
        incomplete: { label: 'Discard incomplete upload', description: 'Deletes partial upload data. Add original file again to upload it' },
        cleanup: { label: 'Remove leftover upload data', description: 'Uploaded file and its placements stay' },
        removing: { label: 'Finish removal', description: 'Deletes remaining upload data of removed file' },
    } satisfies Record<(typeof Recovery.Type)['status'], { readonly label: string; readonly description: string }>;
    const queuedUploads = new Set(Array.filterMap(files, (file) => (file.meta.prepared !== undefined && file.progress.uploadComplete !== true ? Result.succeed(file.meta.prepared.asset.id) : Result.failVoid)));
    return (
        <section className="border-t border-line pt-5">
            <div className="flex items-center justify-between gap-3">
                <h3 className="text-lg tracking-tight">Upload recovery</h3>
                <button className="button button-ghost" disabled={recovery.waiting || discarded.waiting} onClick={refreshRecovery} type="button">
                    Refresh
                </button>
            </div>
            {discarded.waiting && <output className="mt-3 block hint">Removing upload data…</output>}
            {AsyncResult.builder(discarded)
                .onError(({ upload, error }) => (
                    <div className="mt-3">
                        <p className="mb-2 hint wrap-anywhere">{upload.name}</p>
                        <RequestAlert error={error} />
                    </div>
                ))
                .orNull()}
            {AsyncResult.builder(recovery)
                .onInitial(() => <output className="mt-3 block hint">Checking incomplete uploads…</output>)
                .onError((error) => <RequestAlert error={error} />)
                .onSuccess((value) =>
                    value.length === 0 ? (
                        <p className="mt-3 hint">No incomplete uploads</p>
                    ) : (
                        <>
                            <p className="mt-3 hint">Interrupted uploads stay private. Discard uploads missing from queue and add their files again</p>
                            <ul className="mt-3 divide-y divide-line">
                                {value.map((upload) => (
                                    <li className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1 py-3" key={upload.id}>
                                        <span className="min-w-0 flex-1 text-sm wrap-anywhere">{upload.name}</span>
                                        <ModalDialog.Confirm {...confirmation} description={joinDetails([upload.name, actions[upload.status].description])} isDisabled={discarded.waiting || queuedUploads.has(upload.id)} label={actions[upload.status].label} onConfirm={() => discard(upload)} />
                                        {queuedUploads.has(upload.id) && <p className="w-full hint">Retry or remove file in upload queue first</p>}
                                    </li>
                                ))}
                            </ul>
                        </>
                    ),
                )
                .render()}
        </section>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { UploadRecovery };
