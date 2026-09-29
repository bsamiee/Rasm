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
    return attempted(row.label, function () {
        if (accessor.rows !== undefined) {
            var lines = [];
            var rows = accessor.rows(row);
            for (var index = 0; index < rows.length; index++) lines = lines.concat(converged(rows[index], artifacts));
            return lines;
        }
        var held = accessor.read(row.access, row.target, artifacts);
        var wanted = accessor.wanted === undefined ? row.target : accessor.wanted(row.access, row.target, held);
        if (same(held, wanted)) return [];
        accessor.write(row.access, row.target, artifacts, held);
        return changes(row.label, held, wanted);
    });
}

function converge(request) {
    var lines = attempted('header', function () {
        return [['header'].concat(product.header()).join('\t')];
    });
    for (var index = 0; index < request.rows.length; index++) lines = lines.concat(converged(request.rows[index], request.artifacts));
    return lines;
}

function release() {
    var paths = [];
    var lines = [];
    for (var index = app.documents.length - 1; index >= 0; index--) {
        var document = app.documents[index];
        var state = product.state(document);
        if (state.untitled) {
            document.close(product.drop);
        } else {
            var path = document.fullName.fsName;
            paths.unshift(path);
            if (state.modified) lines.unshift(failed(path + ' holds unsaved changes'));
        }
    }
    return [['measurement', spelled(paths)].join('\t')].concat(lines);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

var requests = { Release: release, Converge: converge };

function main(argument) {
    var request = eval('(' + argument + ')');
    return product.scoped(function () {
        return attempted(request.type, function () {
            return requests[request.type](request);
        }).join('\n');
    });
}

main(arguments[0]);
