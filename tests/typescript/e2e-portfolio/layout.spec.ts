import { expect, test } from '#/fixtures';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('index switches between the side navigation and the sticky select with viewport width', async ({ owner, page }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const index = page.getByRole('navigation', { name: 'Work index' });
    const select = page.getByRole('combobox', { name: 'Index' });
    const study = `Tidal Studies ${testId}`;
    await test.step('publish a project and a study', async () => {
        await editor.getByRole('button', { name: 'New project' }).click();
        await editor.getByRole('textbox', { name: 'Title' }).fill(`Harbor Library ${testId}`);
        await editor.getByRole('button', { name: 'Change project' }).click();
        await editor.getByRole('button', { name: 'New study' }).click();
        await editor.getByRole('textbox', { name: 'Title' }).fill(study);
        await editor.getByRole('button', { name: 'Publish' }).click();
        await expect(editor.getByRole('status')).toHaveText('Published. Your site shows the saved draft.');
    });
    await test.step('show the side navigation at desktop width', async () => {
        await page.reload();
        await expect(index.getByRole('link', { name: study })).toBeVisible();
        await expect(select).toBeHidden();
    });
    await test.step('show the sticky select at phone width', async () => {
        await page.setViewportSize({ width: 390, height: 844 });
        await expect(index.getByRole('link', { name: study })).toBeHidden();
        await expect(select).toBeVisible();
        await select.selectOption(await select.getByRole('option', { name: study }).getAttribute('value'));
        await expect(page.getByRole('heading', { name: study, level: 3 })).toBeFocused();
        await expect(select.getByRole('option', { selected: true })).toContainText(study);
        await expect(page).toPassAxe('body');
    });
});
