import { type ComponentProps, useState } from 'react';
import { fn } from 'storybook/test';
import { ReorderableList } from '../reorderable.tsx';
import preview from './preview.ts';

// --- [MODELS] --------------------------------------------------------------------------

interface Sheet {
    readonly id: string;
    readonly title: string;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Sheets = ({ items, onChange, ...props }: ComponentProps<typeof ReorderableList<Sheet>>) => {
    const [order, setOrder] = useState(items);
    return (
        <ReorderableList
            {...props}
            items={order}
            onChange={(next) => {
                setOrder(next);
                onChange(next);
            }}
        />
    );
};

const meta = preview.meta({
    component: Sheets,
    args: {
        'aria-label': 'Sheets',
        items: [
            { id: 'plan', title: 'Plan' },
            { id: 'section', title: 'Section' },
            { id: 'detail', title: 'Detail' },
        ],
        onChange: fn(),
        textValue: ({ title }) => title,
        actions: (keys) => <output>{`${keys.size} selected`}</output>,
        children: ({ title }) => <span>{title}</span>,
        moveLabel: 'Move',
    },
});

const Default = meta.story();
const Single = meta.story({ args: { items: [{ id: 'plan', title: 'Plan' }] } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Default, Single };
