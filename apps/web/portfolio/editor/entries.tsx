import { useAtomSet, useAtomValue } from '@effect/atom-react';
import { Choice, Field, ReorderableList } from '@rasm/ui';
import { Array, Optic, Struct } from 'effect';
import { useState } from 'react';
import { Button } from 'react-aria-components';
import { ConfirmDialog } from '#/editor/confirm-dialog.tsx';
import { draft, portfolio, updateEntry } from '#/editor/state.ts';
import { Destination, nextDestination, pendingUploads } from '#/editor/uploader.ts';
import { choice, details, entryTitle, format, options } from '#/media/labels.ts';
import { Entry, type EntryId } from '#/model/portfolio.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const EntryList = ({ entries, selected, onSelect, onRemove }: { readonly entries: readonly (typeof Entry.Type)[]; readonly selected: typeof EntryId.Type | undefined; readonly onSelect: (entry: typeof Entry.Type) => void; readonly onRemove: () => void }) => {
    const setDraft = useAtomSet(draft);
    const setTarget = useAtomSet(nextDestination);
    const pending = useAtomValue(pendingUploads);
    const locked = (keys: ReadonlySet<string>) => pending.some((target) => Destination.$is('Entry')(target) && keys.has(target.id));
    const remove = (keys: ReadonlySet<string>) => {
        setDraft(portfolio.key('entries').modify(Array.filter((candidate) => !keys.has(candidate.id))));
        setTarget((target) => (Destination.$is('Entry')(target) && keys.has(target.id) ? Destination.Library() : target));
        onRemove();
    };
    return (
        <ReorderableList
            actions={(keys) => <ConfirmDialog disabled={locked(keys)} label={format.quantity(keys.size, { one: (shown) => `Remove ${shown} selected entry`, other: (shown) => `Remove ${shown} selected entries` })} onConfirm={() => remove(keys)} />}
            aria-label="Projects and studies"
            className="editor-sequence"
            items={entries}
            moveLabel="Move"
            onChange={(items) => setDraft(Optic.replace(portfolio.key('entries'), items))}
            textValue={entryTitle}
        >
            {(item) => (
                <div className="flex items-center justify-between gap-4.5 pb-2.5 max-sm:items-start max-sm:gap-2">
                    <Button aria-pressed={selected === item.id} className="aria-pressed:text-accent-text min-w-0 flex-1 text-left text-lg wrap-anywhere aria-pressed:underline aria-pressed:underline-offset-5" onPress={() => onSelect(item)}>
                        {entryTitle(item)}
                        <span className="meta-line no-underline">{format.quantity(item.compositions.length, { one: (shown) => `${shown} composition`, other: (shown) => `${shown} compositions` })}</span>
                    </Button>
                    <ConfirmDialog disabled={locked(new Set([item.id]))} label="Remove entry" onConfirm={() => remove(new Set([item.id]))} />
                </div>
            )}
        </ReorderableList>
    );
};
const EntryFields = ({ entry, titleId }: { readonly entry: typeof Entry.Type; readonly titleId: string }) => {
    const [open, setOpen] = useState(entry.title.length === 0);
    const setDraft = useAtomSet(draft);
    const update = (fields: Partial<typeof Entry.Type>) => setDraft(updateEntry(entry.id, (current) => ({ ...current, ...fields })));
    return (
        <details onToggle={(event) => setOpen(event.currentTarget.open)} open={open}>
            <summary className="min-h-11 cursor-pointer content-center text-sm">Entry details</summary>
            <div className="flex flex-col gap-5 py-3">
                <div className="field-pair">
                    <Field id={titleId} label="Title" onChange={(title) => update({ title })} value={entry.title} />
                    <Choice {...choice} label="Entry type" onChange={({ id }) => update({ kind: id })} options={options(Entry.fields.kind.literals)} value={entry.kind} />
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
                        <span className="eyebrow min-w-0 wrap-anywhere">{details(['Cover', entry.cover.asset.name])}</span>
                        <button className="button button-ghost" onClick={() => setDraft(updateEntry(entry.id, Struct.omit(['cover'])))} type="button">
                            Clear cover
                        </button>
                    </div>
                )}
            </div>
        </details>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { EntryFields, EntryList };
