(function (request) {
    // --- [OPERATIONS] -------------------------------------------------------------------

    // --- [VALUES]
    function spelled(value) {
        if (value instanceof Array) {
            var cells = [];
            for (var index = 0; index < value.length; index++) cells.push(spelled(value[index]));
            return '[' + cells.join(',') + ']';
        }
        if (value !== null && typeof value === 'object' && value.constructor === Object) {
            var members = [];
            for (var key in value) members.push(key + ':' + spelled(value[key]));
            return '{' + members.join(',') + '}';
        }
        return typeof value === 'number' ? String(Number(value.toFixed(10))) : String(value);
    }

    function same(left, right) {
        return spelled(left) === spelled(right);
    }

    function bytes(channels) {
        var rounded = [];
        for (var index = 0; index < channels.length; index++) rounded.push(Math.round(channels[index]));
        return rounded;
    }

    function repeated(value, count) {
        var values = [];
        for (var index = 0; index < count; index++) values.push(value);
        return values;
    }

    // --- [ILLUSTRATOR]
    function slotted(path, key) {
        var keys = [];
        for (var index = 1; app.preferences.preferenceExists(path.slot + '_' + index); index++) keys.push(key + '_' + index);
        return keys;
    }

    function startupFiles(path) {
        var keys = slotted(path, path.slot);
        var named = {};
        var files = [];
        for (var index = 0; index < keys.length; index++) {
            var file = app.preferences.getStringPreference(keys[index]);
            if (!named[file]) files.push(file);
            named[file] = true;
        }
        return files;
    }

    function namedIn(collection, name) {
        for (var index = 0; index < collection.length; index++) if (collection[index].name === name) return collection[index];
        return null;
    }

    function spaceOf(document) {
        return document.documentColorSpace === DocumentColorSpace.CMYK ? 'CMYK' : 'RGB';
    }

    function channelsOf(color) {
        return color.typename === 'CMYKColor' ? bytes([color.cyan, color.magenta, color.yellow, color.black]) : bytes([color.red, color.green, color.blue]);
    }

    function converted(space, rgb) {
        var purpose = ColorConvertPurpose.defaultpurpose;
        var achromatic = rgb[0] === rgb[1] && rgb[1] === rgb[2];
        if (space !== 'CMYK') return rgb;
        return achromatic ? app.convertSampleColor(ImageColorSpace.GrayScale, app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.GrayScale, purpose), ImageColorSpace.CMYK, purpose) : app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.CMYK, purpose);
    }

    function painted(space, rgb) {
        var values = converted(space, rgb);
        var color = space === 'CMYK' ? new CMYKColor() : new RGBColor();
        var fields = space === 'CMYK' ? ['cyan', 'magenta', 'yellow', 'black'] : ['red', 'green', 'blue'];
        for (var index = 0; index < fields.length; index++) color[fields[index]] = values[index];
        return color;
    }

    function heldTemplate(document, target) {
        var group = namedIn(document.swatchGroups, target.library.name);
        var swatches = group === null ? [] : group.getAllSwatches();
        var held = [];
        for (var swatch = 0; swatch < swatches.length; swatch++) held.push([swatches[swatch].name, channelsOf(swatches[swatch].color)]);
        return {
            space: spaceOf(document),
            stroke: channelsOf(document.defaultStrokeColor),
            resolution: document.rasterEffectSettings.resolution,
            library: group === null ? request.absent : { name: group.name, swatches: held }
        };
    }

    function templated(held, target) {
        var swatches = [];
        for (var index = 0; index < target.library.swatches.length; index++) swatches.push([target.library.swatches[index][0], bytes(converted(held.space, target.library.swatches[index][1]))]);
        return {
            space: held.space,
            stroke: bytes(converted(held.space, target.stroke)),
            resolution: target.resolution,
            library: { name: target.library.name, swatches: swatches }
        };
    }

    function writeTemplate(document, held, target) {
        var wanted = templated(held, target);
        if (!same(held.stroke, wanted.stroke)) document.defaultStrokeColor = painted(held.space, target.stroke);
        if (!same(held.resolution, wanted.resolution)) document.rasterEffectSettings.resolution = target.resolution;
        if (!same(held.library, wanted.library)) {
            var group = namedIn(document.swatchGroups, target.library.name);
            if (group !== null) group.remove();
            var made = document.swatchGroups.add();
            made.name = target.library.name;
            for (var index = 0; index < target.library.swatches.length; index++) {
                var named = namedIn(document.swatches, target.library.swatches[index][0]);
                var swatch = named === null ? document.swatches.add() : named;
                swatch.name = target.library.swatches[index][0];
                swatch.color = painted(held.space, target.library.swatches[index][1]);
                made.addSwatch(swatch);
            }
        }
    }

    // --- [PHOTOSHOP]
    function applicationReference(owner) {
        var reference = new ActionReference();
        reference.putProperty(charIDToTypeID('Prpr'), stringIDToTypeID(owner));
        reference.putEnumerated(charIDToTypeID('capp'), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        return reference;
    }

    function heldApplication(owner) {
        return executeActionGet(applicationReference(owner));
    }

    function readNode(descriptor, key, node) {
        var id = stringIDToTypeID(key);
        if (!descriptor || !descriptor.hasKey(id)) return request.absent;
        switch (node[0]) {
            case 'object':
                var object = descriptor.getObjectValue(id);
                var members = {};
                for (var member in node[2]) members[member] = readNode(object, member, node[2][member]);
                return [node[0], node[1], members];
            case 'boolean':
                return [node[0], descriptor.getBoolean(id)];
            case 'integer':
                return [node[0], descriptor.getInteger(id)];
            case 'double':
                return [node[0], descriptor.getDouble(id)];
            case 'unitDouble':
                return [node[0], descriptor.getUnitDoubleValue(id)];
            case 'enumerated':
                return [node[0], node[1], typeIDToStringID(descriptor.getEnumerationValue(id))];
            case 'RGBColor':
                var color = descriptor.getObjectValue(id);
                return [node[0]].concat(bytes([color.getDouble(stringIDToTypeID('red')), color.getDouble(stringIDToTypeID('grain')), color.getDouble(stringIDToTypeID('blue'))]));
        }
    }

    function putNode(descriptor, key, node, held, name) {
        var id = stringIDToTypeID(key);
        switch (node[0]) {
            case 'object':
                var object = new ActionDescriptor();
                var inner = held && held.hasKey(stringIDToTypeID(name)) ? held.getObjectValue(stringIDToTypeID(name)) : null;
                for (var member in node[2]) putNode(object, member, node[2][member], inner, member);
                return descriptor.putObject(id, stringIDToTypeID(node[1]), object);
            case 'boolean':
                return descriptor.putBoolean(id, node[1]);
            case 'integer':
                return descriptor.putInteger(id, node[1]);
            case 'double':
                return descriptor.putDouble(id, node[1]);
            case 'unitDouble':
                if (!held || !held.hasKey(stringIDToTypeID(name))) throw new Error('store holds no unit for ' + name);
                return descriptor.putUnitDouble(id, held.getUnitDoubleType(stringIDToTypeID(name)), node[1]);
            case 'enumerated':
                return descriptor.putEnumerated(id, stringIDToTypeID(node[1]), stringIDToTypeID(node[2]));
            case 'RGBColor':
                var color = new ActionDescriptor();
                color.putDouble(stringIDToTypeID('red'), node[1]);
                color.putDouble(stringIDToTypeID('grain'), node[2]);
                color.putDouble(stringIDToTypeID('blue'), node[3]);
                return descriptor.putObject(id, stringIDToTypeID('RGBColor'), color);
        }
    }

    function setApplication(owner, node) {
        var set = new ActionDescriptor();
        set.putReference(charIDToTypeID('null'), applicationReference(owner));
        putNode(set, 'to', node, heldApplication(owner), owner);
        executeAction(charIDToTypeID('setd'), set, DialogModes.NO);
    }

    function canvasModes(path) {
        var modes = heldApplication(path.owner).getObjectValue(stringIDToTypeID(path.owner)).getList(stringIDToTypeID(path.key));
        var items = [];
        for (var index = 0; index < modes.count; index++) items.push(modes.getObjectValue(index));
        return items;
    }

    function enumeration(descriptor, key) {
        return typeIDToStringID(descriptor.getEnumerationValue(stringIDToTypeID(key)));
    }

    // --- [DOM]
    function owner(path) {
        var names = path.split('.');
        var object = app;
        for (var index = 0; index < names.length - 1; index++) object = object[names[index]];
        return { object: object, name: names[names.length - 1] };
    }

    function readProperty(path) {
        var property = owner(path);
        return property.object[property.name];
    }

    function writeProperty(path, value) {
        var property = owner(path);
        property.object[property.name] = value;
    }

    // --- [INDESIGN]
    function removable(collection, reserved) {
        var fixed = {};
        for (var name = 0; name < reserved.length; name++) fixed[reserved[name]] = true;
        var items = [];
        for (var index = 0; index < collection.length; index++) if (!fixed[collection[index].name]) items.push(collection[index]);
        return items;
    }

    // --- [ACCESSORS]
    var accessors = {
        preference: {
            read: function (path) {
                return app.preferences['get' + path.kind + 'Preference'](path.key);
            },
            write: function (path, target) {
                app.preferences['set' + path.kind + 'Preference'](path.key, target);
            }
        },
        channels: {
            read: function (path) {
                var values = [];
                for (var index = 0; index < path.keys.length; index++) values.push(app.preferences['get' + path.kind + 'Preference'](path.keys[index]) * 255 / path.scale);
                return bytes(values);
            },
            write: function (path, target) {
                for (var index = 0; index < path.keys.length; index++) {
                    var stored = target[index] / 255 * path.scale;
                    app.preferences['set' + path.kind + 'Preference'](path.keys[index], path.kind === 'Integer' ? Math.round(stored) : stored);
                }
            }
        },
        slots: {
            read: function (path) {
                var keys = slotted(path, path.key);
                var values = [];
                for (var index = 0; index < keys.length; index++) values.push(app.preferences.getIntegerPreference(keys[index]));
                return values;
            },
            stored: function (path, target) {
                return repeated(target, slotted(path, path.key).length);
            },
            write: function (path, target) {
                var keys = slotted(path, path.key);
                for (var index = 0; index < keys.length; index++) app.preferences.setIntegerPreference(keys[index], target);
            }
        },
        descriptor: {
            read: function (path, target) {
                return readNode(heldApplication(path.owner), path.owner, target);
            },
            write: function (path, target) {
                setApplication(path.owner, target);
            }
        },
        canvas: {
            read: function (path) {
                var modes = canvasModes(path);
                var values = [];
                for (var index = 0; index < modes.length; index++) values.push([enumeration(modes[index], 'canvasColorMode'), enumeration(modes[index], 'canvasFrame')].concat(readNode(modes[index], 'color', ['RGBColor']).slice(1)));
                return values;
            },
            stored: function (path, target) {
                return repeated(['custom', 'none'].concat(target), canvasModes(path).length);
            },
            write: function (path, target) {
                var modes = canvasModes(path);
                var list = new ActionList();
                for (var index = 0; index < modes.length; index++) {
                    var mode = new ActionDescriptor();
                    mode.putEnumerated(stringIDToTypeID('screenMode'), stringIDToTypeID('canvasScreenMode'), stringIDToTypeID(enumeration(modes[index], 'screenMode')));
                    mode.putEnumerated(stringIDToTypeID('canvasColorMode'), stringIDToTypeID('canvasColorType'), stringIDToTypeID('custom'));
                    mode.putEnumerated(stringIDToTypeID('canvasFrame'), stringIDToTypeID('canvasFrameStyle'), stringIDToTypeID('none'));
                    putNode(mode, 'color', ['RGBColor'].concat(target), null, 'color');
                    list.putObject(stringIDToTypeID('canvasAttributes'), mode);
                }
                var value = new ActionDescriptor();
                value.putList(stringIDToTypeID(path.key), list);
                var set = new ActionDescriptor();
                set.putReference(charIDToTypeID('null'), applicationReference(path.owner));
                set.putObject(stringIDToTypeID('to'), stringIDToTypeID(path.owner), value);
                executeAction(charIDToTypeID('setd'), set, DialogModes.NO);
            }
        },
        property: {
            read: function (path) {
                var value = readProperty(path.path);
                if (path.enumeration) return String(value);
                return value instanceof Array ? bytes(value) : value;
            },
            stored: function (path, target) {
                return path.enumeration ? String($.global[path.enumeration][target]) : target;
            },
            write: function (path, target) {
                writeProperty(path.path, path.enumeration ? $.global[path.enumeration][target] : target);
            }
        },
        preset: {
            read: function (path) {
                var item = app.documentPresets.itemByName(path.preset);
                return item.isValid ? item[path.name] : request.absent;
            },
            write: function (path, target) {
                var item = app.documentPresets.itemByName(path.preset);
                (item.isValid ? item : app.documentPresets.add({ name: path.preset }))[path.name] = target;
            }
        },
        presets: {
            read: function (path) {
                var presets = removable(app.documentPresets, path.reserved);
                var names = [];
                for (var index = 0; index < presets.length; index++) names.push(presets[index].name);
                return names;
            },
            write: function (path, target) {
                var kept = {};
                for (var index = 0; index < target.length; index++) kept[target[index]] = true;
                var presets = removable(app.documentPresets, path.reserved);
                for (var item = presets.length - 1; item >= 0; item--) if (!kept[presets[item].name]) presets[item].remove();
            }
        },
        panel: {
            read: function (path) {
                return String(app.panels.itemByName(path.panel)[path.member]);
            },
            stored: function (path, target) {
                return String($.global[path.enumeration][target]);
            },
            write: function (path, target) {
                app.panels.itemByName(path.panel)[path.member] = $.global[path.enumeration][target];
            }
        },
        swatches: {
            read: function (path) {
                var swatches = removable(app.swatches, path.reserved);
                var values = [];
                for (var index = 0; index < swatches.length; index++) values.push([swatches[index].name, swatches[index].space === ColorSpace.RGB ? bytes(swatches[index].colorValue) : swatches[index].colorValue]);
                return values;
            },
            write: function (path, target) {
                var kept = {};
                for (var index = 0; index < target.length; index++) kept[target[index][0]] = target[index][1];
                var swatches = removable(app.swatches, path.reserved);
                for (var item = swatches.length - 1; item >= 0; item--) if (!kept[swatches[item].name]) swatches[item].remove();
                for (var swatch = 0; swatch < target.length; swatch++) {
                    var color = app.colors.itemByName(target[swatch][0]);
                    (color.isValid ? color : app.colors.add({ name: target[swatch][0] })).properties = { model: ColorModel.PROCESS, space: ColorSpace.RGB, colorValue: target[swatch][1] };
                }
            }
        },
        active: {
            read: function () {
                return product.workspace();
            },
            write: function (path, target) {
                app[path.command](target);
            }
        },
        profile: {
            read: function (path, target) {
                var files = startupFiles(path);
                var values = [];
                for (var index = 0; index < files.length; index++) {
                    var document = app.open(new File(files[index]));
                    values.push(heldTemplate(document, target));
                    document.close(SaveOptions.DONOTSAVECHANGES);
                }
                return values;
            },
            stored: function (path, target, before) {
                var values = [];
                for (var index = 0; index < before.length; index++) values.push(templated(before[index], target));
                return values;
            },
            write: function (path, target, before) {
                var files = startupFiles(path);
                for (var index = 0; index < files.length; index++) {
                    if (same(before[index], templated(before[index], target))) continue;
                    var document = app.open(new File(files[index]));
                    writeTemplate(document, before[index], target);
                    document.close(SaveOptions.SAVECHANGES);
                }
            }
        }
    };

    // --- [PRODUCTS]
    var products = {
        illustrator: function () {
            return {
                header: function () {
                    return [app.version, request.settings];
                },
                quiet: [{ owner: app, name: 'userInteractionLevel', value: UserInteractionLevel.DONTDISPLAYALERTS }],
                untitled: function (item) {
                    return item.path.fsName === '';
                },
                workspace: function () {
                    return app.preferences.getStringPreference('plugin/WorkspacePrefix/Last Used Workspace Name');
                },
                keep: SaveOptions.SAVECHANGES,
                drop: SaveOptions.DONOTSAVECHANGES
            };
        },
        photoshop: function () {
            return {
                header: function () {
                    return [app.version, app.preferencesFolder.fsName];
                },
                quiet: [{ owner: app, name: 'displayDialogs', value: DialogModes.NO }],
                untitled: function (item) {
                    var reference = new ActionReference();
                    reference.putIdentifier(stringIDToTypeID('document'), item.id);
                    return !executeActionGet(reference).hasKey(stringIDToTypeID('fileReference'));
                },
                keep: SaveOptions.SAVECHANGES,
                drop: SaveOptions.DONOTSAVECHANGES
            };
        },
        indesign: function () {
            return {
                header: function () {
                    return [app.version, app.scriptPreferences.scriptsFolder.parent.parent.fsName];
                },
                quiet: [
                    { owner: app.scriptPreferences, name: 'userInteractionLevel', value: UserInteractionLevels.NEVER_INTERACT },
                    { owner: app.scriptPreferences, name: 'measurementUnit', value: MeasurementUnits.POINTS }
                ],
                untitled: function (item) {
                    return !item.saved;
                },
                workspace: function () {
                    return app.generalPreferences.setActiveWorkspace;
                },
                keep: SaveOptions.YES,
                drop: SaveOptions.NO
            };
        }
    };

    // --- [REPORT]
    function line(cells) {
        return cells.join('\t');
    }

    function raised(step, error) {
        return line([request.kinds.error, [step, 'raised', error, 'at line', error.line].join(' ')]);
    }

    function converged(row) {
        var access = accessors[row.path.access];
        var before = access.read(row.path, row.target);
        var target = access.stored ? access.stored(row.path, row.target, before) : row.target;
        if (same(before, target)) return [];
        access.write(row.path, row.target, before);
        return [line([request.kinds.change, row.label, spelled(before), spelled(target)])];
    }

    function release(product) {
        for (var index = app.documents.length - 1; index >= 0; index--) app.documents[index].close(product.untitled(app.documents[index]) ? product.drop : product.keep);
    }

    function report(product) {
        var lines = [];
        try {
            lines.push(line([request.kinds.header].concat(product.header())));
        } catch (error) {
            lines.push(raised('header', error));
        }
        for (var index = 0; index < request.rows.length; index++) {
            try {
                lines = lines.concat(converged(request.rows[index]));
            } catch (error) {
                lines.push(raised(request.rows[index].label, error));
            }
        }
        try {
            release(product);
        } catch (error) {
            lines.push(raised('document release', error));
        }
        return lines.join('\n');
    }

    // --- [COMPOSITION] ------------------------------------------------------------------

    var product = products[request.application]();
    var held = [];
    for (var setting = 0; setting < product.quiet.length; setting++) {
        var quiet = product.quiet[setting];
        held.push(quiet.owner[quiet.name]);
        quiet.owner[quiet.name] = quiet.value;
    }
    try {
        return report(product);
    } finally {
        for (var restored = product.quiet.length - 1; restored >= 0; restored--) product.quiet[restored].owner[product.quiet[restored].name] = held[restored];
    }
})(eval(['(', arguments[0], ')'].join('')));
