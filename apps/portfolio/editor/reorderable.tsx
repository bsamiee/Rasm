import { Array } from 'effect';
import { GripVertical } from 'lucide-react';
import type { ReactNode } from 'react';
import { Button, DropIndicator, GridList, GridListItem, type ItemDropTarget, type Key, useDragAndDrop } from 'react-aria-components';
import { numeral } from '../site/navigation.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reorder = <T extends { readonly id: string }>(items: readonly T[], keys: Set<Key>, target: ItemDropTarget): readonly T[] => {
    const end = items.findIndex((item) => item.id === target.key) + (target.dropPosition === 'after' ? 1 : 0);
    const [before, after] = Array.splitAt(
        items.filter((item) => !keys.has(item.id)),
        Array.countBy(items, (item, index) => index < end && !keys.has(item.id)),
    );
    return [...before, ...items.filter((item) => keys.has(item.id)), ...after];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

function ReorderableList<T extends { readonly id: string }>({ label, items, onChange, textValue, children }: { label: string; items: readonly T[]; onChange: (update: (items: readonly T[]) => readonly T[]) => void; textValue: (item: T) => string; children: (item: T) => ReactNode }): ReactNode {
    const { dragAndDropHooks } = useDragAndDrop({
        renderDropIndicator: (target) => <DropIndicator className="h-0.5 bg-transparent drop-target:bg-accent-text" target={target} />,
        getItems: (keys) => [...keys].map((key) => ({ 'text/plain': String(key) })),
        onReorder: (event) => onChange((current) => reorder(current, event.keys, event.target)),
    });
    return (
        <GridList aria-label={label} dragAndDropHooks={dragAndDropHooks} items={items} selectionMode="none">
            {(item): ReactNode => {
                const index = items.indexOf(item);
                return (
                    <GridListItem className="border-line border-t bg-background dragging:opacity-50" id={item.id} textValue={textValue(item)}>
                        <div className="flex items-center gap-2.5 py-[9px] text-muted">
                            <Button aria-label={`Drag item ${index + 1}`} className="grid min-h-11 min-w-11 cursor-grab touch-none place-items-center" slot="drag">
                                <GripVertical className="size-5" />
                            </Button>
                            <span className="eyebrow mr-auto">{numeral(index + 1)}</span>
                        </div>
                        {children(item)}
                    </GridListItem>
                );
            }}
        </GridList>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReorderableList };
