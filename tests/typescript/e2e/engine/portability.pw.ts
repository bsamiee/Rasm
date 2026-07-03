import { Hermetic } from '@rasm/ts-testkit/e2e';
import { expect, test } from '../fixtures.ts';

test.describe('engine portability', () => {
    test('the hermetic origin is a secure context in every engine', async ({ hermetic, page }) => {
        await hermetic.open('/form');
        expect(await page.evaluate(() => window.isSecureContext)).toBe(true);
        await expect(page.getByRole('heading', { name: 'intake' })).toBeVisible();
    });

    test('a phantom route is a 404 verdict, never a hang', async ({ hermetic, page }) => {
        void hermetic;
        const response = await page.goto(`${Hermetic.origin}/phantom`);
        expect(response?.status()).toBe(404);
    });
});
