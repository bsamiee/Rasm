import { expect, test } from '../fixtures.ts';

test.describe('visual gauge', () => {
    test('the deterministic panel matches its committed golden', async ({ page, target }) => {
        await target.open('/panel');
        await expect(page.locator('svg')).toHaveScreenshot('panel.png');
    });

    test('a perturbed panel refutes the same golden', async ({ page, target }) => {
        await target.open('/panel');
        await page
            .locator('svg rect')
            .first()
            .evaluate((node) => node.setAttribute('fill', '#ff0044'));
        await expect(page.locator('svg')).not.toHaveScreenshot('panel.png');
    });
});

test.describe('aria gauge', () => {
    test('the intake form matches its aria contract', async ({ page, target }) => {
        await target.open('/form');
        await expect(page.locator('main')).toMatchAriaSnapshot(`
            - heading "intake" [level=1]
            - text: key
            - textbox "key"
            - button "submit"
        `);
    });

    test('a wrong accessible name refutes the aria contract', async ({ page, target }) => {
        await target.open('/form');
        await expect(page.locator('main')).not.toMatchAriaSnapshot(`
            - heading "intake" [level=1]
            - button "commit"
        `);
    });
});
