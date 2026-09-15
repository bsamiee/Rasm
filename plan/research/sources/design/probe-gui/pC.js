(function(){var o=[];function T(k,f){try{o.push(k+"="+f());}catch(e){o.push(k+"=ERR "+e.message);}}
app.scriptPreferences.userInteractionLevel=UserInteractionLevels.NEVER_INTERACT;
var dir="/Users/bardiasamiee/Developer/Rasm/docs/research/creative-cloud-sources/scratchpad/design/probe-gui/live/";
T("U10_exportPresets",function(){var f=File(dir+"presets-test.joboptions");app.exportPresets(ExportPresetFormat.PDF_EXPORT_PRESETS_FORMAT,f);return "exists="+f.exists+" len="+f.length});
T("U10_exportPresets_noext",function(){var f=File(dir+"presets-test-noext");app.exportPresets(ExportPresetFormat.PDF_EXPORT_PRESETS_FORMAT,f);return "exists="+f.exists+" len="+f.length+" name="+f.name});
T("U10_exportDocPresets",function(){var f=File(dir+"docpresets-test");app.exportPresets(ExportPresetFormat.DOCUMENT_PRESETS_FORMAT,f);return "exists="+f.exists+" len="+f.length});
T("modal1",function(){return String(app.modalState)});
T("U27_exportSettings_File",function(){var f=File(dir+"usersettings-test");var r=app.exportSettings(f);return "ret="+String(r)+" exists="+f.exists+" len="+f.length+" modal="+app.modalState});
T("U27_exportSettings_noarg_typeof",function(){return typeof app.exportSettings});
T("modal2",function(){return String(app.modalState)});
return o.join("\n");})()
