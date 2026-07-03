import { expect, test } from '../fixtures.ts';

test.describe('gpu lane', () => {
    test('webgl2 acquires under the lane flags; the adapter identity rides as evidence', async ({ hermetic, page }) => {
        await hermetic.open('/panel');
        const probe = await page.evaluate(() => {
            const gl = document.createElement('canvas').getContext('webgl2');
            const renderer = gl === null ? 'none' : String(gl.getParameter(gl.RENDERER));
            return { renderer, webgl2: gl !== null, webgpu: 'gpu' in navigator };
        });
        test.info().annotations.push({ description: `${probe.renderer}; webgpu=${probe.webgpu}`, type: 'gpu-evidence' });
        expect(probe.webgl2).toBe(true);
    });

    test('a bogus context kind refutes the acquisition probe', async ({ hermetic, page }) => {
        await hermetic.open('/panel');
        expect(await page.evaluate(() => document.createElement('canvas').getContext('bogus-kind') === null)).toBe(true);
    });
});
