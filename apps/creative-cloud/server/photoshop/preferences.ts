// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Host = import('photoshop').Preferences;
type Class = Exclude<keyof Host, 'typename'>;
type Key<C extends Class> = Exclude<keyof Host[C], 'typename'>;
type Literal<Value> = Value extends string
    ? `${Value}`
    : Value extends readonly (infer Item)[]
      ? readonly Literal<Item>[]
      : Value extends object
        ? { readonly [K in keyof Value]: Literal<Value[K]> }
        : Value;
type Target = { [C in Class]: { [K in Key<C>]: { readonly section: C; readonly key: K; readonly value: Literal<Host[C][K]>; readonly row: number } }[Key<C>] }[Class];
type Section = (typeof Section)['Type'];
type Write = (typeof Write)['Type'];

// --- [TABLE] ---------------------------------------------------------------------------

const TARGETS: Array.NonEmptyReadonlyArray<Target> = [
    { section: 'general', key: 'colorPicker', value: { type: 'photoshopPicker' }, row: 1 },
    { section: 'general', key: 'imageInterpolation', value: 'bicubicAutomatic', row: 3 },
    { section: 'general', key: 'autoUpdateOpenDocuments', value: false, row: 4 },
    { section: 'general', key: 'beepWhenDone', value: false, row: 9 },
    { section: 'general', key: 'exportClipboard', value: true, row: 10 },
    { section: 'interface', key: 'textFontSize', value: 'preferTinyPaletteFontType', row: 20 },
    { section: 'interface', key: 'colorChannelsInColor', value: false, row: 22 },
    { section: 'interface', key: 'dynamicColorSliders', value: true, row: 25 },
    { section: 'tools', key: 'showToolTips', value: true, row: 35 },
    { section: 'tools', key: 'useShiftKeyForToolSwitch', value: true, row: 40 },
    { section: 'tools', key: 'keyboardZoomResizesWindows', value: false, row: 53 },
    { section: 'history', key: 'useHistoryLog', value: false, row: 56 },
    { section: 'history', key: 'numberOfHistoryStates', value: 50, row: 57 },
    { section: 'fileHandling', key: 'imagePreviews', value: 'queryAlways', row: 59 },
    { section: 'fileHandling', key: 'useLowerCaseExtension', value: true, row: 60 },
    { section: 'fileHandling', key: 'askBeforeSavingLayeredTIFF', value: true, row: 70 },
    { section: 'fileHandling', key: 'maximizeCompatibility', value: 'queryAlways', row: 72 },
    { section: 'fileHandling', key: 'recentFileListMaximum', value: 20, row: 73 },
    { section: 'performance', key: 'maxRAMuse', value: 70, row: 78 },
    { section: 'performance', key: 'imageCacheLevels', value: 4, row: 79 },
    { section: 'cursors', key: 'paintingCursors', value: 'brushSize', row: 86 },
    { section: 'cursors', key: 'otherCursors', value: 'precise', row: 86 },
    { section: 'transparencyAndGamut', key: 'gridSize', value: 'medium', row: 88 },
    { section: 'transparencyAndGamut', key: 'gamutWarningOpacity', value: 100, row: 88 },
    { section: 'unitsAndRulers', key: 'rulerUnits', value: 'rulerPixels', row: 89 },
    { section: 'unitsAndRulers', key: 'typeUnits', value: 'rulerPixels', row: 89 },
    { section: 'unitsAndRulers', key: 'pointSize', value: 'POSTSCRIPT', row: 92 },
    { section: 'guidesGridsAndSlices', key: 'guideStyle', value: 'lens', row: 93 },
    { section: 'guidesGridsAndSlices', key: 'gridStyle', value: 'lens', row: 96 },
    { section: 'guidesGridsAndSlices', key: 'gridSubDivisions', value: 3, row: 96 },
    { section: 'guidesGridsAndSlices', key: 'showSliceNumber', value: true, row: 97 },
    { section: 'type', key: 'smartQuotes', value: true, row: 104 },
    { section: 'type', key: 'showEnglishFontNames', value: true, row: 106 },
    { section: 'type', key: 'showTextFeatures', value: 'middleEasternInterface', row: 112 },
    { section: 'notifications', key: 'showToolTips', value: true, row: 35 },
    { section: 'notifications', key: 'useRichToolTips', value: true, row: 36 },
    { section: 'notifications', key: 'showWhatsNew', value: false, row: 37 },
    { section: 'notifications', key: 'showFeatureOnboarding', value: false, row: 38 },
    { section: 'notifications', key: 'quietMode', value: true, row: 34 },
];

// --- [MODELS] --------------------------------------------------------------------------

const Section: Schema.Literals<Class[]> = Schema.Literals(Array.dedupe(Array.map(TARGETS, Struct.get('section'))));

const Write: Schema.Struct<{ readonly section: typeof Section; readonly key: Schema.String; readonly value: Schema.Codec<Schema.Json> }> = Schema.Struct({
    section: Section,
    key: Schema.String,
    value: Schema.Json,
});

const Writes: Schema.NonEmptyArray<typeof Write> = Schema.NonEmptyArray(Write);

const TARGET_ROWS: (typeof Writes)['Type'] = Array.map(TARGETS, Struct.pick(['section', 'key', 'value']));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Write };
export { Section, TARGET_ROWS, TARGETS, Writes };
