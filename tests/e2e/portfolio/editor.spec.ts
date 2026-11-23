import { expect, type Page } from '@playwright/test';
import { it } from '#/fixtures';
import { content } from '#/rendering';
import { GUEST, OWNER, SAME_ORIGIN } from '#/served';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EDITING = '/?edit=true';
const UNREVISED = { 'if-match': '"0"' } as const;
const [PLACED] = content.files;
const SPARE = { asset: { ...PLACED.asset, id: 'c3d4e5f6-a7b8-4c9d-8e0f-2a3b4c5d6e7f', name: 'spare-section.png' }, bytes: PLACED.bytes } as const;
const OWNED = { ...OWNER, ...SAME_ORIGIN } as const;
const TWO = {
    files: [],
    portfolio: {
        entries: [
            { id: '7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f', kind: 'project', title: 'Harbour Library' },
            { id: '0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d', kind: 'study', title: 'Timber Joints' },
        ],
    },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const editing = (owner: Page) => owner.getByRole('dialog', { name: /Portfolio editor/u });

const chapters = (page: Page) => page.getByRole('heading', { level: 3 });

const offered = async (shown: Page) => [await shown.getByRole('link', { name: 'Owner sign-in' }).count(), await shown.getByRole('link', { name: 'Sign out' }).count(), await shown.getByRole('button', { name: 'Edit portfolio' }).count()];

// --- [COMPOSITION] ---------------------------------------------------------------------

it('offers a visitor the owner sign-in, a signed-in guest sign-out alone, and the owner the editor', async ({ page, owner, browser, baseURL }) => {
    const guest = await browser.newContext({ extraHTTPHeaders: GUEST, ...(baseURL !== undefined && { baseURL }) }).then((context) => context.newPage());
    await page.goto('/');
    await guest.goto('/');
    await owner.goto('/');
    expect({ visitor: await offered(page), guest: await offered(guest), owner: await offered(owner) }).toStrictEqual({ visitor: [1, 0, 0], guest: [0, 1, 0], owner: [0, 0, 1] });
    await guest.context().close();
});

it('opens the editor for the owner who follows the sign-in link a visitor is offered', async ({ page, owner }) => {
    await page.goto('/');
    await owner.goto(await page.getByRole('link', { name: 'Owner sign-in' }).evaluate((link: HTMLAnchorElement) => link.href));
    await expect(editing(owner)).toBeVisible();
});

it('answers the draft to the owner alone, and refuses the owner a write another site sends', async ({ request }) => {
    const read = (identity: Record<string, string>) => request.get('/api/draft', { headers: identity }).then((response) => response.status());
    const written = (site: Record<string, string>) => request.put('/api/draft', { headers: { ...OWNER, ...UNREVISED, ...site }, data: {} }).then((response) => response.status());
    expect({ visitor: await read({}), guest: await read(GUEST), owner: await read(OWNER), crossSite: await written({ 'sec-fetch-site': 'cross-site' }), sameOrigin: await written(SAME_ORIGIN) }).toStrictEqual({
        visitor: 403,
        guest: 403,
        owner: 200,
        crossSite: 403,
        sameOrigin: 204,
    });
});

it('keeps a saved draft with the owner across a reload while visitors see no work', async ({ page, owner }) => {
    await owner.goto(EDITING);
    await editing(owner).getByRole('button', { name: 'New project' }).click();
    await editing(owner).getByRole('textbox', { name: 'Title' }).fill('Harbour Library');
    await editing(owner).getByRole('button', { name: 'Save draft' }).click();
    await expect(editing(owner).getByText('Draft saved. Published work unchanged')).toBeVisible();
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'No work published yet' })).toBeVisible();
    await owner.reload();
    await expect(
        editing(owner)
            .getByRole('row')
            .getByRole('button', { name: /^Harbour Library/u }),
    ).toBeVisible();
});

it('saves an untitled entry as a draft, refuses to publish it, and moves to its title from the issue it names', async ({ page, owner }) => {
    await owner.goto(EDITING);
    await editing(owner).getByRole('button', { name: 'New project' }).click();
    await editing(owner).getByRole('button', { name: 'Back to entries' }).click();
    await editing(owner).getByRole('button', { name: 'Save draft' }).click();
    await expect(editing(owner).getByText('Draft saved. Published work unchanged')).toBeVisible();
    await editing(owner).getByRole('button', { name: 'Publish', exact: true }).click();
    const issue = editing(owner)
        .getByRole('alert')
        .getByRole('button', { name: /Title required to publish/u });
    await expect(issue).toBeVisible();
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'No work published yet' })).toBeVisible();
    await issue.click();
    await expect(editing(owner).getByRole('textbox', { name: 'Title' })).toBeFocused();
});

it('previews an unsaved draft on the page, and returns to the published page', async ({ owner }) => {
    await owner.goto(EDITING);
    await editing(owner).getByRole('button', { name: 'New project' }).click();
    await editing(owner).getByRole('textbox', { name: 'Title' }).fill('Harbour Library');
    await editing(owner).getByRole('button', { name: 'Preview draft' }).click();
    await expect(owner.getByText('Private preview · snapshot')).toBeVisible();
    await expect(chapters(owner)).toHaveText(['Harbour Library']);
    await owner.getByRole('button', { name: 'Return to published' }).click();
    await expect(owner.getByRole('heading', { name: 'No work published yet' })).toBeVisible();
    await expect(owner.getByText('Private preview · snapshot')).toBeHidden();
});

it('publishes a draft to the page of the owner at once and of a visitor on the next load', async ({ page, owner }) => {
    await owner.goto(EDITING);
    await editing(owner).getByRole('button', { name: 'New project' }).click();
    await editing(owner).getByRole('textbox', { name: 'Title' }).fill('Harbour Library');
    await editing(owner).getByRole('button', { name: 'Publish', exact: true }).click();
    await expect(editing(owner).getByText('Draft saved and published')).toBeVisible();
    await editing(owner).getByRole('button', { name: 'Close portfolio editor' }).click();
    await expect(chapters(owner)).toHaveText(['Harbour Library']);
    await page.goto('/');
    await expect(chapters(page)).toHaveText(['Harbour Library']);
});

it('removes an entry from the published page once its removal is published', async ({ page, owner, served }) => {
    await served.publish(TWO);
    await owner.goto(EDITING);
    await editing(owner).getByRole('row').filter({ hasText: 'Timber Joints' }).getByRole('button', { name: 'Remove entry' }).click();
    await owner.getByRole('alertdialog').getByRole('button', { name: 'Remove entry' }).click();
    await editing(owner).getByRole('button', { name: 'Publish', exact: true }).click();
    await expect(editing(owner).getByText('Draft saved and published')).toBeVisible();
    await page.goto('/');
    await expect(chapters(page)).toHaveText(['Harbour Library']);
});

it('keeps edits when another tab saved a newer draft, and saves them over it on request', async ({ owner, request }) => {
    await owner.goto(EDITING);
    await editing(owner).getByRole('button', { name: 'New project' }).click();
    await editing(owner).getByRole('textbox', { name: 'Title' }).fill('Harbour Library');
    await request.put('/api/draft', { headers: { ...OWNER, ...SAME_ORIGIN, ...UNREVISED }, data: { name: 'Second tab' } });
    await editing(owner).getByRole('button', { name: 'Save draft' }).click();
    await expect(editing(owner).getByRole('alert')).toContainText('Draft revision 1 saved in another tab');
    await expect(editing(owner).getByRole('textbox', { name: 'Title' })).toHaveValue('Harbour Library');
    await editing(owner).getByRole('button', { name: 'Replace newer draft' }).click();
    await owner.getByRole('alertdialog').getByRole('button', { name: 'Replace newer draft' }).click();
    await expect(editing(owner).getByText('Draft saved. Published work unchanged')).toBeVisible();
    const stored: unknown = await request.get('/api/draft', { headers: OWNER }).then((response) => response.json());
    expect(stored).toMatchObject({ portfolio: { entries: [{ title: 'Harbour Library' }] } });
    expect(stored).not.toHaveProperty('portfolio.name');
});

it('deletes a library file no work places, and refuses to delete a file the published page places', async ({ served, request }) => {
    await served.publish({ files: [...content.files, SPARE], portfolio: content.portfolio });
    const deleted = (id: string) => request.delete(`/api/assets/${id}`, { headers: OWNED }).then((response) => response.status());
    const read = (id: string) => request.get(`/api/assets/${id}`, { headers: OWNER }).then((response) => response.status());
    expect({ placed: await deleted(PLACED.asset.id), spare: await deleted(SPARE.asset.id), kept: await read(PLACED.asset.id), gone: await read(SPARE.asset.id) }).toStrictEqual({ placed: 409, spare: 204, kept: 200, gone: 404 });
});

it('lists an upload that never finished for recovery, and removes it once discarded', async ({ request }) => {
    const listed = (): Promise<unknown> => request.get('/api/assets/recovery', { headers: OWNER }).then((response) => response.json());
    const discarded = () => request.post(`/api/assets/${SPARE.asset.id}/discard`, { headers: OWNED }).then((response) => response.status());
    await request.post('/api/assets', { headers: OWNED, data: SPARE.asset });
    expect({ before: await listed(), discarded: await discarded(), after: await listed(), again: await discarded() }).toStrictEqual({
        before: [{ id: SPARE.asset.id, name: 'spare-section.png', status: 'incomplete' }],
        discarded: 204,
        after: [],
        again: 404,
    });
});
