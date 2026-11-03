import { useAtomSet } from '@effect/atom-react';
import { MenuButton } from '@rasm/ui/collection/menu-button';
import { ReorderableList } from '@rasm/ui/collection/reorderable-list';
import { Choice } from '@rasm/ui/form/choice';
import { Array, Optic, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { useState } from 'react';
import { Button } from 'react-aria-components';
import { ConfirmDialog } from '#/editor/confirm-dialog.tsx';
import { PlacementFields } from '#/editor/placement-fields.tsx';
import { draft, hero, updateEntry } from '#/editor/state.ts';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { choice, compositionLabel, entryTitle, format, options, placementTitle } from '#/media/labels.ts';
import type { Assets } from '#/model/asset.ts';
import { Composition, CompositionId, type Placement } from '#/model/placement.ts';
import type { Entry } from '#/model/portfolio.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionEditor = ({ assets, entry, item }: { readonly assets: typeof Assets.Service; readonly entry: typeof Entry.Type; readonly item: typeof Composition.Type }) => {
    const setDraft = useAtomSet(draft);
    const [first, second] = item.items;
    const update = (change: (compositions: readonly (typeof Composition.Type)[]) => readonly (typeof Composition.Type)[]) => setDraft(updateEntry(entry.id, Struct.evolve({ compositions: change })));
    const replace = (...values: readonly (typeof Composition.Type)[]) => update(Array.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])));
    const fields = (placement: typeof Placement.Type, items: (next: typeof Placement.Type) => (typeof Composition.Type)['items']) => (
        <PlacementFields assets={assets} onChange={(next) => replace({ ...item, items: items(next) })} value={placement}>
            <div className="actions">
                <button className="button button-outline" onClick={() => setDraft(Optic.replace(hero, placement))} type="button">
                    Use as hero
                </button>
                <button className="button button-outline" onClick={() => setDraft(updateEntry(entry.id, Struct.assign({ cover: placement })))} type="button">
                    Use as cover
                </button>
            </div>
        </PlacementFields>
    );
    return (
        <div className="flex min-w-0 flex-col gap-4.5 outline-none" id={item.id} tabIndex={-1}>
            <CompositionGrid active={true} composition={item} presentation="preview" renderMedia={true} />
            <div className="field-pair">
                <Choice {...choice} label="Scale" onChange={({ id }) => replace({ ...item, scale: id })} options={options(Composition.fields.scale.schema.to.literals)} value={item.scale} />
                <Choice {...choice} label="Alignment" onChange={({ id }) => replace({ ...item, align: id })} options={options(Composition.fields.align.schema.to.literals)} value={item.align} />
            </div>
            {fields(first, (next) => (second === undefined ? [next] : [next, second]))}
            {second && fields(second, (next) => [first, next])}
            <div className="actions">
                {second === undefined ? (
                    <MenuButton
                        onAction={({ pair }) => update((compositions) => compositions.filter((candidate) => candidate.id !== pair.id).map((candidate) => (candidate.id === item.id ? { ...candidate, items: [first, pair.items[0]] } : candidate)))}
                        options={entry.compositions.filter((pair) => pair.id !== item.id && pair.items.length === 1).map((pair) => ({ id: pair.id, label: placementTitle(pair.items[0]), pair }))}
                    >
                        Pair with
                    </MenuButton>
                ) : (
                    <>
                        <button className="button button-outline" onClick={() => replace({ ...item, items: [second, first] })} type="button">
                            Swap order
                        </button>
                        <button className="button button-outline" onClick={() => replace({ ...item, items: [first] }, { ...item, id: CompositionId.make(SecureRandom.uuidV7Unsafe()), items: [second] })} type="button">
                            Unpair
                        </button>
                    </>
                )}
                <ConfirmDialog label="Remove composition" onConfirm={() => replace()} />
            </div>
        </div>
    );
};
const Compositions = ({
    assets,
    entry,
    selected,
    onSelect,
    onUpload,
    onPlace,
}: {
    readonly assets: typeof Assets.Service;
    readonly entry: typeof Entry.Type;
    readonly selected: typeof CompositionId.Type | undefined;
    readonly onSelect: (id: typeof CompositionId.Type) => void;
    readonly onUpload: () => void;
    readonly onPlace: () => void;
}) => {
    const setDraft = useAtomSet(draft);
    const [choosing, setChoosing] = useState(false);
    const item = entry.compositions.find((candidate) => candidate.id === selected) ?? entry.compositions[0];
    return (
        <>
            <div className="actions">
                <h3 className="max-editor:w-full mr-auto text-xl tracking-tight">
                    Sequence <span className="text-muted ml-2.5 text-sm">{format.quantity(entry.compositions.length, { one: (shown) => `${shown} composition`, other: (shown) => `${shown} compositions` })}</span>
                </h3>
                <button className="button button-outline" onClick={onUpload} type="button">
                    Add files
                </button>
                <button className="button button-ghost" onClick={onPlace} type="button">
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
                                label={format.quantity(keys.size, { one: (shown) => `Remove ${shown} selected composition`, other: (shown) => `Remove ${shown} selected compositions` })}
                                onConfirm={() => setDraft(updateEntry(entry.id, Struct.evolve({ compositions: Array.filter((candidate) => !keys.has(candidate.id)) })))}
                            />
                        )}
                        aria-label={`${entryTitle(entry)} compositions`}
                        className="editor-sequence"
                        items={entry.compositions}
                        moveLabel="Move"
                        onChange={(reordered) => setDraft(updateEntry(entry.id, Struct.assign({ compositions: reordered })))}
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
