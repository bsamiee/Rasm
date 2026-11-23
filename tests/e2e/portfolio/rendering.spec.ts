import { expect, type Page } from '@playwright/test';
import { shown } from '#/fixtures';
import { cases } from '#/rendering';

// --- [COMPOSITION] ---------------------------------------------------------------------

const viewers = {
    visitor: shown.extend<{ readonly viewed: Page }>({ viewed: ({ page }, use) => use(page) }),
    owner: shown.extend<{ readonly viewed: Page }>({ viewed: ({ owner }, use) => use(owner) }),
};

cases.forEach(({ viewer, path, shows, width, read, expected }) =>
    viewers[viewer](`${viewer} at ${width}: ${shows}`, async ({ viewed }) => {
        await viewed.setViewportSize({ width, height: 900 });
        await viewed.goto(path);
        await expect.poll(() => viewed.evaluate(read), { timeout: 10_000 }).toStrictEqual(expected);
    }),
);
