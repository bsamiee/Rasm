import { useAtomSet, useAtomValue } from '@effect/atom-react';
import { Field, ReorderableList, SelectField } from '@rasm/ui';
import { Array, Optic, String, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { useState } from 'react';
import { Button, type Key } from 'react-aria-components';
import { ConfirmDialog } from '#/editor/confirm-dialog';
import { draft, portfolio, updateEntry } from '#/editor/state';
import { destination, pendingUploads } from '#/editor/uploader';
import { entryTitle, quantity } from '#/media/labels';
import { Entry } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const EntryList = ({ entries, selected, onSelect, onRemove }: { entries: readonly (typeof Entry.Type)[]; selected: string | undefined; onSelect: (entry: typeof Entry.Type) => void; onRemove: () => void }) => {
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(destination);
    const pending = useAtomValue(pendingUploads);
    const locked = (keys: ReadonlySet<Key>) => pending.some((target) => target.kind === 'entry' && keys.has(target.id));
    const remove = (keys: ReadonlySet<Key>) => {
        setDraft(AsyncResult.map(portfolio.key('entries').modify(Array.filter((candidate) => !keys.has(candidate.id)))));
        setTarget((target) => (target.kind === 'entry' && keys.has(target.id) ? { kind: 'library' } : target));
        onRemove();
    };
    return (
        <ReorderableList
            actions={(keys) => (
                <ConfirmDialog
                    description="Remove the selected entries from your draft. Source files remain in the library. Publish to apply this change to the public site."
                    disabled={locked(keys)}
                    label={`Remove ${quantity(keys.size, { one: 'selected entry', other: 'selected entries' })}`}
                    onConfirm={() => remove(keys)}
                />
            )}
            aria-label="Projects and studies"
            className="editor-sequence"
            items={entries}
            moveLabel="Move"
            onChange={(items) => setDraft(AsyncResult.map(Optic.replace(portfolio.key('entries'), items)))}
            textValue={entryTitle}
        >
            {(item) => (
                <div className="flex items-center justify-between gap-4.5 pb-2.5 max-sm:items-start max-sm:gap-2">
                    <Button aria-pressed={selected === item.id} className="aria-pressed:text-accent-text min-w-0 flex-1 text-left text-lg wrap-anywhere aria-pressed:underline aria-pressed:underline-offset-5" onPress={() => onSelect(item)}>
                        {entryTitle(item)}
                        <span className="meta-line no-underline">{quantity(item.compositions.length, { one: 'composition', other: 'compositions' })}</span>
                    </Button>
                    <ConfirmDialog description="Remove this entry from your draft. Source files remain in the library. Publish to apply this change to the public site." disabled={locked(new Set([item.id]))} label="Remove entry" onConfirm={() => remove(new Set([item.id]))} />
                </div>
            )}
        </ReorderableList>
    );
};
const EntryFields = ({ entry, titleId }: { entry: typeof Entry.Type; titleId: string }) => {
    const [open, setOpen] = useState(entry.title.length === 0);
    const setDraft = useAtomSet(draft);
    const update = (fields: Partial<typeof Entry.Type>) => setDraft(AsyncResult.map(updateEntry(entry.id, (current) => ({ ...current, ...fields }))));
    return (
        <details onToggle={(event) => setOpen(event.currentTarget.open)} open={open}>
            <summary className="min-h-11 cursor-pointer content-center text-sm">Entry details</summary>
            <div className="flex flex-col gap-5 py-3">
                <div className="field-pair">
                    <Field id={titleId} label="Title" onChange={(title) => update({ title })} placeholder="Project or study title" value={entry.title} />
                    <SelectField label="Entry type" onChange={({ id }) => update({ kind: id })} options={Entry.fields.kind.literals.map((id) => ({ id, label: String.capitalize(id) }))} value={entry.kind} />
                </div>
                <div className="flex flex-col gap-4.5">
                    <div className="field-pair">
                        <Field label="Year" onChange={(year) => update({ year })} value={entry.year} />
                        <Field label="Location" onChange={(location) => update({ location })} value={entry.location} />
                    </div>
                    <Field label="Description" multiline={true} onChange={(description) => update({ description })} value={entry.description} />
                    <div className="field-pair">
                        <Field label="Role" onChange={(role) => update({ role })} value={entry.role} />
                        <Field label="Credits" onChange={(credits) => update({ credits })} value={entry.credits} />
                    </div>
                </div>
                {entry.cover && (
                    <div className="flex flex-wrap items-center justify-between gap-2.5">
                        <span className="eyebrow min-w-0 wrap-anywhere">Cover · {entry.cover.asset.name}</span>
                        <button className="button button-ghost" onClick={() => setDraft(AsyncResult.map(updateEntry(entry.id, Struct.omit(['cover']))))} type="button">
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
