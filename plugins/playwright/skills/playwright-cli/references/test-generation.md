# [TEST_GENERATION]

Every `playwright cli` action prints its Playwright TypeScript. Planning writes the scenarios a feature needs to a spec file, generation walks each scenario into a test file, and healing fixes a failing test at its paused page, each through a test paused under `--debug=cli`.

Use playwright-tests.md for the paused run and its commands.

## [01]-[GENERATION]

Each action prints `### Ran Playwright code` with the line a test runs, role-based locators where the page offers a role and name.

```bash
playwright cli open https://example.com/login
playwright cli snapshot
playwright cli fill e1 "user@example.com"
# await page.getByRole('textbox', { name: 'Email' }).fill('user@example.com');
playwright cli fill e2 "password123"
# await page.getByRole('textbox', { name: 'Password' }).fill('password123');
playwright cli click e3
# await page.getByRole('button', { name: 'Sign In' }).click();
```

Actions print no assertion, each `expect` comes from a locator and a value read at the page:
- `--raw generate-locator <ref>` prints the locator expression
- `--raw eval "el => el.textContent" <ref>` prints the text as a JSON string for `toHaveText`, `el.value` the value for `toHaveValue` and `toBeEmpty`
- `--raw run-code "page => <locator>.ariaSnapshot()" | jq -r .` prints the `toMatchAriaSnapshot` template, `page.ariaSnapshot()` the whole page
- Templates take a regular expression for an unstable value
- Refs printed before `run-code` fail `not found` until the next `snapshot`
- Locators built from the asserted text take `toBeVisible`, `getByTestId` and `getByLabel` locators take `toHaveText`

```bash
playwright cli --raw generate-locator e5
# getByRole('button', { name: 'Submit' })
playwright cli --raw eval "el => el.textContent" e5
playwright cli --raw eval "el => el.value" e5
playwright cli --raw run-code "page => page.getByRole('navigation').ariaSnapshot()" | jq -r .
```

```typescript
import { expect, test } from 'playwright/test';

test('login flow', async ({ page }) => {
    await page.goto('https://example.com/login');
    await page.getByRole('textbox', { name: 'Email' }).fill('user@example.com');
    await page.getByRole('textbox', { name: 'Password' }).fill('password123');
    await page.getByRole('button', { name: 'Sign In' }).click();

    await expect(page).toHaveURL(/.*dashboard/);
    await expect(page.getByRole('alert', { name: 'Success' })).toBeVisible();
    await expect(page.getByTestId('main-header')).toHaveText('Welcome, user');
    await expect(page.getByRole('textbox', { name: 'Email' })).toHaveValue('user@example.com');
    await expect(page.getByRole('checkbox', { name: 'Enable notifications' })).toBeChecked();
    await expect(page.getByRole('navigation')).toMatchAriaSnapshot(`
        - link "Home"
        - link /\\d+ new messages?/
        - link "Profile"
    `);
});
```

## [02]-[PLANNING]

Planning writes `specs/<feature>.plan.md` listing the scenarios to test. Project `playwright.config.ts` names `testDir`, a seed test under it puts the page in the state every scenario starts from (navigation, login, feature flags). `--debug=cli` pauses the seed at its first Playwright call, the fixture's `page.goto`, every planning and generation session begins there.

```ts
// tests/fixtures.ts
import { test as baseTest } from 'playwright/test';
export { expect } from 'playwright/test';

export const test = baseTest.extend({
    page: async ({ page }, use) => {
        await page.goto('https://example.com/');
        await use(page);
    },
});
```

```ts
// tests/seed.spec.ts
import { test } from './fixtures';

test('seed', async ({ page }) => {
    // Fixture navigates, scenarios start from this state
});
```

Exploration runs through the seed, the page holds the setup the seed performs. `step-over` runs the seed's calls until `### Paused` names `Close context`, the seed's setup done and the page open for every command:

```bash
playwright test -c <project> seed.spec.ts --debug=cli
playwright cli attach tw-<id>
playwright cli -s=tw-<id> step-over
playwright cli -s=tw-<id> snapshot
playwright cli -s=tw-<id> click e5
playwright cli -s=tw-<id> eval "location.href"
playwright cli -s=tw-<id> resume
```

Exploration covers:
- Interactive elements (forms, buttons, lists, filters, modals)
- Primary user journeys end to end
- Edge cases (empty states, validation errors, long input, boundary values)
- Persistence across reload, storage, and URL fragments
- Controls that change the URL, with back and forward behavior

Spec files list groups of scenarios, each scenario independent and starting from the seed's fresh state:
- Scenario names are kebab-case and name the test file (`should-add-single-todo` is `should-add-single-todo.spec.ts`)
- Scenarios cover the happy path, edge cases, validation, negative flows, and persistence
- Steps read at the user level (`Type 'Buy milk' into the input`), each `- expect:` line becomes one assertion

```markdown
# <Feature> Test Plan

## Application Overview

<One paragraph describing what the feature does>

## Test Scenarios

### 1. <Group Name>

**Seed:** `tests/seed.spec.ts`

#### 1.1. <kebab-case-scenario-name>

**File:** `tests/<group>/<kebab-case-scenario-name>.spec.ts`

**Steps:**
1. <Concrete user step>
    - expect: <observable outcome>
    - expect: <another observable outcome>
2. <Next step>
    - expect: <outcome>

#### 1.2. <next-scenario>

### 2. <Next Group>

**Seed:** `tests/seed.spec.ts`
```

## [03]-[GENERATE]

Generation takes a spec file, a target (one scenario `1.2`, one group `1`, or every scenario), and the seed file of the scenario's `**Seed:**` line. Each scenario runs in its own paused seed, one at a time. Live app decides over the spec, a vague step, a missing element, or a contradicted behavior updates the spec line and the walk continues.

```bash
playwright test -c <project> seed.spec.ts --debug=cli
playwright cli attach tw-<id>
playwright cli -s=tw-<id> step-over
playwright cli -s=tw-<id> snapshot
playwright cli -s=tw-<id> fill e3 "John Doe"
playwright cli -s=tw-<id> press Enter
playwright cli -s=tw-<id> click e7
playwright cli -s=tw-<id> resume
```

Test files collect the printed code at the path the spec names, one test per file, each `- expect:` line as one assertion:
- File path, describe name, and test name come verbatim from the spec without the ordinal
- Each numbered step opens with a `// N. <step text>` comment before its actions
- Imports come from the fixtures file by its path from the test (`../fixtures` under `tests/<group>/`), `playwright/test` without one
- `resume` ends each scenario's run and session before the next scenario restarts the seed

```ts
// spec: specs/basic-operations.plan.md
// seed: tests/seed.spec.ts
import { expect, test } from '../fixtures';

test.describe('Signing in and out', () => {
    test('should-sign-in', async ({ page }) => {
        // 1. Navigate to the application
        // (handled by the seed fixture)

        // 2. Type 'John Doe' into the username field
        await page.getByRole('textbox', { name: 'username' }).fill('John Doe');

        // 3. Type password
        await page.getByRole('textbox', { name: 'password' }).fill('TestPassword');

        // 4. Press Enter to submit
        await page.keyboard.press('Enter');

        await expect(page.getByRole('heading')).toContainText('Welcome, John Doe!');
    });
});
```

Generated tests run once, a failure goes to healing:

```bash
playwright test -c <project> <scenario>.spec.ts
```

## [04]-[HEAL]

Healing fixes failing tests one error at a time, with a rerun after each fix:
1. `playwright test -c <project>` lists the failing `<file>:<line>` entries, each with its `error-context.md`
2. One failing test runs paused in the background, `attach` binds its session, `pause-at <file>:<line>` runs up to the failing action or assertion
3. `snapshot`, `console`, `requests`, `generate-locator`, and `eval` at the pause name the cause, `step-over` runs the failing call
4. Corrected interactions run through `playwright cli`, the printed code replaces the failing locator, assertion, step order, or input in the test
5. `resume` ends the paused run, `--last-failed` reruns the failures, a pass ends the cycle, the next error starts one
6. `test.fixme()` marks the test after 3 fix-and-rerun cycles when the failure persists and the test reads correct
7. Fixme comments sit before the failing step and state what happens in place of the expected behavior

Causes are a changed selector, a new wrapper element, a renamed label or ARIA name, a timing gap, an assertion text the app changed, or data leaking between runs. Web-first assertions wait, `networkidle`, `waitForTimeout`, and skipped hooks stay out of every fix.

```bash
playwright test -c <project> <scenario>.spec.ts:<test-line> --debug=cli
playwright cli attach tw-<id>
playwright cli -s=tw-<id> pause-at <scenario>.spec.ts:<failing-line>
playwright cli -s=tw-<id> snapshot
playwright cli -s=tw-<id> console
playwright cli -s=tw-<id> requests
playwright cli -s=tw-<id> --raw generate-locator e10
playwright cli -s=tw-<id> --raw eval "el => el.textContent" e10
playwright cli -s=tw-<id> resume
playwright test -c <project> --last-failed
```

Spec files the test's `// spec:` header names reconcile with the fix at the matching scenario. Technical fixes (locator drift, assertion shape) leave the spec alone, fixes that change user-visible steps, inputs, order, or outcomes update the step and expect lines with the scenario id and file path kept.
