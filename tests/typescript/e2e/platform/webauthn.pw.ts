import { expect, test } from '../fixtures.ts';

test.describe('webauthn ceremony', () => {
    test('an installed authenticator mints a discoverable platform credential', async ({ page, target, webauthn }) => {
        await target.open('/passkey');
        await page.getByTestId('mint').click();
        await expect(page.getByTestId('verdict')).toHaveText('minted', { timeout: 10_000 });
        // The oracle is the authenticator's own registry, never the page's self-report: a credential the
        // page claims to have minted must be readable back with the keypair the ceremony generated.
        const held = await webauthn.held(new URL(target.origin).hostname);
        expect(held).toHaveLength(1);
        expect(held[0]?.publicKey.length).toBeGreaterThan(0);
    });

    // The witness: an assertion ceremony against the installed-but-EMPTY authenticator. The virtual
    // authenticator actively answers "no credentials" as a prompt refusal, where a context with no
    // authenticator at all only pends on the real platform path — so this is the one refute shape the
    // ceremony can observe, and a fixture degraded into a no-op pends here and fails loudly.
    test('an empty authenticator refuses the assertion ceremony', async ({ page, target, webauthn }) => {
        expect(await webauthn.held(new URL(target.origin).hostname)).toHaveLength(0);
        await target.open('/passkey');
        await page.getByTestId('assert').click();
        await expect(page.getByTestId('verdict')).toHaveText(/refused:/, { timeout: 10_000 });
    });
});
