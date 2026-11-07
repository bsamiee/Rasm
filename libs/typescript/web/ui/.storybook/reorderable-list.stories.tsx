import type { StoryContext } from '@storybook/react-vite';
import { type ComponentProps, useState } from 'react';
import { Button } from 'react-aria-components';
import { expect, fn, mocked, screen, waitFor, within } from 'storybook/test';
import preview from '#/.storybook/preview.ts';
import { ReorderableList } from '#/reorderable-list.tsx';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sheet {
    readonly id: string;
    readonly title: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheets = ({ canvas, userEvent, args }: StoryContext<ComponentProps<typeof ReorderableList<Sheet>>>) => {
    const opened = (title: string) => userEvent.click(canvas.getByRole('button', { name: `Move ${title}` }));
    return {
        select: (title: string) => userEvent.click(within(canvas.getByRole('row', { name: title })).getByText(title)),
        opened,
        move: async (title: string, target: string) => {
            await opened(title);
            await userEvent.click(await screen.findByRole('menuitem', { name: target }));
        },
        rows: () => canvas.getAllByRole('row').map((row) => row.dataset.key),
        order: () => mocked(args.onChange).mock.lastCall?.[0].map(({ id }) => id),
        disabled: () => args.items.map(({ title }) => canvas.getByRole('button', { name: `Move ${title}` }).hasAttribute('disabled')),
        targets: () =>
            screen
                .getAllByRole('menuitem')
                .filter((item) => item.getAttribute('aria-disabled') !== 'true')
                .map(({ textContent }) => textContent),
        status: () => canvas.queryByRole('status'),
    };
};

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

const Default = meta.story({
    play: (context) => {
        const { canvas, args } = context;
        const grid = within(canvas.getByRole('grid', { name: 'Sheets' }));
        expect(sheets(context).rows()).toEqual(args.items.map(({ id }) => id));
        const controls = args.items.flatMap(({ title }) => {
            const row = within(grid.getByRole('row', { name: title }));
            return [row.getByRole('checkbox', { name: `Select ${title}` }), row.getByRole('button', { name: `Drag ${title}` }), row.getByRole('button', { name: `Move ${title}` })];
        });
        expect(controls.every((control) => control.checkVisibility())).toBe(true);
    },
});
const Single = meta.story({
    args: { items: [{ id: 'plan', title: 'Plan' }] },
    play: async ({ canvas }) => {
        await expect(canvas.getByRole('button', { name: 'Move Plan' })).toBeDisabled();
    },
});
const MoveMenu = meta.story({
    play: async (context) => {
        const { move, rows, order } = sheets(context);
        await move('Plan', 'Detail');
        expect(order()).toEqual(['section', 'detail', 'plan']);
        await move('Plan', 'Section');
        expect(order()).toEqual(['plan', 'section', 'detail']);
        await waitFor(() => expect(rows()).toEqual(['plan', 'section', 'detail']));
    },
});
const BlockMove = meta.story({
    play: async (context) => {
        const { select, move, order } = sheets(context);
        await select('Plan');
        await select('Section');
        await move('Plan', 'Detail');
        expect(order()).toEqual(['detail', 'plan', 'section']);
    },
});
const GatheredSelection = meta.story({
    play: async (context) => {
        const { select, opened, targets, order } = sheets(context);
        await select('Plan');
        await select('Detail');
        await opened('Detail');
        await waitFor(() => expect(targets()).toEqual(['Section']));
        await context.userEvent.click(screen.getByRole('menuitem', { name: 'Section' }));
        expect(order()).toEqual(['section', 'plan', 'detail']);
    },
});
const UnselectedMove = meta.story({
    play: async (context) => {
        const { select, move, rows, order, status } = sheets(context);
        await select('Plan');
        await move('Detail', 'Plan');
        expect(order()).toEqual(['detail', 'plan', 'section']);
        await waitFor(() => expect(rows()).toEqual(['detail', 'plan', 'section']));
        await expect(status()).toHaveTextContent('1 selected');
    },
});
const SelectionActions = meta.story({
    play: async (context) => {
        const { select, status } = sheets(context);
        await expect(status()).not.toBeInTheDocument();
        await select('Section');
        await waitFor(() => expect(status()).toHaveTextContent('1 selected'));
        await select('Section');
        await waitFor(() => expect(status()).not.toBeInTheDocument());
    },
});
const FullSelection = meta.story({
    play: async (context) => {
        const { select, disabled } = sheets(context);
        await select('Plan');
        await select('Section');
        await select('Detail');
        await waitFor(() => expect(disabled()).toEqual([true, true, true]));
    },
});
const KeyboardReorder = meta.story({
    play: async (context) => {
        const { canvas, userEvent } = context;
        const { order } = sheets(context);
        const handle = canvas.getByRole('button', { name: 'Drag Plan' });
        await userEvent.tab();
        await userEvent.keyboard('{ArrowRight}{ArrowRight}');
        await expect(handle).toHaveFocus();
        await userEvent.keyboard('{Enter}');
        await waitFor(() => expect(handle).not.toHaveFocus());
        await userEvent.keyboard('{ArrowDown}{Enter}');
        await waitFor(() => expect(order()).toEqual(['section', 'plan', 'detail']));
    },
});
const OwnerRemoval = meta.story({
    render: function Owner({ items, ...args }) {
        const [present, setPresent] = useState(items);
        return (
            <>
                <Button onPress={() => setPresent(present.filter(({ id }) => id !== 'detail'))}>Remove Detail</Button>
                <ReorderableList {...args} items={present} />
            </>
        );
    },
    play: async (context) => {
        const { select, opened, targets, status } = sheets(context);
        await select('Detail');
        await select('Plan');
        await waitFor(() => expect(status()).toHaveTextContent('2 selected'));
        await context.userEvent.click(context.canvas.getByRole('button', { name: 'Remove Detail' }));
        await waitFor(() => expect(status()).toHaveTextContent('1 selected'));
        await opened('Plan');
        await waitFor(() => expect(targets()).toEqual(['Section']));
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { BlockMove, Default, FullSelection, GatheredSelection, KeyboardReorder, MoveMenu, OwnerRemoval, SelectionActions, Single, UnselectedMove };
