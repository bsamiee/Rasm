import { Array, Option, pipe, Record, SchemaIssue, Struct } from 'effect';
import type { ReactNode } from 'react';
import { Button, FormValidationContext } from 'react-aria-components';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standard = SchemaIssue.makeFormatterStandardSchemaV1();

// --- [OPERATIONS] ----------------------------------------------------------------------

const lines = (issue: SchemaIssue.Issue, formatter = standard) => formatter(issue).issues.map(({ path = [], message }) => ({ path: path.map((segment): PropertyKey => (typeof segment === 'object' ? segment.key : segment)), message }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Issues = Object.assign(lines, {
    Fields: ({ issue, formatter, name, children }: { readonly issue: Option.Option<SchemaIssue.Issue>; readonly formatter?: typeof standard; readonly name?: (path: readonly PropertyKey[]) => string; readonly children: ReactNode }) => (
        <FormValidationContext
            value={pipe(
                Option.toArray(issue).flatMap((rejected) => lines(rejected, formatter)),
                Array.groupBy(({ path }) => name?.(path) ?? path.map(String).join('.')),
                Record.map(Array.map(Struct.get('message'))),
            )}
        >
            {children}
        </FormValidationContext>
    ),
    List: ({ issue, formatter, text, onField }: { readonly issue: SchemaIssue.Issue; readonly formatter?: typeof standard; readonly text?: (line: ReturnType<typeof lines>[number]) => string; readonly onField?: ((path: readonly PropertyKey[]) => void) | undefined }) => (
        <ul>
            {pipe(
                lines(issue, formatter),
                Array.map((line) => ({ path: line.path, text: text?.(line) ?? (Array.isReadonlyArrayNonEmpty(line.path) ? `${line.path.map(String).join(' › ')} · ${line.message}` : line.message) })),
                Array.dedupeWith((left, right) => left.text === right.text),
                Array.map(({ path, text: shown }) => <li key={shown}>{onField === undefined ? shown : <Button onPress={() => onField(path)}>{shown}</Button>}</li>),
            )}
        </ul>
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Issues };
