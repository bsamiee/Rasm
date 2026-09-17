// --- [IMPORTS] -------------------------------------------------------------------------

import { Context, Crypto, Effect, FileSystem, flow, Layer, Option, Path, Schema, Struct } from 'effect';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { contract, Failure } from '../contract.ts';
import type { BridgeError } from '../errors.ts';
import { HistoryState, type Job } from '../frames.ts';
import { Jobs, probe, SPILL_CHARS, Spilled, spill } from '../jobs.ts';
import { doJavascript, read } from '../osascript.ts';
import { type Answer, answered, type Endpoint, Links } from '../socket.ts';
import { HOSTS, type JobId, PROBE_MS, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { Applied, BatchPlay, Bodies, Descriptors, DocumentState, GetDocument, GetPreferences, Jpeg, type Kind, ListPresets, Played, Preferences, Presets, RunAction, Snapshot } from './jobs.ts';
import { TARGET_ROWS, Writes } from './preferences.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly link: Endpoint;
    readonly host: Jobs['photoshop'];
}

type Services = ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path;

type Scope = Pick<Job, 'suspendHistory' | 'commandName'>;

type Capture = (typeof Jpeg)['Type'] | (typeof Spilled)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.photoshop;
const _READ: Scope = { suspendHistory: Option.none(), commandName: Option.none() };
const _scoped = { suspendHistory: Schema.OptionFromOptionalKey(HistoryState), timeoutMs: Schema.OptionFromOptionalKey(TimeoutMs) } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Channel: Context.Service<Channel, Channel> = Context.Service<Channel>('PhotoshopChannel');

const Reply = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, tookMs: Schema.Number }),
    Schema.Struct({ ...Descriptors.fields, tookMs: Schema.Number }),
    Jpeg,
    Spilled,
    DocumentState,
    Preferences,
    Applied,
    Presets,
    Schema.Struct({ kind: Schema.Literal('report'), text: Schema.String }),
    Failure,
]).pipe(Schema.toTaggedUnion('kind'));

const _row = contract(Channel, [ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

// --- [DISPATCH] ------------------------------------------------------------------------

const _job =
    <K extends Kind>(kind: K, value: (typeof Bodies)['fields'][K]['Type'], scope: Scope) =>
    (jobId: JobId): Job => ({ jobId, kind, body: Schema.encodeSync(Schema.toCodecJson(Bodies.fields[kind]))(value), ...scope });

const _answered = <K extends Kind, S extends Schema.ConstraintCodec<unknown, unknown, never, never>>(
    channel: Channel,
    timeoutMs: number,
    kind: K,
    value: (typeof Bodies)['fields'][K]['Type'],
    result: S,
    scope: Scope,
): Effect.Effect<Answer<S['Type']>, BridgeError, Services> => answered(channel.link, channel.host, timeoutMs, result, flow(_job(kind, value, scope), Effect.succeed));

const _value = <K extends Kind, S extends Schema.ConstraintCodec<unknown, unknown, never, never>>(
    channel: Channel,
    kind: K,
    value: (typeof Bodies)['fields'][K]['Type'],
    result: S,
): Effect.Effect<S['Type'], BridgeError, Services> => Effect.map(_answered(channel, TIMEOUT_MS, kind, value, result, _READ), Struct.get('value'));

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Services> = Layer.provide(
    Layer.mergeAll(
        _row(
            'photoshop_execute',
            'Runs `code` as an async function body in Photoshop UXP and returns its value as JSON. With `commandName` the body runs inside `executeAsModal`, and `suspendHistory` folds its changes on that document into one history state',
            Schema.Struct({ code: Schema.String, commandName: Schema.OptionFromOptionalKey(Schema.String), ..._scoped }),
            Reply.cases.value,
            false,
            (channel, { code, commandName, suspendHistory, timeoutMs }) =>
                Effect.map(
                    _answered(
                        channel,
                        Option.getOrElse(timeoutMs, () => TIMEOUT_MS),
                        'execute',
                        { code, undoName: Option.none() },
                        Schema.Json,
                        { suspendHistory, commandName },
                    ),
                    ({ value, tookMs }) => ({ kind: 'value' as const, value, tookMs }),
                ),
        ),
        _row(
            'photoshop_batch_play',
            'Plays action descriptors through `batchPlay` inside `executeAsModal` titled `commandName`, `suspendHistory` folding the document changes into one history state. Every fulfilled element is inspected: with `continueOnError` the array stays index-aligned and each `_obj: "error"` element fills a `failed` row, without it the batch stops at the first failing descriptor and the call answers `descriptorFailed`; `result -128` answers `userCancelled`',
            Schema.Struct({ ...BatchPlay.fields, commandName: Schema.String, ..._scoped }),
            Reply.cases.descriptors,
            false,
            (channel, { descriptors, continueOnError, immediateRedraw, commandName, suspendHistory, timeoutMs }) =>
                Effect.map(
                    _answered(
                        channel,
                        Option.getOrElse(timeoutMs, () => TIMEOUT_MS),
                        'batchPlay',
                        { descriptors, continueOnError, immediateRedraw },
                        Descriptors,
                        { suspendHistory, commandName: Option.some(commandName) },
                    ),
                    ({ value, tookMs }) => ({ ...value, tookMs }),
                ),
        ),
        _row(
            'photoshop_snapshot',
            'Renders the composite of a document, its selection mask, or one layer as a base64 JPEG through `imaging.getPixels` at the pixel budget, `region` a normalized `[x0, y0, x1, y1]` of the document, and answers the pixel size, the pyramid level, and the full-resolution source bounds; a success above the spill threshold lands under `.artifacts/creative-cloud/photoshop/results/` and answers the file',
            Snapshot,
            Schema.Union([Reply.cases.jpeg, Reply.cases.file]),
            true,
            (channel, capture) =>
                Effect.flatMap(_answered(channel, TIMEOUT_MS, 'snapshot', capture, Jpeg, _READ), ({ jobId, value }): Effect.Effect<Capture, BridgeError, Services> => {
                    const text = JSON.stringify(value);
                    return text.length > SPILL_CHARS ? spill(_HOST.id, jobId, text) : Effect.succeed(value);
                }),
        ),
        _row(
            'photoshop_get_document',
            "Lists the open documents and reads the active or the named document's mode, depth, profile, pixel size, resolution, and its layer tree flattened in panel order to `depth`, paged by `cursor` and `limit`",
            GetDocument,
            Reply.cases.document,
            true,
            (channel, input) => _value(channel, 'getDocument', input, DocumentState),
        ),
        _row(
            'photoshop_get_preferences',
            'Reads every key of the named `app.preferences` classes, the key set reflected from the class prototype on the host',
            GetPreferences,
            Reply.cases.preferences,
            true,
            (channel, input) => _value(channel, 'getPreferences', input, Preferences),
        ),
        _row(
            'photoshop_set_preferences',
            'Writes `app.preferences.<section>.<key>` rows in order inside `executeAsModal` and reads each back, the typed target table when `values` is absent; a `notifications` write while `quietMode` holds answers `preferenceLocked`, and a readback that neither changed nor matched is a rejected row',
            Schema.Struct({ values: Schema.OptionFromOptionalKey(Writes) }),
            Reply.cases.applied,
            false,
            (channel, { values }) =>
                Effect.map(
                    _answered(channel, TIMEOUT_MS, 'setPreferences', { values: Option.getOrElse(values, () => TARGET_ROWS) }, Applied, {
                        suspendHistory: Option.none(),
                        commandName: Option.some('Apply preferences'),
                    }),
                    Struct.get('value'),
                ),
        ),
        _row(
            'photoshop_list_presets',
            "Reads the application's `presetManager` descriptor and answers the names of the group whose descriptor class names `kind`",
            ListPresets,
            Reply.cases.presets,
            true,
            (channel, input) => _value(channel, 'listPresets', input, Presets),
        ),
        _row(
            'photoshop_run_action',
            'Plays the named action of the named set from `app.actionTree` as one history state of the active document, or inside a bare modal scope while no document is open',
            RunAction,
            Reply.cases.value,
            false,
            (channel, input) => Effect.map(_answered(channel, TIMEOUT_MS, 'runAction', input, Played, _READ), ({ value, tookMs }) => ({ kind: 'value' as const, value, tookMs })),
        ),
        _row(
            'photoshop_system_report',
            'Reads `app.systemInformation` through osascript, the text listing every plugin with its load state, and answers `hostBusy` while a plugin job is in flight',
            Schema.Record(Schema.String, Schema.Never),
            Reply.cases.report,
            true,
            (channel) =>
                Effect.map(
                    Effect.flatMap(
                        probe(channel.host, () => read(_HOST.id, _HOST.bundleId, PROBE_MS, doJavascript('app.systemInformation'), Option.none())),
                        Effect.fromResult,
                    ),
                    (text) => ({ kind: 'report' as const, text }),
                ),
        ),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Links, Jobs]), ([links, jobs]) => ({ link: links.photoshop, host: jobs.photoshop })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
