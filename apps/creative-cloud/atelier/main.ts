import { Automation, Capture, Execute, Installation, InstalledFace, Kind, Perform, Pid, Screenshot, State } from '@rasm/automation';
import { FontView } from '@rasm/indesign/fonts';
import { Render } from '@rasm/indesign/render';
import { Script } from '@rasm/indesign/scripts';
import { Hold, InDesign, PATH, WIRE } from '@rasm/indesign/service/contract';
import { Invoke, Menu } from '@rasm/indesign/session';
import { listen } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Attachment, Image, Instructions, PublishedResource, PublishedTool, publish, READING, resource, resourceTemplate, targeted } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Face, Instance, Run, Shaped } from '@rasm/typography/main';
import type { Source } from '@rasm/typography/source';
import { ORIGIN, Peer } from '@rasm/uxp/link';
import { Array, Config, Context, Effect, FileSystem, flow, identity, Layer, Match, Option, Path, Schema, SchemaAST, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const serves = (row: Installation) => (peer: typeof Peer.Type) => row.url.href === peer.application.href;
            const [automation, panel] = yield* Effect.all([Automation, listen(InDesign, { path: PATH, origin: ORIGIN, identity: Peer, serialization: WIRE, middleware: Hold.client, serves })]);
            const served = Effect.zipWith(automation.installed, panel.peers, (rows, peers) => rows.filter((row) => peers.some(serves(row))));
            const instance = Effect.fnUntraced(function* <E, R>(found: Effect.Effect<Installation, E, R>, pid: Option.Option<number>) {
                const [installation, running] = yield* Effect.all([found, SubscriptionRef.get(automation.instances)]);
                const pids = running.filter((row) => row.url.href === installation.url.href && !State.guards.exited(row.state)).map((row) => row.pid);
                return { installation, pid: yield* Unmatched.single(`running instance of ${installation.name}`, Option.map(pid, String))(pids, (each) => [String(each)]) };
            });
            const linked = (installation: Installation) => Effect.tap(panel.call(installation), () => instance(Effect.succeed(installation), Option.none()));
            const repeatable = Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false));
            const specimen = Schema.Struct({ face: Face, shaped: Shaped });
            const answering = (state: typeof State.Type) => State.guards.responding(state) && state.elements.some(Struct.get('requiresAnswer'));
            const reporting = { report: (state: typeof State.Type) => answering(state) || !(State.guards.launching(state) || State.guards.responding(state)) };
            const refusing = { ...reporting, refuse: answering };
            yield* resource({
                uri: 'adobe://applications',
                name: 'applications',
                description: 'Installed Adobe applications with any linked extension, tools taking a scriptable one by name',
                mimeType: 'application/json',
                schema: Schema.Array(Schema.Struct({ installation: Installation, extension: Schema.OptionFromOptionalKey(Peer.fields.extension) })),
                changes: Stream.merge(SubscriptionRef.changes(automation.installations), panel.changes),
                content: Effect.zipWith(automation.installed, panel.peers, (rows, peers) => rows.map((installation) => ({ installation, extension: Option.map(Array.findFirst(peers, serves(installation)), Struct.get('extension')) }))),
            });
            yield* resource({ uri: 'atelier://faces', name: 'faces', description: 'Faces CoreText finds, each with its file', mimeType: 'application/json', schema: Schema.Array(InstalledFace), changes: Stream.empty, content: automation.faces() });
            const status = yield* resourceTemplate`adobe://status/${McpSchema.param('pid', Pid)}`({
                name: 'status',
                description: SchemaAST.resolveDescription(State.ast),
                mimeType: 'application/json',
                schema: State,
                completion: { pid: () => Effect.map(SubscriptionRef.get(automation.instances), Array.map(Struct.get('pid'))) },
                changes: SubscriptionRef.changes(automation.instances),
                content: (_, pid) =>
                    Effect.flatMap(SubscriptionRef.get(automation.instances), (rows) =>
                        Effect.fromOption(
                            Option.map(
                                Array.findFirst(rows, (row) => row.pid === pid),
                                Struct.get('state'),
                            ),
                        ),
                    ),
            });
            const [named, kind] = [McpSchema.param('application', Schema.String), McpSchema.param('kind', Kind)];
            const scriptable = { application: () => Effect.map(automation.installed, (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('name'))) };
            yield* resourceTemplate`adobe://dictionary/${named}`({
                name: 'dictionary',
                description: SchemaAST.resolveDescription(Installation.Terms.ast),
                mimeType: 'application/json',
                schema: Installation.Terms,
                completion: scriptable,
                changes: SubscriptionRef.changes(automation.installations),
                content: (_, given) => Effect.flatMap(Installation.located(given), automation.terms),
            });
            yield* resourceTemplate`adobe://dictionary/${named}/${kind}/${McpSchema.param('name', Schema.String)}`({
                name: 'dictionary_term',
                description: 'Scripting definition XML of every declaration with the kind and name',
                mimeType: 'application/xml',
                schema: Schema.String,
                changes: SubscriptionRef.changes(automation.installations),
                completion: {
                    ...scriptable,
                    kind: () => Effect.succeed([...Kind.literals]),
                    name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                        const chosen = yield* Schema.decodeUnknownEffect(Schema.Struct({ application: named, kind }))(context?.arguments);
                        return Array.map((yield* automation.terms(yield* Installation.located(chosen.application)))[chosen.kind], Struct.get('name'));
                    }),
                },
                content: (_, given, selected, term) => Effect.flatMap(Installation.located(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
            });
            const lookup = Option.match({ onSome: Installation.located, onNone: () => Effect.flatMap(served, (rows) => Unmatched.single('application with a linked extension', Option.none())(rows, (row) => [row.name])) });
            const font = Tool.make('font', {
                description: 'Shapes text in a face opened by name, at the instance an instance PostScript name or variation settings select; with application, its linked extension answers with its own view of the font',
                parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText resolves it; a linked extension takes its native font name or specifier' }) })),
                success: Schema.Union([Schema.Struct({ ...InstalledFace.fields, specimen }), Schema.Struct({ ...FontView.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
            }).annotateMerge(READING);
            const typeset = Effect.fnUntraced(function* ({ name, variations, size, ...run }: Tool.Parameters<typeof font>, chosen: Option.Option<{ readonly installation: Installation }>) {
                const shaped = (source: typeof Source.Type) => Effect.map(Face.open(source, { variations, size }), (face) => ({ face, shaped: face.shape(run) }));
                return yield* Option.match(chosen, {
                    onNone: Effect.fnUntraced(function* () {
                        const [found, files, path] = yield* Effect.all([automation.face({ name }), FileSystem.FileSystem, Path.Path]);
                        return { ...found, specimen: yield* shaped({ data: yield* files.readFile(yield* path.fromFileUrl(found.file)), postscriptName: Option.some(found.postscriptName) }) };
                    }),
                    onSome: Effect.fnUntraced(function* ({ installation }) {
                        const { view, source } = yield* (yield* panel.call(installation))('font', { name, text: run.text });
                        return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, shaped)) };
                    }),
                });
            });
            const script = Tool.make('execute_script', {
                description: "Runs source in the application's linked extension as UXP JavaScript, TypeScript stripped of types, else in the runtime of its dictionary's script command",
                parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body with its return value as result' }) }),
                success: Installation.Script,
            }).annotateMerge(
                Context.make(
                    Instructions,
                    Array.map(Array.fromNullishOr(SchemaAST.resolveDescription(Script.ast)), (text) => `execute_script: ${text}`),
                ),
            );
            const scripted = ({ code }: Tool.Parameters<typeof script>, { installation, pid }: { readonly installation: Installation; readonly pid: number }) =>
                Effect.catchNoSuchElement(linked(installation)).pipe(
                    Effect.map(Option.map((call) => (source: string) => Effect.map(call('execute_script', { code: source }), Struct.assign({ runtime: 'UXP JavaScript' })))),
                    Effect.flatMap((use) => automation.script(pid, code, use)),
                );
            const command = Tool.make('command', {
                description: "Runs an application's dictionary command, Accessibility actions on its status elements, or a linked extension's menu action",
                parameters: Schema.Struct({ run: Schema.TaggedUnion({ Command: Execute.fields, Element: Perform.fields, Menu: { action: Invoke.fields.action } }) }),
                success: Schema.Union([Installation.Executed.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Menu.Action, Schema.Void]),
            });
            const commanded = ({ run }: Tool.Parameters<typeof command>, { installation, pid }: { readonly installation: Installation; readonly pid: number }) =>
                Match.valueTags(run, {
                    Command: (given) => automation.execute({ ...given, pid }),
                    Element: (perform) => automation.perform({ ...perform, pid }),
                    Menu: ({ action }) => Effect.flatMap(linked(installation), (use) => use('invoke', { action })),
                });
            const tools = Effect.fnUntraced(function* (names: readonly string[]) {
                const application = Schema.Literals(names).annotate({ description: 'Installed Adobe application' });
                const route = {
                    fields: { application: Schema.OptionFromOptionalKey(application) },
                    resolve: ({ application: given }: { readonly application: Option.Option<string> }) => instance(lookup(given), Option.none()),
                    depends: ({ pid }: { readonly pid: number }) => Effect.succeed([status(pid)(refusing)]),
                    annotations: Context.make(Tool.OpenWorld, false).pipe(Context.add(Instructions, ['Tools given no application act on the one application with a linked extension. No tool changes the frontmost application.'])),
                    completion: { application: () => Effect.map(served, Array.map(Struct.get('name'))) },
                };
                const extended = yield* publish(
                    InDesign.omit('enter'),
                    {
                        inspect: new PublishedTool({ annotations: READING }),
                        change: new PublishedTool({ annotations: Context.empty() }),
                        create: new PublishedTool({ annotations: Context.empty() }),
                        remove: new PublishedTool({ annotations: Context.empty() }),
                        find_change: new PublishedTool({ annotations: Context.empty() }),
                        render: new PublishedTool({ annotations: READING, success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) }),
                        menu: new PublishedResource({ uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } }),
                        preflight: new PublishedResource({
                            uri: 'indesign://preflight',
                            complete: { document: Array.map((process) => process.targetObject.specifier), profile: (processes) => Array.dedupe(Array.getSomes(Array.map(processes, (process) => Option.flatMap(process.appliedProfile, Struct.get('name'))))) },
                            report: Array.some((process) => Array.isReadonlyArrayNonEmpty(process.issues)),
                        }),
                        queries: new PublishedResource({ uri: 'indesign://queries', complete: { name: Array.map(Struct.get('name')) } }),
                        kind: new PublishedResource({ uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } }),
                    },
                    {
                        route,
                        invoke: (rpc) =>
                            Effect.fnUntraced(function* (payload, { installation }) {
                                return yield* (yield* panel.call(installation))(rpc._tag, payload);
                            }),
                        annotations: Context.make(Instructions, ['InDesign tools run through its panel, linked when InDesign starts.']),
                    },
                );
                const watched =
                    (acts: typeof reporting) =>
                    ({ pid }: { readonly pid: number }, given: { readonly application: Option.Option<string> }) =>
                        Effect.succeed([status(pid)(acts), ...extended(given)]);
                const addressed = {
                    ...route,
                    fields: { ...route.fields, pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Running instance, required when its bundle runs several; a linked extension serves a bundle running one' })) },
                    resolve: ({ application: given, pid }: { readonly application: Option.Option<string>; readonly pid: Option.Option<number> }) => instance(lookup(given), pid),
                    depends: watched(refusing),
                };
                yield* Effect.all(
                    [
                        targeted({ ...route, fields: { application }, resolve: ({ application: given }) => Effect.flatMap(Installation.located(given), ({ url }) => automation.launch({ url })), depends: ({ pid }) => Effect.succeed([status(pid)(reporting)]) })(
                            Tool.make('launch', {
                                description: "Launches the application's own bundle in the background, never another running version, and returns its process once launched or already running",
                                parameters: Schema.Struct({}),
                                success: Schema.Struct({ pid: Pid }),
                            }).annotateMerge(repeatable),
                            (_, { pid }) => Effect.succeed({ pid }),
                        ),
                        targeted(addressed)(
                            Tool.make('screenshot', {
                                description: "Captures the application's on-screen windows as one PNG without raising them",
                                parameters: Capture,
                                success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                            }).annotateMerge(READING),
                            (capture, { pid }) => automation.screenshot({ ...capture, pid }),
                        ),
                        targeted({
                            ...route,
                            resolve: ({ application: given }) => Effect.transposeOption(Option.map(given, (name) => instance(Installation.located(name), Option.none()))),
                            depends: (found, given) => Option.match(found, { onNone: () => Effect.succeed([]), onSome: (chosen) => watched(refusing)(chosen, given) }),
                        })(font, typeset),
                        targeted(addressed)(script, scripted),
                        targeted({ ...addressed, depends: watched(reporting) })(command, commanded),
                    ],
                    { discard: true },
                );
            });
            const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installations).pipe(Stream.filterMap(identity), Stream.map(flow(Array.filter(Struct.get('scriptable')), Array.map(Struct.get('name')), Array.dedupe)), Stream.changes), Sink.take(1));
            yield* Effect.forEach(first, tools, { discard: true });
            yield* Effect.forkScoped(Stream.runForEach(rest, tools));
        }),
    ).pipe(Layer.provide(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.spawned), Layer.unwrap))),
);
