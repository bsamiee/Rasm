import { Array, Filter } from 'effect';
import { GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, GridList, GridListItem, type GridListProps, type ItemDropTarget, type Key, type Selection, useDragAndDrop } from 'react-aria-components';
import { MenuButton } from '#/collection/menu-button';
import { Checkbox } from '#/form/field';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReorderableList = <T extends { readonly id: string }>({
    items,
    onChange,
    textValue,
    actions,
    children,
    moveLabel,
    ...props
}: Omit<GridListProps<T>, 'children' | 'defaultSelectedKeys' | 'dependencies' | 'dragAndDropHooks' | 'items' | 'onSelectionChange' | 'selectedKeys' | 'selectionMode'> & {
    readonly items: readonly T[];
    readonly onChange: (items: readonly T[]) => void;
    readonly textValue: (item: T) => string;
    readonly actions: (keys: ReadonlySet<Key>) => ReactNode;
    readonly children: (item: T) => ReactNode;
    readonly moveLabel: string;
}) => {
    const [selection, setSelection] = useState<Selection>(new Set());
    const selected: ReadonlySet<Key> = new Set(items.flatMap(({ id }) => (selection === 'all' || selection.has(id) ? [id] : [])));
    const reorder = (keys: ReadonlySet<Key>, { key, dropPosition }: Omit<ItemDropTarget, 'type'>) => {
        const [moved, kept] = Array.partition(
            items,
            Filter.fromPredicate((item: T) => keys.has(item.id)),
        );
        const [before, after] = Array.splitAt(
            kept,
            Array.countBy(Array.take(items, items.findIndex(({ id }) => id === key) + (dropPosition === 'after' ? 1 : 0)), ({ id }) => !keys.has(id)),
        );
        onChange([...before, ...moved, ...after]);
    };
    const targets = items.map((target, index) => ({ id: target.id, label: textValue(target), index }));
    const { dragAndDropHooks } = useDragAndDrop<T>({
        getItems: (_keys, values) => values.map((item) => ({ 'text/plain': textValue(item) })),
        onReorder: ({ keys, target }) => reorder(keys, target),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList {...props} dependencies={[items, children, textValue, selected]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item) => {
                    const moved = selected.has(item.id) ? selected : new Set([item.id]);
                    const position = items.indexOf(item);
                    return (
                        <GridListItem id={item.id} textValue={textValue(item)}>
                            <Checkbox slot="selection" />
                            <Button slot="drag">
                                <GripVertical />
                            </Button>
                            <MenuButton onAction={({ id, index }) => reorder(moved, { key: id, dropPosition: index < position ? 'before' : 'after' })} options={targets.filter(({ id }) => !moved.has(id))}>
                                {moveLabel}
                            </MenuButton>
                            {children(item)}
                        </GridListItem>
                    );
                }}
            </GridList>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReorderableList };
