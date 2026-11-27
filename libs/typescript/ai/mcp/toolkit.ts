import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Effect, Formatter, Match, Predicate, Result, SchemaAST, SchemaIssue, Struct } from 'effect';
import { McpSchema, type McpServer } from 'effect/ai';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LISTING = new Intl.ListFormat('en');

// --- [SERVICES] ------------------------------------------------------------------------

const ToolCall = Object.assign(
    Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly state: (line: string) => Effect.Effect<void>;
        readonly progress: (report: Omit<Parameters<McpServer.McpServer['Service']['notifications']['notifications/progress']>[0], 'progressToken' | '_meta'>) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
        readonly awaitRunning: Effect.Effect<void>;
    }>('@rasm/mcp/toolkit/ToolCall', {
        defaultValue: () => ({ add: () => Effect.void, state: () => Effect.void, progress: () => Effect.void, input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })), awaitRunning: Effect.void }),
    }),
    { invalidParams: (issue: SchemaIssue.Issue) => McpSchema.InvalidParams.make({ message: issueLines(issue, []).join('\n') }) },
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const standardIssues = SchemaIssue.makeFormatterStandardSchemaV1();
const parameterPath = (segments: readonly (PropertyKey | { readonly key: PropertyKey })[]) =>
    Array.reduce(segments, '', (text, segment) =>
        Match.value(typeof segment === 'object' ? segment.key : segment).pipe(
            Match.when(Predicate.isNumber, (index) => `${text}[${index}]`),
            Match.when(() => text === '', String),
            Match.orElse((key) => `${text}.${String(key)}`),
        ),
    );
const nearestForms = (query: string, ast: SchemaAST.AST) => {
    const declared = SchemaAST.isObjects(ast) ? ast.propertySignatures : [];
    const forms = Array.flatMap(declared, ({ name, type }) => [
        { name: String(name), form: String(name) },
        ...Array.filterMap(SchemaAST.isUnion(type) ? type.types : [type], (member) => (SchemaAST.isLiteral(member) ? Result.succeed({ name: String(member.literal), form: `${String(name)}:${Formatter.format(member.literal)}` }) : Result.failVoid)),
    ]);
    return Array.match(Unmatched.rank(query, forms, Struct.get('name')), {
        onEmpty: () => Array.match(declared, { onEmpty: () => 'not a parameter, this takes none', onNonEmpty: (each) => `not a parameter, the parameters here are ${LISTING.format(Array.map(each, ({ name }) => String(name)))}` }),
        onNonEmpty: (ranked) => `not a parameter, nearest ${LISTING.format(Array.map(Array.take(ranked, 3), Struct.get('form')))}`,
    });
};
const issueLines = (issue: SchemaIssue.Issue, at: readonly PropertyKey[]): readonly string[] => {
    if (Predicate.isTagged(issue, 'Pointer')) {
        return Predicate.isTagged(issue.issue, 'UnexpectedKey') || (Predicate.isTagged(issue.issue, 'InvalidType') && SchemaAST.isNever(issue.issue.ast)) ? [`${parameterPath([...at, ...issue.path])}: ${nearestForms(issue.path.map(String).join('.'), issue.issue.ast)}`] : issueLines(issue.issue, [...at, ...issue.path]);
    }
    if (Predicate.isTagged(issue, 'Encoding')) {
        return issueLines(issue.issue, at);
    }
    return (Predicate.isTagged(issue, 'Composite') || Predicate.isTagged(issue, 'AnyOf')) && Array.isReadonlyArrayNonEmpty(issue.issues)
        ? Array.flatMap(issue.issues, (inner) => issueLines(inner, at))
        : Array.map(standardIssues(issue).issues, ({ path = [], message }) => Array.match([...at, ...path], { onEmpty: () => message, onNonEmpty: (segments) => `${parameterPath(segments)}: ${message}` }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ToolCall };
