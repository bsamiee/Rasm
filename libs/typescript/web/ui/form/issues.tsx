import { Array, Option, Record, SchemaIssue, Struct } from 'effect';
import type { ReactNode } from 'react';
import { Button, FormValidationContext } from 'react-aria-components';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standard = SchemaIssue.makeFormatterStandardSchemaV1();

// --- [OPERATIONS] ----------------------------------------------------------------------

const lines = (issue: SchemaIssue.Issue) =>
    standard(issue).issues.map(({ path: segments = [], message }) => {
        const path = segments.map((segment): PropertyKey => (typeof segment === 'object' ? segment.key : segment));
        return { path, name: path.map(String).join('.'), message, text: Array.isReadonlyArrayNonEmpty(path) ? `${path.map(String).join(' › ')} · ${message}` : message };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const FieldIssues = ({ issue, children }: { readonly issue: Option.Option<SchemaIssue.Issue>; readonly children: ReactNode }) => (
    <FormValidationContext value={Option.match(issue, { onNone: () => ({}), onSome: (rejected) => Record.map(Array.groupBy(lines(rejected), Struct.get('name')), Array.map(Struct.get('message'))) })}>{children}</FormValidationContext>
);
const IssueList = ({ issue, onField }: { readonly issue: SchemaIssue.Issue; readonly onField?: ((path: readonly PropertyKey[]) => void) | undefined }) => (
    <ul>
        {Array.dedupeWith(lines(issue), (left, right) => left.text === right.text).map(({ path, text }) => (
            <li key={text}>{onField === undefined ? text : <Button onPress={() => onField(path)}>{text}</Button>}</li>
        ))}
    </ul>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { FieldIssues, IssueList };
