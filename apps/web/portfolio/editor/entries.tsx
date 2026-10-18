import { Struct } from 'effect';
import { useState } from 'react';
import { Button, type Key } from 'react-aria-components';
import { entryTitle, quantity } from '../media/display.ts';
import { Entry } from '../model/document.ts';
import { ConfirmDialog, Field, SelectField, SequenceList } from './controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const EntryList = ({
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
    onChange: (entries: readonly (typeof Entry.Type)[]) => void;
    onSelect: (entry: typeof Entry.Type) => void;
    onRemove: (keys: ReadonlySet<Key>) => void;
}) => (
    <SequenceList
        actions={(keys) => (
            <ConfirmDialog
                description="Remove the selected entries from your draft. Source files remain in the library. Publish to apply this change to the public site."
                disabled={entries.some((entry) => keys.has(entry.id) && locked(entry))}
                label={`Remove ${quantity(keys.size, { one: 'selected entry', other: 'selected entries' })}`}
                onConfirm={() => onRemove(keys)}
            />
        )}
        items={entries}
        label="Projects and studies"
        onChange={onChange}
        textValue={entryTitle}
    >
        {(entry) => (
            <div className="flex items-center justify-between gap-4.5 pb-2.5 max-sm:items-start max-sm:gap-2">
                <Button aria-pressed={selected === entry.id} className="wrap-anywhere min-w-0 flex-1 text-left text-lg aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-5" onPress={() => onSelect(entry)}>
                    {entryTitle(entry)}
                    <span className="meta-line no-underline">{quantity(entry.compositions.length, { one: 'composition', other: 'compositions' })}</span>
                </Button>
                <ConfirmDialog description="Remove this entry from your draft. Source files remain in the library. Publish to apply this change to the public site." disabled={locked(entry)} label="Remove entry" onConfirm={() => onRemove(new Set([entry.id]))} />
            </div>
        )}
    </SequenceList>
);
const EntryFields = ({ entry, titleId, onChange }: { entry: typeof Entry.Type; titleId: string; onChange: (update: (entry: typeof Entry.Type) => typeof Entry.Type) => void }) => {
    const [open, setOpen] = useState(entry.title.length === 0);
    return (
        <details onToggle={(event) => setOpen(event.currentTarget.open)} open={open}>
            <summary className="min-h-11 cursor-pointer content-center text-sm">Entry details</summary>
            <div className="flex flex-col gap-5 py-3">
                <div className="field-pair">
                    <Field id={titleId} label="Title" onChange={(title) => onChange((current) => ({ ...current, title }))} placeholder="Project or study title" value={entry.title} />
                    <SelectField label="Entry type" onChange={({ id }) => onChange((current) => ({ ...current, kind: id }))} options={Entry.fields.kind.literals.map((id) => ({ id, label: { project: 'Project', study: 'Independent study' }[id] }))} value={entry.kind} />
                </div>
                <div className="flex flex-col gap-4.5">
                    <div className="field-pair">
                        <Field label="Year" onChange={(year) => onChange((current) => ({ ...current, year }))} value={entry.year} />
                        <Field label="Location" onChange={(location) => onChange((current) => ({ ...current, location }))} value={entry.location} />
                    </div>
                    <Field label="Description" multiline={true} onChange={(description) => onChange((current) => ({ ...current, description }))} value={entry.description} />
                    <div className="field-pair">
                        <Field label="Role" onChange={(role) => onChange((current) => ({ ...current, role }))} value={entry.role} />
                        <Field label="Credits" onChange={(credits) => onChange((current) => ({ ...current, credits }))} value={entry.credits} />
                    </div>
                </div>
                {entry.cover && (
                    <div className="flex flex-wrap items-center justify-between gap-2.5">
                        <span className="eyebrow wrap-anywhere min-w-0">Cover · {entry.cover.asset.name}</span>
                        <button className="button button-ghost" onClick={() => onChange(Struct.omit(['cover']))} type="button">
                            Clear thumbnail
                        </button>
                    </div>
                )}
            </div>
        </details>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { EntryFields, EntryList };
