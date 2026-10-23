import { useAtom, useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { AsyncResult } from 'effect/reactivity';
import { ConfirmDialog } from '#/editor/confirm-dialog';
import { RequestAlert } from '#/editor/request-alert';
import { discardRequest, pendingRequest } from '#/editor/state';
import { queue } from '#/editor/uploader';
import type { PendingUpload } from '#/model/api';

// --- [COMPOSITION] ---------------------------------------------------------------------

const UploadRecovery = () => {
    const files = Object.values(useAtomValue(queue));
    const recovery = useAtomValue(pendingRequest);
    const refreshRecovery = useAtomRefresh(pendingRequest);
    const [discarded, discard] = useAtom(discardRequest);
    const actions = {
        incomplete: { label: 'Discard incomplete upload', description: 'Deletes partial upload data. Add original file again to upload it' },
        cleanup: { label: 'Remove leftover upload data', description: 'Uploaded file and its placements stay' },
        removing: { label: 'Finish removal', description: 'Deletes remaining upload data of removed file' },
    } satisfies Record<(typeof PendingUpload.Type)['status'], { readonly label: string; readonly description: string }>;
    const queuedUploads = new Set(files.flatMap((file) => (file.meta.prepared !== undefined && file.progress.uploadComplete !== true ? [file.meta.prepared.asset.id] : [])));
    return (
        <section className="border-line border-t pt-5">
            <div className="flex items-center justify-between gap-3">
                <h3 className="text-lg tracking-tight">Upload recovery</h3>
                <button className="button button-ghost" disabled={recovery.waiting || discarded.waiting} onClick={refreshRecovery} type="button">
                    Refresh
                </button>
            </div>
            {discarded.waiting && (
                <p className="hint mt-3" role="status">
                    Removing upload data…
                </p>
            )}
            {AsyncResult.builder(discarded)
                .onError(({ upload, error }) => (
                    <div className="mt-3">
                        <p className="hint mb-2 wrap-anywhere">{upload.name}</p>
                        <RequestAlert conflict="Upload changed in another request. Refresh list and retry" error={error} />
                    </div>
                ))
                .orNull()}
            {AsyncResult.builder(recovery)
                .onInitial(() => (
                    <p className="hint mt-3" role="status">
                        Checking incomplete uploads…
                    </p>
                ))
                .onError((error) => <RequestAlert error={error} />)
                .onSuccess((value) =>
                    value.length === 0 ? (
                        <p className="hint mt-3">No incomplete uploads</p>
                    ) : (
                        <>
                            <p className="hint mt-3">Interrupted uploads stay private. Discard uploads missing from queue and add their files again</p>
                            <ul className="divide-line mt-3 divide-y">
                                {value.map((upload) => (
                                    <li className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1 py-3" key={upload.id}>
                                        <span className="min-w-0 flex-1 text-sm wrap-anywhere">{upload.name}</span>
                                        <ConfirmDialog description={`${upload.name} · ${actions[upload.status].description}`} disabled={discarded.waiting || queuedUploads.has(upload.id)} label={actions[upload.status].label} onConfirm={() => discard(upload)} />
                                        {queuedUploads.has(upload.id) && <p className="hint w-full">Retry or remove file in upload queue first</p>}
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
