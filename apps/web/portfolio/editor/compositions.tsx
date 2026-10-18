import { Array, Effect, Optic, String } from 'effect';
import { useState } from 'react';
import { Button } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, placementTitle, quantity } from '../media/display.ts';
import type { AssetCollection } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { Composition, type Placement } from '../model/placement.ts';
import { ConfirmDialog, MenuButton, SequenceList } from './controls.tsx';
import { PlacementFields } from './placement.tsx';
import { identifier } from './services.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionEditor = ({ assets, entry, item, onChange, onHero }: { assets: typeof AssetCollection.Type; entry: typeof Entry.Type; item: typeof Composition.Type; onChange: (update: (entry: typeof Entry.Type) => typeof Entry.Type) => void; onHero: (placement: typeof Placement.Type) => void }) => {
    const [first, second] = item.items;
    const replace = (...values: readonly (typeof Composition.Type)[]) => onChange((current) => ({ ...current, compositions: current.compositions.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])) }));
    const cover = (placement: typeof Placement.Type) => onChange((current) => ({ ...current, cover: placement }));
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
                        <legend className="mb-2 text-muted text-xs">{legend}</legend>
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
            <PlacementFields assets={assets} onChange={(placement) => replace({ ...item, items: second === undefined ? [placement] : [placement, second] })} onCover={cover} onHero={onHero} value={first} />
            {second && <PlacementFields assets={assets} onChange={(placement) => replace({ ...item, items: [first, placement] })} onCover={cover} onHero={onHero} value={second} />}
            <div className="actions">
                {second === undefined ? (
                    singles.length > 0 && (
                        <MenuButton
                            label="Pair with"
                            onAction={({ pair }) => onChange((current) => ({ ...current, compositions: current.compositions.filter((candidate) => candidate.id !== pair.id).map((candidate) => (candidate.id === item.id ? { ...candidate, items: [first, pair.items[0]] } : candidate)) }))}
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
const Compositions = ({
    assets,
    entry,
    selected,
    onSelect,
    onChange,
    onHero,
    onUpload,
    onLibrary,
}: {
    assets: typeof AssetCollection.Type;
    selected: string | undefined;
    onSelect: (id: string | undefined) => void;
    entry: typeof Entry.Type;
    onChange: (update: (entry: typeof Entry.Type) => typeof Entry.Type) => void;
    onHero: (placement: typeof Placement.Type) => void;
    onUpload: () => void;
    onLibrary: () => void;
}) => {
    const item = entry.compositions.find((candidate) => candidate.id === selected) ?? entry.compositions[0];
    const compositions = Optic.id<typeof Entry.Type>().key('compositions');
    const [choosing, setChoosing] = useState(false);
    return (
        <>
            <div className="actions">
                <h3 className="mr-auto text-xl tracking-tight max-editor:w-full">
                    Sequence <span className="ml-2.5 text-muted text-sm">{quantity(entry.compositions.length, { one: 'composition', other: 'compositions' })}</span>
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
            <div className="grid min-w-0 editor:grid-cols-sequence gap-6">
                <div className="editor:editor-scroll min-w-0 max-editor:not-data-open:hidden" data-open={choosing || undefined}>
                    <SequenceList
                        actions={(keys) => (
                            <ConfirmDialog
                                description="Remove the selected compositions from this draft. Source files remain in the library. Published work changes only when you publish."
                                disabled={false}
                                label={`Remove ${quantity(keys.size, { one: 'selected composition', other: 'selected compositions' })}`}
                                onConfirm={() => onChange(compositions.modify(Array.filter((candidate) => !keys.has(candidate.id))))}
                            />
                        )}
                        items={entry.compositions}
                        label={`${entryTitle(entry)} compositions`}
                        onChange={(reordered) => onChange(Optic.replace(compositions, reordered))}
                        textValue={compositionLabel}
                    >
                        {(candidate) => (
                            <Button
                                aria-pressed={item?.id === candidate.id}
                                className="wrap-anywhere block w-full min-w-0 py-2 text-left text-sm aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-4"
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
                    </SequenceList>
                </div>
                {item && <CompositionEditor assets={assets} entry={entry} item={item} key={item.id} onChange={onChange} onHero={onHero} />}
            </div>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Compositions };
