import { stripTypeScriptTypes } from 'node:module';
import { type Extension, LinkError, listen, make, READING, ranked, WRITING, watched } from '@rasm/mcp/extension';
import { Array, Effect, FileSystem, Function, flow, Option, Schema, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import type { HttpRouter } from 'effect/http';
import type { RpcClient, RpcClientError } from 'effect/rpc';
import { Body, Execute, Face, Find, InDesignError, Inspect, Menu, PATH, Panel, Peer, Preflight, Render, Shot, Target } from './contract.ts';
import { grep } from './grep.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Failure = Schema.Union([InDesignError, LinkError]);
const Facts = Shot.mapFields(Struct.omit(['image']));
const Seen = Schema.OptionFromOptionalKey(Facts);
const Served = Schema.Struct({ identifier: Schema.Literal('com.adobe.InDesign'), version: Schema.Option(Schema.String), running: Schema.Boolean });
const extension = (product: string): Extension<never, FileSystem.FileSystem | HttpRouter.HttpRouter | HttpRouter.Request<'Error', unknown> | McpServer.McpServer> =>
    make(
        {
            name: product,
            serves: Served,
            instructions: Array.join(
                [
                    `${product} serves InDesign through its panel, which links when InDesign starts. Measurements are numbers in points.`,
                    '- inspect: no target for application state, read first and after each change; a specifier from any result for that object, with fields to read only those; the document field viewPreferences holds its units, a collection field gives the collection specifier, and a collection specifier with fields gives those fields of every member.',
                    '- execute: Set writes fields, those a dialog would set included; Script runs UXP JavaScript for the rest.',
                    '- render: a page, page region, spread, or page item as an image.',
                    '- find_change: read indesign://grep/reference before writing GREP, or start from the grep prompt.',
                    "- font: also takes InDesign's face name, family and style joined by a tab, and adds the face's status (missing or substituted included), its document uses, and faces only InDesign activates, read through a subset at their default instance. InDesign lays out with these metrics: Auto underline (underlineOffset and underlineWeight -9999) is the face underline, else 0.15 em below the baseline at 0.05 em; Auto strikethrough sits 1/3 em above the baseline at the underline weight. Text takes a feature through opentypeFeatures as [tag, 1] pairs and axis values through designAxes in axis order; opsz follows the point size.",
                    '- indesign://menu, indesign://menu/{area}, indesign://preflight/{profile}, indesign://grep/queries, indesign://grep/query/{name}: menu actions, preflight results, saved queries.',
                ],
                '\n',
            ),
            preview: Target,
            runs: {
                Script: Tool.make('Script', {
                    description: Array.join(
                        [
                            'UXP JavaScript or TypeScript run in InDesign with app, every enumeration and DOM class, and resolve(specifier) in scope; DOM methods take arguments by position.',
                            'A body without top-level await is one undo step undone whole on a throw; with await, work before the first await is that step and later work lands as separate steps a throw keeps; a preview body has no await. A nested app.doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name) is a savepoint whose throw undoes its own changes.',
                            'Results encode enumerators by name, DOM objects as {type, specifier} with invalid: true once removed, and collections as {type, length}. collection[key] reads as collection.item(key), and a constant name written as a string to an enumeration property becomes its enumerator, both listed in corrections. Collections are not iterable: everyItem().getElements() gives an array. DOM objects and enumerators compare with equals(), and reading a property the object state disallows throws naming it.',
                            '"\\r" ends a paragraph and "\\n" breaks a line inside one; marginPreferences.left is the inside margin on facing pages. Name and label created objects so later calls find them.',
                            'A Script call can occasionally hang InDesign until it is force quit: write fields through Set and find_change, and keep Script for work they cannot do.',
                        ],
                        '\n',
                    ),
                    parameters: Body.cases.Script.mapFields(Struct.omit(['_tag'])),
                    success: Execute.successSchema.pipe(Schema.fieldsAssign({ preview: Seen })),
                    failure: Failure,
                }),
                Set: Tool.make('Set', {
                    description: "Writes fields of an object, of every member of a collection, or of a member add creates, as one undo step. Each refused field fails with InDesign's message and undoes the call. Returns each field before and after",
                    parameters: Body.cases.Set.mapFields(Struct.omit(['_tag'])),
                    success: Execute.successSchema.pipe(Schema.fieldsAssign({ preview: Seen })),
                    failure: Failure,
                }),
            },
            tools: {
                inspect: Tool.make('inspect', {
                    description:
                        "Reads InDesign. No target: application state with documents, windows, selection, overset stories, preflight problem count, and undo history. A specifier: the object's fields with value, type, valid values, settable flag, the collection holding valid objects, and the value without the object's overrides where they change it; then its collections, and the geometry of a page or spread. A collection specifier: its members. A class name: its fields and methods from InDesign's object model. An enumeration name: its members",
                    parameters: Inspect.payloadSchema,
                    success: Inspect.successSchema,
                    failure: Failure,
                }).annotateMerge(READING),
                render: Tool.make('render', {
                    description:
                        "Renders a page, page region, spread, or page item of the active document through InDesign's JPEG export. Pixel (x, y) lies at bounds.left + x * (bounds.right - bounds.left) / width and bounds.top + y * (bounds.bottom - bounds.top) / height in points. An item renders with what overlaps it unless isolate",
                    parameters: Render.payloadSchema,
                    success: Facts,
                    failure: Failure,
                }).annotateMerge(READING),
                find_change: Tool.make('find_change', {
                    description:
                        "Finds, and with action changes, text, GREP, glyph, or objects through InDesign's Find/Change. A JavaScript regular expression is translated to InDesign's GREP with each rewrite listed in corrections, and a construct InDesign rejects fails naming its alternative. action Style adds the pattern to a paragraph style as a GREP style; save stores the query for the Find/Change dialog",
                    parameters: Find.payloadSchema,
                    success: Find.successSchema.pipe(Schema.fieldsAssign({ preview: Seen })),
                    failure: Failure,
                }).annotateMerge(WRITING),
            },
            activity: { find_change: Struct.get('description') },
            face: { success: Face.successSchema, failure: Failure },
        },
        Effect.gen(function* () {
            const linked = `InDesign with the ${product} panel`;
            const [panel, files] = yield* Effect.all([listen(Panel, { path: PATH, origin: 'file://', identity: Peer, peer: linked }), FileSystem.FileSystem]);
            const select =
                (version: typeof Served.Type.version) =>
                (peer: typeof Peer.Type): boolean =>
                    Option.match(Option.flatMap(version, Schema.decodeUnknownOption(Peer.fields.version)), { onNone: Function.constTrue, onSome: (major) => major === peer.version });
            const call =
                (target: Option.Option<typeof Served.Type>) =>
                <A>(use: (client: RpcClient.FromGroup<typeof Panel, RpcClientError.RpcClientError>) => Effect.Effect<A, InDesignError | LinkError | RpcClientError.RpcClientError>): Effect.Effect<A, InDesignError | LinkError> =>
                    panel
                        .call(select(Option.flatMap(target, Struct.get('version'))), use, Option.match(target, { onNone: Function.constTrue, onSome: Struct.get('running') }))
                        .pipe(Effect.catchReason('InDesignError', 'Missing', (reason) => Effect.fail(InDesignError.make({ reason: { ...reason, candidates: ranked(reason.query, reason.candidates) } }))));
            const shown = <R extends { readonly preview: Option.Option<typeof Shot.Type> }>({ preview, ...rest }: R) => [{ ...rest, preview: Option.map(preview, Struct.omit(['image'])) }, Array.map(Option.toArray(preview), ({ image }) => McpSchema.ImageContent.make({ data: image, mimeType: 'image/jpeg' }))] as const;
            const stripped = (code: string): Effect.Effect<string, InDesignError> => {
                const [open, close] = ['async function body() {\n', '\n}'];
                return Effect.try({
                    try: () => stripTypeScriptTypes(`${open}${code}${close}`, { mode: 'strip' }).slice(open.length, -close.length),
                    catch: (cause) => InDesignError.make({ reason: { _tag: 'Thrown', cause, line: Option.none(), source: Option.none(), corrections: Option.none(), kept: false } }),
                });
            };
            const menu = (area: Option.Option<string>): Effect.Effect<string, InDesignError | LinkError> => call(Option.none())((client) => client.Menu({ area })).pipe(Effect.flatMap((actions) => Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Menu.successSchema))(actions))));
            yield* Option.none().pipe(call, grep);
            yield* McpServer.registerResource({ uri: 'indesign://menu', name: 'InDesign menu actions', description: 'Every menu action by id, name, and area', mimeType: 'application/json', content: menu(Option.none()) });
            yield* McpServer.registerResource`indesign://menu/${McpSchema.param('area', Schema.String)}`({
                name: 'InDesign menu actions',
                description: 'Menu actions by id, name, and area, for areas starting with the given text',
                mimeType: 'application/json',
                completion: { area: (input) => Effect.map(Effect.orDie(call(Option.none())((client) => client.Menu({ area: Option.none() }))), (actions) => [...ranked(input, Array.dedupe(Array.map(actions, Struct.get('area'))))]) },
                content: (_uri, area) => menu(Option.some(area)),
            });
            yield* McpServer.registerResource`indesign://preflight/${McpSchema.param('profile', Schema.String)}`({
                name: 'InDesign preflight',
                description: 'Preflight results of the active document under a preflight profile',
                mimeType: 'application/json',
                completion: {
                    profile: (input) =>
                        call(Option.none())((client) => client.Preflight({ profile: Option.some(input) })).pipe(
                            Effect.as([input]),
                            Effect.catchReason('InDesignError', 'Missing', ({ candidates }) => Effect.succeed([...candidates])),
                            Effect.orDie,
                        ),
                },
                content: (_uri, profile) => call(Option.none())((client) => client.Preflight({ profile: Option.some(profile) })).pipe(Effect.flatMap((rows) => Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Preflight.successSchema))(rows)))),
            });
            return {
                linked: (target) =>
                    Effect.map(
                        panel.links,
                        flow(
                            Array.findFirst(select(target.version)),
                            Option.map(({ version }) => String(version)),
                        ),
                    ),
                runs: {
                    Script: (target, { code }, unit) =>
                        Effect.map(
                            Effect.flatMap(stripped(code), (plain) => call(Option.some(target))((client) => watched(linked, client.Execute({ ...unit, body: { _tag: 'Script', code: plain } }), client.Enter()))),
                            shown,
                        ),
                    Set: (target, body, unit) =>
                        Effect.map(
                            call(Option.some(target))((client) => client.Execute({ ...unit, body: { _tag: 'Set', ...body } })),
                            shown,
                        ),
                },
                tools: {
                    inspect: (target, request) =>
                        Effect.map(
                            call(Option.some(target))((client) => client.Inspect(request)),
                            (found) => [found, []] as const,
                        ),
                    render: (target, request) =>
                        Effect.map(
                            call(Option.some(target))((client) => client.Render(request)),
                            ({ image, ...facts }) => [facts, [McpSchema.ImageContent.make({ data: image, mimeType: 'image/jpeg' })]] as const,
                        ),
                    find_change: (target, request) =>
                        Effect.map(
                            call(Option.some(target))((client) => client.Find(request)),
                            shown,
                        ),
                },
                face: Effect.fn('face')(function* (target: typeof Served.Type, name: string, characters: string) {
                    const face = yield* call(Option.some(target))((client) => client.Face({ name }));
                    const source = yield* Effect.transposeOption(
                        Option.map(face.installed, ({ location, postscriptName, coordinates }) =>
                            Effect.flatMap(Effect.orDie(files.exists(location)), (own) =>
                                own
                                    ? Effect.succeed({ file: location, postscriptName, coordinates: Option.liftPredicate(coordinates, Array.isReadonlyArrayNonEmpty) })
                                    : Effect.map(
                                          call(Option.some(target))((client) => client.Subset({ postscriptName, text: characters })),
                                          (file) => ({ file, postscriptName, coordinates: Option.none() }),
                                      ),
                            ),
                        ),
                    );
                    return [face, source] as const;
                }),
            };
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
