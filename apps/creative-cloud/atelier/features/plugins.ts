import { SourceMap } from 'node:module';
import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { ExecuteScript, HostError, Reference, Render, Requests } from '@rasm/automation/requests';
import { Manifest as Extension } from '@rasm/cep/manifest';
import { Contract as InDesign } from '@rasm/indesign/host';
import { registerRpcGroup } from '@rasm/mcp/rpc';
import { Instructions } from '@rasm/mcp/server';
import { registerRoutedTool } from '@rasm/mcp/tool';
import { Contract as Photoshop } from '@rasm/photoshop/host';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { Manifest } from '@rasm/uxp/manifest';
import { type JscTarget, transformSync } from '@swc/core';
import { Array, Context, Effect, Layer, Match, Option, Path, Schema, SchemaGetter, Stream, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { RpcGroup } from 'effect/rpc';
import { Capture } from '#/features/applications';
import type { routes } from '#/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Library = Effect.Success<typeof linked>['libraries'][number];

// --- [MODELS] --------------------------------------------------------------------------

const Libraries = RpcGroup.make().merge(InDesign, Photoshop);
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
const Returned = Schema.TaggedStruct('Returned', ExecuteScript.Result.fields);
const Scripted = Schema.Union([
    Schema.NullOr(Schema.Literal('')).pipe(Schema.decodeTo(Schema.toType(Returned), { decode: SchemaGetter.succeed({ _tag: 'Returned' as const, result: Option.none() }), encode: SchemaGetter.forbiddenEncoding })),
    Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Union([Schema.TaggedStruct('Failed', { failed: Schema.Struct({ message: Schema.String, line: Schema.OptionFromNullOr(Schema.Int) }) }), Returned])))),
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const lowered = Effect.fnUntraced(function* (code: string, target: JscTarget, awaits: boolean) {
    const { code: source, map } = yield* Effect.try({
        try: () => transformSync(`(${awaits ? 'async ' : ''}function body() {\n${code}\n})`, { jsc: { target, parser: { syntax: 'typescript' }, assumptions: { iterableIsArray: true } }, sourceMaps: true }),
        catch: (cause) =>
            Option.match(Schema.decodeUnknownOption(Diagnostic)(cause), {
                onNone: () => HostError.refused('TypeScript transform of body')(cause),
                onSome: ({ message: [, text, , line] }) => HostError.thrown(text, Option.some(line - 1))(cause),
            }),
    });
    const mapping = new SourceMap(yield* Effect.orDie(Schema.decodeUnknownEffect(Mapping)(map)));
    const relined = ({ line }: { readonly line: number }, error: HostError) =>
        Effect.fail(HostError.make({ ...error, reason: Option.match(Schema.decodeUnknownOption(Entry)(mapping.findEntry(line - 1, Number.MAX_SAFE_INTEGER)), { onNone: () => ({ _tag: 'Refused' }) as const, onSome: ({ originalLine }) => ({ _tag: 'Thrown', line: originalLine }) as const }) }));
    return { source, relined };
});

const declaring =
    (commands: readonly { readonly name: string; readonly code: string }[]) =>
    (event: string): Option.Option<{ readonly name: string; readonly code: string }> =>
        Array.findFirst(commands, ({ code }) => code.endsWith(event));

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = Effect.gen(function* () {
    const [automation, link] = yield* Effect.all([Automation, Link.listen(Libraries, { middleware: Layer.empty })]);
    const serving = (installed: readonly Installation[], identifier: string) => (peer: Link) => installed.some((row) => row.identifier.startsWith(identifier) && Link.serves(row.url)(peer));
    const owned = (identifier: string) => (peers: readonly Link[]) => Effect.map(automation.installed, (installed) => peers.filter(serving(installed, identifier)));
    const libraries = Array.map(
        [
            { identifier: 'com.adobe.InDesign', Contract: InDesign, target: Manifest.target },
            { identifier: 'com.adobe.Photoshop', Contract: Photoshop, target: Manifest.target },
            { identifier: 'com.adobe.illustrator', Contract: Requests, target: Extension.target },
            { identifier: 'com.adobe.AfterEffects', Contract: Requests, target: Extension.target },
        ] as const satisfies readonly { readonly identifier: string; readonly Contract: RpcGroup.Any; readonly target: JscTarget }[],
        (library) => ({ ...library, peers: Effect.flatMap(link.peers, owned(library.identifier)), changes: Stream.mapEffect(link.changes, owned(library.identifier)), call: link.call }),
    );
    return { libraries, changes: link.changes };
});
const plugins = Effect.fnUntraced(function* ({ serving, scripting }: Effect.Success<ReturnType<typeof routes>>, links: readonly Library[]) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const opened = serving('open');
    const opening = (row: Installation) =>
        Array.map(
            links.filter((library) => library.Contract.requests.has('open') && row.identifier.startsWith(library.identifier)),
            (library) => ({ installation: row, library }),
        );
    const linking = ({ installation, library }: ReturnType<typeof opening>[number]) => Effect.map(library.peers, Array.some(Link.serves(installation.url)));
    const launched = ({ installation, library }: ReturnType<typeof opening>[number]) => automation.launch({ url: installation.url }).pipe(Effect.andThen(Stream.runHead(Stream.filter(library.changes, Array.some(Link.serves(installation.url))))), Effect.timeout('2 minutes'));
    const routed = Effect.fnUntraced(function* (file: URL) {
        const [urls, installed] = yield* Effect.all([automation.applications({ toOpen: file }), automation.installed]);
        const candidates = Array.flatMap(
            installed.filter((row) => Array.contains(Array.map(urls, Struct.get('href')), row.url.href)),
            opening,
        );
        const first = yield* Effect.fromOption(Array.head(candidates), () => Unmatched.make({ subject: 'linked application opening file', query: Option.some(file.href), candidates: Array.map(urls, Struct.get('href')) }));
        const ready = yield* Effect.findFirst(candidates, linking);
        const chosen = Option.getOrElse(ready, () => first);
        yield* Option.isSome(ready) ? Effect.void : launched(chosen);
        return yield* chosen.library.call(Link.serves(chosen.installation.url));
    });
    const scripted = Effect.fnUntraced(function* (code: string, { installation, pid }: { readonly installation: Installation; readonly pid: number }) {
        const { source, relined } = yield* lowered(code, Extension.target, false);
        const terms = yield* automation.terms({ url: installation.url });
        const { name } = yield* Effect.fromOption(Array.findFirst(['DjxM', 'dosc'], declaring(terms.command)), () => Unmatched.make({ subject: 'script command', candidates: Array.map(terms.command, Struct.get('name')) }));
        const definition = yield* automation.definition({ url: installation.url, kind: 'command', name });
        const { reply } = yield* automation.execute({
            pid,
            command: name,
            arguments: {
                direct: `(function (body) {
    function encoded(value) {
        var parts = [], key, at, kind;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        kind = Object.prototype.toString.call(value);
        if (kind === "[object Array]") { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (kind !== "[object Object]") return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    try {
        var value = eval(body).call(this);
        return value === undefined ? "" : encodeURIComponent('{"_tag":"Returned","result":' + encoded(value) + '}');
    } catch (error) {
        return encodeURIComponent('{"_tag":"Failed","failed":' + encoded({ message: String(error && error.message !== undefined ? error.message : error), line: error && (error.line || error.lineNumber) }) + '}');
    }
}).call(this, arguments[0])`,
                'with arguments': [source],
                ...(definition.includes('code="doLg"') ? { doLg: { _tag: 'enumerator', name: 'JSLg' } as const } : {}),
            },
            artifacts: [],
        });
        return yield* Match.valueTags(yield* Schema.decodeUnknownEffect(Scripted)(reply), {
            Failed: ({ failed }) => Effect.catchReason(Effect.fail(HostError.thrown(failed.message, failed.line)(failed)), 'HostError', 'Thrown', relined),
            Returned: ({ result }) => Effect.succeed({ result }),
        });
    });
    yield* registerRoutedTool(scripting)(
        Tool.make('execute_script', {
            description: 'Runs TypeScript function body in application, returns its result as JSON, failures name body line',
            parameters: Schema.Struct({ code: ExecuteScript.fields.code.annotate({ description: 'Function body, return gives result. Scope holds host classes, enumerations, and resolve(specifier), object_model lists members' }) }),
            success: ExecuteScript.Result,
        }).annotate(Instructions, [
            'execute_script: linked InDesign runs body synchronously in one undo step, a throw rolls it back and awaited work runs after the answer. Linked Photoshop awaits body in one history state. Applications without a link run it through their dictionary script command as ExtendScript or Acrobat JavaScript',
            'execute_script results encode InDesign enumerators by name, InDesign and Photoshop objects as {type, specifier, name, valid}, InDesign collections as {type, specifier, length}',
        ]),
        ({ code }, chosen) =>
            Option.match(chosen.linked, {
                onNone: () => scripted(code, chosen),
                onSome: Effect.fnUntraced(function* ({ library, client }) {
                    const { source, relined } = yield* lowered(code, library.target, Context.get(library.Contract.annotations, ExecuteScript.Awaits));
                    return yield* Effect.catchReason(client('execute_script', { code: source }), 'HostError', 'Thrown', relined);
                }),
            }),
    );
    yield* registerRoutedTool(opened.optional('linked application Launch Services picks for file'))(
        Tool.make('open', {
            description: 'Opens document without dialogs and returns its reference, without window where application allows. Without application or pid, launches linked application Launch Services picks when none runs and waits for its link. After Effects replaces its open project',
            parameters: Schema.Struct({ file: Schema.String.annotate({ description: 'Document path' }) }),
            success: Reference,
        }),
        Effect.fnUntraced(function* ({ file }, chosen) {
            const url = yield* path.toFileUrl(file);
            const opener = yield* Option.match(chosen, { onSome: ({ client }) => Effect.succeed(client), onNone: () => routed(url) });
            return yield* opener('open', { file: url });
        }),
    );
    yield* registerRoutedTool(serving('render'))(
        Tool.make('render', {
            description: 'Renders document target as PNG through application exporter, InDesign page items alone and their page or spread with context. Use screenshot for panels, dialogs, and pasteboard',
            parameters: Schema.Struct(Struct.omit(Render.fields, ['file'])),
            success: Capture,
        }).annotate(Tool.Readonly, true),
        (payload, { client }) =>
            Effect.map(
                Capture.written(
                    Effect.fnUntraced(function* (file) {
                        return yield* client('render', { ...payload, file: yield* path.toFileUrl(file) });
                    }),
                ),
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
            adjust: { description: 'Adds kerning, tracking, baseline shift or mark offsets to selected text' },
            kerns: { description: 'Writes pair kerns at insertion indexes or restores metrics kerning' },
            typeset: { description: 'Reads or sets direction, digits, kashida, mark offsets, features and spacing of selected text' },
        },
        resources: {
            object_model: {
                description: 'Class and enumeration names of the linked application, or one declaration with its members',
                complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) },
            },
            menu: { description: 'Menu actions of one area or every area', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
            preflight: {
                description: 'Preflight rows per document, finished live results without profile. Reads wait on a process that restarts every live check',
                complete: { document: Array.map((process) => process.targetObject.specifier) },
                report: Array.some(({ aggregatedResults }) => Array.isReadonlyArrayNonEmpty(aggregatedResults)),
            },
        },
        route: (rpc) => serving(rpc._tag),
        invoke:
            (rpc) =>
            (payload, { client }) =>
                client(rpc._tag, payload),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Library, linked, plugins };
