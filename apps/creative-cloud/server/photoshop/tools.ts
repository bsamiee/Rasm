// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Crypto, Effect, Encoding, FileSystem, Function, Layer, Option, Path, pipe, Record, Schema, Struct, Tuple } from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { answering, forward, type Handlers, plain, register, succeeded, tool, Value } from '../contract.ts';
import { compare, Drift, fingerprint } from '../drift.ts';
import { notDecodable } from '../errors.ts';
import { HistoryState, Link } from '../frames.ts';
import { Hosts } from '../hosts.ts';
import { type Jobs, request, Spilled, spill } from '../jobs.ts';
import { doJavascript } from '../osascript.ts';
import { type Links, linkState, prepared, probing, READ, session } from '../socket.ts';
import { OptionalString, PROBE_MS, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Bodies, Results } from './jobs.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _scoped = { suspendHistory: Schema.OptionFromOptionalKey(HistoryState), timeoutMs: TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS))) } as const;

// --- [SERVICES] ------------------------------------------------------------------------

const _session = session('photoshop');

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path, Hosts]);

const _plain = plain(Bodies, Results);
const _baseline = Schema.OptionFromOptionalKey(Schema.JsonObject);
const _drift = Schema.OptionFromOptionalKey(Drift);

const _toolkit = Toolkit.make(
    _tool(
        'photoshop_execute',
        'Runs `code` as an async function body in Photoshop UXP and returns its value as JSON. With `commandName` the body runs inside `executeAsModal`, and `suspendHistory` folds its changes on that document into one history state',
        Schema.Struct({ code: Schema.String, commandName: OptionalString, ..._scoped }),
        Value,
        false,
    ),
    _tool(
        'photoshop_batch_play',
        'Plays action descriptors through `batchPlay` inside `executeAsModal` titled `commandName`, `suspendHistory` folding the document changes into one history state. Every fulfilled element is inspected: with `continueOnError` the array stays index-aligned and each `_obj: "error"` element fills a `failed` row, without it the batch stops at the first failing descriptor and the call answers `descriptorFailed`; `result -128` answers `userCancelled`',
        Schema.Struct({ ...Bodies.fields.batchPlay.fields, commandName: Schema.String, ..._scoped }),
        Schema.Struct({ ...Results.fields.batchPlay.fields, tookMs: Schema.Number }),
        false,
    ),
    _tool(
        'photoshop_snapshot',
        'Renders the composite of a document, its selection mask, or one layer as a base64 JPEG through `imaging.getPixels` at the pixel budget, `region` normalized over the document, and answers the pixel size, the pyramid level, and the full-resolution source bounds; a success above the spill threshold lands under `.artifacts/creative-cloud/photoshop/results/` and answers the file',
        Schema.Struct({ ...Bodies.fields.snapshot.fields, baseline: _baseline }),
        Schema.Union([Schema.Struct({ ...Results.fields.snapshot.fields, drift: _drift }), Spilled]),
        true,
    ),
    _tool(
        'photoshop_get_document',
        "Lists the open documents and reads the active or the named document's mode, depth, profile, pixel size, resolution, and its layer tree flattened in panel order to `depth`. With baseline, compares the returned native fields",
        Schema.Struct({ ...Bodies.fields.getDocument.fields, baseline: _baseline }),
        Schema.Struct({ ...Results.fields.getDocument.fields, drift: _drift }),
        true,
    ),
    _tool(
        'photoshop_get_preferences',
        'Reads every key of the named `app.preferences` classes, the key set reflected from the class prototype on the host. With baseline, compares exact section/key readings and preserves unreadable keys',
        Schema.Struct({ ...Bodies.fields.getPreferences.fields, baseline: _baseline }),
        Schema.Struct({ ...Results.fields.getPreferences.fields, drift: _drift }),
        true,
    ),
    _plain(
        'photoshop_set_preferences',
        'setPreferences',
        'Writes `app.preferences.<section>.<key>` rows in order inside `executeAsModal` and reads each back, the typed target table when `values` is absent; a `notifications` write while `quietMode` holds answers `preferenceLocked`, and a readback that neither changed nor matched is a rejected row',
        false,
    ),
    _tool(
        'photoshop_list_presets',
        "Reads the application's `presetManager` descriptor and answers the names of the group whose descriptor class names `kind`. With baseline, compares names in native order, including duplicates",
        Schema.Struct({ ...Bodies.fields.listPresets.fields, baseline: _baseline }),
        Schema.Struct({ ...Results.fields.listPresets.fields, drift: _drift }),
        true,
    ),
    _plain(
        'photoshop_apply_type_styles',
        'applyTypeStyles',
        'Applies named character and paragraph style definitions to exact text-layer ids. Every native style property is available, including World-Ready composition and Persian/Arabic digits; values use the native CharacterStyle and ParagraphStyle units without conversion. All targets resolve before any write; the reply reads every supplied property back',
        false,
    ),
    _plain(
        'photoshop_compose_layers',
        'composeLayers',
        'Builds a layer taxonomy and render-pass composition from an ordered table of groups, pixel layers, styled text and exact source layers. Each parent is an earlier group row; native blend modes, opacity, visibility, clipping and locks come from the table. Resolves all sources and styles before writes, preserves source layers, applies locks after children exist and returns native hierarchy and property readbacks',
        false,
    ),
    _tool(
        'photoshop_run_action',
        'Plays the named action of the named set from `app.actionTree` inside `executeAsModal`, as one history state of the active document while one is open, and answers the command name it played under',
        Bodies.fields.runAction,
        Schema.Struct({ kind: Schema.Literal('played'), commandName: Schema.String, tookMs: Schema.Number }),
        false,
    ),
    _tool(
        'photoshop_system_report',
        'Reads `app.systemInformation` through osascript, the text listing every plugin with its load state, and answers `hostBusy` while a plugin job is in flight',
        Tool.EmptyParams,
        Schema.Struct({ kind: Schema.Literal('report'), text: Schema.String }),
        true,
    ),
);

// --- [DISPATCH] ------------------------------------------------------------------------

const _prepared = prepared(Bodies);

const _forward = forward(Bodies, Results);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Hosts | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path> = Layer.provide(
    Layer.provide(
        register(_toolkit),
        _toolkit.toLayer(
            Effect.map(_session.tag, (channel) =>
                Struct.map(
                    {
                        [_toolkit.tools.photoshop_execute.name]: ({ code, commandName, suspendHistory, timeoutMs }) =>
                            Effect.map(
                                _prepared(channel, timeoutMs, 'execute', () => Effect.succeed({ code, undoName: Option.none() }), Results.fields.execute, { suspendHistory, commandName }),
                                ({ value, tookMs }) => Value.make({ kind: 'value', value, tookMs, autocorrections: Option.none(), undo: Option.none() }),
                            ),
                        [_toolkit.tools.photoshop_batch_play.name]: ({ descriptors, continueOnError, immediateRedraw, commandName, suspendHistory, timeoutMs }) =>
                            Effect.map(
                                _prepared(channel, timeoutMs, 'batchPlay', () => Effect.succeed({ descriptors, continueOnError, immediateRedraw }), Results.fields.batchPlay, {
                                    suspendHistory,
                                    commandName: Option.some(commandName),
                                }),
                                ({ value, tookMs }) => ({ ...value, tookMs }),
                            ),
                        [_toolkit.tools.photoshop_snapshot.name]: Effect.fnUntraced(function* (capture) {
                            const { jobId, value } = yield* _prepared(channel, TIMEOUT_MS, 'snapshot', () => Effect.succeed(capture), Results.fields.snapshot, READ);
                            const pixels = Effect.suspend(() => Effect.fromResult(Encoding.decodeBase64(value.base64))).pipe(
                                Effect.flatMap(fingerprint),
                                Effect.catch((cause) => Effect.succeed({ _tag: 'unreadable' as const, cause })),
                            );
                            const drift = Option.isNone(capture.baseline) ? Option.none() : Option.some(yield* compare(capture.baseline.value, { [channel.link.host]: yield* pixels }));
                            const captured = { ...value, drift };
                            const spilled = yield* spill(channel.link.host, jobId, Schema.encodeSync(succeeded(_toolkit.tools.photoshop_snapshot).members[0])(captured));
                            return spilled.kind === 'file' ? spilled : captured;
                        }),
                        [_toolkit.tools.photoshop_get_document.name]: Effect.fnUntraced(function* (input) {
                            const value = yield* _forward(channel, 'getDocument', READ)(input);
                            if (Option.isNone(input.baseline)) {
                                return { ...value, drift: Option.none() };
                            }
                            const observed = Record.map(Schema.encodeSync(Results.fields.getDocument)(value), (field) => ({ _tag: 'value' as const, value: field }));
                            return { ...value, drift: Option.some(yield* compare(input.baseline.value, observed)) };
                        }),
                        [_toolkit.tools.photoshop_get_preferences.name]: Effect.fnUntraced(function* (input) {
                            const value = yield* _forward(channel, 'getPreferences', READ)(input);
                            if (Option.isNone(input.baseline)) {
                                return { ...value, drift: Option.none() };
                            }
                            const rows = Array.flatMap(Record.toEntries(value.values), ([section, values]) => Array.map(Record.toEntries(values), Tuple.appendElement(section)));
                            const observed = Record.fromIterableWith(rows, ([key, field, section]) => [`${section}.${key}`, { _tag: 'value' as const, value: field }]);
                            const unreadable = Record.fromIterableWith(value.unreadable, ({ section, key, cause }) => [`${section}.${key}`, { _tag: 'unreadable' as const, cause }]);
                            const drift = yield* compare(input.baseline.value, { ...observed, ...unreadable });
                            return { ...value, drift: Option.some(drift) };
                        }),
                        [_toolkit.tools.photoshop_set_preferences.name]: _forward(channel, 'setPreferences', { suspendHistory: Option.none(), commandName: Option.some('Apply preferences') }),
                        [_toolkit.tools.photoshop_list_presets.name]: Effect.fnUntraced(function* (input) {
                            const value = yield* _forward(channel, 'listPresets', READ)(input);
                            if (Option.isNone(input.baseline)) {
                                return { ...value, drift: Option.none() };
                            }
                            const observed = Record.map(value, (field) => ({ _tag: 'value' as const, value: field }));
                            return { ...value, drift: Option.some(yield* compare(input.baseline.value, observed)) };
                        }),
                        [_toolkit.tools.photoshop_apply_type_styles.name]: (input) =>
                            Effect.map(
                                _prepared(channel, TIMEOUT_MS, 'applyTypeStyles', () => Effect.succeed(input), Results.fields.applyTypeStyles, {
                                    commandName: Option.some('Apply type styles'),
                                    suspendHistory: Option.some({ documentId: input.documentId, name: 'Apply type styles' }),
                                }),
                                Struct.get('value'),
                            ),
                        [_toolkit.tools.photoshop_compose_layers.name]: (input) =>
                            Effect.map(
                                _prepared(channel, TIMEOUT_MS, 'composeLayers', () => Effect.succeed(input), Results.fields.composeLayers, {
                                    commandName: Option.some('Compose layers'),
                                    suspendHistory: Option.some({ documentId: input.documentId, name: 'Compose layers' }),
                                }),
                                Struct.get('value'),
                            ),
                        [_toolkit.tools.photoshop_run_action.name]: ({ set, action }) =>
                            Effect.gen(function* () {
                                const played = `${set} › ${action}`;
                                const commandName = Option.some(played);
                                const link = yield* linkState(channel.link);
                                const documentId = pipe(Option.liftPredicate(link, Link.guards.attached), Option.flatMap(Struct.get('state')), Option.flatMap(Struct.get('activeDocumentId')));
                                const { tookMs } = yield* _prepared(channel, TIMEOUT_MS, 'runAction', Function.constant(Effect.succeed({ set, action })), Results.fields.runAction, {
                                    suspendHistory: Option.all({ documentId, name: commandName }),
                                    commandName,
                                });
                                return { kind: 'played' as const, commandName: played, tookMs };
                            }),
                        [_toolkit.tools.photoshop_system_report.name]: Effect.fnUntraced(function* () {
                            const entry = yield* request(PROBE_MS);
                            const text = yield* probing(channel, entry, Option.some(doJavascript('app.systemInformation'))).pipe(
                                Effect.flatMap(Effect.fromResult),
                                Effect.flatMap((value) => Schema.decodeUnknownEffect(Schema.String)(value).pipe(Effect.mapError(notDecodable(channel.link.host, value)))),
                            );
                            return { kind: 'report' as const, text };
                        }),
                    } satisfies Handlers<typeof _toolkit.tools>,
                    answering,
                ),
            ),
        ),
    ),
    _session.layer,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
