import { expect } from '@playwright/test';
import { it } from '#/playwright';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('axe reports the rule a page breaks and nothing on a page that breaks none', async ({ page, axe }) => {
    await page.setContent('<!doctype html><html lang="en"><head><title>Sheet</title></head><body><main><h1>Sheet</h1><img src="data:image/gif;base64,R0lGODlhAQABAAAAACw=" /></main></body></html>');
    expect((await axe.analyze()).violations.map(({ id }) => id)).toEqual(['image-alt']);
    await page.setContent('<!doctype html><html lang="en"><head><title>Sheet</title></head><body><main><h1>Sheet</h1><img alt="Plan" src="data:image/gif;base64,R0lGODlhAQABAAAAACw=" /></main></body></html>');
    expect((await axe.analyze()).violations).toEqual([]);
});

it('the base keeps a trace and a screenshot of a failed test alone', async ({ page }, { project }) => {
    await page.setContent('<!doctype html><title>Sheet</title>');
    expect([project.use.trace, project.use.screenshot]).toEqual(['retain-on-failure', 'only-on-failure']);
});
