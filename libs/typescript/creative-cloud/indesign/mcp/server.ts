import { listen } from '@rasm/mcp/link';
import { Progress, READING } from '@rasm/mcp/server';
import { CHARACTERS, Font as Metrics, Options, read } from '@rasm/typography/font';
import { transformSync } from 'amaro';
import { Duration, Effect, FileSystem, Formatter, Option, Path, Record, Schedule, Schema, SchemaIssue, Stream, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Specifier, Transpile } from '../models.ts';
import { Enter, Font, InDesign, PATH, Peer } from './protocol.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extension = Effect.fn('extension')(function* <Route, Encoded>(
    product: string,
    route: {
        readonly schema: Schema.Codec<Route, Encoded> & { readonly fields: Readonly<Record<string, Schema.Codec<unknown, unknown>>> };
        readonly select: (route: Route, peers: readonly (typeof Peer.Type)[]) => Option.Option<(peer: typeof Peer.Type) => boolean>;
        readonly completion: Readonly<Record<string, (peers: readonly (typeof Peer.Type)[], input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => readonly string[]>>;
    },
) {
    const peer = `InDesign with the ${product} panel`;
    const quiet = Duration.seconds(2);
    const link = yield* listen(InDesign, {
        path: PATH,
        origin: 'file://',
        peer,
        identity: Peer,
        route,
        watch: (sent, call) =>
            Stream.fromSchedule(Schedule.min([Schedule.exponential(Duration.millis(2)), Schedule.spaced(Duration.seconds(1))])).pipe(
                Stream.mapEffect(() => Effect.timeoutOption(call(Enter, {}), quiet), { concurrency: 'unbounded', unordered: true }),
                Stream.filter((phase) => !Option.contains(phase, 'Idle')),
                Stream.changes,
                Stream.runForEach((phase) =>
                    Effect.flatMap(Progress, (report) =>
                        report(
                            `${peer}: ${Option.match(phase, {
                                onNone: () => `awaiting an answer for ${Duration.toSeconds(quiet)} s; a dialog opened by the call or long work can hold it until released`,
                                onSome: (stage) => (stage === 'Held' ? 'waiting for an open menu, dialog, or drag before the call starts' : 'running the call'),
                            })}`,
                        ),
                    ),
                ),
                Effect.ignore,
                Effect.forkChild,
                Effect.andThen(sent),
            ),
    });
    const face = Tool.make('font', {
        parameters: Font.payloadSchema.pipe(Schema.fieldsAssign(Options.fields)),
        success: Font.successSchema.pipe(Schema.fieldsAssign({ installed: Schema.OptionFromOptionalKey(Font.successSchema.fields.installed.from.schema.mapFields(Struct.omit(['data'])).pipe(Schema.fieldsAssign({ metrics: Metrics }))) })),
    })
        .annotateMerge(READING)
        .annotateMerge(Font.annotations);
    const [open, close] = ['async function body() {\n', '\n}'];
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const directory = yield* path.fromFileUrl(new URL('./', import.meta.url));
    const references = yield* Effect.all(
        Record.map(
            {
                scripting: 'Choose and call InDesign tools; discover native members, pass references, interpret results, and preserve undo',
                grep: 'Write native GREP expressions and replacements with the intended matches, boundaries, and Unicode characters',
                styles: 'Reusable styles, persistent GREP rules, nested formatting, numbered lists, and generated document text',
                text: 'Compose and inspect text, paragraph rules, tabs, multilingual typography, variable fonts, notes, and conditions',
                layout: 'Pages, parents, layers, coordinate spaces, grids, text threads, and native layout automation',
                graphics: 'Shapes, color, effects, placed graphics, fitting, and text wrap',
                production: 'Prepare linked assets, merged content, books, preflight, accessible exports, and packages',
                interactions: 'Button behaviors, form fields, object states, navigation, media, and animation timing',
            },
            (description, name) => Effect.map(files.readFileString(path.join(directory, 'guidance', `${name}.md`)), (content) => ({ content, description, uri: `indesign://${name}/reference`, name: `${name}_reference`, mimeType: 'text/markdown' })),
        ),
        { concurrency: 'unbounded' },
    );
    const workflows = yield* Effect.all(
        Record.map(
            {
                publication: { description: 'Build or refine a document or presentation from its audience, content, design constraints, and delivery requirements', references: [references.layout, references.text] },
                formatting: { description: 'Build reusable styles and automatic formatting with GREP, lists, tables, and generated document text', references: [references.styles, references.grep] },
                delivery: { description: 'Prepare composed documents for their intended output, reader, accessibility, and source-file delivery', references: [references.production] },
            },
            (workflow, name) => Effect.map(files.readFileString(path.join(directory, 'workflows', `${name}.md`)), (content) => ({ ...workflow, content })),
        ),
        { concurrency: 'unbounded' },
    );
    const parameters = {
        task: Schema.String.annotate({ description: 'Requested outcome, source content, audience, design constraints, and intended output or reader' }),
        scope: Schema.optionalKey(Specifier.annotate({ description: 'Native target specifier for existing work; omit to discover the target or create requested new work' })),
    };
    return {
        peers: link.peers,
        publish: Effect.andThen(
            Effect.all([
                link.publish,
                ...Object.values(references).map(({ content, ...resource }) => McpServer.registerResource({ ...resource, content: Effect.succeed({ contents: [{ uri: resource.uri, mimeType: resource.mimeType, text: content }] }) })),
                ...Object.entries(workflows).map(([name, workflow]) =>
                    McpServer.registerPrompt({
                        name,
                        description: workflow.description,
                        parameters,
                        content: ({ task, scope }) =>
                            Effect.succeed([
                                McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.TextContent.make({ text: `${task}${scope === undefined ? '' : `\nTarget: ${scope}`}` }) }),
                                McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.TextContent.make({ text: workflow.content }) }),
                                ...workflow.references.map(({ uri, mimeType, content }) => McpSchema.PromptMessage.make({ role: 'user', content: McpSchema.EmbeddedResource.make({ resource: McpSchema.TextResourceContents.make({ uri, mimeType, text: content }) }) })),
                            ]),
                    }),
                ),
            ]),
            link.tool(
                face,
                Effect.fnUntraced(function* ({ name, ...options }, call) {
                    const found = yield* call(Font, { name, text: [options.text, ...Object.values(CHARACTERS)].join('') });
                    const installed = yield* Effect.transposeOption(
                        Option.map(
                            found.installed,
                            Effect.fnUntraced(function* ({ data, ...font }) {
                                const coordinates = Option.firstSomeOf([options.coordinates, font.subset ? Option.none() : font.coordinates]);
                                return { ...font, metrics: yield* read(data, font.postscriptName, { ...options, coordinates }) };
                            }),
                        ),
                    );
                    return { ...found, installed };
                }),
            ),
        ).pipe(Effect.provideService(Transpile, (code) => Effect.try({ try: () => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length), catch: (cause) => new SchemaIssue.InvalidValue({ message: Formatter.format(cause) }, code) }))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
