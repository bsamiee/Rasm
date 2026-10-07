import { type ReactNode, useState } from 'react';
import { Button } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, placementLabel } from '../media/display.ts';
import type { Entry } from '../model/document.ts';
import type { Composition, Placement } from '../model/placement.ts';
import { ConfirmDialog, MenuButton, SelectField } from './controls.tsx';
import { PlacementFields } from './placement.tsx';
import { ReorderableList } from './reorderable.tsx';
import { randomId } from './services.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const scales = [
    { id: 'full', label: 'Full' },
    { id: 'medium', label: 'Medium' },
    { id: 'small', label: 'Small' },
] as const;
const alignments = [
    { id: 'left', label: 'Left' },
    { id: 'center', label: 'Center' },
    { id: 'right', label: 'Right' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

function CompositionEditor({ entry, item, onChange, onHero }: { entry: typeof Entry.Type; item: typeof Composition.Type; onChange: (entry: typeof Entry.Type) => void; onHero: (placement: typeof Placement.Type) => void }): ReactNode {
    const [first, second] = item.items;
    const replace = (...values: readonly (typeof Composition.Type)[]): void => onChange({ ...entry, compositions: entry.compositions.flatMap((candidate) => (candidate.id === item.id ? values : [candidate])) });
    const singles = entry.compositions.filter((candidate) => candidate.id !== item.id && candidate.items.length === 1);
    return (
        <div className="flex flex-col gap-[18px] border-foreground border-t pt-[25px]">
            <CompositionGrid active={false} composition={item} preview={true} renderMedia={true} />
            <div className="field-pair">
                <SelectField label="Scale" onChange={({ id }): void => replace({ ...item, scale: id })} options={scales} value={item.scale} />
                <SelectField label="Alignment" onChange={({ id }): void => replace({ ...item, align: id })} options={alignments} value={item.align} />
            </div>
            <PlacementFields onChange={(placement): void => replace({ ...item, items: second ? [placement, second] : [placement] })} onCover={(): void => onChange({ ...entry, cover: first })} onHero={(): void => onHero(first)} value={first} />
            {second && <PlacementFields onChange={(placement): void => replace({ ...item, items: [first, placement] })} onCover={(): void => onChange({ ...entry, cover: second })} onHero={(): void => onHero(second)} value={second} />}
            <div className="actions">
                {second ? (
                    <>
                        <button className="button button-outline" onClick={(): void => replace({ ...item, items: [second, first] })} type="button">
                            Swap pair order
                        </button>
                        <button className="button button-outline" onClick={(): void => replace({ ...item, items: [first] }, { ...item, id: randomId(), items: [second] })} type="button">
                            Unpair
                        </button>
                    </>
                ) : (
                    singles.length > 0 && (
                        <MenuButton
                            label="Pair with"
                            onAction={({ pair }): void => onChange({ ...entry, compositions: entry.compositions.filter((candidate) => candidate.id !== pair.id).map((candidate) => (candidate.id === item.id ? { ...candidate, items: [first, pair.items[0]] } : candidate)) })}
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
function Compositions({ entry, onChange, onHero, onUpload }: { entry: typeof Entry.Type; onChange: (entry: typeof Entry.Type) => void; onHero: (placement: typeof Placement.Type) => void; onUpload: () => void }): ReactNode {
    const [selected, setSelected] = useState<string>();
    const item = entry.compositions.find((candidate) => candidate.id === selected);
    return (
        <>
            <div className="actions">
                <h3 className="mt-2 text-[26px] tracking-[-0.035em]">
                    Compositions <span className="ml-2.5 text-muted text-sm">{entry.compositions.length}</span>
                </h3>
                <button className="button button-outline" onClick={onUpload} type="button">
                    Add files to this entry
                </button>
            </div>
            <p className="note">Each composition is one step in this project. Pair related sheets explicitly; drawings always retain their full extent.</p>
            <ReorderableList items={entry.compositions} label={`${entryTitle(entry)} compositions`} onChange={(compositions): void => onChange({ ...entry, compositions })} textValue={compositionLabel}>
                {(candidate): ReactNode => (
                    <Button aria-pressed={selected === candidate.id} className="wrap-anywhere block w-full pb-2.5 text-left text-sm aria-pressed:text-accent-text aria-pressed:underline aria-pressed:underline-offset-[5px]" onPress={(): void => setSelected(selected === candidate.id ? undefined : candidate.id)}>
                        {compositionLabel(candidate)}
                        <span className="meta-line no-underline">
                            {candidate.items.length === 2 ? 'Pair' : 'Single'} · {candidate.scale} · {candidate.align}
                        </span>
                    </Button>
                )}
            </ReorderableList>
            {item && <CompositionEditor entry={entry} item={item} key={item.id} onChange={onChange} onHero={onHero} />}
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Compositions };
