import { focusElement } from '@rasm/ui';
import { Effect, flow } from 'effect';
import { expect, it } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const nextFrame = () =>
    Effect.runPromise(
        Effect.callback<number>((resume) => {
            const frame = requestAnimationFrame(flow(Effect.succeed, resume));
            return Effect.sync(() => cancelAnimationFrame(frame));
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it('focuses element with ID that needs escaping after next frame', async () => {
    const id = 'entry:1 title';
    const screen = await render(<input aria-label="Title" id={id} />);
    focusElement(id);
    await expect.element(screen.getByRole('textbox', { name: 'Title' })).toHaveFocus();
});
it('focuses element a render commits after the call within the same frame', async () => {
    const id = 'entry:2 title';
    const screen = await render(<input aria-label="Source" />);
    focusElement(id);
    await screen.rerender(
        <>
            <input aria-label="Source" />
            <input aria-label="Title" id={id} />
        </>,
    );
    await expect.element(screen.getByRole('textbox', { name: 'Title' })).toHaveFocus();
});
it('leaves focus in place when the target unmounts before the frame', async () => {
    const id = 'entry:3 title';
    const screen = await render(
        <>
            <input aria-label="Source" />
            <input aria-label="Title" id={id} />
        </>,
    );
    const source = screen.getByRole('textbox', { name: 'Source' });
    await source.click();
    focusElement(id);
    await screen.rerender(<input aria-label="Source" />);
    await nextFrame();
    await expect.element(source).toHaveFocus();
});
