import { Array } from 'effect';
import { Check } from 'lucide-react';
import type { ReactNode } from 'react';
import { CheckboxButton, CheckboxField, type CheckboxFieldProps, FieldError, Input, Label, Slider, SliderOutput, type SliderProps, SliderThumb, SliderTrack, Text, TextArea, TextField, type TextFieldProps } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Field = ({ label, description, multiline = false, placeholder, ...props }: Omit<TextFieldProps, 'children'> & { readonly label: string; readonly description?: string; readonly multiline?: boolean; readonly placeholder?: string }) => (
    <TextField {...props}>
        <Label>{label}</Label>
        {multiline ? <TextArea placeholder={placeholder} /> : <Input {...(placeholder === undefined ? {} : { placeholder })} />}
        {description === undefined ? null : <Text slot="description">{description}</Text>}
        <FieldError />
    </TextField>
);
const SliderField = <T extends number | number[]>({ label, ...props }: Omit<SliderProps<T>, 'children'> & { readonly label: string }) => (
    <Slider {...props}>
        <Label>{label}</Label>
        <SliderOutput />
        <SliderTrack>{({ state }) => Array.makeBy(state.values.length, (index) => <SliderThumb index={index} key={index} />)}</SliderTrack>
    </Slider>
);
const Checkbox = ({ children, ...props }: Omit<CheckboxFieldProps, 'children'> & { readonly children?: ReactNode }) => (
    <CheckboxField {...props}>
        <CheckboxButton>
            <Check />
            {children}
        </CheckboxButton>
    </CheckboxField>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Checkbox, Field, SliderField };
