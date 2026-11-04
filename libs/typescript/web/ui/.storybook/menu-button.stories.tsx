import { fn } from 'storybook/test';
import preview from '#/.storybook/preview';
import { MenuButton } from '#/menu-button';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: MenuButton,
    args: {
        'aria-label': 'Sheet',
        options: [
            { id: 'plan', label: 'Plan' },
            { id: 'section', label: 'Section' },
        ],
        onAction: fn(),
        children: 'Choose sheet',
    },
});

const Default = meta.story();
const Empty = meta.story({ args: { options: [] } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Default, Empty };
