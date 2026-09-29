//@include "accessors.jsx"

// --- [ACCESSORS] -----------------------------------------------------------------------

var accessors = {
    Member: Member,
    Named: {
        read: function (access) {
            var collection = app[access.collection];
            var names = [];
            for (var index = 0; index < collection.length; index++) names.push(collection[index].name);
            return names.sort();
        },
        write: function (access, target, artifacts, held) {
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
    scoped: [
        { owner: app.scriptPreferences, name: 'userInteractionLevel', value: UserInteractionLevels.NEVER_INTERACT },
        { owner: app.scriptPreferences, name: 'measurementUnit', value: MeasurementUnits.POINTS }
    ],
    untitled: function (document) {
        return !document.saved;
    },
    modified: function (document) {
        return document.modified;
    },
    drop: SaveOptions.NO
};

//@include "request.jsx"
