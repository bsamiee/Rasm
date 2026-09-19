// --- [IMPORTS] -------------------------------------------------------------------------

import { HSL, LCH, mix, OKLab, OKLCH, range, sRGB, toGamut, XYZ_D65 } from 'colorjs.io/fn';
import { Array, Crypto, Effect, FileSystem, Function, flow, HashMap, Layer, Match, Number, Option, Order, Path, Predicate, Random, Record, Result, Schema, Stream, Struct, Tuple } from 'effect';
import { type Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import sharp from 'sharp';
import { answering, type Handlers, type Member, type Reply, register, succeeded, tool } from '../contract.ts';
import { compare, Drift, fingerprint, Observation } from '../drift.ts';
import { type BridgeError, inaccessible, notDecodable } from '../errors.ts';
import { Hosts } from '../hosts.ts';
import { BUDGET, PixelBudget } from '../images.ts';
import { Jobs, request, run, submit } from '../jobs.ts';
import { Color, Palette } from '../palette.ts';
import { AbsolutePath, CHANNELS, HOSTS, Ink, type JobId, OptionalString, PageIndex, TIMEOUT_MS, TimeoutMs } from '../values.ts';
import { dispatch, site, Unavailable } from './channel.ts';
import { classes, enumerations } from './dictionary.ts';
import { Cad, CadPlan, CadRejection, CadState, planCad } from './workflows.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _gradientSpaces = { oklab: OKLab, oklch: OKLCH, lch: LCH, hsl: HSL };
const _SPOT_NAME_CODEPOINTS = 31;
const _timeout = { timeoutMs: TimeoutMs.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TIMEOUT_MS))) };
const _unavailable = { unavailable: Schema.Array(Unavailable) };
const _baseline = Schema.OptionFromOptionalKey(Schema.JsonObject);
const _drift = Schema.OptionFromOptionalKey(Drift);
const _scalar = Schema.Union([Schema.Boolean, Schema.Number, Schema.String]);
const _positive = Schema.Number.pipe(Schema.check(Schema.isGreaterThan(0)));
const _count = Schema.Int.check(Schema.isGreaterThan(0));
const _distance = Schema.Number.check(Schema.isGreaterThanOrEqualTo(0));
const _point = Schema.Tuple([Schema.Number, Schema.Number]);
const _bounds = Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]);
const _optionalValue = Schema.OptionFromOptionalKey(Schema.Json);
const _identifiers = Schema.NonEmptyArray(Schema.NonEmptyString);
const _textAttributes = { character: classes.CharacterAttributes, paragraph: classes.ParagraphAttributes };
const _percentage = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: CHANNELS.percent })));
const _preference = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('bool'), key: Schema.NonEmptyString, value: Schema.Boolean }),
    Schema.Struct({ kind: Schema.Literal('integer'), key: Schema.NonEmptyString, value: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('real'), key: Schema.NonEmptyString, value: Schema.Number }),
    Schema.Struct({ kind: Schema.Literal('string'), key: Schema.NonEmptyString, value: Schema.String }),
]).pipe(Schema.toTaggedUnion('kind'));
const _supportedColor = (color: (typeof Color)['Type']): boolean =>
    color.model !== 'Spot' || (Array.fromIterable(color.name).length <= _SPOT_NAME_CODEPOINTS && (color.colorType === 'SPOT' || color.ink.model !== 'LAB'));
const _color = Color.check(Schema.makeFilter(_supportedColor, { description: 'Illustrator spot names contain at most 31 Unicode code points; Lab ink requires a spot color' }));
const _palette = Palette.check(
    Schema.makeFilter((palette) => Array.every([...palette.root, ...Array.flatMap(palette.groups, Struct.get('swatches'))], flow(Struct.get('color'), _supportedColor)), {
        description: 'Every paint satisfies Illustrator spot-name and Lab-ink requirements',
    }),
);
const _swatchRead = Schema.Struct({
    kind: Schema.Literal('colors'),
    rows: Schema.Array(Schema.Struct({ document: AbsolutePath, name: Schema.String, ink: Ink })),
    ..._unavailable,
});
const _swatchRow = { name: Schema.String, group: OptionalString };
const _swatchResults = {
    applied: Schema.Array(Schema.Struct(_swatchRow)),
    rejected: Schema.Array(Schema.Struct({ ..._swatchRow, reason: Schema.Literals(['nameCollision', 'resourceTypeConflict', 'reservedColor', 'colorDefinitionConflict', 'unsupportedColor']) })),
};
const _paintRejection = Schema.Struct({ color: Schema.String, reason: Schema.Literals(['colorDefinitionConflict', 'unsupportedColor']) });
const _styleColorFailure = Schema.Struct({ name: Schema.String, ..._paintRejection.fields });
const _strokeAppearance = Schema.Struct({
    name: Schema.NonEmptyString,
    weight: _positive,
    dash: Schema.Array(_distance),
    cap: Schema.Literals(enumerations.StrokeCap),
    join: Schema.Literals(enumerations.StrokeJoin),
    miterLimit: _positive,
    strokeColor: _color,
    fillColor: Schema.Array(_color).check(Schema.isMaxLength(1)),
    blendMode: Schema.Literals(enumerations.BlendModes),
});
const _raster = Schema.Struct({ resolution: _positive, antiAliasing: Schema.Boolean, padding: _distance });
const _colorSpace = Schema.Literals(enumerations.DocumentColorSpace);
const _artboardProperties = Schema.Struct({
    rulerOrigin: Schema.OptionFromOptionalKey(Schema.Tuple([Schema.Number, Schema.Number])),
    rulerPAR: Schema.OptionFromOptionalKey(Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0.1, maximum: 10 })))),
    ...Record.map({ showCenter: Schema.Boolean, showCrossHairs: Schema.Boolean, showSafeAreas: Schema.Boolean }, Schema.OptionFromOptionalKey),
});
const _artboard = Schema.Union([Schema.Struct({ index: PageIndex }), Schema.Struct({ active: Schema.Literal(true) })]);
const _rtlWrite = Schema.Union(
    Array.flatMap(Record.toEntries(_textAttributes), ([scope, attributes]) =>
        Array.flatMap(Record.toEntries<string, { readonly access: string; readonly type: string }>(attributes), ([property, member]) => {
            if (member.access !== 'readwrite') {
                return [];
            }
            const value =
                member.type === 'number' || member.type === 'boolean'
                    ? Schema.Struct({ literal: member.type === 'number' ? Schema.Number : Schema.Boolean })
                    : Option.match(
                          Array.findFirst(Record.toEntries(enumerations), ([name]) => name === member.type),
                          {
                              onSome: ([name, members]) => Schema.Struct({ enumeration: Schema.Literal(name), member: Schema.Literals(members) }),
                              onNone: () => Schema.Never,
                          },
                      );
            return value === Schema.Never ? [] : [Schema.Struct({ scope: Schema.Literal(scope), property: Schema.Literal(property), value })];
        }),
    ),
);
const _stop = Schema.Struct({ position: _percentage, midpoint: Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 13, maximum: 87 }))), opacity: _percentage, color: _color });
const _gradientIdentity = { gradient: Schema.String, objects: Schema.Array(Schema.String) };
const _gradientRead = Schema.Struct({
    kind: Schema.Literal('applied'),
    applied: Schema.Array(
        Schema.Struct({
            ..._gradientIdentity,
            stops: Schema.NonEmptyArray(Schema.Struct({ ..._stop.fields, rgb: Ink.members[0].fields.values })).pipe(Schema.check(Schema.isMinLength(2))),
        }),
    ),
    rejected: Schema.Array(Schema.Struct({ ..._gradientIdentity, reason: Schema.Literal('unreadableStop') })),
    ..._unavailable,
});
const _gradientWrite = Schema.Struct({
    mode: Schema.Literal('write'),
    attributes: Schema.Array(Schema.Literals(['fill', 'stroke'])),
    gradients: Schema.Array(Schema.Struct({ name: Schema.String, stops: Schema.NonEmptyArray(_stop) })),
});
const _guideDraw = Schema.Union([
    Schema.Struct({
        operation: Schema.Literal('lockup'),
        source: Schema.NonEmptyString,
        layerName: Schema.NonEmptyString,
        color: _color,
        weight: _positive,
        filled: Schema.Boolean,
        lines: Schema.Array(Schema.Struct({ from: _point, to: _point })),
        labels: Schema.Array(Schema.Struct({ text: Schema.String, size: _positive, left: Schema.Number, top: Schema.Number, vertical: Schema.Boolean, width: _distance })),
        copies: Schema.Array(Schema.Struct({ position: _point, rotate: Schema.Number, anchor: Schema.Literals(['top', 'bottom']) })),
    }),
    Schema.Struct({
        operation: Schema.Literal('object'),
        layerName: Schema.NonEmptyString,
        clearLayer: Schema.Boolean,
        color: _color,
        weight: _positive,
        edges: Schema.Struct({
            left: Schema.Boolean,
            top: Schema.Boolean,
            right: Schema.Boolean,
            bottom: Schema.Boolean,
            centerX: Schema.Boolean,
            centerY: Schema.Boolean,
            leftDiagonal: Schema.Boolean,
            rightDiagonal: Schema.Boolean,
        }),
        margins: Schema.Struct({ left: Schema.Number, right: Schema.Number, top: Schema.Number, bottom: Schema.Number }),
        extendTo: Schema.Literals(['object', 'artboard', 'artwork']),
        extension: Schema.Struct({ mode: Schema.Literals(['fraction', 'absolute']), value: Schema.Number }),
        bounds: Schema.Literals(['geometric', 'visible']),
        drawAs: Schema.Literals(['guides', 'strokedPaths']),
    }),
    Schema.Struct({
        operation: Schema.Literal('bento'),
        cells: Schema.NonEmptyArray(Schema.Struct({ x: Schema.Number, y: Schema.Number, w: _positive, h: _positive })),
        cornerRadius: _distance,
        groupName: Schema.NonEmptyString,
        removeSelection: Schema.Boolean,
    }),
    Schema.Struct({
        operation: Schema.Literal('isometric'),
        layerName: Schema.NonEmptyString,
        sublayerTemplate: Schema.NonEmptyString,
        spacing: _positive,
        angle: Schema.Number.check(Schema.isBetween({ minimum: 0, maximum: 90, exclusiveMinimum: true, exclusiveMaximum: true })),
        artboards: Schema.Union([Schema.Literal('all'), Schema.NonEmptyArray(PageIndex)]),
    }),
]).pipe(Schema.toTaggedUnion('operation'));
const _countRange = Schema.Tuple([_count, _count]).check(Schema.makeFilter(([minimum, maximum]) => minimum <= maximum, { description: 'Inclusive minimum and maximum counts' }));
const _fractionRange = Schema.Tuple([_positive, _positive]).check(
    Schema.makeFilter(([minimum, maximum]) => minimum <= maximum && maximum < 1, { description: 'Ordered fractions strictly between zero and one' }),
);
const _guideMeasurementFailure = Schema.Union([
    Schema.Struct({ uuid: Schema.String, count: PageIndex, reason: Schema.Literal('conversionChangedFrameCount') }),
    Schema.Struct({ uuid: Schema.String, reason: Schema.Literal('clippingPathMissing') }),
]);

// --- [TOOLS] ---------------------------------------------------------------------------

const _tool = tool([ChildProcessSpawner.ChildProcessSpawner, Crypto.Crypto, FileSystem.FileSystem, Path.Path, Hosts, Jobs]);

const _toolkit = Toolkit.make(
    _tool(
        'illustrator_execute',
        'Evaluates each ExtendScript command in order in Illustrator and returns its string value. The first host exception ends the call. Commands share one queued host job.',
        Schema.Struct({ commands: Schema.NonEmptyArray(Schema.String), ..._timeout }),
        Schema.Struct({ kind: Schema.Literal('values'), values: Schema.Array(Schema.String), ..._unavailable }),
        false,
    ),
    _tool(
        'illustrator_run_menu_command',
        'Executes an exact native Illustrator menu-command identifier. Accepts installed plug-in and Beta commands as well as factory commands; host exceptions retain their structured native error.',
        Schema.Struct({ command: Schema.NonEmptyString, ..._timeout }),
        Schema.Struct({ kind: Schema.Literal('applied'), command: Schema.String, ..._unavailable }),
        false,
    ),
    _tool(
        'illustrator_inspect',
        'Reads document settings, artboards, resources, styles, and layers. Item listings contain identity and layer names; items can be paged by offset and limit. Unreadable host members retain their path and error. Opens document when supplied.',
        Schema.Struct({
            document: Schema.OptionFromOptionalKey(AbsolutePath),
            items: Schema.OptionFromOptionalKey(Schema.Struct({ layer: OptionalString, offset: PageIndex, limit: _count })),
            baseline: _baseline,
            ..._timeout,
        }),
        Schema.OptionFromOptionalKey(Schema.Array(Schema.Json)).pipe((resources) =>
            Schema.Struct({
                kind: Schema.Literal('inspection'),
                document: _optionalValue,
                swatches: resources,
                gradients: resources,
                patterns: resources,
                brushes: resources,
                symbols: resources,
                graphicStyles: resources,
                characterStyles: resources,
                paragraphStyles: resources,
                layers: resources,
                itemCount: Schema.OptionFromOptionalKey(PageIndex),
                items: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ typename: OptionalString, uuid: OptionalString, name: OptionalString, layer: OptionalString }))),
                unreadable: Schema.Record(Schema.String, Schema.Array(Unavailable)),
                drift: _drift,
                ..._unavailable,
            }),
        ),
        false,
    ),
    _tool(
        'illustrator_get_preferences',
        'Reads named preferences through their typed host accessors. Missing preferences and unreadable values appear in unavailable.',
        Schema.Struct({ keys: Schema.NonEmptyArray(Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literals(Struct.keys(_preference.cases)) })), baseline: _baseline, ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('preferences'),
            rows: Schema.Array(Schema.Struct({ key: Schema.String, value: _scalar })),
            unreadable: Schema.Record(Schema.String, Schema.Array(Unavailable)),
            drift: _drift,
            ..._unavailable,
        }),
        true,
    ),
    _tool(
        'illustrator_set_preferences',
        'Writes typed preference values and reads each back in the same host call. Applied rows contain before and after; rejected rows identify a differing readback.',
        Schema.Struct({ rows: Schema.NonEmptyArray(_preference), ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ key: Schema.String, before: _scalar, after: _scalar })),
            rejected: Schema.Array(Schema.Struct({ key: Schema.String, before: _scalar, after: _scalar, reason: Schema.Literal('readbackDiffers') })),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_import_swatches',
        'Creates process, global and spot swatches at the document root or in named groups. Compatible definitions can be replaced by name; reserved colors and resource-type changes are rejected. Native failures accumulate per row.',
        Schema.Struct({ palette: _palette, replaceByName: Schema.Boolean, ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            ..._swatchResults,
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_sync_swatches',
        'Renames global colors by exact color-model and channel equality with the source. Ambiguous source colors are rejected. Each document’s rename set is preflighted for collisions and staged to support name cycles. Every write is read back; host failures restore the original names, and modified documents remain open for the caller to save.',
        Schema.Struct({ source: AbsolutePath, targets: Schema.NonEmptyArray(AbsolutePath), ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ document: AbsolutePath, from: Schema.String, to: Schema.String })),
            rejected: Schema.Array(Schema.Struct({ document: AbsolutePath, name: Schema.String, reason: Schema.Literals(['unmatched', 'ambiguous', 'renameConflict', 'readbackDiffers']) })),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_transfer_resources',
        'Transfers swatches with groups, symbols, brushes, patterns, or graphic styles from a document into the active document. Process, spot and gradient swatches can replace matching definitions by name. Gradients with conflicting named Spot dependencies are rejected. Pattern swatches and other resources retain their native definitions; existing names and incompatible resource types are rejected. Temporary artwork is removed and both documents’ selection and active layers restore.',
        Schema.Struct({
            source: AbsolutePath,
            kinds: Schema.NonEmptyArray(Schema.Literals(['swatches', 'symbols', 'brushes', 'patterns', 'graphicStyles'])),
            names: Schema.Union([Schema.Literal('all'), _identifiers]),
            replaceSwatchesByName: Schema.Boolean,
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ kind: Schema.String, name: Schema.String })),
            rejected: Schema.Array(
                Schema.Union([
                    Schema.Struct({
                        kind: Schema.String,
                        name: Schema.String,
                        reason: Schema.Literals(['nameCollision', 'resourceTypeConflict', 'reservedColor', 'notCarried', 'colorDefinitionConflict', 'unsupportedColor']),
                    }),
                    Schema.Struct({ name: Schema.String, reason: Schema.Literal('notFound') }),
                ]),
            ),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_write_library',
        'Writes an Illustrator resource library from a source document. Library kinds come from the installed host dictionary.',
        Schema.Struct({ library: Schema.Literals(enumerations.LibraryType), source: AbsolutePath, output: AbsolutePath, ..._timeout }),
        Schema.Struct({ kind: Schema.Literals(['saved', 'notSaved']), path: AbsolutePath, ..._unavailable }),
        false,
    ),
    _tool(
        'illustrator_build_template',
        'Creates an Illustrator document with the supplied dimensions, raster settings and palette, saves a PDF-compatible file with its ICC profile, then reads the saved document settings.',
        Schema.Struct({ colorSpace: _colorSpace, width: _positive, height: _positive, raster: _raster, palette: _palette, output: AbsolutePath, ..._timeout }),
        Schema.Union([
            Schema.Struct({
                kind: Schema.Literal('saved'),
                path: AbsolutePath,
                readback: Schema.OptionFromOptionalKey(Schema.Struct({ colorSpace: Schema.String, rasterResolution: Schema.Number, swatchCount: PageIndex })),
                ..._swatchResults,
                ..._unavailable,
            }),
            Schema.Struct({ kind: Schema.Literal('notSaved'), path: AbsolutePath, ..._swatchResults, ..._unavailable }),
        ]),
        false,
    ),
    _tool(
        'illustrator_build_size_catalog',
        'Creates a document for each supplied size, saves it as artwork or stationery, and reopens it to report artboard geometry and whether it remains associated with the saved file.',
        Schema.Struct({
            sizes: Schema.NonEmptyArray(Schema.Struct({ name: Schema.NonEmptyString, width: _positive, height: _positive, colorSpace: _colorSpace })),
            raster: _raster,
            outputDir: AbsolutePath,
            stationery: Schema.Boolean,
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('saved'),
            paths: Schema.Array(Schema.Struct({ name: Schema.String, path: AbsolutePath, artboardRect: _bounds, stationery: Schema.Boolean })),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_apply_rtl',
        'Applies character and paragraph attributes to selected text ranges, text frames within selected groups, or every frame. Writable property names, value kinds, and enumeration members come from the installed Illustrator dictionary; every write is read back on its native text range.',
        Schema.Struct({ target: Schema.Literals(['selection', 'document']), writes: Schema.NonEmptyArray(_rtlWrite), ..._timeout }),
        Schema.Struct({ frame: PageIndex, scope: Schema.Literals(Struct.keys(_textAttributes)), property: Schema.String }).pipe((attribute) =>
            Schema.Struct({
                kind: Schema.Literal('applied'),
                applied: Schema.Array(Schema.Struct({ ...attribute.fields, value: Schema.Json })),
                rejected: Schema.Array(
                    Schema.Struct({
                        ...attribute.fields,
                        value: _optionalValue,
                        reason: Schema.Literals(['absentFromReflect', 'readbackDiffers']),
                    }),
                ),
                ..._unavailable,
            }),
        ),
        false,
    ),
    _tool(
        'illustrator_stroke_styles',
        'Applies native stroke appearances to selected paths or native object identities. Color, dash pattern, caps, joins, miter limit and blending are explicit. Missing identities, non-path objects and incompatible named colors are reported without silently discarding targets.',
        Schema.Struct({
            styles: Schema.NonEmptyArray(
                Schema.Struct({
                    ..._strokeAppearance.fields,
                    targets: Schema.Union([Schema.Literal('selection'), _identifiers]),
                }),
            ),
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ name: Schema.String, items: PageIndex })),
            rejected: Schema.Array(
                Schema.Union([
                    Schema.Struct({ name: Schema.String, uuid: Schema.String, typename: Schema.String, reason: Schema.Literal('notAPath') }),
                    Schema.Struct({ name: Schema.String, uuid: Schema.String, reason: Schema.Literal('nativeLookupFailed') }),
                    _styleColorFailure,
                ]),
            ),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_normalize_cad',
        'Normalizes imported CAD paths to explicit stroke weights, native pattern replacements, and an ordered layer taxonomy. Reads a complete native inventory before planning changes, rejects missing or ambiguous resources before writing, and optionally removes stray paths and empty groups.',
        Schema.Struct({ ...Cad.fields, ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(
                Schema.Union([
                    Schema.Struct({ kind: Schema.Literal('layer'), path: CadState.fields.layers.value }),
                    Schema.Struct({ kind: Schema.Literal('path'), ...Struct.pick(CadState.fields.paths.value.fields, ['uuid', 'weight', 'pattern']) }),
                    Schema.Struct({ kind: Schema.Literals(['removed', 'retained']), uuid: CadPlan.fields.groups.value }),
                    Schema.Struct({ kind: Schema.Literal('moved'), ...CadPlan.fields.moves.value.fields }),
                ]),
            ),
            rejected: Schema.Array(Schema.Union([CadRejection, Schema.Struct({ kind: Schema.Literal('path'), uuid: Schema.String, reason: Schema.Literal('notAPath') })])),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_compile_styles',
        'Compiles named native graphic styles from explicit stroke appearances using a disposable carrier. Existing names are rejected. Restores the original selection, removes the carrier, and only reports a resource after native creation succeeds.',
        Schema.Struct({ styles: Schema.NonEmptyArray(_strokeAppearance), ..._timeout }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ name: Schema.String, items: Schema.Literal(0), resource: Schema.String })),
            rejected: Schema.Array(
                Schema.Union([
                    Schema.Struct({ name: Schema.String, reason: Schema.Literal('nameCollision') }),
                    Schema.Struct({ name: Schema.String, reason: Schema.Literal('nativeCreationDiffers'), count: PageIndex }),
                    _styleColorFailure,
                ]),
            ),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_artboards',
        'Duplicates an artboard into a spaced grid, inserting copies after the source or at the end. Naming uses %a for the source name and %n for the padded copy number. Artwork copies retain their layers, groups, lock and visibility states; selection, active artboard and coordinates restore. Host canvas-limit failures are reported per copy. Also writes and reads back ruler origins, pixel aspect ratio, center marks, crosshairs and video safe areas.',
        Schema.Struct({
            artboard: _artboard,
            change: Schema.Union([
                Schema.Struct({ operation: Schema.Literal('properties'), values: _artboardProperties }),
                Schema.Struct({
                    operation: Schema.Literal('duplicate'),
                    copies: _count,
                    columns: _count,
                    spacing: _distance,
                    nameTemplate: Schema.NonEmptyString,
                    insertLast: Schema.Boolean,
                    copyArtwork: Schema.Boolean,
                }),
            ]),
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(
                Schema.Union([Schema.Struct({ index: PageIndex, properties: _artboardProperties }), Schema.Struct({ index: PageIndex, name: Schema.String, rect: _bounds, copiedItems: PageIndex })]),
            ),
            rejected: Schema.Array(Schema.Struct({ reason: Schema.Literal('artboardOutOfRange'), count: PageIndex })),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_guides',
        'Creates object guides, proportional logo lockups, Bento layouts, or clipped isometric grids. Measurements come from the active document. Lockups use the first selected object and can explicitly convert area text. Bento layouts use a supplied seed, reject impossible dimensions before drawing, and keep or remove the source selection as requested. All lengths are points; fractional extensions and splits use ratios, and artboard indexes are zero-based.',
        Schema.Struct({
            change: Schema.Union([
                _guideDraw.cases.object,
                _guideDraw.cases.isometric,
                Schema.Struct({
                    ...Struct.pick(_guideDraw.cases.lockup.fields, ['operation', 'layerName', 'color', 'weight', 'filled']),
                    layout: Schema.Literals(['horizontal', 'vertical', 'condensed']),
                    convertAreaText: Schema.Boolean,
                    labelGap: _distance,
                    labelSize: Schema.Struct({ thirds: _positive, subdivisions: _positive }),
                }),
                Schema.Struct({
                    ...Struct.pick(_guideDraw.cases.bento.fields, ['operation', 'cornerRadius', 'groupName', 'removeSelection']),
                    target: Schema.Literals(['selection', 'artboard']),
                    size: Schema.OptionFromOptionalKey(Schema.Struct({ width: _positive, height: _positive })),
                    seed: Schema.String,
                    layout: Schema.Union([
                        Schema.Struct({
                            kind: Schema.Literal('grid'),
                            columns: _countRange,
                            rows: _countRange,
                            columnGutter: _distance,
                            rowGutter: _distance,
                            ...Record.map({ splitColumns: _fractionRange, splitRows: _fractionRange }, Schema.OptionFromOptionalKey),
                        }),
                        Schema.Struct({
                            kind: Schema.Literal('total'),
                            maxCount: _count,
                            gutter: _distance,
                            minSize: _positive,
                            hero: Schema.OptionFromOptionalKey(Schema.Struct({ centered: Schema.Boolean, scale: _fractionRange })),
                        }),
                    ]),
                }),
            ]),
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ operation: Schema.Literals(Struct.keys(_guideDraw.cases)), drawn: PageIndex })),
            rejected: Schema.Array(
                Schema.Union([
                    Schema.Struct({ operation: Schema.Literal('isometric'), index: PageIndex, count: PageIndex, reason: Schema.Literal('artboardOutOfRange') }),
                    ...Schema.Literals(['lockup', 'bento']).pipe((operation) => [
                        ..._guideMeasurementFailure.mapMembers(Tuple.map(Schema.fieldsAssign({ operation }))).members,
                        Schema.Struct({ operation, reason: Schema.Literals(['nothingSelected', 'invalidBounds', 'gutterExceedsCell', 'minimumSizeExceedsBounds', 'insufficientHeroSpace']) }),
                    ]),
                    Schema.Struct({ operation: Schema.Literals(['lockup', 'object']), ..._paintRejection.fields }),
                ]),
            ),
            ..._unavailable,
        }),
        false,
    ),
    _tool(
        'illustrator_snapshot',
        'Captures the active artboard or explicit document-space bounds as an image under the job artifacts directory. Fits the selected pixel budget within Illustrator’s supported resolution range and returns its path and dimensions.',
        Schema.Struct({
            clip: Schema.Union([
                Schema.Struct({ artboard: Schema.Literal('active') }),
                Schema.Struct({
                    bounds: _bounds.pipe(
                        Schema.check(Schema.makeFilter(([left, top, right, bottom]) => right > left && top > bottom, { description: 'Positive-width, positive-height Illustrator bounds' })),
                    ),
                }),
            ]),
            budget: PixelBudget,
            baseline: _baseline,
            ..._timeout,
        }),
        Schema.Struct({ kind: Schema.Literal('image'), path: AbsolutePath, widthPx: PageIndex, heightPx: PageIndex, dpi: Schema.Number, drift: _drift, ..._unavailable }),
        false,
    ),
    _tool(
        'illustrator_gradient_blend',
        'Interpolates selected fill or stroke gradients in a perceptual color space. Keeps endpoint colors and original stop positions, opacity and midpoints; optionally removes intermediate stops first. Illustrator converts process and tinted spot colors using its color profiles. Pure-K CMYK segments retain pure K. Shared document references are reported because the gradient resource changes for every object using it.',
        Schema.Struct({
            attributes: Schema.NonEmptyArray(_gradientWrite.fields.attributes.value),
            precision: Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 1, maximum: 20 }))),
            space: Schema.Literals(Struct.keys(_gradientSpaces)),
            hueArc: Schema.Literals(['shorter', 'longer', 'decreasing', 'increasing']),
            removeIntermediate: Schema.Boolean,
            ..._timeout,
        }),
        Schema.Struct({
            kind: Schema.Literal('applied'),
            applied: Schema.Array(Schema.Struct({ gradient: Schema.String, stopsAdded: PageIndex })),
            rejected: Schema.Array(Schema.Struct({ gradient: Schema.String, reason: Schema.Literals(['unreadableStop', 'notInSelection', 'colorDefinitionConflict', 'unsupportedColor']) })),
            shared: Schema.Array(Schema.Struct(_gradientIdentity)),
            ..._unavailable,
        }),
        false,
    ),
);

// --- [INTERPOLATION] -------------------------------------------------------------------

const _interpolate = (
    gradient: (typeof _gradientRead)['Type']['applied'][number],
    input: Tool.Parameters<typeof _toolkit.tools.illustrator_gradient_blend>,
): Effect.Effect<(typeof _gradientWrite)['Type']['gradients'][number], BridgeError> => {
    const final = Array.lastNonEmpty(gradient.stops);
    const stops = input.removeIntermediate ? [Array.headNonEmpty(gradient.stops), final] : gradient.stops;
    const resolved = Array.map(stops, (stop) => {
        const tint = stop.color.model === 'Spot' ? stop.color.tint / CHANNELS.percent : 1;
        const black = Match.value(stop.color.model === 'Spot' ? stop.color.ink : stop.color).pipe(
            Match.when({ model: 'CMYK', values: [0, 0, 0, Match.number] }, ({ values }) => values[3] * tint),
            Match.option,
        );
        return {
            ...stop,
            black,
            resolved: mix({ space: sRGB, coords: [stop.rgb[0] / CHANNELS.rgb, stop.rgb[1] / CHANNELS.rgb, stop.rgb[2] / CHANNELS.rgb] }, { space: XYZ_D65, coords: sRGB.white }, 1 - tint, {
                space: sRGB,
            }),
        };
    });
    const samples = Array.flatten(
        Array.zipWith(resolved, Array.drop(resolved, 1), (from, to) => {
            const interpolate = range(from.resolved, to.resolved, { space: _gradientSpaces[input.space], hue: input.hueArc, outputSpace: sRGB });
            return Array.makeBy(input.precision, (index) => ({ from, to, interpolate, fraction: index / input.precision }));
        }),
    );
    return Effect.validate(samples, ({ from, to, interpolate, fraction }) => {
        if (fraction === 0) {
            return Effect.succeed(Struct.pick(from, ['position', 'midpoint', 'opacity', 'color']));
        }
        const color: Effect.Effect<(typeof Ink)['Type'], Schema.SchemaError> = Option.match(
            Option.zipWith(from.black, to.black, (a, b) => a + (b - a) * fraction),
            {
                onSome: (black) => Effect.succeed({ model: 'CMYK' as const, values: [0, 0, 0, black] as const }),
                onNone: () =>
                    Schema.decodeUnknownEffect(Ink.members[0])({
                        model: 'RGB',
                        values: Array.map(toGamut(interpolate(fraction)).coords, (channel) => (channel === null ? channel : channel * CHANNELS.rgb)),
                    }),
            },
        );
        return Effect.map(color, (value) => ({
            position: from.position + (to.position - from.position) * fraction,
            midpoint: CHANNELS.percent / 2,
            opacity: from.opacity + (to.opacity - from.opacity) * fraction,
            color: value,
        }));
    }).pipe(
        Effect.map((interpolated) => ({ name: gradient.gradient, stops: Array.append(interpolated, final) })),
        Effect.mapError(notDecodable(HOSTS.illustrator.id, gradient)),
    );
};

// --- [DISPATCH] ------------------------------------------------------------------------

interface Dispatching extends Struct.Lambda {
    <Parameters extends Schema.Codec<{ readonly timeoutMs: TimeoutMs }, unknown, never, never>, Success extends Schema.Codec<unknown, unknown, never, never>>(definition: {
        readonly name: string;
        readonly parametersSchema: Parameters;
        readonly successSchema: Reply<Success>;
    }): (input: Parameters['Type']) => Effect.Effect<Success['Type'], BridgeError, ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path | Hosts | Jobs>;
    readonly '~lambda.out': this['~lambda.in'] extends Tool.Any
        ? (input: Tool.Parameters<this['~lambda.in']>) => Effect.Effect<Member<this['~lambda.in']>, BridgeError, Tool.HandlerServices<this['~lambda.in']>>
        : never;
}

const _dispatch = Struct.lambda<Dispatching>((definition) =>
    Effect.fnUntraced(function* (input: { readonly timeoutMs: TimeoutMs }) {
        const at = yield* site;
        const host = (yield* Jobs).illustrator;
        return yield* run(host, input.timeoutMs, (jobId) =>
            dispatch(
                at,
                input.timeoutMs,
                { entry: definition.name.slice(HOSTS.illustrator.id.length + 1).replaceAll('_', '-'), request: definition.parametersSchema, response: succeeded(definition) },
                input,
                jobId,
            ),
        );
    }),
);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path | Hosts | Jobs> = Layer.provide(
    register(_toolkit),
    _toolkit.toLayer(
        Struct.map(
            {
                ...Struct.map(
                    Struct.omit(_toolkit.tools, [
                        'illustrator_compile_styles',
                        'illustrator_get_preferences',
                        'illustrator_gradient_blend',
                        'illustrator_guides',
                        'illustrator_inspect',
                        'illustrator_normalize_cad',
                        'illustrator_snapshot',
                        'illustrator_sync_swatches',
                    ]),
                    _dispatch,
                ),
                [_toolkit.tools.illustrator_compile_styles.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    const compilation = Schema.Struct({
                        createStyle: Schema.Literal('Adobe New Style Shortcut'),
                        styles: Schema.NonEmptyArray(Schema.Struct({ ..._strokeAppearance.fields, targets: Schema.Tuple([]) })),
                    });
                    return yield* run(host, input.timeoutMs, (jobId) =>
                        dispatch(
                            at,
                            input.timeoutMs,
                            { entry: 'stroke-styles', request: compilation, response: succeeded(_toolkit.tools.illustrator_compile_styles) },
                            {
                                createStyle: compilation.fields.createStyle.literal,
                                styles: Array.map(input.styles, Struct.assign({ targets: [] as const })),
                            },
                            jobId,
                        ),
                    );
                }),
                [_toolkit.tools.illustrator_normalize_cad.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    return yield* run(
                        host,
                        input.timeoutMs,
                        Effect.fnUntraced(function* (
                            jobId: JobId,
                        ): Effect.fn.Return<Member<typeof _toolkit.tools.illustrator_normalize_cad>, BridgeError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> {
                            const before = yield* dispatch(
                                at,
                                input.timeoutMs,
                                { entry: 'cad', request: Schema.Struct({ mode: Schema.Literal('read'), scope: Cad.fields.scope }), response: CadState },
                                { mode: 'read', scope: input.scope },
                                jobId,
                            );
                            if (Array.isReadonlyArrayNonEmpty(before.unavailable)) {
                                return { kind: 'applied', applied: [], rejected: [], unavailable: before.unavailable };
                            }
                            const planned = yield* Effect.result(planCad(input, before));
                            if (Result.isFailure(planned)) {
                                return { kind: 'applied', applied: [], rejected: planned.failure, unavailable: before.unavailable };
                            }
                            return yield* dispatch(at, input.timeoutMs, { entry: 'cad', request: CadPlan, response: succeeded(_toolkit.tools.illustrator_normalize_cad) }, planned.success, jobId);
                        }),
                    );
                }),
                [_toolkit.tools.illustrator_get_preferences.name]: Effect.fnUntraced(function* (input) {
                    const readback = yield* _dispatch(_toolkit.tools.illustrator_get_preferences)(input);
                    if (Option.isNone(input.baseline)) {
                        return readback;
                    }
                    const observed = {
                        ...Record.fromIterableWith(readback.rows, ({ key, value }) => [key, Observation.cases.value.make({ value })]),
                        ...Record.map(readback.unreadable, (cause) => Observation.cases.unreadable.make({ cause })),
                    };
                    return { ...readback, drift: Option.some(yield* compare(input.baseline.value, observed)) };
                }),
                [_toolkit.tools.illustrator_inspect.name]: Effect.fnUntraced(function* (input) {
                    const readback = yield* _dispatch(_toolkit.tools.illustrator_inspect)(input);
                    if (Option.isNone(input.baseline)) {
                        return readback;
                    }
                    const encoded = yield* Schema.encodeEffect(succeeded(_toolkit.tools.illustrator_inspect))(readback).pipe(Effect.orDie);
                    const values: (typeof Schema.JsonObject)['Type'] = Struct.omit(encoded, ['kind', 'unavailable', 'unreadable', 'drift']);
                    const observed = {
                        ...Record.map(values, (value) => Observation.cases.value.make({ value })),
                        ...Record.map(readback.unreadable, (cause) => Observation.cases.unreadable.make({ cause })),
                    };
                    const drift = yield* compare(input.baseline.value, observed);
                    return { ...readback, drift: Option.some(drift) };
                }),
                [_toolkit.tools.illustrator_guides.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    const entry = yield* request(input.timeoutMs);
                    const { change } = input;
                    const response = succeeded(_toolkit.tools.illustrator_guides);
                    if (change.operation === 'object' || change.operation === 'isometric') {
                        return yield* submit(host, entry, dispatch(at, input.timeoutMs, { entry: 'guides', request: _guideDraw, response }, change, entry.jobId));
                    }
                    const read = dispatch(
                        at,
                        input.timeoutMs,
                        {
                            entry: 'guides',
                            request: Schema.Struct({ operation: Schema.Literal('measure'), target: Schema.Literals(['first', 'selection', 'artboard']), convertAreaText: Schema.Boolean }),
                            response: Schema.Struct({
                                kind: Schema.Literal('applied'),
                                applied: Schema.NonEmptyArray(
                                    Schema.Struct({
                                        operation: Schema.Literal('measure'),
                                        artboard: _bounds,
                                        selectionBounds: Schema.OptionFromOptionalKey(_bounds),
                                        items: Schema.Array(
                                            Schema.Struct({
                                                uuid: Schema.NonEmptyString,
                                                typename: Schema.String,
                                                position: _point,
                                                width: Schema.Number,
                                                height: Schema.Number,
                                                geometricBounds: _bounds,
                                            }),
                                        ),
                                    }),
                                ),
                                rejected: Schema.Array(_guideMeasurementFailure.mapMembers(Tuple.map(Schema.fieldsAssign({ operation: Schema.Literal('measure') })))),
                                ..._unavailable,
                            }),
                        },
                        { operation: 'measure', target: change.operation === 'lockup' ? 'first' : change.target, convertAreaText: change.operation === 'lockup' && change.convertAreaText },
                        entry.jobId,
                    );
                    const before = read.pipe(
                        Effect.map(
                            (measured): Result.Result<Effect.Success<typeof read>, Member<typeof _toolkit.tools.illustrator_guides>> =>
                                measured.rejected.length === 0
                                    ? Result.succeed(measured)
                                    : Result.fail(
                                          response.make({
                                              kind: 'applied',
                                              applied: [],
                                              rejected: Array.map(measured.rejected, (row) => ({ ...row, operation: change.operation })),
                                              unavailable: measured.unavailable,
                                          }),
                                      ),
                        ),
                    );
                    let planned: Effect.Effect<
                        Result.Result<{ readonly draw: (typeof _guideDraw)['Type']; readonly unavailable: readonly (typeof Unavailable)['Type'][] }, Member<typeof _toolkit.tools.illustrator_guides>>,
                        BridgeError,
                        ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path
                    >;
                    if (change.operation === 'lockup') {
                        planned = before.pipe(
                            Effect.map(
                                Result.flatMap((measured): Effect.Success<typeof planned> => {
                                    const selected = Array.head(Array.headNonEmpty(measured.applied).items);
                                    if (Option.isNone(selected)) {
                                        return Result.fail(
                                            response.make({ kind: 'applied', applied: [], rejected: [{ operation: change.operation, reason: 'nothingSelected' }], unavailable: measured.unavailable }),
                                        );
                                    }
                                    const {
                                        width,
                                        height,
                                        position: [left, top],
                                        uuid,
                                    } = selected.value;
                                    if (!(width > 0 && height > 0)) {
                                        return Result.fail(
                                            response.make({ kind: 'applied', applied: [], rejected: [{ operation: change.operation, reason: 'invalidBounds' }], unavailable: measured.unavailable }),
                                        );
                                    }
                                    const bottom = top - height;
                                    const thirds = 3;
                                    const quarters = 4;
                                    const ninths = thirds * thirds;
                                    const quarterTurn = 90;
                                    const horizontal = {
                                        horizontal: Array.map(Array.range(-1, quarters + 1), (index): [number, number, number] => [
                                            left - width / 2,
                                            left + width + width / 2,
                                            top - (index * height) / quarters,
                                        ]),
                                        vertical: [
                                            ...Array.makeBy(thirds, (index): [number, number, number] => [left - width, left + 2 * width, top - (index * height) / thirds]),
                                            ...Array.makeBy(thirds + 1, (index): [number, number, number] => [
                                                left - width - 2 * change.labelGap,
                                                left + 2 * width + 2 * change.labelGap,
                                                bottom - (index * height) / thirds,
                                            ]),
                                            ...Array.map(
                                                Array.filter(Array.range(1, ninths - 1), (index) => index % thirds !== 0),
                                                (index): [number, number, number] => [left - width, left + 2 * width, bottom - (index * height) / ninths],
                                            ),
                                        ],
                                        condensed: Array.map(Array.range(-2, 2 * thirds), (index): [number, number, number] => [
                                            left - width / thirds,
                                            left + width + width / thirds,
                                            top - (index * height) / thirds,
                                        ]),
                                    }[change.layout];
                                    const vertical = {
                                        horizontal: Array.map([left - height / quarters, left, left + width, left + width + height / quarters], (x): [number, number, number] => [
                                            x,
                                            top + height / 2,
                                            bottom - height / 2,
                                        ]),
                                        vertical: Array.makeBy(thirds + 1, (index): [number, number, number] => [left + (index * width) / thirds, top, bottom - height]),
                                        condensed: Array.makeBy(quarters + 1, (index): [number, number, number] => [
                                            left + (index * width) / quarters,
                                            bottom - height + width + (2 * width) / thirds,
                                            bottom - height,
                                        ]),
                                    }[change.layout];
                                    const labels = {
                                        horizontal: [],
                                        vertical: [
                                            ...Array.makeBy(2 * thirds, (index) => ({
                                                text: String((index % thirds) + 1),
                                                size: change.labelSize.thirds,
                                                left: left - width - 2 * change.labelGap - change.labelGap,
                                                top: top - ((index + 1 / 2) * height) / thirds,
                                                vertical: false,
                                                width: 0,
                                            })),
                                            ...Array.makeBy(ninths, (index) => ({
                                                text: String((index % thirds) + 1),
                                                size: change.labelSize.subdivisions,
                                                left: left - width,
                                                top: bottom - ((index + 1 / 2) * height) / ninths,
                                                vertical: false,
                                                width: 0,
                                            })),
                                        ],
                                        condensed: [
                                            ...Array.makeBy(thirds, (index) => ({
                                                text: String(index + 1),
                                                size: change.labelSize.thirds,
                                                left: left - width / thirds - change.labelGap,
                                                top: top - ((index + 1 / 2) * height) / thirds,
                                                vertical: false,
                                                width: 0,
                                            })),
                                            ...Array.makeBy(quarters, (index) => ({
                                                text: String(index + 1),
                                                size: change.labelSize.subdivisions,
                                                left: left + (index * width) / quarters,
                                                top: bottom - height - change.labelGap,
                                                vertical: true,
                                                width: width / quarters,
                                            })),
                                        ],
                                    }[change.layout];
                                    return Result.succeed({
                                        draw: _guideDraw.cases.lockup.make({
                                            ...Struct.pick(change, ['operation', 'layerName', 'color', 'weight', 'filled']),
                                            source: uuid,
                                            lines: [
                                                ...Array.map(horizontal, ([from, to, y]) => ({ from: [from, y] as const, to: [to, y] as const })),
                                                ...Array.map(vertical, ([x, from, to]) => ({ from: [x, from] as const, to: [x, to] as const })),
                                            ],
                                            labels,
                                            copies: {
                                                horizontal: [],
                                                vertical: Array.map([left - width, left + width], (x) => ({ position: [x, top] as const, rotate: 0, anchor: 'top' as const })),
                                                condensed: Array.map([left - height, left + width], (x) => ({
                                                    position: [x, bottom - height] as const,
                                                    rotate: quarterTurn,
                                                    anchor: 'bottom' as const,
                                                })),
                                            }[change.layout],
                                        }),
                                        unavailable: measured.unavailable,
                                    });
                                }),
                            ),
                        );
                    } else {
                        const random = yield* Random.Random.pipe(Random.withSeed(change.seed));
                        const dimensions = before.pipe(
                            Effect.map(
                                Result.flatMap(
                                    (
                                        measured,
                                    ): Result.Result<
                                        { readonly measured: typeof measured; readonly left: number; readonly top: number; readonly width: number; readonly height: number },
                                        Member<typeof _toolkit.tools.illustrator_guides>
                                    > => {
                                        const measurement = Array.headNonEmpty(measured.applied);
                                        const bounds = change.target === 'selection' ? measurement.selectionBounds : Option.some(measurement.artboard);
                                        if (Option.isNone(bounds)) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'nothingSelected' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        const [left, top, right, bottom] = bounds.value;
                                        const { width, height } = Option.getOrElse(change.size, () => ({ width: right - left, height: top - bottom }));
                                        if (!(width > 0 && height > 0)) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'invalidBounds' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        return Result.succeed({ measured, left, top, width, height });
                                    },
                                ),
                            ),
                        );
                        const { layout } = change;
                        if (layout.kind === 'grid') {
                            planned = dimensions.pipe(
                                Effect.map(
                                    Result.flatMap(({ measured, left, top, width, height }): Effect.Success<typeof planned> => {
                                        const columnWidth = (width - (layout.columns[1] - 1) * layout.columnGutter) / layout.columns[1];
                                        const rowHeight = (height - (layout.rows[1] - 1) * layout.rowGutter) / layout.rows[1];
                                        if (
                                            !(columnWidth > 0 && rowHeight > 0) ||
                                            (Option.isSome(layout.splitRows) && columnWidth <= layout.columnGutter) ||
                                            (Option.isSome(layout.splitColumns) && rowHeight <= layout.rowGutter)
                                        ) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'gutterExceedsCell' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        const columns = layout.columns[0] + Math.floor(random.nextDoubleUnsafe() * (layout.columns[1] - layout.columns[0] + 1));
                                        const rows = Array.makeBy(columns, () => layout.rows[0] + Math.floor(random.nextDoubleUnsafe() * (layout.rows[1] - layout.rows[0] + 1)));
                                        const coordinates = Array.flatMap(rows, (rowCount, column) => Array.zip(Array.range(0, rowCount - 1), Array.replicate(column, rowCount)));
                                        const w = (width - (columns - 1) * layout.columnGutter) / columns;
                                        const regular = Array.map(coordinates, ([row, column]) => {
                                            const rowCount = Array.getUnsafe(rows, column);
                                            const h = (height - (rowCount - 1) * layout.rowGutter) / rowCount;
                                            return { x: left + column * (w + layout.columnGutter), y: top - row * (h + layout.rowGutter), w, h };
                                        });
                                        const [, split] = Array.mapAccum(regular, 'none', (previous, cell): [string, (typeof _guideDraw.cases.bento)['Type']['cells'][number][]] => {
                                            const horizontal = Option.isSome(layout.splitRows) && previous !== 'row' && random.nextDoubleUnsafe() > 1 / 2;
                                            const vertical = Option.isSome(layout.splitColumns) && previous !== 'column' && random.nextDoubleUnsafe() > 1 / 2;
                                            const rowFraction = horizontal ? layout.splitRows : Option.none();
                                            const fraction = vertical ? layout.splitColumns : rowFraction;
                                            if (Option.isNone(fraction)) {
                                                return ['none', [cell]];
                                            }
                                            const ratio = fraction.value[0] + random.nextDoubleUnsafe() * (fraction.value[1] - fraction.value[0]);
                                            const available = vertical ? cell.h - layout.rowGutter : cell.w - layout.columnGutter;
                                            const size = available * ratio;
                                            return vertical
                                                ? [
                                                      'column',
                                                      [
                                                          { ...cell, h: size },
                                                          { ...cell, y: cell.y - size - layout.rowGutter, h: available - size },
                                                      ],
                                                  ]
                                                : [
                                                      'row',
                                                      [
                                                          { ...cell, w: size },
                                                          { ...cell, x: cell.x + size + layout.columnGutter, w: available - size },
                                                      ],
                                                  ];
                                        });
                                        const cells = Array.flatten(split);
                                        if (!Array.isArrayNonEmpty(cells)) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'invalidBounds' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        return Result.succeed({
                                            draw: _guideDraw.cases.bento.make({ ...Struct.pick(change, ['operation', 'cornerRadius', 'groupName', 'removeSelection']), cells }),
                                            unavailable: measured.unavailable,
                                        });
                                    }),
                                ),
                            );
                        } else {
                            planned = dimensions.pipe(
                                Effect.map(
                                    Result.flatMap(({ measured, left, top, width, height }) => {
                                        if (width < layout.minSize || height < layout.minSize) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'minimumSizeExceedsBounds' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        const scale = Option.isSome(layout.hero)
                                            ? layout.hero.value.scale[0] + random.nextDoubleUnsafe() * (layout.hero.value.scale[1] - layout.hero.value.scale[0])
                                            : 1;
                                        const centered = Option.isSome(layout.hero) && layout.hero.value.centered;
                                        const extent = Array.map([width, height], (dimension) => {
                                            const size = Math.max(layout.minSize, dimension * scale);
                                            const free = dimension - size;
                                            const threshold = layout.minSize + layout.gutter;
                                            if (centered) {
                                                return { start: free / 2, size };
                                            }
                                            const position = random.nextDoubleUnsafe() * free;
                                            const fromEnd = free - position < threshold ? free : position;
                                            return { start: position < threshold ? 0 : fromEnd, size };
                                        });
                                        const x = Array.getUnsafe(extent, 0);
                                        const y = Array.getUnsafe(extent, 1);
                                        const centerpiece = { x: left + x.start, y: top - y.start, w: x.size, h: y.size };
                                        const fullHeight = random.nextDoubleUnsafe() > 1 / 2;
                                        const span = fullHeight ? { x: centerpiece.x, y: top, w: centerpiece.w, h: height } : { x: left, y: centerpiece.y, w: width, h: centerpiece.h };
                                        const minimum = Number.isGreaterThanOrEqualTo(layout.minSize);
                                        const zones = Option.isNone(layout.hero)
                                            ? [{ x: left, y: top, w: width, h: height }]
                                            : Array.filter(
                                                  [
                                                      { x: left, y: span.y, w: x.start - layout.gutter, h: span.h },
                                                      {
                                                          x: centerpiece.x + centerpiece.w + layout.gutter,
                                                          y: span.y,
                                                          w: width - x.start - x.size - layout.gutter,
                                                          h: span.h,
                                                      },
                                                      { x: span.x, y: top, w: span.w, h: y.start - layout.gutter },
                                                      {
                                                          x: span.x,
                                                          y: centerpiece.y - centerpiece.h - layout.gutter,
                                                          w: span.w,
                                                          h: height - y.start - y.size - layout.gutter,
                                                      },
                                                  ],
                                                  Predicate.Struct({ w: minimum, h: minimum }),
                                              );
                                        const fixed = Option.toArray(Option.as(layout.hero, centerpiece));
                                        if (zones.length + fixed.length > layout.maxCount) {
                                            return Result.fail(
                                                response.make({
                                                    kind: 'applied',
                                                    applied: [],
                                                    rejected: [{ operation: change.operation, reason: 'insufficientHeroSpace' }],
                                                    unavailable: measured.unavailable,
                                                }),
                                            );
                                        }
                                        return Result.succeed({ measured, zones, fixed });
                                    }),
                                ),
                                Effect.flatMap(
                                    Result.match({
                                        onFailure: flow(Result.fail, Effect.succeed),
                                        onSuccess: ({ measured, zones, fixed }): Effect.Effect<Effect.Success<typeof planned>> => {
                                            const divisible = Number.isGreaterThanOrEqualTo(2 * layout.minSize + layout.gutter);
                                            const eligible = Predicate.or<(typeof _guideDraw.cases.bento)['Type']['cells'][number]>(
                                                Predicate.Struct({ w: divisible }),
                                                Predicate.Struct({ h: divisible }),
                                            );
                                            const largest = Order.mapInput(Order.flip(Number.Order), (cell: (typeof _guideDraw.cases.bento)['Type']['cells'][number]) => cell.w * cell.h);
                                            const alternateAxisProbability = 0.3;
                                            return Stream.unfold(zones, (leaves) => {
                                                const availableCells = Array.filter(leaves, eligible);
                                                const candidate = Array.match(availableCells, { onEmpty: Option.none, onNonEmpty: flow(Array.min(largest), Option.some) });
                                                if (leaves.length + fixed.length >= layout.maxCount || Option.isNone(candidate)) {
                                                    return Effect.succeed(undefined);
                                                }
                                                const cell = candidate.value;
                                                const canH = cell.w >= 2 * layout.minSize + layout.gutter;
                                                const canV = cell.h >= 2 * layout.minSize + layout.gutter;
                                                const preferHorizontal = random.nextDoubleUnsafe() >= alternateAxisProbability === cell.w > cell.h;
                                                const horizontal = canH && (!canV || preferHorizontal);
                                                const available = (horizontal ? cell.w : cell.h) - layout.gutter;
                                                const size = layout.minSize + random.nextDoubleUnsafe() * (available - 2 * layout.minSize);
                                                const split = horizontal
                                                    ? [
                                                          { ...cell, w: size },
                                                          { ...cell, x: cell.x + size + layout.gutter, w: available - size },
                                                      ]
                                                    : [
                                                          { ...cell, h: size },
                                                          { ...cell, y: cell.y - size - layout.gutter, h: available - size },
                                                      ];
                                                const next = [...Array.remove(leaves, leaves.indexOf(cell)), ...split];
                                                return Effect.succeed([next, next] as const);
                                            }).pipe(
                                                Stream.runLast,
                                                Effect.map((partitioned) => {
                                                    const cells = [...fixed, ...(Option.isSome(partitioned) ? partitioned.value : zones)];
                                                    if (!Array.isArrayNonEmpty(cells)) {
                                                        return Result.fail(
                                                            response.make({
                                                                kind: 'applied',
                                                                applied: [],
                                                                rejected: [{ operation: change.operation, reason: 'invalidBounds' }],
                                                                unavailable: measured.unavailable,
                                                            }),
                                                        );
                                                    }
                                                    return Result.succeed({
                                                        draw: _guideDraw.cases.bento.make({ ...Struct.pick(change, ['operation', 'cornerRadius', 'groupName', 'removeSelection']), cells }),
                                                        unavailable: measured.unavailable,
                                                    });
                                                }),
                                            );
                                        },
                                    }),
                                ),
                            );
                        }
                    }
                    return yield* submit(
                        host,
                        entry,
                        planned.pipe(
                            Effect.flatMap(
                                Result.match({
                                    onFailure: Effect.succeed,
                                    onSuccess: ({ draw, unavailable }) =>
                                        dispatch(at, input.timeoutMs, { entry: 'guides', request: _guideDraw, response }, draw, entry.jobId).pipe(
                                            Effect.map((after) => ({ ...after, unavailable: [...unavailable, ...after.unavailable] })),
                                        ),
                                }),
                            ),
                        ),
                    );
                }),
                [_toolkit.tools.illustrator_sync_swatches.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    return yield* run(
                        host,
                        input.timeoutMs,
                        Effect.fnUntraced(function* (
                            jobId: JobId,
                        ): Effect.fn.Return<Member<typeof _toolkit.tools.illustrator_sync_swatches>, BridgeError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path | Hosts> {
                            const before = yield* dispatch(
                                at,
                                input.timeoutMs,
                                { entry: 'sync-swatches', request: Schema.Struct({ mode: Schema.Literal('read'), documents: Schema.Array(AbsolutePath) }), response: _swatchRead },
                                { mode: 'read', documents: Array.dedupe([input.source, ...input.targets]) },
                                jobId,
                            );
                            const source = Array.reduce(
                                Array.filter(before.rows, (row) => row.document === input.source),
                                HashMap.empty<(typeof Ink)['Type'], Result.Result<{ readonly to: string }, { readonly reason: 'ambiguous' }>>(),
                                (catalog, row) => HashMap.set(catalog, row.ink, HashMap.has(catalog, row.ink) ? Result.fail({ reason: 'ambiguous' as const }) : Result.succeed({ to: row.name })),
                            );
                            const [rejected, rows] = Array.partition(
                                Array.filter(before.rows, (row) => input.targets.includes(row.document)),
                                (row) =>
                                    Result.fromOption(HashMap.get(source, row.ink), Function.constant({ reason: 'unmatched' as const })).pipe(
                                        Result.flatMap(Function.identity),
                                        Result.mapBoth({
                                            onFailure: Struct.assign({ document: row.document, name: row.name }),
                                            onSuccess: Struct.assign({ document: row.document, from: row.name }),
                                        }),
                                    ),
                            );
                            const temporary = Uint8Array.fromHex(jobId.replaceAll('-', '')).toBase64({ alphabet: 'base64url', omitPadding: true });
                            const changes = Array.map(
                                Array.filter(rows, (row) => row.from !== row.to),
                                (row, index) => ({ ...row, temporary: `${temporary}-${index}` }),
                            );
                            const [conflicts, documents] = Array.partition(Record.toEntries(Array.groupBy(changes, Struct.get('document'))), ([path, renames]) =>
                                Record.size(Array.groupBy(renames, Struct.get('to'))) === renames.length
                                    ? Result.succeed({ path: AbsolutePath.make(path), renames: Array.map(renames, Struct.omit(['document'])) })
                                    : Result.fail(renames),
                            );
                            const renameConflicts = Array.map(Array.flatten(conflicts), (row) => ({ document: row.document, name: row.from, reason: 'renameConflict' as const }));
                            if (Array.isArrayEmpty(documents)) {
                                return { kind: 'applied', applied: [], rejected: [...rejected, ...renameConflicts], unavailable: before.unavailable };
                            }
                            const after = yield* dispatch(
                                at,
                                input.timeoutMs,
                                {
                                    entry: 'sync-swatches',
                                    request: Schema.Struct({
                                        mode: Schema.Literal('write'),
                                        documents: Schema.Array(
                                            Schema.Struct({ path: AbsolutePath, renames: Schema.Array(Schema.Struct({ from: Schema.String, to: Schema.String, temporary: Schema.String })) }),
                                        ),
                                    }),
                                    response: succeeded(_toolkit.tools.illustrator_sync_swatches),
                                },
                                { mode: 'write', documents },
                                jobId,
                            );
                            return { ...after, rejected: [...rejected, ...renameConflicts, ...after.rejected], unavailable: [...before.unavailable, ...after.unavailable] };
                        }),
                    );
                }),
                [_toolkit.tools.illustrator_gradient_blend.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    return yield* run(
                        host,
                        input.timeoutMs,
                        Effect.fnUntraced(function* (jobId: JobId) {
                            const before = yield* dispatch(
                                at,
                                input.timeoutMs,
                                { entry: 'gradient-blend', request: Schema.Struct({ mode: Schema.Literal('read'), attributes: _gradientWrite.fields.attributes }), response: _gradientRead },
                                { mode: 'read', attributes: input.attributes },
                                jobId,
                            );
                            const gradients = yield* Effect.validate(before.applied, (gradient) => _interpolate(gradient, input)).pipe(Effect.mapError(notDecodable(HOSTS.illustrator.id, input)));
                            const after = yield* dispatch(
                                at,
                                input.timeoutMs,
                                { entry: 'gradient-blend', request: _gradientWrite, response: succeeded(_toolkit.tools.illustrator_gradient_blend) },
                                { mode: 'write', attributes: input.attributes, gradients },
                                jobId,
                            );
                            return { ...after, rejected: [...before.rejected, ...after.rejected], unavailable: [...before.unavailable, ...after.unavailable] };
                        }),
                    );
                }),
                [_toolkit.tools.illustrator_snapshot.name]: Effect.fnUntraced(function* (input) {
                    const at = yield* site;
                    const host = (yield* Jobs).illustrator;
                    const path = yield* Path.Path;
                    const fs = yield* FileSystem.FileSystem;
                    const captured = yield* run(host, input.timeoutMs, (jobId) =>
                        dispatch(
                            at,
                            input.timeoutMs,
                            {
                                entry: 'snapshot',
                                request: Schema.Struct({
                                    path: AbsolutePath,
                                    clip: _toolkit.tools.illustrator_snapshot.parametersSchema.schema.fields.clip,
                                    budget: Schema.Struct({ longEdgePx: Schema.Number, pixels: Schema.Number }),
                                    resolution: Schema.Struct({ minimum: Schema.Number, maximum: Schema.Number }),
                                }),
                                response: succeeded(_toolkit.tools.illustrator_snapshot),
                            },
                            { path: AbsolutePath.make(path.join(at.jobs, jobId, 'snapshot.png')), clip: input.clip, budget: BUDGET[input.budget], resolution: { minimum: 72, maximum: 2400 } },
                            jobId,
                        ),
                    );
                    const source = sharp(captured.path);
                    const { width, height } = yield* Effect.tryPromise({ try: () => source.metadata(), catch: notDecodable(HOSTS.illustrator.id, captured.path) });
                    const budget = BUDGET[input.budget];
                    const scale = Math.min(1, budget.longEdgePx / Math.max(width, height), Math.sqrt(budget.pixels / (width * height)));
                    const { data, info } = yield* Effect.tryPromise({
                        try: () =>
                            source.resize({ width: Math.max(1, Math.floor(width * scale)), height: Math.max(1, Math.floor(height * scale)), fit: 'inside', withoutEnlargement: true }).toUint8Array(),
                        catch: notDecodable(HOSTS.illustrator.id, captured.path),
                    });
                    yield* Effect.mapError(fs.writeFile(captured.path, data), inaccessible(HOSTS.illustrator.id));
                    const drift = yield* Option.match(input.baseline, {
                        onNone: () => Effect.succeed(Option.none()),
                        onSome: (baseline) =>
                            fingerprint(data).pipe(
                                Effect.flatMap((observed) => compare(baseline, { [HOSTS.illustrator.id]: observed })),
                                Effect.map(Option.some),
                            ),
                    });
                    return { ...captured, widthPx: info.width, heightPx: info.height, dpi: (captured.dpi * info.width) / width, drift };
                }),
            } satisfies Handlers<typeof _toolkit.tools>,
            answering,
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
