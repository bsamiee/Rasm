import { expect } from '@playwright/test';
import { it } from '#/fixtures';
import { cases, content } from '#/rendering';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases.forEach(({ shows, width, read, expected }) =>
    it(shows, async ({ page, served }) => {
        await served.publish(content);
        await page.setViewportSize({ width, height: 900 });
        await page.goto('/');
        await expect.poll(() => page.evaluate(read)).toStrictEqual(expected);
    }),
);
