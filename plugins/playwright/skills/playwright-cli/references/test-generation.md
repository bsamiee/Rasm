# [TEST_GENERATION]

Plans list a feature's scenarios, test files hold one scenario each, and healing fixes a failing test.

Test files wait through web-first assertions and hold none of:
- `networkidle`, `waitForLoadState`, `waitForNavigation`, or `waitForTimeout` waits
- `page.evaluate` calls or skipped hooks

## [01]-[SEED]

Seed tests under the config's `testDir` put the page in the state every scenario starts from (navigation, login, feature flags):
- `fixtures.ts` beside the config exports a `test` with a `page` fixture that navigates
- `seed.spec.ts` requests that fixture and asserts the page it opens

```ts
import { test as base } from '@playwright/test';

export const test = base.extend({
    page: async ({ page }, use) => {
        await page.goto('/');
        await use(page);
    },
});
```

```ts
import { expect } from '@playwright/test';
import { test } from './fixtures.ts';

test('seed', async ({ page }) => {
    await expect(page).toHaveTitle('<title>');
});
```

## [02]-[PLANNING]

Plans sit at `specs/<feature>.plan.md` in the config directory, where the generator finds them.

Exploration covers:
- Interactive elements (forms, buttons, lists, filters, modals)
- Primary user journeys end to end, for each user type the page serves
- Edge cases (empty states, validation errors, long input, boundary values)
- Persistence across reload, storage, and URL fragments
- Controls that change the URL, with back and forward behavior

Plans list groups of scenarios, each scenario independent and starting from the seed's fresh state:
- Test file names are the scenario name in kebab-case (`Add single todo` is `add-single-todo.spec.ts`)
- Scenarios cover the happy path, edge cases, validation, negative flows, and persistence
- Steps read at the user level (`Type 'Buy milk' into the input`), each `- expect:` line becomes one assertion

```markdown
# <Feature> Test Plan

## Application Overview

<One paragraph describing what the feature does>

## Test Scenarios

### 1. <Group Name>

**Seed:** `<testDir>/seed.spec.ts`

#### 1.1. <Scenario name>

**File:** `<testDir>/<group>/<scenario-name>.spec.ts`

**Steps:**
  1. <Concrete user step>
    - expect: <observable outcome>
    - expect: <another observable outcome>
  2. <Next step>
    - expect: <outcome>

#### 1.2. <Next scenario>

### 2. <Next Group>

**Seed:** `<testDir>/seed.spec.ts`
```

## [03]-[TEST_FILES]

Test files hold the recorded code at the path the plan names, one test per file, each `- expect:` line as one assertion:
- File path, describe name, and test name come verbatim from the plan without the ordinal
- `test` comes from the fixtures file by its path from the test (`../fixtures.ts` under `<testDir>/<group>/`), `expect` from `@playwright/test`
- Tests importing `test` from `@playwright/test` skip the fixture's navigation and time out at the first locator
- Locators used twice take a local variable

```ts
import { expect } from '@playwright/test';
import { test } from '../fixtures.ts';

test.describe('Signing in and out', () => {
    test('Sign in with valid credentials', async ({ page }) => {
        await page.getByRole('textbox', { name: 'username' }).fill('John Doe');
        await page.getByRole('textbox', { name: 'password' }).fill('TestPassword');
        await page.keyboard.press('Enter');
        await expect(page.getByRole('heading')).toContainText('Welcome, John Doe!');
    });
});
```

## [04]-[HEALING]

Healing fixes failing tests one error at a time, with a rerun after each fix, and names one cause of each failure:
- Locator drift: a changed selector, a new wrapper element, a renamed label or ARIA name
- Timing gaps, changed assertion text, and data leaked between runs
- Environment dependencies and application changes behind a test assumption

Fixes follow these rules:
- Fixed assertions on dynamic data take a regular expression
- `test.fixme()` marks the test after 3 fix-and-rerun cycles when the failure persists and the test reads correct

Plans whose `**File:**` line names the test reconcile with the fix at the matching scenario:
- Technical fixes (locator drift, assertion shape) leave the plan alone
- Fixes that change user-visible steps, inputs, order, or outcomes update the step and expect lines, scenario id and file path kept
