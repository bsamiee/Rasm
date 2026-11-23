import type { ComponentProps } from 'react';
import { fn } from 'storybook/test';
import preview from '#/.storybook/preview';
import { MenuButton } from '#/menu-button.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: MenuButton,
    args: {
        'aria-label': 'Sort',
        options: [
            { id: 'name', label: 'Name' },
            { id: 'date', label: 'Date' },
        ],
        onAction: fn<ComponentProps<typeof MenuButton>['onAction']>(),
        children: 'Sort by',
    },
});

const Default = meta.story();
const Empty = meta.story({ args: { options: [] } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Default, Empty };
