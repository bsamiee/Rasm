// READ-ONLY probe. Reads app.preferences, colorSettings, fonts, action sets. Writes nothing.
app.displayDialogs = DialogModes.NO;

function val(v) {
    try {
        if (v === null || v === undefined) return null;
        var t = typeof v;
        if (t === "boolean" || t === "number" || t === "string") return v;
        if (v instanceof Array) { var a = []; for (var i = 0; i < v.length; i++) a.push(val(v[i])); return a; }
        if (v instanceof File || v instanceof Folder) return v.fsName;
        return String(v);
    } catch (e) { return "<err:" + e + ">"; }
}

function quote(s) {
    var out = "", c;
    for (var i = 0; i < s.length; i++) {
        c = s.charAt(i);
        if (c === '"' || c === '\\') out += "\\" + c;
        else if (c === "\n") out += "\\n";
        else if (c === "\r") out += "\\r";
        else if (c === "\t") out += "\\t";
        else if (s.charCodeAt(i) < 32) out += "\\u" + ("000" + s.charCodeAt(i).toString(16)).slice(-4);
        else out += c;
    }
    return '"' + out + '"';
}

function ser(v) {
    if (v === null || v === undefined) return "null";
    var t = typeof v;
    if (t === "boolean") return v ? "true" : "false";
    if (t === "number") return isFinite(v) ? String(v) : "null";
    if (t === "string") return quote(v);
    if (v instanceof Array) { var a = []; for (var i = 0; i < v.length; i++) a.push(ser(v[i])); return "[" + a.join(",") + "]"; }
    var p = [];
    for (var k in v) { if (v.hasOwnProperty(k)) p.push(quote(k) + ":" + ser(v[k])); }
    return "{" + p.join(",") + "}";
}

var out = {};

// --- app.preferences, reflected ---
var prefs = {};
try {
    var props = app.preferences.reflect.properties;
    for (var i = 0; i < props.length; i++) {
        var nm = props[i].name;
        if (nm === "__proto__" || nm === "reflect" || nm === "typename") continue;
        try { prefs[nm] = val(app.preferences[nm]); } catch (e) { prefs[nm] = "<unreadable:" + e + ">"; }
    }
} catch (e) { prefs["<reflect-failed>"] = String(e); }
out.preferences = prefs;

// --- application-level scalars ---
var appinfo = {};
var names = ["version", "build", "colorSettings", "freeMemory", "scriptingVersion", "locale",
             "backgroundColor", "foregroundColor", "displayDialogs", "playbackDisplayDialogs",
             "playbackParameters", "preferencesFolder", "path"];
for (var n = 0; n < names.length; n++) {
    try { appinfo[names[n]] = val(app[names[n]]); } catch (e) { appinfo[names[n]] = "<unreadable:" + e + ">"; }
}
try { appinfo.fontsCount = app.fonts.length; } catch (e) { appinfo.fontsCount = String(e); }
try { appinfo.documentsCount = app.documents.length; } catch (e) {}
try { appinfo.measurementScale = String(app.activeDocument); } catch (e) {}
out.application = appinfo;

// --- action sets and actions ---
var sets = [];
try {
    var idASet = charIDToTypeID("ASet");
    var idActn = charIDToTypeID("Actn");
    var idName = charIDToTypeID("Nm  ");
    var idNmbC = charIDToTypeID("NmbC");
    var si = 1;
    while (si < 400) {
        var r = new ActionReference();
        r.putIndex(idASet, si);
        var d;
        try { d = executeActionGet(r); } catch (e) { break; }
        var setName = d.getString(idName);
        var cnt = 0;
        try { cnt = d.getInteger(idNmbC); } catch (e) {}
        var acts = [];
        for (var ai = 1; ai <= cnt; ai++) {
            try {
                var r2 = new ActionReference();
                r2.putIndex(idActn, ai);
                r2.putIndex(idASet, si);
                acts.push(executeActionGet(r2).getString(idName));
            } catch (e2) { acts.push("<err " + ai + ">"); }
        }
        sets.push({ index: si, name: setName, count: cnt, actions: acts });
        si++;
    }
} catch (e) { sets.push({ error: String(e) }); }
out.actionSets = sets;

// --- font family sample (first 40) ---
try {
    var fams = {};
    for (var f = 0; f < app.fonts.length; f++) { fams[app.fonts[f].family] = true; }
    var fl = [];
    for (var fk in fams) { if (fams.hasOwnProperty(fk)) fl.push(fk); }
    out.fontFamilyCount = fl.length;
} catch (e) { out.fontFamilyCount = String(e); }

ser(out);
