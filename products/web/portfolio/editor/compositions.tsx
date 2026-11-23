import { useAtomSet } from '@effect/atom-react';
import { Choice, MenuButton, ModalDialog, ReorderableList } from '@rasm/ui';
import { Array, Optic, Option, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { type Ref, useState } from 'react';
import { Button } from 'react-aria-components';
import { PlacementFields } from '#/editor/placement-fields.tsx';
import { draft, hero, updateEntry } from '#/editor/state';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { choiceLayout, compositionLabel, confirmation, entryTitle, format, literalOptions, placementTitle } from '#/media/labels';
import type { Assets } from '#/model/asset';
import { Alignment, Composition, Scale } from '#/model/placement';
import type { Entry } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionEditor = ({ assets, entry, item, ref }: { readonly assets: Assets['Service']; readonly entry: Entry; readonly item: Composition; readonly ref: Ref<HTMLDivElement> }) => {
    const setDraft = useAtomSet(draft);
    const [first, second] = item.items;
    const update = (change: (compositions: Entry['compositions']) => Entry['compositions']) => setDraft(updateEntry(entry.id, Struct.evolve({ compositions: change })));
    const replace = (...values: readonly Composition[]) => update(Array.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])));
    const fields = (placement: Composition['items'][number], items: (next: Composition['items'][number]) => Composition['items']) => (
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
        <div className="composition-editor" ref={ref} tabIndex={-1}>
            <CompositionGrid active composition={item} presentation="preview" renderMedia />
            <div className="field-pair">
                <Choice {...choiceLayout} label="Scale" onChange={({ id }) => replace({ ...item, scale: id })} options={literalOptions(Scale.literals)} value={item.scale} />
                <Choice {...choiceLayout} label="Alignment" onChange={({ id }) => replace({ ...item, align: id })} options={literalOptions(Alignment.literals)} value={item.align} />
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
                        <button className="button button-outline" onClick={() => replace({ ...item, items: [first] }, { ...item, id: Composition.fields.id.make(SecureRandom.uuidV7Unsafe()), items: [second] })} type="button">
                            Unpair
                        </button>
                    </>
                )}
                <ModalDialog.Confirm {...confirmation} label="Remove composition" onConfirm={() => replace()} />
            </div>
        </div>
    );
};
const Compositions = ({
    assets,
    entry,
    selected,
    editor,
    onSelect,
    onUpload,
    onPlace,
}: {
    readonly assets: Assets['Service'];
    readonly entry: Entry;
    readonly selected: Option.Option<Composition['id']>;
    readonly editor: Ref<HTMLDivElement>;
    readonly onSelect: (id: Composition['id']) => void;
    readonly onUpload: () => void;
    readonly onPlace: () => void;
}) => {
    const setDraft = useAtomSet(draft);
    const [choosing, setChoosing] = useState(false);
    const item = Option.getOrUndefined(
        Option.orElse(
            Array.findFirst(entry.compositions, (candidate) => Option.contains(selected, candidate.id)),
            () => Array.head(entry.compositions),
        ),
    );
    return (
        <>
            <div className="actions">
                <h3 className="sequence-heading">
                    Sequence <span>{format.quantity(entry.compositions.length, { one: (count) => `${count} composition`, other: (count) => `${count} compositions` })}</span>
                </h3>
                <button className="button button-outline" onClick={onUpload} type="button">
                    Add files
                </button>
                <button className="button button-ghost" onClick={onPlace} type="button">
                    Add from library
                </button>
                <button aria-expanded={choosing} aria-label="Choose composition" className="button button-outline sequence-toggle" onClick={() => setChoosing(!choosing)} type="button">
                    {choosing ? 'Close' : 'Choose'}
                </button>
            </div>
            <div className="sequence-layout">
                <div data-open={choosing || undefined}>
                    <ReorderableList
                        actions={(keys) => (
                            <ModalDialog.Confirm
                                {...confirmation}
                                label={format.quantity(keys.size, { one: (count) => `Remove ${count} selected composition`, other: (count) => `Remove ${count} selected compositions` })}
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
                                className="sequence-item"
                                onPress={() => {
                                    onSelect(candidate.id);
                                    setChoosing(false);
                                }}
                            >
                                {candidate.items.every((placement) => placement.kind === 'image') && (
                                    <div inert>
                                        <CompositionGrid active={false} composition={candidate} presentation="thumbnail" renderMedia />
                                    </div>
                                )}
                                {compositionLabel(candidate)}
                            </Button>
                        )}
                    </ReorderableList>
                </div>
                {item && <CompositionEditor assets={assets} entry={entry} item={item} key={item.id} ref={editor} />}
            </div>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Compositions };
