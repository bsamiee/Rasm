import { Array, Option } from 'effect';
import { ChevronDown } from 'lucide-react';
import { Button, type Key, Label, ListBox, ListBoxItem, Popover, RadioButton, RadioField, RadioGroup, type RadioGroupProps, Select, type SelectProps, SelectValue } from 'react-aria-components';
import { Field } from '#/form/field';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Choice = <T extends { readonly id: string; readonly label: string; readonly isDisabled?: boolean }>({
    label,
    description,
    options,
    onChange,
    radios,
    orientation = 'vertical',
    ...props
}: Omit<Pick<RadioGroupProps, keyof RadioGroupProps & keyof SelectProps<T>>, 'children' | 'className' | 'style' | 'render' | 'value' | 'defaultValue' | 'onChange' | 'validate'> &
    Pick<RadioGroupProps, 'orientation'> & {
        readonly label: string;
        readonly description?: string;
        readonly options: readonly T[];
        readonly value: T['id'];
        readonly onChange: (choice: T) => void;
        readonly radios: number;
    }) => {
    const choose = (key: Key | null) => Array.findFirst(options, ({ id }) => id === key).pipe(Option.map(onChange));
    return options.length <= radios ? (
        <RadioGroup {...props} onChange={choose} orientation={orientation}>
            <Label>{label}</Label>
            {options.map((option) => (
                <RadioField isDisabled={option.isDisabled ?? false} key={option.id} value={option.id}>
                    <RadioButton>{option.label}</RadioButton>
                </RadioField>
            ))}
            <Field.Help description={description} />
        </RadioGroup>
    ) : (
        <Select {...props} onChange={choose}>
            <Label>{label}</Label>
            <Button>
                <SelectValue />
                <ChevronDown />
            </Button>
            <Field.Help description={description} />
            <Popover>
                <ListBox items={options}>
                    {(option) => (
                        <ListBoxItem id={option.id} isDisabled={option.isDisabled ?? false} textValue={option.label}>
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
