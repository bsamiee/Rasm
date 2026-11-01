# [PACKAGING]

## [01]-[MANIFEST]

`blender_manifest.toml` takes rules `extension validate` leaves unchecked:
- Unknown keys and tables (a misspelled `[permission]`) pass validation with no effect
- `[permissions]` names each capability code uses with a reason, validation checking key names and reason text alone
- Permissions grant and enforce nothing, Blender listing title-cased keys without reasons and showing no permission an upgrade adds
- `blender_version_max` excludes its own version
- `license` entries take `SPDX:<identifier>`, validation passing a missing prefix and unknown identifiers

## [02]-[WHEELS]

Wheels of enabled extensions alone install into one shared `<extensions>/.local/lib/<python>/site-packages`:
- Blender reads no wheel `METADATA`, and manifests list a wheel per closure distribution, except ones Blender bundles at an accepted version
- Shared folders present at startup sit ahead of Blender's `site-packages`, and a session's first wheel install appends the folder last until restart
- Wheels ahead of Blender's `site-packages` replace bundled copies process-wide
- Shared folders join `sys.path` without `site.addsitedir`, and a `.pth` file there adds no path and runs no code
- Distributions, keyed by wheel file name before its first `-`, resolve to the highest version any enabled extension bundles
- Versions rank by major, minor, and patch, then the tail as text, `1.10` reading as `1.0.10` below `1.9.0` and `1.0.0rc1` outranking `1.0.0`
- First imports of a session hold until restart, a later install or disable rewriting files while `sys.modules` keeps the imported version
- Enabled-set changes (checkbox, `default_set=True` calls, uninstall, failed enables) resync the folder at once, distributions falling back or leaving
- Syncs extract a wheel whose `<name>-<version>.dist-info` folder is absent, a wheel rebuilt at that version keeping the first extraction
- First extractions stay until an enabled-set change leaves no enabled extension listing the wheel, an upgrade install keeping them
- Wheels holding no `.dist-info/RECORD` skip with no message
- Wheels for another platform skip at enable, and an import of their module raises `ModuleNotFoundError`
- `py3` and `cp3` tags match every 3.x, `cpXY` X.Y alone, and `abi3` X.Y onward, extensions with no matching wheel marked incompatible
- Wheels for macOS match by architecture or `universal2`, whatever OS version their tag names

## [03]-[SUBPACKAGES]

Extensions import as `bl_ext.<repo>.<id>`, and no extension folder joins `sys.path`:
- Package modules import siblings and vendored packages relatively (`from .vendor import item`)
- `sys.path` entries and top-level modules loaded from inside an extension folder import, and the extension's row prints a policy violation for each
- Each extension's copy of one subpackage loads its own module tree, classes, and state, while a wheel module loads once for every enabled extension
- Code registering classes or holding module state goes in a subpackage, and wheels hold libraries that do neither
