import { expect } from '@playwright/test';
import { it } from '#/fixtures';
import { cases, content } from '#/rendering';

// --- [COMPOSITION] ---------------------------------------------------------------------

cases.forEach(({ viewer, path, shows, width, read, expected }) =>
    it(shows, async ({ page, owner, served }) => {
        const viewed = { visitor: page, owner }[viewer];
        await served.publish(content);
        await viewed.setViewportSize({ width, height: 900 });
        await viewed.goto(path);
        await expect.poll(() => viewed.evaluate(read), { timeout: 10_000 }).toStrictEqual(expected);
    }),
);
