import { Hermetic } from '@rasm/ts-testkit/e2e';
import { expect, test } from '../fixtures.ts';

test.describe('websocket lane', () => {
    test('routeWebSocket serves a hermetic echo lane', async ({ hermetic, page }) => {
        await page.routeWebSocket(`wss://rasm.test/ws`, (lane) => {
            lane.onMessage((message) => lane.send(`echo:${message}`));
        });
        await hermetic.open('/echo');
        await expect(page.getByTestId('wire')).toHaveText('echo:ping');
    });

    test('a closed lane surfaces as a verdict, never a silent pass', async ({ hermetic, page }) => {
        await page.routeWebSocket(`wss://rasm.test/ws`, (lane) => {
            void lane.close();
        });
        await hermetic.open('/echo');
        await expect(page.getByTestId('wire')).toHaveText('closed');
    });
});

test.describe('cohort isolation', () => {
    test('two cohort clients hold isolated storage', async ({ duo }) => {
        const [first, second] = await duo('/store');
        await expect(first.getByTestId('held')).not.toBeEmpty();
        await expect(second.getByTestId('held')).not.toBeEmpty();
        expect(await first.getByTestId('held').textContent()).not.toBe(await second.getByTestId('held').textContent());
    });

    test('one context shares storage across its pages — the isolation falsifier', async ({ context, hermetic, page }) => {
        await hermetic.open('/store');
        const held = await page.getByTestId('held').textContent();
        const twin = await context.newPage();
        await twin.goto(`${Hermetic.origin}/store`);
        await expect(twin.getByTestId('held')).toHaveText(held ?? '');
    });
});
