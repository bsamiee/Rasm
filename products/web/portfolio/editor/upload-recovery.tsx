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
    } satisfies Record<Recovery['status'], { readonly label: string; readonly description: string }>;
    const queuedUploads = new Set(Array.filterMap(files, (file) => (file.meta.prepared !== undefined && file.progress.uploadComplete !== true ? Result.succeed(file.meta.prepared.asset.id) : Result.failVoid)));
    return (
        <section className="upload-recovery">
            <header>
                <h3>Upload recovery</h3>
                <button className="button button-ghost" disabled={recovery.waiting || discarded.waiting} onClick={refreshRecovery} type="button">
                    Refresh
                </button>
            </header>
            {discarded.waiting && <output className="hint">Removing upload data…</output>}
            {AsyncResult.builder(discarded)
                .onError(({ upload, error }) => (
                    <div>
                        <p className="file-name hint">{upload.name}</p>
                        <RequestAlert error={error} />
                    </div>
                ))
                .orNull()}
            {AsyncResult.builder(recovery)
                .onInitial(() => <output className="hint">Checking incomplete uploads…</output>)
                .onError((error) => <RequestAlert error={error} />)
                .onSuccess((value) =>
                    value.length === 0 ? (
                        <p className="hint">No incomplete uploads</p>
                    ) : (
                        <>
                            <p className="hint">Interrupted uploads stay private. Discard uploads missing from queue and add their files again</p>
                            <ul>
                                {value.map((upload) => (
                                    <li key={upload.id}>
                                        <span>{upload.name}</span>
                                        <ModalDialog.Confirm {...confirmation} description={joinDetails([upload.name, actions[upload.status].description])} isDisabled={discarded.waiting || queuedUploads.has(upload.id)} label={actions[upload.status].label} onConfirm={() => discard(upload)} />
                                        {queuedUploads.has(upload.id) && <p className="hint">Retry or remove file in upload queue first</p>}
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
