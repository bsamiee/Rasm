import { Struct } from 'effect';
import type { ReactNode } from 'react';
import { Button, Disclosure, DisclosurePanel } from 'react-aria-components';
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
    onChange: (entries: readonly (typeof Entry.Type)[]) => void;
    onSelect: (entry: typeof Entry.Type) => void;
    onRemove: (entry: typeof Entry.Type) => void;
}): ReactNode {
    return (
        <ReorderableList items={entries} label="Projects and studies" onChange={onChange} textValue={entryTitle}>
            {(entry): ReactNode => (
                <div className="flex items-center justify-between gap-[18px] pb-2.5 max-sm:items-start max-sm:gap-2">
                    <Button aria-pressed={selected === entry.id} className="wrap-anywhere min-w-0 flex-1 text-left text-[19px] aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-[5px]" onPress={(): void => onSelect(entry)}>
                        {entryTitle(entry)}
                        <span className="meta-line no-underline">{entry.compositions.length} compositions</span>
                    </Button>
                    <ConfirmDialog description="Remove this entry from your draft. Source files remain in the library. Publish to apply this change to the public site." disabled={locked(entry)} label="Remove entry" onConfirm={(): void => onRemove(entry)} />
                </div>
            )}
        </ReorderableList>
    );
}
function EntryFields({ entry, onChange }: { entry: typeof Entry.Type; onChange: (entry: typeof Entry.Type) => void }): ReactNode {
    return (
        <>
            <div className="field-pair">
                <Field id={titleId(entry)} label="Title" onChange={(title): void => onChange({ ...entry, title })} placeholder="Project or study title" value={entry.title} />
                <SelectField label="Entry type" onChange={({ id }): void => onChange({ ...entry, kind: id })} options={kinds} value={entry.kind} />
            </div>
            <Disclosure className="flex flex-col gap-[18px]">
                <Button className="min-h-11 text-left text-sm" slot="trigger">
                    Optional project details
                </Button>
                <DisclosurePanel className="flex flex-col gap-[18px] py-4">
                    <div className="field-pair">
                        <Field label="Year" onChange={(year): void => onChange({ ...entry, year })} value={entry.year} />
                        <Field label="Location" onChange={(location): void => onChange({ ...entry, location })} value={entry.location} />
                    </div>
                    <Field label="Description" multiline={true} onChange={(description): void => onChange({ ...entry, description })} value={entry.description} />
                    <div className="field-pair">
                        <Field label="Role" onChange={(role): void => onChange({ ...entry, role })} value={entry.role} />
                        <Field label="Credits" onChange={(credits): void => onChange({ ...entry, credits })} value={entry.credits} />
                    </div>
                </DisclosurePanel>
            </Disclosure>
            {entry.cover && (
                <div className="flex items-center justify-between gap-2.5">
                    <span className="eyebrow">Chapter thumbnail · {entry.cover.asset.name}</span>
                    <button className="button button-ghost" onClick={(): void => onChange(Struct.omit(entry, ['cover']))} type="button">
                        Clear thumbnail
                    </button>
                </div>
            )}
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { EntryFields, EntryList };
