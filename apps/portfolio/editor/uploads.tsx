import { useAtom, useAtomValue } from '@effect/atom-react';
import Dashboard from '@uppy/react/dashboard';
import type { ReactNode } from 'react';
import '@uppy/core/css/style.min.css';
import '@uppy/dashboard/css/style.min.css';
import { entryTitle } from '../media/display.ts';
import type { Entry } from '../model/document.ts';
import { SelectField } from './controls.tsx';
import { type Destination, destination, uploads } from './uploader.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Uploads({ entries }: { entries: readonly (typeof Entry.Type)[] }): ReactNode {
    const uploader = useAtomValue(uploads);
    const [target, setTarget] = useAtom(destination);
    const options: readonly { readonly id: string; readonly label: string; readonly target: Destination }[] = [
        { id: 'library', label: 'Private file library', target: { kind: 'library' } },
        { id: 'hero', label: 'Hero placement — one file', target: { kind: 'hero' } },
        ...entries.map((entry) => ({ id: entry.id, label: `${entryTitle(entry)} · ${entry.kind}`, target: { kind: 'entry', id: entry.id } satisfies Destination })),
    ];
    return (
        <>
            <SelectField label="Destination for new files" onChange={(option): void => setTarget(option.target)} options={options} value={target.kind === 'entry' ? target.id : target.kind} />
            <div className="min-w-0 [&_.uppy-Dashboard-inner]:rounded-none [&_.uppy-Dashboard-inner]:bg-background">
                <Dashboard height={350} note="JPEG, PNG, WebP, AVIF, PDF, MP4 or WebM · Up to 100 MB per file" proudlyDisplayPoweredByUppy={false} uppy={uploader} width="100%" />
                <p className="note mt-3">The destination stays with each file. You can change tabs or close the editor while uploads continue. Keep this browser tab open until they finish.</p>
                <p className="note">Export CAD and BIM files as PDF or images. Publishing any PDF sheet makes the entire original document public, including unselected pages.</p>
            </div>
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uploads };
