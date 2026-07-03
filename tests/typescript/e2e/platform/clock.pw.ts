import { expect, test } from '../fixtures.ts';

// install() auto-advances from the epoch, so pre-jump readings carry the page-load offset; only the
// fired-at-target instant after fastForward is exact — the assertions ride that surface.
const _T0 = new Date('2026-01-01T00:00:10.000Z');
const _ADVANCED = '2026-01-01T00:05:10.000Z';

test.describe('clock control', () => {
    test('fastForward lands the page on the exact target instant', async ({ hermetic, page, pausedClock }) => {
        await hermetic.open('/clock');
        await pausedClock.pauseAt(_T0);
        await pausedClock.fastForward('05:00');
        await expect(page.getByTestId('now')).toHaveText(_ADVANCED);
    });

    test('a paused clock refutes the advanced reading — wall time never leaks in', async ({ hermetic, page, pausedClock }) => {
        await hermetic.open('/clock');
        await pausedClock.pauseAt(_T0);
        await expect(page.getByTestId('now')).toHaveText(/^2026-01-01T00:00:/);
        await expect(page.getByTestId('now')).not.toHaveText(_ADVANCED);
    });
});
