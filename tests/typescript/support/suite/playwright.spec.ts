import { expect } from '@playwright/test';
import { it } from '#/playwright';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TARGETS = {
    cramped: 'button { inline-size: 12px; block-size: 12px; padding: 0; border: 0; margin: 0 }',
    spaced: 'button { inline-size: 24px; block-size: 24px; padding: 0; border: 0; margin: 0 }',
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it('axe reports a WCAG 2.2 AA failure, reports no best-practice finding for content outside a landmark, and reports nothing once the page meets every A and AA criterion', async ({ page, axe }) => {
    const markup = '<!doctype html><html lang="en"><head><title>Sheet</title></head><body><h1>Sheet</h1><p><button type="button" aria-label="Up"></button><button type="button" aria-label="Down"></button></p></body></html>';
    await page.setContent(markup);
    await page.addStyleTag({ content: TARGETS.cramped });
    expect((await axe.analyze()).violations.map(({ id }) => id)).toEqual(['target-size']);
    await page.setContent(markup);
    await page.addStyleTag({ content: TARGETS.spaced });
    expect((await axe.analyze()).violations).toEqual([]);
});
