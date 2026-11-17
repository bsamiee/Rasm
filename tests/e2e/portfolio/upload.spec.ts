import { expect } from '@playwright/test';
import { Identity } from '@rasm/hosting/identity';
import hosting from '@rasm/portfolio/hosting.config';
import { it } from '@rasm/testing/playwright';
import { Array, Effect, Option } from 'effect';
import { SecureRandom } from 'effect/crypto';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.use({ extraHTTPHeaders: { [Identity.sites.headers.id]: hosting.name, [Identity.sites.headers.email]: Option.getOrThrow(Array.head(hosting.allowed)) } });

it('uploads an image from the editor straight to the bucket and serves it back to the library', async ({ page }) => {
    it.slow();
    await page.goto('/');
    const name = `study-${await SecureRandom.SecureRandom.use((random) => random.uuidV7()).pipe(Effect.provide(SecureRandom.layerWebCrypto), Effect.runPromise)}.png`;
    const buffer = await page.screenshot();
    await page.getByRole('button', { name: 'Edit portfolio' }).click();
    await page.getByRole('tab', { name: /^Files/u }).click();
    await page.locator('input[type=file]').first().setInputFiles({ name, mimeType: 'image/png', buffer });
    await page.getByRole('button', { name: 'Upload 1 file' }).click();
    const listed = page.getByRole('grid', { name: 'Source files' }).getByRole('row', { name });
    await expect(listed).toBeVisible({ timeout: 60_000 });
    await listed.scrollIntoViewIfNeeded();
    await expect(listed.locator('img')).toHaveJSProperty('complete', true, { timeout: 30_000 });
    await expect(listed.locator('img')).not.toHaveJSProperty('naturalWidth', 0);
});
