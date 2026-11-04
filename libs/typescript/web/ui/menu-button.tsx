import type { ReactNode } from 'react';
import { Button, type ButtonProps, Menu, MenuItem, type MenuProps, MenuTrigger, Popover } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MenuButton = <T extends { readonly id: string; readonly label: string }>({
    options,
    onAction,
    trigger,
    children,
    ...props
}: Omit<MenuProps<T>, 'children' | 'items' | 'onAction'> & { readonly options: readonly T[]; readonly onAction: (choice: T) => void; readonly trigger?: Omit<ButtonProps, 'children' | 'isDisabled'>; readonly children: ReactNode }) => (
    <MenuTrigger>
        <Button {...trigger} isDisabled={new Set(props.disabledKeys).isSupersetOf(new Set(options.map(({ id }) => id)))}>
            {children}
        </Button>
        <Popover>
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { MenuButton };
