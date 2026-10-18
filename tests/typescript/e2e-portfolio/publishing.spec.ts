import { expect, test } from './fixtures.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('owner publishes an edited draft that survives reload and reaches signed-out visitors', async ({ owner, page }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const status = editor.getByRole('status');
    const introduction = `Civic buildings ${testId}`;
    const title = `Harbor Library ${testId}`;
    await test.step('edit the introduction and add an entry', async () => {
        await editor.getByRole('tab', { name: 'Identity and hero' }).click();
        await editor.getByRole('textbox', { name: 'Introduction' }).fill(introduction);
        await editor.getByRole('tab', { name: 'Projects and studies' }).click();
        await editor.getByRole('button', { name: 'New project' }).click();
        await expect(editor.getByRole('textbox', { name: 'Title' })).toBeFocused();
        await editor.getByRole('textbox', { name: 'Title' }).fill(title);
        await expect(status).toHaveText('Unsaved draft');
        await expect(owner).toPassAxe('[role="dialog"]');
    });
    await test.step('save the draft without publishing it', async () => {
        await editor.getByRole('button', { name: 'Save draft' }).click();
        await expect(status).toHaveText('Draft saved. Published work is unchanged.');
        await page.reload();
        await expect(page.getByRole('heading', { name: title })).toHaveCount(0);
        await expect(page.getByText(introduction)).toHaveCount(0);
    });
    await test.step('publish the saved draft', async () => {
        await editor.getByRole('button', { name: 'Publish' }).click();
        await expect(status).toHaveText('Published. Your site shows the saved draft.');
    });
    await test.step('reload as owner', async () => {
        await owner.reload();
        await expect(editor.getByRole('grid', { name: 'Projects and studies' }).getByRole('row', { name: title })).toBeVisible();
        await editor.getByRole('button', { name: 'Close portfolio editor' }).click();
        await expect(owner.getByRole('main').getByRole('heading', { name: title, level: 3 })).toBeVisible();
        await expect(owner.locator('meta[name="description"]')).toHaveAttribute('content', introduction);
        await expect(owner).toPassAxe('body');
    });
    await test.step('read the published portfolio signed out', async () => {
        await page.reload();
        await expect(page.getByRole('banner').getByRole('link', { name: 'Owner sign-in' })).toBeVisible();
        await expect(page.getByRole('button', { name: 'Edit portfolio' })).toHaveCount(0);
        await expect(page.getByRole('main').getByText(introduction)).toBeVisible();
        await expect(page.getByRole('navigation', { name: 'Work index' }).getByRole('link', { name: title })).toBeVisible();
        await expect(page.getByRole('main').getByRole('heading', { name: title, level: 3 })).toBeVisible();
    });
});
