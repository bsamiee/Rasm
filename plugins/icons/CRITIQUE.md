# Critique of v1

Numbers in brackets point to `SOURCES.md`.

## What v1 got right

- Sweeping a flat reed along a skeleton is the correct model. Mashq states that a skeleton plus a nib angle rebuilds the letter [29]. v2 keeps the sweep.
- The 110° edge leaning back gives thick down-left strokes and thin up-left rises, which is how a nastaliq bowl behaves. v2 keeps it as the resting angle.
- The nuqta as one press of the reed, the echo stack, the ink and ground colors, and the `<use>`-based SVG are all sound.
- The python staircase is a clear meaning and the owner approved it. v2 keeps it.

## Construction and nib

1. **The reed never turns.** v1 fixes the edge at 110° for every stroke. Arabic-script contrast is "translation with rotation" [23], and Gulzar's pen-angle figure shows the reed turning inside one letter [19]. Because v1's reed cannot turn, a vertical is stuck at 0.34 of the nib, so the alifs are thin and weak, and every horizontal is the same 0.94 slab. Thick strokes run about 2.8 times the thin ones, against Quartal's heavy-to-hairline ratio of well over 10:1 [1] and the "steep cuts, pointy terminals, high contrast" of a sharp Arabic display cut [24, 26].
2. **The only hairlines are pressure tapers.** Easing the width toward 3–40% gives blunt, rounded noses: the msbuild bricks, the dotnet lozenge, and the function-hooks kashida all look like boats or fish. The references end strokes in a hairline drawn on the corner of the reed [29] or in a crisp cut [19, 22], and never in a rounded taper.
3. **Strokes are symmetric.** Where v1 cuts both ends square (the msbuild bricks, the function-hooks kashida), the stroke becomes a plank. Every reference stroke is asymmetric: a reed-cut head and a hairline exit [12, 19, 20].
4. **The nib is too heavy for the compositions.** At 86 units the strokes are the "fat" the brief rules out. Ink mass also swings from rasm's thin alif to msbuild's four slabs, so the set does not share one color. Sarkis notes that Arabic grows in every direction as weight rises and has a ceiling [25]. v1 sits above that ceiling.

## Proportion and measure

5. **rasm's measure is wrong.** The README says four nuqtas size the alif. The nastaliq alif is three dots tall [29]. The dots are also set tip to tip on a diagonal. Yaghan shows that vertex-to-vertex stacking makes a vertical column only at a 45° placing angle; at any other angle the column has to use gap or edge connection [30]. v1's staircase reads as a zipper or `:/`, not as the measure.

## Readings: letters and words

6. **orchestration reads as a word.** Three upright shafts standing on a sweep that dives left and returns right is the shape of a returned-yeh (bargashteh) under teeth or lams: a Persian reader sees a word shape like سی or الی. The brief forbids words.
7. **The other icons fall into letters.**
   - python's three bowls with a hooked left tip are dotless noons (ں).
   - function-hooks pairs a kashida with a hook below it: a Latin J and a nastaliq ے.
   - dotnet reads as a Latin q or a spoon.
   - uxp's two rotated corners read as a Z or an S.
   - blender is an eye, the generic "view" icon and the Eye of Providence, and says nothing about Blender's work (bpy data, node trees, render).
   - rasm reads `!/`.
8. **msbuild has no calligraphy in it.** Four identical slabs stand in for bricks, with no hairline, cut, or rhythm.

## Echo, palette, and small sizes

9. **The echo direction is right and stays.** Shabtab 3 and Harmonia stack their echoes straight down [2, 4]. But v1 lets the echo fall inside closed counters (blender's top arc), where its bands clutter the counter.
10. **The light echo is half as visible as the dark one.** Slate `#AFBDC2` on ground `#EFEAE2` is 1.61:1, while dark slate `#4A5D66` on `#0E0E0E` is 2.80:1. The bands are 14 units, which is 0.9 px at 64 px and 0.4 px at 32 px. In light mode they wash out entirely, while dark mode keeps a visible shadow. Quartal uses this slate only as large flat fills [1], never as thin bands.
11. **Rhythm and fill are uneven.** Each icon is centered but not sized to the family, so ink coverage, extents, and density vary from icon to icon. There is no shared rule for spacing between repeated elements.

## Family coherence

12. The set shares a palette but not a rule. Some icons are all heavy slabs, others mostly thin, and terminals vary between square cuts, rounded tapers, and parallelograms. The references hold a series together through one stroke rule applied to every member [6, 16, 17, 18].
