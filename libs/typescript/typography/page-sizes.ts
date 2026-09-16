// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Match, Option, pipe, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Family = 'us' | 'iso-a' | 'iso-b' | 'board' | 'technical' | 'digital' | 'screen';
type Intent = 'print' | 'web' | 'mobile';

interface Dimensions {
    readonly width: number;
    readonly height: number;
}

type IsoSeries = 'a' | 'b' | 'c';
type AnsiSize = 'a' | 'b' | 'c' | 'd' | 'e' | 'f';
type ArchSize = 'a' | 'b' | 'c' | 'd' | 'e' | 'e1' | 'e2' | 'e3';

type Standard = { readonly _tag: 'iso'; readonly series: IsoSeries; readonly index: number } | { readonly _tag: 'ansi'; readonly size: AnsiSize } | { readonly _tag: 'arch'; readonly size: ArchSize };

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
    readonly module: number;
}

interface Grid {
    readonly margins: Sides;
    readonly columns: { readonly count: number; readonly gutter: number };
    readonly rows: { readonly count: number; readonly lines: number; readonly gutter: number };
    readonly firstBaseline: number;
    readonly lastBaseline: number;
    readonly folio: Option.Option<number>;
    readonly footer: Option.Option<number>;
    readonly sheet: Option.Option<{ readonly border: Sides; readonly modules: { readonly columns: number; readonly rows: number }; readonly strip: number }>;
}

interface PageSize extends Sheet, Grid {
    readonly parents: readonly string[];
    readonly swatchColumn: 'cmyk' | 'rgb';
}

interface Ladder {
    readonly module: number;
    readonly rows: number;
    readonly gutterLines: number;
}

// --- [STANDARDS] -----------------------------------------------------------------------

const _POINTS_PER_INCH = 72;
const _MILLIMETRES_PER_INCH = 25.4;
const _MILLIMETRES_PER_METRE = 1000;
const _POINTS = { mm: _POINTS_PER_INCH / _MILLIMETRES_PER_INCH, in: _POINTS_PER_INCH, px: 1 } as const;
const _mean = (left: Dimensions, right: Dimensions): Dimensions => ({ width: Math.round(Math.sqrt(left.width * right.width)), height: Math.round(Math.sqrt(left.height * right.height)) });
const _halve = (sheet: Dimensions): Dimensions => ({ width: Math.floor(sheet.height / 2), height: sheet.width });
const _double = (sheet: Dimensions): Dimensions => ({ width: sheet.height, height: 2 * sheet.width });
const _A0: Dimensions = { width: Math.round(_MILLIMETRES_PER_METRE * Math.sqrt(Math.SQRT1_2)), height: Math.round(_MILLIMETRES_PER_METRE * Math.sqrt(Math.SQRT2)) };
const _B0: Dimensions = _mean(_A0, _double(_A0));
const _ISO: Readonly<Record<IsoSeries, Dimensions>> = { a: _A0, b: _B0, c: _mean(_A0, _B0) };
const _ladder = (a: Dimensions): Readonly<Record<'a' | 'b' | 'c' | 'd' | 'e', Dimensions>> => {
    const b = _double(a);
    const c = _double(b);
    const d = _double(c);
    return { a, b, c, d, e: _double(d) };
};
const _INCH: { readonly ansi: Readonly<Record<AnsiSize, Dimensions>>; readonly arch: Readonly<Record<ArchSize, Dimensions>> } = {
    ansi: { ..._ladder({ width: 8.5, height: 11 }), f: { width: 28, height: 40 } },
    arch: { ..._ladder({ width: 9, height: 12 }), e1: { width: 30, height: 42 }, e2: { width: 26, height: 38 }, e3: { width: 27, height: 39 } },
};

const _iso = (series: IsoSeries, index: number): Dimensions => (index === 0 ? _ISO[series] : _halve(_iso(series, index - 1)));

const standardSheet = (standard: Standard): Dimensions & { readonly unit: 'mm' | 'in' } =>
    Match.value(standard).pipe(
        Match.withReturnType<Dimensions & { readonly unit: 'mm' | 'in' }>(),
        Match.tagsExhaustive({
            iso: ({ series, index }) => ({ ..._iso(series, index), unit: 'mm' }),
            ansi: ({ size }) => ({ ..._INCH.ansi[size], unit: 'in' }),
            arch: ({ size }) => ({ ..._INCH.arch[size], unit: 'in' }),
        }),
    );

// --- [CATALOGUE] -----------------------------------------------------------------------

const _RULE = {
    body: 0.85,
    document: { columns: 12, sideModules: 3, bottomLines: 4, digitalBottomLines: 6, folioLines: 3, footerLines: 4 },
    technical: { module: 12, drawingModule: 108, inchBorderModules: 2, isoBorder: { left: 20, right: 10 } },
    screen: {
        module: 45,
        sides: [
            { shorter: 1080, modules: 3 },
            { shorter: 540, modules: 2 },
        ],
        rows: [
            { lines: 40, rows: 6, gutterLines: 2 },
            { lines: 20, rows: 4, gutterLines: 2 },
            { lines: 8, rows: 2, gutterLines: 1 },
        ],
        columnModules: 4,
        footerLines: 2,
        footerBottomLines: 3,
        deckColumns: 6,
        deckRows: 4,
    },
} as const;
const _ISO_LADDER: readonly Ladder[] = [
    { module: 36, rows: 6, gutterLines: 2 },
    { module: 11.5, rows: 6, gutterLines: 1 },
    { module: 11.5, rows: 6, gutterLines: 1 },
    { module: 12, rows: 6, gutterLines: 2 },
    { module: 15, rows: 5, gutterLines: 1 },
    { module: 8.5, rows: 5, gutterLines: 1 },
    { module: 7.5, rows: 5, gutterLines: 1 },
];
const _PARENTS = {
    sheet: ['B-Sheet'],
    deck: ['A-Grid', 'B-Title', 'C-Section', 'D-Image-Led', 'E-Text-Led', 'F-Comparison', 'G-Diagram', 'H-Grid'],
    title: ['A-Grid', 'B-Title', 'E-Text-Led'],
} as const;
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

const _sheet = (label: string, family: Family, intent: Intent, sheet: Dimensions & { readonly unit: keyof typeof _POINTS }): Omit<Sheet, 'module'> => ({
    name: `${label} ${sheet.width}x${sheet.height}`,
    family,
    intent,
    unit: sheet.unit === 'px' ? 'px' : 'pt',
    width: sheet.width * _POINTS[sheet.unit],
    height: sheet.height * _POINTS[sheet.unit],
});

const _landscape = <S extends Dimensions>(sheet: S): S => ({ ...sheet, width: sheet.height, height: sheet.width });

const _documents: readonly (Sheet & Ladder & Pick<PageSize, 'parents'>)[] = [
    { ..._sheet('Letter', 'us', 'print', standardSheet({ _tag: 'ansi', size: 'a' })), module: 15, rows: 5, gutterLines: 1, parents: [] },
    { ..._sheet('Legal', 'us', 'print', { width: 8.5, height: 14, unit: 'in' }), module: 15, rows: 6, gutterLines: 1, parents: [] },
    { ..._sheet('Tabloid', 'us', 'print', standardSheet({ _tag: 'ansi', size: 'b' })), module: 12, rows: 6, gutterLines: 1, parents: [] },
    { ..._sheet('Half Letter', 'us', 'print', { width: 5.5, height: 8.5, unit: 'in' }), module: 8.5, rows: 5, gutterLines: 1, parents: [] },
    ...Array.flatMap(['a', 'b'] as const, (series) =>
        Array.map(_ISO_LADDER, (ladder, index) => ({ ..._sheet(`${series.toUpperCase()}${index}`, `iso-${series}`, 'print', standardSheet({ _tag: 'iso', series, index })), ...ladder, parents: [] })),
    ),
    { ..._sheet('Board', 'board', 'print', { width: 48, height: 24, unit: 'in' }), module: 12, rows: 3, gutterLines: 2, parents: [] },
    { ..._sheet('Board A1', 'board', 'print', standardSheet({ _tag: 'iso', series: 'a', index: 1 })), module: 11.5, rows: 6, gutterLines: 1, parents: _PARENTS.sheet },
    { ..._sheet('Digital', 'digital', 'web', { width: 3840, height: 2160, unit: 'px' }), module: _RULE.screen.module, rows: 6, gutterLines: 2, parents: _PARENTS.deck },
];

const _technical: readonly (Sheet & { readonly border: Pick<Sides, 'left' | 'right'> })[] = [
    ...Array.map(['c', 'd', 'e', 'f'] as const, (size) => _sheet(`ANSI ${size.toUpperCase()}`, 'technical', 'print', _landscape(standardSheet({ _tag: 'ansi', size })))),
    ...Array.map(['a', 'b', 'c', 'd', 'e', 'e1', 'e2', 'e3'] as const, (size) => _sheet(`ARCH ${size.toUpperCase()}`, 'technical', 'print', _landscape(standardSheet({ _tag: 'arch', size })))),
].map((sheet) => ({
    ...sheet,
    module: _RULE.technical.module,
    border: { left: _RULE.technical.inchBorderModules * _RULE.technical.module, right: _RULE.technical.inchBorderModules * _RULE.technical.module },
}));

const _isoTechnical: readonly (Sheet & { readonly border: Pick<Sides, 'left' | 'right'> })[] = Array.map([0, 1, 2], (index) => ({
    ..._sheet(`A${index} Sheet`, 'technical', 'print', _landscape(standardSheet({ _tag: 'iso', series: 'a', index }))),
    module: _RULE.technical.module,
    border: { left: _RULE.technical.isoBorder.left * _POINTS.mm, right: _RULE.technical.isoBorder.right * _POINTS.mm },
}));

// --- [RULES] ---------------------------------------------------------------------------

const _grid = ({
    sheet,
    fTop,
    sideModules,
    rows,
    gutterLines,
    bottomLines,
}: {
    readonly sheet: Sheet;
    readonly fTop: number;
    readonly sideModules: number;
    readonly rows: number;
    readonly gutterLines: number;
    readonly bottomLines: number;
}): Omit<Grid, 'folio' | 'footer' | 'sheet'> => {
    const f = fTop * _RULE.body * sheet.module;
    const side = sideModules * sheet.module;
    const lines = Math.floor(((sheet.height - bottomLines * sheet.module - side) / sheet.module + 1 - (rows - 1) * gutterLines) / rows);
    const lastBaseline = side + (rows * lines + (rows - 1) * gutterLines - 1) * sheet.module;
    return {
        margins: { top: side - f, bottom: sheet.height - lastBaseline, left: side, right: side },
        columns: { count: _RULE.document.columns, gutter: sheet.module },
        rows: { count: rows, lines, gutter: gutterLines * sheet.module },
        firstBaseline: side,
        lastBaseline,
    };
};

const _document = (sheet: (typeof _documents)[number], fTop: number): PageSize => {
    const digital = sheet.family === 'digital';
    const grid = _grid({
        sheet,
        fTop,
        sideModules: _RULE.document.sideModules,
        rows: sheet.rows,
        gutterLines: sheet.gutterLines,
        bottomLines: digital ? _RULE.document.digitalBottomLines : _RULE.document.bottomLines,
    });
    const after = Option.some(grid.lastBaseline + (digital ? _RULE.document.footerLines : _RULE.document.folioLines) * sheet.module);
    return { ...sheet, ...grid, folio: digital ? Option.none() : after, footer: digital ? after : Option.none(), sheet: Option.none(), swatchColumn: digital ? 'rgb' : 'cmyk' };
};

const _sheetGrid = (sheet: Sheet & { readonly border: Pick<Sides, 'left' | 'right'> }): PageSize => {
    const { left, right } = sheet.border;
    const across = Math.floor((sheet.width - left - right) / _RULE.technical.drawingModule) - 1;
    const down = Math.floor((sheet.height - 2 * right) / _RULE.technical.drawingModule);
    const vertical = (sheet.height - down * _RULE.technical.drawingModule) / 2;
    return {
        ...Struct.omit(sheet, ['border']),
        margins: { top: vertical, bottom: vertical, left, right },
        columns: { count: 0, gutter: 0 },
        rows: { count: 0, lines: 0, gutter: 0 },
        firstBaseline: vertical,
        lastBaseline: sheet.height - vertical,
        folio: Option.none(),
        footer: Option.none(),
        sheet: Option.some({
            border: { top: vertical, bottom: vertical, left, right },
            modules: { columns: across, rows: down },
            strip: sheet.width - left - right - across * _RULE.technical.drawingModule,
        }),
        parents: _PARENTS.sheet,
        swatchColumn: 'cmyk',
    };
};

const _screen = (format: (typeof _SCREENS)[number], fTop: number): PageSize => {
    const sheet = { ..._sheet(format.label, 'screen', format.intent, { ...format, unit: 'px' }), module: _RULE.screen.module };
    const shorter = Math.min(sheet.width, sheet.height);
    const sideModules = pipe(
        Array.findFirst(_RULE.screen.sides, (entry) => shorter >= entry.shorter),
        Option.map(Struct.get('modules')),
        Option.getOrElse(() => 1),
    );
    const side = sideModules * sheet.module;
    const available = Math.floor((sheet.height - 2 * side) / sheet.module) + 1;
    const rowRule = Option.getOrElse(
        Array.findFirst(_RULE.screen.rows, (candidate) => available >= candidate.lines),
        () => ({ rows: 1, gutterLines: 1 }),
    );
    const count = pipe(
        Array.range(1, _RULE.document.columns),
        Array.filter((divisor) => _RULE.document.columns % divisor === 0),
        Array.reverse,
        Array.findFirst((candidate) => (sheet.width - 2 * side - (candidate - 1) * sheet.module) / candidate >= _RULE.screen.columnModules * sheet.module),
        Option.getOrElse(() => 1),
    );
    const grid = _grid({ sheet, fTop, sideModules, rows: rowRule.rows, gutterLines: rowRule.gutterLines, bottomLines: sideModules });
    return {
        ...sheet,
        ...grid,
        columns: { count, gutter: sheet.module },
        folio: Option.none(),
        footer: grid.margins.bottom >= _RULE.screen.footerBottomLines * sheet.module ? Option.some(grid.lastBaseline + _RULE.screen.footerLines * sheet.module) : Option.none(),
        sheet: Option.none(),
        parents: count >= _RULE.screen.deckColumns && rowRule.rows >= _RULE.screen.deckRows ? _PARENTS.deck : _PARENTS.title,
        swatchColumn: 'rgb',
    };
};

const pageSizes = (fTop: number): readonly PageSize[] => [
    ...Array.map(_documents, (sheet) => _document(sheet, fTop)),
    ...Array.map([..._technical, ..._isoTechnical], _sheetGrid),
    ...Array.map(_SCREENS, (format) => _screen(format, fTop)),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Grid, PageSize, Sheet, Sides, Standard };
export { pageSizes, standardSheet };
