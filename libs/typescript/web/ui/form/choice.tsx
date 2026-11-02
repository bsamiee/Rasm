import { Array, Option } from 'effect';
import { ChevronDown } from 'lucide-react';
import { Button, FieldError, type Key, Label, ListBox, ListBoxItem, Popover, RadioButton, RadioField, RadioGroup, type RadioGroupProps, Select, type SelectProps, SelectValue, Text } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Choice = <T extends { readonly id: string; readonly label: string }>({
    label,
    description,
    options,
    onChange,
    inline = 3,
    ...props
}: Omit<Pick<RadioGroupProps, keyof RadioGroupProps & keyof SelectProps<T>>, 'children' | 'className' | 'style' | 'render' | 'value' | 'defaultValue' | 'onChange' | 'validate'> & {
    readonly label: string;
    readonly description?: string;
    readonly options: readonly T[];
    readonly value: T['id'];
    readonly onChange: (choice: T) => void;
    readonly inline?: number;
}) => {
    const choose = (key: Key | null) =>
        Option.map(
            Array.findFirst(options, ({ id }) => id === key),
            onChange,
        );
    const help = (
        <>
            {description === undefined ? null : <Text slot="description">{description}</Text>}
            <FieldError />
        </>
    );
    return options.length <= inline ? (
        <RadioGroup {...props} onChange={choose} orientation="horizontal">
            <Label>{label}</Label>
            {options.map((option) => (
                <RadioField key={option.id} value={option.id}>
                    <RadioButton>{option.label}</RadioButton>
                </RadioField>
            ))}
            {help}
        </RadioGroup>
    ) : (
        <Select {...props} onChange={choose}>
            <Label>{label}</Label>
            <Button>
                <SelectValue />
                <ChevronDown />
            </Button>
            {help}
            <Popover>
                <ListBox items={options}>
                    {(option) => (
                        <ListBoxItem id={option.id} textValue={option.label}>
                            {option.label}
                        </ListBoxItem>
                    )}
                </ListBox>
            </Popover>
        </Select>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Choice };
