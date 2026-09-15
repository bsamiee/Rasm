(function(){var o=[];function T(k,f){try{o.push(k+"="+f());}catch(e){o.push(k+"=ERR "+e.message);}}
app.scriptPreferences.userInteractionLevel=UserInteractionLevels.NEVER_INTERACT;
T("lang_item",function(){var l=app.languagesWithVendors.item("[No Language]");return "isValid="+l.isValid+" name="+l.name});
T("lang_itemByName",function(){var l=app.languagesWithVendors.itemByName("[No Language]");return "isValid="+l.isValid+" name="+l.name});
T("lang_index0",function(){var l=app.languagesWithVendors.item(0);return "isValid="+l.isValid+" name="+l.name});
T("lang_arabic_item",function(){var l=app.languagesWithVendors.item("Arabic");return "isValid="+l.isValid+" name="+l.name});
T("lang_key",function(){return "translate="+app.translateKeyString("$ID/[No Language]")+" keys="+app.findKeyStrings("[No Language]").join("|")});
T("languages_coll",function(){var l=app.languages.item("[No Language]");return "app.languages isValid="+l.isValid+" count="+app.languages.length});
var doc=app.documents.add(false);
try{
doc.viewPreferences.horizontalMeasurementUnits=MeasurementUnits.POINTS;doc.viewPreferences.verticalMeasurementUnits=MeasurementUnits.POINTS;doc.viewPreferences.rulerOrigin=RulerOrigin.PAGE_ORIGIN;
var pg=doc.pages.item(0);
var fnt=app.fonts.itemByName("Neue Haas Grotesk Text Pro\t55 Roman");var base=doc.paragraphStyles.add({name:"Base"});base.appliedFont=fnt;base.pointSize=12.75;base.leading=15;
var ps=doc.paragraphStyles.add({name:"T"});
T("U9_objects",function(){ps.tabList=[{alignment:TabStopAlignment.LEFT_ALIGN,position:15,leader:""},{alignment:TabStopAlignment.RIGHT_ALIGN,position:522,leader:""}];var t=ps.tabList;var s=[];for(var i=0;i<t.length;i++){var e=t[i];s.push((e.position!==undefined?e.position:String(e))+":"+(e.alignment!==undefined?String(e.alignment):"?"));}return "len="+t.length+" "+s.join(";")});
T("U9_charAlign",function(){ps.tabList=[{alignment:TabStopAlignment.CHARACTER_ALIGN,alignmentCharacter:".",position:60,leader:""}];return "len="+ps.tabList.length+" ac="+ps.tabList[0].alignmentCharacter});
// language assignment forms
T("langAssign_index",function(){var l=app.languagesWithVendors.item(0);ps.appliedLanguage=l;return ps.appliedLanguage.name});
T("langAssign_string",function(){ps.appliedLanguage="[No Language]";return ps.appliedLanguage.name});
T("langAssign_key",function(){ps.appliedLanguage="$ID/[No Language]";return ps.appliedLanguage.name});
T("langAssign_arabicObj",function(){ps.appliedLanguage=app.languagesWithVendors.item("Arabic");return ps.appliedLanguage.name});
T("langAssign_arabicString",function(){ps.appliedLanguage="Arabic";return ps.appliedLanguage.name});
T("langAssign_docLanguages",function(){var l=doc.languagesWithVendors.item("[No Language]");return "doc coll isValid="+l.isValid});
// Arabic block
var arFont=app.fonts.itemByName("Adobe Arabic\tRegular");
var pfa=doc.paragraphStyles.add({name:"Body Fa",basedOn:base});pfa.appliedFont=arFont;pfa.composer="Adobe World-Ready Paragraph Composer";pfa.paragraphDirection=ParagraphDirectionOptions.RIGHT_TO_LEFT_DIRECTION;pfa.justification=Justification.RIGHT_JUSTIFIED;pfa.digitsType=DigitsTypeOptions.FARSI_DIGITS;pfa.kashidas=KashidasOptions.KASHIDAS_OFF;pfa.paragraphKashidaWidth=0;pfa.hyphenation=false;pfa.appliedLanguage="[No Language]";
T("composerReadback",function(){return pfa.composer+" | lang="+pfa.appliedLanguage.name});
var pff=doc.paragraphStyles.add({name:"Body Fa Full",basedOn:pfa});pff.digitsType=DigitsTypeOptions.FULL_FARSI_DIGITS;
var phi=doc.paragraphStyles.add({name:"Body Ar",basedOn:pfa});phi.digitsType=DigitsTypeOptions.HINDI_DIGITS;phi.appliedLanguage="Arabic";phi.kashidas=KashidasOptions.DEFAULT_KASHIDAS;phi.paragraphKashidaWidth=2;
var tf4=pg.textFrames.add({geometricBounds:[60,60,400,360]});tf4.textFramePreferences.insetSpacing=[0,0,0,0];
var ar="این یک متن آزمایشی فارسی است که برای بررسی تراز کردن خط آخر پاراگراف نوشته شده است و باید چند خط را پر کند تا خط آخر دیده شود";
tf4.contents=ar+"\r"+"FARSI ۰۱۲۳۴۵۶ 0123456 ٠١٢٣٤٥٦\r"+"FULL ۰۱۲۳۴۵۶ 0123456 ٠١٢٣٤٥٦\r"+"HINDI ۰۱۲۳۴۵۶ 0123456 ٠١٢٣٤٥٦\r"+"DEFAULT ۰۱۲۳۴۵۶ 0123456 ٠١٢٣٤٥٦\rابجد هوز حطي كلمن سعفص قرشت ثخذ ضظغ نص عربي طويل لاختبار الكشيدة في السطر الأخير من الفقرة";
tf4.paragraphs.everyItem().appliedParagraphStyle=pfa;tf4.paragraphs.item(2).appliedParagraphStyle=pff;tf4.paragraphs.item(3).appliedParagraphStyle=phi;tf4.paragraphs.item(4).digitsType=DigitsTypeOptions.DEFAULT_DIGITS;tf4.paragraphs.item(5).appliedParagraphStyle=phi;
T("U6",function(){var p=tf4.paragraphs.item(0);var n=p.lines.length;var last=p.lines.item(n-1);var first=p.lines.item(0);var gb=tf4.geometricBounds;return "lines="+n+" frameL="+gb[1]+" frameR="+gb[3]+" first[h="+first.horizontalOffset+",end="+first.endHorizontalOffset+"] last[h="+last.horizontalOffset+",end="+last.endHorizontalOffset+"]"});
T("U6_arabicKashida",function(){var p=tf4.paragraphs.item(5);var n=p.lines.length;var last=p.lines.item(n-1);return "lines="+n+" last[h="+last.horizontalOffset+",end="+last.endHorizontalOffset+"] kashidas="+String(p.kashidas)+" width="+p.paragraphKashidaWidth});
T("export",function(){app.jpegExportPreferences.jpegExportRange=ExportRangeOrAllPages.EXPORT_ALL;app.jpegExportPreferences.exportResolution=220;app.jpegExportPreferences.jpegQuality=JPEGOptionsQuality.MAXIMUM;app.jpegExportPreferences.antiAlias=true;doc.exportFile(ExportFormat.JPG,File("/Users/bardiasamiee/Developer/Rasm/docs/research/creative-cloud-sources/scratchpad/design/probe-gui/live/pB.jpg"),false);return "ok"});
T("U19_effect",function(){var before=base.composer+"|"+doc.textDefaults.composer;doc.changeComposer();return "before="+before+" after base="+base.composer+" textDefaults="+doc.textDefaults.composer+" para0="+tf4.paragraphs.item(0).composer});
}catch(e){o.push("FATAL "+e.message+" line "+e.line);}
try{doc.close(SaveOptions.NO);}catch(e){o.push("close ERR "+e.message);}
o.push("docsAfter="+app.documents.length);
return o.join("\n");})()
