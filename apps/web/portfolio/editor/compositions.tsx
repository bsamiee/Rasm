import { useAtomSet } from '@effect/atom-react';
import { ReorderableList } from '@rasm/ui/reorderable.tsx';
import { Array, Effect, String, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { useState } from 'react';
import { Button } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, placementTitle, quantity } from '../media/display.ts';
import type { AssetCollection } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { Composition } from '../model/placement.ts';
import { ConfirmDialog, MenuButton } from './controls.tsx';
import { PlacementFields } from './placement.tsx';
import { draft, identifier, updateEntry } from './services.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionEditor = ({ assets, entry, item }: { assets: typeof AssetCollection.Type; entry: typeof Entry.Type; item: typeof Composition.Type }) => {
    const setDraft = useAtomSet(draft);
    const [first, second] = item.items;
    const update = (change: (compositions: readonly (typeof Composition.Type)[]) => readonly (typeof Composition.Type)[]) => setDraft(AsyncResult.map(updateEntry(entry.id, Struct.evolve({ compositions: change }))));
    const replace = (...values: readonly (typeof Composition.Type)[]) => update(Array.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])));
    const singles = entry.compositions.filter((candidate) => candidate.id !== item.id && candidate.items.length === 1);
    return (
        <div className="flex min-w-0 flex-col gap-4.5 outline-none" id={item.id} tabIndex={-1}>
            <CompositionGrid active={true} composition={item} presentation="preview" renderMedia={true} />
            <div className="field-pair">
                {[
                    { legend: 'Scale', options: Composition.fields.scale.literals.map((scale) => ({ value: scale, checked: item.scale === scale, next: { ...item, scale } })) },
                    { legend: 'Alignment', options: Composition.fields.align.literals.map((align) => ({ value: align, checked: item.align === align, next: { ...item, align } })) },
                ].map(({ legend, options }) => (
                    <fieldset className="min-w-0" key={legend}>
                        <legend className="text-muted mb-2 text-xs">{legend}</legend>
                        <div className="flex flex-wrap gap-1">
                            {options.map(({ value, checked, next }) => (
                                <label className="cursor-pointer" key={value}>
                                    <input checked={checked} className="peer sr-only" name={`${item.id}-${legend}`} onChange={() => replace(next)} type="radio" />
                                    <span className="editor-choice">{String.capitalize(value)}</span>
                                </label>
                            ))}
                        </div>
                    </fieldset>
                ))}
            </div>
            <PlacementFields assets={assets} entryId={entry.id} onChange={(placement) => replace({ ...item, items: second === undefined ? [placement] : [placement, second] })} value={first} />
            {second && <PlacementFields assets={assets} entryId={entry.id} onChange={(placement) => replace({ ...item, items: [first, placement] })} value={second} />}
            <div className="actions">
                {second === undefined ? (
                    singles.length > 0 && (
                        <MenuButton
                            label="Pair with"
                            onAction={({ pair }) => update((compositions) => compositions.filter((candidate) => candidate.id !== pair.id).map((candidate) => (candidate.id === item.id ? { ...candidate, items: [first, pair.items[0]] } : candidate)))}
                            options={singles.map((pair) => ({ id: pair.id, label: placementTitle(pair.items[0]), pair }))}
                        >
                            Pair with
                        </MenuButton>
                    )
                ) : (
                    <>
                        <button className="button button-outline" onClick={() => replace({ ...item, items: [second, first] })} type="button">
                            Swap pair order
                        </button>
                        <button className="button button-outline" onClick={() => replace({ ...item, items: [first] }, { ...item, id: Effect.runSync(identifier), items: [second] })} type="button">
                            Unpair
                        </button>
                    </>
                )}
                <ConfirmDialog description="The source file stays in your library. Published work changes only when you publish." disabled={false} label="Remove composition" onConfirm={() => replace()} />
            </div>
        </div>
    );
};
const Compositions = ({ assets, entry, selected, onSelect, onUpload, onLibrary }: { assets: typeof AssetCollection.Type; entry: typeof Entry.Type; selected: string | undefined; onSelect: (id: string) => void; onUpload: () => void; onLibrary: () => void }) => {
    const setDraft = useAtomSet(draft);
    const [choosing, setChoosing] = useState(false);
    const item = entry.compositions.find((candidate) => candidate.id === selected) ?? entry.compositions[0];
    return (
        <>
            <div className="actions">
                <h3 className="max-editor:w-full mr-auto text-xl tracking-tight">
                    Sequence <span className="text-muted ml-2.5 text-sm">{quantity(entry.compositions.length, { one: 'composition', other: 'compositions' })}</span>
                </h3>
                <button className="button button-outline" onClick={onUpload} type="button">
                    Add files
                </button>
                <button className="button button-ghost" onClick={onLibrary} type="button">
                    Add from library
                </button>
                <button aria-expanded={choosing} aria-label="Choose composition" className="button button-outline editor:hidden" onClick={() => setChoosing(!choosing)} type="button">
                    {choosing ? 'Close' : 'Choose'}
                </button>
            </div>
            <div className="editor:grid-cols-sequence grid min-w-0 gap-6">
                <div className="editor:editor-scroll max-editor:not-data-open:hidden min-w-0" data-open={choosing || undefined}>
                    <ReorderableList
                        actions={(keys) => (
                            <ConfirmDialog
                                description="Remove the selected compositions from this draft. Source files remain in the library. Published work changes only when you publish."
                                disabled={false}
                                label={`Remove ${quantity(keys.size, { one: 'selected composition', other: 'selected compositions' })}`}
                                onConfirm={() => setDraft(AsyncResult.map(updateEntry(entry.id, Struct.evolve({ compositions: Array.filter((candidate) => !keys.has(candidate.id)) }))))}
                            />
                        )}
                        aria-label={`${entryTitle(entry)} compositions`}
                        className="editor-sequence"
                        items={entry.compositions}
                        moveLabel="Move"
                        onChange={(reordered) => setDraft(AsyncResult.map(updateEntry(entry.id, Struct.evolve({ compositions: () => reordered }))))}
                        textValue={compositionLabel}
                    >
                        {(candidate) => (
                            <Button
                                aria-pressed={item?.id === candidate.id}
                                className="aria-pressed:text-accent-text block w-full min-w-0 py-2 text-left text-sm wrap-anywhere aria-pressed:underline aria-pressed:underline-offset-4"
                                onPress={() => {
                                    onSelect(candidate.id);
                                    setChoosing(false);
                                }}
                            >
                                {candidate.items.every((placement) => placement.kind === 'image') && (
                                    <div className="pointer-events-none mb-2 max-w-24">
                                        <CompositionGrid active={false} composition={candidate} presentation="thumbnail" renderMedia={true} />
                                    </div>
                                )}
                                {compositionLabel(candidate)}
                            </Button>
                        )}
                    </ReorderableList>
                </div>
                {item && <CompositionEditor assets={assets} entry={entry} item={item} key={item.id} />}
            </div>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Compositions };
