// --- [IMPORTS] -------------------------------------------------------------------------

import { Measured, MetricsError, metrics } from '@rasm/typography/metrics';
import { Context, Crypto, Duration, Effect, FileSystem, flow, Layer, Option, Path, type PlatformError, Schema, Struct } from 'effect';
import { contract, Failure } from '../contract.ts';
import { BridgeError } from '../errors.ts';
import type { Job } from '../frames.ts';
import { Jobs, run } from '../jobs.ts';
import { attached, dispatch, type Endpoint, Links } from '../socket.ts';
import { AbsolutePath, type JobId, TIMEOUT_MS, TimeoutMs, Undo } from '../values.ts';
import { FAMILIES, FamilyKey } from './families.ts';
import {
    Applied,
    Bodies,
    Capture,
    Enums,
    FindKeyStrings,
    GetLayout,
    GetPreferences,
    Image,
    Keys,
    type Kind,
    Layout,
    ListEnums,
    Preferences,
    Rejected,
    SetPreferences,
    SetTextDefaults,
    Settings,
} from './jobs.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly link: Endpoint;
    readonly host: Jobs['indesign'];
    readonly artifacts: string;
}

type Services = Crypto.Crypto | FileSystem.FileSystem | Path.Path;

interface Answer<Value> {
    readonly value: Value;
    readonly autocorrections: readonly string[];
    readonly tookMs: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = 'indesign';
const _ARTIFACTS = ['..', '..', '..', '..', '.artifacts', 'creative-cloud', _HOST] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Channel: Context.Service<Channel, Channel> = Context.Service<Channel>('InDesignChannel');

const Reply = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, autocorrections: Schema.Array(Schema.String), undo: Undo, tookMs: Schema.Number }),
    Enums,
    Image,
    Layout,
    Keys,
    Preferences,
    Applied,
    Rejected,
    Schema.Struct({ kind: Schema.Literal('metrics'), ...Measured.fields }),
    Schema.Struct({ kind: Schema.Literal('metricsError'), error: MetricsError }),
    Failure,
]).pipe(Schema.toTaggedUnion('kind'));

const _row = contract(Channel, [Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

// --- [DISPATCH] ------------------------------------------------------------------------

const _inaccessible = (error: PlatformError.PlatformError): BridgeError =>
    BridgeError.cases.fileNotAccessible.make({ host: _HOST, path: 'pathOrDescriptor' in error.reason ? String(error.reason.pathOrDescriptor ?? '') : '', reason: error.reason._tag });

const _undecodable =
    (value: Schema.Json) =>
    (error: Schema.SchemaError): BridgeError =>
        BridgeError.cases.resultNotDecodable.make({ host: _HOST, text: JSON.stringify(value), reason: error.message });

const _job =
    <K extends Kind>(kind: K, value: (typeof Bodies)['fields'][K]['Type']) =>
    (jobId: JobId): Job => ({ jobId, kind, body: Schema.encodeSync(Schema.toCodecJson(Bodies.fields[kind]))(value), suspendHistory: Option.none(), commandName: Option.none() });

const _dispatched = <Result extends Schema.ConstraintCodec<unknown, unknown, never, never>>(
    channel: Channel,
    timeoutMs: number,
    result: Result,
    job: (jobId: JobId) => Effect.Effect<Job, BridgeError, Services>,
): Effect.Effect<Answer<Result['Type']>, BridgeError, Services> =>
    Effect.flatMap(
        Effect.timed(
            run(channel.host, timeoutMs, (jobId) =>
                Effect.andThen(
                    attached(channel.link),
                    Effect.flatMap(job(jobId), (built) => dispatch(channel.link, built)),
                ),
            ),
        ),
        ([took, done]) =>
            Effect.map(Effect.mapError(Schema.decodeUnknownEffect(result)(done.value), _undecodable(done.value)), (value) => ({
                value,
                autocorrections: Option.getOrElse(done.autocorrections, () => []),
                tookMs: Duration.toMillis(took),
            })),
    );

const _captured = (channel: Channel, capture: (typeof Capture)['Type'], jobId: JobId): Effect.Effect<Job, BridgeError, Services> =>
    Effect.flatMap(Path.Path, (path) => {
        const directory = AbsolutePath.make(path.join(channel.artifacts, jobId));
        return FileSystem.FileSystem.use((fs) => fs.makeDirectory(directory, { recursive: true })).pipe(Effect.mapError(_inaccessible), Effect.as(_job('snapshot', { ...capture, directory })(jobId)));
    });

const _answer = <K extends Kind, Result extends Schema.ConstraintCodec<unknown, unknown, never, never>>(
    channel: Channel,
    kind: K,
    value: (typeof Bodies)['fields'][K]['Type'],
    result: Result,
): Effect.Effect<Result['Type'], BridgeError, Services> =>
    Effect.map(
        _dispatched(channel, TIMEOUT_MS, result, (jobId) => Effect.succeed(_job(kind, value)(jobId))),
        Struct.get('value'),
    );

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Services> = Layer.provide(
    Layer.mergeAll(
        _row(
            'indesign_execute',
            'Runs `code` as a function body in InDesign UXP after the collection-index and enumeration autocorrect passes and returns its value as JSON. With `undoName` the body is synchronous and runs as one undo step, without it the body can `await`',
            Schema.Struct({ code: Schema.String, undoName: Schema.OptionFromOptionalKey(Schema.String), timeoutMs: Schema.OptionFromOptionalKey(TimeoutMs) }),
            Reply.cases.value,
            false,
            (channel, { code, undoName, timeoutMs }) =>
                Effect.map(
                    _dispatched(
                        channel,
                        Option.getOrElse(timeoutMs, () => TIMEOUT_MS),
                        Schema.Json,
                        (jobId) => Effect.succeed(_job('execute', { code, undoName })(jobId)),
                    ),
                    ({ value, autocorrections, tookMs }) => ({
                        kind: 'value' as const,
                        value,
                        autocorrections,
                        undo: Option.match(undoName, { onNone: () => 'none' as const, onSome: () => 'single' as const }),
                        tookMs,
                    }),
                ),
        ),
        _row(
            'indesign_list_enums',
            'Lists the enumerations the running InDesign registers with their constants and FourCC values, every enumeration or `name` alone',
            ListEnums,
            Reply.cases.enums,
            true,
            (channel, input) => _answer(channel, 'listEnums', input, Enums),
        ),
        _row(
            'indesign_snapshot',
            'Renders a page, a spread, a normalized region of a page, or one page item of the active document to a JPEG or PNG under `.artifacts/` at the resolution the pixel budget gives, and answers the path with its pixel size',
            Capture,
            Reply.cases.image,
            false,
            (channel, capture) =>
                Effect.map(
                    _dispatched(channel, TIMEOUT_MS, Image, (jobId) => _captured(channel, capture, jobId)),
                    Struct.get('value'),
                ),
        ),
        _row(
            'indesign_get_layout',
            "Reads the active document's pages in points: bounds, margins, the content area per page side, guides, and with `includeItems` every page item recursing into groups, paged by `pageCursor`, `itemCursor`, and `limit`",
            GetLayout,
            Reply.cases.layout,
            true,
            (channel, input) => _answer(channel, 'getLayout', input, Layout),
        ),
        _row(
            'indesign_get_font_metrics',
            "Reads a face's metrics from its font file through fontkit, resolved by family key or PostScript name over the font scan roots, with x-height, cap height, and f-height at `size` points",
            Schema.Struct({
                font: Schema.Union([Schema.Struct({ family: FamilyKey }), Schema.Struct({ postScriptName: Schema.String })]),
                size: Schema.Number.pipe(Schema.check(Schema.isGreaterThan(0))),
            }),
            Schema.Union([Reply.cases.metrics, Reply.cases.metricsError]),
            true,
            (_channel, { font, size }) =>
                metrics('family' in font ? FAMILIES[font.family].postScriptName : font.postScriptName, size).pipe(
                    Effect.map((measured) => ({ kind: 'metrics' as const, ...measured })),
                    Effect.catchTag(['fontNotFound', 'metricsMissing', 'faceNotReadable', 'faceNotInFile'], (error) => Effect.succeed({ kind: 'metricsError' as const, error })),
                    Effect.catchTag('PlatformError', flow(_inaccessible, Effect.fail)),
                    Effect.orDie,
                ),
        ),
        _row(
            'indesign_find_key_strings',
            'Answers `app.findKeyStrings(text)`, the `$ID/` key strings behind a user-interface string, beside `app.translateKeyString(text)`',
            FindKeyStrings,
            Reply.cases.keys,
            true,
            (channel, input) => _answer(channel, 'findKeyStrings', input, Keys),
        ),
        _row(
            'indesign_get_preferences',
            'Reads every member of the named `app.<section>` preference objects, enumerators rendered by constant name and DOM objects by name',
            GetPreferences,
            Reply.cases.preferences,
            true,
            (channel, input) => _answer(channel, 'getPreferences', input, Preferences),
        ),
        _row(
            'indesign_set_preferences',
            'Writes `app.<section>.<key>` rows as one undo step while no document is open, resolving a constant name or FourCC to its enumerator, and reads each value back; a readback that neither changed nor matched is a rejected row',
            SetPreferences,
            Settings,
            false,
            (channel, input) => _answer(channel, 'setPreferences', input, Settings),
        ),
        _row(
            'indesign_set_text_defaults',
            "Writes text default rows as one undo step to `app.textDefaults` and `[Basic Paragraph]` at application scope or to the active document's text defaults, each read back",
            SetTextDefaults,
            Reply.cases.applied,
            false,
            (channel, input) => _answer(channel, 'setTextDefaults', input, Applied),
        ),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Links, Jobs, Path.Path]), ([links, jobs, path]) => ({ link: links.indesign, host: jobs.indesign, artifacts: path.resolve(import.meta.dirname, ..._ARTIFACTS) })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
