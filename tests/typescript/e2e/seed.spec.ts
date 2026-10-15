import { expect } from '@playwright/test';
import { test } from './fixtures.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('seed', async ({ page }) => {
    await expect(page).toHaveTitle('Architecture — Portfolio');
});
