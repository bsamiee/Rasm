import { Array } from 'effect';
import { GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, GridList, GridListItem, type GridListProps, type ItemDropTarget, type Key, type Selection, useDragAndDrop } from 'react-aria-components';
import { Field } from '#/field';
import { MenuButton } from '#/menu-button';

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
    readonly actions: (keys: ReadonlySet<T['id']>) => ReactNode;
    readonly children: (item: T) => ReactNode;
    readonly moveLabel: string;
}) => {
    const [selection, setSelection] = useState<Selection>(new Set());
    const selected: ReadonlySet<T['id']> = new Set(items.filter(({ id }) => selection === 'all' || selection.has(id)).map(({ id }) => id));
    const reorder = (keys: ReadonlySet<Key>, { key, dropPosition }: Omit<ItemDropTarget, 'type'>) => {
        const [before, after] = Array.splitAt(items, items.findIndex(({ id }) => id === key) + (dropPosition === 'after' ? 1 : 0));
        onChange([...before.filter(({ id }) => !keys.has(id)), ...items.filter(({ id }) => keys.has(id)), ...after.filter(({ id }) => !keys.has(id))]);
    };
    const targets = items.map((target, index) => ({ id: target.id, label: textValue(target), index }));
    const { dragAndDropHooks } = useDragAndDrop<T>({
        getItems: (_keys, values) => values.map((item) => ({ 'text/plain': textValue(item) })),
        onReorder: ({ keys, target }) => reorder(keys, target),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList {...props} dependencies={[selection, items, children, textValue, onChange, moveLabel]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item) => {
                    const moved: ReadonlySet<T['id']> = selected.has(item.id) ? selected : new Set([item.id]);
                    return (
                        <GridListItem id={item.id} textValue={textValue(item)}>
                            <Field.Checkbox slot="selection" />
                            <Button slot="drag">
                                <GripVertical />
                            </Button>
                            <MenuButton disabledKeys={moved} onAction={({ id, index }) => reorder(moved, { key: id, dropPosition: index < items.findIndex((candidate) => moved.has(candidate.id)) ? 'before' : 'after' })} options={targets} trigger={{ 'aria-label': `${moveLabel} ${textValue(item)}` }}>
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
