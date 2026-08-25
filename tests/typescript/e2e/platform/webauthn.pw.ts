import { expect, test } from '../fixtures.ts';

test.describe('webauthn ceremony', () => {
    test('an installed authenticator mints a discoverable platform credential', async ({ page, target, webauthn }) => {
        await target.open('/passkey');
        await page.getByTestId('mint').click();
        await expect(page.getByTestId('verdict')).toHaveText('minted', { timeout: 10_000 });
        const held = await webauthn.held(new URL(target.origin).hostname);
        expect(held).toHaveLength(1);
        expect(held[0]?.publicKey.length).toBeGreaterThan(0);
    });

    test('an empty authenticator refuses the assertion ceremony', async ({ page, target, webauthn }) => {
        expect(await webauthn.held(new URL(target.origin).hostname)).toHaveLength(0);
        await target.open('/passkey');
        await page.getByTestId('assert').click();
        await expect(page.getByTestId('verdict')).toHaveText(/refused:/, { timeout: 10_000 });
    });
});
