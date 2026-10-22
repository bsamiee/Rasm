import { Input, Label, Text, TextArea, TextField, type TextFieldProps } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Field = ({ label, placeholder, description, multiline, ...props }: Omit<TextFieldProps, 'children'> & { readonly label: string; readonly placeholder?: string; readonly description?: string; readonly multiline?: boolean }) => (
    <TextField {...props}>
        <Label>{label}</Label>
        {multiline === true ? <TextArea placeholder={placeholder} /> : <Input {...(placeholder === undefined ? {} : { placeholder })} />}
        {description === undefined ? null : <Text slot="description">{description}</Text>}
    </TextField>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Field };
