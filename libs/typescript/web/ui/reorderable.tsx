import { Array, Filter } from 'effect';
import { Check, GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, CheckboxButton, CheckboxField, DropIndicator, GridList, GridListItem, type ItemDropTarget, type Key, Menu, MenuItem, MenuTrigger, Popover, type Selection, useDragAndDrop } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReorderableList = <T extends { readonly id: string }>({
    label,
    items,
    onChange,
    textValue,
    actions,
    children,
    classNames,
    dragLabel,
    moveLabel,
}: {
    label: string;
    items: readonly T[];
    onChange: (items: readonly T[]) => void;
    textValue: (item: T) => string;
    actions: (keys: ReadonlySet<Key>) => ReactNode;
    children: (item: T) => ReactNode;
    classNames: Readonly<Record<'list' | 'item' | 'controls' | 'selection' | 'checkbox' | 'check' | 'drag' | 'grip' | 'position' | 'move' | 'popover' | 'option' | 'dropIndicator', string>>;
    dragLabel: string;
    moveLabel: string;
}) => {
    const [selection, setSelection] = useState<Selection>(new Set());
    const order = items.map(({ id }) => id);
    const selected = new Set(order.filter((id) => selection === 'all' || selection.has(id)));
    const reorder = (keys: ReadonlySet<Key>, target: ItemDropTarget) => {
        const end = items.findIndex((item) => item.id === target.key) + (target.dropPosition === 'after' ? 1 : 0);
        const [moved, kept] = Array.partition(
            items,
            Filter.fromPredicate((item: T) => keys.has(item.id)),
        );
        const [before, after] = Array.splitAt(
            kept,
            Array.countBy(items, (item, index) => index < end && !keys.has(item.id)),
        );
        onChange([...before, ...moved, ...after]);
    };
    const { dragAndDropHooks } = useDragAndDrop({
        renderDropIndicator: (target) => <DropIndicator className={classNames.dropIndicator} target={target} />,
        getItems: (keys) => [...keys].map((key) => ({ 'text/plain': String(key) })),
        onReorder: (event) => reorder(event.keys, event.target),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList aria-label={label} className={classNames.list} dependencies={[items, children, textValue, selected]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item) => (
                    <GridListItem className={classNames.item} id={item.id} textValue={textValue(item)}>
                        <div className={classNames.controls}>
                            <CheckboxField slot="selection">
                                <CheckboxButton className={classNames.selection}>
                                    <span className={classNames.checkbox}>
                                        <Check className={classNames.check} />
                                    </span>
                                </CheckboxButton>
                            </CheckboxField>
                            <Button aria-label={`${dragLabel} ${textValue(item)}`} className={classNames.drag} slot="drag">
                                <GripVertical className={classNames.grip} />
                            </Button>
                            <span aria-hidden="true" className={classNames.position} />
                            <MenuTrigger>
                                <Button className={classNames.move} isDisabled={items.length <= (selected.has(item.id) ? selected.size : 1)}>
                                    {moveLabel}
                                </Button>
                                <Popover className={classNames.popover}>
                                    <Menu
                                        dependencies={[textValue]}
                                        disabledKeys={selected.has(item.id) ? selected : [item.id]}
                                        items={items}
                                        onAction={(key) => reorder(selected.has(item.id) ? selected : new Set([item.id]), { type: 'item', key, dropPosition: order.indexOf(String(key)) < order.indexOf(item.id) ? 'before' : 'after' })}
                                    >
                                        {(target) => (
                                            <MenuItem className={classNames.option} id={target.id} textValue={textValue(target)}>
                                                {textValue(target)}
                                            </MenuItem>
                                        )}
                                    </Menu>
                                </Popover>
                            </MenuTrigger>
                        </div>
                        {children(item)}
                    </GridListItem>
                )}
            </GridList>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReorderableList };
