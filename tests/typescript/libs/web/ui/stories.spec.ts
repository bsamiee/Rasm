import { expect } from '@playwright/test';
import { it } from '@rasm/testing/playwright';
import { Effect, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const everyStory = 180_000;

// --- [MODELS] --------------------------------------------------------------------------

const Index = Schema.fromJsonString(Schema.Struct({ entries: Schema.Record(Schema.String, Schema.Struct({ id: Schema.String, type: Schema.Literals(['story', 'docs']), title: Schema.String })) })).annotate({
    description: 'index.json of a served Storybook',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it('serves a docs page for every component and stories that break no WCAG A or AA rule', async ({ page, request, axe }) => {
    it.setTimeout(everyStory);
    const { entries } = await Schema.decodePromise(Index)(await (await request.get('index.json')).text());
    const listed = Object.values(entries);
    const documented = new Set(listed.filter(({ type }) => type === 'docs').map(({ title }) => title));
    expect(listed.filter(({ title }) => !documented.has(title))).toEqual([]);
    const visited = await Effect.runPromise(
        Effect.forEach(
            listed.filter(({ type }) => type === 'story'),
            ({ id }) =>
                Effect.tryPromise(async () => {
                    await page.goto(`iframe.html?viewMode=story&id=${id}&globals=a11y.manual:!true`);
                    await expect(page.locator('#storybook-root > *, [role="dialog"]').first()).toBeAttached();
                    return { id, broken: (await axe.analyze()).violations };
                }),
        ),
    );
    expect(visited.flatMap(({ id, broken }) => broken.map((violation) => `${id} ${violation.id}`))).toEqual([]);
});
