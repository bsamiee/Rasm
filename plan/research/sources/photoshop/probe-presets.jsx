// READ-ONLY probe of the preset manager via executeActionGet. Writes nothing.
app.displayDialogs = DialogModes.NO;

function quote(s) {
    var out = "", c;
    for (var i = 0; i < s.length; i++) {
        c = s.charAt(i);
        if (c === '"' || c === '\\') out += "\\" + c;
        else if (c === "\n") out += "\\n";
        else if (c === "\r") out += "\\r";
        else if (c === "\t") out += "\\t";
        else if (s.charCodeAt(i) < 32 || s.charCodeAt(i) > 126) out += "\\u" + ("000" + s.charCodeAt(i).toString(16)).slice(-4);
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

var DEPTH_CAP = 6;

function idName(id) {
    try { var s = typeIDToStringID(id); if (s && s.length) return s; } catch (e) {}
    try { return typeIDToCharID(id); } catch (e) { return "id" + id; }
}

function readValue(d, key, depth, isList) {
    var t = isList ? d.getType(key) : d.getType(key);
    switch (t) {
        case DescValueType.BOOLEANTYPE: return d.getBoolean(key);
        case DescValueType.INTEGERTYPE: return d.getInteger(key);
        case DescValueType.LARGEINTEGERTYPE: return d.getLargeInteger(key);
        case DescValueType.DOUBLETYPE: return d.getDouble(key);
        case DescValueType.STRINGTYPE: return d.getString(key);
        case DescValueType.UNITDOUBLE: return { unit: idName(d.getUnitDoubleType(key)), value: d.getUnitDoubleValue(key) };
        case DescValueType.ENUMERATEDTYPE: return { enumType: idName(d.getEnumerationType(key)), value: idName(d.getEnumerationValue(key)) };
        case DescValueType.OBJECTTYPE: return depth >= DEPTH_CAP ? "<obj>" : walkDesc(d.getObjectValue(key), depth + 1);
        case DescValueType.LISTTYPE: return depth >= DEPTH_CAP ? "<list>" : walkList(d.getList(key), depth + 1);
        case DescValueType.CLASSTYPE: return { "class": idName(d.getClass(key)) };
        case DescValueType.ALIASTYPE: try { return d.getPath(key).fsName; } catch (e) { return "<alias>"; }
        case DescValueType.RAWTYPE: try { return "<raw " + d.getData(key).length + "b>"; } catch (e) { return "<raw>"; }
        case DescValueType.REFERENCETYPE: return "<ref>";
        default: return "<type" + t + ">";
    }
}

function walkDesc(d, depth) {
    var o = {};
    for (var i = 0; i < d.count; i++) {
        var k = d.getKey(i);
        var nm = idName(k);
        try { o[nm] = readValue(d, k, depth, false); } catch (e) { o[nm] = "<err:" + e + ">"; }
    }
    return o;
}

function walkList(l, depth) {
    var a = [];
    for (var i = 0; i < l.count; i++) {
        var t = l.getType(i);
        try {
            switch (t) {
                case DescValueType.BOOLEANTYPE: a.push(l.getBoolean(i)); break;
                case DescValueType.INTEGERTYPE: a.push(l.getInteger(i)); break;
                case DescValueType.LARGEINTEGERTYPE: a.push(l.getLargeInteger(i)); break;
                case DescValueType.DOUBLETYPE: a.push(l.getDouble(i)); break;
                case DescValueType.STRINGTYPE: a.push(l.getString(i)); break;
                case DescValueType.UNITDOUBLE: a.push({ unit: idName(l.getUnitDoubleType(i)), value: l.getUnitDoubleValue(i) }); break;
                case DescValueType.ENUMERATEDTYPE: a.push({ enumType: idName(l.getEnumerationType(i)), value: idName(l.getEnumerationValue(i)) }); break;
                case DescValueType.OBJECTTYPE: a.push(depth >= DEPTH_CAP ? "<obj>" : walkDesc(l.getObjectValue(i), depth + 1)); break;
                case DescValueType.LISTTYPE: a.push(depth >= DEPTH_CAP ? "<list>" : walkList(l.getList(i), depth + 1)); break;
                case DescValueType.CLASSTYPE: a.push({ "class": idName(l.getClass(i)) }); break;
                default: a.push("<type" + t + ">");
            }
        } catch (e) { a.push("<err:" + e + ">"); }
    }
    return a;
}

var out = {};

// --- preset manager ---
try {
    var r = new ActionReference();
    r.putProperty(stringIDToTypeID("property"), stringIDToTypeID("presetManager"));
    r.putEnumerated(stringIDToTypeID("application"), stringIDToTypeID("ordinal"), stringIDToTypeID("targetEnum"));
    var d = executeActionGet(r);
    out.presetManager = walkDesc(d, 0);
} catch (e) { out.presetManager = "<err:" + e + ">"; }

ser(out);
