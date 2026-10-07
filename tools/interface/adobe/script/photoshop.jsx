//@include "accessors.jsx"

// --- [OPERATIONS] ----------------------------------------------------------------------

function descriptor(value) {
    var converted = new ActionDescriptor();
    converted.putString(stringIDToTypeID('json'), spelled(value));
    return executeAction(stringIDToTypeID('convertJSONdescriptor'), converted, DialogModes.NO).getObjectValue(stringIDToTypeID('object'));
}

function valued(held) {
    var converted = new ActionDescriptor();
    var object = stringIDToTypeID('object');
    converted.putObject(object, object, held);
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

function colored(channels) {
    return { _obj: 'RGBColor', red: channels[0], grain: channels[1], blue: channels[2] };
}

function merged(held, target) {
    var values;
    if (held === undefined) return target.constructor === Array && typeof target[0] === 'number' ? colored(target) : target;
    if (held.constructor === Array) {
        values = [];
        for (var index = 0; index < target.length; index++) values.push(merged(held[index], target[index]));
        return values;
    }
    if (held.constructor !== Object) return target;
    if (held._obj === 'RGBColor') return colored(target);
    if (held.hasOwnProperty('_enum')) return { _enum: held._enum, _value: target };
    if (held.hasOwnProperty('_unit')) return { _unit: held._unit, _value: target };
    values = { _obj: held._obj };
    for (var key in target) {
        var name = typeIDToStringID(stringIDToTypeID(key));
        values[name] = merged(held[name], target[key]);
    }
    return values;
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
    PaletteFont: {
        read: function () {
            return stored('fontSmallSize');
        },
        write: function (access) {
            accessors.ApplicationProperty.write({ owner: 'interfacePrefs' }, { paletteEnhancedFontTypeKey: access.option });
        }
    }
};

// --- [COMPOSITION] ---------------------------------------------------------------------

var product = {
    header: function () {
        return [app.version, app.preferencesFolder.fsName];
    },
    scoped: function (body) {
        var dialogs = app.displayDialogs;
        app.displayDialogs = DialogModes.NO;
        try {
            return body();
        } finally {
            app.displayDialogs = dialogs;
        }
    },
    state: function (document) {
        var reference = new ActionReference();
        reference.putIdentifier(stringIDToTypeID('document'), document.id);
        return { untitled: !executeActionGet(reference).hasKey(stringIDToTypeID('fileReference')), modified: !document.saved };
    },
    drop: SaveOptions.DONOTSAVECHANGES
};

//@include "request.jsx"
