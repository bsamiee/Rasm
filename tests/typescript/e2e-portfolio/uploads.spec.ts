import { expect, test } from './fixtures.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('owner recovers from a rejected file and a failed upload', async ({ owner }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const files = editor.getByRole('tabpanel', { name: 'Files' });
    const name = `plan-${testId}.png`;
    const source = files.getByRole('grid', { name: 'Source files' }).getByRole('row', { name });
    const retry = files.getByRole('button', { name: 'Retry upload' }).filter({ hasText: 'Retry' });
    const add = async (file: { readonly name: string; readonly mimeType: string; readonly buffer: Buffer }) => {
        const chooser = owner.waitForEvent('filechooser');
        await files.getByRole('button', { name: 'browse files' }).click();
        await (await chooser).setFiles(file);
    };
    await editor.getByRole('tab', { name: 'Files' }).click();
    await test.step('reject a file outside the accepted media types', async () => {
        await add({ name: 'notes.txt', mimeType: 'text/plain', buffer: Buffer.from('Site notes') });
        await expect(files.getByRole('alert')).toHaveText('You can only upload: image/jpeg, image/png, image/webp, image/avif, video/mp4, video/webm, application/pdf');
        await expect(files.getByRole('button', { name: 'Upload 1 file' })).toHaveCount(0);
    });
    await test.step('fail the upload of an accepted image', async () => {
        await owner.route('**/api/media', (route) => route.fulfill({ status: 503 }));
        await add({ name, mimeType: 'image/png', buffer: await owner.screenshot({ clip: { x: 0, y: 0, width: 64, height: 48 } }) });
        await files.getByRole('button', { name: 'Upload 1 file' }).click();
        await expect(retry).toBeVisible();
        await expect(editor.getByText('1 file needs attention. Finish, retry, or remove queued files before publishing or reloading.')).toBeVisible();
        await expect(editor.getByRole('button', { name: 'Publish' })).toBeDisabled();
        await expect(source).toHaveCount(0);
    });
    await test.step('retry the upload after the service recovers', async () => {
        await owner.unroute('**/api/media');
        await retry.click();
        await expect(source).toBeVisible();
        await expect(editor.getByText('1 file needs attention.')).toHaveCount(0);
        await expect(editor.getByRole('button', { name: 'Publish' })).toBeEnabled();
    });
});
