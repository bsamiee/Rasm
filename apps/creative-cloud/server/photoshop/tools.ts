// --- [IMPORTS] -------------------------------------------------------------------------

import { Crypto, Effect, FileSystem, Function, Layer, Option, Path, pipe, Schema, Struct } from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { answering, forward, host, plain, tool, Value } from '../contract.ts';
import { HistoryState, Link } from '../frames.ts';
import { Hosts, installed } from '../hosts.ts';
import { type Jobs, probe, Spilled, spill } from '../jobs.ts';
import { doJavascript, read } from '../osascript.ts';
import { answer, type Links, linkState, READ, session } from '../socket.ts';
import { OptionalString, PROBE_MS, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Bodies, Results } from './jobs.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _scoped = { suspendHistory: Schema.OptionFromOptionalKey(HistoryState), timeoutMs: TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS))) } as const;

// --- [SERVICES] ------------------------------------------------------------------------

const _session = session('photoshop');

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path, Hosts]);

const _plain = plain(Bodies, Results);

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
        Bodies.fields.snapshot,
        Schema.Union([Results.fields.snapshot, Spilled]),
        true,
    ),
    _plain(
        'photoshop_get_document',
        'getDocument',
        "Lists the open documents and reads the active or the named document's mode, depth, profile, pixel size, resolution, and its layer tree flattened in panel order to `depth`",
        true,
    ),
    _plain('photoshop_get_preferences', 'getPreferences', 'Reads every key of the named `app.preferences` classes, the key set reflected from the class prototype on the host', true),
    _plain(
        'photoshop_set_preferences',
        'setPreferences',
        'Writes `app.preferences.<section>.<key>` rows in order inside `executeAsModal` and reads each back, the typed target table when `values` is absent; a `notifications` write while `quietMode` holds answers `preferenceLocked`, and a readback that neither changed nor matched is a rejected row',
        false,
    ),
    _plain('photoshop_list_presets', 'listPresets', "Reads the application's `presetManager` descriptor and answers the names of the group whose descriptor class names `kind`", true),
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

const _answer = answer(Bodies);

const _forward = forward(Bodies, Results);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Hosts | ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path> = Layer.provide(
    host(_toolkit, _session.tag, (channel) =>
        answering(_toolkit, {
            [_toolkit.tools.photoshop_execute.name]: ({ code, commandName, suspendHistory, timeoutMs }) =>
                Effect.map(_answer(channel, timeoutMs, 'execute', { code, undoName: Option.none() }, Results.fields.execute, { suspendHistory, commandName }), ({ value, tookMs }) =>
                    Value.make({ kind: 'value', value, tookMs, autocorrections: Option.none(), undo: Option.none() }),
                ),
            [_toolkit.tools.photoshop_batch_play.name]: ({ descriptors, continueOnError, immediateRedraw, commandName, suspendHistory, timeoutMs }) =>
                Effect.map(
                    _answer(channel, timeoutMs, 'batchPlay', { descriptors, continueOnError, immediateRedraw }, Results.fields.batchPlay, { suspendHistory, commandName: Option.some(commandName) }),
                    ({ value, tookMs }) => ({ ...value, tookMs }),
                ),
            [_toolkit.tools.photoshop_snapshot.name]: (capture) =>
                Effect.flatMap(_answer(channel, TIMEOUT_MS, 'snapshot', capture, Results.fields.snapshot, READ), ({ jobId, value }) => spill(channel.link.host, jobId, value)),
            [_toolkit.tools.photoshop_get_document.name]: _forward(channel, 'getDocument', READ),
            [_toolkit.tools.photoshop_get_preferences.name]: _forward(channel, 'getPreferences', READ),
            [_toolkit.tools.photoshop_set_preferences.name]: _forward(channel, 'setPreferences', { suspendHistory: Option.none(), commandName: Option.some('Apply preferences') }),
            [_toolkit.tools.photoshop_list_presets.name]: _forward(channel, 'listPresets', READ),
            [_toolkit.tools.photoshop_run_action.name]: ({ set, action }) =>
                Effect.gen(function* () {
                    const played = `${set} › ${action}`;
                    const commandName = Option.some(played);
                    const link = yield* linkState(channel.link);
                    const documentId = pipe(Option.liftPredicate(link, Link.guards.attached), Option.flatMap(Struct.get('state')), Option.flatMap(Struct.get('activeDocumentId')));
                    const { tookMs } = yield* _answer(channel, TIMEOUT_MS, 'runAction', { set, action }, Results.fields.runAction, {
                        suspendHistory: Option.all({ documentId, name: commandName }),
                        commandName,
                    });
                    return { kind: 'played' as const, commandName: played, tookMs };
                }),
            [_toolkit.tools.photoshop_system_report.name]: () =>
                Effect.gen(function* () {
                    const { bundleId } = yield* installed(channel.link.host, (yield* Hosts)[channel.link.host]);
                    const text = yield* Effect.flatMap(
                        probe(channel.host, Function.constant(read(channel.link.host, bundleId, PROBE_MS, doJavascript('app.systemInformation'), Option.none()))),
                        Effect.fromResult,
                    );
                    return { kind: 'report' as const, text };
                }),
        }),
    ),
    _session.layer,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
