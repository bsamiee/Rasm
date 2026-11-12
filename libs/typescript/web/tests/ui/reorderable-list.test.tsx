import { ReorderableList } from '@rasm/ui';
import { Array, Option } from 'effect';
import type { ComponentProps } from 'react';
import { expect, it, vi } from 'vitest';
import { userEvent } from 'vitest/browser';
import { render } from 'vitest-browser-react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheets = async () => {
    const onChange = vi.fn<ComponentProps<typeof ReorderableList<{ readonly id: string; readonly title: string }>>['onChange']>();
    const list = (present: Parameters<typeof onChange>[0]) => (
        <ReorderableList actions={(keys) => <output>{`${keys.size} selected`}</output>} aria-label="Sheets" items={present} moveLabel="Move" onChange={onChange} textValue={({ title }) => title}>
            {({ title }) => <span>{title}</span>}
        </ReorderableList>
    );
    const items = [
        { id: 'plan', title: 'Plan' },
        { id: 'section', title: 'Section' },
        { id: 'detail', title: 'Detail' },
    ];
    const screen = await render(list(items));
    const select = (title: string) => screen.getByRole('row', { name: title }).getByText(title).click();
    const drag = async (source: string, target: string, half: 'upper' | 'lower') => {
        const row = screen.getByRole('row', { name: target });
        const { height } = row.element().getBoundingClientRect();
        await userEvent.dragAndDrop(screen.getByRole('row', { name: source }), row, { targetPosition: { x: 1, y: { upper: 2, lower: height - 2 }[half] } });
        await screen.rerender(list(Option.getOrThrow(Array.last(onChange.mock.calls))[0]));
    };
    const disabled = () =>
        items.map(({ title }) =>
            screen
                .getByRole('button', { name: `Move ${title}` })
                .element()
                .hasAttribute('disabled'),
        );
    const order = () => onChange.mock.lastCall?.[0].map(({ id }) => id);
    return { screen, select, drag, disabled, order };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('counts select-all as every present item and disables moving', async () => {
    const { screen, disabled } = await sheets();
    await userEvent.tab();
    await userEvent.keyboard('{ControlOrMeta>}a{/ControlOrMeta}');
    await expect.element(screen.getByRole('status')).toHaveTextContent('3 selected');
    await expect.poll(disabled).toEqual([true, true, true]);
});
it('reorders by pointer drag onto lower and upper halves of later row', async () => {
    const { drag, order } = await sheets();
    await drag('Plan', 'Detail', 'lower');
    expect(order()).toEqual(['section', 'detail', 'plan']);
    await drag('Section', 'Plan', 'upper');
    expect(order()).toEqual(['detail', 'section', 'plan']);
});
it('drags every selected row as one block by pointer', async () => {
    const { select, drag, order } = await sheets();
    await select('Plan');
    await select('Section');
    await drag('Plan', 'Detail', 'lower');
    expect(order()).toEqual(['detail', 'plan', 'section']);
});
