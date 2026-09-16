// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Console, Data, Effect, Equal, FileSystem, flow, identity, Match, Option, Path, type PlatformError, pipe, Record, Result, Schema, SchemaGetter, Struct } from 'effect';
import { Argument, Command, Flag } from 'effect/unstable/cli';
import {
    basedOn,
    browse,
    ColumnCount,
    Count,
    canonMargins,
    columns,
    combinations,
    fitLeading,
    type GridError,
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
    quickUnits,
    Span,
    SubdivisionIndex,
    smartColumns,
    smartRows,
    snappedSpan,
    squareGrid,
    squareSize,
    subdivision,
    type Unit,
    unfittedLeading,
    verticalValue,
} from './grid.ts';
import { pageSizes, standardSheet } from './page-sizes.ts';
import { Level, Measure, Preset } from './presets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _Scalar = Schema.Union([Quantized, Schema.String]);
const _Readout = Schema.Record(Schema.String, Schema.Union([_Scalar, Schema.Array(_Scalar)]));
const _Margins = Schema.Struct({ top: Schema.Number, bottom: Schema.Number, inside: Schema.Number, outside: Schema.Number });
const _ImageLine = Schema.OptionFromNullOr(Positive);
const _Millimeters = Points('mm');
const _Input = Schema.Union([
    Schema.TaggedStruct('fitLeading', { height: Span, leading: Positive }),
    Schema.TaggedStruct('quickUnits', { width: Span, height: Span, leading: Positive }),
    Schema.TaggedStruct('subdivision', { height: Span, leading: Positive, index: SubdivisionIndex }),
    Schema.TaggedStruct('modular', { span: Span, modules: Schema.OptionFromNullOr(Count), moduleSize: Positive, subdivisions: Count }),
    Schema.TaggedStruct('lineMargin', { margin: Schema.Number, unit: Positive }),
    Schema.TaggedStruct('snappedSpan', { span: Positive, leading: Positive, imageLine: _ImageLine }),
    Schema.TaggedStruct('squareGrid', { width: Span, vertical: Positive }),
    Schema.TaggedStruct('columns', { span: Positive, count: ColumnCount, gutter: Schema.Number }),
    Schema.TaggedStruct('smartColumns', { lines: Positive, before: Schema.Number, after: Schema.Number, gutter: Schema.Number, count: Count, unit: Positive }),
    Schema.TaggedStruct('smartRows', { lines: Positive, before: Schema.Number, after: Schema.Number, gutter: Schema.Number, count: Count, unit: Positive }),
    Schema.TaggedStruct('combinations', {
        span: Positive,
        leading: Positive,
        imageLine: _ImageLine,
        sort: Schema.Literals(['count', 'lines', 'gutter']),
        direction: Schema.Literals(['ascending', 'descending']),
        kind: Schema.Literals(['columns', 'rows']),
        presses: Count,
    }),
    Schema.TaggedStruct('proportions', { width: Span, height: Span, margins: _Margins }),
    Schema.TaggedStruct('verticalValue', { width: Span, height: Span, margins: _Margins, leading: Positive, imageLine: _ImageLine }),
    Schema.TaggedStruct('unfittedLeading', { width: Span, height: Span, leading: Positive, top: Lines, bottom: Lines, imageLine: _ImageLine }),
    Schema.TaggedStruct('imageLineHeight', { size: Positive, unitsPerEm: Positive, box: Schema.Struct({ minY: Schema.Number, maxY: Schema.Number }) }),
    Schema.TaggedStruct('imageLineTop', { lines: Count, top: Lines, bottom: Lines, leading: Positive, height: Positive }),
    Schema.TaggedStruct('squareSize', {
        gridWidth: Positive,
        leading: Positive,
        measure: Schema.Union([Schema.TaggedStruct('linear', { slope: Positive }), Schema.TaggedStruct('step', { at: Positive, below: Positive, above: Positive })]),
    }),
    Schema.TaggedStruct('basedOn', { height: Span, size: Positive, leading: Schema.OptionFromNullOr(Positive), autoLeading: Positive }),
    Schema.TaggedStruct('lock', { sum: Lines, edited: Lines, other: Lines, lines: Count, count: Count, gutter: Lines }),
    Schema.TaggedStruct('canonMargins', { width: Span, height: Span, canon: Schema.Literals(['van-de-graaf', 'tschichold']) }),
    Schema.TaggedStruct('smartFields', {
        width: Positive,
        height: _Millimeters,
        unit: Positive,
        leading: Positive,
        inside: Schema.Number,
        outside: Schema.Number,
        gutter: Schema.Number,
        columns: Count,
        top: Schema.Number,
        bottom: Schema.Number,
        rows: Count,
    }),
]);
const _Digits = Schema.String.pipe(Schema.check(Schema.isPattern(/^\d+$/u)));
const _Decimal = Schema.TemplateLiteralParser([_Digits, Schema.Literals([',', '.']), _Digits]).pipe(
    Schema.decodeTo(Schema.Number, { decode: SchemaGetter.transform(([whole, _separator, fraction]) => Number(`${whole}.${fraction}`)), encode: SchemaGetter.forbiddenEncoding }),
);
const _Fields = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: ', ' }), encode: SchemaGetter.transform(Array.join(', ')) }));
const _InputCell = Schema.fromJsonString(_Input);
const _Row = Schema.Union([
    Schema.TemplateLiteralParser([
        '|',
        Schema.Trim,
        '|',
        Schema.Trim,
        '|',
        Schema.Trim.pipe(Schema.decodeTo(Schema.TemplateLiteralParser(['`0x', Schema.String, '`']))),
        '|',
        _InputCell,
        '|',
        Schema.fromJsonString(_Readout),
        '|',
    ]).pipe(
        Schema.decodeTo(Schema.toType(Schema.TaggedStruct('readback', { name: Schema.String, address: Schema.String, input: _Input, expected: _Readout })), {
            decode: SchemaGetter.transform(([_open, _index, _name, name, _source, [_prefix, address], _input, input, _expected, expected]) => ({
                _tag: 'readback' as const,
                name,
                address,
                input,
                expected,
            })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
    Schema.TemplateLiteralParser([
        '|',
        Schema.Trim,
        '|',
        Schema.Trim,
        '|',
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
        '|',
        Schema.Trim,
        '|',
        Schema.Trim,
        '|',
        Schema.Trim.pipe(Schema.decodeTo(Schema.TemplateLiteralParser(['`docs/manual-2022-wayback/', Schema.String, ':', Schema.FiniteFromString, '`']))),
        '|',
        _InputCell,
        '|',
        Schema.Trim.pipe(Schema.decodeTo(_Fields)),
        '|',
    ]).pipe(
        Schema.decodeTo(Schema.toType(Schema.TaggedStruct('manual', { name: Schema.String, file: Schema.String, line: Schema.Number, input: _Input, fields: Schema.Array(Schema.String) })), {
            decode: SchemaGetter.transform(([_open, _index, _name, name, _source, [_prefix, file, _colon, line], _input, input, _expected, fields]) => ({
                _tag: 'manual' as const,
                name,
                file,
                line,
                input,
                fields,
            })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    ),
]);
const _ROW = /^\|\s*\[\d+\]/u;
const _Result = Schema.fromJsonString(
    Schema.Struct({
        preview: Schema.Struct({
            tm: Schema.Number,
            bm: Schema.Number,
            lm: Schema.Number,
            rm: Schema.Number,
            colW: Schema.Number,
            rowH: Schema.Number,
            cg: Schema.Number,
            rg: Schema.Number,
            unitH: Schema.Number,
            unitV: Schema.Number,
            cols: Schema.Int,
            rows: Schema.Int,
            blLines: Schema.Int,
        }),
    }),
);
const _Global = Schema.Struct({
    'Height 4': Schema.FiniteFromString,
    'H. Module 2': Schema.FiniteFromString,
    'H. Module 3': Schema.FiniteFromString,
    'V. Module 2': Schema.FiniteFromString,
    'V. Module 3': Schema.FiniteFromString,
    'Style Leading': Measure,
});
const _Master = Schema.Struct({
    'Top margin (lines)': Schema.FiniteFromString,
    'Bottom margin (lines)': Schema.FiniteFromString,
    'Inside margin (lines)': Schema.FiniteFromString,
    'Outside margin (lines)': Schema.FiniteFromString,
    'Main Columns': Level,
    'Main Rows': Level,
});
const _Dimensions = Schema.Struct({ width: Schema.Number, height: Schema.Number });
const _CsvRow = Schema.toType(Schema.Struct({ name: Schema.String, listed: _Dimensions, derived: _Dimensions }));
const _Letters = Schema.Literals(['a', 'b', 'c', 'd', 'e']);
const _Csv = Schema.String.pipe(
    Schema.decodeTo(Schema.String, { decode: SchemaGetter.toLowerCase(), encode: SchemaGetter.toUpperCase() }),
    Schema.decodeTo(
        Schema.Union([
            Schema.TemplateLiteralParser(['din,', Schema.Literals(['a', 'b', 'c']), Schema.FiniteFromString, ',', Schema.FiniteFromString, ',', Schema.FiniteFromString, ',mm,', Schema.String]).pipe(
                Schema.decodeTo(_CsvRow, {
                    decode: SchemaGetter.transform(([_family, series, index, _first, width, _second, height]) => ({
                        name: `${series.toUpperCase()}${index}`,
                        listed: { width, height },
                        derived: standardSheet({ _tag: 'iso', series, index }),
                    })),
                    encode: SchemaGetter.forbiddenEncoding,
                }),
            ),
            Schema.TemplateLiteralParser(['ansi,ansi ', _Letters, ',', Schema.FiniteFromString, ',', Schema.FiniteFromString, ',in,', Schema.String]).pipe(
                Schema.decodeTo(_CsvRow, {
                    decode: SchemaGetter.transform(([_family, size, _first, width, _second, height]) => ({
                        name: `ANSI ${size.toUpperCase()}`,
                        listed: { width, height },
                        derived: standardSheet({ _tag: 'ansi', size }),
                    })),
                    encode: SchemaGetter.forbiddenEncoding,
                }),
            ),
            Schema.TemplateLiteralParser(['arch,arch ', _Letters, ',', Schema.FiniteFromString, ',', Schema.FiniteFromString, ',in,', Schema.String]).pipe(
                Schema.decodeTo(_CsvRow, {
                    decode: SchemaGetter.transform(([_family, size, _first, width, _second, height]) => ({
                        name: `Arch ${size.toUpperCase()}`,
                        listed: { width, height },
                        derived: standardSheet({ _tag: 'arch', size }),
                    })),
                    encode: SchemaGetter.forbiddenEncoding,
                }),
            ),
        ]),
    ),
);
const _Lines = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: '\n' }), encode: SchemaGetter.transform(Array.join('\n')) }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CATALOGUE = { fTop: 0.749, rows: 59 } as const;

// --- [ERRORS] --------------------------------------------------------------------------

type AutomationError = Data.TaggedEnum<{
    readonly pageNotFound: { readonly page: string };
    readonly lineNotFound: { readonly file: string; readonly line: number };
    readonly mismatched: { readonly findings: Array.NonEmptyReadonlyArray<{ readonly name: string; readonly expected: unknown; readonly actual: unknown }> };
}>;

const AutomationError: Data.TaggedEnum.Constructor<AutomationError> = Data.taggedEnum<AutomationError>();

// --- [READOUTS] ------------------------------------------------------------------------

const _outcome: (result: Result.Result<unknown, GridError>) => unknown = Result.match({ onFailure: (error: GridError) => ({ failure: error._tag }), onSuccess: identity });

const _readout: (input: typeof _Input.Type) => unknown = Match.type<typeof _Input.Type>().pipe(
    Match.withReturnType<unknown>(),
    Match.discriminatorsExhaustive('_tag')({
        fitLeading: flow(fitLeading, _outcome),
        quickUnits: flow(quickUnits, _outcome),
        subdivision: flow(subdivision, _outcome),
        modular: flow(modular, _outcome),
        lineMargin,
        snappedSpan: flow(snappedSpan, (span) => ({ span })),
        squareGrid,
        columns: flow(columns, _outcome),
        smartColumns,
        smartRows,
        combinations: ({ kind, presses, ...input }) => {
            const listed = combinations(input);
            return {
                entries: Array.map(listed, (entry) => Schema.encodeSync(Level)({ kind, ...entry })),
                browsed: Array.reduce(Array.range(1, presses), 0, (index) => browse({ count: listed.length, index, step: 'next' })),
                wrapNext: browse({ count: listed.length, index: listed.length, step: 'next' }),
                wrapPrevious: browse({ count: listed.length, index: 0, step: 'previous' }),
            };
        },
        proportions,
        verticalValue,
        unfittedLeading,
        imageLineHeight: flow(imageLineHeight, (height) => ({ height })),
        imageLineTop,
        squareSize: ({ measure, ...input }) =>
            _outcome(
                squareSize(
                    Match.value(measure).pipe(
                        Match.withReturnType<(size: number) => number>(),
                        Match.discriminatorsExhaustive('_tag')({
                            linear:
                                ({ slope }) =>
                                (size) =>
                                    size * slope,
                            step:
                                ({ at, below, above }) =>
                                (size) =>
                                    size < at ? below : above,
                        }),
                    ),
                    input,
                ),
            ),
        basedOn: flow(basedOn, _outcome),
        lock: (input) => {
            const moved = lock(input);
            const fields = smartColumns({ lines: input.lines, before: moved.edited, after: moved.other, gutter: input.gutter, count: input.count, unit: 1 });
            return { edited: moved.edited, other: moved.other, column: fields.field, outside: fields.after };
        },
        canonMargins,
        smartFields: ({ width, height, unit, leading, inside, outside, gutter, columns: count, top, bottom, rows }) =>
            _outcome(
                Result.map(fitLeading({ height, leading }), (fitted) => {
                    const leadingMm = Schema.encodeSync(_Millimeters)(fitted.leading);
                    const across = smartColumns({ lines: width / unit, before: inside, after: outside, gutter, count, unit });
                    const down = smartRows({ lines: fitted.lines, before: top, after: bottom, gutter, count: rows, unit: leadingMm });
                    const tm = down.before * leadingMm;
                    const rowH = down.field * leadingMm;
                    const rg = down.gutter * leadingMm;
                    return {
                        lines: fitted.lines,
                        styleLeading: fitted.leading,
                        moduleMm: leadingMm,
                        hModules: width / unit,
                        hSize: unit,
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
                }),
            ),
    }),
);

// --- [SOURCES] -------------------------------------------------------------------------

const _text = (...segments: readonly string[]): Effect.Effect<string, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> =>
    Effect.flatMap(
        Effect.map(Path.Path, (path) => path.resolve(import.meta.dirname, '..', '..', '..', ...segments)),
        (file) => FileSystem.FileSystem.use((fs) => fs.readFileString(file)),
    );

const _artefacts: (
    sample: string,
) => Effect.Effect<{ readonly text: string; readonly rendered: string; readonly readout: typeof _Readout.Type }, PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =
    Effect.fnUntraced(function* (sample: string) {
        const text = yield* _text('plan', 'research', 'grid-calculator', 'data', 'layout-wizard-samples', `${sample}-preset.txt`);
        const preset = yield* Schema.decodeEffect(Preset)(text);
        const global = yield* Schema.decodeUnknownEffect(_Global)(Record.fromEntries(Array.map(preset.global, ({ key, value }) => [key, value])));
        const master = yield* Schema.decodeUnknownEffect(_Master)(Record.fromEntries(Array.flatMap(preset.masters, ({ entries }) => Array.map(entries, ({ key, value }) => [key, value]))));
        const { preview } = yield* Schema.decodeEffect(_Result)(yield* _text('plan', 'research', 'grid-calculator', 'data', 'layout-wizard-samples', `${sample}-result.json`));
        const height = global['Height 4'];
        const readout = yield* Schema.encodeUnknownEffect(_Readout)({
            lines: global['V. Module 2'],
            styleLeading: global['Style Leading'].value,
            moduleMm: global['V. Module 3'],
            hModules: global['H. Module 2'],
            hSize: global['H. Module 3'],
            inside: master['Inside margin (lines)'],
            outside: master['Outside margin (lines)'],
            column: master['Main Columns'].lines,
            columnGutter: master['Main Columns'].gutter,
            columns: master['Main Columns'].count,
            top: master['Top margin (lines)'],
            bottom: master['Bottom margin (lines)'],
            row: master['Main Rows'].lines,
            rowGutter: master['Main Rows'].gutter,
            rows: master['Main Rows'].count,
            ...Record.map(Struct.omit(preview, ['cols', 'rows', 'blLines']), (fraction) => fraction * height),
        });
        return { text, rendered: yield* Schema.encodeEffect(Preset)(preset), readout };
    });

const _manual: (file: string, line: number) => Effect.Effect<readonly number[], AutomationError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> =
    Effect.fnUntraced(function* (file: string, line: number) {
        const lines = yield* Schema.decodeEffect(_Lines)(yield* _text('plan', 'research', 'grid-calculator', 'docs', 'manual-2022-wayback', file));
        const sentence = yield* Effect.fromOption(Array.get(lines, line - 1), () => AutomationError.lineNotFound({ file, line }));
        return Array.filterMap(sentence.split(' '), (word) => Schema.decodeUnknownResult(_Decimal)(word));
    });

// --- [ORACLE] --------------------------------------------------------------------------

const _verdict = (name: string, expected: unknown, actual: unknown): Effect.Effect<Option.Option<{ readonly name: string; readonly expected: unknown; readonly actual: unknown }>> => {
    const same = Equal.equals(expected, actual);
    return Effect.as(Console.log(`${same ? 'PASS' : 'FAIL'} ${name}`), same ? Option.none() : Option.some({ name, expected, actual }));
};

const _encode: (readout: unknown) => typeof _Readout.Type = Schema.encodeUnknownSync(_Readout);

const _check: (
    row: typeof _Row.Type,
) => Effect.Effect<
    readonly Option.Option<{ readonly name: string; readonly expected: unknown; readonly actual: unknown }>[],
    AutomationError | PlatformError.PlatformError | Schema.SchemaError,
    FileSystem.FileSystem | Path.Path
> = Match.type<typeof _Row.Type>().pipe(
    Match.withReturnType<
        Effect.Effect<
            readonly Option.Option<{ readonly name: string; readonly expected: unknown; readonly actual: unknown }>[],
            AutomationError | PlatformError.PlatformError | Schema.SchemaError,
            FileSystem.FileSystem | Path.Path
        >
    >(),
    Match.discriminatorsExhaustive('_tag')({
        readback: ({ name, input, expected }) => Effect.map(_verdict(`${input._tag}: ${name}`, expected, _encode(_readout(input))), Array.of),
        wizard: ({ name, input, sample }) =>
            Effect.flatMap(_artefacts(sample), ({ text, rendered, readout }) =>
                Effect.all([_verdict(`${input._tag}: ${name}`, readout, _encode(_readout(input))), _verdict(`preset: ${sample} renders byte for byte`, text, rendered)]),
            ),
        manual: ({ name, input, file, line, fields }) =>
            Effect.flatMap(_manual(file, line), (values) =>
                Effect.map(_verdict(`${input._tag}: ${name}`, Record.fromEntries(Array.zip(fields, values)), Struct.pick(_encode(_readout(input)), fields)), Array.of),
            ),
    }),
);

const _readback: Effect.Effect<void, AutomationError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const design = yield* Schema.decodeEffect(_Lines)(yield* _text('plan', 'design', 'typography.md'));
    const rows = yield* Effect.forEach(
        pipe(
            design,
            Array.dropWhile((line) => line !== '## [11]-[ORACLE]'),
            Array.takeWhile((line, index) => index === 0 || !line.startsWith('## ')),
            Array.filter((line) => _ROW.test(line)),
        ),
        (line) => Schema.decodeUnknownEffect(_Row)(line),
    );
    const checks = yield* Effect.forEach(rows, _check);
    const listed = Array.filterMap(yield* Schema.decodeEffect(_Lines)(yield* _text('plan', 'research', 'grid-calculator', 'data', 'page-sizes.csv')), (line) => Schema.decodeUnknownResult(_Csv)(line));
    const standards = yield* Effect.forEach(listed, (row) => _verdict(`standard: ${row.name}`, _encode(row.listed), _encode(row.derived)));
    const catalogue = pageSizes(_CATALOGUE.fTop);
    const verdicts = [
        ...Array.flatten(checks),
        ...standards,
        yield* _verdict(
            `catalogue: ${_CATALOGUE.rows} rows with distinct names`,
            { rows: _CATALOGUE.rows, names: _CATALOGUE.rows },
            { rows: catalogue.length, names: Array.dedupe(Array.map(catalogue, Struct.get('name'))).length },
        ),
    ];
    yield* Array.match(Array.getSomes(verdicts), {
        onEmpty: () => Console.log(`${verdicts.length} checks read back`),
        onNonEmpty: (findings) => Effect.fail(AutomationError.mismatched({ findings })),
    });
});

// --- [ENTRY] ---------------------------------------------------------------------------

const _page = (page: string, unit: Unit): Effect.Effect<{ readonly width: number; readonly height: number }, AutomationError> => {
    const dimension = Schema.FiniteFromString.pipe(Schema.decodeTo(Points(unit)));
    return Option.match(Schema.decodeUnknownOption(Schema.TemplateLiteralParser([dimension, 'x', dimension]))(page), {
        onNone: () =>
            Effect.fromOption(
                Option.map(
                    Array.findFirst(pageSizes(_CATALOGUE.fTop), (row) => row.name === page),
                    (row) => ({ width: row.width, height: row.height }),
                ),
                () => AutomationError.pageNotFound({ page }),
            ),
        onSome: ([width, _separator, height]) => Effect.succeed({ width, height }),
    });
};

const _grid = Command.make(
    'grid',
    {
        page: Argument.String('page').pipe(Argument.withDescription('A page-size name such as `A4 210x297`, or `<width>x<height>` in the unit')),
        leading: Argument.Finite('leading').pipe(Argument.withDescription('Desired leading in points')),
        unit: Flag.Literals('unit', Struct.keys(POINTS)).pipe(Flag.withDescription('Unit of a `<width>x<height>` page and of the unit rows'), Flag.withDefault('pt')),
    },
    ({ page, leading, unit }) =>
        Effect.gen(function* () {
            const { width, height } = yield* _page(page, unit);
            const units = yield* Effect.fromResult(quickUnits({ width, height, leading }));
            const display = Quantized.pipe(Schema.decodeTo(Points(unit)));
            const rows = [
                Schema.encodeSync(_Readout)({ lines: units.lines, leading: units.leading, horizontal: units.horizontal }),
                Schema.encodeSync(Schema.Struct({ width: display, height: display, vertical: display, horizontal: display }))({ width, height, vertical: units.leading, horizontal: units.horizontal }),
            ];
            yield* Console.log(JSON.stringify(rows, null, 4));
        }),
);

Command.run(Command.make('automation').pipe(Command.withSubcommands([_grid, Command.make('oracle', {}, () => _readback)])), { version: '' }).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
