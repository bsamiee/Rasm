import { SourceMap } from 'node:module';
import { AfterEffects } from '@rasm/aftereffects/contract';
import { type Installation, Launched } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { ExecuteScript, HostError, ObjectModel, Reference, Render } from '@rasm/automation/requests';
import { answered, Reply } from '@rasm/cep/contract';
import { ExtensionManifest } from '@rasm/cep/manifest';
import { runtime } from '@rasm/extendscript/runtime';
import { Illustrator } from '@rasm/illustrator/contract';
import { InDesign } from '@rasm/indesign/contract';
import { Route } from '@rasm/mcp/route';
import { registerRpcGroup } from '@rasm/mcp/rpc';
import { Server } from '@rasm/mcp/server';
import { Photoshop } from '@rasm/photoshop/contract';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { type JscTarget, transformSync } from '@swc/core';
import { Array, Context, Duration, Effect, flow, Layer, Option, Path, Schema, Stream, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { AbsolutePath } from '#/absolute-path';
import { Capture } from '#/features/applications';
import { link } from '#/link';
import type { routes } from '#/routes';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINK_WAIT = Duration.minutes(2);

// --- [MODELS] --------------------------------------------------------------------------

const Libraries = InDesign.Contract.merge(Photoshop.Contract);
const Mapping = Schema.fromJsonString(
    Schema.Struct({
        version: Schema.Int,
        sources: Schema.mutable(Schema.Array(Schema.String)),
        sourcesContent: Schema.mutable(Schema.Array(Schema.String)),
        names: Schema.mutable(Schema.Array(Schema.String)),
        mappings: Schema.String,
        file: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
        sourceRoot: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    }),
);
const Entry = Schema.Struct({ originalLine: Schema.Int });
const Diagnostic = Schema.Struct({ message: Schema.TemplateLiteralParser(['  x ', Schema.String, '\n   ,-[', Schema.Int, ':', Schema.Int, ']', Schema.String]) });
const Scripted = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Reply)));
const Result = Schema.toCodecJson(ExecuteScript.Result);
const Opened = Reference.mapFields(Struct.assign({ launched: Schema.OptionFromOptionalKey(Launched.annotate({ description: 'Instance open launched' })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const lowered = Effect.fnUntraced(function* (code: string, target: JscTarget, awaits: boolean) {
    const { code: source, map } = yield* Effect.try({
        try: () => transformSync(ExecuteScript.expression(code, awaits), { jsc: { target, parser: { syntax: 'typescript' }, assumptions: { iterableIsArray: true } }, sourceMaps: true }),
        catch: (cause) =>
            Option.match(Schema.decodeUnknownOption(Diagnostic)(cause), {
                onNone: () => HostError.refused('TypeScript transform of body')(cause),
                onSome: ({ message: [, text, , line] }) => HostError.thrown(text, Option.some(line - 1))(cause),
            }),
    });
    const mapping = new SourceMap(yield* Effect.orDie(Schema.decodeUnknownEffect(Mapping)(map)));
    const original = (line: number) => Option.map(Schema.decodeUnknownOption(Entry)(mapping.findEntry(line - 1, Number.MAX_SAFE_INTEGER)), Struct.get('originalLine'));
    const relined = (error: HostError) => Effect.fail(HostError.fields.reason.guards.Thrown(error.reason) ? HostError.thrown(error.subject, Option.flatMap(error.reason.line, original))(error.cause) : error);
    return { source, relined };
});
const library = (installation: Installation) => Array.findFirst([InDesign, Photoshop, Illustrator, AfterEffects] as const, ({ identifier }) => installation.identifier.startsWith(identifier));

// --- [COMPOSITION] ---------------------------------------------------------------------

const listening = Link.listen(Libraries, { path: link.path, middleware: Layer.empty });
const plugins = Effect.fnUntraced(function* ({ serving, script }: Effect.Success<ReturnType<typeof routes>>, links: Effect.Success<typeof listening>) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const [opened, rendered] = [serving('open'), serving('render')];
    const launched = (installation: Installation) =>
        automation.launch({ url: installation.url }).pipe(
            Effect.tap(() => Stream.runHead(Stream.filter(links.changes, Array.some(Link.serves(installation.url))))),
            Effect.timeoutOrElse({ duration: LINK_WAIT, orElse: () => Effect.fail(HostError.refused(`link from ${installation.name} within ${Duration.format(LINK_WAIT)}`)()) }),
        );
    const opener = Effect.fnUntraced(function* (file: URL) {
        const [rows, peers] = yield* Effect.all([automation.opening(file), links.peers]);
        const candidates = rows.filter(
            flow(
                library,
                Option.exists(({ Contract }) => Contract.requests.has(opened.tag)),
            ),
        );
        const first = yield* Effect.fromOption(Array.head(candidates), () => Unmatched.make({ subject: 'linked application opening file', query: Option.some(file.href), candidates: Array.map(rows, Struct.get('name')) }));
        const [installation, launch] = yield* Option.match(
            Array.findFirst(candidates, (row) => peers.some(Link.serves(row.url))),
            {
                onSome: (linked) => Effect.succeed([linked, Option.none()] as const),
                onNone: () => Effect.map(launched(first), (instance) => [first, Option.some(instance)] as const),
            },
        );
        return { client: yield* links.call(installation.url), launched: launch };
    });
    const replying = yield* Effect.sync(() => transformSync(`var runtime = ${String(runtime)};`, { jsc: { target: ExtensionManifest.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code);
    const scripted = Effect.fnUntraced(function* (code: string, { pid }: { readonly pid: number }) {
        const { source, relined } = yield* lowered(code, ExtensionManifest.target, false);
        const { reply } = yield* automation.script({
            pid,
            direct: `(function (self) { var body = ${source}\n${replying}\nreturn encodeURIComponent(runtime().reply(function () { return { result: body.call(self) }; }));\n})(this)`,
        });
        return yield* Schema.decodeUnknownEffect(Scripted)(reply).pipe(Effect.flatMap(answered(Result, ExecuteScript.make({ code: source }).label)), Effect.catchTag('HostError', relined));
    });
    yield* Route.register(script)(
        Tool.make(script.tag, {
            description: 'Runs TypeScript function body in application, returns its result as JSON, failures name body line',
            parameters: Schema.Struct({ code: ExecuteScript.fields.code.annotate({ description: 'Function body, return gives result. Scope holds host classes, enumerations, and resolve(specifier), object_model lists members' }) }),
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
        ({ code }, chosen) =>
            Option.match(chosen.linked, {
                onNone: () => scripted(code, chosen),
                onSome: Effect.fnUntraced(function* ({ library: { target, Contract }, client }) {
                    const { source, relined } = yield* lowered(code, target, Context.get(Contract.annotations, ExecuteScript.Awaits));
                    return yield* Effect.catchTag(client(script.tag, { code: source }), 'HostError', relined);
                }),
            }),
    );
    yield* Route.register(opened.optional('linked application Launch Services picks for file'))(
        Tool.make(opened.tag, {
            description: 'Opens document without dialogs and returns its reference, without window where application allows, launching the picked application when none runs and waiting for its link. After Effects replaces its open project',
            parameters: Schema.Struct({ file: AbsolutePath.annotate({ description: 'Document path' }) }),
            success: Opened,
        }),
        Effect.fnUntraced(function* ({ file }, chosen) {
            const url = yield* path.toFileUrl(file);
            const opening = yield* Option.match(chosen, { onSome: ({ client }) => Effect.succeed({ client, launched: Option.none() }), onNone: () => opener(url) });
            return { ...(yield* opening.client(opened.tag, { file: url })), launched: opening.launched };
        }),
    );
    yield* Route.register(rendered)(
        Tool.make(rendered.tag, {
            description: 'Renders document target as PNG, an InDesign page item or Photoshop layer without the rest of its page, spread, or document. Use screenshot for panels, dialogs, and pasteboard',
            parameters: Schema.Struct(Struct.omit(Render.fields, ['file'])),
            success: Capture,
        }).annotate(Tool.Readonly, true),
        (payload, { client }) =>
            Effect.map(
                Capture.written((file) => client(rendered.tag, { ...payload, file })),
                Tuple.get(0),
            ),
    );
    yield* registerRpcGroup(Libraries, {
        tools: {
            inspect: { description: 'Reads object fields with declarations and typed view, or collection members, / for application. File fields read as paths', annotations: Context.make(Tool.Readonly, true) },
            change: { description: 'Writes object fields in one undo step, failures name every refused field by path' },
            call: { description: 'Calls object or collection method in one undo step, object_model lists parameters' },
            find_change: { description: 'Finds or changes matches in one undo step and restores search preferences, values take change forms' },
            invoke: { description: 'Invokes menu action in one undo step, menu lists actions by id, name, and area' },
            adjust: { description: 'Adds kerning, tracking, baseline shift or mark offsets to target text, / for text defaults' },
            kerns: { description: 'Writes pair kerns at insertion indexes or restores metrics kerning' },
            typeset: { description: 'Reads or sets direction, digits, kashida, mark offsets, features and spacing of target text, / for text defaults' },
        },
        resources: {
            object_model: {
                description: 'Class and enumeration names of the linked application, or one declaration with its members',
                completion: { name: ObjectModel.Result.match({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) },
            },
            menu: { description: 'Menu actions of one area or every area', completion: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
            preflight: {
                description: 'Preflight rows per document, finished live results without profile. Reads wait on a process that restarts every live check',
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

export { Libraries, library, listening, plugins };
