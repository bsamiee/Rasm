import { Array, Struct } from 'effect';
import { Check, GripVertical } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { Button, CheckboxButton, CheckboxField, DropIndicator, GridList, GridListItem, type ItemDropTarget, type Key, Menu, MenuItem, MenuTrigger, Popover, type Selection, useDragAndDrop } from 'react-aria-components';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reorder =
    (keys: Set<Key>, target: ItemDropTarget) =>
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
}: {
    label: string;
    items: readonly T[];
    onChange: (update: (items: readonly T[]) => readonly T[]) => void;
    textValue: (item: T) => string;
    actions: (keys: ReadonlySet<Key>) => ReactNode;
    children: (item: T) => ReactNode;
}): ReactNode {
    const [selection, setSelection] = useState<Selection>(new Set());
    const selected = new Set(items.filter((item) => selection === 'all' || selection.has(item.id)).map(Struct.get('id')));
    const { dragAndDropHooks } = useDragAndDrop({
        renderDropIndicator: (target) => <DropIndicator className="h-0.5 bg-transparent drop-target:bg-accent-text" target={target} />,
        getItems: (keys) => [...keys].map((key) => ({ 'text/plain': String(key) })),
        onReorder: (event) => onChange(reorder(event.keys, event.target)),
    });
    return (
        <>
            {selected.size > 0 && actions(selected)}
            <GridList aria-label={label} className="[counter-reset:item]" dependencies={[items, children, textValue, selected]} dragAndDropHooks={dragAndDropHooks} items={items} onSelectionChange={setSelection} selectedKeys={selection} selectionMode="multiple">
                {(item): ReactNode => {
                    const keys = selected.has(item.id) ? selected : new Set([item.id]);
                    return (
                        <GridListItem className="border-line border-t bg-background dragging:opacity-50 [counter-increment:item]" id={item.id} textValue={textValue(item)}>
                            <div className="flex items-center gap-1 py-1 text-muted">
                                <CheckboxField slot="selection">
                                    <CheckboxButton className="group grid min-h-11 min-w-11 place-items-center">
                                        <span className="grid size-5 place-items-center border border-control-line group-selected:border-accent-text group-selected:bg-accent-text group-selected:text-background">
                                            <Check className="size-3.5 opacity-0 group-selected:opacity-100" />
                                        </span>
                                    </CheckboxButton>
                                </CheckboxField>
                                <Button aria-label={`Drag ${textValue(item)}`} className="grid min-h-11 min-w-11 cursor-grab touch-none place-items-center" slot="drag">
                                    <GripVertical className="size-5" />
                                </Button>
                                <span aria-hidden="true" className="before:content-['['_counter(item,decimal-leading-zero)_']'] mr-auto whitespace-nowrap font-mono text-muted text-xs" />
                                <MenuTrigger>
                                    <Button className="button button-ghost px-2 text-xs" isDisabled={items.length <= keys.size}>
                                        Move
                                    </Button>
                                    <Popover className="max-h-[50vh] max-w-[min(480px,calc(100vw-32px))] overflow-auto border border-control-line bg-background">
                                        <Menu
                                            aria-label="Move to position"
                                            dependencies={[textValue]}
                                            disabledKeys={keys}
                                            items={items}
                                            onAction={(key): void => {
                                                const order = items.map(Struct.get('id'));
                                                onChange(reorder(keys, { type: 'item', key, dropPosition: order.indexOf(String(key)) < order.indexOf(item.id) ? 'before' : 'after' }));
                                            }}
                                        >
                                            {(target): ReactNode => (
                                                <MenuItem className="wrap-anywhere min-h-11 cursor-pointer px-4 py-2.5 focus:bg-foreground focus:text-background disabled:opacity-40" id={target.id} textValue={textValue(target)}>
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
