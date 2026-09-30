//@include "accessors.jsx"

// --- [ACCESSORS] -----------------------------------------------------------------------

var accessors = {
    Member: Member,
    Item: member(
        function (access) {
            var item = app[access.owner].itemByName(access.item);
            return item.isValid ? item : null;
        },
        function (access) {
            return app[access.owner].add({ name: access.item });
        }
    ),
    Named: {
        read: function (access) {
            var collection = app[access.collection];
            var names = [];
            for (var index = 0; index < collection.length; index++) names.push(collection[index].name);
            return names.sort();
        },
        write: function (access, target, held) {
            var kept = {};
            for (var index = 0; index < target.length; index++) kept[target[index]] = true;
            for (var name = 0; name < held.length; name++) if (!kept[held[name]]) app[access.collection].itemByName(held[name]).remove();
        }
    },
    Active: {
        read: function () {
            return app.generalPreferences.setActiveWorkspace;
        },
        write: function (access, target) {
            app.applyWorkspace(target);
        }
    }
};

// --- [COMPOSITION] ---------------------------------------------------------------------

var product = {
    header: function () {
        return [app.version, app.scriptPreferences.scriptsFolder.parent.parent.fsName];
    },
    scoped: function (body) {
        var preferences = app.scriptPreferences;
        var level = preferences.userInteractionLevel;
        var unit = preferences.measurementUnit;
        preferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
        preferences.measurementUnit = MeasurementUnits.POINTS;
        try {
            return body();
        } finally {
            preferences.measurementUnit = unit;
            preferences.userInteractionLevel = level;
        }
    },
    state: function (document) {
        return { untitled: !document.saved, modified: document.modified };
    },
    drop: SaveOptions.NO
};

//@include "request.jsx"
