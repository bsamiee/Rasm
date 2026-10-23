import { focus } from '@rasm/ui';
import { expect, it } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('focuses element with ID that needs escaping after next frame', async () => {
    const id = 'entry:1 title';
    const screen = await render(<input aria-label="Title" id={id} />);
    focus(id);
    await expect.element(screen.getByRole('textbox', { name: 'Title' })).toHaveFocus();
});
