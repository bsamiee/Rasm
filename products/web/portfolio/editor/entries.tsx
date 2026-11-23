import { useAtomSet, useAtomValue } from '@effect/atom-react';
import { Choice, Field, ModalDialog, ReorderableList } from '@rasm/ui';
import { Array, Optic, Option, Struct } from 'effect';
import { type Ref, useState } from 'react';
import { Button } from 'react-aria-components';
import { draft, entered, portfolio, updateEntry } from '#/editor/state';
import { Destination, nextDestination, pendingUploads } from '#/editor/uploader';
import { choiceLayout, confirmation, entryTitle, format, joinDetails, literalOptions } from '#/media/labels';
import { Entry, type EntryId } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const EntryList = ({ entries, selected, onSelect, onRemove }: { readonly entries: readonly (typeof Entry.Type)[]; readonly selected: Option.Option<typeof EntryId.Type>; readonly onSelect: (entry: typeof Entry.Type) => void; readonly onRemove: () => void }) => {
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
            actions={(keys) => <ModalDialog.Confirm {...confirmation} isDisabled={locked(keys)} label={format.quantity(keys.size, { one: (count) => `Remove ${count} selected entry`, other: (count) => `Remove ${count} selected entries` })} onConfirm={() => remove(keys)} />}
            aria-label="Projects and studies"
            className="editor-sequence"
            items={entries}
            moveLabel="Move"
            onChange={(items) => setDraft(Optic.replace(portfolio.key('entries'), items))}
            textValue={entryTitle}
        >
            {(item) => (
                <div className="entry-row">
                    <Button aria-pressed={Option.contains(selected, item.id)} onPress={() => onSelect(item)}>
                        {entryTitle(item)}
                        <span className="meta-line">{format.quantity(item.compositions.length, { one: (count) => `${count} composition`, other: (count) => `${count} compositions` })}</span>
                    </Button>
                    <ModalDialog.Confirm {...confirmation} isDisabled={locked(new Set([item.id]))} label="Remove entry" onConfirm={() => remove(new Set([item.id]))} />
                </div>
            )}
        </ReorderableList>
    );
};
const EntryFields = ({ entry, title }: { readonly entry: typeof Entry.Type; readonly title: Ref<HTMLInputElement> }) => {
    const [open, setOpen] = useState(entry.title === undefined);
    const setDraft = useAtomSet(draft);
    const field = (key: 'title' | 'year' | 'location' | 'description' | 'role' | 'credits') => ({ value: entry[key] ?? '', onChange: (input: string) => setDraft(updateEntry(entry.id, Optic.replace(Optic.id<typeof Entry.Type>().optionalKey(key), entered(input)))) });
    return (
        <details className="entry-details" onToggle={(event) => setOpen(event.currentTarget.open)} open={open}>
            <summary>Entry details</summary>
            <div>
                <div className="field-pair">
                    <Field {...field('title')} control={title} label="Title" />
                    <Choice {...choiceLayout} label="Entry type" onChange={({ id }) => setDraft(updateEntry(entry.id, Struct.assign({ kind: id })))} options={literalOptions(Entry.fields.kind.literals)} value={entry.kind} />
                </div>
                <div className="entry-fields">
                    <div className="field-pair">
                        <Field {...field('year')} label="Year" />
                        <Field {...field('location')} label="Location" />
                    </div>
                    <Field {...field('description')} label="Description" multiline />
                    <div className="field-pair">
                        <Field {...field('role')} label="Role" />
                        <Field {...field('credits')} label="Credits" />
                    </div>
                </div>
                {entry.cover && (
                    <div className="entry-cover">
                        <span className="eyebrow">{joinDetails(['Cover', entry.cover.asset.name])}</span>
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
