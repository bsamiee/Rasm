import { Automation, AvailableFace, Capture, Execute, FaceName, Installation, Kind, Perform, Pid, Screenshot, State, strip } from '@rasm/automation';
import { InDesign } from '@rasm/indesign/contract';
import { FontView } from '@rasm/indesign/fonts';
import { Render } from '@rasm/indesign/render';
import { Script } from '@rasm/indesign/scripts';
import { main } from '@rasm/mcp/main';
import { Attachment, Image, Instructions, PublishedResource, PublishedTool, publish, READING, resource, resourceTemplate, targeted } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Face, Instance, Run, Shaped } from '@rasm/typography/main';
import type { Source } from '@rasm/typography/source';
import { Array, Config, Context, Effect, FileSystem, Function, flow, identity, Layer, Match, Option, Path, Predicate, Schema, SchemaAST, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { RpcClient } from 'effect/rpc';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = Effect.fnUntraced(function* (executable: string) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const entry = yield* path.toFileUrl(path.join(path.dirname(path.dirname(executable)), 'Resources', 'main.idjs'));
    const [running, , exited] = State.members;
    const [launching, responding] = running.fields.running.fields.status.members;
    const runs = (installation: Installation) => (row: { readonly url: URL; readonly state: typeof State.Type }) => row.url.href === installation.url.href && !Schema.is(exited)(row.state);
    const named = (row: Installation) => [row.name];
    const extended = Effect.map(automation.applications({ toOpen: entry }), (urls) => (row: Installation) => urls.some((url) => url.href === row.url.href));
    const indesign = (pid: number) => RpcClient.make(InDesign, { flatten: true }).pipe(Effect.provide(Automation.protocol(pid, (message) => ({ command: 'do script', arguments: { direct: { _tag: 'file', url: entry.href }, language: 'uxpscript', 'with arguments': [message] } }))));
    const instance = Effect.fnUntraced(function* <E, R>(found: Effect.Effect<Installation, E, R>, pid: Option.Option<number>) {
        const [installation, rows] = yield* Effect.all([found, SubscriptionRef.get(automation.instances)]);
        return { installation, pid: yield* Unmatched.single(`running instance of ${installation.name}`, Option.map(pid, String))(rows.filter(runs(installation)).map(Struct.get('pid')), (each) => [String(each)]) };
    });
    const among = (hosts: Predicate.Predicate<Installation>) => (given: Option.Option<string>) =>
        Effect.zipWith(automation.installed, SubscriptionRef.get(automation.instances), (installed, rows) => installed.filter((row) => hosts(row) && (Option.isSome(given) || rows.some((instance) => runs(row)(instance) && instance.activationPolicy === 'regular'))));
    const lookup = (hosts: Predicate.Predicate<Installation>) => (given: Option.Option<string>) => Effect.flatMap(among(hosts)(given), (rows) => Unmatched.single(Option.match(given, { onSome: () => 'installed Adobe application', onNone: () => 'running Adobe application in the Dock' }), given)(rows, named));
    const dictionaries = Effect.zipWith(automation.installed, extended, (rows, hosts) => rows.filter((row) => row.scriptable && !hosts(row)));
    const described = (given: string) => Effect.flatMap(dictionaries, (rows) => Unmatched.single('installed Adobe application its dictionary describes', Option.some(given))(rows, named));
    const repeatable = Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false));
    const specimen = Schema.Struct({ face: Face, shaped: Shaped });
    const answering = (state: typeof State.Type) => Schema.is(running)(state) && Schema.is(responding)(state.running.status) && state.running.status.responding.elements.some(Struct.get('requiresAnswer'));
    const reporting = { report: (state: typeof State.Type) => answering(state) || !(Schema.is(running)(state) && (Schema.is(launching)(state.running.status) || Schema.is(responding)(state.running.status))) };
    const refusing = { ...reporting, refuse: answering };
    yield* resource({
        uri: 'adobe://applications',
        name: 'applications',
        description: 'Installed Adobe applications; each name is the application other tools take',
        mimeType: 'application/json',
        schema: Schema.Array(Installation),
        changes: SubscriptionRef.changes(automation.installations),
        content: automation.installed,
    });
    yield* resource({ uri: 'atelier://faces', name: 'faces', description: 'Faces available to CoreText, each with the file holding it', mimeType: 'application/json', schema: Schema.Array(AvailableFace), changes: Stream.empty, content: automation.faces() });
    const status = yield* resourceTemplate`adobe://status/${McpSchema.param('pid', Pid)}`({
        name: 'status',
        description: SchemaAST.resolveDescription(State.ast),
        mimeType: 'application/json',
        schema: State,
        completion: { pid: () => Effect.map(SubscriptionRef.get(automation.instances), Array.map(Struct.get('pid'))) },
        changes: SubscriptionRef.changes(automation.instances),
        content: (_, pid) =>
            SubscriptionRef.get(automation.instances).pipe(
                Effect.flatMap((rows) => Effect.fromOption(Array.findFirst(rows, (row) => row.pid === pid))),
                Effect.flatMap(({ state }) => (Schema.is(exited)(state) ? Effect.succeed(state) : automation.status({ pid }))),
            ),
    });
    const [application, kind] = [McpSchema.param('application', Schema.String), McpSchema.param('kind', Kind)];
    const scriptable = { application: () => Effect.map(dictionaries, Array.map(Struct.get('name'))) };
    yield* resourceTemplate`adobe://dictionary/${application}`({
        name: 'dictionary',
        description: SchemaAST.resolveDescription(Installation.Terms.ast),
        mimeType: 'application/json',
        schema: Installation.Terms,
        completion: scriptable,
        changes: SubscriptionRef.changes(automation.installations),
        content: (_, given) => Effect.flatMap(described(given), automation.terms),
    });
    yield* resourceTemplate`adobe://dictionary/${application}/${kind}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML of every declaration with the kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(automation.installations),
        completion: {
            ...scriptable,
            kind: () => Effect.succeed([...Kind.literals]),
            name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                const chosen = yield* Schema.decodeUnknownEffect(Schema.Struct({ application, kind }))(context?.arguments);
                return Array.map((yield* automation.terms(yield* described(chosen.application)))[chosen.kind], Struct.get('name'));
            }),
        },
        content: (_, given, selected, term) => Effect.flatMap(described(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
    });
    const font = Tool.make('font', {
        description: 'Shapes text in a face opened by name, at the instance an instance PostScript name or variation settings select; with application, InDesign answers with its own view of the font',
        parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.String.annotate({ description: `${SchemaAST.resolveDescription(FaceName.ast)}; InDesign takes its native font name or specifier` }) })),
        success: Schema.Union([Schema.Struct({ ...AvailableFace.fields, specimen }), Schema.Struct({ ...FontView.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
    }).annotateMerge(READING);
    const typeset = Effect.fnUntraced(function* ({ name, variations, size, ...run }: Tool.Parameters<typeof font>, chosen: Option.Option<{ readonly pid: number }>) {
        const shaped = (source: typeof Source.Type) => Effect.map(Face.open(source, { variations, size }), (face) => ({ face, shaped: face.shape(run) }));
        const native = Effect.fnUntraced(function* ({ pid }: { readonly pid: number }) {
            const call = yield* indesign(pid);
            const { view, source } = yield* call('font', { name, text: run.text });
            return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, shaped)) };
        }, Effect.scoped);
        const shaping = Effect.gen(function* () {
            const [found, files] = yield* Effect.all([automation.face({ name }), FileSystem.FileSystem]);
            return { ...found, specimen: yield* shaped({ data: yield* files.readFile(yield* path.fromFileUrl(found.file)), postscriptName: Option.some(found.postscriptName) }) };
        });
        return yield* Option.match(chosen, { onNone: () => shaping, onSome: native });
    });
    const script = Tool.make('execute_script', {
        description: "Runs source in InDesign as UXP JavaScript, TypeScript stripped of types, and in any other application in the runtime of its dictionary's script command, whose errors arrive as the host raises them; ExtendScript alert, confirm, and prompt activate their application before showing",
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body with its return value as result' }) }),
        success: Installation.Script,
    }).annotateMerge(
        Context.make(
            Instructions,
            Array.map(Array.fromNullishOr(SchemaAST.resolveDescription(Script.ast)), (text) => `execute_script: ${text}`),
        ),
    );
    const Form = Schema.TaggedUnion({ Command: Execute.fields, Element: Perform.fields });
    const command = Tool.make('command', {
        description: "Runs a dictionary command of an application no extension serves, or Accessibility actions on an application's status elements; Element actions answer a dialog requiring an answer, Command is refused while one shows, and answering a dialog an application shows while launching can activate it",
        parameters: Schema.Struct({}),
        success: Schema.Union([Installation.Executed.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Schema.Void]),
    });
    const commanded = (_: Tool.Parameters<typeof command>, { pid, run }: { readonly pid: number; readonly run: typeof Form.Type }) =>
        Match.valueTags(run, {
            Command: (given) => automation.execute({ ...given, pid }),
            Element: (perform) => automation.perform({ ...perform, pid }),
        });
    const tools = Effect.fnUntraced(function* (names: readonly string[]) {
        const [installed, hosts] = yield* Effect.all([automation.installed, extended]);
        const listed = (holds: Predicate.Predicate<Installation>) => Schema.Literals(Array.intersection(names, installed.filter(holds).map(Struct.get('name')))).annotate({ description: 'Installed Adobe application' });
        const route = (holds: Predicate.Predicate<Installation>) => ({
            fields: {
                application: Schema.OptionFromOptionalKey(listed(holds).annotate({ description: 'Installed Adobe application, absent for the one running in the Dock' })),
                pid: Schema.OptionFromOptionalKey(Pid.annotate({ description: 'Running instance, required when its bundle runs several' })),
            },
            resolve: ({ application: given, pid }: { readonly application: Option.Option<string>; readonly pid: Option.Option<number> }) => instance(lookup(holds)(given), pid),
            depends: ({ pid }: { readonly pid: number }) => Effect.succeed([status(pid)(refusing)]),
            annotations: Context.make(Tool.OpenWorld, false).pipe(Context.add(Instructions, ['A call that changes state returns once its application shows a dialog requiring an answer. No tool changes the frontmost application.'])),
            completion: { application: () => Effect.map(among(holds)(Option.none()), Array.map(Struct.get('name'))) },
        });
        const [any, designing] = [route(Function.constTrue), route(hosts)];
        const forms = { Command: Predicate.not(hosts), Element: Function.constTrue } satisfies Record<(typeof Form.Type)['_tag'], Predicate.Predicate<Installation>>;
        const scripted = ({ code }: Tool.Parameters<typeof script>, { installation, pid }: { readonly installation: Installation; readonly pid: number }) =>
            hosts(installation) ? Effect.scoped(Effect.flatMap(Effect.all([strip(code), indesign(pid)]), ([source, call]) => call('execute_script', { code: source }))) : automation.script(pid, code);
        yield* publish(
            InDesign,
            {
                inspect: new PublishedTool({ annotations: READING }),
                change: new PublishedTool({ annotations: Context.empty() }),
                create: new PublishedTool({ annotations: Context.empty() }),
                remove: new PublishedTool({ annotations: Context.empty() }),
                find_change: new PublishedTool({ annotations: Context.empty() }),
                invoke: new PublishedTool({ annotations: Context.empty() }),
                render: new PublishedTool({ annotations: READING, success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) }),
                menu: new PublishedResource({ uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } }),
                preflight: new PublishedResource({
                    uri: 'indesign://preflight',
                    complete: { document: Array.map((process) => process.targetObject.specifier) },
                }),
                kind: new PublishedResource({ uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } }),
            },
            {
                route: designing,
                invoke: (rpc) =>
                    Effect.fnUntraced(function* (payload, { pid }) {
                        const call = yield* indesign(pid);
                        return yield* call(rpc._tag, payload);
                    }, Effect.scoped),
                annotations: Context.make(Instructions, ['InDesign tools run as UXP scripts through its do script command.']),
            },
        );
        yield* Effect.all(
            [
                targeted({ ...any, fields: { application: listed(Function.constTrue) }, resolve: ({ application: given }) => Effect.flatMap(Installation.located(given), ({ url }) => automation.launch({ url })), depends: ({ pid }) => Effect.succeed([status(pid)(reporting)]) })(
                    Tool.make('launch', {
                        description: "Launches the application's own bundle in the background, never another running version, and returns its process once launched or already running",
                        parameters: Schema.Struct({}),
                        success: Schema.Struct({ pid: Pid }),
                    }).annotateMerge(repeatable),
                    (_, { pid }) => Effect.succeed({ pid }),
                ),
                targeted(any)(
                    Tool.make('screenshot', {
                        description: "Captures the application's on-screen windows as one PNG without raising them",
                        parameters: Capture,
                        success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                    }).annotateMerge(READING),
                    (capture, { pid }) => automation.screenshot({ ...capture, pid }),
                ),
                targeted({
                    ...designing,
                    fields: { application: Schema.OptionFromOptionalKey(listed(hosts).annotate({ description: 'Installed InDesign that answers, absent for CoreText' })) },
                    resolve: ({ application: given }) => Effect.transposeOption(Option.map(given, (name) => instance(lookup(hosts)(Option.some(name)), Option.none()))),
                    depends: Option.match({ onNone: () => Effect.succeed([]), onSome: designing.depends }),
                })(font, typeset),
                targeted(any)(script, scripted),
                targeted({
                    ...any,
                    fields: { ...any.fields, run: Form },
                    resolve: ({ run, application: given, pid }) => Effect.map(instance(lookup(forms[run._tag])(given), pid), (chosen) => ({ ...chosen, run })),
                    depends: ({ pid, run }) => Effect.succeed([status(pid)(run._tag === 'Element' ? reporting : refusing)]),
                })(command, commanded),
            ],
            { discard: true },
        );
    });
    const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installations).pipe(Stream.filterMap(identity), Stream.map(flow(Array.map(Struct.get('name')), Array.dedupe)), Stream.changes), Sink.take(1));
    yield* Effect.forEach(first, tools, { discard: true });
    yield* Effect.forkScoped(Stream.runForEach(rest, tools));
});

main(manifest, Layer.unwrap(Effect.map(Config.String('ATELIER_PATH'), (executable) => Layer.effectDiscard(served(executable)).pipe(Layer.provide(Automation.spawned(executable))))));
