(function (table) {
    // --- [CONSTANTS] --------------------------------------------------------------------

    var KEY = 'alias';

    // --- [OPERATIONS] -------------------------------------------------------------------

    // --- [PRODUCTS]
    function selectClass(command) {
        var reference = new ActionReference();
        reference.putClass(stringIDToTypeID(command.id));
        var select = new ActionDescriptor();
        select.putReference(charIDToTypeID('null'), reference);
        executeAction(stringIDToTypeID('select'), select, DialogModes.NO);
    }

    function targeted() {
        var reference = new ActionReference();
        reference.putProperty(stringIDToTypeID('property'), stringIDToTypeID('targetLayersIDs'));
        reference.putEnumerated(stringIDToTypeID('document'), stringIDToTypeID('ordinal'), stringIDToTypeID('targetEnum'));
        return executeActionGet(reference).getList(stringIDToTypeID('targetLayersIDs')).count;
    }

    function remembered() {
        return new File(app.preferencesFolder.fsName + '/' + KEY + '.txt');
    }

    function identified(command) {
        return command.id;
    }

    var products = {
        illustrator: {
            title: identified,
            run: {
                tool: function (command) {
                    app.selectTool(command.id);
                },
                menu: function (command) {
                    app.executeMenuCommand(command.id);
                }
            },
            selected: function () {
                return app.documents.length > 0 && app.activeDocument.selection.length > 0;
            },
            last: function () {
                return app.preferences.getStringPreference(KEY);
            },
            remember: function (name) {
                app.preferences.setStringPreference(KEY, name);
            }
        },
        photoshop: {
            title: identified,
            run: {
                tool: selectClass,
                menu: function (command) {
                    app.runMenuItem(stringIDToTypeID(command.id));
                }
            },
            selected: function () {
                return app.documents.length > 0 && targeted() > 0;
            },
            last: function () {
                var file = remembered();
                if (!file.exists) return '';
                file.open('r');
                var name = file.read();
                file.close();
                return name;
            },
            remember: function (name) {
                var file = remembered();
                file.open('w');
                file.write(name);
                file.close();
            }
        },
        indesign: {
            title: function (command) {
                return command.kind === 'menu' ? app.menuActions.itemByID(command.id).name : command.id;
            },
            run: {
                tool: function (command) {
                    app.toolBoxTools.currentTool = UITools[command.id];
                },
                menu: function (command) {
                    app.menuActions.itemByID(command.id).invoke();
                }
            },
            selected: function () {
                return app.documents.length > 0 && app.selection.length > 0;
            },
            last: function () {
                return app.extractLabel(KEY);
            },
            remember: function (name) {
                app.insertLabel(KEY, name);
            }
        }
    };

    var product = products[table.prompt.form];
    var commands = table.prompt.commands;

    // --- [PROMPT]
    function acts(command) {
        return !command.selection || product.selected();
    }

    function matching(prefix) {
        var names = [];
        for (var name in commands) if (name.indexOf(prefix) === 0) names.push(name);
        return names;
    }

    function prompt() {
        var last = product.last();
        var chosen = '';
        var dialog = new Window('dialog', table.label);
        dialog.alignChildren = 'fill';
        var field = dialog.add('edittext', undefined, '');
        field.characters = 8;
        var listing = dialog.add('group');
        listing.orientation = 'column';
        listing.alignChildren = 'left';
        var run = dialog.add('button', undefined, 'Run', { name: 'ok' });

        function show(typed) {
            while (listing.children.length > 0) listing.remove(listing.children[0]);
            if (typed === '') {
                for (var key in table.families) listing.add('statictext', undefined, key + '  ' + table.families[key]);
            } else {
                var names = matching(typed);
                for (var index = 0; index < names.length; index++) listing.add('statictext', undefined, names[index] + '  ' + product.title(commands[names[index]])).enabled = acts(commands[names[index]]);
            }
            dialog.layout.layout(true);
        }

        function runs(name) {
            if (!commands.hasOwnProperty(name) || !acts(commands[name])) return;
            chosen = name;
            dialog.close();
        }

        field.onChanging = function () {
            var typed = field.text.toUpperCase();
            var spaced = typed.charAt(typed.length - 1) === ' ';
            var kept = spaced ? typed.slice(0, -1) : typed;
            field.text = matching(kept).length > 0 ? kept : kept.slice(0, -1);
            show(field.text);
            if (spaced) runs(field.text || last);
        };
        run.onClick = function () {
            runs(field.text || last);
        };
        show('');
        field.active = true;
        dialog.show();
        if (chosen === '') return;
        product.remember(chosen);
        product.run[commands[chosen].kind](commands[chosen]);
    }

    // --- [COMPOSITION] ------------------------------------------------------------------

    return prompt();
})
