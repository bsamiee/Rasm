// Read-only document probe. Opens each target without a window, dumps JSON, closes without saving.
#target "indesign"
app.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;

var OUT = "/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/indesign/";
var GD = "/Users/bardiasamiee/Library/CloudStorage/GoogleDrive-b.samiee@mzn-group.com/My Drive/03.Digital Asset Database/";

var TARGETS = [
  ["default-template", GD + "05.Software Related Assets/99.Default Profiles/InDesign/Default Template.indt"],
  ["school-document-template", GD + "98.Templates/02.Documentation Templates/99.School & Personal Templates/School Document Template.indd"],
  ["kelman-us-letter-portrait-grid", GD + "98.Templates/02.Documentation Templates/00.General/Report-Grids/US Letter Portrait Grid System.indt"],
  ["kelman-digital-presentation-landscape-grid-v2", GD + "98.Templates/02.Documentation Templates/00.General/Presentation-Grids/Digital Presentation Landscape Grid System v2.indd"]
];

// ---------------- enum reverse map ----------------
var ENUM_CLASSES = ["MeasurementUnits","RulerOrigin","HorizontalOrVertical","PageOrientation","PagePositions",
"DocumentIntentOptions","BlendMode","BlendingSpace","BaselineGridRelativeOption","Justification","SingleWordJustification",
"FirstBaseline","VerticalJustification","AlignmentStyleOptions","LeadingModel","DropCapStyleOptions","RuleWidth",
"ListType","CharacterAlignment","Position","Capitalization","StrokeAlignment","PageNumberStyle","TabStopAlignType",
"StartParagraph","SpanColumnTypeOptions","SpanColumnCountOptions","OTFFigureStyle","PositionalForms","ColorModel",
"ColorSpace","InkType","ImageTypes","GradientType","AutoSizingTypeEnum","AutoSizingReferenceEnum","AnchorPoint",
"AnchoredRelativeTo","AnchorPosition","EmptyFrameFittingOptions","FitOptions","StoryDirectionOptions",
"StoryHorizontalOrVertical","DisplaySettingOptions","ViewDisplaySettings","RenderingIntent","ColorSettingsPolicy",
"FontStatus","TextStrokeAlign","TableDirections","HeaderFooterBreakTypes","CellTypeEnum","GuideTypeOptions",
"PasteboardOrientation","KashidasOptions","DiacriticPositionOptions","ParagraphDirectionOptions",
"ParagraphJustificationOptions","DigitsTypeOptions","ChangeConditionsModes","UserInteractionLevels","UndoModes",
"NothingEnum","AutoEnum","NumberingStyle","BulletCharacterType","TextFrameContents","OverprintModes",
"ParagraphShadingWidthType","ParagraphShadingClipOptions","AlternateLayoutTypes","LayoutRuleOptions",
"LiquidPageRuleOptions","BalanceLinesStyle","LeadingType","CharacterDirectionOptions","RubyTypes","KentenCharacter",
"ConditionMarkerWidth","ConditionIndicatorMethod","ConditionIndicatorMode","CrossReferenceType","HyperlinkAppearanceHighlight",
"HyperlinkAppearanceWidth","HyperlinkAppearanceStyle","XMLElementLocation","LanguageState","SaveOptions","OpenOptions",
"TabStopAlignment","RuleDataType","PreflightProfileRuleFlag","VerticalAlignment","EndnoteRestartNumbering",
"FootnoteRestarting","FootnoteMarkerPositioning","FootnoteNumberingStyle","FootnotePrefixSuffix","TableStyleOverridesPreferences",
"CornerOptions","EndCap","EndJoin","ArrowHead","StrokeCornerAdjustment","ImageTypes","LinkStatus","GlyphForm",
"TextExportCharacterSet","PageNumberPosition","PageNumberingOptions","SpecialCharacters"];

var ENUMMAP = {};
for (var ei = 0; ei < ENUM_CLASSES.length; ei++) {
  try {
    var cls = ENUM_CLASSES[ei], obj = eval(cls);
    if (!obj || !obj.reflect) continue;
    var rp = obj.reflect.properties;
    for (var pi = 0; pi < rp.length; pi++) {
      var pn = rp[pi].name;
      if (pn === "__proto__" || pn === "reflect" || pn === "constructor") continue;
      try { var num = Number(obj[pn]); if (!isNaN(num) && num !== 0 && !ENUMMAP[num]) ENUMMAP[num] = cls + "." + pn; } catch (e) {}
    }
  } catch (e) {}
}
function fourCC(n) {
  if (n < 0x20202020 || n > 0x7e7e7e7e) return null;
  var s = "";
  for (var i = 3; i >= 0; i--) { var c = (n >> (i * 8)) & 0xff; if (c < 32 || c > 126) return null; s += String.fromCharCode(c); }
  return s;
}

// ---------------- serializer ----------------
function esc(s) {
  s = String(s); var o = "";
  for (var i = 0; i < s.length; i++) {
    var c = s.charAt(i), cc = s.charCodeAt(i);
    if (c === '"') o += '\\"';
    else if (c === "\\") o += "\\\\";
    else if (cc === 10) o += "\\n"; else if (cc === 13) o += "\\r"; else if (cc === 9) o += "\\t";
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
  if (v instanceof Array) { var a = []; for (var i = 0; i < v.length; i++) a.push(ser(v[i], depth + 1)); return "[" + a.join(",") + "]"; }
  var ctor = ""; try { ctor = v.constructor && v.constructor.name ? v.constructor.name : ""; } catch (e0) {}
  if (ctor === "Enumerator") {
    var n = Number(v), nm = ENUMMAP[n];
    if (nm) return esc(nm);
    var f = fourCC(n); return esc("<enum " + n + (f ? " '" + f + "'" : "") + ">");
  }
  if (ctor === "File" || ctor === "Folder") { try { return esc(ctor + ":" + v.fsName); } catch (e) { return esc(ctor); } }
  if (ctor === "Font") { try { return esc("Font:" + v.fontFamily + " | " + v.fontStyleName + " | status " + String(ENUMMAP[Number(v.status)] || v.status)); } catch (e) { return esc("Font:?"); } }
  if (ctor === "UnitValue" || ctor === "Date") return esc(String(v));
  if (t === "object") {
    if (depth > 8) return esc("<deep>");
    if (ctor === "Object") {
      var parts = [];
      for (var k in v) { if (!v.hasOwnProperty(k)) continue; parts.push(esc(k) + ":" + ser(v[k], depth + 1)); }
      return "{" + parts.join(",") + "}";
    }
    var nm2 = null; try { nm2 = v.name; } catch (e3) {}
    if (nm2 !== null && nm2 !== undefined) return esc("<" + (ctor || "object") + " " + nm2 + ">");
    try { return esc("<" + (ctor || "object") + " " + String(v) + ">"); } catch (e4) { return esc("<" + ctor + ">"); }
  }
  return esc(String(v));
}
function P(o) {
  var r = {}; var p;
  try { p = o.properties; } catch (e) { return { "__error": String(e) }; }
  for (var k in p) { try { r[k] = p[k]; } catch (e2) { r[k] = "<error:" + e2 + ">"; } }
  return r;
}
function S(fn) { try { var v = fn(); return v === undefined ? "<undefined>" : v; } catch (e) { return "<err: " + e + ">"; } }

// ---------------- collectors ----------------
function styleGroupPath(st) {
  var path = [], p = null;
  try { p = st.parent; } catch (e) { return ""; }
  while (p && p.constructor && (p.constructor.name === "ParagraphStyleGroup" || p.constructor.name === "CharacterStyleGroup" ||
         p.constructor.name === "ObjectStyleGroup" || p.constructor.name === "TableStyleGroup" || p.constructor.name === "CellStyleGroup")) {
    path.unshift(p.name);
    try { p = p.parent; } catch (e) { break; }
  }
  return path.join(" / ");
}

function paraStyle(st) {
  var o = P(st);
  o.__group = styleGroupPath(st);
  o.__basedOn = S(function () { return st.basedOn ? st.basedOn.name : null; });
  o.__nextStyle = S(function () { return st.nextStyle ? st.nextStyle.name : null; });
  o.__appliedFont = S(function () { var f = st.appliedFont; return (typeof f === "string") ? f : (f.fontFamily + " | " + f.fontStyleName); });
  o.__tabStops = S(function () {
    var a = [];
    for (var i = 0; i < st.tabStops.length; i++) {
      var ts = st.tabStops[i];
      a.push({ position: ts.position, alignment: String(ENUMMAP[Number(ts.alignment)] || ts.alignment), leader: ts.leader, alignmentCharacter: ts.alignmentCharacter });
    }
    return a;
  });
  o.__nestedStyles = S(function () {
    var a = [];
    for (var i = 0; i < st.nestedStyles.length; i++) {
      var n = st.nestedStyles[i];
      a.push({ appliedCharacterStyle: S(function () { return n.appliedCharacterStyle.name; }), delimiter: String(n.delimiter), inclusive: n.inclusive, repetition: n.repetition });
    }
    return a;
  });
  o.__nestedGrepStyles = S(function () {
    var a = [];
    for (var i = 0; i < st.nestedGrepStyles.length; i++) {
      var n = st.nestedGrepStyles[i];
      a.push({ appliedCharacterStyle: S(function () { return n.appliedCharacterStyle.name; }), grepExpression: n.grepExpression });
    }
    return a;
  });
  o.__nestedLineStyles = S(function () {
    var a = [];
    for (var i = 0; i < st.nestedLineStyles.length; i++) {
      var n = st.nestedLineStyles[i];
      a.push({ appliedCharacterStyle: S(function () { return n.appliedCharacterStyle.name; }), lineCount: n.lineCount });
    }
    return a;
  });
  o.__bullet = S(function () { return { bulletCharacter: String(st.bulletChar.characterType) + ":" + st.bulletChar.character, font: S(function () { return st.bulletsCharacterStyle ? st.bulletsCharacterStyle.name : null; }) }; });
  o.__numbering = S(function () { return { format: st.numberingFormat, expression: st.numberingExpression, level: st.numberingLevel, startAt: st.numberingStartAt, continueNumbering: st.numberingContinue, list: S(function () { return st.numberingList ? st.numberingList.name : null; }) }; });
  return o;
}

function charStyleNonInherited(st) {
  var raw = P(st);
  var o = { __name: st.name, __group: styleGroupPath(st), __basedOn: S(function () { return st.basedOn ? st.basedOn.name : null; }) };
  var set = {}, unset = [];
  for (var k in raw) {
    var v = raw[k];
    var isNothing = false;
    try { if (v !== null && typeof v === "object" && v.constructor && v.constructor.name === "Enumerator" && Number(v) === Number(NothingEnum.NOTHING)) isNothing = true; } catch (e) {}
    if (isNothing) unset.push(k); else set[k] = v;
  }
  o.__set = set;
  o.__unsetCount = unset.length;
  o.__appliedFont = S(function () { var f = st.appliedFont; return (typeof f === "string") ? f : (f.fontFamily + " | " + f.fontStyleName); });
  return o;
}

function objStyle(st) {
  var o = P(st);
  o.__group = styleGroupPath(st);
  o.__basedOn = S(function () { return st.basedOn ? st.basedOn.name : null; });
  o.__enabledCategories = S(function () {
    return {
      fill: st.enableFill, stroke: st.enableStroke, strokeAndCornerOptions: st.enableStrokeAndCornerOptions,
      paragraphStyle: st.enableParagraphStyle, textFrameGeneralOptions: st.enableTextFrameGeneralOptions,
      textFrameBaselineOptions: st.enableTextFrameBaselineOptions, textFrameAutoSizingOptions: st.enableTextFrameAutoSizingOptions,
      textFrameFootnoteOptions: st.enableTextFrameFootnoteOptions, storyOptions: st.enableStoryOptions,
      textWrapAndOthers: st.enableTextWrapAndOthers, anchoredObjectOptions: st.enableAnchoredObjectOptions,
      frameFittingOptions: st.enableFrameFittingOptions, objectExportOptions: st.enableExportTagging,
      transparency: st.enableTransparency, dropShadow: st.enableDropShadow, feather: st.enableFeather,
      innerShadow: st.enableInnerShadow, outerGlow: st.enableOuterGlow, innerGlow: st.enableInnerGlow,
      bevelEmboss: st.enableBevelEmboss, satin: st.enableSatin, directionalFeather: st.enableDirectionalFeather,
      gradientFeather: st.enableGradientFeather, fillTransparency: st.enableFillTransparency,
      strokeTransparency: st.enableStrokeTransparency, contentTransparency: st.enableContentTransparency
    };
  });
  o.__textFramePreferences = S(function () { return P(st.textFramePreferences); });
  o.__frameFittingOptions = S(function () { return P(st.frameFittingOptions); });
  o.__anchoredObjectSettings = S(function () { return P(st.anchoredObjectSettings); });
  o.__textWrapPreferences = S(function () { return P(st.textWrapPreferences); });
  o.__baselineFrameGridOptions = S(function () { return P(st.baselineFrameGridOptions); });
  o.__storyPreferences = S(function () { return P(st.storyPreferences); });
  o.__transparencySettings = S(function () { return P(st.transparencySettings.blendingSettings); });
  return o;
}

function guideList(container) {
  var a = [];
  try {
    for (var i = 0; i < container.guides.length; i++) {
      var g = container.guides[i];
      a.push({ orientation: String(ENUMMAP[Number(g.orientation)] || g.orientation), location: g.location, layer: S(function () { return g.itemLayer.name; }), viewThreshold: g.viewThreshold, locked: g.locked, color: S(function () { return String(g.guideColor); }) });
    }
  } catch (e) { return "<err: " + e + ">"; }
  return a;
}

function pageItemSummary(container, limit) {
  var a = [];
  try {
    var n = container.pageItems.length;
    for (var i = 0; i < n && i < limit; i++) {
      var it = container.pageItems[i];
      a.push({
        kind: S(function () { return it.constructor.name; }),
        name: S(function () { return it.name; }),
        label: S(function () { return it.label; }),
        bounds: S(function () { return it.geometricBounds.join(", "); }),
        layer: S(function () { return it.itemLayer.name; }),
        objectStyle: S(function () { return it.appliedObjectStyle.name; }),
        contents: S(function () { return it.constructor.name === "TextFrame" ? String(it.contents).substr(0, 160) : null; })
      });
    }
    return { count: n, items: a };
  } catch (e) { return "<err: " + e + ">"; }
}

function dumpDoc(doc) {
  var D = {};
  D.meta = {
    name: doc.name, saved: doc.saved, modified: doc.modified,
    fullName: S(function () { return doc.fullName.fsName; }),
    converted: S(function () { return doc.converted; }),
    readOnly: S(function () { return doc.readOnly; })
  };
  D.documentPreferences = P(doc.documentPreferences);
  D.marginPreferences = P(doc.marginPreferences);
  D.gridPreferences = P(doc.gridPreferences);
  D.guidePreferences = P(doc.guidePreferences);
  D.viewPreferences = P(doc.viewPreferences);
  D.textPreferences = S(function () { return P(doc.textPreferences); });
  D.storyPreferences = P(doc.storyPreferences);
  D.pasteboardPreferences = S(function () { return P(doc.pasteboardPreferences); });
  D.transparencyPreferences = S(function () { return P(doc.transparencyPreferences); });
  D.transparencySettings = S(function () { return { blendingSpace: String(ENUMMAP[Number(doc.transparencyPreferences.blendingSpace)] || doc.transparencyPreferences.blendingSpace) }; });
  D.colorProfiles = S(function () {
    return {
      cmykProfile: doc.cmykProfile, rgbProfile: doc.rgbProfile,
      cmykPolicy: String(ENUMMAP[Number(doc.cmykPolicy)] || doc.cmykPolicy),
      rgbPolicy: String(ENUMMAP[Number(doc.rgbPolicy)] || doc.rgbPolicy),
      solidColorIntent: String(ENUMMAP[Number(doc.solidColorIntent)] || doc.solidColorIntent),
      defaultImageIntent: String(ENUMMAP[Number(doc.defaultImageIntent)] || doc.defaultImageIntent),
      afterBlendingIntent: String(ENUMMAP[Number(doc.afterBlendingIntent)] || doc.afterBlendingIntent),
      cmykProfileList: S(function () { return doc.cmykProfileList.length; }),
      zoneBasedScreening: S(function () { return doc.zoneBasedScreening; })
    };
  });
  D.baselineGrid = S(function () {
    var g = doc.gridPreferences;
    return { baselineStart: g.baselineStart, baselineDivision: g.baselineDivision, baselineGridShown: g.baselineGridShown,
             baselineGridRelativeOption: String(ENUMMAP[Number(g.baselineGridRelativeOption)] || g.baselineGridRelativeOption),
             baselineViewThreshold: g.baselineViewThreshold,
             documentGridShown: g.documentGridShown, horizontalGridlineDivision: g.horizontalGridlineDivision,
             horizontalGridSubdivision: g.horizontalGridSubdivision, verticalGridlineDivision: g.verticalGridlineDivision,
             verticalGridSubdivision: g.verticalGridSubdivision, gridsInBack: g.gridsInBack };
  });

  D.layers = S(function () {
    var a = [];
    for (var i = 0; i < doc.layers.length; i++) {
      var l = doc.layers[i];
      a.push({ index: i, name: l.name, visible: l.visible, locked: l.locked, printable: l.printable,
               showGuides: l.showGuides, lockGuides: l.lockGuides, ignoreWrap: l.ignoreWrap,
               color: String(ENUMMAP[Number(l.layerColor)] || l.layerColor),
               pageItems: S(function () { return l.pageItems.length; }) });
    }
    return a;
  });

  D.masterSpreads = S(function () {
    var a = [];
    for (var i = 0; i < doc.masterSpreads.length; i++) {
      var m = doc.masterSpreads[i];
      var pages = [];
      for (var p = 0; p < m.pages.length; p++) {
        var pg = m.pages[p];
        pages.push({ name: pg.name, side: String(ENUMMAP[Number(pg.side)] || pg.side),
                     bounds: S(function () { return pg.bounds.join(", "); }),
                     margins: S(function () { return P(pg.marginPreferences); }),
                     guides: guideList(pg), items: pageItemSummary(pg, 40) });
      }
      a.push({ name: m.name, baseName: m.baseName, namePrefix: m.namePrefix,
               basedOn: S(function () { return m.appliedMaster ? m.appliedMaster.name : null; }),
               pageCount: m.pages.length, spreadGuides: guideList(m), pages: pages,
               showMasterItems: S(function () { return m.showMasterItems; }) });
    }
    return a;
  });

  D.pages = S(function () {
    var a = [];
    for (var i = 0; i < doc.pages.length && i < 60; i++) {
      var pg = doc.pages[i];
      a.push({ name: pg.name, index: pg.documentOffset, side: String(ENUMMAP[Number(pg.side)] || pg.side),
               appliedMaster: S(function () { return pg.appliedMaster ? pg.appliedMaster.name : null; }),
               appliedSection: S(function () { return pg.appliedSection ? pg.appliedSection.name : null; }),
               bounds: S(function () { return pg.bounds.join(", "); }),
               margins: S(function () { return P(pg.marginPreferences); }),
               guides: guideList(pg), items: pageItemSummary(pg, 40),
               pageColor: S(function () { return String(pg.pageColor); }) });
    }
    return { count: doc.pages.length, listed: a };
  });

  D.sections = S(function () {
    var a = [];
    for (var i = 0; i < doc.sections.length; i++) {
      var s = doc.sections[i];
      a.push({ name: s.name, pageStart: S(function () { return s.pageStart.name; }), continueNumbering: s.continueNumbering,
               includeSectionPrefix: s.includeSectionPrefix, marker: s.marker, pageNumberStart: s.pageNumberStart,
               pageNumberStyle: String(ENUMMAP[Number(s.pageNumberStyle)] || s.pageNumberStyle), sectionPrefix: s.sectionPrefix });
    }
    return a;
  });

  D.paragraphStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.allParagraphStyles.length; i++) a.push(paraStyle(doc.allParagraphStyles[i]));
    return a;
  });
  D.paragraphStyleGroups = S(function () { var a = []; for (var i = 0; i < doc.allParagraphStyleGroups.length; i++) a.push(doc.allParagraphStyleGroups[i].name); return a; });
  D.characterStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.allCharacterStyles.length; i++) a.push(charStyleNonInherited(doc.allCharacterStyles[i]));
    return a;
  });
  D.objectStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.allObjectStyles.length; i++) a.push(objStyle(doc.allObjectStyles[i]));
    return a;
  });
  D.tableStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.allTableStyles.length; i++) {
      var t = doc.allTableStyles[i];
      var o = P(t);
      o.__basedOn = S(function () { return t.basedOn ? t.basedOn.name : null; });
      o.__regionCellStyles = S(function () {
        return { headerRegion: t.headerRegionCellStyle ? t.headerRegionCellStyle.name : null,
                 bodyRegion: t.bodyRegionCellStyle ? t.bodyRegionCellStyle.name : null,
                 footerRegion: t.footerRegionCellStyle ? t.footerRegionCellStyle.name : null,
                 leftColumn: t.leftColumnRegionCellStyle ? t.leftColumnRegionCellStyle.name : null,
                 rightColumn: t.rightColumnRegionCellStyle ? t.rightColumnRegionCellStyle.name : null };
      });
      a.push(o);
    }
    return a;
  });
  D.cellStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.allCellStyles.length; i++) {
      var c = doc.allCellStyles[i];
      var o = P(c);
      o.__basedOn = S(function () { return c.basedOn ? c.basedOn.name : null; });
      o.__appliedParagraphStyle = S(function () { return c.appliedParagraphStyle ? c.appliedParagraphStyle.name : null; });
      a.push(o);
    }
    return a;
  });
  D.tocStyles = S(function () {
    var a = [];
    for (var i = 0; i < doc.tocStyles.length; i++) {
      var t = doc.tocStyles[i];
      var entries = [];
      for (var e = 0; e < t.tocStyleEntries.length; e++) {
        var en = t.tocStyleEntries[e];
        entries.push({ styleName: en.styleName, level: en.level,
                       formatStyle: S(function () { return en.formatStyle ? en.formatStyle.name : null; }),
                       pageNumberPosition: String(ENUMMAP[Number(en.pageNumberPosition)] || en.pageNumberPosition),
                       pageNumberStyle: S(function () { return en.pageNumberStyle ? en.pageNumberStyle.name : null; }),
                       separator: en.separator,
                       separatorStyle: S(function () { return en.separatorStyle ? en.separatorStyle.name : null; }),
                       sortAlphabet: en.sortAlphabet });
      }
      a.push({ name: t.name, title: t.title, titleStyle: S(function () { return t.titleStyle.name; }),
               includeBookDocuments: t.includeBookDocuments, createBookmarks: t.createBookmarks,
               createPDFBookmarks: S(function () { return t.createPDFBookmarks; }),
               includeTextOnHiddenLayers: S(function () { return t.includeTextOnHiddenLayers; }),
               runIn: S(function () { return t.runIn; }), entries: entries });
    }
    return a;
  });
  D.textVariables = S(function () {
    var a = [];
    for (var i = 0; i < doc.textVariables.length; i++) {
      var v = doc.textVariables[i];
      a.push({ name: v.name, type: String(ENUMMAP[Number(v.variableType)] || v.variableType), options: S(function () { return P(v.variableOptions); }) });
    }
    return a;
  });
  D.swatches = S(function () {
    var a = [];
    for (var i = 0; i < doc.swatches.length; i++) {
      var s = doc.swatches[i];
      a.push({ name: s.name, kind: S(function () { return s.constructor.name; }),
               model: S(function () { return String(ENUMMAP[Number(s.model)] || s.model); }),
               space: S(function () { return String(ENUMMAP[Number(s.space)] || s.space); }),
               value: S(function () { return s.colorValue ? s.colorValue.join(", ") : null; }),
               group: S(function () { return s.parent && s.parent.constructor.name === "ColorGroup" ? s.parent.name : null; }) });
    }
    return a;
  });
  D.colorGroups = S(function () { var a = []; for (var i = 0; i < doc.colorGroups.length; i++) a.push({ name: doc.colorGroups[i].name, members: doc.colorGroups[i].colorGroupSwatches.length }); return a; });
  D.inks = S(function () { var a = []; for (var i = 0; i < doc.inks.length; i++) a.push({ name: doc.inks[i].name, type: String(ENUMMAP[Number(doc.inks[i].inkType)] || doc.inks[i].inkType) }); return a; });
  D.conditions = S(function () { var a = []; for (var i = 0; i < doc.conditions.length; i++) { var c = doc.conditions[i]; a.push({ name: c.name, visible: c.visible, indicatorMethod: String(ENUMMAP[Number(c.indicatorMethod)] || c.indicatorMethod), indicatorColor: String(c.indicatorColor) }); } return a; });
  D.conditionSets = S(function () { var a = []; for (var i = 0; i < doc.conditionSets.length; i++) a.push(doc.conditionSets[i].name); return a; });
  D.hyperlinks = S(function () { return { hyperlinks: doc.hyperlinks.length, destinations: doc.hyperlinkURLDestinations.length + doc.hyperlinkTextDestinations.length + doc.hyperlinkPageDestinations.length, crossReferenceFormats: doc.crossReferenceFormats.length, bookmarks: doc.bookmarks.length }; });
  D.crossReferenceFormats = S(function () { var a = []; for (var i = 0; i < doc.crossReferenceFormats.length; i++) a.push(doc.crossReferenceFormats[i].name); return a; });
  D.numberedLists = S(function () { var a = []; for (var i = 0; i < doc.numberingLists.length; i++) a.push(doc.numberingLists[i].name); return a; });
  D.xmlTags = S(function () { var a = []; for (var i = 0; i < doc.xmlTags.length; i++) a.push(doc.xmlTags[i].name); return a; });
  D.stories = S(function () {
    var a = [];
    for (var i = 0; i < doc.stories.length && i < 30; i++) {
      var s = doc.stories[i];
      a.push({ index: i, length: s.length, paragraphs: s.paragraphs.length, contents: String(s.contents).substr(0, 200) });
    }
    return { count: doc.stories.length, listed: a };
  });
  D.pageItemTotals = S(function () { return { allPageItems: doc.allPageItems.length, spreads: doc.spreads.length, links: doc.links.length }; });
  D.links = S(function () { var a = []; for (var i = 0; i < doc.links.length && i < 60; i++) { var l = doc.links[i]; a.push({ name: l.name, status: String(ENUMMAP[Number(l.status)] || l.status), path: S(function () { return l.filePath; }) }); } return a; });
  D.fonts = S(function () {
    var a = [];
    for (var i = 0; i < doc.fonts.length; i++) {
      var f = doc.fonts[i];
      a.push({ name: S(function () { return f.name; }), family: S(function () { return f.fontFamily; }),
               style: S(function () { return f.fontStyleName; }), type: S(function () { return String(ENUMMAP[Number(f.fontType)] || f.fontType); }),
               status: String(ENUMMAP[Number(f.status)] || f.status),
               postscript: S(function () { return f.postscriptName; }), location: S(function () { return f.location; }) });
    }
    return a;
  });
  D.preflightProfiles = S(function () { var a = []; for (var i = 0; i < doc.preflightProfiles.length; i++) a.push(doc.preflightProfiles[i].name); return a; });
  D.activePreflightProcess = S(function () { return app.preflightProcesses.length; });
  D.documentPresetUsed = "<InDesign records no document-preset reference on a document>";
  D.printPreferences = S(function () { return P(doc.printPreferences); });
  D.pdfExportPresetsInDoc = S(function () { var a = []; for (var i = 0; i < doc.pdfExportPresets.length; i++) a.push(doc.pdfExportPresets[i].name); return a; });
  D.footnoteOptions = S(function () { return P(doc.footnoteOptions); });
  D.endnoteOptions = S(function () { return P(doc.endnoteOptions); });
  D.textFramePreferences = S(function () { return P(doc.textFramePreferences); });
  D.baselineFrameGridOptions = S(function () { return P(doc.baselineFrameGridOptions); });
  D.textDefaults = S(function () {
    var td = doc.textDefaults;
    return { composer: td.composer, appliedFont: S(function () { var f = td.appliedFont; return typeof f === "string" ? f : f.fontFamily + " | " + f.fontStyleName; }),
             pointSize: td.pointSize, leading: String(ENUMMAP[Number(td.leading)] || td.leading),
             justification: String(ENUMMAP[Number(td.justification)] || td.justification),
             appliedLanguage: S(function () { return td.appliedLanguage.name; }),
             alignToBaseline: td.alignToBaseline, hyphenation: td.hyphenation };
  });
  return D;
}

// ---------------- run ----------------
var LOG = [];
for (var ti = 0; ti < TARGETS.length; ti++) {
  var key = TARGETS[ti][0], path = TARGETS[ti][1];
  var f = new File(path);
  if (!f.exists) { LOG.push(key + ": MISSING " + path); continue; }
  var doc = null;
  try {
    doc = app.open(f, false);
    var D = dumpDoc(doc);
    D.__source = path;
    var fh = new File(OUT + "doc-" + key + ".json");
    fh.encoding = "UTF-8"; fh.open("w"); fh.write(ser(D, 0)); fh.close();
    LOG.push(key + ": OK -> " + fh.fsName);
  } catch (e) {
    LOG.push(key + ": ERROR " + e);
  }
  if (doc !== null) { try { doc.close(SaveOptions.NO); } catch (e2) { LOG.push(key + ": CLOSE ERROR " + e2); } }
}
LOG.join("\n") + "\n docsStillOpen=" + app.documents.length;
