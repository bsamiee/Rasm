// Follow-up read-only probe: composer, text defaults, swatches, color settings.
#target "indesign"
app.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
var OUT = "/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/indesign/";

function S(fn) { try { var v = fn(); return (v === undefined) ? "<undefined>" : v; } catch (e) { return "<err: " + e + ">"; } }
function J(v) {
  if (v === null || v === undefined) return "null";
  var t = typeof v;
  if (t === "boolean") return v ? "true" : "false";
  if (t === "number") return String(v);
  if (t === "string") { return '"' + v.replace(/\\/g, "\\\\").replace(/"/g, '\\"').replace(/\n/g, "\\n") + '"'; }
  if (v instanceof Array) { var a = []; for (var i = 0; i < v.length; i++) a.push(J(v[i])); return "[" + a.join(",") + "]"; }
  if (t === "object") {
    var c = ""; try { c = v.constructor.name; } catch (e) {}
    if (c === "Object") { var p = []; for (var k in v) if (v.hasOwnProperty(k)) p.push(J(k) + ":" + J(v[k])); return "{" + p.join(",") + "}"; }
    return J("<" + c + " " + S(function () { return String(v); }) + ">");
  }
  return J(String(v));
}

var R = {};
R.textDefaults = S(function () {
  var td = app.textDefaults;
  return {
    composer: td.composer,
    appliedFont: String(td.appliedFont),
    fontStyle: td.fontStyle,
    pointSize: td.pointSize,
    leading: String(td.leading),
    justification: String(td.justification),
    hyphenation: td.hyphenation,
    appliedLanguage: String(td.appliedLanguage),
    kerningMethod: td.kerningMethod,
    tracking: td.tracking,
    firstLineIndent: td.firstLineIndent,
    leftIndent: td.leftIndent,
    rightIndent: td.rightIndent,
    spaceBefore: td.spaceBefore,
    spaceAfter: td.spaceAfter,
    alignToBaseline: td.alignToBaseline,
    hyphenateAfterFirst: td.hyphenateAfterFirst,
    hyphenateBeforeLast: td.hyphenateBeforeLast,
    hyphenateWordsLongerThan: td.hyphenateWordsLongerThan,
    hyphenateCapitalizedWords: td.hyphenateCapitalizedWords,
    hyphenateLadderLimit: td.hyphenateLadderLimit,
    hyphenationZone: td.hyphenationZone,
    hyphenWeight: td.hyphenWeight,
    minimumWordSpacing: td.minimumWordSpacing,
    desiredWordSpacing: td.desiredWordSpacing,
    maximumWordSpacing: td.maximumWordSpacing,
    minimumLetterSpacing: td.minimumLetterSpacing,
    desiredLetterSpacing: td.desiredLetterSpacing,
    maximumLetterSpacing: td.maximumLetterSpacing,
    minimumGlyphScaling: td.minimumGlyphScaling,
    desiredGlyphScaling: td.desiredGlyphScaling,
    maximumGlyphScaling: td.maximumGlyphScaling,
    paragraphDirection: S(function () { return String(td.paragraphDirection); }),
    characterDirection: S(function () { return String(td.characterDirection); }),
    digitsType: S(function () { return String(td.digitsType); }),
    kashidas: S(function () { return String(td.kashidas); }),
    diacriticPosition: S(function () { return String(td.diacriticPosition); })
  };
});

R.basicParagraphComposer = S(function () { return app.paragraphStyles.itemByName("[Basic Paragraph]").composer; });
R.noParagraphStyleComposer = S(function () { return app.paragraphStyles[0].composer; });
R.composerValueType = S(function () { return typeof app.paragraphStyles.itemByName("[Basic Paragraph]").composer; });
// Read the raw internal key InDesign stores for the composer, without writing anything.
R.composerRawOnTextDefaults = S(function () { return String(app.textDefaults.properties.composer); });

R.colorSettings = S(function () {
  var c = app.colorSettings;
  var o = {};
  var p = c.properties;
  for (var k in p) { o[k] = S(function () { return String(p[k]); }); }
  return o;
});

R.transparencyPreferences = S(function () {
  var o = {}; var p = app.transparencyPreferences.properties;
  for (var k in p) o[k] = S(function () { return String(p[k]); });
  return o;
});

R.swatches = S(function () {
  var out = [];
  for (var i = 0; i < app.swatches.length; i++) {
    var s = app.swatches[i];
    out.push({
      name: s.name,
      kind: S(function () { return s.constructor.name; }),
      model: S(function () { return String(s.model); }),
      space: S(function () { return String(s.space); }),
      value: S(function () { return s.colorValue ? s.colorValue.join(", ") : null; })
    });
  }
  return out;
});

R.inkList = S(function () { var o = []; for (var i = 0; i < app.inks.length; i++) o.push(app.inks[i].name); return o; });

R.userDictionaries = S(function () { var o = []; for (var i = 0; i < app.userDictionaries.length; i++) o.push(app.userDictionaries[i].name); return o; });

R.autoCorrectTables = S(function () {
  var o = [];
  for (var i = 0; i < app.autoCorrectTables.length; i++) {
    var t = app.autoCorrectTables[i];
    o.push({ language: String(t.language), pairs: S(function () { return t.autoCorrectWordPairList ? t.autoCorrectWordPairList.length : 0; }) });
  }
  return o;
});

R.glyphSets = S(function () {
  var o = []; for (var i = 0; i < app.glyphSets.length; i++) o.push(app.glyphSets[i].name); return o;
});

R.pdfExportPreferencesKey = S(function () {
  var p = app.pdfExportPreferences;
  return {
    acrobatCompatibility: String(p.acrobatCompatibility),
    standardsCompliance: String(p.standardsCompliance),
    colorBitmapCompression: String(p.colorBitmapCompression),
    colorBitmapSampling: String(p.colorBitmapSampling),
    colorBitmapSamplingDPI: p.colorBitmapSamplingDPI,
    grayscaleBitmapSamplingDPI: p.grayscaleBitmapSamplingDPI,
    monochromeBitmapSamplingDPI: p.monochromeBitmapSamplingDPI,
    pdfColorSpace: String(p.pdfColorSpace),
    pdfDestinationProfile: String(p.pdfDestinationProfile),
    includeICCProfiles: String(p.includeICCProfiles),
    exportGuidesAndGrids: p.exportGuidesAndGrids,
    exportLayers: p.exportLayers,
    exportNonprintingObjects: p.exportNonprintingObjects,
    bleedTop: p.bleedTop, bleedBottom: p.bleedBottom, bleedInside: p.bleedInside, bleedOutside: p.bleedOutside,
    useDocumentBleedWithPDF: p.useDocumentBleedWithPDF,
    cropMarks: p.cropMarks, bleedMarks: p.bleedMarks, pageInformationMarks: p.pageInformationMarks,
    registrationMarks: p.registrationMarks, colorBars: p.colorBars,
    subsetFontsBelow: p.subsetFontsBelow,
    optimizePDF: p.optimizePDF,
    viewPDF: p.viewPDF,
    generateThumbnails: p.generateThumbnails,
    pageMarksOffset: p.pageMarksOffset,
    exportReaderSpreads: p.exportReaderSpreads,
    exportWhich: String(p.exportWhich)
  };
});

R.fontsInstalledFamilies = S(function () {
  var seen = {}, o = [];
  for (var i = 0; i < app.fonts.length; i++) {
    try { var f = app.fonts[i].fontFamily; if (!seen[f]) { seen[f] = 1; o.push(f); } } catch (e) {}
  }
  o.sort();
  return o;
});

var fh = new File(OUT + "app-preferences-2.json");
fh.encoding = "UTF-8"; fh.open("w"); fh.write(J(R)); fh.close();
"WROTE " + fh.fsName;
