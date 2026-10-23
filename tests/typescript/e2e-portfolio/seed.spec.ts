import { expect } from '@playwright/test';
import { test } from '#/fixtures';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('seed', async ({ page }) => {
    await expect(page).toHaveTitle('Architecture · Portfolio');
});
