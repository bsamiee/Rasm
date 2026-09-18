// --- [IMPORTS] -------------------------------------------------------------------------

import { Measured, MetricsError, metrics } from '@rasm/typography/metrics';
import { Crypto, Effect, FileSystem, flow, Layer, Match, Option, Path, Schema, Struct } from 'effect';
import { Toolkit } from 'effect/unstable/ai';
import { answering, forward, host, plain, tool, Value } from '../contract.ts';
import { inaccessible } from '../errors.ts';
import { artifacts, type Jobs } from '../jobs.ts';
import { answer, type Links, prepared, READ, session } from '../socket.ts';
import { AbsolutePath, type JobId, OptionalString, TIMEOUT_MS, TimeoutMs, type Undo } from '../values.ts';
import { Bodies, FAMILIES, Results } from './jobs.ts';

// --- [SERVICES] ------------------------------------------------------------------------

const _session = session('indesign');

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([Crypto.Crypto, FileSystem.FileSystem, Path.Path]);

const _plain = plain(Bodies, Results);

const _toolkit = Toolkit.make(
    _tool(
        'indesign_execute',
        'Runs `code` as a function body in InDesign UXP after the collection-index and enumeration autocorrect passes and returns its value as JSON. With `undoName` the body is synchronous and runs as one undo step, without it the body can `await`',
        Schema.Struct({ code: Schema.String, undoName: OptionalString, timeoutMs: TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS))) }),
        Value,
        false,
    ),
    _plain(
        'indesign_list_enums',
        'listEnums',
        'Lists the enumerations the running InDesign registers with their constants and FourCC values, every enumeration or `name` alone, beside the names `require("indesign")` exports as functions',
        true,
    ),
    _tool(
        'indesign_snapshot',
        'Renders a page, a spread, a normalized region of a page, or one page item of the active document to a JPEG or PNG under `.artifacts/` at the resolution the pixel budget gives, and answers the path with its pixel size',
        Schema.Struct(Struct.omit(Bodies.fields.snapshot.fields, ['directory'])),
        Results.fields.snapshot,
        false,
    ),
    _plain(
        'indesign_get_layout',
        'getLayout',
        "Reads the active document's pages in points: bounds, margins, the content area per page side, guides, and with `includeItems` every page item recursing into groups",
        true,
    ),
    _tool(
        'indesign_get_font_metrics',
        "Reads a face's metrics from its font file through fontkit, resolved by family key or PostScript name over the font scan roots, with x-height, cap height, and f-height at `size` points",
        Schema.Struct({
            font: Schema.Union([
                Schema.Struct({ kind: Schema.Literal('family'), family: Schema.Literals(Struct.keys(FAMILIES)) }),
                Schema.Struct({ kind: Schema.Literal('postScriptName'), postScriptName: Schema.String }),
            ]),
            size: Schema.Number.pipe(Schema.check(Schema.isGreaterThan(0))),
        }),
        Schema.Union([Schema.Struct({ kind: Schema.Literal('metrics'), ...Measured.fields }), Schema.Struct({ kind: Schema.Literal('metricsError'), error: MetricsError })]),
        true,
    ),
    _plain('indesign_find_key_strings', 'findKeyStrings', 'Answers `app.findKeyStrings(text)`, the `$ID/` key strings behind a user-interface string, beside `app.translateKeyString(text)`', true),
    _plain('indesign_get_preferences', 'getPreferences', 'Reads every member of the named `app.<section>` preference objects, enumerators rendered by constant name and DOM objects by name', true),
    _plain(
        'indesign_set_preferences',
        'setPreferences',
        'Writes `app.<section>.<key>` rows as one undo step while no document is open, resolving a constant name or FourCC to its enumerator, and reads each value back; a readback that neither changed nor matched is a rejected row',
        false,
    ),
    _plain(
        'indesign_set_text_defaults',
        'setTextDefaults',
        "Writes text default rows as one undo step to `app.textDefaults` and `[Basic Paragraph]` at application scope or to the active document's text defaults, each read back",
        false,
    ),
);

// --- [DISPATCH] ------------------------------------------------------------------------

const _answer = answer(Bodies);

const _prepared = prepared(Bodies);

const _forward = forward(Bodies, Results);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Crypto.Crypto | FileSystem.FileSystem | Path.Path> = Layer.provide(
    host(_toolkit, _session.tag, (channel) =>
        answering(_toolkit, {
            [_toolkit.tools.indesign_execute.name]: ({ code, undoName, timeoutMs }) => {
                const undo: Option.Option<(typeof Undo)['Type']> = Option.some(Option.match(undoName, { onNone: () => 'none', onSome: () => 'single' }));
                return Effect.map(_answer(channel, timeoutMs, 'execute', { code, undoName }, Results.fields.execute, READ), ({ value, autocorrections, tookMs }) =>
                    Value.make({ kind: 'value', value, tookMs, autocorrections: Option.some(autocorrections), undo }),
                );
            },
            [_toolkit.tools.indesign_list_enums.name]: _forward(channel, 'listEnums', READ),
            [_toolkit.tools.indesign_snapshot.name]: (capture) =>
                Effect.map(
                    _prepared(
                        channel,
                        TIMEOUT_MS,
                        'snapshot',
                        Effect.fnUntraced(function* (jobId: JobId) {
                            const fs = yield* FileSystem.FileSystem;
                            const directory = AbsolutePath.make(yield* artifacts(channel.link.host, jobId));
                            yield* Effect.mapError(fs.makeDirectory(directory, { recursive: true }), inaccessible(channel.link.host));
                            return { ...capture, directory };
                        }),
                        Results.fields.snapshot,
                        READ,
                    ),
                    Struct.get('value'),
                ),
            [_toolkit.tools.indesign_get_layout.name]: _forward(channel, 'getLayout', READ),
            [_toolkit.tools.indesign_get_font_metrics.name]: ({ font, size }) =>
                metrics(Match.value(font).pipe(Match.discriminatorsExhaustive('kind')({ family: ({ family }) => FAMILIES[family], postScriptName: Struct.get('postScriptName') })), size).pipe(
                    Effect.map((measured) => ({ kind: 'metrics' as const, ...measured })),
                    Effect.catchIf(Schema.is(MetricsError), (error) => Effect.succeed({ kind: 'metricsError' as const, error })),
                    Effect.catchTag('PlatformError', flow(inaccessible(channel.link.host), Effect.fail)),
                    Effect.catchTag(['ConfigError', 'SchemaError'], Effect.die),
                ),
            [_toolkit.tools.indesign_find_key_strings.name]: _forward(channel, 'findKeyStrings', READ),
            [_toolkit.tools.indesign_get_preferences.name]: _forward(channel, 'getPreferences', READ),
            [_toolkit.tools.indesign_set_preferences.name]: _forward(channel, 'setPreferences', READ),
            [_toolkit.tools.indesign_set_text_defaults.name]: _forward(channel, 'setTextDefaults', READ),
        }),
    ),
    _session.layer,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
