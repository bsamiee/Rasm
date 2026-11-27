import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Effect, Formatter, Match, Number, Option, Order, Predicate, Record, Result, Schema, SchemaAST, SchemaIssue, type Stream, Struct, UndefinedOr } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Report } from '#/report';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LISTING = new Intl.ListFormat('en');

// --- [SERVICES] ------------------------------------------------------------------------

const ToolCall = Object.assign(
    Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly progress: (report: Omit<Parameters<McpServer.McpServer['Service']['notifications']['notifications/progress']>[0], 'progressToken' | '_meta'>) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
        readonly returnEarly: (ends: Stream.Stream<Effect.Effect<string>>) => Effect.Effect<void>;
        readonly awaitRunning: Effect.Effect<void>;
        readonly call: number;
    }>('@rasm/mcp/toolkit/ToolCall', {
        defaultValue: () => ({
            add: () => Effect.void,
            progress: () => Effect.void,
            input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })),
            returnEarly: () => Effect.void,
            awaitRunning: Effect.void,
            call: 0,
        }),
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
const keysAmiss = (member: SchemaIssue.Issue): number =>
    Match.value(member).pipe(
        Match.tag('Composite', ({ issues }) => Number.sumAll(issues.map(keysAmiss))),
        Match.tag('Encoding', 'Filter', ({ issue }) => keysAmiss(issue)),
        Match.tag('Pointer', ({ issue }) => (Predicate.isTagged(issue, 'MissingKey') || Predicate.isTagged(issue, 'UnexpectedKey') ? 1 : 0)),
        Match.orElse(() => 0),
    );
const issueLines = (issue: SchemaIssue.Issue, at: readonly PropertyKey[]): readonly string[] => {
    if (Predicate.isTagged(issue, 'Pointer')) {
        return Predicate.isTagged(issue.issue, 'UnexpectedKey') || (Predicate.isTagged(issue.issue, 'InvalidType') && SchemaAST.isNever(issue.issue.ast)) ? [`${parameterPath([...at, ...issue.path])}: ${nearestForms(issue.path.map(String).join('.'), issue.issue.ast)}`] : issueLines(issue.issue, [...at, ...issue.path]);
    }
    if (Predicate.isTagged(issue, 'Encoding')) {
        return issueLines(issue.issue, at);
    }
    if (Predicate.isTagged(issue, 'AnyOf') && Array.isReadonlyArrayNonEmpty(issue.issues)) {
        return issueLines(Array.min(issue.issues, Order.mapInput(Order.Number, keysAmiss)), at);
    }
    return Predicate.isTagged(issue, 'Composite') && Array.isReadonlyArrayNonEmpty(issue.issues)
        ? Array.flatMap(issue.issues, (inner) => issueLines(inner, at))
        : Array.map(standardIssues(issue).issues, ({ path = [], message }) => Array.match([...at, ...path], { onEmpty: () => message, onNonEmpty: (segments) => `${parameterPath(segments)}: ${message}` }));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const registerToolkit: <Tools extends Record.ReadonlyRecord<string, Tool.Any & { readonly parametersSchema: Schema.Decoder<unknown> }>>(
    toolkit: Toolkit.Toolkit<Tools>,
    options?: { readonly descriptions?: { readonly [Name in keyof Tools]?: string }; readonly annotations?: Context.Context<never> },
) => Effect.Effect<void, never, McpServer.McpServer | McpServer.ToolkitRequirements<Tools>> = Effect.fnUntraced(function* <Tools extends Record.ReadonlyRecord<string, Tool.Any & { readonly parametersSchema: Schema.Decoder<unknown> }>>(
    toolkit: Toolkit.Toolkit<Tools>,
    { descriptions = {}, annotations = Context.empty() }: { readonly descriptions?: Record.ReadonlyRecord<string, string | undefined>; readonly annotations?: Context.Context<never> } = {},
) {
    const server = yield* McpServer.McpServer;
    const decoders: Record.ReadonlyRecord<string, (payload: unknown, options: SchemaAST.ParseOptions) => Effect.Effect<unknown, Schema.SchemaError>> = Record.map(toolkit.tools, (tool) => Schema.decodeUnknownEffect(Schema.toCodecJson(tool.parametersSchema)));
    const served = McpServer.McpServer.of({
        ...server,
        addTool: ({ services = Context.empty(), ...registration }) =>
            server.addTool({
                ...registration,
                services,
                annotations: Context.merge(registration.annotations, annotations),
                tool: UndefinedOr.match(descriptions[registration.tool.name], { onUndefined: () => registration.tool, onDefined: (clause) => McpSchema.Tool.make({ ...registration.tool, description: [...Array.fromNullishOr(registration.tool.description), clause].join('; ') }) }),
                handle: (payload: Record.ReadonlyRecord<string, Schema.Json>) =>
                    registration.handle(payload).pipe(
                        Effect.catchTag('InvalidParams', (refused) =>
                            Option.match(Record.get(decoders, registration.tool.name), {
                                onNone: () => Effect.fail(refused),
                                onSome: (decode) => Effect.matchEffect(decode(payload, { errors: 'all', onExcessProperty: 'error' }), { onFailure: ({ issue }) => ToolCall.invalidParams(issue), onSuccess: () => refused }),
                            }),
                        ),
                        Effect.map((outcome) =>
                            outcome instanceof McpSchema.InputRequired
                                ? outcome
                                : McpSchema.CallToolResult.make({
                                      ...Struct.omit(outcome, ['structuredContent']),
                                      content: UndefinedOr.match(outcome.structuredContent, { onUndefined: () => outcome.content, onDefined: (encoded) => [McpSchema.TextContent.make({ text: Predicate.isString(encoded) ? encoded : Report.text(Report.of(encoded)) })] }),
                                  }),
                        ),
                        Effect.updateContext((request: Context.Context<McpSchema.McpRequestContext>) => Context.merge(services, request)),
                    ),
            }),
    });
    yield* toolkit.pipe(
        Struct.lambda<<Held extends Record.ReadonlyRecord<string, Tool.Any>>(held: Toolkit.Toolkit<Held>) => Toolkit.Toolkit<Held>>((held) => Toolkit.make(...Record.values(held.tools).map((tool) => tool.annotate(Tool.Strict, true)))),
        McpServer.registerToolkit,
        Effect.provideService(McpServer.McpServer, served),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerToolkit, ToolCall };
