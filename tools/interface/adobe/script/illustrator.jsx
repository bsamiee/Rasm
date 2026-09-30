//@include "accessors.jsx"

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREFERENCES]
function slots(access) {
    var indexes = [];
    for (var index = 1; app.preferences.preferenceExists(access.slot + '_' + index); index++) indexes.push(index);
    return indexes;
}

// --- [PROFILES]
function profiles(access) {
    var seen = {};
    var files = [];
    var indexes = slots(access);
    for (var index = 0; index < indexes.length; index++) {
        var file = app.preferences.getStringPreference(access.slot + '_' + indexes[index]);
        if (!seen[file]) files.push({ slot: indexes[index], file: file });
        seen[file] = true;
    }
    return files;
}

function named(collection, name) {
    for (var index = 0; index < collection.length; index++) if (collection[index].name === name) return collection[index];
    return null;
}

function channels(color) {
    return rounded(color.typename === 'CMYKColor' ? [color.cyan, color.magenta, color.yellow, color.black] : [color.red, color.green, color.blue]);
}

function converted(space, rgb) {
    var purpose = ColorConvertPurpose.defaultpurpose;
    if (space === 'RGB') return rgb;
    return rgb[0] === rgb[1] && rgb[1] === rgb[2]
        ? app.convertSampleColor(ImageColorSpace.GrayScale, app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.GrayScale, purpose), ImageColorSpace.CMYK, purpose)
        : app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.CMYK, purpose);
}

function painted(space, rgb) {
    var values = converted(space, rgb);
    var color;
    if (space === 'CMYK') {
        color = new CMYKColor();
        color.cyan = values[0];
        color.magenta = values[1];
        color.yellow = values[2];
        color.black = values[3];
    } else {
        color = new RGBColor();
        color.red = values[0];
        color.green = values[1];
        color.blue = values[2];
    }
    return color;
}

function held(document, target) {
    var group = named(document.swatchGroups, target.group);
    var members = group === null ? [] : group.getAllSwatches();
    var swatches = [];
    for (var index = 0; index < members.length; index++) swatches.push([members[index].name, channels(members[index].color)]);
    return {
        space: document.documentColorSpace === DocumentColorSpace.CMYK ? 'CMYK' : 'RGB',
        stroke: channels(document.defaultStrokeColor),
        resolution: document.rasterEffectSettings.resolution,
        group: group === null ? null : group.name,
        swatches: swatches
    };
}

function wanted(space, target) {
    var swatches = [];
    for (var index = 0; index < target.swatches.length; index++) swatches.push([target.swatches[index][0], rounded(converted(space, target.swatches[index][1]))]);
    return { space: space, stroke: rounded(converted(space, target.stroke)), resolution: target.resolution, group: target.group, swatches: swatches };
}

function written(document, space, target) {
    document.defaultStrokeColor = painted(space, target.stroke);
    document.rasterEffectSettings.resolution = target.resolution;
    var group = named(document.swatchGroups, target.group);
    if (group !== null) group.remove();
    var made = document.swatchGroups.add();
    made.name = target.group;
    for (var index = 0; index < target.swatches.length; index++) {
        var swatch = named(document.swatches, target.swatches[index][0]);
        var placed = swatch === null ? document.swatches.add() : swatch;
        placed.name = target.swatches[index][0];
        placed.color = painted(space, target.swatches[index][1]);
        made.addSwatch(placed);
    }
}

// --- [ACCESSORS] -----------------------------------------------------------------------

var accessors = {
    Preference: {
        read: function (access) {
            return app.preferences['get' + access.kind + 'Preference'](access.key);
        },
        write: function (access, target) {
            app.preferences['set' + access.kind + 'Preference'](access.key, target);
        }
    },
    Slots: {
        rows: function (row) {
            var indexes = slots(row.access);
            var rows = [];
            for (var index = 0; index < indexes.length; index++) {
                rows.push({ label: row.label.replace('<n>', indexes[index]), access: { type: 'Preference', kind: 'Integer', key: row.access.key + '_' + indexes[index] }, target: row.target });
            }
            return rows;
        }
    },
    Profile: {
        rows: function (row) {
            var files = profiles(row.access);
            var rows = [];
            for (var index = 0; index < files.length; index++) {
                rows.push({ label: row.label.replace('<n>', files[index].slot), access: { type: 'Document', file: files[index].file }, target: row.target });
            }
            return rows;
        }
    },
    Document: {
        read: function (access, target) {
            var document = app.open(new File(access.file));
            var record = held(document, target);
            document.close(SaveOptions.DONOTSAVECHANGES);
            return record;
        },
        wanted: function (access, target, record) {
            return wanted(record.space, target);
        },
        write: function (access, target, record) {
            var document = app.open(new File(access.file));
            written(document, record.space, target);
            document.close(SaveOptions.SAVECHANGES);
        }
    },
    Active: {
        read: function () {
            return app.preferences.getStringPreference('plugin/WorkspacePrefix/Last Used Workspace Name');
        },
        write: function (access, target) {
            app.switchWorkspace(target);
        }
    }
};

// --- [COMPOSITION] ---------------------------------------------------------------------

var product = {
    header: function () {
        return [app.version, new File(app.preferences.getStringPreference('startupFileType')).parent.parent.fsName];
    },
    scoped: function (body) {
        var level = app.userInteractionLevel;
        app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
        try {
            return body();
        } finally {
            app.userInteractionLevel = level;
        }
    },
    state: function (document) {
        return { untitled: document.path.fsName === '', modified: !document.saved };
    },
    drop: SaveOptions.DONOTSAVECHANGES
};

//@include "request.jsx"
