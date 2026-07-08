import { expect, test } from '../fixtures.ts';

// The worker seam probe: the viewer, chart, and browser planes all cross module workers with
// transferred buffers, so the platform contract — spawn, transfer detachment, reply — proves here.
test.describe('worker lane', () => {
    test('a blob worker computes over a transferred buffer and the source detaches', async ({ page, target }) => {
        await target.open('/pool');
        await expect(page.getByTestId('pool')).toHaveText('sum:10');
        await expect(page.getByTestId('detached')).toHaveText('true');
    });

    test('a throwing worker surfaces the fault verdict — the lane can fail', async ({ page, target }) => {
        await target.open('/pool');
        const verdict = await page.evaluate(
            () =>
                new Promise<string>((resolve) => {
                    const lane = new Worker(URL.createObjectURL(new Blob(['throw new Error("boom");'], { type: 'text/javascript' })));
                    lane.onerror = () => resolve('fault');
                }),
        );
        expect(verdict).toBe('fault');
    });
});
