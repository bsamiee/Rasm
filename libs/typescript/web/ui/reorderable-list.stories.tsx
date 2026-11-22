import type { StoryContext } from '@storybook/react-vite';
import { Array } from 'effect';
import { type ComponentProps, useState } from 'react';
import { Button } from 'react-aria-components';
import { expect, fn, mocked, screen, waitFor, within } from 'storybook/test';
import preview from '#/.storybook/preview.ts';
import { ReorderableList } from '#/reorderable-list.tsx';

// --- [TYPES] ---------------------------------------------------------------------------

interface Item {
    readonly id: string;
    readonly title: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const list = ({ canvas, userEvent, args }: StoryContext<ComponentProps<typeof ReorderableList<Item>>>) => {
    const opened = (title: string) => userEvent.click(canvas.getByRole('button', { name: `Move ${title}` }));
    return {
        select: (title: string) => userEvent.click(within(canvas.getByRole('row', { name: title })).getByText(title)),
        opened,
        move: async (title: string, target: string) => {
            await opened(title);
            await userEvent.click(await screen.findByRole('menuitem', { name: target }));
        },
        rows: () => canvas.getAllByRole('row').map((row) => row.dataset['key']),
        order: () => mocked(args.onChange).mock.lastCall?.[0].map(({ id }) => id),
        disabled: () => args.items.map(({ title }) => canvas.getByRole('button', { name: `Move ${title}` }).hasAttribute('disabled')),
        targets: () =>
            screen
                .getAllByRole('menuitem')
                .filter((item) => item.getAttribute('aria-disabled') !== 'true')
                .map(({ textContent }) => textContent),
        status: (): HTMLElement | null => canvas.queryByRole('status'),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const Owner = ({ items, ...args }: ComponentProps<typeof ReorderableList<Item>>) => {
    const [present, setPresent] = useState(items);
    return (
        <>
            <Button onPress={() => setPresent(present.filter(({ id }) => id !== 'gamma'))}>Remove Gamma</Button>
            <ReorderableList {...args} items={present} />
        </>
    );
};
const Items = ({ items, onChange, ...props }: ComponentProps<typeof ReorderableList<Item>>) => {
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
    component: Items,
    args: {
        'aria-label': 'Items',
        items: [
            { id: 'alpha', title: 'Alpha' },
            { id: 'beta', title: 'Beta' },
            { id: 'gamma', title: 'Gamma' },
        ],
        onChange: fn<ComponentProps<typeof ReorderableList<Item>>['onChange']>(),
        textValue: ({ title }) => title,
        actions: (keys) => <output>{`${keys.size} selected`}</output>,
        children: ({ title }) => <span>{title}</span>,
        moveLabel: 'Move',
    },
});

const Default = meta.story({
    play: async (context) => {
        const { canvas, args } = context;
        const grid = within(canvas.getByRole('grid', { name: 'Items' }));
        await expect(list(context).rows()).toEqual(args.items.map(({ id }) => id));
        const controls = args.items.flatMap(({ title }) => {
            const row = within(grid.getByRole('row', { name: title }));
            return Array.make(row.getByRole('checkbox', { name: `Select ${title}` }), row.getByRole('button', { name: `Drag ${title}` }), row.getByRole('button', { name: `Move ${title}` }));
        });
        await expect(controls.every((control) => control.checkVisibility())).toBe(true);
    },
});
const Single = meta.story({
    args: { items: [{ id: 'alpha', title: 'Alpha' }] },
    play: async ({ canvas }) => {
        await expect(canvas.getByRole('button', { name: 'Move Alpha' })).toBeDisabled();
    },
});
const MoveMenu = meta.story({
    play: async (context) => {
        const { move, rows, order } = list(context);
        await move('Alpha', 'Gamma');
        await expect(order()).toEqual(['beta', 'gamma', 'alpha']);
        await move('Alpha', 'Beta');
        await expect(order()).toEqual(['alpha', 'beta', 'gamma']);
        await waitFor(() => expect(rows()).toEqual(['alpha', 'beta', 'gamma']));
    },
});
const BlockMove = meta.story({
    play: async (context) => {
        const { select, move, order } = list(context);
        await select('Alpha');
        await select('Beta');
        await move('Alpha', 'Gamma');
        await expect(order()).toEqual(['gamma', 'alpha', 'beta']);
    },
});
const GatheredSelection = meta.story({
    play: async (context) => {
        const { select, opened, targets, order } = list(context);
        await select('Alpha');
        await select('Gamma');
        await opened('Gamma');
        await waitFor(() => expect(targets()).toEqual(['Beta']));
        await context.userEvent.click(screen.getByRole('menuitem', { name: 'Beta' }));
        await expect(order()).toEqual(['beta', 'alpha', 'gamma']);
    },
});
const UnselectedMove = meta.story({
    play: async (context) => {
        const { select, move, rows, order, status } = list(context);
        await select('Alpha');
        await move('Gamma', 'Alpha');
        await expect(order()).toEqual(['gamma', 'alpha', 'beta']);
        await waitFor(() => expect(rows()).toEqual(['gamma', 'alpha', 'beta']));
        await expect(status()).toHaveTextContent('1 selected');
    },
});
const SelectionActions = meta.story({
    play: async (context) => {
        const { select, status } = list(context);
        await expect(status()).not.toBeInTheDocument();
        await select('Beta');
        await waitFor(() => expect(status()).toHaveTextContent('1 selected'));
        await select('Beta');
        await waitFor(() => expect(status()).not.toBeInTheDocument());
    },
});
const FullSelection = meta.story({
    play: async (context) => {
        const { select, disabled } = list(context);
        await select('Alpha');
        await select('Beta');
        await select('Gamma');
        await waitFor(() => expect(disabled()).toEqual([true, true, true]));
    },
});
const KeyboardReorder = meta.story({
    play: async (context) => {
        const { canvas, userEvent } = context;
        const { order } = list(context);
        const handle = canvas.getByRole('button', { name: 'Drag Alpha' });
        await userEvent.tab();
        await userEvent.keyboard('{ArrowRight}{ArrowRight}');
        await expect(handle).toHaveFocus();
        await userEvent.keyboard('{Enter}');
        await waitFor(() => expect(handle).not.toHaveFocus());
        await userEvent.keyboard('{ArrowDown}{Enter}');
        await waitFor(() => expect(order()).toEqual(['beta', 'alpha', 'gamma']));
    },
});
const OwnerRemoval = meta.story({
    render: Owner,
    play: async (context) => {
        const { select, opened, targets, status } = list(context);
        await select('Gamma');
        await select('Alpha');
        await waitFor(() => expect(status()).toHaveTextContent('2 selected'));
        await context.userEvent.click(context.canvas.getByRole('button', { name: 'Remove Gamma' }));
        await waitFor(() => expect(status()).toHaveTextContent('1 selected'));
        await opened('Alpha');
        await waitFor(() => expect(targets()).toEqual(['Beta']));
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { BlockMove, Default, FullSelection, GatheredSelection, KeyboardReorder, MoveMenu, OwnerRemoval, SelectionActions, Single, UnselectedMove };
