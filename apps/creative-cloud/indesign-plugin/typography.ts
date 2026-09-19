// --- [IMPORTS] -------------------------------------------------------------------------

import {
    AnchoredRelativeTo,
    AnchorPoint,
    AnchorPosition,
    AutoSizingTypeEnum,
    BaselineFrameGridRelativeOption,
    Capitalization,
    CharacterDirectionOptions,
    CornerOptions,
    DiacriticPositionOptions,
    DigitsTypeOptions,
    EmptyFrameFittingOptions,
    FirstBaseline,
    FlexDirection,
    FlexPosition,
    FlexWidthHeightMode,
    FlexWrap,
    HorizontalAlignment,
    Justification,
    KashidasOptions,
    ListAlignment,
    ListType,
    OTFFigureStyle,
    ParagraphDirectionOptions,
    ParagraphJustificationOptions,
    ParagraphShadingWidthEnum,
    Position,
    RuleWidth,
    SingleWordJustification,
    SourceType,
    SpanColumnCountOptions,
    SpanColumnTypeOptions,
    StartParagraph,
    StoryDirectionOptions,
    StrokeAlignment,
    StrokeOrderTypes,
    TabStopAlignment,
    TagType,
    TextWrapModes,
    VerticalAlignment,
    VerticalJustification,
    VerticallyRelativeTo,
} from 'adobe:indesign';
import type { CharacterStyle, Color, Document, Font, LanguageWithVendors, ObjectStyle, ParagraphStyle, TableStyle } from '@rasm/creative-cloud-server/indesign';
import { type Body, FontSource, TYPOGRAPHY_ANCHORS, TYPOGRAPHY_ROLES } from '@rasm/creative-cloud-server/indesign/jobs';
import { fitMetrics, MetricsError } from '@rasm/typography/metrics';
import { Array, Effect, Equal, Function, HashMap, identity, Option, Record, Result, Schema, Struct } from 'effect';

// --- [ROLES] ---------------------------------------------------------------------------

const _PARAGRAPHS = {
    base: 'Base',
    body: 'Body',
    first: 'Body First',
    listed: 'Body Listed',
    small: 'Body Small',
    heading: 'Heading',
    h1: 'H1',
    h2: 'H2',
    display: 'Display',
    title: 'Title',
    subtitle: 'Subtitle',
    caption: 'Caption',
    note: 'Note',
    running: 'Running Header',
    indicator: 'Section Indicator',
    tableBody: 'Table Body',
    tableHeader: 'Table Header',
    tableNumeric: 'Table Numeric',
    tableTotal: 'Table Total',
    contentsTitle: 'Contents Title',
    contents1: 'Contents L1',
    contents2: 'Contents L2',
    contents3: 'Contents L3',
    bullet: 'List Bullet',
    numbered: 'List Numbered',
    numberedFirst: 'List Numbered First',
    quote: 'Pull Quote',
    code: 'Code',
    tableCaption: 'Table Caption',
    label: 'Form Label',
    lead: 'Body Lead',
    bulletSplit: 'List Bullet Split',
    numberedSplit: 'List Numbered Split',
    bodyFa: 'Body Fa',
    headingFa: 'Heading Fa',
    captionFa: 'Caption Fa',
    bodyAr: 'Body Ar',
    headingAr: 'Heading Ar',
    captionAr: 'Caption Ar',
} as const;
const _CHARACTERS = {
    emphasis: 'Emphasis',
    strong: 'Strong',
    strongEmphasis: 'Strong Emphasis',
    smallCaps: 'Small Caps',
    note: 'Note Reference',
    code: 'Code',
    figures: 'Tabular Figures',
    noBreak: 'No Break',
    hyperlink: 'Hyperlink',
    accent: 'Accent',
    muted: 'Muted',
} as const;
const _OBJECTS = {
    text: 'Text Frame',
    persian: 'Text Frame Fa',
    arabic: 'Text Frame Ar',
    figure: 'Figure',
    caption: 'Caption',
    sidebar: 'Sidebar',
    table: 'Table Frame',
    form: 'Form Field',
    button: 'Button',
    field: 'Field',
    row: 'Flex Row',
    column: 'Flex Column',
} as const;
const _SCALE = { small: 0.75, text: 1, subtitle: 1.5, heading: 2, title: 3, display: 4 } as const;
const _DETAIL = { muted: 50, shading: 6, tintMaximum: 100, inset: 3, corner: 5, dropCapAlignment: 3 } as const;

interface Typography {
    readonly paragraphs: Readonly<Record<keyof typeof _PARAGRAPHS, ParagraphStyle>>;
    readonly characters: Readonly<Record<keyof typeof _CHARACTERS, CharacterStyle>>;
    readonly objects: Readonly<Record<keyof typeof _OBJECTS, ObjectStyle>>;
    readonly white: Readonly<Record<keyof typeof _PARAGRAPHS, ParagraphStyle>>;
    readonly table: TableStyle;
    readonly ink: { readonly above: number; readonly below: number };
    readonly alignment: { readonly above: number; readonly below: number };
    readonly extents: Readonly<Record<keyof typeof _PARAGRAPHS, { readonly above: number; readonly below: number }>>;
    readonly rule: number;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const rightToLeft = (role: Exclude<keyof Body<'buildTypography'>['languages'], 'latin'>, language: LanguageWithVendors, composer: string): ParagraphStyle['properties'] => ({
    composer,
    appliedLanguage: language,
    paragraphDirection: ParagraphDirectionOptions.RIGHT_TO_LEFT_DIRECTION,
    characterDirection: CharacterDirectionOptions.DEFAULT_DIRECTION,
    ...{
        persian: { digitsType: DigitsTypeOptions.FARSI_DIGITS, kashidas: KashidasOptions.KASHIDAS_OFF, paragraphKashidaWidth: 0 },
        arabic: { digitsType: DigitsTypeOptions.HINDI_DIGITS, kashidas: KashidasOptions.DEFAULT_KASHIDAS, paragraphKashidaWidth: 2 },
    }[role],
    justification: Justification.RIGHT_JUSTIFIED,
    hyphenation: false,
    otfMark: true,
    otfLocale: true,
    ignoreEdgeAlignment: true,
    balanceRaggedLines: false,
    diacriticPosition: DiacriticPositionOptions.OPENTYPE_POSITION_FROM_BASELINE,
    xOffsetDiacritic: 0,
    yOffsetDiacritic: 0,
});

const typography: (
    document: Document,
    input: {
        readonly module: number;
        readonly measure: number;
        readonly measured: Body<'buildTypography'>['fonts'];
        readonly fonts: Readonly<Record<keyof Body<'buildTypography'>['fonts'], Font>>;
        readonly languages: Readonly<Record<keyof Body<'buildTypography'>['languages'] | 'code', LanguageWithVendors>>;
        readonly localized: { readonly composer: string; readonly rtl: string; readonly kerning: string };
        readonly colors: Readonly<Record<'Accent' | 'Ink' | 'Field', Color>>;
    },
) => Effect.Effect<Typography, Array.NonEmptyReadonlyArray<typeof MetricsError.Type>> = Effect.fnUntraced(function* (
    document,
    { module: leading, measure, measured, fonts, languages, localized, colors },
) {
    const bodyLine = yield* Effect.fromOption(
        Option.all(Struct.pick(measured.body.metrics, ['ascent', 'descent', 'lineGap'])),
        () => [MetricsError.cases.metricsMissing.make({ postScriptName: measured.body.postScriptName, fields: ['ascent', 'descent', 'lineGap'] })] as const,
    );
    const body = yield* Effect.fromResult(fitMetrics(measured.body, { height: leading, extent: bodyLine.ascent - bodyLine.descent + bodyLine.lineGap })).pipe(Effect.mapError(Array.of));
    const anchors = Struct.pick(TYPOGRAPHY_ANCHORS, ['cap', 'ascender', 'lowercase']);
    const reference = yield* Effect.fromOption(
        Option.all(Record.map(anchors, (point) => HashMap.get(body.glyphs, point))),
        () => [MetricsError.cases.metricsMissing.make({ postScriptName: body.postScriptName, fields: ['glyphs'] })] as const,
    );
    const primary = Record.map(Struct.pick(TYPOGRAPHY_ROLES, Array.dedupe(Array.map(Record.values(TYPOGRAPHY_ROLES), Struct.get('family')))), ({ alignment }, role) =>
        Result.gen(function* () {
            const extents = yield* Result.fromOption(
                Option.all({ glyph: HashMap.get(measured[role].glyphs, alignment.glyph), reference: HashMap.get(body.glyphs, alignment.reference) }),
                Function.constant(MetricsError.cases.metricsMissing.make({ postScriptName: measured[role].postScriptName, fields: [String(alignment.glyph)] })),
            );
            return yield* fitMetrics(measured[role], { height: extents.reference.yBearing, extent: extents.glyph.yBearing });
        }),
    );
    const alignments = Record.map(TYPOGRAPHY_ROLES, (definition, role) =>
        Result.gen(function* () {
            const parent = yield* primary[definition.family];
            const face = role === definition.family ? parent : yield* fitMetrics(measured[role], { height: parent.size, extent: measured[role].size });
            const line = yield* Result.fromOption(
                Option.all(Struct.pick(face.metrics, ['ascent', 'descent', 'lineGap'])),
                Function.constant(MetricsError.cases.metricsMissing.make({ postScriptName: face.postScriptName, fields: ['ascent', 'descent', 'lineGap'] })),
            );
            return { face, line };
        }),
    );
    const [errors] = Array.partition(Record.values(alignments), identity);
    if (Array.isArrayNonEmpty(errors)) {
        return yield* Effect.fail(Array.dedupe(errors));
    }
    const aligned = yield* Effect.fromResult(Result.all(alignments)).pipe(Effect.mapError(Array.of));
    const family = Record.map(fonts, (font, role) => ({
        appliedFont: font,
        fontStyle: font.fontStyleName,
        ...(measured[role].designAxes.length > 0 ? { designAxes: [...measured[role].designAxes] } : {}),
    }));
    const type = Record.map(aligned, ({ face, line: metrics }, role) =>
        Record.map(_SCALE, (step) => {
            const quantum = leading * (_SCALE.text - _SCALE.small);
            const line = Math.ceil(((metrics.ascent - metrics.descent + metrics.lineGap) * step) / quantum) * quantum;
            return { ...family[role], pointSize: face.size * step, leading: line, alignToBaseline: true, gridAlignFirstLineOnly: !Number.isInteger(line / leading) };
        }),
    );
    const strokeFields = ['underlineThickness', 'strikeoutThickness'] as const;
    const strokes = Array.filter(Array.flatMap(Record.values(Struct.pick(body.metrics, strokeFields)), Option.toArray), (thickness) => thickness > 0);
    if (!Array.isArrayNonEmpty(strokes)) {
        return yield* Effect.fail([MetricsError.cases.metricsMissing.make({ postScriptName: body.postScriptName, fields: strokeFields })] as const);
    }
    const rule = Math.max(...strokes);
    const hairline = Math.min(...strokes);
    const none = document.swatches.firstItem();
    const characterProperties = {
        emphasis: family.emphasis,
        strong: family.strong,
        strongEmphasis: family.strongEmphasis,
        smallCaps: { capitalization: Capitalization.CAP_TO_SMALL_CAP },
        note: { position: Position.OT_SUPERSCRIPT },
        code: family.mono,
        figures: { otfFigureStyle: OTFFigureStyle.TABULAR_LINING },
        noBreak: { noBreak: true },
        hyperlink: { underline: true },
        accent: { fillColor: colors.Accent },
        muted: { fillTint: _DETAIL.muted },
    } satisfies Readonly<Record<keyof typeof _CHARACTERS, CharacterStyle['properties']>>;
    const characters = Record.map(characterProperties, (properties, role) => {
        let collection = document.characterStyles;
        if ('appliedFont' in properties) {
            const name = properties.appliedFont.fontFamily;
            const group = document.characterStyleGroups.itemByName(name);
            collection = (group.isValid ? group : document.characterStyleGroups.add({ name })).characterStyles;
        }
        return collection.add({ name: _CHARACTERS[role], basedOn: document.characterStyles.firstItem(), ...properties });
    });
    const list = document.numberingLists.add({ name: 'Body List', continueNumbersAcrossStories: true, continueNumbersAcrossDocuments: true });
    const paragraphs = Record.map(_PARAGRAPHS, (name) => document.paragraphStyles.add({ name }));
    const base: ParagraphStyle['properties'] = {
        basedOn: document.paragraphStyles.firstItem(),
        ...type.body.text,
        spaceBefore: 0,
        spaceAfter: 0,
        leftIndent: 0,
        rightIndent: 0,
        firstLineIndent: 0,
        lastLineIndent: 0,
        justification: Justification.LEFT_ALIGN,
        singleWordJustification: SingleWordJustification.LEFT_ALIGN,
        composer: localized.composer,
        appliedLanguage: languages.latin,
        kerningMethod: localized.kerning,
        tracking: 0,
        hyphenation: false,
        hyphenateWordsLongerThan: 6,
        hyphenateAfterFirst: 3,
        hyphenateBeforeLast: 3,
        hyphenateLadderLimit: 2,
        hyphenateCapitalizedWords: false,
        hyphenateLastWord: false,
        hyphenateAcrossColumns: false,
        hyphenWeight: 5,
        minimumWordSpacing: 90,
        desiredWordSpacing: 100,
        maximumWordSpacing: 110,
        minimumLetterSpacing: -2,
        desiredLetterSpacing: 0,
        maximumLetterSpacing: 2,
        minimumGlyphScaling: 98,
        desiredGlyphScaling: 100,
        maximumGlyphScaling: 102,
        otfFigureStyle: OTFFigureStyle.PROPORTIONAL_LINING,
        otfContextualAlternate: true,
        ligatures: true,
        otfDiscretionaryLigature: false,
        otfStylisticSets: 0,
        capitalization: Capitalization.NORMAL,
        keepLinesTogether: true,
        keepFirstLines: 2,
        keepLastLines: 2,
        keepAllLinesTogether: false,
        keepWithNext: 0,
        startParagraph: StartParagraph.ANYWHERE,
        spanColumnType: SpanColumnTypeOptions.SINGLE_COLUMN,
        dropCapLines: 0,
        dropCapCharacters: 0,
        ruleAbove: false,
        ruleBelow: false,
        paragraphShadingOn: false,
        paragraphBorderOn: false,
        bulletsAndNumberingListType: ListType.NO_LIST,
        tabList: [],
        fillColor: colors.Ink,
        ignoreEdgeAlignment: false,
        balanceRaggedLines: false,
        paragraphDirection: ParagraphDirectionOptions.LEFT_TO_RIGHT_DIRECTION,
        characterDirection: CharacterDirectionOptions.DEFAULT_DIRECTION,
        digitsType: DigitsTypeOptions.DEFAULT_DIGITS,
        kashidas: KashidasOptions.KASHIDAS_OFF,
        paragraphKashidaWidth: 0,
        paragraphJustification: ParagraphJustificationOptions.DEFAULT_JUSTIFICATION,
        diacriticPosition: DiacriticPositionOptions.OPENTYPE_POSITION_FROM_BASELINE,
        otfJustificationAlternate: false,
    };
    const heading: ParagraphStyle['properties'] = { basedOn: paragraphs.base, ...type.strong.text, spaceBefore: leading, keepWithNext: 2, keepAllLinesTogether: true, balanceRaggedLines: true };
    const entries = Record.map({ contents1: 0, contents2: 1, contents3: 2 }, (indent): ParagraphStyle['properties'] => ({
        basedOn: paragraphs.base,
        ...(indent === 0 ? type.strong.text : type.body.text),
        leftIndent: indent * leading,
        keepWithNext: indent === 0 ? 1 : 0,
        tabList: [{ alignment: TabStopAlignment.RIGHT_ALIGN, position: measure, leader: indent === 0 ? '' : '.' }],
        otfFigureStyle: OTFFigureStyle.TABULAR_LINING,
    }));
    const rtl = Record.map(Struct.pick(languages, ['persian', 'arabic']), (language, role) => rightToLeft(role, language, localized.rtl));
    const ragged = Record.map(rtl, (properties) => ({ ...properties, ...Struct.pick(base, ['kashidas', 'paragraphKashidaWidth']), justification: Justification.RIGHT_ALIGN }));
    const paragraph: Readonly<Record<keyof typeof paragraphs, ParagraphStyle['properties']>> = {
        base,
        body: { basedOn: paragraphs.base, hyphenation: true, firstLineIndent: leading },
        first: { basedOn: paragraphs.body, firstLineIndent: 0 },
        listed: { basedOn: paragraphs.body, leftIndent: 2 * leading, firstLineIndent: 0 },
        small: { basedOn: paragraphs.base, ...type.body.small, hyphenation: true },
        heading,
        h1: {
            ...heading,
            ...type.strong.heading,
            spaceBefore: 2 * leading,
            ruleBelow: true,
            ruleBelowLineWeight: rule,
            ruleBelowColor: colors.Ink,
            ruleBelowWidth: RuleWidth.COLUMN_WIDTH,
        },
        h2: { ...heading, ...type.strong.subtitle },
        display: { ...heading, ...type.display.display, alignToBaseline: false, spaceBefore: 0, ignoreEdgeAlignment: true },
        title: { ...heading, ...type.display.title, spaceBefore: 0, ignoreEdgeAlignment: true },
        subtitle: { basedOn: paragraphs.base, ...type.body.subtitle, spaceAfter: leading, keepWithNext: 2, keepAllLinesTogether: true },
        caption: { basedOn: paragraphs.base, ...type.body.small, keepAllLinesTogether: true },
        note: { basedOn: paragraphs.small, leftIndent: leading, firstLineIndent: -leading, tabList: [{ alignment: TabStopAlignment.LEFT_ALIGN, position: leading, leader: '' }] },
        running: { basedOn: paragraphs.caption, otfFigureStyle: OTFFigureStyle.TABULAR_LINING, keepLinesTogether: false, keepAllLinesTogether: false },
        indicator: { basedOn: paragraphs.caption, justification: Justification.RIGHT_ALIGN, otfFigureStyle: OTFFigureStyle.TABULAR_LINING, keepLinesTogether: false, keepAllLinesTogether: false },
        tableBody: { basedOn: paragraphs.base, ...type.body.small, leading, alignToBaseline: false, otfFigureStyle: OTFFigureStyle.TABULAR_LINING, keepLinesTogether: false },
        tableHeader: { basedOn: paragraphs.tableBody, ...type.strong.small, leading, alignToBaseline: false },
        tableNumeric: { basedOn: paragraphs.tableBody, justification: Justification.RIGHT_ALIGN },
        tableTotal: { basedOn: paragraphs.tableBody, ...type.strong.small, leading, alignToBaseline: false, justification: Justification.RIGHT_ALIGN },
        contentsTitle: { ...heading, ...type.strong.heading, spaceBefore: 0, spaceAfter: leading },
        ...entries,
        bullet: {
            basedOn: paragraphs.base,
            hyphenation: true,
            leftIndent: leading,
            firstLineIndent: -leading,
            tabList: [{ alignment: TabStopAlignment.LEFT_ALIGN, position: leading, leader: '' }],
            bulletsAndNumberingListType: ListType.BULLET_LIST,
            bulletsCharacterStyle: document.characterStyles.firstItem(),
            bulletsTextAfter: '^t',
            bulletsAlignment: ListAlignment.LEFT_ALIGN,
        },
        numbered: {
            basedOn: paragraphs.base,
            hyphenation: true,
            leftIndent: 2 * leading,
            firstLineIndent: -2 * leading,
            tabList: [{ alignment: TabStopAlignment.LEFT_ALIGN, position: 2 * leading, leader: '' }],
            bulletsAndNumberingListType: ListType.NUMBERED_LIST,
            appliedNumberingList: list,
            numberingExpression: '^#.^t',
            numberingFormat: '1, 2, 3, 4...',
            numberingAlignment: ListAlignment.RIGHT_ALIGN,
            numberingCharacterStyle: characters.figures,
            numberingContinue: true,
            numberingLevel: 1,
        },
        numberedFirst: { basedOn: paragraphs.numbered, numberingContinue: false, numberingStartAt: 1 },
        quote: {
            basedOn: paragraphs.base,
            ...type.quote.subtitle,
            leftIndent: 2 * leading,
            spaceBefore: leading,
            spaceAfter: leading,
            keepAllLinesTogether: true,
            ignoreEdgeAlignment: true,
            otfFigureStyle: OTFFigureStyle.PROPORTIONAL_OLDSTYLE,
            otfDiscretionaryLigature: true,
            spanColumnType: SpanColumnTypeOptions.SPAN_COLUMNS,
            spanSplitColumnCount: SpanColumnCountOptions.ALL,
            spanColumnMinSpaceBefore: leading,
            spanColumnMinSpaceAfter: leading,
        },
        code: {
            basedOn: paragraphs.base,
            ...type.mono.small,
            ligatures: false,
            otfContextualAlternate: false,
            leftIndent: leading,
            spaceBefore: leading,
            spaceAfter: leading,
            keepFirstLines: 3,
            keepLastLines: 3,
            paragraphShadingOn: true,
            paragraphShadingColor: colors.Ink,
            paragraphShadingTint: _DETAIL.shading,
            paragraphShadingLeftOffset: leading / 2,
            paragraphShadingRightOffset: leading / 2,
            paragraphShadingTopOffset: 0,
            paragraphShadingBottomOffset: 0,
            paragraphShadingWidth: ParagraphShadingWidthEnum.COLUMN_WIDTH,
            otfFigureStyle: OTFFigureStyle.TABULAR_LINING,
            appliedLanguage: languages.code,
            hyphenation: false,
        },
        tableCaption: { basedOn: paragraphs.caption, keepWithNext: 1, spaceBefore: leading },
        label: {
            basedOn: paragraphs.caption,
            ...type.serif.small,
            capitalization: Capitalization.CAP_TO_SMALL_CAP,
            otfFigureStyle: OTFFigureStyle.TABULAR_LINING,
            keepLinesTogether: false,
            keepAllLinesTogether: false,
        },
        lead: { basedOn: paragraphs.body, firstLineIndent: 0, dropCapLines: 2, dropCapCharacters: 1, dropCapStyle: characters.strong, dropcapDetail: _DETAIL.dropCapAlignment },
        ...Record.map({ bulletSplit: paragraphs.bullet, numberedSplit: paragraphs.numbered }, (basedOn) => ({
            basedOn,
            spanColumnType: SpanColumnTypeOptions.SPLIT_COLUMNS,
            spanSplitColumnCount: 2,
            splitColumnInsideGutter: leading,
            splitColumnOutsideGutter: 0,
        })),
        bodyFa: { basedOn: paragraphs.body, ...type.persian.text, ...rtl.persian },
        headingFa: { ...heading, ...type.persianHeading.text, ...ragged.persian },
        captionFa: { basedOn: paragraphs.caption, ...type.persian.small, ...ragged.persian },
        bodyAr: { basedOn: paragraphs.body, ...type.arabic.text, ...rtl.arabic },
        headingAr: { ...heading, ...type.arabicHeading.text, ...ragged.arabic },
        captionAr: { basedOn: paragraphs.caption, ...type.arabic.small, ...ragged.arabic },
    };
    const next = {
        ...paragraphs,
        first: paragraphs.body,
        lead: paragraphs.body,
        heading: paragraphs.first,
        h1: paragraphs.first,
        h2: paragraphs.first,
        display: paragraphs.title,
        title: paragraphs.subtitle,
        subtitle: paragraphs.first,
        tableHeader: paragraphs.tableBody,
        contentsTitle: paragraphs.contents1,
        numberedFirst: paragraphs.numbered,
        quote: paragraphs.first,
        tableCaption: paragraphs.first,
        bulletSplit: paragraphs.bullet,
        numberedSplit: paragraphs.numbered,
        headingFa: paragraphs.bodyFa,
        headingAr: paragraphs.bodyAr,
    };
    Array.forEach(Record.toEntries(paragraph), ([role, properties]) => {
        paragraphs[role].properties = { ...properties, nextStyle: next[role] };
    });
    const instances = Array.map(Record.toEntries(aligned), ([role, metrics]) => ({ native: measured[role].native, metrics }));
    const measurements = yield* Effect.all(
        Record.map(paragraphs, (style) => {
            const font = style.appliedFont;
            const native = Schema.decodeUnknownSync(FontSource)(font.properties);
            const instance = Array.findFirst(
                instances,
                ({ native: expected, metrics: { face } }) => Equal.equals(expected, native) && Equal.equals(face.designAxes, face.designAxes.length === 0 ? [] : style.designAxes),
            );
            return Effect.map(
                Effect.fromOption(instance, () => MetricsError.cases.metricsMissing.make({ postScriptName: font.postscriptName, fields: ['fontInstance'] })),
                ({ metrics: { face, line } }) => {
                    const scale = Number(style.pointSize) / face.size;
                    return { above: line.ascent * scale, below: -line.descent * scale };
                },
            );
        }),
        { mode: 'result' },
    );
    const [measurementErrors] = Array.partition(Record.values(measurements), identity);
    if (Array.isArrayNonEmpty(measurementErrors)) {
        return yield* Effect.fail(measurementErrors);
    }
    const extents = yield* Effect.fromResult(Result.all(measurements)).pipe(Effect.mapError(Array.of));
    const ruleOffset = extents.h1.below + rule;
    paragraphs.h1.properties = {
        ruleBelowOffset: ruleOffset,
        spaceAfter: Math.max(0, Math.ceil((ruleOffset + rule + extents.first.above) / leading) * leading - Number(paragraphs.first.leading)),
    };
    const grep = [
        { styles: [paragraphs.tableBody, paragraphs.tableHeader, paragraphs.tableNumeric, paragraphs.tableTotal], expression: String.raw`\d+([.,:]\d+)*`, character: characters.figures },
        {
            styles: [paragraphs.body, paragraphs.small, paragraphs.note, paragraphs.caption],
            expression: String.raw`\d+ ?(?:mm|cm|m|km|pt|pc|px|in|ft|kg|g)\b|\d+ ?[%°]`,
            character: characters.noBreak,
        },
        { styles: [paragraphs.body, paragraphs.small, paragraphs.note, paragraphs.caption], expression: String.raw`(?:§|No\.|Nos\.|Fig\.|Figs\.|Tab\.|p\.|pp\.) ?\d+`, character: characters.noBreak },
        { styles: [paragraphs.bodyFa, paragraphs.headingFa, paragraphs.captionFa], expression: String.raw`[^\s~j]+(?:~j[^\s~j]+)+`, character: characters.noBreak },
        { styles: [paragraphs.indicator], expression: '^[^/]+(?=/)', character: characters.accent },
        { styles: [paragraphs.indicator], expression: '(?<=/).+$', character: characters.muted },
    ];
    Array.forEach(
        Array.flatMap(grep, (row) => Array.map(row.styles, (style) => ({ style, ...Struct.omit(row, ['styles']) }))),
        ({ style, expression, character }) => style.nestedGrepStyles.add({ grepExpression: expression, appliedCharacterStyle: character }),
    );
    const whiteGroup = document.paragraphStyleGroups.add({ name: 'White' });
    const white = Record.map(paragraphs, (parent) =>
        whiteGroup.paragraphStyles.add({
            name: parent.name,
            basedOn: parent,
            fillColor: colors.Field,
            ...Record.map(
                Record.filter(
                    {
                        ruleAboveColor: parent.ruleAbove,
                        ruleBelowColor: parent.ruleBelow,
                        paragraphBorderColor: parent.paragraphBorderOn,
                    },
                    Function.identity,
                ),
                () => colors.Field,
            ),
            ...(parent.paragraphShadingOn ? { paragraphShadingColor: colors.Ink, paragraphShadingTint: _DETAIL.tintMaximum - parent.paragraphShadingTint } : {}),
        }),
    );
    const reversed = HashMap.fromIterable(Array.map(Record.toEntries(paragraphs), ([role, parent]) => [parent.id, white[role]] as const));
    Array.forEach(Record.keys(white), (role) =>
        Option.map(HashMap.get(reversed, paragraphs[role].nextStyle.id), (nextStyle) => {
            white[role].nextStyle = nextStyle;
        }),
    );
    const tags = {
        ...Record.map(paragraphs, () => 'P'),
        h1: 'H1',
        h2: 'H2',
        heading: 'H3',
        display: 'H1',
        title: 'H1',
        contentsTitle: 'H1',
        headingFa: 'H3',
        headingAr: 'H3',
        running: 'Artifact',
        indicator: 'Artifact',
    };
    Array.forEach(Record.toEntries(paragraphs), ([role, style]) => style.styleExportTagMaps.add('PDF', tags[role], '', ''));
    Array.forEach(Record.toEntries(white), ([role, style]) => style.styleExportTagMaps.add('PDF', tags[role], '', ''));

    const cellBody = document.cellStyles.add({
        name: 'Body',
        basedOn: document.cellStyles.firstItem(),
        appliedParagraphStyle: paragraphs.tableBody,
        textTopInset: 0,
        textBottomInset: 0,
        textLeftInset: leading / _DETAIL.inset,
        textRightInset: leading / _DETAIL.inset,
        verticalJustification: VerticalJustification.TOP_ALIGN,
        firstBaselineOffset: FirstBaseline.LEADING_OFFSET,
        minimumFirstBaselineOffset: 0,
        fillColor: none,
        topEdgeStrokeWeight: 0,
        bottomEdgeStrokeWeight: 0,
        leftEdgeStrokeWeight: 0,
        rightEdgeStrokeWeight: 0,
        clipContentToTextCell: false,
    });
    const cells = Record.map(
        {
            header: {
                name: 'Header',
                appliedParagraphStyle: paragraphs.tableHeader,
                bottomEdgeStrokeWeight: rule,
                bottomEdgeStrokeColor: colors.Ink,
                verticalJustification: VerticalJustification.BOTTOM_ALIGN,
            },
            numeric: { name: 'Numeric', appliedParagraphStyle: paragraphs.tableNumeric },
            total: { name: 'Total', appliedParagraphStyle: paragraphs.tableTotal, topEdgeStrokeWeight: rule, topEdgeStrokeColor: colors.Ink },
        },
        (properties) => document.cellStyles.add({ basedOn: cellBody, ...properties }),
    );
    const table = document.tableStyles.add({
        name: 'Table',
        basedOn: document.tableStyles.firstItem(),
        headerRegionCellStyle: cells.header,
        bodyRegionCellStyle: cellBody,
        footerRegionCellStyle: cells.total,
        leftColumnRegionCellStyle: cellBody,
        rightColumnRegionCellStyle: cellBody,
        topBorderStrokeWeight: rule,
        topBorderStrokeColor: colors.Ink,
        bottomBorderStrokeWeight: rule,
        bottomBorderStrokeColor: colors.Ink,
        leftBorderStrokeWeight: 0,
        rightBorderStrokeWeight: 0,
        startRowStrokeCount: 1,
        startRowStrokeWeight: hairline,
        startRowStrokeColor: colors.Ink,
        startRowStrokeTint: _DETAIL.muted,
        endRowStrokeCount: 1,
        endRowStrokeWeight: hairline,
        endRowStrokeColor: colors.Ink,
        endRowStrokeTint: _DETAIL.muted,
        startColumnStrokeCount: 0,
        endColumnStrokeCount: 0,
        startRowFillCount: 0,
        endRowFillCount: 0,
        spaceBefore: 0,
        spaceAfter: leading,
        strokeOrder: StrokeOrderTypes.ROW_ON_TOP,
        textTopInset: 0,
        textBottomInset: 0,
        textLeftInset: leading / _DETAIL.inset,
        textRightInset: leading / _DETAIL.inset,
    });
    const objects = Record.map(_OBJECTS, (name) => document.objectStyles.add({ name }));
    const object: Readonly<Record<keyof typeof objects, ObjectStyle['properties']>> = {
        text: {
            basedOn: document.objectStyles.firstItem(),
            enableFill: true,
            enableStroke: true,
            enableTextFrameGeneralOptions: true,
            enableTextFrameBaselineOptions: true,
            enableStoryOptions: true,
            enableParagraphStyle: true,
            enableTextFrameAutoSizingOptions: true,
            enableTextWrapAndOthers: true,
            fillColor: none,
            strokeColor: none,
            appliedParagraphStyle: paragraphs.body,
            applyNextParagraphStyle: false,
            textFramePreferences: {
                insetSpacing: 0,
                firstBaselineOffset: FirstBaseline.FIXED_HEIGHT,
                minimumFirstBaselineOffset: extents.body.above,
                verticalJustification: VerticalJustification.TOP_ALIGN,
                textColumnCount: 1,
                textColumnGutter: leading,
                autoSizingType: AutoSizingTypeEnum.OFF,
                ignoreWrap: false,
            },
            baselineFrameGridOptions: { useCustomBaselineFrameGrid: false },
            storyPreferences: { opticalMarginAlignment: true, opticalMarginSize: body.size, storyDirection: StoryDirectionOptions.LEFT_TO_RIGHT_DIRECTION },
            textWrapPreferences: { textWrapMode: TextWrapModes.NONE },
        },
        ...Record.map({ persian: 'bodyFa', arabic: 'bodyAr' } as const, (role) => ({
            basedOn: objects.text,
            enableStoryOptions: true,
            appliedParagraphStyle: paragraphs[role],
            textFramePreferences: { minimumFirstBaselineOffset: extents[role].above },
            storyPreferences: { storyDirection: StoryDirectionOptions.RIGHT_TO_LEFT_DIRECTION, opticalMarginAlignment: false },
        })),
        row: {
            basedOn: document.objectStyles.firstItem(),
            enableFill: true,
            enableStroke: true,
            enableFlexLayoutAttributes: true,
            fillColor: none,
            strokeColor: none,
            strokeWeight: 0,
            flexLayoutAttributeOptions: {
                flexDirection: FlexDirection.FLEX_ROW,
                flexWrap: FlexWrap.NO_WRAP,
                flexWidthMode: FlexWidthHeightMode.FLEX_FIXED,
                flexHeightMode: FlexWidthHeightMode.FLEX_FIXED,
                justifyContent: FlexPosition.FLEX_START,
                alignItems: FlexPosition.FLEX_START,
                alignContent: FlexPosition.FLEX_START,
                flexGapRow: 0,
                flexGapColumn: 0,
                flexPaddingTop: 0,
                flexPaddingBottom: 0,
                flexPaddingLeft: 0,
                flexPaddingRight: 0,
            },
        },
        column: { basedOn: objects.row, flexLayoutAttributeOptions: { flexDirection: FlexDirection.FLEX_COLUMN } },
        figure: {
            basedOn: document.objectStyles.firstItem(),
            enableFill: true,
            enableStroke: true,
            enableFrameFittingOptions: true,
            enableStrokeAndCornerOptions: true,
            enableTextWrapAndOthers: true,
            fillColor: none,
            strokeColor: none,
            frameFittingOptions: { fittingOnEmptyFrame: EmptyFrameFittingOptions.PROPORTIONALLY, autoFit: false },
            topLeftCornerOption: CornerOptions.NONE,
            topRightCornerOption: CornerOptions.NONE,
            bottomLeftCornerOption: CornerOptions.NONE,
            bottomRightCornerOption: CornerOptions.NONE,
            textWrapPreferences: { textWrapMode: TextWrapModes.BOUNDING_BOX_TEXT_WRAP, textWrapOffset: [0, leading, leading, leading] },
            objectExportOptions: {
                altTextSourceType: SourceType.SOURCE_XMP_DESCRIPTION,
                actualTextSourceType: SourceType.SOURCE_XMP_TITLE,
                applyTagType: TagType.TAG_BASED_ON_OBJECT,
                epubType: 'figure',
                epubAriaRole: 'figure',
            },
        },
        caption: {
            basedOn: objects.text,
            enableTextFrameAutoSizingOptions: true,
            enableParagraphStyle: true,
            enableAnchoredObjectOptions: true,
            appliedParagraphStyle: paragraphs.caption,
            textFramePreferences: {
                firstBaselineOffset: FirstBaseline.LEADING_OFFSET,
                minimumFirstBaselineOffset: type.body.small.leading,
                ignoreWrap: true,
            },
            baselineFrameGridOptions: {
                useCustomBaselineFrameGrid: true,
                baselineFrameGridRelativeOption: BaselineFrameGridRelativeOption.TOP_OF_FRAME,
                startingOffsetForBaselineFrameGrid: type.body.small.leading,
                baselineFrameGridIncrement: type.body.small.leading,
            },
            anchoredObjectSettings: {
                anchoredPosition: AnchorPosition.ABOVE_LINE,
                anchorPoint: AnchorPoint.TOP_LEFT_ANCHOR,
                horizontalAlignment: HorizontalAlignment.TEXT_ALIGN,
                verticalAlignment: VerticalAlignment.TOP_ALIGN,
                spineRelative: false,
                anchorSpaceAbove: 0,
                anchorYoffset: 0,
                lockPosition: true,
            },
            objectExportOptions: { applyTagType: TagType.TAG_FROM_STRUCTURE },
        },
        sidebar: {
            basedOn: objects.text,
            enableParagraphStyle: true,
            enableAnchoredObjectOptions: true,
            enableTextWrapAndOthers: true,
            appliedParagraphStyle: paragraphs.small,
            fillColor: colors.Ink,
            fillTint: _DETAIL.shading,
            textFramePreferences: { insetSpacing: leading, firstBaselineOffset: FirstBaseline.LEADING_OFFSET },
            baselineFrameGridOptions: {
                useCustomBaselineFrameGrid: true,
                baselineFrameGridRelativeOption: BaselineFrameGridRelativeOption.TOP_OF_FRAME,
                startingOffsetForBaselineFrameGrid: leading,
                baselineFrameGridIncrement: type.body.small.leading,
            },
            textWrapPreferences: { textWrapMode: TextWrapModes.BOUNDING_BOX_TEXT_WRAP, textWrapOffset: leading },
            anchoredObjectSettings: {
                anchoredPosition: AnchorPosition.ANCHORED,
                spineRelative: true,
                anchorPoint: AnchorPoint.TOP_RIGHT_ANCHOR,
                horizontalReferencePoint: AnchoredRelativeTo.PAGE_MARGINS,
                horizontalAlignment: HorizontalAlignment.RIGHT_ALIGN,
                verticalReferencePoint: VerticallyRelativeTo.LINE_BASELINE,
                verticalAlignment: VerticalAlignment.TOP_ALIGN,
                anchorXoffset: 0,
                anchorYoffset: 0,
                pinPosition: true,
            },
            objectExportOptions: { applyTagType: TagType.TAG_FROM_STRUCTURE, epubAriaRole: 'complementary' },
        },
        table: {
            basedOn: objects.text,
            appliedParagraphStyle: paragraphs.tableBody,
            textFramePreferences: {
                firstBaselineOffset: FirstBaseline.LEADING_OFFSET,
                minimumFirstBaselineOffset: leading,
            },
            storyPreferences: { opticalMarginAlignment: false },
        },
        form: {
            basedOn: document.objectStyles.firstItem(),
            enableFill: true,
            enableStroke: true,
            enableStrokeAndCornerOptions: true,
            enableTextFrameGeneralOptions: true,
            fillColor: colors.Field,
            strokeColor: colors.Ink,
            strokeWeight: rule,
            strokeAlignment: StrokeAlignment.INSIDE_ALIGNMENT,
            topLeftCornerOption: CornerOptions.NONE,
            topRightCornerOption: CornerOptions.NONE,
            bottomLeftCornerOption: CornerOptions.NONE,
            bottomRightCornerOption: CornerOptions.NONE,
            textFramePreferences: {
                insetSpacing: [leading / _DETAIL.corner, leading / _DETAIL.inset, leading / _DETAIL.corner, leading / _DETAIL.inset],
                verticalJustification: VerticalJustification.CENTER_ALIGN,
            },
            objectExportOptions: { applyTagType: TagType.TAG_BASED_ON_OBJECT },
        },
        button: {
            basedOn: objects.form,
            enableParagraphStyle: true,
            fillColor: colors.Accent,
            strokeColor: none,
            appliedParagraphStyle: paragraphs.label,
            topLeftCornerOption: CornerOptions.ROUNDED_CORNER,
            topRightCornerOption: CornerOptions.ROUNDED_CORNER,
            bottomLeftCornerOption: CornerOptions.ROUNDED_CORNER,
            bottomRightCornerOption: CornerOptions.ROUNDED_CORNER,
            topLeftCornerRadius: leading / _DETAIL.corner,
            topRightCornerRadius: leading / _DETAIL.corner,
            bottomLeftCornerRadius: leading / _DETAIL.corner,
            bottomRightCornerRadius: leading / _DETAIL.corner,
        },
        field: {
            basedOn: document.objectStyles.firstItem(),
            enableFill: true,
            enableStroke: true,
            fillColor: colors.Field,
            strokeColor: none,
            objectExportOptions: { applyTagType: TagType.TAG_ARTIFACT },
        },
    };
    Array.forEach(Record.toEntries(object), ([role, properties]) => {
        objects[role].properties = properties;
    });
    return { paragraphs, characters, objects, white, table, ink: extents.body, alignment: { above: reference.ascender.yBearing, below: 0 }, extents, rule };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Typography };
export { rightToLeft, typography };
