// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Match, Option, Record, Schema, Struct } from 'effect';
import { type AbsolutePath, PageIndex } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Operation = (typeof Operation)['Type'];
type Save = Option.Option<AbsolutePath>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROFILE = { print: 'GRACoL2013_CRPC6.icc', digital: 'sRGB IEC61966-2.1' } as const;
const _PDF_UA = JSON.stringify('Verify compliance with PDF/UA-1 (syntax checks only)');
const _FIELD_JS =
    "var readField = function (f) { var row = { name: f.name, type: f.type, page: f.page, rect: f.rect }; var props = ['readonly', 'required', 'value', 'defaultValue', 'calcOrderIndex', 'charLimit', 'multiline', 'exportValues', 'textSize', 'textFont', 'strokeColor', 'fillColor', 'lineWidth', 'borderStyle', 'style', 'alignment', 'numItems']; for (var i = 0; i < props.length; i++) { try { row[props[i]] = f[props[i]]; } catch (e) {} } if (row.numItems !== undefined) { row.items = []; for (var j = 0; j < f.numItems; j++) { row.items.push({ label: f.getItemAt(j, false), export: f.getItemAt(j, true) }); } } return row; };";
const _MENU_JS =
    'var menu = []; var walk = function (items, depth) { for (var i = 0; i < items.length; i++) { menu.push({ cName: items[i].cName, depth: depth }); walk(items[i].oChildren, depth + 1); } }; walk(app.listMenuItems(), 0);';

// --- [MODELS] --------------------------------------------------------------------------

const _NON_PRINT = [0, 1, 2] as const;
const _optionalPage = Schema.OptionFromOptionalKey(PageIndex);
const _optionalNumber = Schema.OptionFromOptionalKey(Schema.Number);

const Color: Schema.$Array<Schema.Union<readonly [Schema.String, Schema.Number]>> = Schema.Array(Schema.Union([Schema.String, Schema.Number]));

const Operation: Schema.Union<
    readonly [
        Schema.Struct<{ readonly op: Schema.Literal<'colorConvertPage'>; readonly page: Schema.Int; readonly target: Schema.Literals<Array<keyof typeof _PROFILE>> }>,
        Schema.Struct<{ readonly op: Schema.Literal<'embedOutputIntent'>; readonly profile: Schema.String }>,
        Schema.Struct<{
            readonly op: Schema.Literal<'flattenPages'>;
            readonly nonPrint: Schema.Literals<typeof _NON_PRINT>;
            readonly start: Schema.OptionFromOptionalKey<Schema.Int>;
            readonly end: Schema.OptionFromOptionalKey<Schema.Int>;
        }>,
        Schema.Struct<{
            readonly op: Schema.Literal<'addWatermarkFromText'>;
            readonly text: Schema.String;
            readonly font: Schema.OptionFromOptionalKey<Schema.String>;
            readonly size: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly color: Schema.OptionFromOptionalKey<typeof Color>;
            readonly opacity: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly rotation: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly start: Schema.OptionFromOptionalKey<Schema.Int>;
            readonly end: Schema.OptionFromOptionalKey<Schema.Int>;
        }>,
        Schema.Struct<{ readonly op: Schema.Literal<'applyRedactions'> }>,
    ]
> = Schema.Union([
    Schema.Struct({ op: Schema.Literal('colorConvertPage'), page: PageIndex, target: Schema.Literals(Struct.keys(_PROFILE)) }),
    Schema.Struct({ op: Schema.Literal('embedOutputIntent'), profile: Schema.String }),
    Schema.Struct({ op: Schema.Literal('flattenPages'), nonPrint: Schema.Literals(_NON_PRINT), start: _optionalPage, end: _optionalPage }),
    Schema.Struct({
        op: Schema.Literal('addWatermarkFromText'),
        text: Schema.String,
        font: Schema.OptionFromOptionalKey(Schema.String),
        size: _optionalNumber,
        color: Schema.OptionFromOptionalKey(Color),
        opacity: _optionalNumber,
        rotation: _optionalNumber,
        start: _optionalPage,
        end: _optionalPage,
    }),
    Schema.Struct({ op: Schema.Literal('applyRedactions') }),
]);

// --- [BODIES] --------------------------------------------------------------------------

const _save: (save: Save) => string = Option.match({ onNone: () => '', onSome: (target: AbsolutePath) => `d.saveAs({ cPath: ${JSON.stringify(target)} });` });

const _operation = (operation: Operation): string =>
    Match.value(operation).pipe(
        Match.discriminatorsExhaustive('op')({
            colorConvertPage: ({ page, target }) =>
                `var a = d.getColorConvertAction(); a.matchAttributesAny = -1; a.matchSpaceTypeAny = ~a.constants.spaceFlags.AlternateSpace; a.matchIntent = a.constants.renderingIntents.Any; a.action = a.constants.actions.Convert; a.convertProfile = ${JSON.stringify(_PROFILE[target])}; a.embed = true; a.preserveBlack = true; a.useBlackPointCompensation = true; d.colorConvertPage(${page}, [a], []);`,
            embedOutputIntent: ({ profile }) => `d.embedOutputIntent(${JSON.stringify(profile)});`,
            flattenPages: ({ start, end, nonPrint }) => `d.flattenPages(${JSON.stringify(Record.getSomes<string, Schema.Json>({ nStart: start, nEnd: end, nNonPrint: Option.some(nonPrint) }))});`,
            addWatermarkFromText: ({ text, font, size, color, opacity, rotation, start, end }) =>
                `d.addWatermarkFromText(${JSON.stringify(Record.getSomes<string, Schema.Json>({ cText: Option.some(text), cFont: font, nFontSize: size, aColor: color, nOpacity: opacity, nRotation: rotation, nStart: start, nEnd: end }))});`,
            applyRedactions: () => 'd.applyRedactions();',
        }),
    );

const envelope = (code: string): string =>
    `(function () { try { var value = (function () { ${code} })(); return JSON.stringify({ _tag: 'Success', success: value === undefined ? null : value }); } catch (e) { return JSON.stringify({ _tag: 'Failure', failure: e._tag === undefined ? { _tag: 'scriptThrew', name: e.name, message: e.message, line: e.lineNumber, fileName: e.fileName } : e }); } })()`;

const withDocument = (device: string, document: AbsolutePath, code: string): string =>
    `var docs = app.activeDocs; var d; for (var n = 0; n < docs.length; n++) { if (docs[n].path === ${JSON.stringify(device)}) { d = docs[n]; } } if (d === undefined) { throw { _tag: 'documentNotOpen', path: ${JSON.stringify(document)} }; } ${code}`;

const state: string =
    "var docs = app.activeDocs; var rows = []; for (var i = 0; i < docs.length; i++) { rows.push({ path: docs[i].path, fileName: docs[i].documentFileName, numPages: docs[i].numPages, numFields: docs[i].numFields, dirty: docs[i].dirty }); } return { kind: 'state', viewerVersion: app.viewerVersion, viewerType: app.viewerType, language: app.language, platform: app.platform, documents: rows, converters: app.fromPDFConverters };";

const openDocument = (path: AbsolutePath, hidden: boolean): string => {
    const target = JSON.stringify(path);
    return `var d = app.openDoc({ cPath: ${target}, bHidden: ${hidden} }); return { kind: 'opened', path: ${target}, fileName: d.documentFileName, numPages: d.numPages };`;
};

const menu: string = `${_MENU_JS} return { kind: 'menu', items: menu };`;

const execMenuItem = (name: string, scoped: boolean): string => {
    const item = JSON.stringify(name);
    return `${_MENU_JS} var listed = false; for (var m = 0; m < menu.length; m++) { if (menu[m].cName === ${item}) { listed = true; } } if (!listed) { throw { _tag: 'menuItemNotListed', name: ${item} }; } app.execMenuItem(${item}${scoped ? ', d' : ''}); return { kind: 'executed', name: ${item} };`;
};

const getFields = (names: Option.Option<Array.NonEmptyReadonlyArray<string>>): string =>
    `${_FIELD_JS} var names = ${Option.match(names, { onNone: () => 'null', onSome: JSON.stringify })}; if (names === null) { names = []; for (var i = 0; i < d.numFields; i++) { names.push(d.getNthFieldName(i)); } } var fields = []; var absent = []; for (var j = 0; j < names.length; j++) { var f = d.getField(names[j]); if (f === null) { absent.push(names[j]); } else { fields.push(readField(f)); } } return { kind: 'fields', fields: fields, absent: absent };`;

const setFields = (specs: Schema.Json, save: Save): string =>
    `${_FIELD_JS} var applyFormat = function (f, fmt) { if (fmt.kind === 'plain') { if (fmt.charLimit !== undefined) { f.charLimit = fmt.charLimit; } f.comb = false; f.doNotSpellCheck = true; f.doNotScroll = false; f.richText = false; } if (fmt.kind === 'number' || fmt.kind === 'total') { f.setAction('Format', 'AFNumber_Format(' + fmt.decimals + ', 0, 0, 0, "", true);'); f.setAction('Keystroke', 'AFNumber_Keystroke(' + fmt.decimals + ', 0, 0, 0, "", true);'); f.alignment = 'right'; } if (fmt.kind === 'number' && fmt.range !== undefined) { f.setAction('Validate', 'AFRange_Validate(true, ' + fmt.range.min + ', true, ' + fmt.range.max + ');'); } if (fmt.kind === 'percent') { f.setAction('Format', 'AFPercent_Format(' + fmt.decimals + ', 0);'); f.setAction('Keystroke', 'AFPercent_Keystroke(' + fmt.decimals + ', 0);'); f.setAction('Validate', 'AFRange_Validate(true, 0, true, 100);'); f.alignment = 'right'; } if (fmt.kind === 'date') { f.setAction('Format', 'AFDate_FormatEx("yyyy-mm-dd");'); f.setAction('Keystroke', 'AFDate_KeystrokeEx("yyyy-mm-dd");'); } if (fmt.kind === 'time') { f.setAction('Format', 'AFTime_Format(0);'); f.setAction('Keystroke', 'AFTime_Keystroke(0);'); } if (fmt.kind === 'total') { f.setAction('Calculate', 'AFSimple_Calculate("SUM", new Array(' + fmt.operands.map(function (name) { return JSON.stringify(name); }).join(', ') + '));'); f.readonly = true; var top = -1; for (var k = 0; k < fmt.operands.length; k++) { var operand = d.getField(fmt.operands[k]); if (operand !== null && operand.calcOrderIndex > top) { top = operand.calcOrderIndex; } } f.calcOrderIndex = top + 1; } }; var specs = ${JSON.stringify(specs)}; var applied = []; var rejected = []; for (var i = 0; i < specs.length; i++) { var s = specs[i]; var f = d.getField(s.name); if (f === null && s.create === undefined) { rejected.push({ name: s.name, reason: { _tag: 'fieldAbsent' } }); continue; } var from = f === null ? null : readField(f); if (f === null) { f = d.addField(s.name, s.create.type, s.create.page, s.create.rect); } if (s.format !== undefined) { applyFormat(f, s.format); } if (f.type === 'checkbox') { f.exportValues = s.exportValues === undefined ? ['Yes'] : s.exportValues; f.style = 'check'; f.defaultIsChecked(0, false); } if (f.type === 'radiobutton') { f.style = 'circle'; f.radiosInUnison = false; if (s.exportValues !== undefined) { f.exportValues = s.exportValues; } } if (f.type === 'combobox' || f.type === 'listbox') { if (s.items !== undefined) { var rows = []; for (var k = 0; k < s.items.length; k++) { rows.push([s.items[k].label, s.items[k].export]); } f.setItems(rows); } f.commitOnSelChange = true; } if (f.type === 'combobox') { f.editable = false; } if (f.type === 'listbox') { f.multipleSelection = false; } if (f.type === 'button') { if (s.caption !== undefined) { f.buttonSetCaption(s.caption); } if (s.mouseUp !== undefined) { f.setAction('MouseUp', s.mouseUp === 'resetForm' ? 'this.resetForm();' : 'this.submitForm(' + JSON.stringify(s.mouseUp.submitForm) + ');'); } } if (s.readOnly !== undefined) { f.readonly = s.readOnly; } if (s.required !== undefined) { f.required = s.required; } if (s.defaultValue !== undefined) { f.defaultValue = s.defaultValue; } applied.push({ name: s.name, from: from }); } d.calculateNow(); for (var a = 0; a < applied.length; a++) { applied[a].to = readField(d.getField(applied[a].name)); } ${_save(save)} return { kind: 'fieldsApplied', applied: applied, rejected: rejected };`;

const tabOrder = (pages: Option.Option<Array.NonEmptyReadonlyArray<number>>, order: string, save: Save): string =>
    `var pages = ${Option.match(pages, { onNone: () => 'null', onSome: JSON.stringify })}; if (pages === null) { pages = []; for (var i = 0; i < d.numPages; i++) { pages.push(i); } } for (var j = 0; j < pages.length; j++) { d.setPageTabOrder(pages[j], ${JSON.stringify(order)}); } ${_save(save)} return { kind: 'tabOrder', applied: pages };`;

const autotag: string = `var p = Preflight.getProfileByName(${_PDF_UA}); if (p === undefined) { throw { _tag: 'profileAbsent', profile: ${_PDF_UA} }; } var before = d.preflight(p, true).numErrors; app.execMenuItem('Adobe:MakeAccessible', d); return { before: before };`;

const tagged = (before: number, save: Save): string =>
    `var after = d.preflight(Preflight.getProfileByName(${_PDF_UA}), true).numErrors; var dirty = d.dirty; ${_save(save)} return { kind: 'tagged', numErrors: { before: ${before}, after: after }, dirty: dirty };`;

const preflight = (method: 'getProfileByName' | 'createComplianceProfile', profile: string, fixups: boolean, report: boolean, save: Save): string => {
    const label = JSON.stringify(profile);
    return `var p = Preflight.${method}(${label}); if (p === undefined) { throw { _tag: 'profileAbsent', profile: ${label} }; } var r = d.preflight(p, ${!fixups}); var out = { numErrors: r.numErrors, numWarnings: r.numWarnings, numInfos: r.numInfos, numFixed: r.numFixed, numNotFixed: r.numNotFixed }; ${report ? 'out.report = r.report();' : ''} ${_save(save)} return out;`;
};

const printProduction = (operations: readonly Operation[], save: Save): string =>
    `var ops = [${Array.join(
        Array.map(operations, (operation) => `function () { ${_operation(operation)} }`),
        ', ',
    )}]; var applied = []; var rejected = []; for (var i = 0; i < ops.length; i++) { try { ops[i](); applied.push(i); } catch (e) { rejected.push({ opIndex: i, reason: { name: e.name, message: e.message } }); } } ${_save(save)} return { kind: 'production', applied: applied, rejected: rejected };`;

const combine = (sources: Schema.Json, output: AbsolutePath): string => {
    const target = JSON.stringify(output);
    return `var sources = ${JSON.stringify(sources)}; var d = app.newDoc(); var skipped = []; var marks = []; for (var i = 0; i < sources.length; i++) { var s = sources[i]; var first = d.numPages - 1; var args = { nPage: d.numPages - 1, cPath: s.path }; if (s.start !== undefined) { args.nStart = s.start; args.nEnd = s.end; } try { d.insertPages(args); marks.push({ label: s.label, page: first }); } catch (e) { skipped.push({ path: s.path, reason: { _tag: 'insertRefused', message: e.message } }); } } if (marks.length === 0) { d.closeDoc(true); return { kind: 'nothingInserted', skipped: skipped }; } d.deletePages(0); var count = 0; for (var j = 0; j < marks.length; j++) { if (marks[j].label !== undefined) { d.bookmarkRoot.createChild(marks[j].label, 'this.pageNum = ' + marks[j].page, count); count++; } } d.saveAs({ cPath: ${target} }); var numPages = d.numPages; d.closeDoc(true); return { kind: 'combined', path: ${target}, numPages: numPages, bookmarks: count, skipped: skipped };`;
};

const perFile = (source: AbsolutePath, body: string): string =>
    `var d = app.openDoc({ cPath: ${JSON.stringify(source)}, bHidden: true }); try { ${body} } finally { d.closeDoc(true); } return { saved: true };`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { autotag, Color, combine, envelope, execMenuItem, getFields, menu, Operation, openDocument, perFile, preflight, printProduction, setFields, state, tabOrder, tagged, withDocument };
