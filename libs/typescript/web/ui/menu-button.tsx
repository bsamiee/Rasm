import type { ReactNode } from 'react';
import { Button, Menu, MenuItem, MenuTrigger, Popover } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MenuButton = <T extends { readonly id: string; readonly label: string }>({ label, options, onAction, children }: { label: string; options: readonly T[]; onAction: (choice: T) => void; children: ReactNode }) => (
    <MenuTrigger>
        <Button isDisabled={options.length === 0}>{children}</Button>
        <Popover>
            <Menu aria-label={label} dependencies={[onAction]} items={options}>
                {(item) => (
                    <MenuItem id={item.id} onAction={() => onAction(item)} textValue={item.label}>
                        {item.label}
                    </MenuItem>
                )}
            </Menu>
        </Popover>
    </MenuTrigger>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { MenuButton };
