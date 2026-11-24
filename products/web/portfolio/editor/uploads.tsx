import { useAtom, useAtomMount, useAtomValue } from '@effect/atom-react';
import { Choice, MenuButton } from '@rasm/ui';
import { useUppyState } from '@uppy/react';
import Dashboard from '@uppy/react/dashboard';
import { Array, Predicate, Result, String, Struct } from 'effect';
import { useState } from 'react';
import { Destination, nextDestination, pendingUploads, preprocessing, queue, uppy } from '#/editor/uploader';
import { choiceLayout, entryTitle, format, joinDetails } from '#/media/labels';
import { Asset, type Assets, uploadLimit } from '#/model/asset';
import type { Entry } from '#/model/portfolio';

// --- [OPERATIONS] ----------------------------------------------------------------------

const key = (value: Destination): string => (Destination.$is('Entry')(value) ? value.id : String.uncapitalize(value._tag));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Uploads = ({ assets, entries, onPlace }: { readonly assets: Assets['Service']; readonly entries: readonly Entry[]; readonly onPlace: (entryId: Entry['id'], assetId: keyof Assets['Service']) => void }) => {
    const uploader = useAtomValue(uppy);
    useAtomMount(preprocessing);
    const files = Object.values(useAtomValue(queue));
    const pending = useAtomValue(pendingUploads);
    const batches = useUppyState(uploader, Struct.get('currentUploads'));
    const notices = useUppyState(uploader, Struct.get('info'));
    const [selected, setSelected] = useState<string>();
    const [target, setTarget] = useAtom(nextDestination);
    const options = [{ target: Destination.Library(), label: 'Library' }, { target: Destination.Hero(), label: 'Hero (one file)' }, ...entries.map((entry) => ({ target: Destination.Entry({ id: entry.id }), label: joinDetails([entryTitle(entry), String.capitalize(entry.kind)]) }))].map((option) => ({
        ...option,
        id: key(option.target),
    }));
    const documents = Array.filterMap(files, (file) => {
        const asset = file.meta.prepared === undefined ? undefined : assets[file.meta.prepared.asset.id];
        const { destination } = file.meta;
        const entry = entries.find((item) => Destination.$is('Entry')(destination) && item.id === destination.id);
        return file.progress.uploadComplete === true && Asset.guards.pdf(asset) && entry !== undefined ? Result.succeed({ id: file.id, label: joinDetails([asset.name, entryTitle(entry)]), entryId: entry.id, assetId: asset.id }) : Result.failVoid;
    });
    const selectedFile = files.find((file) => file.id === selected);
    return (
        <>
            <Choice {...choiceLayout} label="Destination for new files" onChange={(option) => setTarget(option.target)} options={options} value={key(target)} />
            {files.length > 0 && (
                <MenuButton onAction={(item) => setSelected(item.id)} options={files.map((item) => ({ id: item.id, label: item.name }))}>
                    File destinations
                </MenuButton>
            )}
            {selectedFile !== undefined && (
                <div>
                    <p className="file-name hint">{selectedFile.name}</p>
                    {selectedFile.progress.uploadStarted === null && selectedFile.meta.prepared === undefined && !Object.values(batches).some((batch) => batch.fileIDs.includes(selectedFile.id)) ? (
                        <Choice
                            {...choiceLayout}
                            label="File destination"
                            onChange={(option) => uploader.setFileMeta(selectedFile.id, { destination: option.target })}
                            options={options.map((option) => ({ ...option, isDisabled: Destination.$is('Hero')(option.target) && !Destination.$is('Hero')(selectedFile.meta.destination) && pending.some(Destination.$is('Hero')) }))}
                            value={key(selectedFile.meta.destination)}
                        />
                    ) : (
                        <p className="hint">{joinDetails(['Destination', options.find((option) => option.id === key(selectedFile.meta.destination))?.label ?? 'Removed entry', selectedFile.progress.uploadComplete === true ? 'Uploaded' : 'Locked after processing starts'])}</p>
                    )}
                </div>
            )}
            <div className="uploads">
                <p className="hint">
                    {joinDetails([
                        format.list(
                            Asset.mediaTypes.map((type) => type.slice(type.indexOf('/') + 1).toUpperCase()),
                            'disjunction',
                        ),
                        `Up to ${format.bytes(uploadLimit)} per file`,
                    ])}
                </p>
                <div data-filled={files.length > 0}>
                    <Dashboard disableInformer doneButtonHandler={() => uploader.removeFiles(files.map(Struct.get('id')))} height="100%" proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                </div>
                {notices
                    .filter(({ isHidden }) => isHidden !== true)
                    .map(({ message, details: detail }) => [message, ...(Predicate.isString(detail) ? [detail] : Object.values(detail ?? {}))].filter(String.isNonEmpty).join(' '))
                    .map((text) => (
                        <p className="hint" key={text} role="alert">
                            {text}
                        </p>
                    ))}
                {documents.length > 0 && (
                    <MenuButton onAction={({ entryId, assetId }) => onPlace(entryId, assetId)} options={documents}>
                        Choose uploaded sheets
                    </MenuButton>
                )}
                <p className="hint">Images and videos place directly. Choose PDF sheets after upload</p>
                {pending.length > 0 && <p className="hint">Keep tab open until uploads finish. Closing editor is safe</p>}
                <p className="hint">Publishing any sheet makes whole PDF public</p>
                <details className="hint">
                    <summary>Upload guidance</summary>
                    <p>File destinations retargets queued files until processing starts</p>
                    <p>Done clears queue. Uploaded files stay in library</p>
                    <p>Export CAD and BIM files as PDF or images</p>
                </details>
            </div>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
