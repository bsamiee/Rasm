import { ReorderableList } from '@rasm/ui';
import { Default, Single } from '@rasm/ui/stories/reorderable-list';
import type { ComponentProps } from 'react';
import { expect, it, vi } from 'vitest';
import { page, userEvent } from 'vitest/browser';
import { render } from 'vitest-browser-react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _targets = () =>
    page
        .getByRole('menuitem')
        .elements()
        .filter((item) => item.getAttribute('aria-disabled') !== 'true')
        .map(({ textContent }) => textContent);
const _sheets = async () => {
    const onChange = vi.fn<NonNullable<ComponentProps<typeof Default.Component>['onChange']>>();
    const screen = await render(<Default.Component onChange={onChange} />);
    const select = (title: string) => screen.getByRole('row', { name: title }).getByText(title).click();
    const opened = (title: string) => screen.getByRole('button', { name: `Move ${title}` }).click();
    const move = async (title: string, target: string) => {
        await opened(title);
        await page.getByRole('menuitem', { name: target }).click();
    };
    const drag = (source: string, target: string, half: 'upper' | 'lower') => {
        const row = screen.getByRole('row', { name: target });
        const { height } = row.element().getBoundingClientRect();
        return userEvent.dragAndDrop(screen.getByRole('row', { name: source }), row, { targetPosition: { x: 1, y: half === 'upper' ? 2 : height - 2 } });
    };
    const rows = () =>
        screen
            .getByRole('row')
            .elements()
            .map((row) => row.getAttribute('data-key'));
    const disabled = () =>
        Default.composed.args.items.map(({ title }) =>
            screen
                .getByRole('button', { name: `Move ${title}` })
                .element()
                .hasAttribute('disabled'),
        );
    const order = () => onChange.mock.lastCall?.[0].map(({ id }) => id);
    return { screen, select, opened, move, drag, rows, disabled, order };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('moves one item past later and earlier targets through Move menu', async () => {
    const { move, rows, order } = await _sheets();
    await move('Plan', 'Detail');
    expect(order()).toEqual(['section', 'detail', 'plan']);
    await move('Plan', 'Section');
    expect(order()).toEqual(['plan', 'section', 'detail']);
    await expect.poll(rows).toEqual(['plan', 'section', 'detail']);
});
it('moves selected items as one block in relative order', async () => {
    const { select, move, order } = await _sheets();
    await select('Plan');
    await select('Section');
    await move('Plan', 'Detail');
    expect(order()).toEqual(['detail', 'plan', 'section']);
});
it('gathers non-contiguous selection into one block placed by its first row and enables only unselected targets', async () => {
    const { select, opened, order } = await _sheets();
    await select('Plan');
    await select('Detail');
    await opened('Detail');
    await expect.poll(_targets).toEqual(['Section']);
    await page.getByRole('menuitem', { name: 'Section' }).click();
    expect(order()).toEqual(['section', 'plan', 'detail']);
});
it('moves unselected row alone while selection holds other rows', async () => {
    const { screen, select, move, rows, order } = await _sheets();
    await select('Plan');
    await move('Detail', 'Plan');
    expect(order()).toEqual(['detail', 'plan', 'section']);
    await expect.poll(rows).toEqual(['detail', 'plan', 'section']);
    await expect.element(screen.getByRole('status')).toHaveTextContent('1 selected');
});
it('renders actions only during selection with selected count', async () => {
    const { screen, select } = await _sheets();
    await expect.element(screen.getByRole('status')).not.toBeInTheDocument();
    await select('Section');
    await expect.element(screen.getByRole('status')).toHaveTextContent('1 selected');
    await select('Section');
    await expect.element(screen.getByRole('status')).not.toBeInTheDocument();
});
it('disables moving when selection covers every item', async () => {
    const { select, disabled } = await _sheets();
    await select('Plan');
    await select('Section');
    await select('Detail');
    await expect.poll(disabled).toEqual([true, true, true]);
});
it('counts select-all as every present item and disables moving', async () => {
    const { screen, disabled } = await _sheets();
    await userEvent.tab();
    await userEvent.keyboard('{ControlOrMeta>}a{/ControlOrMeta}');
    await expect.element(screen.getByRole('status')).toHaveTextContent('3 selected');
    await expect.poll(disabled).toEqual([true, true, true]);
});
it('disables moving in one-item list', async () => {
    const screen = await render(<Single.Component />);
    await expect.element(screen.getByRole('button', { name: 'Move Plan' })).toBeDisabled();
});
it('reorders by pointer drag onto lower and upper halves of later row', async () => {
    const { drag, order } = await _sheets();
    await drag('Plan', 'Detail', 'lower');
    expect(order()).toEqual(['section', 'detail', 'plan']);
    await drag('Section', 'Plan', 'upper');
    expect(order()).toEqual(['detail', 'section', 'plan']);
});
it('drags every selected row as one block by pointer', async () => {
    const { select, drag, order } = await _sheets();
    await select('Plan');
    await select('Section');
    await drag('Plan', 'Detail', 'lower');
    expect(order()).toEqual(['detail', 'plan', 'section']);
});
it('reorders by keyboard drag and drop from drag button starting at dragged row', async () => {
    const { screen, order } = await _sheets();
    await userEvent.tab();
    await userEvent.keyboard('{ArrowRight}{ArrowRight}');
    await expect.element(screen.getByRole('button', { name: 'Drag Plan' })).toHaveFocus();
    await userEvent.keyboard('{Enter}');
    await expect.element(screen.getByRole('button', { name: 'Drag Plan' })).not.toHaveFocus();
    await userEvent.keyboard('{ArrowDown}{Enter}');
    await expect.poll(order).toEqual(['section', 'plan', 'detail']);
});
it('drops selection of items the owner removes and lists only present targets', async () => {
    const { items } = Default.composed.args;
    const list = (present: typeof items) => <ReorderableList {...Default.composed.args} items={present} onChange={vi.fn()} />;
    const screen = await render(list(items));
    await screen.getByRole('row', { name: 'Detail' }).getByText('Detail').click();
    await screen.getByRole('row', { name: 'Plan' }).getByText('Plan').click();
    await expect.element(screen.getByRole('status')).toHaveTextContent('2 selected');
    await screen.rerender(list(items.filter(({ id }) => id !== 'detail')));
    await expect.element(screen.getByRole('status')).toHaveTextContent('1 selected');
    await screen.getByRole('button', { name: 'Move Plan' }).click();
    await expect.poll(_targets).toEqual(['Section']);
});
it('exposes grid, rows, and row controls by role and name', async () => {
    const { screen } = await _sheets();
    await expect.element(screen.getByRole('grid', { name: 'Sheets' })).toMatchAriaInlineSnapshot(`
      - grid "Sheets":
        - row "Plan":
          - gridcell "Select Plan Drag Plan Move Plan Plan":
            - checkbox "Select Plan"
            - button "Drag Plan"
            - button "Move Plan"
            - text: Plan
        - row "Section":
          - gridcell "Select Section Drag Section Move Section Section":
            - checkbox "Select Section"
            - button "Drag Section"
            - button "Move Section"
            - text: Section
        - row "Detail":
          - gridcell "Select Detail Drag Detail Move Detail Detail":
            - checkbox "Select Detail"
            - button "Drag Detail"
            - button "Move Detail"
            - text: Detail
    `);
});
