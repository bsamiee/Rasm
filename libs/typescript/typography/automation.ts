// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Console, Data, Effect, Equal, FileSystem, flow, identity, Number, Option, Order, Path, type PlatformError, pipe, Record, Result, Schema, SchemaGetter, Struct } from 'effect';
import { Arbitrary } from 'effect/unstable/arbitrary';
import { Argument, Command, Flag } from 'effect/unstable/cli';
import {
    atLeast,
    basedOn,
    browse,
    ColumnCount,
    Count,
    columns,
    combinations,
    fitLeading,
    fitSpan,
    type GridError,
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
} from './grid.ts';
import { LETTERS, pageSizes, SERIES, Standard, standardSheet } from './page-sizes.ts';
import { Level, Measure, Preset } from './presets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Finding {
    readonly name: string;
    readonly expected: unknown;
    readonly actual: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROW = /^\|\s*\[\d+\]/u;
const _NAME = /`(?<name>[^`]*\d+(?:\.\d+)?x\d+(?:\.\d+)?)`/u;
const _PAGE = { minimum: RULE.pageInches.minimum * POINTS.in, maximum: RULE.pageInches.maximum * POINTS.in } as const;
const _LAW = {
    seed: 20_260_915,
    runs: 200,
    spanLines: 200,
    quantum: RULE.rounding.half / RULE.rounding.precision,
    real: {
        margin: { minimum: 0, maximum: _PAGE.maximum },
        ratio: { minimum: 0.4, maximum: 0.8 },
    },
    positive: {
        leading: _PAGE,
        module: RULE.modulePoints,
        size: { minimum: 0.1, maximum: 1296 },
        autoLeading: { minimum: 0.5, maximum: 5 },
    },
    count: {
        columns: { minimum: 1, maximum: RULE.entries },
        modules: { minimum: 1, maximum: _PAGE.maximum },
        unitsPerEm: { minimum: 16, maximum: 16_384 },
    },
    whole: {
        fontUnit: { minimum: -32_768, maximum: 32_767 },
        iso: { minimum: 0, maximum: 9 },
    },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const _Scalar = Schema.Union([Quantized, Schema.String]);
const _Readout = Schema.Record(Schema.String, Schema.Union([_Scalar, Schema.Array(_Scalar)]));
const _Millimeters = Points('mm');
const _Real = Record.map(_LAW.real, (bounds) => Schema.Number.pipe(Schema.check(Schema.isBetween(bounds))));
const _Positive = Record.map(_LAW.positive, (bounds) => Positive.pipe(Schema.check(Schema.isBetween(bounds))));
const _Count = Record.map(_LAW.count, (bounds) => Count.pipe(Schema.check(Schema.isBetween(bounds))));
const _Whole = Record.map(_LAW.whole, (bounds) => Schema.Int.pipe(Schema.check(Schema.isBetween(bounds))));
const _Lines = Lines.pipe(Schema.check(Schema.isLessThanOrEqualTo(_PAGE.maximum)));
const _OptionalLeading = Schema.OptionFromNullOr(_Positive.leading);
const _Series = Schema.Literals(SERIES);
const _Letters = Schema.Literals(LETTERS);
const _Fields = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: ', ' }), encode: SchemaGetter.transform(Array.join(', ')) }));
const _Input = Schema.TaggedUnion({
    fitLeading: { height: Span, leading: Positive },
    subdivision: { height: Span, leading: Positive, index: SubdivisionIndex },
    smartFields: {
        width: Span,
        height: _Millimeters.pipe(Schema.decodeTo(Span)),
        unit: Positive,
        leading: Positive,
        inside: Schema.Number,
        outside: Schema.Number,
        gutter: Schema.Number,
        columns: Count,
        top: Schema.Number,
        bottom: Schema.Number,
        rows: Count,
    },
});
const _Reach = Schema.Struct({ span: Span, leading: Positive, lines: Schema.Number.pipe(Schema.check(Schema.isLessThanOrEqualTo(_LAW.spanLines))) });
const _Digits = Schema.String.pipe(Schema.check(Schema.isPattern(/^\d+$/u)));
const _Decimal = Schema.TemplateLiteralParser([_Digits, Schema.Literals([',', '.']), _Digits]).pipe(
    Schema.decodeTo(Schema.FiniteFromString, { decode: SchemaGetter.transform(([whole, _separator, fraction]) => `${whole}.${fraction}`), encode: SchemaGetter.forbiddenEncoding }),
);
const _CELLS = ['|', Schema.Trim, '|', Schema.Trim, '|'] as const;
const _InputCell = Schema.Trim.pipe(Schema.decodeTo(Schema.fromJsonString(_Input)));
const _Row = Schema.Union([
    Schema.TemplateLiteralParser([
        ..._CELLS,
        Schema.Trim.pipe(Schema.decodeTo(Schema.TemplateLiteralParser(['`data/layout-wizard-samples/', Schema.String, '`']))),
        '|',
        _InputCell,
        '|',
        Schema.Trim.pipe(Schema.decodeTo(Schema.Literal('the artefacts'))),
        '|',
    ]).pipe(
        Schema.decodeTo(Schema.toType(Schema.TaggedStruct('wizard', { name: Schema.String, sample: Schema.String, input: _Input })), {
            decode: SchemaGetter.transform(([_open, _index, _name, name, _source, [_prefix, sample], _input, input]) => ({ _tag: 'wizard' as const, name, sample, input })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.TemplateLiteralParser([
        ..._CELLS,
        Schema.Trim.pipe(Schema.decodeTo(Schema.TemplateLiteralParser(['`docs/manual-2022-wayback/', Schema.String, ':', Schema.FiniteFromString, '`']))),
        '|',
        _InputCell,
        '|',
        Schema.Trim.pipe(Schema.decodeTo(_Fields)),
        '|',
    ]).pipe(
        Schema.decodeTo(Schema.toType(Schema.TaggedStruct('manual', { name: Schema.String, file: Schema.String, line: Schema.Number, input: _Input, expected: _Fields })), {
            decode: SchemaGetter.transform(([_open, _index, _name, name, _source, [_prefix, file, _colon, line], _input, input, _expected, expected]) => ({
                _tag: 'manual' as const,
                name,
                file,
                line,
                input,
                expected,
            })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
]).pipe(Schema.toTaggedUnion('_tag'));
const _Result = Schema.fromJsonString(Schema.Struct({ preview: Schema.Record(Schema.Literals(['tm', 'bm', 'lm', 'rm', 'colW', 'rowH', 'cg', 'rg', 'unitH', 'unitV']), Schema.Number) }));
const _Artefact = Schema.Struct({
    'Height 4': Schema.FiniteFromString,
    'H. Module 2': Schema.FiniteFromString,
    'H. Module 3': Schema.FiniteFromString,
    'V. Module 2': Schema.FiniteFromString,
    'V. Module 3': Schema.FiniteFromString,
    'Style Leading': Measure,
    'Top margin (lines)': Schema.FiniteFromString,
    'Bottom margin (lines)': Schema.FiniteFromString,
    'Inside margin (lines)': Schema.FiniteFromString,
    'Outside margin (lines)': Schema.FiniteFromString,
    'Main Columns': Level,
    'Main Rows': Level,
});
const _StandardCell = Schema.Union([
    Schema.TemplateLiteralParser([_Series, Schema.FiniteFromString]).pipe(
        Schema.decodeTo(Schema.TaggedStruct('iso', { series: _Series, index: Schema.Number }), {
            decode: SchemaGetter.transform(([series, index]) => ({ _tag: 'iso' as const, series, index })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.TemplateLiteralParser(['ANSI ', _Letters]).pipe(
        Schema.decodeTo(Schema.TaggedStruct('ansi', { size: _Letters }), {
            decode: SchemaGetter.transform(([_family, size]) => ({ _tag: 'ansi' as const, size })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.TemplateLiteralParser(['Arch ', _Letters]).pipe(
        Schema.decodeTo(Schema.TaggedStruct('arch', { size: _Letters }), {
            decode: SchemaGetter.transform(([_family, size]) => ({ _tag: 'arch' as const, size })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
]);
const _Measured = Schema.Struct({ width: Schema.Number, height: Schema.Number, unit: Schema.Literals(['mm', 'in']) });
const _Csv = Schema.TemplateLiteralParser([Schema.String, ',', _StandardCell, ',', Schema.FiniteFromString, ',', Schema.FiniteFromString, ',', Schema.Literals(['mm', 'in']), ',', Schema.String]).pipe(
    Schema.decodeTo(Schema.toType(Schema.Struct({ name: Schema.String, listed: _Measured, derived: _Measured })), {
        decode: SchemaGetter.transform(([_family, _comma, standard, _second, width, _third, height, _fourth, unit]) => {
            const sheet = standardSheet(standard);
            return { name: sheet.name, listed: { width, height, unit }, derived: { width: sheet.width, height: sheet.height, unit: sheet.unit } };
        }),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _TextLines = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: '\n' }), encode: SchemaGetter.transform(Array.join('\n')) }));

// --- [ERRORS] --------------------------------------------------------------------------

type AutomationError = Data.TaggedEnum<{
    readonly pageNotFound: { readonly page: string };
    readonly lineNotFound: { readonly file: string; readonly line: number };
    readonly mismatched: { readonly findings: Array.NonEmptyReadonlyArray<Finding> };
}>;

const AutomationError: Data.TaggedEnum.Constructor<AutomationError> = Data.taggedEnum<AutomationError>();

// --- [READOUTS] ------------------------------------------------------------------------

const _outcome: (result: Result.Result<unknown, GridError>) => unknown = Result.match({ onFailure: (error: GridError) => ({ failure: error._tag }), onSuccess: identity });

const _readout: (input: typeof _Input.Type) => unknown = _Input.match<unknown>({
    fitLeading: flow(fitLeading, _outcome),
    subdivision: flow(subdivision, _outcome),
    smartFields: ({ width, height, unit, leading, inside, outside, gutter, columns: count, top, bottom, rows }) =>
        _outcome(
            Result.map(
                Result.all({ fitted: fitLeading({ height, leading }), horizontal: modular({ span: width, modules: Option.none(), moduleSize: unit, subdivisions: Count.make(1) }) }),
                ({ fitted, horizontal }) => {
                    const leadingMm = Schema.encodeSync(_Millimeters)(fitted.leading);
                    const across = smartLevel(round)({ lines: Lines.make(horizontal.modules), before: inside, after: outside, gutter, count, unit });
                    const down = smartLevel(Math.floor)({ lines: Lines.make(fitted.lines), before: top, after: bottom, gutter, count: rows, unit: Positive.make(leadingMm) });
                    const tm = down.before * leadingMm;
                    const rowH = down.field * leadingMm;
                    const rg = down.gutter * leadingMm;
                    return {
                        lines: fitted.lines,
                        styleLeading: fitted.leading,
                        moduleMm: leadingMm,
                        hModules: horizontal.modules,
                        hSize: horizontal.size,
                        inside: across.before,
                        outside: across.after,
                        column: across.field,
                        columnGutter: across.gutter,
                        columns: count,
                        top: down.before,
                        bottom: down.after,
                        row: down.field,
                        rowGutter: down.gutter,
                        rows,
                        tm,
                        bm: Schema.encodeSync(_Millimeters)(height) - tm - rows * rowH - (rows - 1) * rg,
                        lm: across.before * unit,
                        rm: across.after * unit,
                        colW: across.field * unit,
                        rowH,
                        cg: across.gutter * unit,
                        rg,
                        unitH: unit,
                        unitV: leadingMm,
                    };
                },
            ),
        ),
});

// --- [SOURCES] -------------------------------------------------------------------------

const _text = (...segments: readonly string[]): Effect.Effect<string, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> =>
    Effect.flatMap(
        Effect.map(Path.Path, (path) => path.resolve(import.meta.dirname, '..', '..', '..', ...segments)),
        (file) => FileSystem.FileSystem.use((fs) => fs.readFileString(file)),
    );

const _rows = (design: readonly string[], heading: string): readonly string[] =>
    pipe(
        design,
        Array.dropWhile((line) => line !== heading),
        Array.takeWhile((line, index) => index === 0 || !line.startsWith('## ')),
        Array.filter((line) => _ROW.test(line)),
    );

const _verdict = (name: string, expected: unknown, actual: unknown): Effect.Effect<Option.Option<Finding>> => {
    const same = Equal.equals(expected, actual);
    return Effect.as(Console.log(`${same ? 'PASS' : 'FAIL'} ${name}`), same ? Option.none() : Option.some({ name, expected, actual }));
};

const _encode: (readout: unknown) => typeof _Readout.Type = Schema.encodeUnknownSync(_Readout);

const _check: (row: typeof _Row.Type) => Effect.Effect<readonly Option.Option<Finding>[], AutomationError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =
    _Row.match({
        wizard: Effect.fnUntraced(function* ({ name, input, sample }) {
            const text = yield* _text('plan', 'research', 'grid-calculator', 'data', 'layout-wizard-samples', `${sample}-preset.txt`);
            const preset = yield* Schema.decodeEffect(Preset)(text);
            const artefact = yield* Schema.decodeUnknownEffect(_Artefact)(
                Record.fromIterableWith(Array.appendAll(preset.global, Array.flatMap(preset.masters, Struct.get('entries'))), ({ key, value }) => [key, value]),
            );
            const { preview } = yield* Schema.decodeEffect(_Result)(yield* _text('plan', 'research', 'grid-calculator', 'data', 'layout-wizard-samples', `${sample}-result.json`));
            const readout = yield* Schema.encodeUnknownEffect(_Readout)({
                lines: artefact['V. Module 2'],
                styleLeading: artefact['Style Leading'].value,
                moduleMm: artefact['V. Module 3'],
                hModules: artefact['H. Module 2'],
                hSize: artefact['H. Module 3'],
                inside: artefact['Inside margin (lines)'],
                outside: artefact['Outside margin (lines)'],
                column: artefact['Main Columns'].lines,
                columnGutter: artefact['Main Columns'].gutter,
                columns: artefact['Main Columns'].count,
                top: artefact['Top margin (lines)'],
                bottom: artefact['Bottom margin (lines)'],
                row: artefact['Main Rows'].lines,
                rowGutter: artefact['Main Rows'].gutter,
                rows: artefact['Main Rows'].count,
                ...Record.map(preview, (fraction) => fraction * artefact['Height 4']),
            });
            const reached = Array.filterMap(
                [
                    { level: artefact['Main Columns'], lines: artefact['H. Module 2'] - artefact['Inside margin (lines)'] - artefact['Outside margin (lines)'], unit: artefact['H. Module 3'] },
                    { level: artefact['Main Rows'], lines: artefact['V. Module 2'] - artefact['Top margin (lines)'] - artefact['Bottom margin (lines)'], unit: artefact['V. Module 3'] },
                ],
                ({ level, lines, unit }) =>
                    Result.map(Schema.decodeUnknownResult(_Reach)({ span: lines * unit, leading: unit, lines }), (reach) => ({
                        ...reach,
                        level: Struct.pick(level, ['count', 'lines', 'gutter']),
                        kind: level.kind,
                    })),
            );
            return [
                ...(yield* Effect.all([
                    _verdict(`${input._tag}: ${name}`, readout, _encode(_readout(input))),
                    _verdict(`preset: ${sample} renders byte for byte`, text, yield* Schema.encodeEffect(Preset)(preset)),
                ])),
                ...(yield* Effect.forEach(reached, ({ level, kind, span, leading, lines }) =>
                    _verdict(
                        `combinations: ${sample} lists the ${level.count} ${kind} of the ${lines}-line span`,
                        Option.some(level),
                        Array.findFirst(combinations({ span, leading, imageLine: Option.none(), sort: 'count', direction: 'ascending' }), Equal.equals(level)),
                    ),
                )),
            ];
        }),
        manual: Effect.fnUntraced(function* ({ name, input, file, line, expected }) {
            const lines = yield* Schema.decodeEffect(_TextLines)(yield* _text('plan', 'research', 'grid-calculator', 'docs', 'manual-2022-wayback', file));
            const sentence = yield* Effect.fromOption(Array.get(lines, line - 1), () => AutomationError.lineNotFound({ file, line }));
            const values = Array.filterMap(sentence.split(' '), (word) => Schema.decodeUnknownResult(_Decimal)(word));
            return Array.of(yield* _verdict(`${input._tag}: ${name}`, Record.fromEntries(Array.zip(expected, values)), Struct.pick(_encode(_readout(input)), expected)));
        }),
    });

// --- [LAWS] ----------------------------------------------------------------------------

const _close = (left: number, right: number): boolean => {
    const scale = Math.max(1, Math.abs(right));
    return within(left / scale, right / scale);
};

const _law = <A>(name: string, input: Arbitrary.Arbitrary<A>, law: (value: A) => boolean): Effect.Effect<Option.Option<Finding>> =>
    Effect.flatMap(Arbitrary.checkEffect(input, law, { seed: _LAW.seed, runs: _LAW.runs }), (result) =>
        _verdict(`law: ${name}`, 'Passed', result._tag === 'Passed' ? result._tag : Arbitrary.formatCheckFailure(result)),
    );

const _LAWS: readonly Effect.Effect<Option.Option<Finding>>[] = [
    _law('fitLeading', Arbitrary.schema(Schema.Struct({ height: Span, leading: _Positive.leading })), ({ height, leading }) => {
        const lines = round(height / leading);
        const applied = quantize(height / lines);
        return Result.match(fitLeading({ height, leading }), {
            onSuccess: (fitted) => lines !== 0 && fitted.lines === lines && fitted.leading === applied && atLeast(applied, 1) && !atLeast(applied, quantize(height)),
            onFailure: (error) => error._tag === 'leadingOutOfRange' && { noLine: lines === 0, belowOnePoint: !atLeast(applied, 1), atOrAbovePage: atLeast(applied, quantize(height)) }[error.reason],
        });
    }),
    _law('subdivision', Arbitrary.schema(Schema.Struct({ height: Span, leading: _Positive.leading, index: SubdivisionIndex })), ({ height, leading, index }) => {
        const none = index === 0;
        const division = !none && index <= RULE.steps;
        const multiplied = none || division ? leading : leading * (index - RULE.steps + 1);
        const fit = fitSpan(height, multiplied);
        return Result.match(subdivision({ height, leading, index }), {
            onSuccess: (outcome) =>
                ({ none, division, multiplication: !(none || division) })[outcome.kind] &&
                outcome.factor === { none: 1, division: index + 1, multiplication: index - RULE.steps + 1 }[outcome.kind] &&
                outcome.leading === quantize(fit) &&
                outcome.lines === round(height / fit) &&
                within(outcome.fine * outcome.factor, outcome.leading),
            onFailure: (error) => error._tag === 'leadingOutOfRange' && (Result.isFailure(fitLeading({ height, leading })) || (!division && greaterThan(fit, height))),
        });
    }),
    _law(
        'modular',
        Arbitrary.schema(Schema.Struct({ span: Span, modules: Schema.OptionFromNullOr(_Count.modules), moduleSize: _Positive.leading, subdivisions: _Count.columns })),
        ({ span, modules, moduleSize, subdivisions }) =>
            Result.match(modular({ span, modules, moduleSize, subdivisions }), {
                onSuccess: (grid) =>
                    within(grid.unit * grid.division, span) &&
                    grid.division === grid.modules * subdivisions &&
                    grid.size === span / grid.modules &&
                    Option.match(modules, { onNone: () => grid.modules === round(span / moduleSize), onSome: (count) => grid.modules === count }),
                onFailure: (error) =>
                    ({
                        moduleCountZero: Option.isNone(modules) && round(span / moduleSize) === 0,
                        moduleSizeOutOfRange: Option.exists(modules, (count) => greaterThan(RULE.modulePoints.minimum, span / count) || greaterThan(span / count, RULE.modulePoints.maximum)),
                        leadingOutOfRange: false,
                        gutterTooLarge: false,
                        squareAboveLeading: false,
                    })[error._tag],
            }),
    ),
    _law('lineMargin', Arbitrary.schema(Schema.Struct({ margin: _Real.margin, unit: _Positive.leading })), ({ margin, unit }) => {
        const { lines, applied } = lineMargin({ margin, unit });
        return applied === lines * unit && atLeast(unit * RULE.rounding.half, Math.abs(applied - margin));
    }),
    _law('snappedSpan', Arbitrary.schema(Schema.Struct({ span: Span, height: Span, leading: _Positive.leading, imageLine: _OptionalLeading })), ({ span, height, leading, imageLine }) => {
        const snapped = snappedSpan({ span, height, leading, imageLine });
        const lines = (snapped + Option.getOrElse(imageLine, () => 0)) / leading;
        return Option.isNone(imageLine) && atLeast(1 / RULE.rounding.precision, round(span / leading) * leading) ? snapped === height : _close(lines, round(lines));
    }),
    _law('squareGrid', Arbitrary.schema(Schema.Struct({ width: Span, vertical: _Positive.leading })), ({ width, vertical }) => {
        const square = squareGrid({ width, vertical });
        const rounded = round(width / vertical);
        return square.unit === vertical && square.width === square.lines * vertical && atLeast(square.width, width) && (square.lines === rounded || square.lines === rounded + 1);
    }),
    _law('columns', Arbitrary.schema(Schema.Struct({ span: Span, count: ColumnCount.pipe(Schema.check(Schema.isLessThan(RULE.entries))), gutter: _Real.margin })), ({ span, count, gutter }) => {
        const width = (span - (count - 1) * gutter) / count;
        return Result.match(columns({ span, count, gutter }), {
            onSuccess: ({ runs }) =>
                atLeast(width, 0) && runs.length === 2 * count - 1 && _close(Number.sumAll(runs), span) && Array.every(runs, (run, index) => run === (index % 2 === 0 ? width : gutter)),
            onFailure: (error) => error._tag === 'gutterTooLarge' && !atLeast(width, 0),
        });
    }),
    _law(
        'smartLevel',
        Arbitrary.schema(Schema.Struct({ lines: _Lines, before: _Real.margin, after: _Real.margin, gutter: _Real.margin, count: _Count.columns, unit: _Positive.leading })),
        ({ lines, before, after, gutter, count, unit }) => {
            const rounded = smartLevel(round)({ lines, before, after, gutter, count, unit });
            const floored = smartLevel(Math.floor)({ lines, before, after, gutter, count, unit });
            return (
                Array.every(
                    [rounded, floored],
                    (level) =>
                        level.before === lineMargin({ margin: before, unit }).lines &&
                        level.gutter === lineMargin({ margin: gutter, unit }).lines &&
                        _close(level.before + count * level.field + (count - 1) * level.gutter + level.after, lines),
                ) &&
                (rounded.field === floored.field || rounded.field === floored.field + 1) &&
                floored.after - rounded.after === count * (rounded.field - floored.field)
            );
        },
    ),
    _law(
        'combinations',
        Arbitrary.filter(
            Arbitrary.schema(Schema.Struct({ span: Span, leading: _Positive.module, imageLine: _OptionalLeading, sort: Schema.Literals(['count', 'lines', 'gutter']) })),
            ({ span, leading }) => span / leading <= _LAW.spanLines,
        ),
        ({ span, leading, imageLine, sort }) => {
            const ascending = combinations({ span, leading, imageLine, sort, direction: 'ascending' });
            const descending = combinations({ span, leading, imageLine, sort, direction: 'descending' });
            const byCount = combinations({ span, leading, imageLine, sort: 'count', direction: 'ascending' });
            const image = Option.getOrElse(imageLine, () => 0);
            const extra = Option.isSome(imageLine) ? 1 : 0;
            const total = Option.isSome(imageLine) ? Math.ceil(span / leading) : round(span / leading);
            return (
                Array.every(ascending, (entry) => {
                    const term = entry.count * entry.lines + (entry.count - 1) * (entry.gutter + extra);
                    const lineSpan = entry.lines * leading + image;
                    return (
                        entry.count >= 2 &&
                        entry.count < RULE.entries &&
                        entry.lines >= 2 &&
                        (entry.gutter === 0
                            ? entry.count * (entry.lines + extra) === total
                            : atLeast(span / 2, lineSpan) &&
                              atLeast(lineSpan + 2 * _LAW.quantum, entry.gutter * leading + extra * (leading - image)) &&
                              atLeast(_LAW.quantum * leading * (entry.lines + entry.gutter + extra), Math.abs(span - image - leading * term)))
                    );
                }) &&
                Equal.equals(Array.reverse(descending), ascending) &&
                Array.every(Array.zip(ascending, Array.drop(ascending, 1)), ([left, right]) => left[sort] <= right[sort]) &&
                ascending.length === byCount.length &&
                Array.every(ascending, (entry) => Array.containsWith(Equal.equals)(byCount, entry))
            );
        },
    ),
    _law(
        'browse',
        Arbitrary.filter(Arbitrary.schema(Schema.Struct({ count: _Lines, index: _Lines })), ({ count, index }) => index <= count),
        ({ count, index }) => {
            const next = browse({ count, index, step: 'next' });
            const previous = browse({ count, index, step: 'previous' });
            return (
                browse({ count, index: Lines.make(next), step: 'previous' }) === index &&
                browse({ count, index: Lines.make(previous), step: 'next' }) === index &&
                browse({ count, index: count, step: 'next' }) === 0 &&
                browse({ count, index: Lines.make(0), step: 'previous' }) === count
            );
        },
    ),
    _law(
        'proportions',
        Arbitrary.schema(Schema.Struct({ width: Span, height: Span, margins: Schema.Struct({ top: _Real.margin, bottom: _Real.margin, inside: _Real.margin, outside: _Real.margin }) })),
        ({ width, height, margins }) => {
            const ratio = proportions({ width, height, margins });
            const measure = width - margins.inside - margins.outside;
            return _close(ratio.document * width, height) && (measure === 0 || _close(ratio.typeArea * measure, height - margins.top - margins.bottom));
        },
    ),
    _law('verticalValue', Arbitrary.schema(Schema.Struct({ width: Span, height: Span, leading: _Positive.leading })), ({ width, height, leading }) => {
        const vertical = verticalValue({ width, height, margins: { top: 0, bottom: 0, inside: 0, outside: 0 }, leading, imageLine: Option.none() });
        return Result.match(quickUnits({ width, height, leading }), {
            onSuccess: (quick) =>
                vertical.span === height &&
                vertical.leading === quick.leading &&
                vertical.lines === round(height / vertical.leading) &&
                atLeast(RULE.rounding.half + (_LAW.quantum * quick.lines) / vertical.leading, Math.abs(vertical.lines - quick.lines)) &&
                atLeast(_LAW.quantum * (width / height + 1), Math.abs(vertical.horizontal - quick.horizontal)),
            onFailure: () => vertical.leading === quantize(fitSpan(height, leading)),
        });
    }),
    _law(
        'unfittedLeading',
        Arbitrary.schema(Schema.Struct({ width: Span, height: Span, leading: _Positive.leading, top: _Lines, bottom: _Lines, imageLine: _OptionalLeading })),
        ({ width, height, leading, top, bottom, imageLine }) => {
            const grid = unfittedLeading({ width, height, leading, top, bottom, imageLine });
            return (
                grid.leading === quantize(leading) &&
                grid.remainder >= 0 &&
                grid.remainder < grid.leading &&
                _close(grid.lines * grid.leading + grid.remainder, height) &&
                (grid.lines === Math.floor(height / grid.leading) || _close(grid.lines * grid.leading, height)) &&
                grid.top === top * grid.leading + Option.getOrElse(imageLine, () => 0) &&
                grid.bottom === grid.remainder + bottom * grid.leading &&
                grid.subdivisions.length === 2 * RULE.steps &&
                Array.every(grid.subdivisions, (entry, index) => {
                    const factor = (index % RULE.steps) + 2;
                    return index < RULE.steps ? atLeast(factor * _LAW.quantum, Math.abs(entry * factor - grid.leading)) : atLeast(_LAW.quantum, Math.abs(entry - factor * grid.leading));
                })
            );
        },
    ),
    _law(
        'imageLineHeight',
        Arbitrary.filter(
            Arbitrary.schema(Schema.Struct({ size: _Positive.size, unitsPerEm: _Count.unitsPerEm, box: Schema.Struct({ minY: _Whole.fontUnit, maxY: _Whole.fontUnit }) })),
            ({ box }) => box.maxY >= box.minY,
        ),
        ({ size, unitsPerEm, box }) => {
            const height = imageLineHeight({ size, unitsPerEm, box });
            const shifted = imageLineHeight({ size, unitsPerEm, box: { minY: 0, maxY: box.maxY - box.minY } });
            const doubled = imageLineHeight({ size: Positive.make(2 * size), unitsPerEm, box });
            return (
                height >= 0 &&
                height === shifted &&
                _close(height * RULE.rounding.precision, round(height * RULE.rounding.precision)) &&
                atLeast(doubled, height) &&
                atLeast(_LAW.quantum + 2 * _LAW.quantum, Math.abs(doubled - 2 * height))
            );
        },
    ),
    _law(
        'imageLineTop',
        Arbitrary.schema(Schema.Struct({ lines: _Lines, top: _Lines, bottom: _Lines, leading: _Positive.leading, height: _Positive.leading })),
        ({ lines, top, bottom, leading, height }) => {
            const placed = imageLineTop({ lines, top, bottom, leading, height });
            return placed.top === top * leading + height && _close(placed.top + placed.span + bottom * leading, lines * leading);
        },
    ),
    _law(
        'squareSize',
        Arbitrary.filter(Arbitrary.schema(Schema.Struct({ gridWidth: _Positive.module, leading: _Positive.module, slope: _Real.ratio })), ({ gridWidth, slope }) =>
            atLeast(quantize(gridWidth), RULE.sizeStart * slope),
        ),
        ({ gridWidth, leading, slope }) => {
            const target = quantize(gridWidth);
            return Result.match(
                squareSize((size) => size * slope, { gridWidth, leading }),
                {
                    onSuccess: (square) =>
                        !greaterThan(target, leading) &&
                        square.size >= RULE.sizeStart &&
                        square.height === square.size * slope &&
                        (square.match === 'exact' ? within(square.height, target) : atLeast(slope * 2 * _LAW.quantum, Math.abs(square.height - target))),
                    onFailure: (error) => error._tag === 'squareAboveLeading' && greaterThan(target, leading),
                },
            );
        },
    ),
    _law('basedOn', Arbitrary.schema(Schema.Struct({ height: Span, size: _Positive.size, leading: _OptionalLeading, autoLeading: _Positive.autoLeading })), ({ height, size, leading, autoLeading }) =>
        Result.match(basedOn({ height, size, leading, autoLeading }), {
            onSuccess: (fitted) =>
                fitted.size === size &&
                Option.match(leading, { onNone: () => fitted.desired === autoLeading * size, onSome: (own) => fitted.desired === own }) &&
                fitted.lines >= 1 &&
                atLeast(fitted.lines * _LAW.quantum, Math.abs(fitted.lines * fitted.leading - height)),
            onFailure: (error) => error._tag === 'leadingOutOfRange' && error.span === height,
        }),
    ),
    _law('lock', Arbitrary.schema(Schema.Struct({ sum: _Lines, edited: _Lines, other: _Lines })), ({ sum, edited, other }) => {
        const moved = lock({ sum, edited, other });
        return moved.edited + moved.other === sum && (atLeast(sum - edited, 0) ? moved.edited === edited : moved.other === other);
    }),
    _law('standardSheet iso', Arbitrary.schema(Schema.Struct({ series: _Series, index: _Whole.iso })), ({ series, index }) => {
        const sheet = standardSheet(Standard.iso({ series, index }));
        const next = standardSheet(Standard.iso({ series, index: index + 1 }));
        return sheet.unit === 'mm' && sheet.name === `${series}${index}` && next.width === Math.floor(sheet.height / 2) && next.height === sheet.width;
    }),
    _law('standardSheet inch', Arbitrary.schema(Schema.Struct({ family: Schema.Literals(['ansi', 'arch']) })), ({ family }) => {
        const sheets = Array.map(LETTERS, (size) => standardSheet(Standard[family]({ size })));
        return (
            Array.every(sheets, (sheet) => sheet.unit === 'in' && sheet.name.startsWith(family.toUpperCase())) &&
            Array.every(Array.zip(sheets, Array.drop(sheets, 1)), ([previous, next]) => next.width === previous.height && next.height === 2 * previous.width)
        );
    }),
];

const _oracle: Effect.Effect<void, AutomationError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const design = yield* Schema.decodeEffect(_TextLines)(yield* _text('plan', 'design', 'typography.md'));
    const rows = yield* Effect.forEach(_rows(design, '## [11]-[ORACLE]'), (line) => Schema.decodeUnknownEffect(_Row)(line));
    const checks = yield* Effect.forEach(rows, _check);
    const listed = Array.filterMap(yield* Schema.decodeEffect(_TextLines)(yield* _text('plan', 'research', 'grid-calculator', 'data', 'page-sizes.csv')), (line) =>
        Schema.decodeUnknownResult(_Csv)(line),
    );
    const standards = yield* Effect.forEach(listed, (row) => _verdict(`standard: ${row.name}`, _encode(row.listed), _encode(row.derived)));
    const named = Array.getSomes(
        Array.map(_rows(design, '## [04]-[PAGE_SIZES]'), (line) =>
            Option.fromNullishOr(_NAME.exec(line)).pipe(Option.flatMapNullishOr(Struct.get('groups')), Option.flatMapNullishOr(Struct.get('name'))),
        ),
    );
    const verdicts = [
        ...Array.flatten(checks),
        ...standards,
        yield* _verdict(
            `catalogue: the ${named.length} sheets the design names are the distinct names of pageSizes`,
            Array.sort(named, Order.String),
            Array.sort(Array.dedupe(Array.map(pageSizes, Struct.get('name'))), Order.String),
        ),
        ...(yield* Effect.all(_LAWS)),
    ];
    yield* Array.match(Array.getSomes(verdicts), {
        onEmpty: () => Console.log(`${verdicts.length} checks read back`),
        onNonEmpty: (findings) => Effect.fail(AutomationError.mismatched({ findings })),
    });
});

// --- [ENTRY] ---------------------------------------------------------------------------

const _grid = Command.make(
    'grid',
    {
        page: Argument.String('page').pipe(Argument.withDescription('A page-size name such as `A4 210x297`, or `<width>x<height>` in the unit')),
        leading: Argument.Finite('leading').pipe(Argument.withDescription('Desired leading in points')),
        unit: Flag.Literals('unit', Struct.keys(POINTS)).pipe(Flag.withDescription('Unit of a `<width>x<height>` page and of the unit rows'), Flag.withDefault('pt')),
    },
    Effect.fnUntraced(function* ({ page, leading, unit }) {
        const dimension = Schema.FiniteFromString.pipe(Schema.decodeTo(Points(unit)));
        const sheet = yield* Effect.fromOption(
            Option.orElse(
                Option.map(Schema.decodeUnknownOption(Schema.TemplateLiteralParser([dimension, 'x', dimension]))(page), ([across, _separator, down]) => ({ width: across, height: down })),
                () =>
                    Option.map(
                        Array.findFirst(pageSizes, (row) => row.name === page),
                        (row) => ({ width: row.width, height: row.height }),
                    ),
            ),
            () => AutomationError.pageNotFound({ page }),
        );
        const units = yield* Effect.fromResult(quickUnits(yield* Schema.decodeUnknownEffect(Schema.Struct({ width: Span, height: Span, leading: Positive }))({ ...sheet, leading })));
        const display = Quantized.pipe(Schema.decodeTo(Points(unit)));
        const rows = [
            Schema.encodeSync(_Readout)({ lines: units.lines, leading: units.leading, horizontal: units.horizontal }),
            Schema.encodeSync(Schema.Struct({ width: display, height: display, vertical: display, horizontal: display }))({ ...sheet, vertical: units.leading, horizontal: units.horizontal }),
        ];
        yield* Console.log(JSON.stringify(rows, null, 4));
    }),
);

Command.run(Command.make('automation').pipe(Command.withSubcommands([_grid, Command.make('oracle', {}, () => _oracle)])), { version: '' }).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
