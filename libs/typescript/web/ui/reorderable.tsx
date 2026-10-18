import { Array, Filter } from 'effect';
import { GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, GridList, GridListItem, type GridListProps, type ItemDropTarget, type Key, Menu, MenuItem, MenuTrigger, Popover, type Selection, useDragAndDrop } from 'react-aria-components';
import { Checkbox } from './checkbox.tsx';

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
    items: readonly T[];
    onChange: (items: readonly T[]) => void;
    textValue: (item: T) => string;
    actions: (keys: ReadonlySet<Key>) => ReactNode;
    children: (item: T) => ReactNode;
    moveLabel: string;
}) => {
    const [selection, setSelection] = useState<Selection>(new Set());
    const selected = new Set(items.flatMap(({ id }) => (selection === 'all' || selection.has(id) ? [id] : [])));
    const reorder = (keys: ReadonlySet<Key>, { key, dropPosition }: ItemDropTarget) => {
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
    const { dragAndDropHooks } = useDragAndDrop({
        getItems: (keys) => [...keys].map((key) => ({ 'text/plain': String(key) })),
        onReorder: ({ keys, target }) => reorder(keys, target),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList {...props} dependencies={[items, children, textValue, selected]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item) => {
                    const moved = selected.has(item.id) ? selected : new Set([item.id]);
                    const targets = items.filter(({ id }) => !moved.has(id));
                    return (
                        <GridListItem id={item.id} textValue={textValue(item)}>
                            <Checkbox slot="selection" />
                            <Button slot="drag">
                                <GripVertical />
                            </Button>
                            <MenuTrigger>
                                <Button isDisabled={targets.length === 0}>{moveLabel}</Button>
                                <Popover>
                                    <Menu items={targets} onAction={(key) => reorder(moved, { type: 'item', key, dropPosition: items.findIndex(({ id }) => id === key) < items.indexOf(item) ? 'before' : 'after' })}>
                                        {(target) => <MenuItem>{textValue(target)}</MenuItem>}
                                    </Menu>
                                </Popover>
                            </MenuTrigger>
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
