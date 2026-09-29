(function (table) {
    // --- [OPERATIONS] ------------------------------------------------------------------

    // --- [PRODUCTS]
    function targeted() {
        var reference = new ActionReference();
        reference.putProperty(stringIDToTypeID('property'), stringIDToTypeID('targetLayersIDs'));
        reference.putEnumerated(stringIDToTypeID('document'), stringIDToTypeID('ordinal'), stringIDToTypeID('targetEnum'));
        return executeActionGet(reference).getList(stringIDToTypeID('targetLayersIDs')).count;
    }

    var products = {
        illustrator: {
            Tool: function (command) {
                return function () {
                    app.selectTool(command.id);
                };
            },
            Menu: function (command) {
                return function () {
                    app.executeMenuCommand(command.id);
                };
            },
            selected: function () {
                return app.documents.length > 0 && app.activeDocument.selection.length > 0;
            }
        },
        photoshop: {
            Tool: function (command) {
                return function () {
                    app.currentTool = command.id;
                };
            },
            Menu: function (command) {
                var event = stringIDToTypeID(command.id);
                return function () {
                    app.runMenuItem(event);
                };
            },
            selected: function () {
                return app.documents.length > 0 && targeted() > 0;
            }
        },
        indesign: {
            Tool: function (command) {
                var tool = UITools[command.id];
                return function () {
                    app.toolBoxTools.currentTool = tool;
                };
            },
            Menu: function (command) {
                var action = app.menuActions.itemByID(command.id);
                return function () {
                    action.invoke();
                };
            },
            selected: function () {
                return app.documents.length > 0 && app.selection.length > 0;
            }
        }
    };

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
        var last = $.getenv(table.title);
        var chosen = '';
        var dialog = new Window('dialog', table.title);
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
                for (var index = 0; index < names.length; index++) listing.add('statictext', undefined, names[index] + '  ' + commands[names[index]].label).enabled = acts(commands[names[index]]);
            }
            dialog.layout.layout(true);
        }

        function choose(name) {
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
            if (spaced) choose(field.text || last);
        };
        run.onClick = function () {
            choose(field.text || last);
        };
        show('');
        field.active = true;
        dialog.show();
        if (chosen === '') return;
        $.setenv(table.title, chosen);
        commands[chosen].run();
    }

    // --- [COMPOSITION] -----------------------------------------------------------------

    var product = products[table.product];
    var commands = {};
    for (var alias in table.commands) {
        var command = table.commands[alias][1];
        commands[alias] = { label: table.commands[alias][0], run: product[command.type](command), selection: command.selection };
    }
    return prompt();
})
