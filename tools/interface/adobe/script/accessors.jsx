// --- [OPERATIONS] ----------------------------------------------------------------------

function spelled(value) {
    var escapes = { '\\': '\\\\', '"': '\\"', '\t': '\\t', '\n': '\\n', '\r': '\\r' };
    var parts = [];
    switch (value === null ? null : value.constructor) {
        case Array:
            for (var index = 0; index < value.length; index++) parts.push(spelled(value[index]));
            return '[' + parts.join(',') + ']';
        case Object:
            for (var key in value) parts.push(spelled(key) + ':' + spelled(value[key]));
            return '{' + parts.join(',') + '}';
        case String:
            return '"' + value.replace(/[\\"\t\n\r]/g, function (character) {
                return escapes[character];
            }) + '"';
        default:
            return String(value);
    }
}

function same(left, right) {
    return spelled(left) === spelled(right);
}

function rounded(channels) {
    var values = [];
    for (var index = 0; index < channels.length; index++) values.push(Math.round(channels[index]));
    return values;
}

// --- [ACCESSORS] -----------------------------------------------------------------------

var Member = {
    holder: function (access) {
        var owner = app[access.owner];
        if (access.item === null) return owner;
        var item = owner.itemByName(access.item);
        return item.isValid ? item : null;
    },
    read: function (access) {
        var holder = Member.holder(access);
        if (holder === null) return null;
        var value = holder[access.name];
        return access.enumeration === null ? value : String(value);
    },
    wanted: function (access, target) {
        return access.enumeration === null ? target : String($.global[access.enumeration][target]);
    },
    write: function (access, target) {
        var holder = Member.holder(access);
        (holder === null ? app[access.owner].add({ name: access.item }) : holder)[access.name] = access.enumeration === null ? target : $.global[access.enumeration][target];
    }
};
