// Read-only probe of application-level preferences. Writes JSON to the scratchpad.
#target "indesign"
app.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;

var OUT = "/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/indesign/";

// ---- enumeration reverse map -------------------------------------------------
var ENUM_CLASSES = ["MeasurementUnits","RulerOrigin","HorizontalOrVertical","PageOrientation","PagePositions",
"DocumentIntentOptions","BlendMode","BlendingSpace","BaselineGridRelativeOption","Justification","SingleWordJustification",
"FirstBaseline","VerticalJustification","AlignmentStyleOptions","LeadingModel","DropCapStyleOptions","RuleWidth",
"ListType","CharacterAlignment","Position","Capitalization","StrokeAlignment","PageNumberStyle","TabStopAlignType",
"StartParagraph","SpanColumnTypeOptions","SpanColumnCountOptions","OTFFigureStyle","PositionalForms","ColorModel",
"ColorSpace","InkType","ImageTypes","GradientType","AutoSizingTypeEnum","AutoSizingReferenceEnum","AnchorPoint",
"AnchoredRelativeTo","AnchorPosition","EmptyFrameFittingOptions","FitOptions","StoryDirectionOptions",
"StoryHorizontalOrVertical","DisplaySettingOptions","ViewDisplaySettings","RenderingIntent","ColorSettingsPolicy",
"FontStatus","TextStrokeAlign","TableDirections","HeaderFooterBreakTypes","CellTypeEnum","PDFXStandards","AcrobatCompatibility",
"ColorOutputModes","PDFColorSpace","PDFProfileSelector","PDFCrop","CropOptions","GuideTypeOptions","PasteboardOrientation",
"KashidasOptions","DiacriticPositionOptions","ParagraphDirectionOptions","ParagraphJustificationOptions","DigitsTypeOptions",
"BookContentStatus","ChangeConditionsModes","UserInteractionLevels","UndoModes","Sampling","CompressionType","VersionState",
"TextTypeAlignments","LinkStatus","ImportPlatform","ImageAlpha","GlyphForm","SpecialCharacters","OpenOptions","SaveOptions",
"CharacterDirectionOptions","KentenCharacter","RubyTypes","WhenScrollingOptions","ClipboardPreferencesPDFTypes",
"BaselineFrameGridRelativeOption","NothingEnum","AutoEnum","DisplayPerformanceOptions","GreekingTypeOptions",
"RulerUnits","TransparencyFlattenerLevel","ColorGroupType","NumberingStyle","BulletCharacterType","TextFrameContents",
"XMLElementLocation","LanguageState","PreflightProfileRuleType","PreflightStatus","OverprintModes","ParagraphShadingWidthType",
"ParagraphShadingClipOptions","ScriptLanguage","UndoModes","AlternateLayoutTypes","LayoutRuleOptions","LiquidPageRuleOptions",
"BalanceLinesStyle","LeadingType","TextExportCharacterSet","XFLRasterizeFormat","PDFMarkWeight","PageRange","Trapping",
"FlattenerLevel","ColorBar","PageInformationMarks"];

var ENUMMAP = {};
for (var ei = 0; ei < ENUM_CLASSES.length; ei++) {
  var cls = ENUM_CLASSES[ei];
  try {
    var obj = eval(cls);
    if (!obj || !obj.reflect) continue;
    var rp = obj.reflect.properties;
    for (var pi = 0; pi < rp.length; pi++) {
      var pn = rp[pi].name;
      if (pn === "__proto__" || pn === "reflect" || pn === "constructor") continue;
      try {
        var v = obj[pn];
        var num = Number(v);
        if (!isNaN(num) && num !== 0) { if (!ENUMMAP[num]) ENUMMAP[num] = cls + "." + pn; }
      } catch (e1) {}
    }
  } catch (e2) {}
}

function fourCC(n) {
  if (n < 0x20202020 || n > 0x7e7e7e7e) return null;
  var s = "";
  for (var i = 3; i >= 0; i--) {
    var c = (n >> (i * 8)) & 0xff;
    if (c < 32 || c > 126) return null;
    s += String.fromCharCode(c);
  }
  return s;
}

// ---- JSON writer -------------------------------------------------------------
function esc(s) {
  s = String(s);
  var o = "";
  for (var i = 0; i < s.length; i++) {
    var c = s.charAt(i), cc = s.charCodeAt(i);
    if (c === '"') o += '\\"';
    else if (c === "\\") o += "\\\\";
    else if (cc === 10) o += "\\n";
    else if (cc === 13) o += "\\r";
    else if (cc === 9) o += "\\t";
    else if (cc < 32 || cc > 126) { var h = cc.toString(16); while (h.length < 4) h = "0" + h; o += "\\u" + h; }
    else o += c;
  }
  return '"' + o + '"';
}

function ser(v, depth) {
  depth = depth || 0;
  if (v === null || v === undefined) return "null";
  var t = typeof v;
  if (t === "boolean") return v ? "true" : "false";
  if (t === "number") { if (isNaN(v) || !isFinite(v)) return esc(String(v)); return String(v); }
  if (t === "string") return esc(v);
  if (v instanceof Array) {
    var a = [];
    for (var i = 0; i < v.length; i++) a.push(ser(v[i], depth + 1));
    return "[" + a.join(",") + "]";
  }
  // Enumerator
  var ctor = "";
  try { ctor = v.constructor && v.constructor.name ? v.constructor.name : ""; } catch (e0) {}
  if (ctor === "Enumerator" || (t === "object" && v.reflect && v.reflect.name === "Enumerator")) {
    var n = Number(v);
    var nm = ENUMMAP[n];
    if (nm) return esc(nm);
    var f = fourCC(n);
    return esc("<enum " + n + (f ? " '" + f + "'" : "") + ">");
  }
  if (ctor === "File" || ctor === "Folder") { try { return esc(ctor + ":" + v.fsName); } catch (e) { return esc(ctor); } }
  if (ctor === "UnitValue") { return esc(String(v)); }
  if (ctor === "Date") { return esc(String(v)); }
  if (t === "object") {
    if (depth > 6) return esc("<deep>");
    // plain object from our own code
    if (ctor === "Object") {
      var parts = [];
      for (var k in v) { if (!v.hasOwnProperty(k)) continue; parts.push(esc(k) + ":" + ser(v[k], depth + 1)); }
      return "{" + parts.join(",") + "}";
    }
    // InDesign DOM object: summarize
    var lbl = ctor || "object";
    var nm2 = null;
    try { nm2 = v.name; } catch (e3) {}
    if (nm2 !== null && nm2 !== undefined) return esc("<" + lbl + " " + nm2 + ">");
    try { return esc("<" + lbl + " " + String(v) + ">"); } catch (e4) { return esc("<" + lbl + ">"); }
  }
  return esc(String(v));
}

function props(o) {
  var r = {};
  var p;
  try { p = o.properties; } catch (e) { return { "__error": String(e) }; }
  for (var k in p) {
    if (k === "parent" || k === "index" || k === "id" || k === "__proto__") { }
    try { r[k] = p[k]; } catch (e2) { r[k] = "<error:" + e2 + ">"; }
  }
  return r;
}

function safe(fn, label) {
  try { return fn(); } catch (e) { return "<unavailable: " + label + ": " + e + ">"; }
}

// ---- application preferences -------------------------------------------------
var A = {};
A.__meta = {
  name: app.name, version: app.version, fullName: app.fullName.fsName,
  locale: String(app.locale), serialNumber: safe(function () { return app.serialNumber; }, "sn"),
  documentsOpen: app.documents.length,
  probedAt: String(new Date())
};

var PREF_OBJECTS = ["generalPreferences","displayPerformancePreferences","viewPreferences","textPreferences",
"textEditingPreferences","clipboardPreferences","guidePreferences","gridPreferences","marginPreferences",
"documentPreferences","textFramePreferences","storyPreferences","pasteboardPreferences","transformPreferences",
"dictionaryPreferences","linkingPreferences","imagePreferences","colorSettings","autoCorrectPreferences",
"spellPreferences","scriptPreferences","transparencyPreferences","printPreferences","pdfExportPreferences",
"documentPreset","layoutAdjustmentPreferences","galleyPreferences","storyWindowPreferences","trackChangesPreferences",
"footnoteOptions","endnoteOptions","tocStyles","contentPlacerPreferences","smartGuidePreferences","toolsPreferences",
"panelPreferences","notePreferences","xmlPreferences","xmlViewPreferences","excelImportPreferences",
"taggedTextExportPreferences","taggedTextImportPreferences","textImportPreferences","wordRTFImportPreferences",
"objectStyleOptions","anchoredObjectSettings","findChangeGrepOptions","findChangeTextOptions","findChangeObjectOptions",
"changeGrepPreferences","findGrepPreferences","changeTextPreferences","findTextPreferences","typePreferences",
"fontLockingPreferences","interactivePDFExportPreferences","dataMergeOptions","indexingSortOptions","jpegExportPreferences",
"pngExportPreferences","imageIOPreferences","metadataPreferences","polygonPreferences","textDefault","strokeFillProxySettings",
"transparencyDefaultContainerObject","alignDistributePreferences","baselineFrameGridOptions","conditionalTextPreferences",
"dashedStrokeOptions","dottedStrokeOptions","stripedStrokeOptions","gridDataInformation","layoutGridData",
"mojikumiUiPreferences","kinsokuTable","trapPreset","watermarkPreferences","htmlFXLExportPreferences",
"ePubFixedLayoutExportPreferences","displaySettings","eventListeners"];

A.preferences = {};
for (var pj = 0; pj < PREF_OBJECTS.length; pj++) {
  var key = PREF_OBJECTS[pj];
  try {
    var o = app[key];
    if (o === undefined) { A.preferences[key] = "<API lacks this property on app>"; continue; }
    if (o === null) { A.preferences[key] = null; continue; }
    if (o.constructor && o.constructor.name === "Array") { A.preferences[key] = "<collection length " + o.length + ">"; continue; }
    if (typeof o.properties === "undefined") { A.preferences[key] = ser(o, 0); continue; }
    A.preferences[key] = props(o);
  } catch (e) {
    A.preferences[key] = "<API lacks this property on app or raised: " + e + ">";
  }
}

// documentPresets
A.documentPresets = safe(function () {
  var out = [];
  for (var i = 0; i < app.documentPresets.length; i++) {
    var d = app.documentPresets[i];
    out.push(props(d));
  }
  return out;
}, "documentPresets");

A.pdfExportPresets = safe(function () {
  var out = [];
  for (var i = 0; i < app.pdfExportPresets.length; i++) out.push(app.pdfExportPresets[i].name);
  return out;
}, "pdfExportPresets");

A.printerPresets = safe(function () {
  var out = [];
  for (var i = 0; i < app.printerPresets.length; i++) out.push(app.printerPresets[i].name);
  return out;
}, "printerPresets");

A.preflightProfiles = safe(function () {
  var out = [];
  for (var i = 0; i < app.preflightProfiles.length; i++) {
    var p = app.preflightProfiles[i];
    var rules = [];
    try {
      for (var r = 0; r < p.preflightProfileRules.length; r++) {
        var rr = p.preflightProfileRules[r];
        var rd = { id: rr.id, name: safe(function () { return rr.name; }, "n"), flag: String(rr.flag) };
        try {
          var dl = rr.ruleDataObjects;
          var d2 = [];
          for (var q = 0; q < dl.length; q++) d2.push({ name: dl[q].name, dataType: String(dl[q].dataType), value: dl[q].dataValue });
          rd.data = d2;
        } catch (e5) {}
        rules.push(rd);
      }
    } catch (e6) { rules = "<rules unavailable: " + e6 + ">"; }
    out.push({ name: p.name, rules: rules });
  }
  return out;
}, "preflightProfiles");

A.menuActionsCount = safe(function () { return app.menuActions.length; }, "menuActions");
A.menusCount = safe(function () { return app.menus.length; }, "menus");
A.scriptMenuActionsCount = safe(function () { return app.scriptMenuActions.length; }, "scriptMenuActions");
A.keyboardShortcutSets = safe(function () { return app.keyboardShortcutSets; }, "keyboardShortcutSets");
A.findChangeQueriesProperty = safe(function () { return app.findChangeQueries; }, "findChangeQueries");
A.activeWorkspace = safe(function () { return app.activeWorkspace ? app.activeWorkspace.name : null; }, "activeWorkspace");
A.workspaces = safe(function () { var o = []; for (var i = 0; i < app.workspaces.length; i++) o.push(app.workspaces[i].name); return o; }, "workspaces");
A.textVariablesApp = safe(function () { var o = []; for (var i = 0; i < app.textVariables.length; i++) o.push({ name: app.textVariables[i].name, type: String(app.textVariables[i].variableType) }); return o; }, "textVariables");
A.appParagraphStyles = safe(function () { var o = []; for (var i = 0; i < app.paragraphStyles.length; i++) o.push(app.paragraphStyles[i].name); return o; }, "paragraphStyles");
A.appCharacterStyles = safe(function () { var o = []; for (var i = 0; i < app.characterStyles.length; i++) o.push(app.characterStyles[i].name); return o; }, "characterStyles");
A.appObjectStyles = safe(function () { var o = []; for (var i = 0; i < app.objectStyles.length; i++) o.push(app.objectStyles[i].name); return o; }, "objectStyles");
A.appSwatches = safe(function () { var o = []; for (var i = 0; i < app.swatches.length; i++) { var s = app.swatches[i]; o.push({ name: s.name, model: String(s.model), space: safe(function () { return String(s.space); }, "sp"), value: safe(function () { return s.colorValue; }, "cv") }); } return o; }, "swatches");
A.appLanguages = safe(function () { var o = []; for (var i = 0; i < app.languagesWithVendors.length; i++) o.push(app.languagesWithVendors[i].name); return o; }, "languages");
A.appDictionaries = safe(function () { return app.userDictionaries ? app.userDictionaries.length : null; }, "userDictionaries");

// composer probe (read only)
A.composerProbe = safe(function () {
  var r = {};
  r.appStoryComposer = app.storyPreferences.composer;
  r.appTextDefaultComposer = safe(function () { return app.textDefaults.composer; }, "textDefaults");
  r.appParagraphStyleZeroComposer = safe(function () { return app.paragraphStyles[0].composer; }, "ps0");
  r.composerTypeof = typeof app.storyPreferences.composer;
  r.hasComposerEnum = safe(function () { return typeof ComposerType; }, "ComposerType");
  // installed composer names via the fully qualified enum-ish list is not exposed; report what the DOM gives
  r.appStoryPrefsAll = props(app.storyPreferences);
  return r;
}, "composerProbe");

// fonts installed count and a sample of the families the user has
A.fonts = safe(function () {
  var o = { count: app.fonts.length, sample: [] };
  var lim = app.fonts.length < 4000 ? app.fonts.length : 4000;
  var seen = {};
  for (var i = 0; i < lim; i++) {
    try {
      var f = app.fonts[i];
      var fam = f.fontFamily;
      if (!seen[fam]) { seen[fam] = 1; o.sample.push(fam); }
    } catch (e) {}
  }
  return o;
}, "fonts");

var fh = new File(OUT + "app-preferences.json");
fh.encoding = "UTF-8";
fh.open("w");
fh.write(ser(A, 0));
fh.close();

"WROTE " + fh.fsName + " enumMapSize=" + (function () { var c = 0; for (var k in ENUMMAP) c++; return c; })();
