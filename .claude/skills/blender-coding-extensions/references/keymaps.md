# [KEYMAPS]

## [01]-[ITEMS]

Keymap items join `bpy.context.window_manager.keyconfigs.addon`, present in background and GUI runs alike, and release through the keymap that made them:
- Addon keymaps merge into the user keymap of their name, space, and region, and one matching no built-in keymap never reaches `keyconfigs.user`
- `keyconfigs.default.keymaps` lists every built-in name, space, and region, background runs included, where code reads `space_type` and `region_type`
- `keymaps.new` returns the keymap of its name, space, and region where one exists, defaulting to space `EMPTY` and region `WINDOW`
- Extensions share one addon keymap per name, space, and region, and `keymaps.remove` on it drops other extensions' items
- `keyconfigs.addon` refuses modal keymaps with `RuntimeError`, built-in modal maps included, and modal keys stay in the operator's `modal`
- `keymap_items.new` appends with `repeat` off, `head=True` prepends, and held-key items pass `repeat=True`
- Modifier keywords take `1` held, `0` released, or `-1` either state, and `any=True` matches every modifier state
- Keymap rows take the built-in keyconfig data form `(<operator>, <event>, {"properties": [(<name>, <value>), ...], "active": False})`
- `"active": False` adds a binding the user enables in preferences
- `keymap_init_from_data` prints a warning for a property the operator lacks and keeps the item without it
- Items calling an operator no class registers read `properties` as `None` after `keyconfigs.update()` without `keep_properties`
- `ctrl=1` items ignore Cmd on macOS, and addon rows gain Cmd copies through `keyconfig_data_oskey_from_ctrl_for_macos` alone
- Cmd copies precede each row naming `ctrl`, except Ctrl alone on `TAB`, `H`, `M`, `W`, `SPACE`, `PERIOD`, and `ACCENT_GRAVE` and Ctrl+Alt on `Q`
- Built-in Cmd copies keep their operator when an addon item takes the Ctrl shortcut they copy
- Background runs hold the C-defined default items alone until `bpy.utils.keyconfig_init()` loads the built-in preset
- Addon items reach `keyconfigs.user` through `keyconfigs.update()`, and GUI event loops run the update themselves

## [02]-[EVENT_ORDER]

Events reach items in user keyconfig order, the first matching item whose operator polls true taking the event:
- Addon items precede every built-in item of their keymap and win an event a built-in item there binds
- Addon items join in reverse add order, the last added winning a shared event and `head=True` losing to earlier addon items
- 3D Viewport regions run the active tool's keymap, then the mode keymaps (`Object Mode`, `Mesh`), then `3D View Generic` and `3D View`
- Items overriding a built-in shortcut join the built-in item's keymap, a built-in `Object Mode` item beating addon items in `3D View` or `Window`
- Items with an operator failing `poll` hand events to the next matching item, and an operator returning `PASS_THROUGH` runs and hands them on
- Repeat events skip items with `repeat` off, and `CLICK_DRAG` items with a `direction` match drags that way alone

## [03]-[USER_EDITS]

User edits sit on `keyconfigs.user` as differences from built-in and addon items:
- `unregister()` ends with `keyconfigs.update(keep_properties=True)` after classes unregister, else a disable or update drops edits to extension items
- Edits follow an item through a changed key or modifier, matched by operator and property values
- Edits skip an item with changed property values and stay stored, applying again once values match
- Bindings belong in `keyconfigs.addon` alone, and a user keymap edit in the event-loop pass that adds addon items stores those items as removed
- `keyconfigs.update()` rebuilds user items at new addresses, a held user `KeyMapItem` reading freed memory, and each use finds its user item again
- `keyconfigs.user` holds no keymap while startup `register()` runs
- `KeyMap.restore_item_to_default(<user item>)` resets an item to the extension's row and deletes its edit
- `KeyMap.restore_to_default()` resets every edited item of a user keymap, built-in edits included
- Operators under an extension's own category lack right-click Assign Shortcut, and its preference page draws their items

Preference pages pair each owned addon item with its user item through `find_match` at each draw, and `draw_kmi` follows `context_pointer_set("keymap", <user keymap>)` for restore and remove buttons reading `context.keymap`:

```python
class ItemPreferences(bpy.types.AddonPreferences):
    bl_idname = __name__.rpartition(".")[0]

    def draw(self, context: bpy.types.Context) -> None:
        match root_in(bpy.types.WindowManager, context).map(lambda manager: manager.keyconfigs):
            case Option(tag="some", some=bpy.types.KeyConfigurations(addon=bpy.types.KeyConfig() as addon, user=bpy.types.KeyConfig() as user)):
                owned = frozenset(getattr(cls, "bl_idname", cls.__name__) for cls in CLASSES)
            case _:
                return
        for owner, entry in (
            (user_keymap, user_item)
            for keymap in addon.keymaps
            if (user_keymap := user.keymaps.find_match(keymap))
            for item in keymap.keymap_items
            if {item.idname, item.properties and item.properties.get("name")} & owned
            if (user_item := user_keymap.keymap_items.find_match(keymap, item))
        ):
            self.layout.context_pointer_set("keymap", owner)
            rna_keymap_ui.draw_kmi([], user, owner, entry, self.layout, 0)
```

`owned` names the classes `CLASSES` lists, matching items that call the extension's operators or open its menus and panels through `wm.call_menu`, `wm.call_menu_pie`, and `wm.call_panel`. `rasm_bpy.commands.Keymap.drawn` draws the same editors over the items of one `Keymap` row.
