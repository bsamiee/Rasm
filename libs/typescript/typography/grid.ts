// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Data, Iterable, Match, Option, Order, pipe, Result, Schema, SchemaGetter } from 'effect';

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

// --- [CONSTANTS] -----------------------------------------------------------------------

const POINTS = { mm: 2.834_646_464_646_465, in: 72, pt: 1, px: 1 } as const;
const RULE = {
    rounding: { half: 0.5, precision: 1000, tolerance: 1e-8 },
    pageInches: { minimum: 0.0139, maximum: 216 },
    modulePoints: { minimum: 1, maximum: 1000 },
    columns: 2,
    entries: 41,
    sizeStart: 4,
    steps: 4,
} as const;

// --- [ERRORS] --------------------------------------------------------------------------

type GridError = Data.TaggedEnum<{
    readonly leadingOutOfRange: { readonly span: number; readonly leading: number; readonly reason: 'noLine' | 'belowOnePoint' | 'atOrAbovePage' };
    readonly gutterTooLarge: { readonly count: number; readonly gutter: number };
    readonly moduleCountZero: { readonly span: number; readonly desired: number };
    readonly moduleSizeOutOfRange: { readonly span: number; readonly modules: number; readonly size: number };
    readonly squareAboveLeading: { readonly gridWidth: number; readonly leading: number };
}>;

const GridError: Data.TaggedEnum.Constructor<GridError> = Data.taggedEnum<GridError>();

// --- [PRIMITIVES] ----------------------------------------------------------------------

const round = (value: number): number => Math.floor(value + RULE.rounding.half);

const fitSpan = (span: number, value: number): number => span / round(span / value);

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

// --- [MODES] ---------------------------------------------------------------------------

const fitLeading = ({ height, leading }: { readonly height: Span; readonly leading: Positive }): Result.Result<Fitted, GridError> => {
    const lines = round(height / leading);
    const applied = quantize(height / lines);
    return Option.match(
        Array.findFirst(
            [
                ['noLine', lines === 0],
                ['belowOnePoint', !atLeast(applied, 1)],
                ['atOrAbovePage', atLeast(applied, quantize(height))],
            ] as const,
            ([, failed]) => failed,
        ),
        {
            onNone: () => Result.succeed({ lines, leading: applied }),
            onSome: ([reason]) => Result.fail(GridError.leadingOutOfRange({ span: height, leading, reason })),
        },
    );
};

const quickUnits = ({ width, height, leading }: { readonly width: Span; readonly height: Span; readonly leading: Positive }): Result.Result<Fitted & { readonly horizontal: number }, GridError> =>
    Result.map(fitLeading({ height, leading }), (fitted) => ({ ...fitted, horizontal: (height / fitted.lines) * (width / height) }));

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
    const multiplied = step.kind === 'multiplication' ? leading * step.factor : leading;
    const fit = fitSpan(height, multiplied);
    const applied = quantize(fit);
    return Result.flatMap(fitLeading({ height, leading }), () =>
        step.kind !== 'division' && greaterThan(fit, height)
            ? Result.fail(GridError.leadingOutOfRange({ span: height, leading: multiplied, reason: 'atOrAbovePage' }))
            : Result.succeed({ ...step, lines: round(height / fit), leading: applied, fine: applied / step.factor }),
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
    Result.map(
        Option.match(modules, {
            onNone: (): Result.Result<number, GridError> => {
                const count = round(span / moduleSize);
                return count === 0 ? Result.fail(GridError.moduleCountZero({ span, desired: moduleSize })) : Result.succeed(count);
            },
            onSome: (count): Result.Result<number, GridError> => {
                const size = span / count;
                return greaterThan(RULE.modulePoints.minimum, size) || greaterThan(size, RULE.modulePoints.maximum)
                    ? Result.fail(GridError.moduleSizeOutOfRange({ span, modules: count, size }))
                    : Result.succeed(count);
            },
        }),
        (count) => ({ modules: count, size: span / count, unit: span / count / subdivisions, division: count * subdivisions }),
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
    const width = (span - (count - 1) * gutter) / count;
    return atLeast(width, 0) ? Result.succeed({ width, runs: Array.makeBy(2 * count - 1, (index) => (index % 2 === 0 ? width : gutter)) }) : Result.fail(GridError.gutterTooLarge({ count, gutter }));
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
}): readonly Combination[] => {
    const start = Option.match(imageLine, { onNone: () => ({ line: 2 * leading, gutter: leading }), onSome: (image) => ({ line: 2 * leading + image, gutter: 2 * leading - image }) });
    const total = Option.match(imageLine, { onNone: () => round(span / leading), onSome: () => Math.ceil(span / leading) });
    const sorted = pipe(
        Iterable.range(2),
        Iterable.map((lines) => ({ lines, lineSpan: start.line + (lines - 2) * leading })),
        Iterable.takeWhile(({ lineSpan }) => atLeast(span / 2, lineSpan)),
        Iterable.flatMap((row) =>
            pipe(
                Iterable.range(1),
                Iterable.map((gutter) => ({ gutter, gutterSpan: start.gutter + (gutter - 1) * leading })),
                Iterable.takeWhile(({ gutterSpan }) => atLeast(1 / RULE.rounding.precision, gutterSpan - row.lineSpan)),
                Iterable.filterMap(({ gutter, gutterSpan }) => {
                    const count = quantize((span - row.lineSpan) / (row.lineSpan + gutterSpan));
                    return within(count, round(count)) && greaterThan(RULE.entries, count) ? Result.succeed({ count: count + 1, lines: row.lines, gutter }) : Result.failVoid;
                }),
            ),
        ),
        Iterable.appendAll(
            pipe(
                Iterable.range(RULE.columns),
                Iterable.takeWhile((count) => count <= Math.floor(total / 2)),
                Iterable.filterMap((count) => {
                    const available = Option.match(imageLine, { onNone: () => total, onSome: () => total - count });
                    return available % count === 0 && count < RULE.entries && (Option.isNone(imageLine) || available / count > 1)
                        ? Result.succeed({ count, lines: available / count, gutter: 0 })
                        : Result.failVoid;
                }),
            ),
        ),
        Array.fromIterable,
        Array.sortBy(...Array.map(Array.dedupe([sort, 'count', 'lines', 'gutter'] as const), (key) => Order.mapInput(Order.Number, (entry: Combination) => entry[key]))),
    );
    return direction === 'descending' ? Array.reverse(sorted) : sorted;
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
}): Fitted & { readonly span: number; readonly horizontal: number; readonly modules: number } => {
    const span = height - (margins.top + margins.bottom);
    const measure = width - (margins.inside + margins.outside);
    const fitted = Option.match(imageLine, { onNone: () => span, onSome: (image) => span + image });
    const applied = quantize(fitSpan(fitted, leading));
    const horizontal = applied * (measure / fitted);
    return { span, lines: round(span / applied), leading: applied, horizontal: quantize(horizontal), modules: round(measure / horizontal) };
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
}): Fitted & { readonly remainder: number; readonly top: number; readonly bottom: number; readonly horizontal: number; readonly modules: number; readonly subdivisions: readonly number[] } => {
    const applied = quantize(leading);
    const filled = round(height / applied) * applied;
    const kept = greaterThan(filled, height) ? filled - applied : filled;
    const remainder = greaterThan(height, kept) ? height - kept : 0;
    const horizontal = quantize(fitSpan(height, applied) * (width / height));
    const factors = Array.range(2, RULE.steps + 1);
    return {
        leading: applied,
        lines: round((height - remainder) / applied),
        remainder,
        top: top * applied + Option.getOrElse(imageLine, () => 0),
        bottom: remainder + bottom * applied,
        horizontal,
        modules: round(width / horizontal),
        subdivisions: Array.appendAll(
            Array.map(factors, (factor) => quantize(applied / factor)),
            Array.map(factors, (factor) => quantize(applied * factor)),
        ),
    };
};

const imageLineHeight = ({ size, unitsPerEm, box }: { readonly size: Positive; readonly unitsPerEm: Count; readonly box: { readonly minY: number; readonly maxY: number } }): number =>
    quantize(((box.maxY - box.minY) * size) / unitsPerEm);

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

const squareSize = (measure: (size: number) => number, { gridWidth, leading }: { readonly gridWidth: Positive; readonly leading: Positive }): Result.Result<SquareSize, GridError> => {
    const target = quantize(gridWidth);
    const bracket = (size: number): Pick<SquareSize, 'size' | 'height'> => {
        const height = measure(size);
        return atLeast(height, target) ? { size, height } : bracket(size + 1);
    };
    const bisect = (low: number, high: number): SquareSize => {
        const size = quantize((low + high) / 2);
        const height = measure(size);
        if (within(height, target)) {
            return { size, height, match: 'exact' };
        }
        const bounds = greaterThan(target, height) ? { low: size + 1 / RULE.rounding.precision, high } : { low, high: size - 1 / RULE.rounding.precision };
        return atLeast(bounds.high, bounds.low) ? bisect(bounds.low, bounds.high) : { size, height, match: 'closest' };
    };
    const search = (): SquareSize => {
        const reached = bracket(RULE.sizeStart);
        return within(reached.height, target) ? { ...reached, match: 'exact' } : bisect(reached.size - 1, reached.size);
    };
    return greaterThan(target, leading) ? Result.fail(GridError.squareAboveLeading({ gridWidth: target, leading })) : Result.succeed(search());
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

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Combination, Fitted, Margins, SquareSize, Step, Unit };
export {
    atLeast,
    basedOn,
    browse,
    ColumnCount,
    Count,
    columns,
    combinations,
    fitLeading,
    fitSpan,
    GridError,
    greaterThan,
    imageLineHeight,
    imageLineTop,
    Lines,
    lineMargin,
    lock,
    modular,
    POINTS,
    Points,
    Positive,
    proportions,
    Quantized,
    quantize,
    quickUnits,
    RULE,
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
