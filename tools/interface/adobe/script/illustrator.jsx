//@include "accessors.jsx"

// --- [CONSTANTS] -----------------------------------------------------------------------

var CHANNELS = { RGB: ['red', 'green', 'blue'], CMYK: ['cyan', 'magenta', 'yellow', 'black'] };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PREFERENCES]
function slots(access) {
    var indexes = [];
    for (var index = 1; app.preferences.preferenceExists(access.slot + '_' + index); index++) indexes.push(index);
    return indexes;
}

function savedSet(name) {
    for (var index = 1; index <= app.preferences.getIntegerPreference('plugin/Action/SavedSetCount'); index++) {
        var set = 'plugin/Action/SavedSets/set-' + index;
        if (app.preferences.getStringPreference(set + '/name') === name) return set;
    }
    return null;
}

// --- [PROFILES]
function profiles(access) {
    var seen = {};
    var files = [];
    var indexes = slots(access);
    for (var index = 0; index < indexes.length; index++) {
        var file = app.preferences.getStringPreference(access.slot + '_' + indexes[index]);
        if (!seen[file]) files.push(file);
        seen[file] = true;
    }
    return files;
}

function named(collection, name) {
    for (var index = 0; index < collection.length; index++) if (collection[index].name === name) return collection[index];
    return null;
}

function channels(color) {
    var names = CHANNELS[color.typename === 'CMYKColor' ? 'CMYK' : 'RGB'];
    var values = [];
    for (var index = 0; index < names.length; index++) values.push(color[names[index]]);
    return rounded(values);
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
    var color = space === 'CMYK' ? new CMYKColor() : new RGBColor();
    for (var index = 0; index < CHANNELS[space].length; index++) color[CHANNELS[space][index]] = values[index];
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

function rewrite(document, stored, target) {
    var declared = wanted(stored.space, target);
    if (!same(stored.stroke, declared.stroke)) document.defaultStrokeColor = painted(stored.space, target.stroke);
    if (!same(stored.resolution, declared.resolution)) document.rasterEffectSettings.resolution = target.resolution;
    if (same([stored.group, stored.swatches], [declared.group, declared.swatches])) return;
    var group = named(document.swatchGroups, target.group);
    if (group !== null) group.remove();
    var made = document.swatchGroups.add();
    made.name = target.group;
    for (var index = 0; index < target.swatches.length; index++) {
        var swatch = named(document.swatches, target.swatches[index][0]);
        var placed = swatch === null ? document.swatches.add() : swatch;
        placed.name = target.swatches[index][0];
        placed.color = painted(stored.space, target.swatches[index][1]);
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
        read: function (access, target) {
            var indexes = slots(access);
            var values = [];
            for (var index = 0; index < indexes.length; index++) values.push(app.preferences.getIntegerPreference(access.key + '_' + indexes[index]));
            for (var slot = 0; slot < values.length; slot++) if (values[slot] !== target) return values;
            return target;
        },
        write: function (access, target) {
            var indexes = slots(access);
            for (var index = 0; index < indexes.length; index++) app.preferences.setIntegerPreference(access.key + '_' + indexes[index], target);
        }
    },
    Profile: {
        read: function (access, target) {
            var files = profiles(access);
            var records = [];
            var differs = false;
            for (var index = 0; index < files.length; index++) {
                var document = app.open(new File(files[index]));
                records.push(held(document, target));
                differs = differs || !same(records[index], wanted(records[index].space, target));
                document.close(SaveOptions.DONOTSAVECHANGES);
            }
            return differs ? records : target;
        },
        write: function (access, target, artifacts, records) {
            var files = profiles(access);
            for (var index = 0; index < files.length; index++) {
                if (same(records[index], wanted(records[index].space, target))) continue;
                var document = app.open(new File(files[index]));
                rewrite(document, records[index], target);
                document.close(SaveOptions.SAVECHANGES);
            }
        }
    },
    ActionSet: {
        read: function (access) {
            var set = savedSet(access.name);
            return set === null ? null : app.preferences.getIntegerPreference(set + '/action-1/keyIndex');
        },
        write: function (access, target, artifacts, stored) {
            if (stored !== null) app.unloadAction(access.name, '');
            app.loadAction(new File(artifacts + '/' + access.file));
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
    scoped: [{ owner: app, name: 'userInteractionLevel', value: UserInteractionLevel.DONTDISPLAYALERTS }],
    untitled: function (document) {
        return document.path.fsName === '';
    },
    modified: function (document) {
        return !document.saved;
    },
    drop: SaveOptions.DONOTSAVECHANGES
};

//@include "request.jsx"
