import type { ReactNode } from 'react';
import { Button, Menu, MenuItem, type MenuProps, MenuTrigger, Popover } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MenuButton = <T extends { readonly id: string; readonly label: string }>({ options, onAction, children, ...props }: Omit<MenuProps<T>, 'children' | 'items' | 'onAction'> & { readonly options: readonly T[]; readonly onAction: (choice: T) => void; readonly children: ReactNode }) => (
    <MenuTrigger>
        <Button isDisabled={options.length === 0}>{children}</Button>
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
