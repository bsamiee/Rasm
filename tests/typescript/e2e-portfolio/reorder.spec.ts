import { expect, test } from '#/fixtures';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('owner reorders entries from the keyboard and the saved order survives reload', async ({ owner }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const entries = editor.getByRole('grid', { name: 'Projects and studies' });
    const plan = `Plan ${testId}`;
    const section = `Section ${testId}`;
    const detail = `Detail ${testId}`;
    const create = async (title: string) => {
        await editor.getByRole('button', { name: 'New project' }).click();
        await editor.getByRole('textbox', { name: 'Title' }).fill(title);
        await editor.getByRole('button', { name: 'Change project' }).click();
    };
    await test.step('add the entries to reorder', async () => {
        await create(plan);
        await create(section);
        await create(detail);
        await expect(entries).toMatchAriaSnapshot(`
          - grid:
            - row "${plan}"
            - row "${section}"
            - row "${detail}"
        `);
    });
    await test.step('drag an entry one position down from its drag button', async () => {
        await entries.getByRole('button', { name: `Drag ${plan}` }).focus();
        await owner.keyboard.press('Enter');
        await owner.keyboard.press('ArrowDown');
        await owner.keyboard.press('Enter');
        await expect(entries).toMatchAriaSnapshot(`
          - grid:
            - row "${section}"
            - row "${plan}"
            - row "${detail}"
        `);
    });
    await test.step('move an entry to the top through its move menu', async () => {
        await entries.getByRole('row', { name: detail }).getByRole('button', { name: 'Move', exact: true }).click();
        await owner.getByRole('menu', { name: 'Move' }).getByRole('menuitem', { name: section }).click();
        await expect(owner.getByRole('menu', { name: 'Move' })).toBeHidden();
        await expect(entries.getByRole('row').first()).toHaveAccessibleName(detail);
        await expect(entries).toMatchAriaSnapshot(`
          - grid:
            - row "${detail}"
            - row "${section}"
            - row "${plan}"
        `);
    });
    await test.step('save the order and reload', async () => {
        await editor.getByRole('button', { name: 'Save draft' }).click();
        await expect(editor.getByRole('status')).toHaveText('Draft saved. Published work is unchanged.');
        await owner.reload();
        await expect(entries).toMatchAriaSnapshot(`
          - grid:
            - row "${detail}"
            - row "${section}"
            - row "${plan}"
        `);
    });
});
