import { Struct } from 'effect';
import { type ReactNode, useState } from 'react';
import { Button, type Key } from 'react-aria-components';
import { entryTitle } from '../media/display.ts';
import type { Entry } from '../model/document.ts';
import { ConfirmDialog, Field, SelectField } from './controls.tsx';
import { titleId } from './focus.ts';
import { ReorderableList } from './reorderable.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const kinds = [
    { id: 'project', label: 'Project' },
    { id: 'study', label: 'Independent study' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

function EntryList({
    entries,
    selected,
    locked,
    onChange,
    onSelect,
    onRemove,
}: {
    entries: readonly (typeof Entry.Type)[];
    selected: string | undefined;
    locked: (entry: typeof Entry.Type) => boolean;
    onChange: (update: (entries: readonly (typeof Entry.Type)[]) => readonly (typeof Entry.Type)[]) => void;
    onSelect: (entry: typeof Entry.Type) => void;
    onRemove: (keys: ReadonlySet<Key>) => void;
}): ReactNode {
    return (
        <ReorderableList
            actions={(keys): ReactNode => (
                <ConfirmDialog
                    description="Remove the selected entries from your draft. Source files remain in the library. Publish to apply this change to the public site."
                    disabled={entries.some((entry) => keys.has(entry.id) && locked(entry))}
                    label={`Remove ${keys.size} selected ${keys.size === 1 ? 'entry' : 'entries'}`}
                    onConfirm={(): void => onRemove(keys)}
                />
            )}
            items={entries}
            label="Projects and studies"
            onChange={onChange}
            textValue={entryTitle}
        >
            {(entry): ReactNode => (
                <div className="flex items-center justify-between gap-[18px] pb-2.5 max-sm:items-start max-sm:gap-2">
                    <Button aria-pressed={selected === entry.id} className="wrap-anywhere min-w-0 flex-1 text-left text-lg aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-[5px]" onPress={(): void => onSelect(entry)}>
                        {entryTitle(entry)}
                        <span className="meta-line no-underline">
                            {entry.compositions.length} {entry.compositions.length === 1 ? 'composition' : 'compositions'}
                        </span>
                    </Button>
                    <ConfirmDialog description="Remove this entry from your draft. Source files remain in the library. Publish to apply this change to the public site." disabled={locked(entry)} label="Remove entry" onConfirm={(): void => onRemove(new Set([entry.id]))} />
                </div>
            )}
        </ReorderableList>
    );
}
function EntryFields({ entry, onChange }: { entry: typeof Entry.Type; onChange: (update: (entry: typeof Entry.Type) => typeof Entry.Type) => void }): ReactNode {
    const [open, setOpen] = useState(entry.title.length === 0);
    return (
        <details onToggle={(event): void => setOpen(event.currentTarget.open)} open={open}>
            <summary className="min-h-11 cursor-pointer content-center text-sm">Entry details</summary>
            <div className="flex flex-col gap-5 py-3">
                <div className="field-pair">
                    <Field id={titleId(entry)} label="Title" onChange={(title): void => onChange((current) => ({ ...current, title }))} placeholder="Project or study title" value={entry.title} />
                    <SelectField label="Entry type" onChange={({ id }): void => onChange((current) => ({ ...current, kind: id }))} options={kinds} value={entry.kind} />
                </div>
                <div className="flex flex-col gap-[18px]">
                    <div className="field-pair">
                        <Field label="Year" onChange={(year): void => onChange((current) => ({ ...current, year }))} value={entry.year} />
                        <Field label="Location" onChange={(location): void => onChange((current) => ({ ...current, location }))} value={entry.location} />
                    </div>
                    <Field label="Description" multiline={true} onChange={(description): void => onChange((current) => ({ ...current, description }))} value={entry.description} />
                    <div className="field-pair">
                        <Field label="Role" onChange={(role): void => onChange((current) => ({ ...current, role }))} value={entry.role} />
                        <Field label="Credits" onChange={(credits): void => onChange((current) => ({ ...current, credits }))} value={entry.credits} />
                    </div>
                </div>
                {entry.cover && (
                    <div className="flex flex-wrap items-center justify-between gap-2.5">
                        <span className="eyebrow wrap-anywhere min-w-0">Cover · {entry.cover.asset.name}</span>
                        <button className="button button-ghost" onClick={(): void => onChange(Struct.omit(['cover']))} type="button">
                            Clear thumbnail
                        </button>
                    </div>
                )}
            </div>
        </details>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { EntryFields, EntryList };
