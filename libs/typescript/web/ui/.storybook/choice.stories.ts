import { fn } from 'storybook/test';
import preview from '#/.storybook/preview.ts';
import { Choice } from '#/choice.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: Choice,
    args: {
        label: 'Scale',
        description: 'Width of the composition',
        options: [
            { id: 'narrow', label: 'Narrow' },
            { id: 'wide', label: 'Wide' },
            { id: 'full', label: 'Full', isDisabled: true },
        ],
        value: 'narrow',
        onChange: fn(),
        radios: 3,
    },
});

const Radios = meta.story();
const Horizontal = meta.story({ args: { orientation: 'horizontal' } });
const Select = meta.story({ args: { radios: 2 } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Horizontal, Radios, Select };
