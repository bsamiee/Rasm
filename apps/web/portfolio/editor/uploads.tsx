import { useAtom, useAtomValue } from '@effect/atom-react';
import { MenuButton } from '@rasm/ui/menu-button.tsx';
import { SelectField } from '@rasm/ui/select-field.tsx';
import { useUppyState } from '@uppy/react';
import Dashboard from '@uppy/react/dashboard';
import { Schema, String, Struct } from 'effect';
import { useState } from 'react';
import '@uppy/core/css/style.min.css';
import '@uppy/dashboard/css/style.min.css';
import { byteSize, disjunction, entryTitle } from '../media/labels.ts';
import { type AssetCollection, mediaTypes, PdfAsset, uploadLimit } from '../model/asset.ts';
import type { Entry } from '../model/portfolio.ts';
import { type Destination, destination, pendingUploads, queue, uppy } from './uploader.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = ({ assets, entries, onChoose }: { assets: typeof AssetCollection.Type; entries: readonly (typeof Entry.Type)[]; onChoose: (entryId: string, assetId: string) => void }) => {
    const uploader = useAtomValue(uppy);
    const files = Object.values(useAtomValue(queue));
    const pending = useAtomValue(pendingUploads);
    const batches = useUppyState(uploader, Struct.get('currentUploads'));
    const notices = useUppyState(uploader, Struct.get('info'));
    const [selected, setSelected] = useState<string>();
    const [target, setTarget] = useAtom(destination);
    const key = (value: Destination) => (value.kind === 'entry' ? value.id : value.kind);
    const options = [
        { target: { kind: 'library' } as const, label: 'Private file library' },
        { target: { kind: 'hero' } as const, label: 'Hero placement — one file' },
        ...entries.map((entry) => ({ target: { kind: 'entry', id: entry.id } as const, label: `${entryTitle(entry)} · ${String.capitalize(entry.kind)}` })),
    ].map((option) => ({
        ...option,
        id: key(option.target),
    }));
    const documents = files.flatMap((file) => {
        const asset = file.meta.prepared === undefined ? undefined : assets[file.meta.prepared.asset.id];
        const entry = entries.find((item) => file.meta.destination.kind === 'entry' && item.id === file.meta.destination.id);
        return file.progress.uploadComplete === true && Schema.is(PdfAsset)(asset) && entry !== undefined ? [{ id: file.id, label: `${asset.name} · ${entryTitle(entry)}`, entryId: entry.id, assetId: asset.id }] : [];
    });
    const selectedFile = files.find((file) => file.id === selected);
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
                    <p className="hint mb-2 wrap-anywhere">{selectedFile.name}</p>
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
                    {disjunction.format(mediaTypes.flatMap((type) => type.split('/').slice(1)).map((subtype) => subtype.toUpperCase()))} · Up to {byteSize.format(uploadLimit)} per file
                </p>
                <div className={files.length > 0 ? 'h-88' : 'h-50'}>
                    <Dashboard disableInformer={true} doneButtonHandler={() => uploader.removeFiles(files.map(Struct.get('id')))} height="100%" proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                </div>
                {notices.map(({ message, details }) => (
                    <p className="hint mt-3 wrap-anywhere" key={`${message}${details}`} role="alert">
                        {[message, details].filter(Boolean).join(' ')}
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
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
