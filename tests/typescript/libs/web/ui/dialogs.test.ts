import { dialogOpen, openDialogs } from '@rasm/ui';
import { AtomRegistry } from 'effect/reactivity';
import { expect, it } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('opens and closes each dialog id apart from the others in the page-wide set', () => {
    const registry = AtomRegistry.make();
    registry.set(dialogOpen('editor'), true);
    registry.set(dialogOpen('uploads'), true);
    registry.set(dialogOpen('editor'), false);
    expect([registry.get(dialogOpen('editor')), registry.get(dialogOpen('uploads')), [...registry.get(openDialogs)]]).toEqual([false, true, ['uploads']]);
});
