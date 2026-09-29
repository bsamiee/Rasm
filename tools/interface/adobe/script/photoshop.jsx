//@include "accessors.jsx"

// --- [CONSTANTS] -----------------------------------------------------------------------

var UNAVAILABLE = -25920;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DESCRIPTORS]
function descriptor(value) {
    var converted = new ActionDescriptor();
    converted.putString(stringIDToTypeID('json'), spelled(value));
    return executeAction(stringIDToTypeID('convertJSONdescriptor'), converted, DialogModes.NO).getObjectValue(stringIDToTypeID('object'));
}

function valued(held) {
    var converted = new ActionDescriptor();
    converted.putObject(stringIDToTypeID('object'), stringIDToTypeID('object'), held);
    return eval('(' + executeAction(stringIDToTypeID('convertJSONdescriptor'), converted, DialogModes.NO).getString(stringIDToTypeID('json')) + ')');
}

function property(owner) {
    return { _ref: [{ _ref: 'property', _property: owner }, { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' }] };
}

function stored(owner) {
    return valued(executeAction(stringIDToTypeID('get'), descriptor({ 'null': property(owner) }), DialogModes.NO))[owner];
}

function projected(held, target) {
    var values;
    if (held.constructor === Array) {
        values = [];
        for (var index = 0; index < target.length; index++) values.push(projected(held[index], target[index]));
        return values;
    }
    if (held.constructor !== Object) return held;
    if (held._obj === 'RGBColor') return rounded([held.red, held.grain, held.blue]);
    if (held.hasOwnProperty('_value')) return held._value;
    values = {};
    for (var key in target) {
        var name = typeIDToStringID(stringIDToTypeID(key));
        values[key] = held.hasOwnProperty(name) ? projected(held[name], target[key]) : null;
    }
    return values;
}

function merged(held, target) {
    var values;
    if (held.constructor === Array) {
        values = [];
        for (var index = 0; index < target.length; index++) values.push(merged(held[index], target[index]));
        return values;
    }
    if (held.constructor !== Object) return target;
    if (held._obj === 'RGBColor') return { _obj: 'RGBColor', red: target[0], grain: target[1], blue: target[2] };
    if (held.hasOwnProperty('_enum')) return { _enum: held._enum, _value: target };
    if (held.hasOwnProperty('_unit')) return { _unit: held._unit, _value: target };
    values = { _obj: held._obj };
    for (var key in target) {
        var name = typeIDToStringID(stringIDToTypeID(key));
        values[name] = merged(held.hasOwnProperty(name) ? held[name] : { _obj: 'RGBColor' }, target[key]);
    }
    return values;
}

// --- [ACTIONS]
function actionSetReference(name) {
    var reference = new ActionReference();
    reference.putName(stringIDToTypeID('actionSet'), name);
    return reference;
}

function contents(file) {
    file.encoding = 'BINARY';
    file.open('r');
    var data = file.read();
    file.close();
    return data;
}

// --- [ACCESSORS] -----------------------------------------------------------------------

var accessors = {
    Member: Member,
    ApplicationProperty: {
        read: function (access, target) {
            return projected(stored(access.owner), target);
        },
        write: function (access, target) {
            executeAction(stringIDToTypeID('set'), descriptor({ 'null': property(access.owner), to: merged(stored(access.owner), target) }), DialogModes.NO);
        }
    },
    ActionSet: {
        read: function (access, target, artifacts) {
            try {
                executeActionGet(actionSetReference(access.name));
            } catch (error) {
                if (error.number !== UNAVAILABLE) throw error;
                return null;
            }
            var palette = contents(new File(app.preferencesFolder.fsName + '/Actions Palette.psp'));
            return palette.indexOf(contents(new File(artifacts + '/' + access.file)).slice(4)) >= 0;
        },
        write: function (access, target, artifacts, held) {
            if (held !== null) {
                var removal = new ActionDescriptor();
                removal.putReference(stringIDToTypeID('null'), actionSetReference(access.name));
                executeAction(stringIDToTypeID('delete'), removal, DialogModes.NO);
            }
            app.load(new File(artifacts + '/' + access.file));
        }
    }
};

// --- [COMPOSITION] ---------------------------------------------------------------------

var product = {
    header: function () {
        return [app.version, app.preferencesFolder.fsName];
    },
    scoped: [{ owner: app, name: 'displayDialogs', value: DialogModes.NO }],
    untitled: function (document) {
        var reference = new ActionReference();
        reference.putIdentifier(stringIDToTypeID('document'), document.id);
        return !executeActionGet(reference).hasKey(stringIDToTypeID('fileReference'));
    },
    modified: function (document) {
        return !document.saved;
    },
    drop: SaveOptions.DONOTSAVECHANGES
};

//@include "request.jsx"
