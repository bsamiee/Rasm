// --- [IMPORTS] -------------------------------------------------------------------------

import type { McpUiToolMeta } from '@modelcontextprotocol/ext-apps';
import { bolder, catalogue, FontError, type FontRequest, FontSelector, inspect, metrics, select } from '@rasm/typography/fonts';
import { CodePoint, CSS, FontFace, Measured, MetricsError } from '@rasm/typography/metrics';
import {
    Array,
    Crypto,
    Effect,
    Equal,
    Equivalence,
    FileSystem,
    Function,
    flow,
    HashMap,
    identity,
    Layer,
    Number,
    Option,
    Order,
    Path,
    pipe,
    Record,
    Result,
    Schema,
    String,
    Struct,
    Tuple,
} from 'effect';
import { Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { answering, forward, type Handlers, plain, register, tool, Value } from '../contract.ts';
import { exited, inaccessible, notDecodable } from '../errors.ts';
import { artifacts, type Jobs } from '../jobs.ts';
import { reply } from '../osascript.ts';
import { fourcc } from '../sdef.ts';
import { type Links, prepared, READ, session } from '../socket.ts';
import { AbsolutePath, type JobId, OptionalString, TIMEOUT_MS, TimeoutMs, type Undo } from '../values.ts';
import { Bodies, FontSource, PAIRINGS, Results, TYPOGRAPHY_ANCHORS, TYPOGRAPHY_ROLES, TypographyError } from './jobs.ts';
import { persistPublishing } from './publishing.ts';
import { SNAPSHOT_VIEW } from './resources.ts';

// --- [SERVICES] ------------------------------------------------------------------------

const _session = session('indesign');

// --- [FONT SOURCES] --------------------------------------------------------------------

const _faceIdentity = Equivalence.Struct({ digest: Equivalence.String, index: Equivalence.Number });
const _RegistrationFailure = Schema.Union([
    Schema.Struct({ fontAttribute: Schema.Struct({ index: Schema.Int, attribute: Schema.Literals(['postScriptName', 'file', 'axes', 'nameTable']) }) }),
    Schema.Struct({ fontSubstitution: Schema.Struct({ index: Schema.Int, expectedName: Schema.String, actualName: Schema.String, expectedFile: Schema.String, actualFile: Schema.String }) }),
]);
const _Registry = Schema.Struct({
    accepted: Schema.Array(
        Schema.Struct({
            ...Struct.pick(FontFace.fields, ['postScriptName', 'nameTableDigest']),
            file: AbsolutePath,
            axes: Schema.Array(Schema.Struct({ id: Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 0xff_ff_ff_ff }))), value: Schema.Finite })),
        }),
    ),
    rejected: Schema.Array(_RegistrationFailure),
});
const _registered = reply(ChildProcess.make('NativeHost', ['fonts'])).pipe(
    Effect.mapError(exited('indesign')),
    Effect.flatMap((output) => Schema.decodeEffect(Schema.fromJsonString(_Registry))(output).pipe(Effect.mapError(notDecodable('indesign', output)))),
);

const _available = Effect.fnUntraced(function* (registry: typeof _Registry.Type, files: readonly string[]) {
    const [unreadable, opened] = yield* Effect.partition(Array.dedupe([...Array.map(registry.accepted, Struct.get('file')), ...files]), inspect);
    const faces = Array.flatMap(opened, Struct.get('faces'));
    const byFile = Array.groupBy(faces, Struct.get('file'));
    const [missing, bindings] = Array.partition(registry.accepted, (registration) =>
        Result.gen(function* () {
            const request = { postScriptName: registration.postScriptName, axes: {}, codePoints: [] };
            const candidates = Array.filter(Option.getOrElse(Record.get(byFile, registration.file), Array.empty), flow(Struct.get('nameTableDigest'), Equal.equals(registration.nameTableDigest)));
            const face = yield* Result.fromOption(Option.filter(Array.head(candidates), Function.constant(candidates.length === 1)), () =>
                Array.isArrayNonEmpty(candidates) ? FontError.cases.fontAmbiguous.make({ request, faces: candidates }) : FontError.cases.fontUnavailable.make({ request }),
            );
            const axes = HashMap.fromIterable(Array.map(registration.axes, ({ id, value }) => [id, value] as const));
            const invalid = Array.map(Array.difference(HashMap.keys(axes), Array.map(Record.keys(face.axes), fourcc)), globalThis.String);
            if (Array.isArrayNonEmpty(invalid)) {
                return yield* Result.fail(MetricsError.cases.invalidFontCoordinates.make({ file: registration.file, axes: invalid }));
            }
            const coordinates = Record.map(face.axes, (axis, tag) =>
                Number.clamp(Option.getOrElse(HashMap.get(axes, fourcc(tag)), Function.constant(axis.default)), { minimum: axis.min, maximum: axis.max }),
            );
            return { face, coordinates, registration };
        }),
    );
    return {
        faces,
        bindings,
        rejected: [...registry.rejected, ...unreadable, ...Array.flatMap(opened, Struct.get('rejected')), ...missing],
    };
});

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([Crypto.Crypto, FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);

const _plain = plain(Bodies, Results);
const _Directories = Schema.OptionFromOptionalKey(Schema.NonEmptyArray(AbsolutePath));
const _FontFailure = Schema.Union([FontError, MetricsError]);
const _Sources = Schema.Array(Schema.Union([FontError, MetricsError, _RegistrationFailure, Results.fields.fontSources.fields.rejected.value]));
const _FontFailures = Schema.Struct({ kind: Schema.Literal('rejected'), errors: Schema.NonEmptyArray(_FontFailure), rejected: _Sources });
const _BuildResult = Schema.Union([
    Schema.Struct({ ...Results.fields.buildTypography.cases.completed.fields, rejected: _Sources }),
    Schema.Struct({
        ...Results.fields.buildTypography.cases.rejected.fields,
        errors: Schema.NonEmptyArray(Schema.Union([Results.fields.buildTypography.cases.rejected.fields.errors.value, FontError])),
        rejected: _Sources,
    }),
]);
const _anchors = Record.map(TYPOGRAPHY_ROLES, (definition, role) =>
    Array.dedupe([
        ...('alignment' in definition ? [definition.alignment.glyph] : []),
        ...(role === 'body' ? [TYPOGRAPHY_ANCHORS.ascender, ...Array.flatMap(Record.values(TYPOGRAPHY_ROLES), (entry) => ('alignment' in entry ? [entry.alignment.reference] : []))] : []),
    ]),
);
const _timeout = TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS)));

const _toolkit = Toolkit.make(
    _plain(
        'indesign_apply_grid',
        'applyGrid',
        'Applies structured or imported preset geometry to explicit document pages or parents, including margins, unequal columns, subdivisions and baseline/document grids. Reads back native geometry and reports rejected targets',
        false,
    ),
    _tool(
        'indesign_build_typography',
        'Builds the digital default template or full size catalogue from active InDesign font identities, registered source files, variable coordinates and measured glyph anchors. Supply native language IDs for latin, persian and arabic; each pairing gets its own output directory. Each palette swatch has an sRGB triple and optional group; Accent, Ink and Field are required. Rejected font sources accompany results; font, language, palette, metric and geometry failures are reported before their dependent writes',
        Schema.Struct({
            ...Struct.omit(Bodies.fields.buildTypography.fields, ['fonts']),
            timeoutMs: _timeout,
        }),
        _BuildResult,
        false,
    ),
    _tool(
        'indesign_execute',
        'Runs `code` as a function body in InDesign UXP after the collection-index and enumeration autocorrect passes and returns its value as JSON. With `undoName` the body is synchronous and runs as one undo step, without it the body can `await`',
        Schema.Struct({ code: Schema.String, undoName: OptionalString, timeoutMs: _timeout }),
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
        'Renders a page, spread, normalized page region or page item to JPEG or PNG at the pixel-budget resolution. Returns the artifact, dimensions and image, with fit, pan and zoom in MCP Apps hosts',
        Schema.Struct(Struct.omit(Bodies.fields.snapshot.fields, ['directory'])),
        Results.fields.snapshot,
        false,
    ).annotate(Tool.Meta, { ui: { resourceUri: SNAPSHOT_VIEW } satisfies McpUiToolMeta }),
    _plain(
        'indesign_get_layout',
        'getLayout',
        'Reads selected document pages in page-local points: bounds, margins, content areas, and guide endpoints. `includeItems` returns paginated page items, including nested groups',
        true,
    ),
    _tool(
        'indesign_get_font_metrics',
        'Selects an exact face, PostScript or full name, or matches family traits, variation axes and Unicode coverage. Returns face metadata, variation-aware OpenType metrics and nominal glyph extents for codePoints. Uses a fresh OS font registration inventory by default; optional directories select an offline catalogue. Rejected sources accompany measurements and failures',
        Schema.Struct({ directories: _Directories, request: FontSelector, size: Schema.Finite.pipe(Schema.check(Schema.isGreaterThan(0))), codePoints: Schema.Array(CodePoint) }),
        Schema.Union([Schema.Struct({ kind: Schema.Literal('metrics'), ...Measured.fields, rejected: _Sources }), _FontFailures]),
        true,
    ),
    _plain('indesign_find_key_strings', 'findKeyStrings', 'Answers `app.findKeyStrings(text)`, the `$ID/` key strings behind a user-interface string, beside `app.translateKeyString(text)`', true),
    _plain('indesign_get_preferences', 'getPreferences', 'Reads every member of the named `app.<section>` preference objects, enumerators rendered by constant name and DOM objects by name', true),
    _plain(
        'indesign_set_preferences',
        'setPreferences',
        'Writes `app.<section>.<key>` rows while no document is open, resolving native enumerators and measurement values, then compares every final readback with the intended value and reports rejected rows',
        false,
    ),
    _plain(
        'indesign_set_text_defaults',
        'setTextDefaults',
        "Writes text defaults to `app.textDefaults` and `[Basic Paragraph]` at application scope or to the active document's text defaults, then reads each value back. Native text-default assignments do not create undo history",
        false,
    ),
    _tool(
        'indesign_publishing',
        'Inspects or configures native preflight profiles, output presets, saved queries and dictionaries; runs document preflight; imports PDF feedback or changes explicit review statuses. Changing comment status does not apply its edit',
        Bodies.fields.publishing,
        Results.fields.publishing,
        false,
    ),
    _plain(
        'indesign_edit_document',
        'editDocument',
        'Applies native text direction, numbering, special characters or language-specific RTL defaults and styles, or runs scoped Text/GREP queries with change counts and restored search preferences',
        false,
    ),
    _plain('indesign_read_library', 'readLibrary', 'Reads a native InDesign library and its asset identities, preserving libraries already open in the application', true),
    _plain('indesign_write_library', 'writeLibrary', 'Stores identified document items as native library assets with metadata and reports successful and rejected assets independently', false),
    _plain(
        'indesign_place_library_asset',
        'placeLibraryAsset',
        'Places an identified native library asset into a document or at a story insertion point and returns the resulting item identities',
        false,
    ),
    _plain('indesign_write_snippet', 'writeSnippet', 'Exports an identified document item as a native InDesign snippet', false),
    _plain('indesign_place_snippet', 'placeSnippet', 'Places an InDesign snippet on the requested page, with optional position and layer, and returns the resulting item identities', false),
    _plain('indesign_import_styles', 'importStyles', 'Imports selected native style families from a document with the caller-selected collision policy and returns the resulting styles', false),
    _plain('indesign_set_export_tags', 'setExportTags', 'Applies caller-supplied export mappings to exact paragraph and character style identities and reads the mappings back', false),
    _plain('indesign_set_layers', 'setLayers', 'Applies an ordered layer configuration and reads back native layer identities, order and properties', false),
);

// --- [DISPATCH] ------------------------------------------------------------------------

const _prepared = prepared(Bodies);

const _forward = forward(Bodies, Results);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Crypto.Crypto | FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> = Layer.provide(
    Layer.provide(
        register(_toolkit),
        _toolkit.toLayer(
            Effect.map(_session.tag, (channel) =>
                Struct.map(
                    {
                        [_toolkit.tools.indesign_apply_grid.name]: _forward(channel, 'applyGrid', READ),
                        [_toolkit.tools.indesign_build_typography.name]: Effect.fnUntraced(
                            function* (input: typeof _toolkit.tools.indesign_build_typography.parametersSchema.Type) {
                                const path = yield* Path.Path;
                                const [{ value: native }, registry] = yield* Effect.all(
                                    [_prepared(channel, input.timeoutMs, 'fontSources', () => Effect.succeed({}), Results.fields.fontSources, READ), _registered],
                                    { concurrency: 'unbounded' },
                                );
                                const available = yield* _available(registry, Array.filter(Array.map(native.fonts, Struct.get('location')), path.isAbsolute));
                                const registered = Array.groupBy(available.bindings, ({ registration }) => String.toLowerCase(registration.postScriptName));
                                const byFile = Array.groupBy(available.faces, Struct.get('file'));
                                const [unbound, bindings] = yield* Effect.partition(
                                    native.fonts,
                                    Effect.fnUntraced(function* (font: typeof Results.fields.fontSources.fields.fonts.value.Type) {
                                        const request = { postScriptName: font.postscriptName, axes: {}, codePoints: [] };
                                        const unavailable = FontError.cases.fontUnavailable.make({ request });
                                        const candidates = path.isAbsolute(font.location)
                                            ? Option.getOrElse(Record.get(byFile, font.location), Array.empty)
                                            : Array.map(Option.getOrElse(Record.get(registered, String.toLowerCase(font.postscriptName)), Array.empty), Struct.get('face'));
                                        const compatible = pipe(
                                            candidates,
                                            Array.dedupeWith(_faceIdentity),
                                            Array.filter(
                                                flow(
                                                    Struct.get('axes'),
                                                    Record.values,
                                                    Array.sortWith(Struct.get('axisIndex'), Order.Number),
                                                    Array.map(Struct.pick(['min', 'max'])),
                                                    Array.map(Record.values),
                                                    Equal.equals(Option.match(font.axes, { onNone: Array.empty, onSome: Struct.get('designAxesRange') })),
                                                ),
                                            ),
                                        );
                                        const face = yield* Effect.fromResult(
                                            compatible.length === 1
                                                ? Result.fromOption(Array.head(compatible), Function.constant(unavailable))
                                                : Result.map(select(compatible, request), Struct.get('face')),
                                        );
                                        const axes = Array.sortWith(Record.toEntries(face.axes), flow(Tuple.get(1), Struct.get('axisIndex')), Order.Number);
                                        const coordinates = Record.fromEntries(
                                            Array.zip(Array.map(axes, Tuple.get(0)), Option.match(font.axes, { onNone: Array.empty, onSome: Struct.get('designAxesValues') })),
                                        );
                                        const selection = yield* Effect.fromResult(select([face], { ...Struct.pick(face, ['digest', 'index']), axes: coordinates, codePoints: [] }));
                                        return { ...selection, native: Struct.omit(font, ['axes']) };
                                    }),
                                );
                                const faces = pipe(bindings, Array.map(Struct.get('face')), Array.dedupeWith(_faceIdentity));
                                const nativeByFace = Array.reduce(bindings, HashMap.empty<Pick<FontFace, 'digest' | 'index'>, typeof bindings>(), (grouped, binding) =>
                                    HashMap.modifyAt(grouped, Struct.pick(binding.face, ['digest', 'index']), flow(Option.getOrElse(Array.empty), Array.append(binding), Option.some)),
                                );
                                const rejected = [...available.rejected, ...native.rejected, ...unbound];
                                const selections = Record.map(TYPOGRAPHY_ROLES, ({ family, italic, bolder: bold }) => {
                                    const primary = TYPOGRAPHY_ROLES[family];
                                    const request: FontRequest = {
                                        family: PAIRINGS[input.pairing][family],
                                        weight: CSS.weight.normal,
                                        width: CSS.width,
                                        angle: 0,
                                        italic,
                                        axes: 'axes' in primary ? primary.axes : {},
                                        codePoints: _anchors[family],
                                    };
                                    const selected = bold ? bolder(faces, request) : select(faces, request);
                                    return Result.isFailure(selected) ||
                                        !italic ||
                                        Option.getOrElse(Record.get(selected.success.coordinates, 'ital'), Function.constant(selected.success.face.italic ? 1 : 0)) > 0 ||
                                        Option.getOrElse(
                                            Record.get(selected.success.coordinates, 'slnt'),
                                            Function.constant(Record.has(selected.success.face.axes, 'ital') ? 0 : selected.success.face.angle),
                                        ) !== 0
                                        ? selected
                                        : Result.fail(FontError.cases.fontUnavailable.make({ request }));
                                });
                                const resolved = yield* Effect.all(
                                    Record.map(
                                        selections,
                                        Effect.fnUntraced(function* (selection: (typeof selections)[keyof typeof selections], role: keyof typeof selections) {
                                            const { face, coordinates } = yield* Effect.fromResult(selection);
                                            const nativeBindings = Option.getOrElse(HashMap.get(nativeByFace, Struct.pick(face, ['digest', 'index'])), Array.empty);
                                            const exact = Array.filter(
                                                nativeBindings,
                                                flow(Struct.get('native'), Struct.get('postscriptName'), String.toLowerCase, Equal.equals(String.toLowerCase(face.postScriptName))),
                                            );
                                            const matching = Array.isArrayNonEmpty(exact)
                                                ? exact
                                                : Array.filter(nativeBindings, flow(Struct.get('coordinates'), Equal.equals({ ...Record.map(face.axes, Struct.get('default')), ...coordinates })));
                                            const candidates = Array.dedupeWith(Array.map(matching, Struct.get('native')), Schema.toEquivalence(FontSource));
                                            const source = yield* Option.filter(Array.head(candidates), Function.constant(candidates.length === 1)).pipe(
                                                Effect.fromOption(Function.constant(TypographyError.cases.nativeFontSelection.make({ role, postScriptName: face.postScriptName, candidates }))),
                                            );
                                            return { ...(yield* metrics(face, coordinates, Option.none(), _anchors[role])), native: source };
                                        }),
                                    ),
                                    { mode: 'result', concurrency: 'unbounded' },
                                );
                                const [errors] = Array.partition(Record.values(resolved), identity);
                                if (Array.isArrayNonEmpty(errors)) {
                                    return { kind: 'rejected' as const, errors, rejected };
                                }
                                const fonts = yield* Effect.fromResult(Result.all(resolved)).pipe(Effect.orDie);
                                const fs = yield* FileSystem.FileSystem;
                                const destination = path.join(input.directory, input.pairing);
                                yield* fs.makeDirectory(path.join(destination, 'Sizes', 'Book'), { recursive: true });
                                const directory = AbsolutePath.make(yield* fs.realPath(destination));
                                const response = yield* _prepared(
                                    channel,
                                    input.timeoutMs,
                                    'buildTypography',
                                    () => Effect.succeed({ ...Struct.omit(input, ['timeoutMs']), directory, fonts }),
                                    Results.fields.buildTypography,
                                    READ,
                                );
                                return { ...response.value, rejected };
                            },
                            Effect.catchTag('PlatformError', flow(inaccessible(channel.link.host), Effect.fail)),
                        ),
                        [_toolkit.tools.indesign_execute.name]: ({ code, undoName, timeoutMs }) => {
                            const undo: Option.Option<(typeof Undo)['Type']> = Option.some(Option.match(undoName, { onNone: () => 'none', onSome: () => 'single' }));
                            return Effect.map(
                                _prepared(channel, timeoutMs, 'execute', () => Effect.succeed({ code, undoName }), Results.fields.execute, READ),
                                ({ value, autocorrections, tookMs }) => Value.make({ kind: 'value', value, tookMs, autocorrections: Option.some(autocorrections), undo }),
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
                        [_toolkit.tools.indesign_get_font_metrics.name]: Effect.fnUntraced(function* ({
                            directories,
                            request,
                            size,
                            codePoints,
                        }: typeof _toolkit.tools.indesign_get_font_metrics.parametersSchema.Type) {
                            const scanned = yield* Option.match(directories, {
                                onNone: Function.constant(
                                    _registered.pipe(
                                        Effect.flatMap((registry) => _available(registry, [])),
                                        Effect.map((available) => Result.succeed({ ...available, faces: pipe(available.bindings, Array.map(Struct.get('face')), Array.dedupeWith(_faceIdentity)) })),
                                    ),
                                ),
                                onSome: flow(catalogue, Effect.result),
                            });
                            if (Result.isFailure<Result.Result.Success<typeof scanned>, Result.Result.Failure<typeof scanned>>(scanned)) {
                                return { kind: 'rejected' as const, errors: scanned.failure, rejected: scanned.failure };
                            }
                            const { faces, rejected } = scanned.success;
                            const selector = { ...request, codePoints: Array.dedupe([...request.codePoints, ...codePoints]) };
                            const registered =
                                'bindings' in scanned.success && 'postScriptName' in request
                                    ? Array.filter(scanned.success.bindings, ({ registration }) => String.toLowerCase(registration.postScriptName) === String.toLowerCase(request.postScriptName))
                                    : [];
                            const matching = pipe(
                                registered,
                                Array.filterMap(({ face, coordinates }) =>
                                    select([face], { ...Struct.pick(face, ['digest', 'index']), axes: { ...coordinates, ...selector.axes }, codePoints: selector.codePoints }),
                                ),
                                Array.dedupeWith((left, right) => _faceIdentity(left.face, right.face) && Equal.equals(left.coordinates, right.coordinates)),
                            );
                            const selected = Array.isArrayNonEmpty(registered)
                                ? Result.fromOption(Option.filter(Array.head(matching), Function.constant(matching.length === 1)), () =>
                                      Array.isArrayNonEmpty(matching)
                                          ? FontError.cases.fontAmbiguous.make({ request: selector, faces: Array.map(matching, Struct.get('face')) })
                                          : FontError.cases.fontUnavailable.make({ request: selector }),
                                  )
                                : select(faces, selector);
                            if (Result.isFailure(selected)) {
                                return { kind: 'rejected' as const, errors: Array.of(selected.failure), rejected };
                            }
                            const measured = yield* Effect.result(metrics(selected.success.face, selected.success.coordinates, Option.some(size), codePoints));
                            return Result.match(measured, {
                                onFailure: (error) => ({ kind: 'rejected' as const, errors: Array.of(error), rejected }),
                                onSuccess: (value) => ({ kind: 'metrics' as const, ...value, rejected }),
                            });
                        }),
                        [_toolkit.tools.indesign_find_key_strings.name]: _forward(channel, 'findKeyStrings', READ),
                        [_toolkit.tools.indesign_get_preferences.name]: _forward(channel, 'getPreferences', READ),
                        [_toolkit.tools.indesign_set_preferences.name]: _forward(channel, 'setPreferences', READ),
                        [_toolkit.tools.indesign_set_text_defaults.name]: _forward(channel, 'setTextDefaults', READ),
                        [_toolkit.tools.indesign_publishing.name]: (input) => _forward(channel, 'publishing', READ)(input).pipe(Effect.flatMap((output) => persistPublishing(input, output))),
                        [_toolkit.tools.indesign_edit_document.name]: _forward(channel, 'editDocument', READ),
                        [_toolkit.tools.indesign_read_library.name]: _forward(channel, 'readLibrary', READ),
                        [_toolkit.tools.indesign_write_library.name]: _forward(channel, 'writeLibrary', READ),
                        [_toolkit.tools.indesign_place_library_asset.name]: _forward(channel, 'placeLibraryAsset', READ),
                        [_toolkit.tools.indesign_write_snippet.name]: _forward(channel, 'writeSnippet', READ),
                        [_toolkit.tools.indesign_place_snippet.name]: _forward(channel, 'placeSnippet', READ),
                        [_toolkit.tools.indesign_import_styles.name]: _forward(channel, 'importStyles', READ),
                        [_toolkit.tools.indesign_set_export_tags.name]: _forward(channel, 'setExportTags', READ),
                        [_toolkit.tools.indesign_set_layers.name]: _forward(channel, 'setLayers', READ),
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
