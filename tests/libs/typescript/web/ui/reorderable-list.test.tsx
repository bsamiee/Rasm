import { ReorderableList } from '@rasm/ui';
import { useState } from 'react';
import { expect, it } from 'vitest';
import { render } from 'vitest-browser-react';
import { userEvent } from 'vitest/browser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHEETS = [
    { id: 'plan', title: 'Plan' },
    { id: 'section', title: 'Section' },
    { id: 'detail', title: 'Detail' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const Sheets = () => {
    const [items, setItems] = useState<readonly (typeof SHEETS)[number][]>(SHEETS);
    return (
        <ReorderableList actions={(keys) => <output>{[...keys].join(' ')}</output>} aria-label="Sheets" items={items} moveLabel="Move" onChange={setItems} textValue={({ title }) => title}>
            {({ title }) => <span>{title}</span>}
        </ReorderableList>
    );
};

it('passes every present item to the selection actions on select-all', async () => {
    const screen = await render(<Sheets />);
    await userEvent.tab();
    await userEvent.keyboard('{ControlOrMeta>}a{/ControlOrMeta}');
    await expect.element(screen.getByRole('status')).toHaveTextContent('plan section detail');
});

it.for([
    { selected: [], source: 'Plan', target: 'Detail', half: 'lower', order: ['section', 'detail', 'plan'] },
    { selected: [], source: 'Detail', target: 'Plan', half: 'upper', order: ['detail', 'plan', 'section'] },
    { selected: ['Plan', 'Section'], source: 'Plan', target: 'Detail', half: 'lower', order: ['detail', 'plan', 'section'] },
] as const)('drops $source with selection $selected dragged by pointer onto the $half half of $target as $order', async ({ selected, source, target, half, order }) => {
    const screen = await render(<Sheets />);
    await selected.reduce<Promise<void>>((clicked, title) => clicked.then(() => screen.getByRole('row', { name: title }).getByText(title).click()), Promise.resolve());
    const onto = screen.getByRole('row', { name: target });
    const { height } = onto.element().getBoundingClientRect();
    await userEvent.dragAndDrop(screen.getByRole('row', { name: source }), onto, { targetPosition: { x: 1, y: { upper: 2, lower: height - 2 }[half] } });
    await expect
        .poll(() =>
            screen
                .getByRole('row')
                .elements()
                .map((row) => row.dataset['key']),
        )
        .toStrictEqual(order);
});
