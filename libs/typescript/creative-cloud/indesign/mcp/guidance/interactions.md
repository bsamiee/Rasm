# [INTERACTIONS]

## [01]-[OUTPUT]

Choose output format and reader before assigning interactions. Read action support in Buttons and Forms: interactive PDF and fixed-layout EPUB support different actions. Native authoring access does not establish exported behavior. Test playback in the intended reader; `render` shows appearance without playing events, media, or animations.

Use [Production](indesign://production/reference) for export and accessible document structure. Choose appearance-only export when controls must retain artwork without actions.

## [02]-[BUTTONS_AND_BEHAVIORS]

Inspect page `formFields`, then follow returned control references to supported collections. Read `Button.behaviors` for existing actions. Add actions through their specific behavior collection, assigning `behaviorEvent` from `BehaviorEvents` and selecting native targets. Set `enableBehavior` independently of trigger and target. Scripting `eventListeners` execute inside InDesign; exported actions belong to behaviors.

Edit `Button.states` for normal, rollover, and click artwork. Select appearances through `State.statetype` and edit `State.pageItems`; appearance does not assign an action. Use `Button.hiddenUntilTriggered` for initial exported visibility and `ShowHideFieldsBehavior` for event-driven visibility. Layer visibility and nonprinting settings do not replace those relationships.

With `button` holding a native button reference, add and return a URL action:

```js
return button.gotoURLBehaviors.add({
    behaviorEvent: BehaviorEvents.MOUSE_UP,
    url: 'https://example.com/',
});
```

Use [Scripting](indesign://scripting/reference) for references and constructor arguments. Returned actions retain native specifiers for inspection and edits.

## [03]-[FORMS]

Choose `TextBox` for entered text and `TextFrame` for document text. Inspect field options before setting input requirements, read-only behavior, defaults, choices, or exported typography. Set `FormField.description` to describe the field accessibly.

Give radio buttons a shared name for one choice group and distinct `exportValue` values for submitted choices. Set `CheckBox.exportValue` independently of `checkedByDefault`. Assign native field references to `Page.tabOrder` for keyboard focus order. Select PDF export's tab-order policy: field order and tagged structure can differ from visual stacking.

Author fields and form actions in InDesign; add PDF calculations and custom validation in Acrobat after export. Creating `SubmitFormBehavior` configures submission without submitting data during authoring.

## [04]-[OBJECT_STATES]

Use `MultiStateObject.states` for alternate content with one state visible at a time. Call `addItemsAsState` to add a state or `State.addItemsToState` to add artwork within an existing state. Overlapping page items alone do not establish state membership.

Assign `GotoStateBehavior.associatedMultiStateObject` and `stateName` for a specific destination. Next-state and previous-state behaviors follow native state order; `State.move` changes that order. Inspect `initiallyHidden` separately from active state. Button appearances and multi-state content share `State` objects but use different behavior relationships.

Edit states when interactions must remain intact. `State.releaseAsObject` removes one state and releases artwork; `MultiStateObject.releaseAsObjects` releases every state and destroys the container.

## [05]-[NAVIGATION]

Create hyperlinks from native text or page-item sources to destinations. Reuse destinations for links to the same text anchor, page, or URL. Choose bookmarks for navigation entries without visible source artwork, or button behaviors for navigation triggered by control events.

For cross-document jumps, retain destination-document identity and anchor references. Export documents with the intended relative placement. Renamed output files or missing targets can break navigation despite valid source references.

## [06]-[MEDIA_AND_ANIMATION]

Inspect placed `Movie` and `Sound` objects for sources, posters, and playback settings. Target native media references from button behaviors. Posters supply display artwork without proving playback. Use [Production](indesign://production/reference) for source links and updates.

Choose available `MotionPreset` objects and configure `PageItem.animationSettings` for duration, easing, paths, and keyframes. Motion paths and static transforms describe different states of an animated object.

Choose playback triggers through `TimingSetting.timingLists` and `DynamicTriggerEvents`. Arrange `timingGroups` within each list and `timingTargets` within each group for animation, media, or object-state targets. Set `TimingTarget.delaySeconds` for delay. Use native group placement, repetition, `move`, and `unlink` for sequence and concurrency. Duration changes do not assign triggers or order.

For button-triggered playback, inspect animation behaviors. For native timing events, inspect spread or object `timingSettings`. Preserve those playback relationships instead of adding script timers or duplicating animated objects.
