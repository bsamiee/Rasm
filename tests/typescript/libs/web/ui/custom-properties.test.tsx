import { CustomProperties } from '@rasm/ui/custom-properties';
import type { ComponentProps } from 'react';
import { expect, it } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _scoped = (text: string, properties: ComponentProps<typeof CustomProperties>['properties']) => <CustomProperties properties={properties}>{(scope) => <p {...scope}>{text}</p>}</CustomProperties>;
const _hoisted = (names: readonly string[], values: readonly string[]) => {
    const declared = [...document.styleSheets]
        .flatMap(({ cssRules }) => [...cssRules])
        .filter((rule) => rule instanceof CSSStyleRule)
        .flatMap(({ style }) => names.map((name) => style.getPropertyValue(name)));
    return values.map((value) => declared.filter((candidate) => candidate === value).length);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('resolves declared properties on element with scope attribute', async () => {
    const screen = await render(_scoped('Scoped', { '--ratio': 1.5, '--tone': 'red' }));
    const style = getComputedStyle(screen.getByText('Scoped').element());
    expect([style.getPropertyValue('--ratio'), style.getPropertyValue('--tone')]).toEqual(['1.5', 'red']);
});
it('resolves values holding spaces, commas, parentheses, quotes, and percentages verbatim', async () => {
    const properties = { '--pair-columns': 'minmax(0, 1.5fr) minmax(0, 1fr)', '--ratio-sum': 2.5, '--media-focus': '37.5% 100%', '--family': '"Söhne Breit", sans-serif' };
    const screen = await render(_scoped('Composition', properties));
    const style = getComputedStyle(screen.getByText('Composition').element());
    expect(Object.keys(properties).map((name) => style.getPropertyValue(name))).toEqual(Object.values(properties).map(String));
});
it('keeps scopes with different declarations apart and hoists one shared rule for equal declarations', async () => {
    const screen = await render(
        <>
            {_scoped('First', { '--ratio': 2 })}
            {_scoped('Second', { '--ratio': 2 })}
            {_scoped('Third', { '--ratio': 0.5 })}
        </>,
    );
    expect([..._hoisted(['--ratio'], ['2', '0.5']), ...['First', 'Second', 'Third'].map((text) => getComputedStyle(screen.getByText(text).element()).getPropertyValue('--ratio'))]).toEqual([1, 1, '2', '2', '0.5']);
});
it('follows changed declarations on rerender', async () => {
    const screen = await render(_scoped('Figure', { '--media-ratio': 1.5, '--media-focus': '50% 50%' }));
    await screen.rerender(_scoped('Figure', { '--media-ratio': 0.75 }));
    const element = screen.getByText('Figure').element();
    await expect.poll(() => [getComputedStyle(element).getPropertyValue('--media-ratio'), getComputedStyle(element).getPropertyValue('--media-focus')]).toEqual(['0.75', '']);
});
it('hoists one rule per declaration whatever the key order or the other declarations', async () => {
    await render(
        <>
            {_scoped('First', { '--ratio': 3, '--tone': 'blue' })}
            {_scoped('Second', { '--tone': 'blue', '--ratio': 3 })}
            {_scoped('Third', { '--ratio': 3, '--tone': 'green' })}
        </>,
    );
    expect(_hoisted(['--ratio', '--tone'], ['3', 'blue', 'green'])).toEqual([1, 1, 1]);
});
it('keeps braces, semicolons, and markup inside the declaration value', async () => {
    const screen = await render(_scoped('Escaped', { '--tone': 'red;--other:1}p{outline:solid}</style><b>' }));
    const style = getComputedStyle(screen.getByText('Escaped').element());
    expect([style.getPropertyValue('--other'), style.outlineStyle, document.querySelectorAll('b').length]).toEqual(['', 'none', 0]);
});
