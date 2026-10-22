import { CustomProperties } from '@rasm/ui';
import { expect, it } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('resolves each declared property on the element its scope attribute selects', async () => {
    const screen = await render(<CustomProperties properties={{ '--ratio': 1.5, '--tone': 'red' }}>{(scope) => <p {...scope}>Scoped</p>}</CustomProperties>);
    const style = getComputedStyle(screen.getByText('Scoped').element());
    expect([style.getPropertyValue('--ratio'), style.getPropertyValue('--tone')]).toEqual(['1.5', 'red']);
});
it('hoists one shared rule into the head for scopes with equal declarations', async () => {
    const screen = await render(
        <>
            <CustomProperties properties={{ '--ratio': 2 }}>{(scope) => <p {...scope}>First</p>}</CustomProperties>
            <CustomProperties properties={{ '--ratio': 2 }}>{(scope) => <p {...scope}>Second</p>}</CustomProperties>
        </>,
    );
    expect([[...document.head.querySelectorAll('style')].filter(({ textContent }) => textContent.includes('--ratio:2')).length, ...['First', 'Second'].map((text) => getComputedStyle(screen.getByText(text).element()).getPropertyValue('--ratio'))]).toEqual([1, '2', '2']);
});
