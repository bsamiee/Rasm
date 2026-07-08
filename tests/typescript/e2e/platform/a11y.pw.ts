import { expect, test } from '../fixtures.ts';

// The axe conformance gauge over the deterministic corpus: the audit's rule tags are the fixture's
// policy row, so a served product project inherits the same wcag floor with zero new surface.
test.describe('a11y gauge', () => {
    test('the intake form clears the wcag floor end to end', async ({ a11y, target }) => {
        await target.open('/form');
        expect(await a11y()).toEqual([]);
    });

    test('an unlabeled control refutes the audit — the gauge can fail', async ({ a11y, page, target }) => {
        await target.open('/form');
        await page.evaluate(() => document.querySelector('label')?.remove());
        expect((await a11y()).map((violation) => violation.id)).toContain('label');
    });

    test('a scoped audit never reads outside its selector', async ({ a11y, page, target }) => {
        await target.open('/form');
        await page.evaluate(() => document.querySelector('html')?.removeAttribute('lang'));
        expect((await a11y()).map((violation) => violation.id)).toContain('html-has-lang');
        expect(await a11y('main')).toEqual([]);
    });
});
