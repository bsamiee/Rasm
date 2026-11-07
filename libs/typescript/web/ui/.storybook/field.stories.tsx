import preview from '#/.storybook/preview';
import { Field } from '#/field';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({ component: Field, args: { label: 'Title', description: 'Shown above the entry', placeholder: 'Untitled entry' } });

const Text = meta.story();
const Multiline = meta.story({ args: { label: 'Description', multiline: true } });
const Slider = meta.story({ render: () => <Field.Slider defaultValue={50} label="Horizontal focal point" /> });
const Range = meta.story({ render: () => <Field.Slider defaultValue={[0, 10]} label="Visible range" maxValue={10} thumbs={['Start', 'End']} /> });
const Checkbox = meta.story({ render: () => <Field.Checkbox>Publish</Field.Checkbox> });
const Indeterminate = meta.story({ render: () => <Field.Checkbox isIndeterminate={true}>Every sheet</Field.Checkbox> });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Checkbox, Indeterminate, Multiline, Range, Slider, Text };
