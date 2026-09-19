/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, each, failure, ink, items, present, run, select, split }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const syncSwatches = (
    request:
        | { readonly mode: 'read'; readonly documents: string[] }
        | { readonly mode: 'write'; readonly documents: { readonly path: string; readonly renames: { readonly from: string; readonly to: string; readonly temporary: string }[] }[] },
    at: Site,
): Reading<JsonObject> => {
    const retained = items(app.documents);
    const active = retained.length > 0 ? [app.activeDocument] : [];
    const reading =
        request.mode === 'read'
            ? each(at, request.documents, (path, site): Reading<JsonObject[]> => {
                  const document = app.open(new File(path));
                  let colors: Reading<JsonObject[]>;
                  try {
                      colors = each(
                          site,
                          select(items(document.spots), (spot): boolean => spot.colorType !== ColorModel.REGISTRATION),
                          (spot): Reading<JsonObject> => present({ document: path, name: spot.name, ink: ink(spot) }),
                      );
                  } catch (error) {
                      colors = { value: [], unavailable: [failure(error, site)] };
                  }
                  const closed = each(site, contains(retained, document) ? [] : [document], (opened): Reading<null> => {
                      opened.close(SaveOptions.DONOTSAVECHANGES);
                      return present(null);
                  });
                  return { value: colors.value, unavailable: colors.unavailable.concat(closed.unavailable) };
              })
            : each(at, request.documents, ({ path, renames }, site): Reading<JsonObject[]> => {
                  const changes = select(renames, (rename): boolean => rename.from !== rename.to);
                  if (changes.length === 0) {
                      return present([]);
                  }
                  const document = app.open(new File(path));
                  const held = collect(changes, (rename) => ({ rename, spot: document.spots.getByName(rename.from) }));
                  const moving = collect(held, ({ rename }) => rename.from);
                  const names = collect(items(document.swatches), ({ name }): string => name);
                  const occupied = select(names, (name): boolean => !contains(moving, name));
                  const conflicts = select(held, ({ rename }): boolean => contains(occupied, rename.to) || contains(names, rename.temporary));
                  if (conflicts.length > 0) {
                      return present(collect(changes, (rename): JsonObject => ({ document: path, name: rename.from, reason: 'renameConflict' })));
                  }
                  const staged = each(site, held, ({ rename, spot }): Reading<null> => {
                      spot.name = rename.temporary;
                      return present(null);
                  });
                  const written =
                      staged.unavailable.length > 0
                          ? staged
                          : each(site, held, ({ rename, spot }): Reading<null> => {
                                spot.name = rename.to;
                                return present(null);
                            });
                  if (written.unavailable.length > 0) {
                      const reset = each(site, held, ({ rename, spot }): Reading<null> => {
                          spot.name = rename.temporary;
                          return present(null);
                      });
                      const restored = each(site, held, ({ rename, spot }): Reading<null> => {
                          spot.name = rename.from;
                          return present(null);
                      });
                      return { value: [], unavailable: written.unavailable.concat(reset.unavailable, restored.unavailable) };
                  }
                  return each(
                      site,
                      held,
                      ({ rename, spot }): Reading<JsonObject> =>
                          present(spot.name === rename.to ? { document: path, from: rename.from, to: spot.name } : { document: path, name: rename.from, reason: 'readbackDiffers' }),
                  );
              });
    const restored = each(at, active, (document): Reading<null> => {
        app.activeDocument = document;
        return present(null);
    });
    const rows: JsonObject[] = Array.prototype.concat.apply([], reading.value);
    return { value: request.mode === 'read' ? { kind: 'colors', rows } : split(rows), unavailable: reading.unavailable.concat(restored.unavailable) };
};

run(syncSwatches);
