import { fileURLToPath } from 'node:url';
import { expect, test } from '../fixtures.ts';

const _ARCHIVE = fileURLToPath(new URL('./replay.har', import.meta.url));

test.describe('har replay lane', () => {
    test('an archived exchange replays without a network', async ({ context, page }) => {
        await context.routeFromHAR(_ARCHIVE, { notFound: 'abort' });
        await page.goto('https://rasm.test/replayed');
        await expect(page.getByTestId('origin')).toHaveText('har');
    });

    test('a request outside the archive aborts — the fall-through refutation', async ({ context, page }) => {
        await context.routeFromHAR(_ARCHIVE, { notFound: 'abort' });
        const verdict = await page.goto('https://rasm.test/phantom').then(
            () => 'served',
            () => 'aborted',
        );
        expect(verdict).toBe('aborted');
    });
});
