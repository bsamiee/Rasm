import { Array, Option } from 'effect';
import { ChevronDown } from 'lucide-react';
import { Button, Label, ListBox, ListBoxItem, Popover, Select, SelectValue } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const SelectField = <T extends { readonly id: string; readonly label: string }>({ label, value, options, onChange }: { label: string; value: T['id']; options: readonly T[]; onChange: (choice: T) => void }) => (
    <Select
        onChange={(key) =>
            Option.map(
                Array.findFirst(options, (item) => item.id === key),
                onChange,
            )
        }
        value={value}
    >
        <Label>{label}</Label>
        <Button>
            <SelectValue />
            <ChevronDown />
        </Button>
        <Popover>
            <ListBox items={options}>
                {(item) => (
                    <ListBoxItem id={item.id} textValue={item.label}>
                        {item.label}
                    </ListBoxItem>
                )}
            </ListBox>
        </Popover>
    </Select>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { SelectField };
