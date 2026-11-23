import { Array, Option, Record } from 'effect';
import { Check, Minus } from 'lucide-react';
import type { ReactNode, Ref } from 'react';
import { CheckboxButton, CheckboxField, type CheckboxFieldProps, FieldError, Input, Label, Slider, SliderOutput, type SliderProps, SliderThumb, SliderTrack, Text, TextArea, TextField, type TextFieldProps } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Field = Object.assign(
    ({
        label,
        description,
        multiline,
        control,
        placeholder,
        ...props
    }: Omit<TextFieldProps, 'children'> & { readonly label: string; readonly description?: string; readonly placeholder?: string } & ({ readonly multiline?: false; readonly control?: Ref<HTMLInputElement> } | { readonly multiline: true; readonly control?: Ref<HTMLTextAreaElement> })) => {
        const entry = Record.getSomes({ placeholder: Option.fromUndefinedOr(placeholder) });
        return (
            <TextField {...props}>
                <Label>{label}</Label>
                {multiline === true ? <TextArea {...entry} ref={control} /> : <Input {...entry} ref={control} />}
                <Field.Help description={description} />
            </TextField>
        );
    },
    {
        Help: ({ description }: { readonly description?: string | undefined }) => (
            <>
                {description === undefined ? null : <Text slot="description">{description}</Text>}
                <FieldError />
            </>
        ),
        Slider: <T extends number | number[]>({ label, thumbs, ...props }: Omit<SliderProps<T>, 'children'> & { readonly label: string; readonly thumbs?: readonly string[] }) => (
            <Slider {...props}>
                <Label>{label}</Label>
                <SliderOutput />
                <SliderTrack>{({ state }) => Array.makeBy(state.values.length, (index) => <SliderThumb {...Record.getSomes({ 'aria-label': Option.fromUndefinedOr(thumbs?.[index]) })} index={index} key={index} />)}</SliderTrack>
            </Slider>
        ),
        Checkbox: ({ children, ...props }: Omit<CheckboxFieldProps, 'children'> & { readonly children?: ReactNode }) => (
            <CheckboxField {...props}>
                <CheckboxButton>
                    {({ isIndeterminate }) => (
                        <>
                            {isIndeterminate ? <Minus /> : <Check />}
                            {children}
                        </>
                    )}
                </CheckboxButton>
            </CheckboxField>
        ),
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Field };
