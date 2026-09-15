#target "indesign"
app.scriptPreferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
var OUT = "/Users/bardiasamiee/Developer/Rasm/docs/research/creative-cloud-sources/scratchpad/indesign/";
var GD = "/Users/bardiasamiee/Library/CloudStorage/GoogleDrive-b.samiee@mzn-group.com/My Drive/03.Digital Asset Database/";
var out = [];
var f = new File(GD + "05.Software Related Assets/99.Default Profiles/InDesign/Default Template.indt");
var doc = app.open(f, false);
for (var i = 0; i < doc.tocStyles.length; i++) {
  var t = doc.tocStyles[i];
  out.push("TOC STYLE: " + t.name + " | title='" + t.title + "' | titleStyle=" + t.titleStyle.name);
  var pr = t.properties; var s = [];
  for (var k in pr) { try { s.push(k + "=" + String(pr[k])); } catch (e) {} }
  out.push("  props: " + s.join("; "));
  for (var e = 0; e < t.tocStyleEntries.length; e++) {
    var en = t.tocStyleEntries[e]; var p2 = en.properties; var s2 = [];
    for (var k2 in p2) { try { s2.push(k2 + "=" + String(p2[k2])); } catch (e3) {} }
    out.push("  entry " + e + ": " + s2.join("; "));
  }
}
out.push("SWATCH GROUPS:");
for (var g = 0; g < doc.colorGroups.length; g++) {
  var cg = doc.colorGroups[g]; var names = [];
  for (var m = 0; m < cg.colorGroupSwatches.length; m++) {
    var sw = cg.colorGroupSwatches[m].swatchItemRef;
    var val = ""; try { val = sw.colorValue.join("/"); } catch (e4) {}
    names.push(sw.name + "[" + val + "]");
  }
  out.push("  " + cg.name + ": " + names.join(", "));
}
out.push("FONT STYLES for Source Sans 3:");
for (var q = 0; q < app.fonts.length; q++) {
  try { if (app.fonts[q].fontFamily === "Source Sans 3") out.push("  " + app.fonts[q].fontStyleName + " | " + String(app.fonts[q].status) + " | " + app.fonts[q].location); } catch (e5) {}
}
out.push("MENU ACTION world-ready: " + (function(){ try { var m = app.menuActions.itemByName("Apply Adobe World-Ready Composers"); return m.isValid ? ("found, enabled=" + m.enabled) : "not found"; } catch(e){ return "err " + e; } })());
doc.close(SaveOptions.NO);
var fh = new File(OUT + "toc-and-swatches.txt"); fh.encoding = "UTF-8"; fh.open("w"); fh.write(out.join("\n")); fh.close();
"WROTE " + fh.fsName;
