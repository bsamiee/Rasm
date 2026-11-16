import { Launched, ProcessInstance, type RunningInstance, State } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import runtime from '@rasm/atelier-extendscript?inline';
import { Completion, ExecuteScript, Render } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { HostError, ObjectModel, Reference } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';
import { lower, traced } from '@rasm/cep/node';
import { Resource, Route, registerRpcGroup, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Context, Duration, Effect, flow, Number, Option, Path, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { AbsolutePath } from '#/absolute-path';
import { Adobe } from '#/adobe';
import { Capture } from '#/features/applications';
import { Libraries, library, type listening } from '#/libraries';
import type { routes } from '#/routes';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINK_WAIT = Duration.minutes(2);

// --- [MODELS] --------------------------------------------------------------------------

const Scripted = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(ExecuteScript.Result);
const Body = Schema.Struct({
    code: ExecuteScript.fields.code.annotate({ description: 'Function body, return gives result. Scope holds host classes, enumerations, and resolve(specifier), object_model lists members' }),
    requirePlugin: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Require a linked plugin and refuse when disconnected' })),
});
const Opening = Schema.Struct({ file: AbsolutePath.annotate({ description: 'Document path' }) });
const Rendering = Schema.Struct(Struct.omit(Render.fields, ['file']));
const Opened = Reference.mapFields(Struct.assign({ launched: Schema.OptionFromOptionalKey(Launched.annotate({ description: 'Instance open launched' })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const lowered = Effect.fnUntraced(function* (code: string, target: Parameters<typeof lower>[1], awaits: boolean) {
    const { code: source, map } = yield* lower(ExecuteScript.expression(code, awaits), target, 'typescript').pipe(Effect.mapError((error) => (HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.map(error.reason.line, Number.decrement))(error.cause) : error)));
    const original = yield* traced(map);
    const relined = (error: HostError) =>
        Effect.fail(
            HostError.fields.reason.isAnyOf(['Thrown'])(error.reason)
                ? HostError.thrown(
                      error.subject,
                      Option.map(Option.flatMap(error.reason.line, original), ({ line }) => Number.decrement(line)),
                  )(error.cause)
                : error,
        );
    return { source, relined };
});
const opener = Effect.fnUntraced(function* (links: Effect.Success<typeof listening>, serves: ReturnType<Effect.Success<ReturnType<typeof routes>>['serving']>['serves'], file: URL) {
    const [adobe, automation] = yield* Effect.all([Adobe, Automation]);
    const [rows, peers] = yield* Effect.all([automation.opening(file, {}), links.peers]);
    const candidates = rows.filter(
        flow(
            library,
            Option.exists((served) => serves({ library: served })),
        ),
    );
    const first = yield* Effect.fromOption(Array.head(candidates), () => Unmatched.make({ subject: 'linked application opening file', query: Option.some(file.href), candidates: Array.map(rows, Struct.get('name')) }));
    const [installation, launch] = yield* Option.match(
        Array.findFirst(candidates, (row) => peers.some(Peer.serves(row.url))),
        {
            onSome: (linked) => Effect.succeed([linked, Option.none()] as const),
            onNone: () =>
                adobe.launched({ url: first.url }).pipe(
                    Effect.tap(() => Stream.runHead(Stream.filter(links.changes, Array.some(Peer.serves(first.url))))),
                    Effect.timeoutOrElse({ duration: LINK_WAIT, orElse: () => Effect.fail(HostError.refused(`link from ${first.name} within ${Duration.format(LINK_WAIT)}`)()) }),
                    Effect.map((instance) => [first, Option.some(instance)] as const),
                ),
        },
    );
    return { client: yield* links.call(installation.url), launched: launch };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const plugins = Effect.fnUntraced(function* ({ serving, script }: Effect.Success<ReturnType<typeof routes>>, links: Effect.Success<typeof listening>) {
    const [automation, path, adobe] = yield* Effect.all([Automation, Path.Path, Adobe]);
    yield* Resource.template`plugin/${McpSchema.param('instance', ProcessInstance)}`({
        name: 'plugin',
        schema: Peer,
        changes: links.changes,
        content: Effect.fnUntraced(function* (_, instance) {
            const [instances, peers] = yield* Effect.all([SubscriptionRef.get(adobe.instances), links.peers]);
            const running = yield* Effect.fromNullishOr(instances.find((row) => row.instance === instance && !State.isAnyOf(['exited'])(row.state)));
            return yield* Unmatched.single(peers, 'linked plugin for the application', Option.some(running.url.href), (peer) => [peer.application.href]);
        }),
    });
    const [opened, rendered] = [serving('open'), serving('render')];
    const rendering = (client: Effect.Success<ReturnType<typeof links.call>>) => (request: typeof Rendering.Type & { readonly file: URL }) => client(rendered.tag, request);
    const scripted = Effect.fnUntraced(function* (code: string, { instance, installation: { url } }: Pick<RunningInstance, 'instance'> & { readonly installation: { readonly url: URL } }) {
        const { source, relined } = yield* lowered(code, ExtensionManifest.target, false);
        const { reply } = yield* automation.script({
            instance,
            url,
            events: ['DjxM', 'dosc'],
            declared: { doLg: { _tag: 'enumerator', name: 'JSLg' } },
            direct: `(function (self) { var body = ${source}\n${runtime}\nreturn encodeURIComponent(runtime.reply(function () { return { result: body.call(self) }; }));\n})(this)`,
        });
        return yield* Schema.decodeUnknownEffect(Scripted)(reply).pipe(Effect.flatMap(Completion.answered(ResultJson, ExecuteScript.make({ code: source }).label)), Effect.catchTag('HostError', relined));
    });
    const linkedScript = Effect.fnUntraced(function* (code: string, { library: { target, Contract }, client }: Option.Option.Value<Effect.Success<ReturnType<typeof script.resolve>>['linked']>) {
        const { source, relined } = yield* lowered(code, target, Context.get(Contract.annotations, ExecuteScript.Awaits));
        return yield* Effect.catchTag(client(script.tag, { code: source }), 'HostError', relined);
    });
    yield* Route(script)(
        Tool.make(script.tag, {
            description: 'Runs TypeScript function body in application, returns its result as JSON, failures name body line',
            parameters: Body,
            success: ExecuteScript.Result,
        }).annotate(Server.Instructions, [
            `${script.tag} runs body in linked InDesign synchronously in one undo step that a throw rolls back`,
            `${script.tag} runs awaited work of a linked InDesign body after the answer`,
            `${script.tag} awaits body in linked Photoshop in one history state per open document`,
            `${script.tag} runs body in linked Illustrator synchronously with alerts off in one undo step`,
            `${script.tag} runs body without a link through the dictionary script command as ExtendScript or Acrobat JavaScript`,
            `${script.tag} results encode InDesign enumerators by name and InDesign collections as {type, specifier, length}`,
            `${script.tag} results encode InDesign, Photoshop, and Illustrator objects as {type, specifier, name, valid}`,
        ]),
        ({ code, requirePlugin }, chosen) => {
            const withoutPlugin = requirePlugin === true ? Effect.fail(HostError.refused(chosen.installation.name)('Atelier plugin is not connected')) : scripted(code, chosen);
            return Option.match(chosen.linked, {
                onNone: () => withoutPlugin,
                onSome: (linked) => linkedScript(code, linked),
            });
        },
    );
    yield* Route(opened.optional('linked application Launch Services picks for file'))(
        Tool.make(opened.tag, {
            description: 'Opens the document without dialogs and returns its reference, with no window where the application allows it. Starts the application Launch Services picks when none runs and waits for its link. In After Effects the document replaces the open project',
            parameters: Opening,
            success: Opened,
        }),
        Effect.fnUntraced(function* ({ file }, chosen) {
            const url = yield* path.toFileUrl(file);
            const opening = yield* Option.match(chosen, { onSome: ({ client }) => Effect.succeed({ client, launched: Option.none() }), onNone: () => opener(links, opened.serves, url) });
            return { ...(yield* opening.client(opened.tag, { file: url })), launched: opening.launched };
        }),
    );
    yield* Route(rendered)(
        Tool.make(rendered.tag, {
            description: 'Renders document target as PNG, an InDesign page item or Photoshop layer without the rest of its page, spread, or document. Use screenshot for panels, dialogs, and pasteboard',
            parameters: Rendering,
            success: Capture,
        }).annotate(Tool.Readonly, true),
        (payload, { client }) => Capture.written(payload, rendering(client)),
    );
    yield* registerRpcGroup(Libraries, {
        tools: {
            inspect: { description: 'Reads object fields with declarations and typed view, or collection members, / for application. File fields read as paths', annotations: Context.make(Tool.Readonly, true) },
            change: { description: 'Writes object fields in one undo step, failures name every refused field by path' },
            call: { description: 'Calls object or collection method in one undo step, object_model lists parameters' },
            find_change: { description: 'Finds or changes matches in one undo step and restores search preferences, values take change forms' },
            invoke: { description: 'Invokes menu action in one undo step, menu lists actions by id, name, and area' },
            adjust: { description: 'Adds kerning, tracking, baseline shift or mark offsets to target text, / for text defaults' },
            kerns: { description: 'Writes pair kerns and baseline shifts over character runs, restoring metrics kerning over a given run first' },
        },
        resources: {
            object_model: {
                description: 'Class and enumeration names of the linked application, or one declaration with its members',
                completion: { name: ObjectModel.Result.match({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) },
            },
            menu: { description: 'Menu actions of one area or every area', completion: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
            preflight: {
                description: 'Preflight rows per document, or the finished live results when no profile is given. Each read waits for the preflight process, which restarts with every live check',
                completion: { document: Array.map((process) => process.targetObject.specifier) },
                report: Array.some(({ aggregatedResults }) => Array.isReadonlyArrayNonEmpty(aggregatedResults)),
            },
        },
        route: serving,
        invoke:
            (rpc) =>
            (payload, { client }) =>
                client(rpc._tag, payload),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { plugins };
