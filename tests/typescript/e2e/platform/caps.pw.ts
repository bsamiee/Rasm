import { Hermetic } from '@rasm/ts-testkit/e2e';
import { expect, test } from '../fixtures.ts';

// Capability rows ride the built-in context surface directly — a fixture wrapping grantPermissions
// would be a rename adapter, not a row.
const _SITE = { latitude: 35.6892, longitude: 51.389 };

test.describe('capability lanes', () => {
    test('a granted geolocation lane serves the declared coordinates', async ({ context, hermetic, page }) => {
        await context.grantPermissions(['geolocation'], { origin: Hermetic.origin });
        await context.setGeolocation(_SITE);
        await hermetic.open('/form');
        const probe = await page.evaluate(
            () =>
                new Promise<{ readonly latitude: number; readonly longitude: number }>((resolve, reject) => {
                    navigator.geolocation.getCurrentPosition(
                        ({ coords }) => resolve({ latitude: coords.latitude, longitude: coords.longitude }),
                        (fault) => reject(new Error(fault.message)),
                        { timeout: 2000 },
                    );
                }),
        );
        expect(probe).toEqual(_SITE);
    });

    test('clearing permissions refutes the granted state', async ({ context, hermetic, page }) => {
        await context.grantPermissions(['geolocation'], { origin: Hermetic.origin });
        await hermetic.open('/form');
        const query = () => page.evaluate(async () => (await navigator.permissions.query({ name: 'geolocation' })).state);
        expect(await query()).toBe('granted');
        await context.clearPermissions();
        expect(await query()).not.toBe('granted');
    });
});
