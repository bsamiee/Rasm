import { expect, it, vi } from 'vitest';
import { page, userEvent } from 'vitest/browser';
import { render } from 'vitest-browser-react';
import { Default, Single } from '#/libs/typescript/web/ui/.storybook/reorderable-list.stories';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _sheets = async () => {
    const onChange = vi.fn();
    const screen = await render(<Default.Component onChange={onChange} />);
    const select = (title: string) => screen.getByRole('row', { name: title }).getByText(title).click();
    const move = async (row: number, target: string) => {
        await screen.getByRole('button', { name: 'Move' }).nth(row).click();
        await page.getByRole('menuitem', { name: target }).click();
    };
    const order = () => onChange.mock.lastCall?.[0].map(({ id }: { readonly id: string }) => id);
    return { screen, select, move, order };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('moves one item past later and earlier targets through Move menu', async () => {
    const { screen, move, order } = await _sheets();
    await move(0, 'Detail');
    expect(order()).toEqual(['section', 'detail', 'plan']);
    await move(2, 'Section');
    expect(order()).toEqual(['plan', 'section', 'detail']);
    await expect
        .poll(() =>
            screen
                .getByRole('row')
                .elements()
                .map((row) => row.getAttribute('data-key')),
        )
        .toEqual(['plan', 'section', 'detail']);
});
it('moves selected items as one block in relative order', async () => {
    const { select, move, order } = await _sheets();
    await select('Plan');
    await select('Section');
    await move(0, 'Detail');
    expect(order()).toEqual(['detail', 'plan', 'section']);
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
    const { screen, select } = await _sheets();
    await select('Plan');
    await select('Section');
    await select('Detail');
    await expect
        .poll(() =>
            screen
                .getByRole('button', { name: 'Move' })
                .elements()
                .map((button) => button.hasAttribute('disabled')),
        )
        .toEqual([true, true, true]);
});
it('disables moving in one-item list', async () => {
    const screen = await render(<Single.Component />);
    await expect.element(screen.getByRole('button', { name: 'Move' })).toBeDisabled();
});
it('reorders by pointer drag onto lower half of later row', async () => {
    const { screen, order } = await _sheets();
    const target = screen.getByRole('row', { name: 'Detail' });
    await userEvent.dragAndDrop(screen.getByRole('row', { name: 'Plan' }), target, { targetPosition: { x: 1, y: target.element().getBoundingClientRect().height - 2 } });
    expect(order()).toEqual(['section', 'detail', 'plan']);
});
it('reorders by keyboard drag and drop from drag button starting at dragged row', async () => {
    const { screen, order } = await _sheets();
    await userEvent.tab();
    await userEvent.keyboard('{ArrowRight}{ArrowRight}');
    await expect.element(screen.getByRole('button', { name: 'Drag Plan' })).toHaveFocus();
    await userEvent.keyboard('{Enter}{ArrowDown}{Enter}');
    expect(order()).toEqual(['section', 'plan', 'detail']);
});
it('exposes grid, rows, and row controls by role and name', async () => {
    const { screen } = await _sheets();
    await expect.element(screen.getByRole('grid', { name: 'Sheets' })).toMatchAriaInlineSnapshot(`
      - grid "Sheets":
        - row "Plan":
          - gridcell "Select Plan Drag Plan Move Plan":
            - checkbox "Select Plan"
            - button "Drag Plan"
            - button "Move"
            - text: Plan
        - row "Section":
          - gridcell "Select Section Drag Section Move Section":
            - checkbox "Select Section"
            - button "Drag Section"
            - button "Move"
            - text: Section
        - row "Detail":
          - gridcell "Select Detail Drag Detail Move Detail":
            - checkbox "Select Detail"
            - button "Drag Detail"
            - button "Move"
            - text: Detail
    `);
});
