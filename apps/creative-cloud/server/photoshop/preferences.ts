// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Record, Schema, Struct } from 'effect';

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

// --- [TABLE] ---------------------------------------------------------------------------

const _TARGETS = {
    general: { colorPicker: { type: 'photoshopPicker' }, imageInterpolation: 'bicubicAutomatic', autoUpdateOpenDocuments: false, beepWhenDone: false, exportClipboard: true },
    interface: { textFontSize: 'preferTinyPaletteFontType', colorChannelsInColor: false, dynamicColorSliders: true },
    tools: { showToolTips: true, useShiftKeyForToolSwitch: true, keyboardZoomResizesWindows: false },
    history: { useHistoryLog: false, numberOfHistoryStates: 50 },
    fileHandling: { imagePreviews: 'queryAlways', useLowerCaseExtension: true, askBeforeSavingLayeredTIFF: true, maximizeCompatibility: 'queryAlways', recentFileListMaximum: 20 },
    performance: { maxRAMuse: 70, imageCacheLevels: 4 },
    cursors: { paintingCursors: 'brushSize', otherCursors: 'precise' },
    transparencyAndGamut: { gridSize: 'medium', gamutWarningOpacity: 100 },
    unitsAndRulers: { rulerUnits: 'rulerPixels', typeUnits: 'rulerPixels', pointSize: 'POSTSCRIPT' },
    guidesGridsAndSlices: { guideStyle: 'lens', gridStyle: 'lens', gridSubDivisions: 3, showSliceNumber: true },
    type: { smartQuotes: true, showEnglishFontNames: true, showTextFeatures: 'middleEasternInterface' },
    notifications: { showToolTips: true, useRichToolTips: true, showWhatsNew: false, showFeatureOnboarding: false, quietMode: true },
} as const satisfies { readonly [C in Class]: Partial<{ readonly [K in Key<C>]: Literal<Host[C][K]> }> };

// --- [MODELS] --------------------------------------------------------------------------

const Section: Schema.Literals<Class[]> = Schema.Literals(Struct.keys(_TARGETS));

const Writes: Schema.NonEmptyArray<Schema.Struct<{ readonly section: typeof Section; readonly key: Schema.String; readonly value: Schema.Codec<Schema.Json> }>> = Schema.NonEmptyArray(
    Schema.Struct({ section: Section, key: Schema.String, value: Schema.Json }),
);

const TARGET_ROWS: (typeof Writes)['Type'] = Schema.decodeUnknownSync(Writes)(
    Array.flatMap(Record.toEntries(_TARGETS), ([section, keys]) => Array.map(Record.toEntries<string, Schema.Json>(keys), ([key, value]) => ({ section, key, value }))),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Section, TARGET_ROWS, Writes };
