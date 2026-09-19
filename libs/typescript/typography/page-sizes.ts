// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Data, Match, Option, pipe, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Family = 'us' | 'iso-a' | 'iso-b' | 'board' | 'technical' | 'digital' | 'screen';
type Intent = 'print' | 'web' | 'mobile';

interface Dimensions {
    readonly width: number;
    readonly height: number;
}

type Standard = Data.TaggedEnum<{
    readonly iso: { readonly series: (typeof SERIES)[number]; readonly index: number };
    readonly ansi: { readonly size: (typeof LETTERS)[number] | 'F' };
    readonly arch: { readonly size: (typeof LETTERS)[number] | 'E1' | 'E2' | 'E3' };
}>;

interface StandardSheet extends Dimensions {
    readonly name: string;
    readonly unit: 'mm' | 'in';
    readonly standard: Standard;
}

interface Sides {
    readonly top: number;
    readonly bottom: number;
    readonly left: number;
    readonly right: number;
}

interface Sheet extends Dimensions {
    readonly name: string;
    readonly family: Family;
    readonly intent: Intent;
    readonly unit: 'pt' | 'px';
    readonly sourceUnit: keyof typeof POINTS;
    readonly standard: Option.Option<Standard>;
    readonly module: number;
}

interface PageSize extends Sheet {
    readonly margins: Omit<Sides, 'top'>;
    readonly columns: Option.Option<{ readonly count: number; readonly gutter: number }>;
    readonly rows: Option.Option<{ readonly count: number; readonly lines: number; readonly gutter: number }>;
    readonly firstBaseline: number;
    readonly lastBaseline: number;
    readonly folio: Option.Option<number>;
    readonly footer: Option.Option<number>;
    readonly sheet: Option.Option<{ readonly border: Sides; readonly modules: { readonly columns: number; readonly rows: number }; readonly strip: number }>;
}

interface ModuleRule {
    readonly module: number;
    readonly rows: number;
    readonly gutterLines: number;
}

// --- [STANDARDS] -----------------------------------------------------------------------

const Standard: Data.TaggedEnum.Constructor<Standard> = Data.taggedEnum<Standard>();
const _UNIT = { pointsPerInch: 72, millimetresPerInch: 25.4, millimetresPerMetre: 1000 } as const;
const POINTS: Readonly<Record<'mm' | 'in' | 'pt' | 'px', number>> = { mm: _UNIT.pointsPerInch / _UNIT.millimetresPerInch, in: _UNIT.pointsPerInch, pt: 1, px: 1 };
const _mean = (left: Dimensions, right: Dimensions): Dimensions => ({ width: Math.round(Math.sqrt(left.width * right.width)), height: Math.round(Math.sqrt(left.height * right.height)) });
const _double = (sheet: Dimensions): Dimensions => ({ width: sheet.height, height: 2 * sheet.width });
const _A0: Dimensions = { width: Math.round(_UNIT.millimetresPerMetre * Math.sqrt(Math.SQRT1_2)), height: Math.round(_UNIT.millimetresPerMetre * Math.sqrt(Math.SQRT2)) };
const _B0: Dimensions = _mean(_A0, _double(_A0));
const SERIES = ['A', 'B', 'C'] as const;
const LETTERS = ['A', 'B', 'C', 'D', 'E'] as const;

const _iso = (series: (typeof SERIES)[number], index: number): Dimensions => {
    const base = Match.value(series).pipe(
        Match.when('A', () => _A0),
        Match.when('B', () => _B0),
        Match.when('C', () => _mean(_A0, _B0)),
        Match.exhaustive,
    );
    const divisor = 2 ** Math.floor(index / 2);
    return index % 2 === 0
        ? { width: Math.floor(base.width / divisor), height: Math.floor(base.height / divisor) }
        : { width: Math.floor(base.height / (2 * divisor)), height: Math.floor(base.width / divisor) };
};

const _ladder = (base: Dimensions, size: (typeof LETTERS)[number]): Dimensions =>
    Array.reduce(
        Array.takeWhile(LETTERS, (letter) => letter !== size),
        base,
        _double,
    );

const standardSheet = (standard: Standard): StandardSheet =>
    Standard.$match(standard, {
        iso: ({ series, index }): StandardSheet => ({ name: `${series}${index}`, ..._iso(series, index), unit: 'mm', standard }),
        ansi: ({ size }): StandardSheet => ({ name: `ANSI ${size}`, ...(size === 'F' ? { width: 28, height: 40 } : _ladder({ width: 8.5, height: 11 }, size)), unit: 'in', standard }),
        arch: ({ size }): StandardSheet => ({
            name: `ARCH ${size}`,
            ...Match.value(size).pipe(
                Match.withReturnType<Dimensions>(),
                Match.when('E1', () => ({ width: 30, height: 42 })),
                Match.when('E2', () => ({ width: 26, height: 38 })),
                Match.when('E3', () => ({ width: 27, height: 39 })),
                Match.orElse((letter) => _ladder({ width: 9, height: 12 }, letter)),
            ),
            unit: 'in',
            standard,
        }),
    });

// --- [CATALOGUE] -----------------------------------------------------------------------

const _RULE = {
    document: { columns: 12, sideModules: 3, bottomLines: 4, digitalBottomLines: 6, folioLines: 3, footerLines: 4 },
    technical: { module: 12, drawingModule: 108, inchBorderModules: 2, isoBorder: { left: 20, right: 10 }, isoSheets: 3 },
    screen: {
        module: 45,
        sides: [
            { shorter: 0, modules: 1 },
            { shorter: 540, modules: 2 },
            { shorter: 1080, modules: 3 },
        ],
        rows: [
            { lines: 0, rows: 1, gutterLines: 1 },
            { lines: 8, rows: 2, gutterLines: 1 },
            { lines: 20, rows: 4, gutterLines: 2 },
            { lines: 40, rows: 6, gutterLines: 2 },
        ],
        columnModules: 4,
        footerLines: 2,
        footerBottomLines: 3,
    },
} as const;
const _ISO_MODULES: readonly ModuleRule[] = [
    { module: 36, rows: 6, gutterLines: 2 },
    { module: 11.5, rows: 6, gutterLines: 1 },
    { module: 11.5, rows: 6, gutterLines: 1 },
    { module: 12, rows: 6, gutterLines: 2 },
    { module: 15, rows: 5, gutterLines: 1 },
    { module: 8.5, rows: 5, gutterLines: 1 },
    { module: 7.5, rows: 5, gutterLines: 1 },
];
const _SCREENS: readonly (Dimensions & { readonly label: string; readonly intent: Intent })[] = [
    { label: 'Web', width: 600, height: 300, intent: 'web' },
    { label: 'Web', width: 640, height: 480, intent: 'web' },
    { label: 'Web', width: 760, height: 420, intent: 'web' },
    { label: 'Web', width: 800, height: 600, intent: 'web' },
    { label: 'Web', width: 984, height: 588, intent: 'web' },
    { label: 'iPad', width: 1024, height: 768, intent: 'mobile' },
    { label: 'Web', width: 1240, height: 620, intent: 'web' },
    { label: 'Web', width: 1280, height: 800, intent: 'web' },
    { label: 'Web', width: 1366, height: 768, intent: 'web' },
    { label: 'Web', width: 1920, height: 1080, intent: 'web' },
    { label: 'Kindle Fire Nook', width: 1024, height: 600, intent: 'mobile' },
    { label: 'Android', width: 720, height: 1280, intent: 'mobile' },
    { label: 'iPhone 8', width: 750, height: 1334, intent: 'mobile' },
    { label: 'Google Pixel', width: 1080, height: 1920, intent: 'mobile' },
    { label: 'iPhone X', width: 1125, height: 2436, intent: 'mobile' },
    { label: 'iPhone 8 Plus', width: 1242, height: 2208, intent: 'mobile' },
    { label: 'Google Pixel XL', width: 1440, height: 2560, intent: 'mobile' },
    { label: 'Samsung S8', width: 1440, height: 2960, intent: 'mobile' },
    { label: 'iPad Retina', width: 2048, height: 1536, intent: 'mobile' },
    { label: 'Surface Pro 3', width: 2160, height: 1440, intent: 'mobile' },
    { label: 'iPad Pro 10.5', width: 2224, height: 1668, intent: 'mobile' },
    { label: 'iPad Pro 12.9', width: 2732, height: 2048, intent: 'mobile' },
    { label: 'Surface Pro 4', width: 2736, height: 1824, intent: 'mobile' },
];
const _TECHNICAL: readonly Standard[] = [
    ...Array.map(['C', 'D', 'E', 'F'] as const, (size) => Standard.ansi({ size })),
    ...Array.map([...LETTERS, 'E1', 'E2', 'E3'] as const, (size) => Standard.arch({ size })),
    ...Array.makeBy(_RULE.technical.isoSheets, (index) => Standard.iso({ series: 'A', index })),
];

const _sheet = (label: string, family: Family, intent: Intent, sheet: Dimensions & { readonly unit: keyof typeof POINTS; readonly standard?: Standard }): Omit<Sheet, 'module'> => ({
    name: `${label} ${sheet.width}x${sheet.height}`,
    family,
    intent,
    unit: sheet.unit === 'px' ? 'px' : 'pt',
    sourceUnit: sheet.unit,
    standard: Option.fromUndefinedOr(sheet.standard),
    width: sheet.width * POINTS[sheet.unit],
    height: sheet.height * POINTS[sheet.unit],
});

const _documents: readonly (Sheet & ModuleRule)[] = [
    { ..._sheet('Letter', 'us', 'print', standardSheet(Standard.ansi({ size: 'A' }))), module: 15, rows: 5, gutterLines: 1 },
    { ..._sheet('Legal', 'us', 'print', { width: 8.5, height: 14, unit: 'in' }), module: 15, rows: 6, gutterLines: 1 },
    { ..._sheet('Tabloid', 'us', 'print', standardSheet(Standard.ansi({ size: 'B' }))), module: 12, rows: 6, gutterLines: 1 },
    { ..._sheet('Half Letter', 'us', 'print', { width: 5.5, height: 8.5, unit: 'in' }), module: 8.5, rows: 5, gutterLines: 1 },
    ...Array.flatMap(
        [
            { series: 'A', family: 'iso-a' },
            { series: 'B', family: 'iso-b' },
        ] as const,
        ({ series, family }) =>
            Array.map(_ISO_MODULES, (rule, index) => {
                const sheet = standardSheet(Standard.iso({ series, index }));
                return { ..._sheet(sheet.name, family, 'print', sheet), ...rule };
            }),
    ),
    { ..._sheet('Board', 'board', 'print', { width: 48, height: 24, unit: 'in' }), module: 12, rows: 3, gutterLines: 2 },
    { ..._sheet('Board A1', 'board', 'print', standardSheet(Standard.iso({ series: 'A', index: 1 }))), module: 11.5, rows: 6, gutterLines: 1 },
    { ..._sheet('Digital', 'digital', 'web', { width: 3840, height: 2160, unit: 'px' }), module: _RULE.screen.module, rows: 6, gutterLines: 2 },
];

const _technical: readonly (Sheet & { readonly border: Pick<Sides, 'left' | 'right'> })[] = Array.map(_TECHNICAL, (standard) => {
    const sheet = standardSheet(standard);
    const inchBorder = _RULE.technical.inchBorderModules * _RULE.technical.module;
    return {
        ..._sheet(sheet.unit === 'in' ? sheet.name : `${sheet.name} Sheet`, 'technical', 'print', { ...sheet, width: sheet.height, height: sheet.width }),
        module: _RULE.technical.module,
        border: sheet.unit === 'in' ? { left: inchBorder, right: inchBorder } : { left: _RULE.technical.isoBorder.left * POINTS.mm, right: _RULE.technical.isoBorder.right * POINTS.mm },
    };
});

// --- [RULES] ---------------------------------------------------------------------------

const _grid = ({
    sheet,
    sideModules,
    rows,
    gutterLines,
    bottomLines,
}: {
    readonly sheet: Sheet;
    readonly sideModules: number;
    readonly rows: number;
    readonly gutterLines: number;
    readonly bottomLines: number;
}): Pick<PageSize, 'margins' | 'columns' | 'rows' | 'firstBaseline' | 'lastBaseline'> => {
    const side = sideModules * sheet.module;
    const lines = Math.floor(((sheet.height - bottomLines * sheet.module - side) / sheet.module + 1 - (rows - 1) * gutterLines) / rows);
    const lastBaseline = side + (rows * lines + (rows - 1) * gutterLines - 1) * sheet.module;
    return {
        margins: { bottom: sheet.height - lastBaseline, left: side, right: side },
        columns: Option.some({ count: _RULE.document.columns, gutter: sheet.module }),
        rows: Option.some({ count: rows, lines, gutter: gutterLines * sheet.module }),
        firstBaseline: side,
        lastBaseline,
    };
};

const _document = (sheet: (typeof _documents)[number]): PageSize => {
    const digital = sheet.family === 'digital';
    const grid = _grid({
        sheet,
        sideModules: _RULE.document.sideModules,
        rows: sheet.rows,
        gutterLines: sheet.gutterLines,
        bottomLines: digital ? _RULE.document.digitalBottomLines : _RULE.document.bottomLines,
    });
    const after = Option.some(grid.lastBaseline + (digital ? _RULE.document.footerLines : _RULE.document.folioLines) * sheet.module);
    return { ...sheet, ...grid, folio: digital ? Option.none() : after, footer: digital ? after : Option.none(), sheet: Option.none() };
};

const _sheetGrid = (sheet: (typeof _technical)[number]): PageSize => {
    const { left, right } = sheet.border;
    const across = Math.floor((sheet.width - left - right) / _RULE.technical.drawingModule) - 1;
    const down = Math.floor((sheet.height - 2 * right) / _RULE.technical.drawingModule);
    const vertical = sheet.sourceUnit === 'mm' ? right : (sheet.height - down * _RULE.technical.drawingModule) / 2;
    return {
        ...Struct.omit(sheet, ['border']),
        margins: { bottom: vertical, left, right },
        columns: Option.none(),
        rows: Option.none(),
        firstBaseline: vertical,
        lastBaseline: sheet.height - vertical,
        folio: Option.none(),
        footer: Option.none(),
        sheet: Option.some({
            border: { top: vertical, bottom: vertical, left, right },
            modules: { columns: across, rows: down },
            strip: sheet.width - left - right - across * _RULE.technical.drawingModule,
        }),
    };
};

const _screen = (format: (typeof _SCREENS)[number]): PageSize => {
    const sheet = { ..._sheet(format.label, 'screen', format.intent, { ...format, unit: 'px' }), module: _RULE.screen.module };
    const shorter = Math.min(sheet.width, sheet.height);
    const sideModules = Array.reduce(_RULE.screen.sides, _RULE.screen.sides[0].modules, (current: number, band) => (shorter >= band.shorter ? band.modules : current));
    const side = sideModules * sheet.module;
    const available = Math.floor((sheet.height - 2 * side) / sheet.module) + 1;
    const rowRule = Array.reduce(_RULE.screen.rows, _RULE.screen.rows[0], (current: (typeof _RULE.screen.rows)[number], band) => (available >= band.lines ? band : current));
    const count = pipe(
        Array.range(1, _RULE.document.columns),
        Array.filter((divisor) => _RULE.document.columns % divisor === 0),
        Array.findLast((candidate) => (sheet.width - 2 * side - (candidate - 1) * sheet.module) / candidate >= _RULE.screen.columnModules * sheet.module),
        Option.getOrElse(() => 1),
    );
    const grid = _grid({ sheet, sideModules, rows: rowRule.rows, gutterLines: rowRule.gutterLines, bottomLines: sideModules });
    return {
        ...sheet,
        ...grid,
        columns: Option.some({ count, gutter: sheet.module }),
        folio: Option.none(),
        footer: grid.margins.bottom >= _RULE.screen.footerBottomLines * sheet.module ? Option.some(grid.lastBaseline + _RULE.screen.footerLines * sheet.module) : Option.none(),
        sheet: Option.none(),
    };
};

const pageSizes: readonly PageSize[] = [...Array.map(_documents, _document), ...Array.map(_technical, _sheetGrid), ...Array.map(_SCREENS, _screen)];

// --- [EXPORTS] -------------------------------------------------------------------------

export type { PageSize, Sheet, Sides, StandardSheet };
export { LETTERS, POINTS, pageSizes, SERIES, Standard, standardSheet };
