import { SourceMap } from 'node:module';
import { AfterEffects } from '@rasm/aftereffects/contract';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { ExecuteScript, HostError, ObjectModel, Reference, Render } from '@rasm/automation/requests';
import { answered, Reply } from '@rasm/cep/contract';
import { ExtensionManifest } from '@rasm/cep/manifest';
import { Illustrator } from '@rasm/illustrator/contract';
import { InDesign } from '@rasm/indesign/contract';
import { Route } from '@rasm/mcp/route';
import { registerRpcGroup } from '@rasm/mcp/rpc';
import { Server } from '@rasm/mcp/server';
import { Photoshop } from '@rasm/photoshop/contract';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { type JscTarget, transformSync } from '@swc/core';
import { Array, Context, Duration, Effect, Layer, Option, Path, Schema, Stream, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcGroup } from 'effect/rpc';
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
    const relined = (error: HostError) => Effect.fail(HostError.fields.reason.match(error.reason, { Refused: () => error, Thrown: ({ line }) => HostError.thrown(error.subject, Option.flatMap(line, original))(error.cause) }));
    return { source, relined };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const listening = Effect.gen(function* () {
    const [automation, listened] = yield* Effect.all([Automation, Link.listen(Libraries, { path: link.path, middleware: Layer.empty })]);
    const owns = (installed: readonly Installation[], identifier: string) => (peer: Link) => installed.some((row) => row.identifier.startsWith(identifier) && Link.serves(row.url)(peer));
    const owned = (identifier: string) => (peers: readonly Link[]) => Effect.map(automation.installed, (installed) => peers.filter(owns(installed, identifier)));
    const libraries = Array.map([InDesign, Photoshop, Illustrator, AfterEffects] as const satisfies readonly { readonly identifier: string; readonly Contract: RpcGroup.Any; readonly target: JscTarget }[], ({ identifier, Contract: contract, target }) => ({
        identifier,
        contract,
        target,
        peers: Effect.flatMap(listened.peers, owned(identifier)),
        changes: Stream.mapEffect(listened.changes, owned(identifier)),
    }));
    return { libraries, changes: listened.changes, call: listened.call };
});
const plugins = Effect.fnUntraced(function* ({ serving, script }: Effect.Success<ReturnType<typeof routes>>, { libraries, call }: Effect.Success<typeof listening>) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const [opened, rendered] = [serving('open'), serving('render')];
    const opening = (row: Installation) =>
        Array.map(
            libraries.filter((library) => library.contract.requests.has(opened.tag) && row.identifier.startsWith(library.identifier)),
            (library) => ({ installation: row, library }),
        );
    const linking = ({ installation, library }: ReturnType<typeof opening>[number]) => Effect.map(library.peers, Array.some(Link.serves(installation.url)));
    const launched = ({ installation, library }: ReturnType<typeof opening>[number]) =>
        automation
            .launch({ url: installation.url })
            .pipe(Effect.andThen(Stream.runHead(Stream.filter(library.changes, Array.some(Link.serves(installation.url))))), Effect.timeoutOrElse({ duration: LINK_WAIT, orElse: () => Effect.fail(HostError.refused(`${library.identifier} link from ${installation.name} within ${Duration.format(LINK_WAIT)}`)()) }));
    const opener = Effect.fnUntraced(function* (file: URL) {
        const rows = yield* automation.opening(file);
        const candidates = Array.flatMap(rows, opening);
        const first = yield* Effect.fromOption(Array.head(candidates), () => Unmatched.make({ subject: 'linked application opening file', query: Option.some(file.href), candidates: Array.map(rows, Struct.get('name')) }));
        const { installation } = yield* Option.match(yield* Effect.findFirst(candidates, linking), { onSome: Effect.succeed, onNone: () => Effect.as(launched(first), first) });
        return yield* call(installation.url);
    });
    const scripted = Effect.fnUntraced(function* (code: string, { pid }: { readonly pid: number }) {
        const { source, relined } = yield* lowered(code, ExtensionManifest.target, false);
        const { reply } = yield* automation.script({
            pid,
            direct: `(function () { var body = ${source}
    function encoded(value) {
        var parts = [], key, at, kind;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\\\u2028\\u2029]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        kind = Object.prototype.toString.call(value);
        if (kind === "[object Array]") { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (kind !== "[object Object]") return encoded(String(value));
        for (key in value) if (value[key] !== undefined) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    try {
        var value = body.call(this);
        return encodeURIComponent('{"_tag":"Value","value":{' + (value === undefined ? "" : '"result":' + encoded(value)) + '}}');
    } catch (error) {
        return encodeURIComponent(encoded({ _tag: "Thrown", message: String(error && error.message !== undefined ? error.message : error), number: error && error.number, line: error && (error.line || error.lineNumber) }));
    }\n}).call(this)`,
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
                onSome: Effect.fnUntraced(function* ({ library, client }) {
                    const { source, relined } = yield* lowered(code, library.target, Context.get(library.contract.annotations, ExecuteScript.Awaits));
                    return yield* Effect.catchTag(client(script.tag, { code: source }), 'HostError', relined);
                }),
            }),
    );
    yield* Route.register(opened.optional('linked application Launch Services picks for file'))(
        Tool.make(opened.tag, {
            description: 'Opens document without dialogs and returns its reference, without window where application allows, launching the picked application when none runs and waiting for its link. After Effects replaces its open project',
            parameters: Schema.Struct({ file: AbsolutePath.annotate({ description: 'Document path' }) }),
            success: Reference,
        }),
        Effect.fnUntraced(function* ({ file }, chosen) {
            const url = yield* path.toFileUrl(file);
            const client = yield* Option.match(chosen, { onSome: (picked) => Effect.succeed(picked.client), onNone: () => opener(url) });
            return yield* client(opened.tag, { file: url });
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

export { Libraries, listening, plugins };
