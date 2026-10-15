import { Array, Struct } from 'effect';
import { Check, GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, CheckboxButton, CheckboxField, DropIndicator, GridList, GridListItem, type ItemDropTarget, type Key, Menu, MenuItem, MenuTrigger, Popover, type Selection, useDragAndDrop } from 'react-aria-components';

// --- [TYPES] ---------------------------------------------------------------------------

type Part = 'list' | 'item' | 'controls' | 'selection' | 'checkbox' | 'check' | 'drag' | 'grip' | 'position' | 'move' | 'popover' | 'option' | 'dropIndicator';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reorder =
    (keys: ReadonlySet<Key>, target: ItemDropTarget) =>
    <T extends { readonly id: string }>(items: readonly T[]): readonly T[] => {
        const end = items.findIndex((item) => item.id === target.key) + (target.dropPosition === 'after' ? 1 : 0);
        const [before, after] = Array.splitAt(
            items.filter((item) => !keys.has(item.id)),
            Array.countBy(items, (item, index) => index < end && !keys.has(item.id)),
        );
        return [...before, ...items.filter((item) => keys.has(item.id)), ...after];
    };

// --- [COMPOSITION] ---------------------------------------------------------------------

function ReorderableList<T extends { readonly id: string }>({
    label,
    items,
    onChange,
    textValue,
    actions,
    children,
    classNames,
    dragLabel,
    moveLabel,
    moveMenuLabel,
}: {
    label: string;
    items: readonly T[];
    onChange: (update: (items: readonly T[]) => readonly T[]) => void;
    textValue: (item: T) => string;
    actions: (keys: ReadonlySet<Key>) => ReactNode;
    children: (item: T) => ReactNode;
    classNames: Readonly<Record<Part, string>>;
    dragLabel: string;
    moveLabel: string;
    moveMenuLabel: string;
}): ReactNode {
    const [selection, setSelection] = useState<Selection>(new Set());
    const selected = new Set(items.filter((item) => selection === 'all' || selection.has(item.id)).map(Struct.get('id')));
    const { dragAndDropHooks } = useDragAndDrop({
        renderDropIndicator: (target) => <DropIndicator className={classNames.dropIndicator} target={target} />,
        getItems: (keys) => [...keys].map((key) => ({ 'text/plain': String(key) })),
        onReorder: (event) => onChange(reorder(event.keys, event.target)),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList aria-label={label} className={classNames.list} dependencies={[items, children, textValue, selected]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item): ReactNode => {
                    const keys = selected.has(item.id) ? selected : new Set([item.id]);
                    return (
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
                                    <Button className={classNames.move} isDisabled={items.length <= keys.size}>
                                        {moveLabel}
                                    </Button>
                                    <Popover className={classNames.popover}>
                                        <Menu
                                            aria-label={moveMenuLabel}
                                            dependencies={[textValue]}
                                            disabledKeys={keys}
                                            items={items}
                                            onAction={(key): void => {
                                                const order = items.map(Struct.get('id'));
                                                onChange(reorder(keys, { type: 'item', key, dropPosition: order.indexOf(String(key)) < order.indexOf(item.id) ? 'before' : 'after' }));
                                            }}
                                        >
                                            {(target): ReactNode => (
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
                    );
                }}
            </GridList>
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReorderableList };
