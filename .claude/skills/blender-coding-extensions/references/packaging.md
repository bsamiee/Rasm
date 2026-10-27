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
- Shared folders present at startup sit ahead of Blender's `site-packages`, and a session's first wheel install appends the folder last until restart
- Wheels ahead of Blender's `site-packages` replace bundled copies process-wide, and extensions omit one Blender bundles at a version they accept
- Distributions, keyed by wheel file name before its first `-`, resolve to the highest version any enabled extension bundles
- Versions rank by major, minor, and patch, then the tail as text, `1.10` reading as `1.0.10` below `1.9.0` and `1.0.0rc1` outranking `1.0.0`
- Code registering classes or holding module state sits in a subpackage, a wheel module importing once for every enabled extension
- First imports of a session hold until restart, a later install or disable rewriting files while `sys.modules` keeps the imported version
- Enabled-set changes (checkbox, `default_set=True` calls, uninstall, failed enables) resync the folder at once, distributions falling back or leaving
- Syncs extract a wheel whose `<name>-<version>.dist-info` folder is absent, a wheel rebuilt at that version keeping the first extraction
- First extractions stay until an enabled-set change leaves no enabled extension listing the wheel, an upgrade install keeping them
- Wheels for another platform skip at enable, and the import in `register()` raises `ModuleNotFoundError`
- Wheels for another CPython minor version mark the extension incompatible, and `abi3` wheels of an older CPython load
- Wheels for macOS match by architecture or `universal2`, whatever OS version their tag names
