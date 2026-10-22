import { sessionPath } from '@rasm/hosting/authentication';
import { expect, test } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _selectedEntries = /^Remove \d+ selected (?:entry|entries)$/v;

// --- [COMPOSITION] ---------------------------------------------------------------------

test('visitor reads landmarks and the empty state the owner published', async ({ owner, page }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const entries = editor.getByRole('grid', { name: 'Projects and studies' });
    await test.step('publish a portfolio without entries or introduction', async () => {
        await editor.getByRole('button', { name: 'New project' }).click();
        await editor.getByRole('textbox', { name: 'Title' }).fill(`Draft ${testId}`);
        await editor.getByRole('button', { name: 'Change project' }).click();
        await entries.getByRole('row', { name: `Draft ${testId}` }).focus();
        await owner.keyboard.press('ControlOrMeta+a');
        await editor.getByRole('button', { name: _selectedEntries }).click();
        await owner.getByRole('alertdialog').getByRole('button', { name: _selectedEntries }).click();
        await expect(entries.getByRole('row')).toHaveCount(0);
        await editor.getByRole('tab', { name: 'Identity and hero' }).click();
        await editor.getByRole('textbox', { name: 'Introduction' }).fill('');
        await editor.getByRole('button', { name: 'Publish' }).click();
        await expect(editor.getByRole('status')).toHaveText('Published. Your site shows the saved draft.');
    });
    await test.step('read the empty portfolio signed out', async () => {
        await page.reload();
        await expect(page).toHaveTitle('Architecture — Portfolio');
        await expect(page.getByRole('banner').getByRole('link', { name: 'Portfolio' })).toBeVisible();
        await expect(page.getByRole('banner').getByRole('link', { name: 'Owner sign-in' })).toHaveAttribute('href', sessionPath('signin', '/?edit=1'));
        await expect(page.getByRole('navigation', { name: 'Work index' })).toMatchAriaSnapshot(`
          - navigation "Work index":
            - link "Index":
              - /url: "#work"
            - link "Selected work":
              - /url: "#work"
        `);
        await expect(page.getByRole('main').getByRole('heading', { level: 1 })).toHaveAccessibleName('Selected work.');
        await expect(page.getByRole('main').getByRole('heading', { name: 'No work published yet', level: 3 })).toBeVisible();
        await expect(page.getByRole('contentinfo').getByRole('link', { name: 'Back to top' })).toBeVisible();
        await expect(page).toPassAxe('body');
    });
});
test('skip link moves keyboard focus to the work section', async ({ page }) => {
    await page.keyboard.press('Tab');
    await expect(page.getByRole('link', { name: 'Skip to work' })).toBeFocused();
    await page.keyboard.press('Enter');
    await expect(page.locator('#work')).toBeFocused();
});
