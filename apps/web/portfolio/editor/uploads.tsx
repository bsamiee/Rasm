import { useAtom, useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { useUppyEvent, useUppyState } from '@uppy/react';
import Dashboard from '@uppy/react/dashboard';
import { Schema, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { useState } from 'react';
import '@uppy/core/css/style.min.css';
import '@uppy/dashboard/css/style.min.css';
import { byteSize, entryKind, entryTitle } from '../media/display.ts';
import type { PendingUpload } from '../model/api.ts';
import { type AssetCollection, mediaTypes, PdfAsset, uploadLimit } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { ConfirmDialog, MenuButton, RequestAlert, SelectField } from './controls.tsx';
import { discardRequest, pendingRequest } from './services.ts';
import { type Destination, destination, pendingUploads, queue, uploads } from './uploader.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = ({ assets, entries, onChoose }: { assets: typeof AssetCollection.Type; entries: readonly (typeof Entry.Type)[]; onChoose: (entryId: string, assetId: string) => void }) => {
    const uploader = useAtomValue(uploads);
    const files = Object.values(useAtomValue(queue));
    const pending = useAtomValue(pendingUploads);
    const recovery = useAtomValue(pendingRequest);
    const refreshRecovery = useAtomRefresh(pendingRequest);
    const [discarded, discard] = useAtom(discardRequest);
    useUppyEvent(uploader, 'complete', refreshRecovery);
    useUppyEvent(uploader, 'file-removed', (file) => {
        if (file.meta.prepared !== undefined && file.progress.uploadComplete !== true) {
            refreshRecovery();
        }
    });
    const batches = useUppyState(uploader, Struct.get('currentUploads'));
    const notices = useUppyState(uploader, Struct.get('info'));
    const [selected, setSelected] = useState<string>();
    const [target, setTarget] = useAtom(destination);
    const key = (value: Destination) => (value.kind === 'entry' ? value.id : value.kind);
    const options = [{ target: { kind: 'library' } as const, label: 'Private file library' }, { target: { kind: 'hero' } as const, label: 'Hero placement — one file' }, ...entries.map((entry) => ({ target: { kind: 'entry', id: entry.id } as const, label: `${entryTitle(entry)} · ${entryKind[entry.kind]}` }))].map(
        (option) => ({
            ...option,
            id: key(option.target),
        }),
    );
    const actions = {
        incomplete: { label: 'Discard incomplete upload', description: 'Discard the incomplete upload and its stored data. Add the original file again to upload it.' },
        cleanup: { label: 'Clean up upload data', description: 'Remove leftover upload data. The completed source file and its placements are kept.' },
        removing: { label: 'Finish removal', description: 'Finish removing this source file and its remaining stored data.' },
    } satisfies Record<(typeof PendingUpload.Type)['status'], { readonly label: string; readonly description: string }>;
    const documents = files.flatMap((file) => {
        const asset = file.meta.prepared === undefined ? undefined : assets[file.meta.prepared.asset.id];
        const entry = entries.find((item) => file.meta.destination.kind === 'entry' && item.id === file.meta.destination.id);
        return file.progress.uploadComplete === true && Schema.is(PdfAsset)(asset) && entry !== undefined ? [{ id: file.id, label: `${asset.name} · ${entryTitle(entry)}`, entryId: entry.id, assetId: asset.id }] : [];
    });
    const selectedFile = files.find((file) => file.id === selected);
    const queuedUploads = new Set(files.flatMap((file) => (file.meta.prepared !== undefined && file.progress.uploadComplete !== true ? [file.meta.prepared.asset.id] : [])));
    return (
        <>
            <SelectField label="Destination for new files" onChange={(option) => setTarget(option.target)} options={options} value={key(target)} />
            {files.length > 0 && (
                <MenuButton label="File destinations" onAction={(item) => setSelected(item.id)} options={files.map((item) => ({ id: item.id, label: item.name }))}>
                    File destinations
                </MenuButton>
            )}
            {selectedFile !== undefined && (
                <div className="min-w-0">
                    <p className="hint wrap-anywhere mb-2">{selectedFile.name}</p>
                    {selectedFile.progress.uploadStarted === null && selectedFile.meta.prepared === undefined && !Object.values(batches).some((batch) => batch.fileIDs.includes(selectedFile.id)) ? (
                        <SelectField
                            label="File destination"
                            onChange={(option) => uploader.setFileMeta(selectedFile.id, { destination: option.target })}
                            options={options.filter((option) => option.target.kind !== 'hero' || !files.some((item) => item.id !== selectedFile.id && item.meta.destination.kind === 'hero' && item.progress.uploadComplete !== true))}
                            value={key(selectedFile.meta.destination)}
                        />
                    ) : (
                        <p className="hint">
                            Destination: {options.find((option) => option.id === key(selectedFile.meta.destination))?.label ?? 'Removed entry'} · {selectedFile.progress.uploadComplete === true ? 'Uploaded' : 'Fixed after processing starts'}
                        </p>
                    )}
                </div>
            )}
            <div className="upload-dashboard min-w-0">
                <p className="hint mb-3">
                    {new Intl.ListFormat('en', { type: 'disjunction' }).format(mediaTypes.flatMap((type) => type.split('/').slice(1)).map((subtype) => subtype.toUpperCase()))} · Up to {byteSize.format(uploadLimit)} per file
                </p>
                <div className={files.length > 0 ? 'h-88' : 'h-50'}>
                    <Dashboard disableInformer={true} doneButtonHandler={() => uploader.removeFiles(files.map(Struct.get('id')))} height="100%" proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                </div>
                {notices.map(({ message }) => (
                    <p className="hint wrap-anywhere mt-3" key={message} role="alert">
                        {message}
                    </p>
                ))}
                {documents.length > 0 && (
                    <div className="mt-3">
                        <MenuButton label="Choose uploaded sheets" onAction={({ entryId, assetId }) => onChoose(entryId, assetId)} options={documents}>
                            Choose uploaded sheets
                        </MenuButton>
                    </div>
                )}
                <p className="hint mt-3">Choose PDF sheets after upload. Images and videos are placed directly.</p>
                {pending.length > 0 && <p className="hint mt-3">Keep this browser tab open until uploads finish. You can close the editor.</p>}
                <p className="hint mt-3">Publishing a PDF sheet makes the entire original document public, including unselected pages.</p>
                <details className="hint mt-3">
                    <summary className="min-h-11 cursor-pointer content-center">Upload guidance</summary>
                    <p>File destinations lets you change a queued file’s target before processing starts. The destination above applies to new files.</p>
                    <p className="mt-2">Done clears the queue. Files remain in the library under “Add to entry”.</p>
                    <p className="mt-2">Export CAD and BIM files as PDF or images.</p>
                </details>
            </div>
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
                            <p className="hint wrap-anywhere mb-2">{upload.name}</p>
                            <RequestAlert conflict="This upload changed in another request. Refresh the list before trying again." error={error} />
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
                            <p className="hint mt-3">No incomplete uploads.</p>
                        ) : (
                            <>
                                <p className="hint mt-3">Files interrupted before completion stay private. If this tab no longer has the file, discard its incomplete upload and add the original again.</p>
                                <ul className="mt-3 divide-y divide-line">
                                    {value.map((upload) => (
                                        <li className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1 py-3" key={upload.id}>
                                            <span className="wrap-anywhere min-w-0 flex-1 text-sm">{upload.name}</span>
                                            <ConfirmDialog description={`${upload.name}: ${actions[upload.status].description}`} disabled={discarded.waiting || queuedUploads.has(upload.id)} label={actions[upload.status].label} onConfirm={() => discard(upload)} />
                                            {queuedUploads.has(upload.id) && <p className="hint w-full">Retry or remove this file in the upload queue before discarding its stored data.</p>}
                                        </li>
                                    ))}
                                </ul>
                            </>
                        ),
                    )
                    .render()}
            </section>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
