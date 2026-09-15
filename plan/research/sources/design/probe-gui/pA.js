(function(){var o=[];function T(k,f){try{o.push(k+"="+f());}catch(e){o.push(k+"=ERR "+e.message);}}
app.scriptPreferences.userInteractionLevel=UserInteractionLevels.NEVER_INTERACT;
var doc=app.documents.add(false);
try{
doc.viewPreferences.horizontalMeasurementUnits=MeasurementUnits.POINTS;doc.viewPreferences.verticalMeasurementUnits=MeasurementUnits.POINTS;doc.viewPreferences.rulerOrigin=RulerOrigin.PAGE_ORIGIN;
doc.documentPreferences.pageWidth="612pt";doc.documentPreferences.pageHeight="792pt";doc.documentPreferences.facingPages=false;
var pg=doc.pages.item(0);pg.marginPreferences.top="35.45025pt";pg.marginPreferences.bottom="102pt";pg.marginPreferences.left="45pt";pg.marginPreferences.right="45pt";
doc.gridPreferences.baselineGridRelativeOption=BaselineGridRelativeOption.TOP_OF_MARGIN_OF_BASELINE_GRID_RELATIVE_OPTION;doc.gridPreferences.baselineStart="9.54975pt";doc.gridPreferences.baselineDivision="15pt";
var fontName="Neue Haas Grotesk Text Pro\tRoman";var fnt=app.fonts.itemByName("Neue Haas Grotesk Text Pro\t55 Roman");T("fontValid",function(){return fnt.isValid+" "+String(fnt.status)});
var em=doc.characterStyles.add({name:"Emphasis"});em.fontStyle="Italic";var st=doc.characterStyles.add({name:"Strong"});st.fontStyle="Bold";
var base=doc.paragraphStyles.add({name:"Base"});base.appliedFont=fnt;base.pointSize=12.75;base.leading=15;base.alignToBaseline=true;
// U9 tabList
var ps=doc.paragraphStyles.add({name:"T"});T("U9_emptyArray",function(){ps.tabList=[];return "accepted len="+ps.tabList.length});
T("U9_objects",function(){ps.tabList=[{alignment:TabStopAlignment.LEFT_ALIGN,position:15,leader:""},{alignment:TabStopAlignment.RIGHT_ALIGN,position:522,leader:""}];return "len="+ps.tabList.length+" first="+ps.tabList[0].join?ps.tabList[0].join("/"):String(ps.tabList[0])});
T("U9_clearAgain",function(){ps.tabList=[];return "len="+ps.tabList.length});
// U8 hyphenWeight
T("U8",function(){var r=[];ps.hyphenWeight=0;r.push(ps.hyphenWeight);ps.hyphenWeight=5;r.push(ps.hyphenWeight);ps.hyphenWeight=10;r.push(ps.hyphenWeight);try{ps.hyphenWeight=11;r.push("11->"+ps.hyphenWeight);}catch(e){r.push("11 rejected: "+e.message);}return r.join(",")});
// U28 first baseline snap
var tf=pg.textFrames.add({geometricBounds:[35.45025,45,200,300]});tf.textFramePreferences.firstBaselineOffset=FirstBaseline.CAP_HEIGHT;tf.textFramePreferences.insetSpacing=[0,0,0,0];
tf.contents="Hello world first line\rSecond paragraph";tf.paragraphs.everyItem().appliedParagraphStyle=base;
T("U28_snapped",function(){return "line0 baseline="+tf.lines.item(0).baseline+" line1="+tf.lines.item(1).baseline});
base.alignToBaseline=false;T("U28_unsnapped",function(){return "line0 baseline="+tf.lines.item(0).baseline});base.alignToBaseline=true;
// U1 nested vs GREP
var pn=doc.paragraphStyles.add({name:"N",basedOn:base});pn.nestedStyles.add({appliedCharacterStyle:em,delimiter:NestedStyleDelimiters.ANY_WORD,repetition:1,inclusive:false});pn.nestedGrepStyles.add({grepExpression:"^\\w+",appliedCharacterStyle:st});
tf.paragraphs.item(0).appliedParagraphStyle=pn;T("U1_nestedItalic_vs_grepBold",function(){return "char0="+tf.paragraphs.item(0).characters.item(0).fontStyle+" char1="+tf.paragraphs.item(0).characters.item(1).fontStyle});
var pg2=doc.paragraphStyles.add({name:"G",basedOn:base});pg2.nestedGrepStyles.add({grepExpression:"^\\w+",appliedCharacterStyle:em});pg2.nestedGrepStyles.add({grepExpression:"^\\w+",appliedCharacterStyle:st});
tf.paragraphs.item(0).appliedParagraphStyle=pg2;T("U1_grepItalic_then_grepBold",function(){return "char0="+tf.paragraphs.item(0).characters.item(0).fontStyle});
var pg3=doc.paragraphStyles.add({name:"G2",basedOn:base});pg3.nestedGrepStyles.add({grepExpression:"^\\w+",appliedCharacterStyle:st});pg3.nestedGrepStyles.add({grepExpression:"^\\w+",appliedCharacterStyle:em});
tf.paragraphs.item(0).appliedParagraphStyle=pg3;T("U1_grepBold_then_grepItalic",function(){return "char0="+tf.paragraphs.item(0).characters.item(0).fontStyle});
tf.paragraphs.item(0).appliedParagraphStyle=base;
// U5 table rows
var tb=doc.paragraphStyles.add({name:"Table Body",basedOn:base});tb.pointSize=9.5625;tb.leading=15;tb.alignToBaseline=false;
var tf2=pg.textFrames.add({geometricBounds:[300,45,500,567]});tf2.textFramePreferences.insetSpacing=[0,0,0,0];tf2.textFramePreferences.firstBaselineOffset=FirstBaseline.LEADING_OFFSET;
var tbl=tf2.insertionPoints.item(0).tables.add({headerRowCount:0,bodyRowCount:3,columnCount:2});
tbl.cells.everyItem().textTopInset=0;tbl.cells.everyItem().textBottomInset=0;tbl.cells.everyItem().textLeftInset=5;tbl.cells.everyItem().textRightInset=5;tbl.cells.everyItem().firstBaselineOffset=FirstBaseline.LEADING_OFFSET;tbl.cells.everyItem().minimumFirstBaselineOffset=0;tbl.cells.everyItem().verticalJustification=VerticalJustification.TOP_ALIGN;
tbl.rows.item(0).cells.item(0).contents="one line";tbl.rows.item(1).cells.item(0).contents="line one\nline two";tbl.rows.item(2).cells.item(0).contents="x";
tbl.cells.everyItem().texts.everyItem().appliedParagraphStyle=tb;
T("U5_autoHeights",function(){return "r0="+tbl.rows.item(0).height+" r1="+tbl.rows.item(1).height+" r2="+tbl.rows.item(2).height+" minFirst="+tbl.rows.item(0).minimumHeight});
T("U5_fixed",function(){var r=tbl.rows.item(2);r.autoGrow=false;r.height=15;return "r2="+r.height+" autoGrow="+r.autoGrow});
T("U5_fixedTooSmall",function(){var r=tbl.rows.item(1);r.autoGrow=false;r.height=15;return "r1 two lines forced 15 -> height="+r.height+" overflows="+r.cells.item(0).overflows});
// U2 / U21 story direction via object style
var osf=doc.objectStyles.add({name:"Text Frame Fa"});osf.enableStoryOptions=true;osf.storyPreferences.storyDirection=StoryDirectionOptions.RIGHT_TO_LEFT_DIRECTION;
var tf3=pg.textFrames.add({geometricBounds:[520,45,700,300]});tf3.contents="abc";
T("U2_before",function(){return String(tf3.parentStory.storyPreferences.storyDirection)});
tf3.appliedObjectStyle=osf;T("U2_afterStyle",function(){return String(tf3.parentStory.storyPreferences.storyDirection)});
tf3.parentStory.storyPreferences.storyDirection=StoryDirectionOptions.LEFT_TO_RIGHT_DIRECTION;T("U21_storySetLTR",function(){return String(tf3.parentStory.storyPreferences.storyDirection)+" overrides="+tf3.overridden});
tf3.applyObjectStyle(osf,true,false);T("U21_reapplyClear",function(){return String(tf3.parentStory.storyPreferences.storyDirection)});
// U19
T("U19_changeComposer",function(){return typeof doc.changeComposer});
// U20 camelCase in ExtendScript
T("U20_camel",function(){return "rightToLeftDirection="+String(ParagraphDirectionOptions.rightToLeftDirection)+" UPPER="+String(ParagraphDirectionOptions.RIGHT_TO_LEFT_DIRECTION)});
// U22 createGuides
var ms=doc.masterSpreads.item(0);var lay=doc.layers.add({name:"Grid"});
T("U22",function(){ms.createGuides(5,4,"15pt","15pt",UIColors.GREEN,true,false,lay);var g=ms.guides.everyItem().getElements();var h=[],v=[];for(var i=0;i<g.length;i++){(String(g[i].orientation)=="HORIZONTAL"?h:v).push(Math.round(g[i].location*1000)/1000);}h.sort(function(a,b){return a-b});v.sort(function(a,b){return a-b});return "H="+h.join(",")+" V="+v.join(",")});
// U6 / U18 Arabic
var arNames=["Adobe Arabic\tRegular","Geeza Pro\tRegular","Noto Naskh Arabic\tRegular","Vazirmatn\tRegular","Estedad\tRegular"];var arFont=null;for(var i=0;i<arNames.length;i++){var f=app.fonts.itemByName(arNames[i]);if(f.isValid){arFont=f;break;}}
T("arFont",function(){return arFont?arFont.name:"none"});
var pfa=doc.paragraphStyles.add({name:"Body Fa",basedOn:base});if(arFont)pfa.appliedFont=arFont;pfa.composer="Adobe World-Ready Paragraph Composer";pfa.paragraphDirection=ParagraphDirectionOptions.RIGHT_TO_LEFT_DIRECTION;pfa.justification=Justification.RIGHT_JUSTIFIED;pfa.digitsType=DigitsTypeOptions.FARSI_DIGITS;pfa.kashidas=KashidasOptions.KASHIDAS_OFF;pfa.paragraphKashidaWidth=0;pfa.hyphenation=false;pfa.appliedLanguage=app.languagesWithVendors.item("[No Language]");
T("composerReadback",function(){return pfa.composer});
var pff=doc.paragraphStyles.add({name:"Body Fa Full",basedOn:pfa});pff.digitsType=DigitsTypeOptions.FULL_FARSI_DIGITS;
var tf4=pg.textFrames.add({geometricBounds:[560,320,760,567]});tf4.textFramePreferences.insetSpacing=[0,0,0,0];
var ar="این یک متن آزمایشی فارسی است که برای بررسی تراز کردن خط آخر پاراگراف نوشته شده است و باید چند خط را پر کند تا خط آخر دیده شود";
tf4.contents=ar+" ۰۱۲۳ 0123 ٠١٢٣\r"+"۰۱۲۳ 0123 ٠١٢٣ FARSI\r"+"۰۱۲۳ 0123 ٠١٢٣ FULL\r"+"۰۱۲۳ 0123 ٠١٢٣ HINDI";
tf4.paragraphs.everyItem().appliedParagraphStyle=pfa;tf4.paragraphs.item(2).appliedParagraphStyle=pff;
var phi=doc.paragraphStyles.add({name:"Body Ar",basedOn:pfa});phi.digitsType=DigitsTypeOptions.HINDI_DIGITS;tf4.paragraphs.item(3).appliedParagraphStyle=phi;
T("U6",function(){var p=tf4.paragraphs.item(0);var n=p.lines.length;var last=p.lines.item(n-1);var first=p.lines.item(0);var gb=tf4.geometricBounds;return "lines="+n+" frameL="+gb[1]+" frameR="+gb[3]+" first[h="+first.horizontalOffset+",end="+first.endHorizontalOffset+"] last[h="+last.horizontalOffset+",end="+last.endHorizontalOffset+"]"});
T("digitsReadback",function(){return String(tf4.paragraphs.item(1).digitsType)+"/"+String(tf4.paragraphs.item(2).digitsType)+"/"+String(tf4.paragraphs.item(3).digitsType)});
T("export",function(){app.jpegExportPreferences.jpegExportRange=ExportRangeOrAllPages.EXPORT_ALL;app.jpegExportPreferences.exportResolution=200;app.jpegExportPreferences.jpegQuality=JPEGOptionsQuality.MAXIMUM;app.jpegExportPreferences.antiAlias=true;doc.exportFile(ExportFormat.JPG,File("/Users/bardiasamiee/Developer/Rasm/docs/research/creative-cloud-sources/scratchpad/design/probe-gui/live/pA.jpg"),false);return "ok"});
}catch(e){o.push("FATAL "+e.message+" line "+e.line);}
try{doc.close(SaveOptions.NO);}catch(e){o.push("close ERR "+e.message);}
o.push("docsAfter="+app.documents.length);
return o.join("\n");})()
