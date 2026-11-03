import { useAtom, useAtomMount, useAtomValue } from '@effect/atom-react';
import { MenuButton } from '@rasm/ui/collection/menu-button';
import { Choice } from '@rasm/ui/form/choice';
import { useUppyState } from '@uppy/react';
import Dashboard from '@uppy/react/dashboard';
import { Predicate, String, Struct } from 'effect';
import { useState } from 'react';
import '@uppy/core/css/style.min.css';
import '@uppy/dashboard/css/style.min.css';
import { Destination, nextDestination, pendingUploads, preprocessing, queue, uppy } from '#/editor/uploader.ts';
import { choice, details, entryTitle, format } from '#/media/labels.ts';
import { Asset, type AssetId, type Assets, uploadLimit } from '#/model/asset.ts';
import type { Entry, EntryId } from '#/model/portfolio.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = ({ assets, entries, onPlace }: { assets: typeof Assets.Service; entries: readonly (typeof Entry.Type)[]; onPlace: (entryId: typeof EntryId.Type, assetId: typeof AssetId.Type) => void }) => {
    const uploader = useAtomValue(uppy);
    useAtomMount(preprocessing);
    const files = Object.values(useAtomValue(queue));
    const pending = useAtomValue(pendingUploads);
    const batches = useUppyState(uploader, Struct.get('currentUploads'));
    const notices = useUppyState(uploader, Struct.get('info'));
    const [selected, setSelected] = useState<string>();
    const [target, setTarget] = useAtom(nextDestination);
    const key = (value: Destination): string => (Destination.$is('Entry')(value) ? value.id : String.uncapitalize(value._tag));
    const options = [{ target: Destination.Library(), label: 'Library' }, { target: Destination.Hero(), label: 'Hero (one file)' }, ...entries.map((entry) => ({ target: Destination.Entry({ id: entry.id }), label: details([entryTitle(entry), String.capitalize(entry.kind)]) }))].map((option) => ({
        ...option,
        id: key(option.target),
    }));
    const documents = files.flatMap((file) => {
        const asset = file.meta.prepared === undefined ? undefined : assets[file.meta.prepared.asset.id];
        const { destination } = file.meta;
        const entry = entries.find((item) => Destination.$is('Entry')(destination) && item.id === destination.id);
        return file.progress.uploadComplete === true && Asset.guards.pdf(asset) && entry !== undefined ? [{ id: file.id, label: details([asset.name, entryTitle(entry)]), entryId: entry.id, assetId: asset.id }] : [];
    });
    const selectedFile = files.find((file) => file.id === selected);
    return (
        <>
            <Choice {...choice} label="Destination for new files" onChange={(option) => setTarget(option.target)} options={options} value={key(target)} />
            {files.length > 0 && (
                <MenuButton onAction={(item) => setSelected(item.id)} options={files.map((item) => ({ id: item.id, label: item.name }))}>
                    File destinations
                </MenuButton>
            )}
            {selectedFile !== undefined && (
                <div className="min-w-0">
                    <p className="hint mb-2 wrap-anywhere">{selectedFile.name}</p>
                    {selectedFile.progress.uploadStarted === null && selectedFile.meta.prepared === undefined && !Object.values(batches).some((batch) => batch.fileIDs.includes(selectedFile.id)) ? (
                        <Choice
                            {...choice}
                            label="File destination"
                            onChange={(option) => uploader.setFileMeta(selectedFile.id, { destination: option.target })}
                            options={options.map((option) => ({ ...option, isDisabled: Destination.$is('Hero')(option.target) && !Destination.$is('Hero')(selectedFile.meta.destination) && pending.some(Destination.$is('Hero')) }))}
                            value={key(selectedFile.meta.destination)}
                        />
                    ) : (
                        <p className="hint">{details(['Destination', options.find((option) => option.id === key(selectedFile.meta.destination))?.label ?? 'Removed entry', selectedFile.progress.uploadComplete === true ? 'Uploaded' : 'Locked after processing starts'])}</p>
                    )}
                </div>
            )}
            <div className="upload-dashboard min-w-0">
                <p className="hint mb-3">
                    {details([
                        format.list(
                            Asset.mediaTypes.map((type) => type.slice(type.indexOf('/') + 1).toUpperCase()),
                            'disjunction',
                        ),
                        `Up to ${format.bytes(uploadLimit)} per file`,
                    ])}
                </p>
                <div className={files.length > 0 ? 'h-88' : 'h-50'}>
                    <Dashboard disableInformer={true} doneButtonHandler={() => uploader.removeFiles(files.map(Struct.get('id')))} height="100%" proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                </div>
                {notices
                    .filter(({ isHidden }) => isHidden !== true)
                    .map(({ message, details: detail }) => [message, ...(Predicate.isString(detail) ? [detail] : Object.values(detail ?? {}))].filter(String.isNonEmpty).join(' '))
                    .map((text) => (
                        <p className="hint mt-3 wrap-anywhere" key={text} role="alert">
                            {text}
                        </p>
                    ))}
                {documents.length > 0 && (
                    <div className="mt-3">
                        <MenuButton onAction={({ entryId, assetId }) => onPlace(entryId, assetId)} options={documents}>
                            Choose uploaded sheets
                        </MenuButton>
                    </div>
                )}
                <p className="hint mt-3">Images and videos place directly. Choose PDF sheets after upload</p>
                {pending.length > 0 && <p className="hint mt-3">Keep tab open until uploads finish. Closing editor is safe</p>}
                <p className="hint mt-3">Publishing any sheet makes whole PDF public</p>
                <details className="hint mt-3">
                    <summary className="min-h-11 cursor-pointer content-center">Upload guidance</summary>
                    <p>File destinations retargets queued files until processing starts</p>
                    <p className="mt-2">Done clears queue. Uploaded files stay in library</p>
                    <p className="mt-2">Export CAD and BIM files as PDF or images</p>
                </details>
            </div>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
