import { Array, Equivalence, Option, pipe, Record, SchemaIssue, Struct } from 'effect';
import type { ReactNode } from 'react';
import { Button, FormValidationContext } from 'react-aria-components';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standardFormatter = SchemaIssue.makeFormatterStandardSchemaV1();
const MINIMUM_TARGET = { minBlockSize: 24, minInlineSize: 24 } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const lines = (issue: SchemaIssue.Issue) => standardFormatter(issue).issues.map(({ path = [], message }) => ({ path: path.map((segment): PropertyKey => (typeof segment === 'object' ? segment.key : segment)), message }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Issues = Object.assign(lines, {
    Fields: ({ issue, name, children }: { readonly issue: Option.Option<SchemaIssue.Issue>; readonly name?: (path: readonly PropertyKey[]) => string; readonly children: ReactNode }) => (
        <FormValidationContext
            value={Record.map(
                Array.groupBy(Option.toArray(issue).flatMap(lines), ({ path }) => name?.(path) ?? path.map(String).join('.')),
                Array.map(Struct.get('message')),
            )}
        >
            {children}
        </FormValidationContext>
    ),
    List: ({ issue, text, onField }: { readonly issue: SchemaIssue.Issue; readonly text?: (line: ReturnType<typeof lines>[number]) => string; readonly onField?: ((path: readonly PropertyKey[]) => void) | undefined }) => (
        <ul>
            {pipe(
                lines(issue),
                Array.map((line) => ({ path: line.path, shown: text?.(line) ?? (Array.isReadonlyArrayNonEmpty(line.path) ? `${line.path.map(String).join(' › ')} · ${line.message}` : line.message) })),
                Array.dedupeWith(Equivalence.mapInput(Equivalence.String, Struct.get('shown'))),
                Array.map(({ path, shown }) => (
                    <li key={shown}>
                        {onField === undefined ? (
                            shown
                        ) : (
                            <Button onPress={() => onField(path)} style={MINIMUM_TARGET}>
                                {shown}
                            </Button>
                        )}
                    </li>
                )),
            )}
        </ul>
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Issues };
