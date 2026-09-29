// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REPORT]
function failed(text) {
    return ['error', text].join('\t');
}

function attempted(step, body) {
    try {
        return body();
    } catch (error) {
        return [failed(step + ' raised ' + error.number + ' ' + spelled(error.message) + ' at line ' + error.line)];
    }
}

function changes(label, held, target) {
    var lines = [];
    if (held !== null && held.constructor === Object && target.constructor === Object) {
        for (var key in target) lines = lines.concat(changes(label + '.' + key, held[key], target[key]));
        return lines;
    }
    return same(held, target) ? lines : [['change', label, held === null ? 'absent' : spelled(held), spelled(target)].join('\t')];
}

// --- [REQUESTS]
function converged(row, artifacts) {
    var accessor = accessors[row.access.type];
    var held = accessor.read(row.access, row.target, artifacts);
    if (same(held, row.target)) return [];
    accessor.write(row.access, row.target, artifacts, held);
    return changes(row.label, held, row.target);
}

function converge(request) {
    var lines = attempted('header', function () {
        return [['header'].concat(product.header()).join('\t')];
    });
    for (var index = 0; index < request.rows.length; index++) {
        var row = request.rows[index];
        lines = lines.concat(attempted(row.label, function () {
            return converged(row, request.artifacts);
        }));
    }
    return lines;
}

function release() {
    var paths = [];
    var lines = [];
    for (var index = app.documents.length - 1; index >= 0; index--) {
        var document = app.documents[index];
        if (product.untitled(document)) {
            document.close(product.drop);
        } else {
            var path = document.fullName.fsName;
            paths.unshift(path);
            if (product.modified(document)) lines.unshift(failed(path + ' holds unsaved changes'));
        }
    }
    return [['measurement', spelled(paths)].join('\t')].concat(lines);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

var requests = { Release: release, Converge: converge };

function main(argument) {
    var request = eval('(' + argument + ')');
    var held = [];
    for (var index = 0; index < product.scoped.length; index++) {
        var scope = product.scoped[index];
        held.push(scope.owner[scope.name]);
        scope.owner[scope.name] = scope.value;
    }
    try {
        return attempted(request.type, function () {
            return requests[request.type](request);
        }).join('\n');
    } finally {
        for (var restored = product.scoped.length - 1; restored >= 0; restored--) product.scoped[restored].owner[product.scoped[restored].name] = held[restored];
    }
}

main(arguments[0]);
