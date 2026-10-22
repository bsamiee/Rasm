import { Default, Single } from '@rasm/ui/.storybook/reorderable-list.stories.tsx';
import { expect, it, vi } from 'vitest';
import { page, userEvent } from 'vitest/browser';
import { render } from 'vitest-browser-react';

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

it('moves one item after a later target and before an earlier one through the move menu', async () => {
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
it('moves a selected block as one and keeps its relative order', async () => {
    const { select, move, order } = await _sheets();
    await select('Plan');
    await select('Section');
    await move(0, 'Detail');
    expect(order()).toEqual(['detail', 'plan', 'section']);
});
it('renders actions only while a selection exists and counts the selected keys', async () => {
    const { screen, select } = await _sheets();
    await expect.element(screen.getByRole('status')).not.toBeInTheDocument();
    await select('Section');
    await expect.element(screen.getByRole('status')).toHaveTextContent('1 selected');
    await select('Section');
    await expect.element(screen.getByRole('status')).not.toBeInTheDocument();
});
it('disables moving when the selection covers every item', async () => {
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
it('disables moving in a list of one item', async () => {
    const screen = await render(<Single.Component />);
    await expect.element(screen.getByRole('button', { name: 'Move' })).toBeDisabled();
});
it('reorders through a pointer drag of a row onto the lower half of a later row', async () => {
    const { screen, order } = await _sheets();
    const target = screen.getByRole('row', { name: 'Detail' });
    await userEvent.dragAndDrop(screen.getByRole('row', { name: 'Plan' }), target, { targetPosition: { x: 1, y: target.element().getBoundingClientRect().height - 2 } });
    expect(order()).toEqual(['section', 'detail', 'plan']);
});
it('reorders through keyboard drag and drop from the drag button, starting at the dragged position', async () => {
    const { screen, order } = await _sheets();
    await userEvent.tab();
    await userEvent.keyboard('{ArrowRight}{ArrowRight}');
    await expect.element(screen.getByRole('button', { name: 'Drag Plan' })).toHaveFocus();
    await userEvent.keyboard('{Enter}{ArrowDown}{Enter}');
    expect(order()).toEqual(['section', 'plan', 'detail']);
});
it('exposes the grid, its rows, and their controls by role and name', async () => {
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
