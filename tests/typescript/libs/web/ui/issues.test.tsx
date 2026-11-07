import { Field } from '@rasm/ui/field';
import { Issues } from '@rasm/ui/issues';
import { Array, Option, Result, Schema, SchemaParser } from 'effect';
import { Form } from 'react-aria-components';
import { expect, it, vi } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [MODELS] --------------------------------------------------------------------------

const Portfolio = Schema.Struct({ entries: Schema.Array(Schema.Struct({ title: Schema.NonEmptyString })) });

// --- [FIXTURES] ------------------------------------------------------------------------

const issue = Option.getOrThrow(Result.getFailure(SchemaParser.decodeResult(Portfolio)({ entries: [{ title: 'Plan' }, { title: '' }] }, { errors: 'all' })));

// --- [COMPOSITION] ---------------------------------------------------------------------

it('maps each issue path to keys and segments with key segments read from their object form', () => {
    expect(Issues(issue).map(({ path }) => path)).toEqual([['entries', 1, 'title']]);
});
it('routes field errors by the dot-joined path or by a caller name mapping', async () => {
    const screen = await render(
        <Form>
            <Issues.Fields issue={Option.some(issue)}>
                <Field label="Dotted" name="entries.1.title" />
            </Issues.Fields>
            <Issues.Fields issue={Option.some(issue)} name={(path) => path.map((segment) => (typeof segment === 'number' ? `[${segment}]` : `.${String(segment)}`)).join('')}>
                <Field label="Bracketed" name=".entries[1].title" />
            </Issues.Fields>
        </Form>,
    );
    await expect.element(screen.getByRole('textbox', { name: 'Dotted' })).toHaveAttribute('aria-invalid', 'true');
    await expect.element(screen.getByRole('textbox', { name: 'Bracketed' })).toHaveAttribute('aria-invalid', 'true');
});
it('lists each issue once through the caller text and reports its path on press', async () => {
    const onField = vi.fn<(path: readonly PropertyKey[]) => void>();
    const text = ({ path, message }: ReturnType<typeof Issues>[number]) => `Entry ${String(Number(path[1]) + 1)} title: ${message}`;
    const screen = await render(<Issues.List issue={issue} onField={onField} text={text} />);
    await screen.getByRole('button', { name: text(Option.getOrThrow(Array.head(Issues(issue)))) }).click();
    expect(onField).toHaveBeenCalledWith(['entries', 1, 'title']);
});
