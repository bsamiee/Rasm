import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Effect, Optic, String } from 'effect';
import { type ReactNode, useState } from 'react';
import { Button } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, placementLabel } from '../media/display.ts';
import type { AssetCollection } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { Composition, type Placement } from '../model/placement.ts';
import { ConfirmDialog, MenuButton } from './controls.tsx';
import { PlacementFields } from './placement.tsx';
import { ReorderableList } from './reorderable.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function CompositionEditor({ assets, entry, item, onChange, onHero }: { assets: typeof AssetCollection.Type; entry: typeof Entry.Type; item: typeof Composition.Type; onChange: (update: (entry: typeof Entry.Type) => typeof Entry.Type) => void; onHero: (placement: typeof Placement.Type) => void }): ReactNode {
    const [first, second] = item.items;
    const replace = (...values: readonly (typeof Composition.Type)[]): void => onChange((current) => ({ ...current, compositions: current.compositions.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])) }));
    const singles = entry.compositions.filter((candidate) => candidate.id !== item.id && candidate.items.length === 1);
    return (
        <div className="flex min-w-0 flex-col gap-[18px] outline-none" id={item.id} tabIndex={-1}>
            <CompositionGrid active={true} composition={item} presentation="preview" renderMedia={true} />
            <div className="field-pair">
                <fieldset className="min-w-0">
                    <legend className="mb-2 text-muted text-xs">Scale</legend>
                    <div className="flex flex-wrap gap-1">
                        {Composition.fields.scale.literals.map((scale) => (
                            <label className="cursor-pointer" key={scale}>
                                <input checked={item.scale === scale} className="peer sr-only" name={`${item.id}-scale`} onChange={(): void => replace({ ...item, scale })} type="radio" />
                                <span className="grid min-h-11 min-w-14 place-items-center px-2 text-sm peer-checked:text-accent-text peer-checked:underline peer-checked:underline-offset-4 peer-focus-visible:outline-2 peer-focus-visible:outline-accent-text">{String.capitalize(scale)}</span>
                            </label>
                        ))}
                    </div>
                </fieldset>
                <fieldset className="min-w-0">
                    <legend className="mb-2 text-muted text-xs">Alignment</legend>
                    <div className="flex flex-wrap gap-1">
                        {Composition.fields.align.literals.map((align) => (
                            <label className="cursor-pointer" key={align}>
                                <input checked={item.align === align} className="peer sr-only" name={`${item.id}-align`} onChange={(): void => replace({ ...item, align })} type="radio" />
                                <span className="grid min-h-11 min-w-14 place-items-center px-2 text-sm peer-checked:text-accent-text peer-checked:underline peer-checked:underline-offset-4 peer-focus-visible:outline-2 peer-focus-visible:outline-accent-text">{String.capitalize(align)}</span>
                            </label>
                        ))}
                    </div>
                </fieldset>
            </div>
            <PlacementFields assets={assets} onChange={(placement): void => replace({ ...item, items: second ? [placement, second] : [placement] })} onCover={(): void => onChange((current) => ({ ...current, cover: first }))} onHero={(): void => onHero(first)} value={first} />
            {second && <PlacementFields assets={assets} onChange={(placement): void => replace({ ...item, items: [first, placement] })} onCover={(): void => onChange((current) => ({ ...current, cover: second }))} onHero={(): void => onHero(second)} value={second} />}
            <div className="actions">
                {second ? (
                    <>
                        <button className="button button-outline" onClick={(): void => replace({ ...item, items: [second, first] })} type="button">
                            Swap pair order
                        </button>
                        <button className="button button-outline" onClick={(): void => replace({ ...item, items: [first] }, { ...item, id: Effect.runSync(BrowserCrypto.WebCrypto).randomUUID(), items: [second] })} type="button">
                            Unpair
                        </button>
                    </>
                ) : (
                    singles.length > 0 && (
                        <MenuButton
                            label="Pair with"
                            onAction={({ pair }): void => onChange((current) => ({ ...current, compositions: current.compositions.filter((candidate) => candidate.id !== pair.id).map((candidate) => (candidate.id === item.id ? { ...candidate, items: [first, pair.items[0]] } : candidate)) }))}
                            options={singles.map((pair) => ({ id: pair.id, label: placementLabel(pair.items[0]), pair }))}
                        >
                            Pair with
                        </MenuButton>
                    )
                )}
                <ConfirmDialog description="The source file stays in your library. Published work changes only when you publish." disabled={false} label="Remove composition" onConfirm={(): void => replace()} />
            </div>
        </div>
    );
}
function Compositions({
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
}): ReactNode {
    const item = entry.compositions.find((candidate) => candidate.id === selected) ?? entry.compositions[0];
    const [choosing, setChoosing] = useState(false);
    return (
        <>
            <div className="actions">
                <h3 className="mr-auto text-xl tracking-tight max-[799px]:w-full">
                    Sequence <span className="ml-2.5 text-muted text-sm">{entry.compositions.length} compositions</span>
                </h3>
                <button className="button button-outline" onClick={onUpload} type="button">
                    Add files
                </button>
                <button className="button button-ghost" onClick={onLibrary} type="button">
                    Add from library
                </button>
                <button aria-expanded={choosing} aria-label="Choose composition" className="button button-outline min-[800px]:hidden" onClick={(): void => setChoosing(!choosing)} type="button">
                    {choosing ? 'Close' : 'Choose'}
                </button>
            </div>
            <div className="grid min-w-0 gap-6 min-[800px]:grid-cols-[15rem_minmax(0,1fr)]">
                <div className={`${choosing ? '' : 'max-[799px]:hidden'} min-w-0 min-[800px]:max-h-[65dvh] min-[800px]:overflow-y-auto min-[800px]:pr-4`}>
                    <ReorderableList
                        actions={(keys): ReactNode => (
                            <ConfirmDialog
                                description="Remove the selected compositions from this draft. Source files remain in the library. Published work changes only when you publish."
                                disabled={false}
                                label={`Remove ${keys.size} selected ${keys.size === 1 ? 'composition' : 'compositions'}`}
                                onConfirm={(): void =>
                                    onChange(
                                        Optic.id<typeof Entry.Type>()
                                            .key('compositions')
                                            .modify(Array.filter((candidate) => !keys.has(candidate.id))),
                                    )
                                }
                            />
                        )}
                        items={entry.compositions}
                        label={`${entryTitle(entry)} compositions`}
                        onChange={(update): void => onChange((current) => ({ ...current, compositions: update(current.compositions) }))}
                        textValue={compositionLabel}
                    >
                        {(candidate): ReactNode => (
                            <Button
                                aria-pressed={item?.id === candidate.id}
                                className="wrap-anywhere block w-full min-w-0 py-2 text-left text-sm aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-4"
                                onPress={(): void => {
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
                {item && <CompositionEditor assets={assets} entry={entry} item={item} key={item.id} onChange={onChange} onHero={onHero} />}
            </div>
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Compositions };
