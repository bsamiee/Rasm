---
type: llm
weight: 3
---

PASS if the reply reports the mapping read from the installed Photoshop UXP runtime (ALL to `display`, ERROR to `silent`, NONE to `dontDisplay`), recommends the `silent` dialog option in place of NONE (as `DialogModes.ERROR` or as the `"silent"` `dialogOptions` string), and names the installed file it read (a `Required/UXP/app.js` or equivalent).
FAIL if the reply recommends `DialogModes.NONE`, gives a mapping that differs from the one above, or answers from documentation alone without reading the installed app.
