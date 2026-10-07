import { useAtom, useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { useUppyEvent, useUppyState } from '@uppy/react';
import Dashboard from '@uppy/react/dashboard';
import { Match, Option, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { type ReactNode, useState } from 'react';
import './uploads.css';
import { byteSize, entryTitle } from '../media/display.ts';
import { type AssetCollection, uploadLimit } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { ConfirmDialog, MenuButton, RequestAlert, SelectField } from './controls.tsx';
import { discardRequest, pendingRequest } from './services.ts';
import { type Destination, destination, pendingUploads, queuedFiles, uploads } from './uploader.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Uploads({ assets, entries, onChoose }: { assets: typeof AssetCollection.Type; entries: readonly (typeof Entry.Type)[]; onChoose: (entryId: string, assetId: string) => void }): ReactNode {
    const dashboardHeight = { empty: 200, queue: 350 };
    const uploader = useAtomValue(uploads);
    const files = useAtomValue(queuedFiles);
    const pending = useAtomValue(pendingUploads);
    const refreshFiles = useAtomRefresh(queuedFiles);
    const recovery = useAtomValue(pendingRequest);
    const refreshRecovery = useAtomRefresh(pendingRequest);
    const [discarded, discard] = useAtom(discardRequest);
    useUppyEvent(uploader, 'complete', refreshRecovery);
    useUppyEvent(uploader, 'file-removed', (file): void => {
        if (file.meta.prepared && !file.progress.uploadComplete) {
            refreshRecovery();
        }
    });
    const batches = useUppyState(uploader, Struct.get('currentUploads'));
    const [selected, setSelected] = useState<string>();
    const [target, setTarget] = useAtom(destination);
    const options: readonly { readonly id: string; readonly label: string; readonly target: Destination }[] = [
        { id: 'library', label: 'Private file library', target: { kind: 'library' } },
        { id: 'hero', label: 'Hero placement — one file', target: { kind: 'hero' } },
        ...entries.map((entry) => ({ id: entry.id, label: `${entryTitle(entry)} · ${entry.kind}`, target: { kind: 'entry', id: entry.id } satisfies Destination })),
    ];
    const documents = files.flatMap((file) => {
        const asset = file.meta.prepared && assets[file.meta.prepared.asset.id];
        const entry = entries.find((item) => file.meta.destination.kind === 'entry' && item.id === file.meta.destination.id);
        return file.progress.uploadComplete && asset?.mime === 'application/pdf' && entry ? [{ id: file.id, label: `${asset.name} · ${entryTitle(entry)}`, entryId: entry.id, assetId: asset.id }] : [];
    });
    const selectedFile = files.find((file) => file.id === selected);
    const editable = selectedFile && !selectedFile.progress.uploadStarted && !selectedFile.meta.prepared && !Object.values(batches).some((batch) => batch.fileIDs.includes(selectedFile.id));
    const queuedUploads = new Set(files.flatMap((file) => (file.meta.prepared && !file.progress.uploadComplete ? [file.meta.prepared.asset.id] : [])));
    return (
        <>
            <SelectField label="Destination for new files" onChange={(option): void => setTarget(option.target)} options={options} value={target.kind === 'entry' ? target.id : target.kind} />
            {files.length > 0 && (
                <MenuButton label="File destinations" onAction={(item): void => setSelected(item.id)} options={files.map((item) => ({ id: item.id, label: item.name }))}>
                    File destinations
                </MenuButton>
            )}
            {selectedFile && (
                <div className="min-w-0">
                    <p className="hint wrap-anywhere mb-2">{selectedFile.name}</p>
                    {editable ? (
                        <SelectField
                            label="File destination"
                            onChange={(option): void => {
                                uploader.setFileMeta(selectedFile.id, { destination: option.target });
                                refreshFiles();
                            }}
                            options={options.filter((option) => option.target.kind !== 'hero' || !files.some((item) => item.id !== selectedFile.id && item.meta.destination.kind === 'hero' && !item.progress.uploadComplete))}
                            value={selectedFile.meta.destination.kind === 'entry' ? selectedFile.meta.destination.id : selectedFile.meta.destination.kind}
                        />
                    ) : (
                        <p className="hint">
                            Destination: {options.find((option) => option.id === (selectedFile.meta.destination.kind === 'entry' ? selectedFile.meta.destination.id : selectedFile.meta.destination.kind))?.label ?? 'Removed entry'} · {selectedFile.progress.uploadComplete ? 'Uploaded' : 'Fixed after processing starts'}
                        </p>
                    )}
                </div>
            )}
            <div className="min-w-0 [&_.uppy-Dashboard-inner]:rounded-none [&_.uppy-Dashboard-inner]:bg-background">
                <p className="hint mb-3">JPEG, PNG, WebP, AVIF, PDF, MP4 or WebM · Up to {byteSize.format(uploadLimit)} per file</p>
                <Dashboard doneButtonHandler={(): void => uploader.removeFiles(uploader.getFiles().map(Struct.get('id')))} height={files.length > 0 ? dashboardHeight.queue : dashboardHeight.empty} proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                {documents.length > 0 && (
                    <div className="mt-3">
                        <MenuButton label="Choose uploaded sheets" onAction={({ entryId, assetId }): void => onChoose(entryId, assetId)} options={documents}>
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
                {Option.match(AsyncResult.error(discarded), {
                    onNone: (): ReactNode => null,
                    onSome: ({ upload, error }): ReactNode => (
                        <div className="mt-3">
                            <p className="hint wrap-anywhere mb-2">{upload.name}</p>
                            <RequestAlert conflict="This upload changed in another request. Refresh the list before trying again." error={error} />
                        </div>
                    ),
                })}
                {AsyncResult.matchWithError(recovery, {
                    onInitial: (): ReactNode => (
                        <p className="hint mt-3" role="status">
                            Checking incomplete uploads…
                        </p>
                    ),
                    onError: (error): ReactNode => <RequestAlert conflict="Upload recovery could not be loaded." error={error} />,
                    onDefect: (defect): never => {
                        throw defect;
                    },
                    onSuccess: ({ value }): ReactNode =>
                        value.length === 0 ? (
                            <p className="hint mt-3">No incomplete uploads.</p>
                        ) : (
                            <>
                                <p className="hint mt-3">Files interrupted before completion stay private. If this tab no longer has the file, discard its incomplete upload and add the original again.</p>
                                <ul className="mt-3 divide-y divide-line">
                                    {value.map((upload) => {
                                        const action = Match.value(upload.status).pipe(
                                            Match.when('incomplete', () => ({ label: 'Discard incomplete upload', description: 'Discard the incomplete upload and its stored data. Add the original file again to upload it.' })),
                                            Match.when('cleanup', () => ({ label: 'Clean up upload data', description: 'Remove leftover upload data. The completed source file and its placements are kept.' })),
                                            Match.when('removing', () => ({ label: 'Finish removal', description: 'Finish removing this source file and its remaining stored data.' })),
                                            Match.exhaustive,
                                        );
                                        const queued = queuedUploads.has(upload.id);
                                        return (
                                            <li className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1 py-3" key={upload.id}>
                                                <span className="wrap-anywhere min-w-0 flex-1 text-sm">{upload.name}</span>
                                                <ConfirmDialog description={`${upload.name}: ${action.description}`} disabled={discarded.waiting || queued} label={action.label} onConfirm={(): void => discard(upload)} />
                                                {queued && <p className="hint w-full">Retry or remove this file in the upload queue before discarding its stored data.</p>}
                                            </li>
                                        );
                                    })}
                                </ul>
                            </>
                        ),
                })}
            </section>
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
