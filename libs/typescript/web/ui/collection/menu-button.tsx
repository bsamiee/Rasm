import type { ReactNode } from 'react';
import { Button, type ButtonProps, Menu, MenuItem, type MenuProps, MenuTrigger, Popover, type PopoverProps } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MenuButton = <T extends { readonly id: string; readonly label: string }>({
    options,
    onAction,
    trigger,
    popover,
    children,
    ...props
}: Omit<MenuProps<T>, 'children' | 'items' | 'onAction'> & { readonly options: readonly T[]; readonly onAction: (choice: T) => void; readonly trigger?: Omit<ButtonProps, 'children'>; readonly popover?: Omit<PopoverProps, 'children'>; readonly children: ReactNode }) => {
    const disabled = new Set(props.disabledKeys);
    return (
        <MenuTrigger>
            <Button {...trigger} isDisabled={(trigger?.isDisabled ?? false) || options.every(({ id }) => disabled.has(id))}>
                {children}
            </Button>
            <Popover {...popover}>
                <Menu {...props} items={options} onAction={(_key, choice) => onAction(choice)}>
                    {(option) => (
                        <MenuItem id={option.id} textValue={option.label}>
                            {option.label}
                        </MenuItem>
                    )}
                </Menu>
            </Popover>
        </MenuTrigger>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { MenuButton };
