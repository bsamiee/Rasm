import { expect, test } from '../fixtures.ts';

test.describe('webauthn ceremony', () => {
    test('a virtual authenticator mints a platform credential', async ({ hermetic, page, webauthn }) => {
        expect(webauthn.id.length).toBeGreaterThan(0);
        await hermetic.open('/passkey');
        await page.getByTestId('mint').click();
        await expect(page.getByTestId('verdict')).toHaveText('minted', { timeout: 10_000 });
    });

    test('removing the authenticator refutes the ceremony', async ({ hermetic, page, webauthn }) => {
        await webauthn.remove();
        await hermetic.open('/passkey');
        await page.getByTestId('mint').click();
        await expect(page.getByTestId('verdict')).toHaveText(/refused:/, { timeout: 10_000 });
    });
});
