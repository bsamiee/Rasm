import { Array, Option, Record, SchemaIssue, Struct } from 'effect';
import type { ReactNode } from 'react';
import { Button, FormValidationContext } from 'react-aria-components';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standard = SchemaIssue.makeFormatterStandardSchemaV1();

// --- [OPERATIONS] ----------------------------------------------------------------------

const lines = (issue: SchemaIssue.Issue, formatter = standard) =>
    formatter(issue).issues.map(({ path = [], message }) => ({ path: path.map((segment): PropertyKey => (typeof segment === 'object' ? segment.key : segment)), message }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Issues = Object.assign(lines, {
    Fields: ({ issue, formatter, name = (path) => path.map(String).join('.'), children }: { readonly issue: Option.Option<SchemaIssue.Issue>; readonly formatter?: typeof standard; readonly name?: (path: readonly PropertyKey[]) => string; readonly children: ReactNode }) => (
        <FormValidationContext value={Option.match(issue, { onNone: () => ({}), onSome: (rejected) => Record.map(Array.groupBy(lines(rejected, formatter), ({ path }) => name(path)), Array.map(Struct.get('message'))) })}>{children}</FormValidationContext>
    ),
    List: ({
        issue,
        formatter,
        text = ({ path, message }) => (Array.isReadonlyArrayNonEmpty(path) ? `${path.map(String).join(' › ')} · ${message}` : message),
        onField,
    }: {
        readonly issue: SchemaIssue.Issue;
        readonly formatter?: typeof standard;
        readonly text?: (line: ReturnType<typeof lines>[number]) => string;
        readonly onField?: ((path: readonly PropertyKey[]) => void) | undefined;
    }) => (
        <ul>
            {Array.dedupeWith(
                lines(issue, formatter).map((line) => ({ path: line.path, text: text(line) })),
                (left, right) => left.text === right.text,
            ).map(({ path, text: shown }) => (
                <li key={shown}>{onField === undefined ? shown : <Button onPress={() => onField(path)}>{shown}</Button>}</li>
            ))}
        </ul>
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Issues };
