import { Installation, Launched, Launching, ProcessInstance, State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { indesign } from '@rasm/atelier-indesign-plugin/tools';
import { ExecuteScript, Rasterize } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { ExtensionManifest } from '@rasm/cep';
import { Completion } from '@rasm/extendscript';
import runtime from '@rasm/extendscript/host?inline';
import { lower, traced } from '@rasm/extendscript/node';
import { Resource, Route, registerRpcGroup, Server } from '@rasm/mcp';
import { HostError, Reference } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Array, Context, Duration, Effect, FileSystem, Number, Option, Path, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { imageSize } from 'image-size';
import { AbsolutePath } from '#/absolute-path';
import { Image } from '#/features/applications';
import { Libraries, LIBRARIES, type listening } from '#/libraries';
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
const Rasterizing = Schema.Struct(Struct.omit(Rasterize.fields, ['file']));
const Opened = Reference.mapFields(Struct.assign({ launched: Schema.OptionFromOptionalKey(Launched.annotate({ description: 'Instance open launched' })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const lowered = Effect.fnUntraced(function* (program: string, target: Parameters<typeof lower>[1]) {
    const { code: source, map } = yield* lower(program, target, 'typescript').pipe(Effect.mapError((error) => (HostError.fields.reason.isAnyOf(['Thrown'])(error.reason) ? HostError.thrown(error.subject, Option.map(error.reason.line, Number.decrement))(error.cause) : error)));
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const plugins = Effect.fnUntraced(function* ({ serving, script }: Effect.Success<ReturnType<typeof routes>>, links: Effect.Success<typeof listening>) {
    const [server, files, path] = yield* Effect.all([ApplicationServer, FileSystem.FileSystem, Path.Path]);
    const [opened, rasterized] = [serving('open'), serving('rasterize')];
    yield* Resource.template`plugin/${McpSchema.param('instance', ProcessInstance)}`({
        name: 'plugin',
        schema: Peer,
        changes: links.changes,
        content: Effect.fnUntraced(function* (_, instance) {
            const [installed, instances, peers] = yield* Effect.all([server.installed, SubscriptionRef.get(server.instances), links.peers]);
            const running = yield* Effect.fromNullishOr(instances.find((row) => row.instance === instance && !State.isAnyOf(['exited'])(row.state)));
            const installation = yield* Effect.fromNullishOr(installed.find((row) => Installation.runs(row)(running)));
            return yield* Unmatched.single(peers.filter(Peer.serves(installation)), `linked plugin of ${installation.name}`, Option.none(), ({ application }) => [String(application)]);
        }),
    });
    const scripted = Effect.fnUntraced(function* (code: string, { instance, installation: { url } }: Effect.Success<ReturnType<typeof script.resolve>>) {
        const { source, relined } = yield* lowered(`var body = ${ExecuteScript.expression(code, false)}`, ExtensionManifest.target);
        const { reply } = yield* server.script({
            instance,
            url,
            events: ['DjxM', 'dosc'],
            declared: { doLg: { _tag: 'enumerator', name: 'JSLg' } },
            direct: `(function (self) { ${source}\n${runtime}\nreturn encodeURIComponent(runtime.reply(function () { return { result: body.call(self) }; }));\n})(this)`,
        });
        return yield* Schema.decodeUnknownEffect(Scripted)(reply).pipe(Effect.flatMap(Completion.answered(ResultJson, ExecuteScript.make({ code: ExecuteScript.expression(code, false) }).label)), Effect.catchTag('HostError', relined));
    });
    const linkedScript = Effect.fnUntraced(function* (code: string, { library: { target, Contract }, client }: Option.Option.Value<Effect.Success<ReturnType<typeof script.resolve>>['linked']>) {
        const { source, relined } = yield* lowered(ExecuteScript.expression(code, Context.get(Contract.annotations, ExecuteScript.Awaits)), target);
        return yield* Effect.catchTag(client(script.tag, { code: source }), 'HostError', relined);
    });
    const opener = Effect.fnUntraced(function* (file: URL) {
        const [rows, peers] = yield* Effect.all([server.opening(file), links.peers]);
        const candidates = rows.filter((installation) => opened.serves({ installation }));
        const first = yield* Effect.fromOption(Array.head(candidates), () => Unmatched.make({ subject: 'linked application opening file', query: Option.some(file.href), candidates: Array.map(rows, Struct.get('name')) }));
        const relinked = Stream.runHead(Stream.filter(links.changes, Array.some(Peer.serves(first))));
        const launching = Launching.launched(server.launch({ url: first.url })).pipe(
            Effect.tap(() => relinked),
            Effect.timeoutOrElse({ duration: LINK_WAIT, orElse: () => Effect.fail(HostError.refused(`link from ${first.name} within ${Duration.format(LINK_WAIT)}`)()) }),
            Effect.map((instance) => [first, Option.some(instance)] as const),
        );
        const [installation, launched] = yield* Option.match(
            Array.findFirst(candidates, (row) => peers.some(Peer.serves(row))),
            { onSome: (linked) => Effect.succeed([linked, Option.none()] as const), onNone: () => launching },
        );
        return { client: yield* links.call(Peer.serves(installation), `linked plugin of ${installation.name}`), launched };
    });
    yield* Route(script)(
        Tool.make(script.tag, { description: 'Runs TypeScript function body in application, returns its result as JSON, failures name body line', parameters: Body, success: ExecuteScript.Result }).annotate(Server.Instructions, [
            ...LIBRARIES.flatMap(Struct.get('instructions')),
            `${script.tag} runs body without a link through the dictionary script command as ExtendScript or Acrobat JavaScript`,
        ]),
        ({ code, requirePlugin }, chosen) => {
            const unlinked = requirePlugin === true ? Effect.fail(HostError.refused(chosen.installation.name)('Atelier plugin is not connected')) : scripted(code, chosen);
            return Option.match(chosen.linked, { onNone: () => unlinked, onSome: (linked) => linkedScript(code, linked) });
        },
    );
    yield* Route(opened.optional('linked application Launch Services picks for file'))(
        Tool.make(opened.tag, {
            description: 'Opens the document without dialogs and returns its reference, with no window where the application allows it. Starts the application Launch Services picks when none runs and waits for its link',
            parameters: Opening,
            success: Opened,
        }),
        Effect.fnUntraced(function* ({ file }, chosen) {
            const url = yield* path.toFileUrl(file);
            const opening = yield* Option.match(chosen, { onSome: ({ client }) => Effect.succeed({ client, launched: Option.none() }), onNone: () => opener(url) });
            return { ...(yield* opening.client(opened.tag, { file: url })), launched: opening.launched };
        }),
    );
    yield* Route(rasterized)(
        Tool.make(rasterized.tag, {
            description: 'Rasterizes document target as PNG through its host, an InDesign page item or Photoshop layer without the rest of its page, spread, or document. capture reads panels, dialogs, and pasteboard',
            parameters: Rasterizing,
            success: Image,
        }).annotate(Tool.Readonly, true),
        Effect.fnUntraced(function* (payload, { client }) {
            const file = yield* files.makeTempFileScoped({ suffix: '.png' });
            yield* client(rasterized.tag, { ...payload, file: yield* path.toFileUrl(file) });
            const image = yield* files.readFile(file);
            const { width, height } = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`PNG size of ${file}`) });
            return { image, width, height };
        }, Effect.scoped),
    );
    yield* registerRpcGroup(Libraries, {
        ...indesign.group,
        route: serving,
        invoke:
            (rpc) =>
            (payload, { client }) =>
                client(rpc._tag, payload),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { plugins };
