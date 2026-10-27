# [KEYMAPS]

## [01]-[ITEMS]

Keymap items join `bpy.context.window_manager.keyconfigs.addon`, present in every process mode, and release through the keymap that made them:
- Keymap names with their space and region come from `keyconfigs.default.keymaps`, and addon keymaps matching none never fire
- `keymaps.new` and `keymaps.find` default to space `EMPTY` and region `WINDOW`, and editor keymaps pass `space_type`
- Extensions share one addon keymap per name, space, and region, and `keymaps.remove` on it drops other extensions' items
- `keyconfigs.addon` refuses modal keymaps with `RuntimeError`, stock modal maps included, and modal keys stay in the operator's `modal`
- `keymap_items.new` leaves `repeat` off, held-key items passing `repeat=True`
- Modifier keywords take `1`, `0`, or `-1` for either state, and `any=True` matches every modifier state
- Keymap rows take the stock keyconfig data form `(<operator>, <event>, {"properties": [(<name>, <value>), ...]})`, events naming pressed modifiers alone
- `ctrl=1` items ignore Cmd on macOS
- `keyconfig_data_oskey_from_ctrl_for_macos` keeps each row and adds a Cmd copy of each event naming `ctrl`, Ctrl shortcuts stock macOS keymaps keep excepted
- Background runs hold C-defined default items until `bpy.utils.keyconfig_init()` loads the stock preset
- Addon items reach `keyconfigs.user` through `keyconfigs.update()`, and GUI event loops run the update themselves

## [02]-[EVENT_ORDER]

Events reach items in user keyconfig order:
- Addon items precede every stock item of their keymap and win an event a stock item there binds
- Addon items join in reverse add order, the last added winning a shared event and `head=True` losing to earlier addon items
- Items overriding a stock shortcut join the stock item's keymap, a stock Object Mode item beating addon items in 3D View or Window
- Items with an operator failing `poll` hand events to the next matching item, and an operator returning `PASS_THROUGH` runs and hands them on

## [03]-[USER_EDITS]

User edits sit on `keyconfigs.user` as differences from addon items:
- `unregister()` ends with `keyconfigs.update(keep_properties=True)` after classes unregister, else a disable or update drops edits to extension items
- Edits follow an item through a changed key or modifier, matched by operator and property values
- Edits skip an item with changed property values and stay stored, applying again once values match
- `keyconfigs.user` holds no keymap while startup `register()` runs
- `KeyMap.restore_item_to_default(<user item>)` resets an item to the extension's row and deletes its edit
- `KeyMap.restore_to_default()` resets every edited item of a user keymap, stock edits included
- Operators under an extension's own category lack right-click Assign Shortcut, and its preference page draws their items

Preference pages pair each addon item of an extension's category with its user item through `find_match` at each draw, and `draw_kmi` follows `context_pointer_set("keymap", <user keymap>)`, restore and remove buttons reading `context.keymap`:

```python
from collections.abc import Iterator

import bpy
from expression import Option
import rna_keymap_ui


def user_items(keyconfigs: bpy.types.KeyConfigurations, category: str) -> Iterator[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    addon: bpy.types.KeyConfig = keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    user: bpy.types.KeyConfig = keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        (user_keymap, user_item)
        for keymap in addon.keymaps
        for item in keymap.keymap_items
        if item.idname.startswith(f"{category}.")
        for user_keymap in Option.of_optional(user.keymaps.find_match(keymap)).to_list()
        for user_item in Option.of_optional(user_keymap.keymap_items.find_match(keymap, item)).to_list()
    )


class ItemPreferences(bpy.types.AddonPreferences):
    bl_idname = "bl_ext.user_default.item"

    def draw(self, context: bpy.types.Context) -> None:
        keyconfigs: bpy.types.KeyConfigurations = context.window_manager.keyconfigs  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        for keymap, item in user_items(keyconfigs, "item"):
            self.layout.context_pointer_set("keymap", keymap)
            rna_keymap_ui.draw_kmi([], keyconfigs.user, keymap, item, self.layout, 0)
```

`bl_ext.user_default.item` stands for the extension package and `item` for its manifest `id`.
