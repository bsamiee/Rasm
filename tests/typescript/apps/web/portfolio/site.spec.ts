import { expect } from '@playwright/test';
import { it } from '@rasm/test-support/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('serves the site with its landmarks, the work index, and no axe violation', async ({ page, axe }) => {
    await page.goto('/');
    await expect(page).toHaveTitle('Portfolio · Architecture');
    await expect(page.getByRole('main').getByRole('heading', { level: 1 })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Projects and studies' })).toBeVisible();
    await expect(page.getByRole('banner')).toBeVisible();
    await expect(page.getByRole('contentinfo')).toBeVisible();
    expect((await axe.withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa', 'best-practice']).analyze()).violations.map(({ id }) => id)).toEqual([]);
});
