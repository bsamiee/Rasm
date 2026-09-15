// Read-only dump of an Illustrator document to JSON. Never saves.
// Reads job file: line 1 = document path, line 2 = output json path, line 3 = "deep" or "shallow"

#target illustrator

function esc(s) {
    s = String(s);
    var out = "";
    for (var i = 0; i < s.length; i++) {
        var c = s.charAt(i), n = s.charCodeAt(i);
        if (c === '"') out += '\\"';
        else if (c === "\\") out += "\\\\";
        else if (n < 32 || n > 126) out += "\\u" + ("0000" + n.toString(16)).slice(-4);
        else out += c;
    }
    return '"' + out + '"';
}

function ser(v, depth) {
    if (v === null || v === undefined) return "null";
    var t = typeof v;
    if (t === "number") return isFinite(v) ? String(v) : "null";
    if (t === "boolean") return v ? "true" : "false";
    if (t === "string") return esc(v);
    if (v instanceof Array) {
        var a = [];
        for (var i = 0; i < v.length; i++) a.push(ser(v[i], depth + 1));
        return "[" + a.join(",") + "]";
    }
    var o = [];
    for (var k in v) {
        if (!v.hasOwnProperty(k)) continue;
        o.push(esc(k) + ":" + ser(v[k], depth + 1));
    }
    return "{" + o.join(",") + "}";
}

function attempt(fn, fallback) {
    try { return fn(); } catch (e) { return fallback === undefined ? ("<err: " + e + ">") : fallback; }
}

function colorInfo(c) {
    if (!c) return null;
    var o = { type: attempt(function () { return c.typename; }) };
    switch (o.type) {
        case "CMYKColor": o.cyan = c.cyan; o.magenta = c.magenta; o.yellow = c.yellow; o.black = c.black; break;
        case "RGBColor": o.red = c.red; o.green = c.green; o.blue = c.blue; break;
        case "GrayColor": o.gray = c.gray; break;
        case "LabColor": o.l = c.l; o.a = c.a; o.b = c.b; break;
        case "SpotColor": o.spotName = attempt(function () { return c.spot.name; }); o.tint = attempt(function () { return c.tint; }); o.spotKind = attempt(function () { return String(c.spot.spotKind); }); o.spotColor = attempt(function () { return colorInfo(c.spot.color); }); break;
        case "GradientColor": o.gradientName = attempt(function () { return c.gradient.name; }); break;
        case "PatternColor": o.patternName = attempt(function () { return c.pattern.name; }); break;
        case "NoColor": break;
    }
    return o;
}

function run() {
    var job = new File("/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/illustrator/job.txt");
    job.open("r");
    var docPath = job.readln();
    var outPath = job.readln();
    var mode = job.readln();
    job.close();

    app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;

    var report = { docPath: docPath, mode: mode, startedAt: new Date().toString() };
    var t0 = new Date().getTime();
    var doc = null;
    try {
        doc = app.open(new File(docPath));
    } catch (e) {
        report.openError = String(e);
        writeOut(outPath, report);
        return;
    }
    report.openSeconds = (new Date().getTime() - t0) / 1000;

    report.document = {
        name: attempt(function () { return doc.name; }),
        colorSpace: attempt(function () { return String(doc.documentColorSpace); }),
        colorProfileName: attempt(function () { return doc.colorProfileName; }),
        rulerUnits: attempt(function () { return String(doc.rulerUnits); }),
        rulerOrigin: attempt(function () { return [doc.rulerOrigin[0], doc.rulerOrigin[1]]; }),
        width: attempt(function () { return doc.width; }),
        height: attempt(function () { return doc.height; }),
        cropStyle: attempt(function () { return String(doc.cropStyle); }),
        showPlacedImages: attempt(function () { return doc.showPlacedImages; }),
        printTiles: attempt(function () { return doc.printTiles; }),
        splitLongPaths: attempt(function () { return doc.splitLongPaths; }),
        useDefaultScreen: attempt(function () { return doc.useDefaultScreen; }),
        outputResolution: attempt(function () { return doc.outputResolution; }),
        geometricBounds: attempt(function () { return [doc.geometricBounds[0], doc.geometricBounds[1], doc.geometricBounds[2], doc.geometricBounds[3]]; }),
        stationery: attempt(function () { return doc.stationery; }),
        scaleFactor: attempt(function () { return doc.scaleFactor; })
    };

    report.rasterEffectSettings = attempt(function () {
        var r = doc.rasterEffectSettings;
        return {
            resolution: r.resolution,
            colorModel: String(r.colorModel),
            antiAliasing: r.antiAliasing,
            transparency: r.transparency,
            convertSpotColors: r.convertSpotColors,
            preserveSpotColors: attempt(function () { return r.preserveSpotColors; }),
            clippingMask: r.clippingMask,
            padding: r.padding
        };
    });

    report.artboards = attempt(function () {
        var a = [], abs = doc.artboards;
        for (var i = 0; i < abs.length; i++) {
            var ab = abs[i], r = ab.artboardRect;
            a.push({
                index: i, name: ab.name,
                rect: [r[0], r[1], r[2], r[3]],
                widthPt: r[2] - r[0], heightPt: r[1] - r[3],
                rulerOrigin: attempt(function () { return [ab.rulerOrigin[0], ab.rulerOrigin[1]]; }),
                showCenter: attempt(function () { return ab.showCenter; }),
                showCrossHairs: attempt(function () { return ab.showCrossHairs; }),
                showSafeAreas: attempt(function () { return ab.showSafeAreas; })
            });
        }
        return a;
    });
    report.artboardCount = attempt(function () { return doc.artboards.length; });

    // Swatch groups first, to map membership
    var memberOf = {};
    report.swatchGroups = attempt(function () {
        var g = [], sgs = doc.swatchGroups;
        for (var i = 0; i < sgs.length; i++) {
            var sg = sgs[i], names = [];
            var sw = attempt(function () { return sg.getAllSwatches(); }, []);
            for (var j = 0; j < sw.length; j++) {
                var nm = attempt(function () { return sw[j].name; }, "?");
                names.push(nm);
                memberOf[nm] = sg.name;
            }
            g.push({ name: sg.name, count: names.length, swatches: names });
        }
        return g;
    });

    report.swatches = attempt(function () {
        var a = [], sws = doc.swatches;
        for (var i = 0; i < sws.length; i++) {
            var s = sws[i];
            a.push({
                name: attempt(function () { return s.name; }),
                color: attempt(function () { return colorInfo(s.color); }),
                group: memberOf[attempt(function () { return s.name; }, "")] || null
            });
        }
        return a;
    });
    report.swatchCount = attempt(function () { return doc.swatches.length; });

    report.spots = attempt(function () {
        var a = [], sp = doc.spots;
        for (var i = 0; i < sp.length; i++) {
            a.push({ name: sp[i].name, spotKind: attempt(function () { return String(sp[i].spotKind); }), color: attempt(function () { return colorInfo(sp[i].color); }) });
        }
        return a;
    });

    report.gradients = attempt(function () {
        var a = [], gs = doc.gradients;
        for (var i = 0; i < gs.length; i++) {
            var g = gs[i], stops = [];
            var st = attempt(function () { return g.gradientStops; }, []);
            for (var j = 0; j < st.length; j++) {
                stops.push({ rampPoint: attempt(function () { return st[j].rampPoint; }), midPoint: attempt(function () { return st[j].midPoint; }), opacity: attempt(function () { return st[j].opacity; }), color: attempt(function () { return colorInfo(st[j].color); }) });
            }
            a.push({ name: g.name, type: attempt(function () { return String(g.type); }), stops: stops });
        }
        return a;
    });

    report.patterns = attempt(function () {
        var a = [], ps = doc.patterns;
        for (var i = 0; i < ps.length; i++) a.push({ name: attempt(function () { return ps[i].name; }) });
        return a;
    });

    report.brushes = attempt(function () {
        var a = [], bs = doc.brushes;
        for (var i = 0; i < bs.length; i++) a.push({ name: attempt(function () { return bs[i].name; }) });
        return a;
    });

    report.symbols = attempt(function () {
        var a = [], ss = doc.symbols;
        for (var i = 0; i < ss.length; i++) a.push({ name: attempt(function () { return ss[i].name; }), registrationPoint: attempt(function () { return String(ss[i].registrationPoint); }) });
        return a;
    });

    report.graphicStyles = attempt(function () {
        var a = [], gs = doc.graphicStyles;
        for (var i = 0; i < gs.length; i++) a.push({ name: attempt(function () { return gs[i].name; }) });
        return a;
    });

    report.characterStyles = attempt(function () {
        var a = [], cs = doc.characterStyles;
        for (var i = 0; i < cs.length; i++) {
            var c = cs[i];
            a.push({
                name: attempt(function () { return c.name; }),
                textFont: attempt(function () { return c.characterAttributes.textFont.name; }),
                size: attempt(function () { return c.characterAttributes.size; }),
                leading: attempt(function () { return c.characterAttributes.leading; }),
                tracking: attempt(function () { return c.characterAttributes.tracking; }),
                fillColor: attempt(function () { return colorInfo(c.characterAttributes.fillColor); })
            });
        }
        return a;
    });

    report.paragraphStyles = attempt(function () {
        var a = [], ps = doc.paragraphStyles;
        for (var i = 0; i < ps.length; i++) {
            var p = ps[i];
            a.push({
                name: attempt(function () { return p.name; }),
                justification: attempt(function () { return String(p.paragraphAttributes.justification); }),
                leftIndent: attempt(function () { return p.paragraphAttributes.leftIndent; }),
                spaceBefore: attempt(function () { return p.paragraphAttributes.spaceBefore; }),
                spaceAfter: attempt(function () { return p.paragraphAttributes.spaceAfter; }),
                textFont: attempt(function () { return p.characterAttributes.textFont.name; }),
                size: attempt(function () { return p.characterAttributes.size; }),
                leading: attempt(function () { return p.characterAttributes.leading; })
            });
        }
        return a;
    });

    report.counts = {
        textFrames: attempt(function () { return doc.textFrames.length; }),
        pageItems: attempt(function () { return doc.pageItems.length; }),
        pathItems: attempt(function () { return doc.pathItems.length; }),
        placedItems: attempt(function () { return doc.placedItems.length; }),
        rasterItems: attempt(function () { return doc.rasterItems.length; }),
        symbolItems: attempt(function () { return doc.symbolItems.length; }),
        groupItems: attempt(function () { return doc.groupItems.length; }),
        compoundPathItems: attempt(function () { return doc.compoundPathItems.length; }),
        meshItems: attempt(function () { return doc.meshItems.length; }),
        pluginItems: attempt(function () { return doc.pluginItems.length; }),
        legacyTextItems: attempt(function () { return doc.legacyTextItems.length; }),
        layers: attempt(function () { return doc.layers.length; }),
        fonts: attempt(function () { return app.fonts.length; })
    };

    function layerInfo(ly, depth, deep) {
        var o = {
            name: attempt(function () { return ly.name; }),
            depth: depth,
            visible: attempt(function () { return ly.visible; }),
            locked: attempt(function () { return ly.locked; }),
            printable: attempt(function () { return ly.printable; }),
            preview: attempt(function () { return ly.preview; }),
            opacity: attempt(function () { return ly.opacity; }),
            isTemplate: attempt(function () { return ly.template; }),
            pageItemCount: attempt(function () { return ly.pageItems.length; }),
            sublayerCount: attempt(function () { return ly.layers.length; })
        };
        if (deep) {
            o.items = attempt(function () {
                var out = [], pis = ly.pageItems, cap = 400;
                for (var i = 0; i < pis.length && i < cap; i++) {
                    var pi = pis[i];
                    out.push({
                        typename: attempt(function () { return pi.typename; }),
                        name: attempt(function () { return pi.name; }),
                        note: attempt(function () { return pi.note; }, ""),
                        hidden: attempt(function () { return pi.hidden; }),
                        locked: attempt(function () { return pi.locked; }),
                        bounds: attempt(function () { var b = pi.geometricBounds; return [Math.round(b[0]), Math.round(b[1]), Math.round(b[2]), Math.round(b[3])]; }),
                        contents: attempt(function () { return pi.typename === "TextFrame" ? String(pi.contents).slice(0, 120) : undefined; }, undefined),
                        symbolName: attempt(function () { return pi.typename === "SymbolItem" ? pi.symbol.name : undefined; }, undefined),
                        childCount: attempt(function () { return pi.typename === "GroupItem" ? pi.pageItems.length : undefined; }, undefined)
                    });
                }
                if (pis.length > cap) out.push({ truncatedAfter: cap, total: pis.length });
                return out;
            });
        }
        o.sublayers = attempt(function () {
            var s = [], ls = ly.layers;
            for (var i = 0; i < ls.length; i++) s.push(layerInfo(ls[i], depth + 1, deep));
            return s;
        });
        return o;
    }

    report.layers = attempt(function () {
        var a = [], ls = doc.layers;
        for (var i = 0; i < ls.length; i++) a.push(layerInfo(ls[i], 0, mode === "deep"));
        return a;
    });

    report.totalSeconds = (new Date().getTime() - t0) / 1000;
    report.finishedAt = new Date().toString();

    writeOut(outPath, report);

    try { doc.close(SaveOptions.DONOTSAVECHANGES); } catch (e) { }
}

function writeOut(outPath, report) {
    var f = new File(outPath);
    f.encoding = "UTF-8";
    f.open("w");
    f.write(ser(report, 0));
    f.close();
}

run();
"OK";
