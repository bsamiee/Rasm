// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Brand, Chunk, Data, Effect, Function, HashMap, Iterable, Match, Option, Order, pipe, Record, Result, Schema, SchemaGetter, Struct } from 'effect';
import { type PageSize, POINTS } from './page-sizes.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Unit = keyof typeof POINTS;
type Span = typeof Span.Type;
type Positive = typeof Positive.Type;
type Count = typeof Count.Type;
type Lines = typeof Lines.Type;
type ColumnCount = typeof ColumnCount.Type;
type SubdivisionIndex = typeof SubdivisionIndex.Type;

interface Fitted {
    readonly lines: number;
    readonly leading: number;
}

interface Margins {
    readonly top: number;
    readonly bottom: number;
    readonly inside: number;
    readonly outside: number;
}

interface Combination {
    readonly count: number;
    readonly lines: number;
    readonly gutter: number;
}

interface SquareSize {
    readonly size: number;
    readonly height: number;
    readonly match: 'exact' | 'closest';
}

interface Step {
    readonly kind: 'none' | 'division' | 'multiplication';
    readonly factor: number;
}

interface Bounds {
    readonly top: number;
    readonly left: number;
    readonly bottom: number;
    readonly right: number;
}

interface PageGrid {
    readonly bounds: Bounds;
    readonly tracks: Readonly<Record<'columns' | 'rows', Brand.Branded<Array.NonEmptyReadonlyArray<{ readonly start: number; readonly end: number }>, 'RegularTracks'>>>;
    readonly gutters: Readonly<Record<'columns' | 'rows', number>>;
    readonly baselines: Array.NonEmptyReadonlyArray<number>;
    readonly imageLines: Array.NonEmptyReadonlyArray<number>;
    readonly strip: Option.Option<Bounds>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAX_ARRAY_LENGTH = 0xff_ff_ff_ff;
const RULE = {
    rounding: { half: 0.5, precision: 1000, tolerance: 1e-8 },
    pageInches: { minimum: 0.0139, maximum: 216 },
    modulePoints: { minimum: 1, maximum: 1000 },
    columns: 2,
    entries: 41,
    steps: 4,
} as const;

// --- [ERRORS] --------------------------------------------------------------------------

type GridError = Data.TaggedEnum<{
    readonly leadingOutOfRange: { readonly span: number; readonly leading: number; readonly reason: 'noLine' | 'belowOnePoint' | 'atOrAbovePage' | 'unrepresentable' };
    readonly combinationsOutOfRange: {
        readonly span: number;
        readonly leading: number;
        readonly imageLine: Option.Option<number>;
        readonly fields: Array.NonEmptyReadonlyArray<'leading' | 'imageLine'>;
    };
    readonly capacityExceeded: { readonly requested: number; readonly maximum: number };
    readonly invalidGutter: { readonly gutter: number };
    readonly gutterTooLarge: { readonly count: number; readonly gutter: number };
    readonly moduleSizeOutOfRange: { readonly span: number; readonly modules: number; readonly size: number };
    readonly squareAboveLeading: { readonly gridWidth: number; readonly leading: number };
}>;

const GridError: Data.TaggedEnum.Constructor<GridError> = Data.taggedEnum<GridError>();
type GeometryError = typeof GeometryError.Type;
const GeometryError: Schema.TaggedUnion<{
    readonly invalidPageGrid: Schema.TaggedStruct<
        'invalidPageGrid',
        { readonly fields: Schema.NonEmptyArray<Schema.Literals<readonly ['page', 'ink', 'module', 'tracks', 'columns', 'rows', 'baselines', 'strip']>> }
    >;
    readonly invalidTrackDemand: Schema.TaggedStruct<
        'invalidTrackDemand',
        {
            readonly entries: Schema.NonEmptyArray<Schema.Struct<{ readonly index: Schema.Number; readonly fields: Schema.NonEmptyArray<Schema.Literals<readonly ['minimum', 'preferred']>> }>>;
        }
    >;
    readonly tracksDoNotFit: Schema.TaggedStruct<
        'tracksDoNotFit',
        { readonly axis: Schema.Literals<readonly ['columns', 'rows']>; readonly minimum: Schema.NonEmptyArray<Schema.Number>; readonly available: Schema.Number; readonly tracks: Schema.Number }
    >;
    readonly captionDoesNotFit: Schema.TaggedStruct<'captionDoesNotFit', { readonly height: Schema.Number; readonly gap: Schema.Number; readonly available: Schema.Number }>;
    readonly squareSizeOutOfRange: Schema.TaggedStruct<'squareSizeOutOfRange', { readonly size: Schema.Number; readonly measuredSize: Schema.Number; readonly extent: Schema.Number }>;
}> = Schema.TaggedUnion({
    invalidPageGrid: { fields: Schema.NonEmptyArray(Schema.Literals(['page', 'ink', 'module', 'tracks', 'columns', 'rows', 'baselines', 'strip'])) },
    invalidTrackDemand: { entries: Schema.NonEmptyArray(Schema.Struct({ index: Schema.Number, fields: Schema.NonEmptyArray(Schema.Literals(['minimum', 'preferred'])) })) },
    tracksDoNotFit: { axis: Schema.Literals(['columns', 'rows']), minimum: Schema.NonEmptyArray(Schema.Number), available: Schema.Number, tracks: Schema.Number },
    captionDoesNotFit: { height: Schema.Number, gap: Schema.Number, available: Schema.Number },
    squareSizeOutOfRange: { size: Schema.Number, measuredSize: Schema.Number, extent: Schema.Number },
});

// --- [PRIMITIVES] ----------------------------------------------------------------------

const round = (value: number): number => Math.floor(value + RULE.rounding.half);

const fitSpan = (span: number, value: number): Result.Result<Fitted, GridError> => {
    if (!(Number.isFinite(span) && span > 0 && Number.isFinite(value) && value > 0)) {
        return Result.fail(GridError.leadingOutOfRange({ span, leading: value, reason: span <= 0 ? 'noLine' : 'unrepresentable' }));
    }
    const requested = span / value;
    const lower = Math.max(1, Math.floor(requested));
    const upper = Math.max(1, Math.ceil(requested));
    const lines = Math.abs(span / lower - value) <= Math.abs(span / upper - value) ? lower : upper;
    const leading = span / lines;
    return Number.isSafeInteger(lines) && lines > 0 && Number.isFinite(leading) && leading > 0
        ? Result.succeed({ lines, leading })
        : Result.fail(GridError.leadingOutOfRange({ span, leading: value, reason: 'unrepresentable' }));
};

const quantize = (value: number): number => round(value * RULE.rounding.precision) / RULE.rounding.precision;

const greaterThan = (left: number, right: number): boolean => left > right + RULE.rounding.tolerance;

const atLeast = (left: number, right: number): boolean => left >= right - RULE.rounding.tolerance;

const within = (left: number, right: number): boolean => Math.abs(left - right) < RULE.rounding.tolerance;

// --- [MODELS] --------------------------------------------------------------------------

const Quantized: Schema.Codec<number> = Schema.Number.pipe(Schema.decodeTo(Schema.Number, { decode: SchemaGetter.passthrough(), encode: SchemaGetter.transform(quantize) }));
const Points = (unit: Unit): Schema.Codec<number> =>
    Schema.Number.pipe(
        Schema.decodeTo(Schema.Number, { decode: SchemaGetter.transform((value: number) => value * POINTS[unit]), encode: SchemaGetter.transform((points: number) => points / POINTS[unit]) }),
    );
const Span: Schema.brand<Schema.Number, 'Span'> = Schema.Number.pipe(
    Schema.check(Schema.isBetween({ minimum: RULE.pageInches.minimum * POINTS.in, maximum: RULE.pageInches.maximum * POINTS.in })),
    Schema.brand('Span'),
);
const Positive: Schema.brand<Schema.Number, 'Positive'> = Schema.Number.pipe(Schema.check(Schema.isGreaterThan(0)), Schema.brand('Positive'));
const Count: Schema.brand<Schema.Int, 'Count'> = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(1)), Schema.brand('Count'));
const Lines: Schema.brand<Schema.Int, 'Lines'> = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)), Schema.brand('Lines'));
const ColumnCount: Schema.brand<Schema.Int, 'ColumnCount'> = Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(RULE.columns)), Schema.brand('ColumnCount'));
const SubdivisionIndex: Schema.brand<Schema.Int, 'SubdivisionIndex'> = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 2 * RULE.steps })), Schema.brand('SubdivisionIndex'));
const RegularTracks: Brand.Constructor<PageGrid['tracks']['columns']> = Brand.nominal();

const _Axis: Schema.Literals<readonly ['columns', 'rows']> = Schema.Literals(['columns', 'rows']);
const _NonNegative: Schema.Finite = Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0));
const _Positive: Schema.Finite = Schema.Finite.check(Schema.isGreaterThan(0));
const _Module: Schema.Struct<{ readonly count: Schema.OptionFromOptionalKey<typeof Count>; readonly size: Schema.Finite; readonly subdivisions: typeof Count }> = Schema.Struct({
    count: Schema.OptionFromOptionalKey(Count),
    size: _Positive,
    subdivisions: Count,
});
const _Division: Schema.Struct<{ readonly size: Schema.Finite; readonly subdivisions: typeof Count }> = Schema.Struct({ size: _Positive, subdivisions: Count });
const _Baseline: Schema.Struct<{ readonly origin: Schema.Literals<readonly ['page', 'margin']>; readonly start: Schema.Finite }> = Schema.Struct({
    origin: Schema.Literals(['page', 'margin']),
    start: _NonNegative,
});
const _Sides: Schema.Struct<Readonly<Record<keyof Margins, Schema.Finite>>> = Schema.Struct({ top: _NonNegative, bottom: _NonNegative, inside: _NonNegative, outside: _NonNegative });
const _OptionalSize: Schema.OptionFromOptionalKey<Schema.Finite> = Schema.OptionFromOptionalKey(_Positive);
const _OptionalSides: Schema.OptionFromOptionalKey<typeof _Sides> = Schema.OptionFromOptionalKey(_Sides);
const _Track: Schema.Struct<Readonly<Record<'start' | 'end', Schema.Finite>>> = Schema.Struct({ start: Schema.Finite, end: Schema.Finite });
const _LayoutLevel: Schema.Struct<{
    readonly axis: typeof _Axis;
    readonly role: Schema.Literals<readonly ['main', 'sub', 'secondary']>;
    readonly fit: Schema.Literals<readonly ['margins', 'page']>;
    readonly unit: Schema.Literals<readonly ['pt', 'line']>;
    readonly gutter: Schema.Finite;
    readonly fields: Schema.TaggedUnion<{
        readonly equal: Schema.TaggedStruct<'equal', { readonly count: typeof Count; readonly size: Schema.OptionFromOptionalKey<Schema.Finite> }>;
        readonly unequal: Schema.TaggedStruct<'unequal', { readonly sizes: Schema.NonEmptyArray<Schema.Finite> }>;
    }>;
}> = Schema.Struct({
    axis: _Axis,
    role: Schema.Literals(['main', 'sub', 'secondary']),
    fit: Schema.Literals(['margins', 'page']),
    unit: Schema.Literals(['pt', 'line']),
    gutter: _NonNegative,
    fields: Schema.TaggedUnion({ equal: { count: Count, size: Schema.OptionFromOptionalKey(_Positive) }, unequal: { sizes: Schema.NonEmptyArray(_Positive) } }),
});

const GridProblem: Schema.TaggedStruct<
    'GridProblem',
    { readonly context: Schema.String; readonly reason: Schema.Literals<readonly ['missing', 'invalid', 'unsupported', 'geometry']>; readonly cause: Schema.Defect }
> = Schema.TaggedStruct('GridProblem', {
    context: Schema.String,
    reason: Schema.Literals(['missing', 'invalid', 'unsupported', 'geometry']),
    cause: Schema.Defect(),
});
type GridProblem = typeof GridProblem.Type;

/** A physical grid request; horizontal lines use grid subdivisions and vertical lines use baseline leading */
const GridDefinition: Schema.Struct<{
    readonly width: typeof Span;
    readonly height: typeof Span;
    readonly facingPages: Schema.Boolean;
    readonly baseline: Schema.OptionFromOptionalKey<typeof _Baseline>;
    readonly rhythm: Schema.TaggedUnion<{
        readonly quick: Schema.TaggedStruct<
            'quick',
            {
                readonly leading: Schema.Finite;
                readonly fitLeading: Schema.Boolean;
                readonly subdivision: typeof SubdivisionIndex;
                readonly square: Schema.Boolean;
                readonly horizontal: Schema.OptionFromOptionalKey<Schema.Finite>;
            }
        >;
        readonly modular: Schema.TaggedStruct<'modular', Readonly<Record<'columns' | 'rows', typeof _Module>>>;
        readonly applied: Schema.TaggedStruct<'applied', Readonly<Record<'columns' | 'rows', typeof _Division>> & { readonly leading: Schema.Finite }>;
    }>;
    readonly layouts: Schema.NonEmptyArray<
        Schema.Struct<{
            readonly index: typeof Count;
            readonly pages: typeof Count;
            readonly margins: Schema.$Record<typeof _Axis, Schema.Struct<{ readonly before: Schema.Finite; readonly after: Schema.Finite; readonly unit: Schema.Literals<readonly ['pt', 'line']> }>>;
            readonly imageLine: Schema.OptionFromOptionalKey<Schema.Finite>;
            readonly levels: Schema.$Array<typeof _LayoutLevel>;
            readonly typeArea: Schema.$Record<typeof _Axis, Schema.OptionFromOptionalKey<typeof Count>>;
        }>
    >;
    readonly bleed: Schema.OptionFromOptionalKey<typeof _Sides>;
    readonly slug: Schema.OptionFromOptionalKey<typeof _Sides>;
}> = Schema.Struct({
    width: Span,
    height: Span,
    facingPages: Schema.Boolean,
    baseline: Schema.OptionFromOptionalKey(_Baseline),
    rhythm: Schema.TaggedUnion({
        quick: { leading: _Positive, fitLeading: Schema.Boolean, subdivision: SubdivisionIndex, square: Schema.Boolean, horizontal: _OptionalSize },
        modular: { columns: _Module, rows: _Module },
        applied: { columns: _Division, rows: _Division, leading: _Positive },
    }),
    layouts: Schema.NonEmptyArray(
        Schema.Struct({
            index: Count,
            pages: Count,
            margins: Schema.Record(_Axis, Schema.Struct({ before: _NonNegative, after: _NonNegative, unit: Schema.Literals(['pt', 'line']) })),
            imageLine: _OptionalSize,
            levels: Schema.Array(_LayoutLevel),
            typeArea: Schema.Record(_Axis, Schema.OptionFromOptionalKey(Count)),
        }),
    ),
    bleed: _OptionalSides,
    slug: _OptionalSides,
});
type GridDefinition = typeof GridDefinition.Type;

/** Exact application geometry, independent of native page coordinates and ruler preferences */
const GridResolution: Schema.Struct<
    Pick<typeof GridDefinition.fields, 'width' | 'height' | 'facingPages' | 'bleed' | 'slug'> & {
        readonly grid: Schema.$Record<typeof _Axis, typeof _Division>;
        readonly leading: Schema.Finite;
        readonly baseline: typeof _Baseline;
        readonly layouts: Schema.$Array<
            Schema.Struct<{
                readonly index: typeof Count;
                readonly pages: typeof Count;
                readonly margins: typeof _Sides;
                readonly tracks: Schema.$Record<typeof _Axis, Schema.NonEmptyArray<typeof _Track>>;
                readonly guides: Schema.$Record<typeof _Axis, Schema.$Array<Schema.Finite>>;
            }>
        >;
    }
> = Schema.Struct({
    width: Span,
    height: Span,
    facingPages: Schema.Boolean,
    grid: Schema.Record(_Axis, _Division),
    leading: _Positive,
    baseline: _Baseline,
    layouts: Schema.Array(
        Schema.Struct({
            index: Count,
            pages: Count,
            margins: _Sides,
            tracks: Schema.Record(_Axis, Schema.NonEmptyArray(_Track)),
            guides: Schema.Record(_Axis, Schema.Array(Schema.Finite)),
        }),
    ),
    bleed: _OptionalSides,
    slug: _OptionalSides,
});
type GridResolution = typeof GridResolution.Type;

// --- [PAGE GEOMETRY] -------------------------------------------------------------------

const resolveGrid = (page: PageSize, ink: { readonly above: number; readonly below: number }): Result.Result<PageGrid, GeometryError> => {
    const bounds: Bounds = Option.match(page.sheet, {
        onNone: () => ({ top: page.firstBaseline - ink.above, left: page.margins.left, bottom: page.lastBaseline + ink.below, right: page.width - page.margins.right }),
        onSome: ({ border }) => ({ top: border.top, left: border.left, bottom: page.height - border.bottom, right: page.width - border.right }),
    });
    const source = Option.match(page.sheet, {
        onNone: () =>
            Option.map(Option.all({ columns: page.columns, rows: page.rows }), ({ columns: horizontal, rows: vertical }) => ({
                columns: { start: bounds.left, end: bounds.right, count: horizontal.count, gutter: horizontal.gutter },
                rows: { start: bounds.top, end: bounds.bottom, count: vertical.count, gutter: page.module + vertical.gutter - ink.above - ink.below },
            })),
        onSome: ({ modules, strip }) =>
            Option.some({
                columns: { start: bounds.left, end: bounds.right - strip, count: modules.columns, gutter: 0 },
                rows: { start: bounds.top, end: bounds.bottom, count: modules.rows, gutter: 0 },
            }),
    });
    if (Option.isNone(source)) {
        return Result.fail(GeometryError.cases.invalidPageGrid.make({ fields: ['tracks'] }));
    }
    const axes = source.value;
    const first = bounds.top + ink.above;
    const last = bounds.bottom - ink.below;
    const baselineCount = Math.floor((last - first + RULE.rounding.tolerance) / page.module) + 1;
    const representable = Record.map({ columns: axes.columns.count, rows: axes.rows.count, baselines: baselineCount }, (count) => Number.isInteger(count) && count > 0 && count <= MAX_ARRAY_LENGTH);
    const valid = {
        page: Number.isFinite(page.width) && Number.isFinite(page.height) && bounds.top >= 0 && bounds.left >= 0 && bounds.right <= page.width && bounds.bottom <= page.height,
        ink: Number.isFinite(ink.above) && Number.isFinite(ink.below) && ink.above > 0 && ink.below >= 0 && atLeast(page.module, ink.above + ink.below),
        module: Number.isFinite(page.module) && page.module > 0,
        ...Record.map(
            axes,
            ({ start, end, count, gutter }, axis) => representable[axis] && Number.isFinite(gutter) && gutter >= 0 && Number.isFinite(end - start) && end - start > (count - 1) * gutter,
        ),
        baselines:
            representable.baselines &&
            first <= last &&
            (Option.isSome(page.sheet) ||
                Option.exists(
                    page.rows,
                    ({ count, lines, gutter }) =>
                        Number.isInteger(lines) &&
                        lines > 0 &&
                        gutter >= 0 &&
                        within(gutter, round(gutter / page.module) * page.module) &&
                        within(page.lastBaseline, page.firstBaseline + (count * lines - 1) * page.module + (count - 1) * gutter),
                )),
        strip: Option.isNone(page.sheet) || Option.exists(page.sheet, ({ strip }) => Number.isFinite(strip) && strip > 0 && strip < bounds.right - bounds.left),
    };
    const fields = Array.filter(Record.keys(valid), (key) => !valid[key]);
    if (Array.isArrayNonEmpty(fields)) {
        return Result.fail(GeometryError.cases.invalidPageGrid.make({ fields }));
    }
    const tracks = Record.map(axes, ({ start, end, count, gutter }) => {
        const span = (end - start - (count - 1) * gutter) / count;
        return Array.makeBy(count, (index) => ({ start: start + index * (span + gutter), end: start + index * (span + gutter) + span }));
    });
    const invalid = Array.filter(Record.keys(tracks), (axis) => !Array.every(tracks[axis], ({ start, end }) => Number.isFinite(start) && Number.isFinite(end) && end > start));
    if (Array.isArrayNonEmpty(invalid)) {
        return Result.fail(GeometryError.cases.invalidPageGrid.make({ fields: invalid }));
    }
    const baselines = Array.makeBy(baselineCount, (index) => first + index * page.module);
    return Result.succeed({
        bounds,
        tracks: Record.map(tracks, RegularTracks),
        gutters: Record.map(axes, ({ count, gutter }) => (count > 1 ? gutter : 0)),
        baselines,
        imageLines: Array.map(baselines, (baseline) => baseline - ink.above),
        strip: Option.map(page.sheet, () => ({ ...bounds, left: axes.columns.end })),
    });
};

const partition = <A>(
    tracks: PageGrid['tracks'],
    axis: keyof PageGrid['tracks'],
    demands: Array.NonEmptyReadonlyArray<{ readonly item: A; readonly minimum: number; readonly preferred: number }>,
): Result.Result<Array.NonEmptyReadonlyArray<{ readonly item: A; readonly tracks: PageGrid['tracks'] }>, GeometryError> => {
    const invalid = Array.filterMap(demands, ({ minimum, preferred }, index) => {
        const extents = { minimum, preferred };
        const fields = Array.filter(Record.keys(extents), (key) => !Number.isFinite(extents[key]) || extents[key] <= 0);
        return Array.isArrayNonEmpty(fields) ? Result.succeed({ index, fields }) : Result.failVoid;
    });
    if (Array.isArrayNonEmpty(invalid)) {
        return Result.fail(GeometryError.cases.invalidTrackDemand.make({ entries: invalid }));
    }
    const source = tracks[axis];
    const { start } = Array.headNonEmpty(source);
    const required = pipe(
        demands,
        Array.map((demand) =>
            pipe(
                Array.findFirstIndex(source, ({ end }) => atLeast(end - start, demand.minimum)),
                Option.map((index) => ({ ...demand, count: index + 1 })),
            ),
        ),
        Option.all,
        Option.map((entries) => ({ entries, count: Array.reduce(entries, 0, (sum, entry) => sum + entry.count) })),
        Option.filter(({ count }) => count <= source.length),
    );
    if (Option.isNone(required)) {
        return Result.fail(
            GeometryError.cases.tracksDoNotFit.make({ axis, minimum: Array.map(demands, Struct.get('minimum')), available: Array.lastNonEmpty(source).end - start, tracks: source.length }),
        );
    }
    const allocated = Array.reduce(Array.take(source, source.length - required.value.count), required.value.entries, (entries) => {
        const next = Array.min(
            entries,
            Order.mapInput(Order.Number, ({ count, preferred }: { readonly count: number; readonly preferred: number }) => Array.getUnsafe(source, count - 1).end - start - preferred),
        );
        return Array.map(entries, (entry) => (entry === next ? { ...entry, count: entry.count + 1 } : entry));
    });
    const [, selected] = Array.mapAccum(allocated, 0, (offset, { item, count }) => [
        offset + count,
        { item, tracks: { ...tracks, [axis]: RegularTracks(Array.makeBy(count, (index) => Array.getUnsafe(source, offset + index))) } },
    ]);
    return Result.succeed(selected);
};

const fieldBounds = (
    tracks: PageGrid['tracks'],
    caption: Option.Option<{ readonly height: number; readonly gap: number }>,
): Result.Result<{ readonly body: Bounds; readonly caption: Option.Option<Bounds> }, GeometryError> => {
    const selected = Record.map(tracks, (axis) => ({ start: Array.headNonEmpty(axis).start, end: Array.lastNonEmpty(axis).end }));
    const body: Bounds = { top: selected.rows.start, left: selected.columns.start, bottom: selected.rows.end, right: selected.columns.end };
    return Option.match(caption, {
        onNone: () => Result.succeed({ body, caption: Option.none() }),
        onSome: ({ height, gap }) =>
            Number.isFinite(height) && Number.isFinite(gap) && height > 0 && gap >= 0 && body.bottom - body.top > height + gap
                ? Result.succeed({ body: { ...body, bottom: body.bottom - height - gap }, caption: Option.some({ ...body, top: body.bottom - height }) })
                : Result.fail(GeometryError.cases.captionDoesNotFit.make({ height, gap, available: body.bottom - body.top })),
    });
};

// --- [MODES] ---------------------------------------------------------------------------

const fitLeading = ({ height, leading }: { readonly height: Span; readonly leading: Positive }): Result.Result<Fitted, GridError> =>
    Result.flatMap(fitSpan(height, leading), (fitted) =>
        Option.match(
            Array.findFirst(
                [
                    ['belowOnePoint', !atLeast(fitted.leading, 1)],
                    ['atOrAbovePage', atLeast(fitted.leading, height)],
                ] as const,
                ([, failed]) => failed,
            ),
            {
                onNone: () => Result.succeed(fitted),
                onSome: ([reason]) => Result.fail(GridError.leadingOutOfRange({ span: height, leading, reason })),
            },
        ),
    );

const quickUnits = ({ width, height, leading }: { readonly width: Span; readonly height: Span; readonly leading: Positive }): Result.Result<Fitted & { readonly horizontal: number }, GridError> =>
    Result.map(fitLeading({ height, leading }), (fitted) => ({ ...fitted, horizontal: width / fitted.lines }));

const subdivision = ({
    height,
    leading,
    index,
}: {
    readonly height: Span;
    readonly leading: Positive;
    readonly index: SubdivisionIndex;
}): Result.Result<Fitted & Step & { readonly fine: number }, GridError> => {
    const step = Match.value(index).pipe(
        Match.withReturnType<Step>(),
        Match.when(0, () => ({ kind: 'none', factor: 1 })),
        Match.when(
            (steps: number) => steps <= RULE.steps,
            (steps) => ({ kind: 'division', factor: steps + 1 }),
        ),
        Match.orElse((steps) => ({ kind: 'multiplication', factor: steps - RULE.steps + 1 })),
    );
    return Result.flatMap(fitLeading({ height, leading }), (fitted) =>
        Result.map(step.kind === 'multiplication' ? fitSpan(height, leading * step.factor) : Result.succeed(fitted), (applied) => ({ ...step, ...applied, fine: applied.leading / step.factor })),
    );
};

const modular = ({
    span,
    modules,
    moduleSize,
    subdivisions,
}: {
    readonly span: Span;
    readonly modules: Option.Option<Count>;
    readonly moduleSize: Positive;
    readonly subdivisions: Count;
}): Result.Result<{ readonly modules: number; readonly size: number; readonly unit: number; readonly division: number }, GridError> =>
    Result.flatMap(
        Option.match(modules, {
            onNone: (): Result.Result<number, GridError> => Result.map(fitSpan(span, moduleSize), Struct.get('lines')),
            onSome: (count): Result.Result<number, GridError> => {
                const size = span / count;
                return greaterThan(RULE.modulePoints.minimum, size) || greaterThan(size, RULE.modulePoints.maximum)
                    ? Result.fail(GridError.moduleSizeOutOfRange({ span, modules: count, size }))
                    : Result.succeed(count);
            },
        }),
        (count) => {
            const division = count * subdivisions;
            return Number.isSafeInteger(division)
                ? Result.succeed({ modules: count, size: span / count, unit: span / division, division })
                : Result.fail(GridError.capacityExceeded({ requested: division, maximum: Number.MAX_SAFE_INTEGER }));
        },
    );

const lineMargin = ({ margin, unit }: { readonly margin: number; readonly unit: Positive }): { readonly lines: number; readonly applied: number } => {
    const lines = round(margin / unit);
    return { lines, applied: lines * unit };
};

const snappedSpan = ({ span, height, leading, imageLine }: { readonly span: number; readonly height: Span; readonly leading: Positive; readonly imageLine: Option.Option<Positive> }): number =>
    Option.match(imageLine, {
        onNone: () => {
            const snapped = round(span / leading) * leading;
            return atLeast(1 / RULE.rounding.precision, snapped) ? height : snapped;
        },
        onSome: (image) => round((span + image) / leading) * leading - image,
    });

const squareGrid = ({ width, vertical }: { readonly width: Span; readonly vertical: Positive }): { readonly lines: number; readonly width: number; readonly unit: number } => {
    const rounded = round(width / vertical);
    const lines = greaterThan(width, rounded * vertical) ? rounded + 1 : rounded;
    return { lines, width: lines * vertical, unit: vertical };
};

const columns = ({
    span,
    count,
    gutter,
}: {
    readonly span: Span;
    readonly count: ColumnCount;
    readonly gutter: number;
}): Result.Result<{ readonly width: number; readonly runs: readonly number[] }, GridError> => {
    if (!(Number.isFinite(gutter) && gutter >= 0)) {
        return Result.fail(GridError.invalidGutter({ gutter }));
    }
    const width = (span - (count - 1) * gutter) / count;
    const length = 2 * count - 1;
    if (length > MAX_ARRAY_LENGTH) {
        return Result.fail(GridError.capacityExceeded({ requested: length, maximum: MAX_ARRAY_LENGTH }));
    }
    return atLeast(width, 0) ? Result.succeed({ width, runs: Array.makeBy(length, (index) => (index % 2 === 0 ? width : gutter)) }) : Result.fail(GridError.gutterTooLarge({ count, gutter }));
};

const smartLevel =
    (rounding: (value: number) => number) =>
    ({
        lines,
        before,
        after,
        gutter,
        count,
        unit,
    }: {
        readonly lines: Lines;
        readonly before: number;
        readonly after: number;
        readonly gutter: number;
        readonly count: Count;
        readonly unit: Positive;
    }): { readonly before: number; readonly after: number; readonly gutter: number; readonly field: number } => {
        const beforeLines = round(before / unit);
        const gutterLines = round(gutter / unit);
        const field = rounding((lines - beforeLines - round(after / unit) - (count - 1) * gutterLines) / count);
        return { before: beforeLines, after: lines - beforeLines - count * field - (count - 1) * gutterLines, gutter: gutterLines, field };
    };

const combinations = ({
    span,
    leading,
    imageLine,
    sort,
    direction,
}: {
    readonly span: Span;
    readonly leading: Positive;
    readonly imageLine: Option.Option<Positive>;
    readonly sort: keyof Combination;
    readonly direction: 'ascending' | 'descending';
}): Result.Result<readonly Combination[], GridError> => {
    const image = Option.getOrElse(imageLine, () => 0);
    const total = Option.isNone(imageLine) ? round(span / leading) : Math.ceil(span / leading);
    const valid = { leading: Number.isFinite(leading) && Number.isSafeInteger(total), imageLine: Number.isFinite(image) };
    const fields = Array.filter(Record.keys(valid), (field) => !valid[field]);
    if (Array.isArrayNonEmpty(fields)) {
        return Result.fail(GridError.combinationsOutOfRange({ span, leading, imageLine, fields }));
    }
    const offset = Option.isSome(imageLine) ? 1 : 0;
    const start = { line: 2 * leading + image, gutter: (1 + offset) * leading - image };
    const halfQuantum = RULE.rounding.half / RULE.rounding.precision;
    const intervals = pipe(
        Iterable.range(1, RULE.entries - 1),
        Iterable.flatMap((count) => {
            const minimum = Math.max(
                2,
                Math.floor((span - image - (count + halfQuantum) * (2 * image + 1 / RULE.rounding.precision + RULE.rounding.tolerance)) / ((2 * (count + halfQuantum) + 1) * leading)),
            );
            const maximum =
                Math.floor(Math.min((span / 2 - image + RULE.rounding.tolerance) / leading, (span - image - (count - halfQuantum) * (1 + offset) * leading) / ((count - halfQuantum + 1) * leading))) +
                1;
            return pipe(
                Iterable.range(minimum, maximum),
                Iterable.map((lines) => ({ count, lines, lineSpan: start.line + (lines - 2) * leading })),
            );
        }),
        Iterable.filter(({ lineSpan }) => atLeast(span / 2, lineSpan)),
        Iterable.flatMap(({ count, lines, lineSpan }) => {
            const numerator = span - lineSpan;
            const minimum = Math.max(1, Math.floor(numerator / (count + halfQuantum) / leading - lines - offset));
            const maximum =
                Math.floor(Math.min(numerator / (count - halfQuantum) / leading - lines - offset, lines + (2 * image + 1 / RULE.rounding.precision + RULE.rounding.tolerance) / leading - offset)) + 1;
            return minimum <= maximum ? Array.map([1, -1], (step) => ({ count, lines, lineSpan, minimum, maximum, step })) : [];
        }),
        Iterable.map(({ count, lines, lineSpan, minimum, maximum, step }) =>
            pipe(
                Iterable.range(minimum, maximum),
                Iterable.map((gutter) => (step === 1 ? gutter : maximum - (gutter - minimum))),
                Iterable.findFirst((gutter) => {
                    const gutterSpan = start.gutter + (gutter - 1) * leading;
                    return atLeast(1 / RULE.rounding.precision, gutterSpan - lineSpan) && within(quantize((span - lineSpan) / (lineSpan + gutterSpan)), count);
                }),
                Option.map((gutter) => ({ count: count + 1, lines, gutter })),
            ),
        ),
        Iterable.chunksOf(2),
        Iterable.map((bounds) =>
            Option.zipWith(Array.getUnsafe(bounds, 0), Array.getUnsafe(bounds, 1), (minimum, maximum) => ({
                count: minimum.count,
                lines: minimum.lines,
                minimum: minimum.gutter,
                maximum: maximum.gutter,
            })),
        ),
        Iterable.getSomes,
        Iterable.appendAll(
            pipe(
                Iterable.range(RULE.columns, RULE.entries - 1),
                Iterable.filter((count) => count <= Math.floor(total / 2)),
                Iterable.filterMap((count) => {
                    const available = total - offset * count;
                    return available % count === 0 && (offset === 0 || available / count > 1) ? Result.succeed({ count, lines: available / count, minimum: 0, maximum: 0 }) : Result.failVoid;
                }),
            ),
        ),
    );
    const capacity = pipe(
        intervals,
        Iterable.scan(0, (count, { minimum, maximum }) => count + maximum - minimum + 1),
        Iterable.findFirst((count) => count > MAX_ARRAY_LENGTH),
    );
    if (Option.isSome(capacity)) {
        return Result.fail(GridError.capacityExceeded({ requested: capacity.value, maximum: MAX_ARRAY_LENGTH }));
    }
    const sorted = pipe(
        intervals,
        Iterable.flatMap(({ count, lines, minimum, maximum }) =>
            pipe(
                Iterable.range(minimum, maximum),
                Iterable.map((gutter) => ({ count, lines, gutter })),
            ),
        ),
        Array.fromIterable,
        Array.sortBy(...Array.map(Array.dedupe([sort, 'count', 'lines', 'gutter'] as const), (key) => Order.mapInput(Order.Number, (entry: Combination) => entry[key]))),
    );
    return Result.succeed(direction === 'descending' ? Array.reverse(sorted) : sorted);
};

const browse = ({ count, index, step }: { readonly count: Lines; readonly index: Lines; readonly step: 'next' | 'previous' }): number =>
    ({ next: index === count ? 0 : index + 1, previous: index === 0 ? count : index - 1 })[step];

const proportions = ({ width, height, margins }: { readonly width: Span; readonly height: Span; readonly margins: Margins }): { readonly document: number; readonly typeArea: number } => ({
    document: height / width,
    typeArea: (height - margins.top - margins.bottom) / (width - margins.inside - margins.outside),
});

const verticalValue = ({
    width,
    height,
    margins,
    leading,
    imageLine,
}: {
    readonly width: Span;
    readonly height: Span;
    readonly margins: Margins;
    readonly leading: Positive;
    readonly imageLine: Option.Option<Positive>;
}): Result.Result<Fitted & { readonly span: number; readonly horizontal: number; readonly modules: number }, GridError> => {
    const span = height - (margins.top + margins.bottom);
    const measure = width - (margins.inside + margins.outside);
    const fitted = Option.match(imageLine, { onNone: () => span, onSome: (image) => span + image });
    return Result.flatMap(fitSpan(fitted, leading), (grid) =>
        span > 0 && Number.isFinite(measure) && measure > 0
            ? Result.succeed({ ...grid, span, horizontal: measure / grid.lines, modules: grid.lines })
            : Result.fail(GridError.leadingOutOfRange({ span: fitted, leading, reason: 'unrepresentable' })),
    );
};

const unfittedLeading = ({
    width,
    height,
    leading,
    top,
    bottom,
    imageLine,
}: {
    readonly width: Span;
    readonly height: Span;
    readonly leading: Positive;
    readonly top: Lines;
    readonly bottom: Lines;
    readonly imageLine: Option.Option<Positive>;
}): Result.Result<
    Fitted & { readonly remainder: number; readonly top: number; readonly bottom: number; readonly horizontal: number; readonly modules: number; readonly subdivisions: readonly number[] },
    GridError
> => {
    const image = Option.getOrElse(imageLine, () => 0);
    if (!Number.isFinite(image)) {
        return Result.fail(GridError.leadingOutOfRange({ span: height, leading, reason: 'unrepresentable' }));
    }
    return Result.flatMap(fitSpan(height, leading), (fitted) => {
        const nearest = round(height / leading);
        const lines = greaterThan(nearest * leading, height) ? nearest - 1 : nearest;
        const remainder = Math.max(0, height - lines * leading);
        const factors = Array.range(2, RULE.steps + 1);
        const grid = {
            leading,
            lines,
            remainder,
            top: top * leading + image,
            bottom: remainder + bottom * leading,
            horizontal: width / fitted.lines,
            modules: fitted.lines,
            subdivisions: Array.appendAll(
                Array.map(factors, (factor) => leading / factor),
                Array.map(factors, (factor) => leading * factor),
            ),
        };
        return Number.isSafeInteger(lines) && Array.every([grid.top, grid.bottom, ...grid.subdivisions], Number.isFinite)
            ? Result.succeed(grid)
            : Result.fail(GridError.leadingOutOfRange({ span: height, leading, reason: 'unrepresentable' }));
    });
};

const imageLineHeight = ({ size, unitsPerEm, box }: { readonly size: Positive; readonly unitsPerEm: Count; readonly box: { readonly minY: number; readonly maxY: number } }): number =>
    ((box.maxY - box.minY) * size) / unitsPerEm;

const imageLineTop = ({
    lines,
    top,
    bottom,
    leading,
    height,
}: {
    readonly lines: Lines;
    readonly top: Lines;
    readonly bottom: Lines;
    readonly leading: Positive;
    readonly height: Positive;
}): { readonly top: number; readonly span: number } => ({ top: top * leading + height, span: (lines - top - bottom) * leading - height });

const squareSize = (
    { size: measuredSize, extent }: { readonly size: Positive; readonly extent: Positive },
    { gridWidth, leading }: { readonly gridWidth: Positive; readonly leading: Positive },
): Result.Result<SquareSize, GridError | GeometryError> => {
    const target = quantize(gridWidth);
    if (greaterThan(target, leading)) {
        return Result.fail(GridError.squareAboveLeading({ gridWidth: target, leading }));
    }
    const proportion = extent / measuredSize;
    const requested = (target / proportion) * RULE.rounding.precision;
    if (!(Array.every([measuredSize, extent, leading, proportion, requested], Number.isFinite) && target > 0 && proportion > 0)) {
        return Result.fail(GeometryError.cases.squareSizeOutOfRange.make({ size: requested / RULE.rounding.precision, measuredSize, extent }));
    }
    const candidates = Array.filterMap(Array.dedupe([Math.max(1, Math.floor(requested)), Math.max(1, Math.ceil(requested))]), (quanta) => {
        const size = quanta / RULE.rounding.precision;
        const height = quantize(size * proportion);
        return Number.isSafeInteger(quanta) && Number.isFinite(height) && height > 0 && atLeast(leading, height)
            ? Result.succeed({ size, height, match: within(height, target) ? ('exact' as const) : ('closest' as const) })
            : Result.failVoid;
    });
    return Array.isArrayNonEmpty(candidates)
        ? Result.succeed(
              Array.min(
                  candidates,
                  Order.mapInput(Order.Tuple([Order.Number, Order.Number]), (candidate: SquareSize): readonly [number, number] => [
                      Math.abs(candidate.height - target),
                      Math.abs(candidate.size * proportion - target),
                  ]),
              ),
          )
        : Result.fail(GeometryError.cases.squareSizeOutOfRange.make({ size: requested / RULE.rounding.precision, measuredSize, extent }));
};

const basedOn = ({
    height,
    size,
    leading,
    autoLeading,
}: {
    readonly height: Span;
    readonly size: Positive;
    readonly leading: Option.Option<Positive>;
    readonly autoLeading: Positive;
}): Result.Result<Fitted & { readonly desired: number; readonly size: number }, GridError> => {
    const desired = Option.getOrElse(leading, () => Positive.make(autoLeading * size));
    return Result.map(fitLeading({ height, leading: desired }), (fitted) => ({ ...fitted, desired, size }));
};

const lock = ({ sum, edited, other }: { readonly sum: Lines; readonly edited: Lines; readonly other: Lines }): { readonly edited: number; readonly other: number } =>
    atLeast(sum - edited, 0) ? { edited, other: sum - edited } : { edited: sum - other, other };

// --- [APPLICATION GEOMETRY] ------------------------------------------------------------

const layoutTracks = (level: typeof _LayoutLevel.Type, span: typeof _Track.Type, unit: number, imageLine: number): Result.Result<Array.NonEmptyReadonlyArray<typeof _Track.Type>, GridProblem> => {
    const scale = level.unit === 'line' ? unit : 1;
    const image = level.axis === 'rows' && level.unit === 'line' ? imageLine : 0;
    const gutter = level.gutter * scale + (image > 0 ? unit - image : 0);
    const count = level.fields._tag === 'equal' ? level.fields.count : level.fields.sizes.length;
    if (!(Number.isSafeInteger(count) && count > 0 && count <= MAX_ARRAY_LENGTH)) {
        return Result.fail(GridProblem.make({ context: `${level.axis}.${level.role}`, reason: 'geometry', cause: 'The field count exceeds the representable array capacity.' }));
    }
    const available = span.end - span.start - (count - 1) * gutter;
    const sizes =
        level.fields._tag === 'unequal'
            ? Array.map(level.fields.sizes, (size) => size * scale + image)
            : Array.replicate(Option.match(level.fields.size, { onNone: () => available / count, onSome: (size) => size * scale + image }), count);
    const [end, tracks] = Array.mapAccum(sizes, span.start, (start, size) => [start + size + gutter, { start, end: start + size }]);
    return Array.isArrayNonEmpty(tracks) &&
        gutter >= 0 &&
        Array.every(tracks, (track) => Number.isFinite(track.start) && Number.isFinite(track.end) && track.end > track.start) &&
        atLeast(span.end, end - gutter)
        ? Result.succeed(tracks)
        : Result.fail(GridProblem.make({ context: `${level.axis}.${level.role}`, reason: 'geometry', cause: 'The requested fields and gutters do not fit their containing span.' }));
};

const resolveGridDefinition: (definition: GridDefinition, baseline: GridResolution['baseline']) => Effect.Effect<GridResolution, Array.NonEmptyReadonlyArray<GridProblem>> = Effect.fnUntraced(
    function* (definition: GridDefinition, baseline: GridResolution['baseline']) {
        const { width, height, rhythm } = definition;
        const lattice = yield* Match.value(rhythm).pipe(
            Match.when({ _tag: 'applied' }, ({ columns: horizontal, rows: vertical, leading }) => Effect.succeed({ width, columns: horizontal, rows: vertical, leading })),
            Match.when({ _tag: 'modular' }, (input) =>
                Effect.gen(function* () {
                    const axes = yield* Effect.validate(Record.toEntries({ columns: width, rows: height }), ([axis, span]) =>
                        Effect.fromResult(modular({ span, modules: input[axis].count, moduleSize: Positive.make(input[axis].size), subdivisions: input[axis].subdivisions })),
                    );
                    const [horizontal, vertical] = axes;
                    if (!(horizontal && vertical)) {
                        return yield* Effect.die('Both document axes are required.');
                    }
                    return {
                        width,
                        columns: { size: horizontal.size, subdivisions: input.columns.subdivisions },
                        rows: { size: vertical.size, subdivisions: input.rows.subdivisions },
                        leading: vertical.unit,
                    };
                }),
            ),
            Match.when({ _tag: 'quick' }, (input) =>
                Effect.gen(function* () {
                    const divided = yield* Effect.fromResult(subdivision({ height, leading: Positive.make(input.leading), index: input.subdivision }));
                    const leading = input.fitLeading ? divided.leading : input.leading;
                    const horizontal = yield* Effect.fromResult(
                        Option.match(input.horizontal, {
                            onNone: () => Result.succeed((divided.leading * width) / height),
                            onSome: (size) => Result.map(fitSpan(width, size), Struct.get('leading')),
                        }),
                    );
                    const squared = squareGrid({ width, vertical: Positive.make(leading) });
                    const subdivisions = Count.make(divided.factor);
                    return {
                        width: input.square ? squared.width : width,
                        columns: { size: input.square ? squared.unit : horizontal, subdivisions },
                        rows: { size: leading, subdivisions },
                        leading,
                    };
                }),
            ),
            Match.exhaustive,
            Effect.mapError((cause) => [GridProblem.make({ context: 'rhythm', reason: 'geometry', cause })] as const),
        );
        const spans = { columns: lattice.width, rows: height };
        const unit = { columns: lattice.columns.size / lattice.columns.subdivisions, rows: lattice.leading };
        if (
            !(
                Number.isFinite(baseline.start) &&
                baseline.start >= 0 &&
                baseline.start < height &&
                Schema.is(Span)(lattice.width) &&
                lattice.leading < height &&
                Record.every(unit, (value) => Number.isFinite(value) && value > 0)
            )
        ) {
            return yield* Effect.fail([GridProblem.make({ context: 'rhythm', reason: 'geometry', cause: 'Page dimensions, leading or baseline phase exceed the physical page.' })] as const);
        }
        const prepared = Array.map(definition.layouts, (layout) => {
            const image = Option.getOrElse(layout.imageLine, Function.constant(0));
            const bounds = Record.map(layout.margins, ({ before, after, unit: marginUnit }, axis) => {
                const scale = marginUnit === 'line' ? unit[axis] : 1;
                const remainder = axis === 'rows' && marginUnit === 'line' && rhythm._tag === 'quick' && !rhythm.fitLeading ? height - Math.floor(height / lattice.leading) * lattice.leading : 0;
                return { start: before * scale + (axis === 'rows' && marginUnit === 'line' && image > 0 ? lattice.leading - image : 0), end: spans[axis] - after * scale - remainder };
            });
            return { ...layout, image, bounds, baselineStart: baseline.start + (baseline.origin === 'margin' ? bounds.rows.start : 0) };
        });
        const requests = Array.flatMap(prepared, (layout) => Array.map(layout.levels, (level) => ({ layout, level })));
        const invalid = Array.filter(
            prepared,
            ({ bounds, levels }) =>
                !Record.every(bounds, ({ start, end }, axis) => end > start && start >= 0 && end <= spans[axis]) ||
                Array.dedupeWith(levels, (left, right) => left.axis === right.axis && left.role === right.role).length !== levels.length,
        );
        if (Array.dedupe(Array.map(prepared, Struct.get('index'))).length !== prepared.length || invalid.length > 0) {
            return yield* Effect.fail([
                GridProblem.make({ context: 'layouts', reason: 'geometry', cause: 'Layouts have unique identities, positive type areas, and one definition per axis and level.' }),
            ] as const);
        }
        const primary = yield* Effect.validate(
            Array.filter(requests, ({ level }) => level.role === 'main'),
            ({ layout, level }) => {
                const span = level.fit === 'page' ? { start: 0, end: spans[level.axis] } : layout.bounds[level.axis];
                if (level.axis === 'columns' && level.fit === 'page') {
                    return Effect.fail(GridProblem.make({ context: `${layout.index}.columns.main`, reason: 'invalid', cause: 'Main columns fit the type area.' }));
                }
                return Effect.map(Effect.fromResult(layoutTracks(level, span, unit[level.axis], layout.image)), (tracks) => ({ index: layout.index, axis: level.axis, tracks }));
            },
        );
        const primaryByAxis = HashMap.fromIterable(Array.map(primary, ({ index, axis, tracks }) => [[index, axis] as const, tracks] as const));
        const secondary = yield* Effect.validate(
            Array.filter(requests, ({ level }) => level.role !== 'main'),
            ({ layout, level }) => {
                const parent = HashMap.get(primaryByAxis, [layout.index, level.axis]);
                if (level.role === 'sub' && (Option.isNone(parent) || level.fit === 'page')) {
                    return Effect.fail<Array.NonEmptyArray<GridProblem>>([
                        GridProblem.make({ context: `${layout.index}.${level.axis}.sub`, reason: 'invalid', cause: 'Subfields split existing main fields.' }),
                    ]);
                }
                const containers = level.role === 'sub' && Option.isSome(parent) ? parent.value : [level.fit === 'page' ? { start: 0, end: spans[level.axis] } : layout.bounds[level.axis]];
                return Effect.validate(containers, (span) => Effect.fromResult(layoutTracks(level, span, unit[level.axis], layout.image))).pipe(
                    Effect.map((fields) => ({ index: layout.index, axis: level.axis, tracks: Array.flatten(fields) })),
                );
            },
        ).pipe(Effect.mapError(Array.flatten));
        const ranges = Array.flatMap(prepared, (layout) =>
            Array.map(Record.keys(spans), (axis) => {
                const subdivisionCount = Option.getOrElse(layout.typeArea[axis], Function.constant(0));
                const numerator = axis === 'columns' ? layout.bounds.columns.end - layout.bounds.columns.start : lattice.leading;
                const fine = subdivisionCount === 0 ? lattice.leading : numerator / subdivisionCount;
                const start =
                    axis === 'columns'
                        ? layout.bounds.columns.start
                        : layout.baselineStart + Math.max(0, Math.ceil((layout.bounds.rows.start - layout.baselineStart - RULE.rounding.tolerance) / fine)) * fine;
                const count = subdivisionCount === 0 ? 0 : Math.max(0, Math.floor((layout.bounds[axis].end - start + RULE.rounding.tolerance) / fine) + 1);
                const first =
                    layout.baselineStart + Math.max(0, Math.ceil((layout.bounds.rows.start + layout.image - layout.baselineStart - RULE.rounding.tolerance) / lattice.leading)) * lattice.leading;
                const imageCount = axis === 'rows' && layout.image > 0 ? Math.max(0, Math.floor((layout.bounds.rows.end - first + RULE.rounding.tolerance) / lattice.leading) + 1) : 0;
                return { index: layout.index, axis, fine, start, count, first, image: layout.image, imageCount };
            }),
        );
        const overCapacity = Array.filter(ranges, ({ count, imageCount }) => !Number.isSafeInteger(count + imageCount) || count < 0 || count + imageCount > MAX_ARRAY_LENGTH);
        if (overCapacity.length > 0) {
            return yield* Effect.fail([GridProblem.make({ context: 'guides', reason: 'geometry', cause: 'Guide count exceeds the representable array capacity.' })] as const);
        }
        const edges = Array.flatMap([...primary, ...secondary], ({ index, axis, tracks }) =>
            Array.flatMap(tracks, ({ start, end }) => [
                { index, axis, location: start },
                { index, axis, location: end },
            ]),
        );
        const finer = Array.flatMap(ranges, ({ index, axis, count, start, fine, first, image, imageCount }) => [
            ...(count === 0 ? [] : Array.makeBy(count, (step) => ({ index, axis, location: start + step * fine }))),
            ...(imageCount === 0 ? [] : Array.makeBy(imageCount, (step) => ({ index, axis, location: first + step * lattice.leading - image }))),
        ]);
        const guideByAxis = Array.reduce([...edges, ...finer], HashMap.empty<readonly [number, typeof _Axis.Type], Chunk.Chunk<number>>(), (map, { index, axis, location }) =>
            HashMap.modifyAt(map, [index, axis], (found) => Option.some(Chunk.append(Option.getOrElse(found, Chunk.empty<number>), location))),
        );
        const layouts = Array.map(prepared, ({ index, pages, bounds }) => ({
            index,
            pages,
            margins: { top: bounds.rows.start, bottom: height - bounds.rows.end, inside: bounds.columns.start, outside: lattice.width - bounds.columns.end },
            tracks: Record.map(bounds, (span, axis) => Option.getOrElse(HashMap.get(primaryByAxis, [index, axis]), Function.constant([span] as const))),
            guides: Record.map(bounds, (_, axis) => pipe(HashMap.get(guideByAxis, [index, axis]), Option.getOrElse(Chunk.empty<number>), Array.sort(Order.Number), Array.dedupeAdjacentWith(within))),
        }));
        const incomplete = Array.filter(
            layouts,
            ({ tracks, margins }) => !(within(Array.headNonEmpty(tracks.columns).start, margins.inside) && within(Array.lastNonEmpty(tracks.columns).end, lattice.width - margins.outside)),
        );
        if (Array.isArrayNonEmpty(incomplete)) {
            return yield* Effect.fail(
                Array.map(incomplete, ({ index }) => GridProblem.make({ context: `${index}.columns`, reason: 'geometry', cause: 'Main columns must cover the entire type area.' })),
            );
        }
        return GridResolution.make({
            ...Struct.pick(definition, ['height', 'facingPages', 'bleed', 'slug']),
            width: Span.make(lattice.width),
            grid: Struct.pick(lattice, ['columns', 'rows']),
            leading: lattice.leading,
            baseline,
            layouts,
        });
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Bounds, Combination, Fitted, Margins, PageGrid, SquareSize, Step, Unit };
export {
    atLeast,
    basedOn,
    browse,
    ColumnCount,
    Count,
    columns,
    combinations,
    fieldBounds,
    fitLeading,
    fitSpan,
    GeometryError,
    GridDefinition,
    GridError,
    GridProblem,
    GridResolution,
    greaterThan,
    imageLineHeight,
    imageLineTop,
    Lines,
    layoutTracks,
    lineMargin,
    lock,
    modular,
    Points,
    Positive,
    partition,
    proportions,
    Quantized,
    quantize,
    quickUnits,
    RULE,
    resolveGrid,
    resolveGridDefinition,
    round,
    Span,
    SubdivisionIndex,
    smartLevel,
    snappedSpan,
    squareGrid,
    squareSize,
    subdivision,
    unfittedLeading,
    verticalValue,
    within,
};
