# VRC Fury - Non-Destructive Tools for VRChat Avatars

**Clothing Attacher // Toggle Builder // Gesture Manager // Controller Merger // Avatar Optimizer // Modular Setup // All Reversible!**

## Download

### \>> [Download VRCFury](https://vrcfury.com/download) || [Support Discord](https://vrcfury.com/discord) <<

## Benefits
* **Easy to use**
  * Define toggles, gestures, item modes and more using a simple GUI in Unity
* **No more layers, no more menu editing**
  * The animation controller, VRC Menu, and synced parameters are all generated automatically by VRC Fury.
* **Great for asset artists**
  * Prefabs can contain their own VRC Fury definitions
  * Distribute your avatar addons with everything needed for the user to add your animations / props to their menu.
  * No more complicated "copy my layers into your fx"! If their project has VRC Fury, they can just drop your prefab into their project, and upload!
  * For more details, check out [VRCFury for Artists](https://gitlab.com/VRCFury/VRCFury/-/wikis/VRCFury-for-Artists)
* **No more absolute paths for animations**
  * VRC Fury props defined in prefabs will automatically have their clips rewritten to work properly, no matter where in the hierarchy they are ultimately placed.
  * Write your animation clips from the root of the prefab, and VRCFury will handle rewriting it when it winds up on an avatar.
* **No clips? No problem!**
  * Just want to toggle a game object or a blend shape? No worries!
  * VRC Fury can create these toggles for you, without you needing to touch animation clips whatsoever.
* **Gestures, Idle Animation Support, and more!**
  * Fury isn't just about props. It also has logic to build every single animation layer that I personally use myself for my avatars. If it can't build a layer the way you want, let us know and maybe we can support it!
* **Already got your avatar perfect?**
  * VRC Fury still works perfectly with avatars that already have animations.
  * Your existing layers, parameters, and menus will be untouched, and VRC Fury will keep its work totally separate from yours.
  * This also means VRC Fury will not clobber the work of TPS, VRCLens, etc.
  * VRCFury does its work just before your avatar uploads. It makes a copy of your work, adds the features you've requested, then ships that off! This means your animation controller files are never touched, and if you're unhappy with the results, you can just remove it and your next upload will be like it was never there.
  * Note: VRC Fury works with existing controllers using *either* Write Defaults ON or OFF.
* **No more write defaults pain**
  * Every layer generated by VRC Fury will automatically have its default states calculated and maintained, based on the resting state of your avatar in the editor. This includes animation clips you give VRC Fury, so now you only have to make "on" animations, no more "off"!
* **And more!**
  * We're constantly adding more and more non-destructive avatar features to VRCFury. Keep an eye out for updates!

## How to use

* Download and import the package above (it may take a bit to finish importing)
* On your main avatar object, or the prop you are trying to setup, click `Add Component` -> `VRC Fury`.
* Add features using the `+` button on the component. See the `Feature Modules` section below for information about each type of feature.
* You're done! There's no "building" to do. VRCFury will non-destructively update your controllers, VRC menus and params automatically before each upload.

## Feature Modules

Once you add a VRCFury component to your avatar (or prop), you can add any combination of these feature modules. These are in alphabetical order, but one of the most popular is the `Toggle`.

### Advanced Visemes

This feature allows you to use VRCFury actions as visemes.

Benefits:

* Use animation clips, material flipbooks, or any other VRCFury action as a speech viseme.
* You can use bone transforms in your visemes, meaning you can open your jaw rather than using an "open mouth" blend shape.
* This can enhance some features, such as tongue movement, while your mouth is open during speech.

### Anchor Override Fix

Adding this feature ensures that all of your avatar meshes use the same anchor override, which ensures
different meshes receive the same environment lighting level.

### Armature Link (Attach clothing, props, etc)

Is your prop a skinned mesh that "attaches" to the bones of the root avatar? Armature Link is what you need! Give it the root bone (hips) within your prop and the path to that same root bone in the avatar, and VRCFury will automatically attach the mesh to the avatar's bones. Just make sure the bones you want to link are all in the same order and have the same names.

You can also use this feature to attach an object to an avatar's bone. For instance, if your prop contains a
empty that must be on the avatar's hand, you can use Armature Link to place it there.

### Blendshape Link

Add this feature, specify a clothing mesh, and a path to the Body object on the avatar, and the blendshapes
from the clothing will automatically be linked to the avatar body. This means any sliders / toggles affecting
the body will be reflected in the clothing as well automatically.

### Blendshape Optimizer

Automatically bakes all non-animated blendshapes into your avatar's meshes. Reduces your VRAM usage for free, no configuration required!

### Blink Controller

Include a single-frame animation of your avatar with its eyes closed (or click the plus and give it the blend shape name), and VRC Fury will drive your avatar's blink cycle.

Benefits:
* Blinking will stop automatically when your avatar performs vrcfury gestures affecting its eyes. This means no more 'double-blinking'.
* Unlike vrc's built-in eye tracking disable feature, your eyes will not freeze closed, partially closed, unfreeze unexpectedly due to combo-gestures.
* Your eye blink will be synchronized with all other clients (I'm unsure if the default vrc eye blink is synced or not).

### Bounding Box Fix

This feature ensures that every mesh on your avatar has a suitably large bounding box. This prevents the issue when some objects on your avatar dissappear when viewed at extreme angles.

### Breathing Controller

Automatically creates an animation for your avatar's breathing cycle. Provide either a gameobject (which will be scaled between the provided "min" and "max" scale), or a blendshape, which will be animated between 0 and 1.

### Cross-Eye Fix

VRChat introduces roll to your eye bones in some circumstances, making it appear that you've gone cross-eyed.
Adding this fix will solve this problem automatically through a combination of rotation constraints to eliminate roll.

### Direct Tree Optimizer

If you add this feature to your avatar, VRCFury will attempt to convert all "plain" toggle layers on your avatar into a compressed
direct blend-tree. This reduces the number of animator layers on your avatar, which can have a meaningful improvement on performance
(frame time).

### Fix Write Defaults

This feature will automatically align Write Defaults for every state on your avatar. If will automatically prefer whichever your avatar is "closest to," meaning it will select On or Off depending on which requires the fewest changes to your avatar. If you'd like, you can override the selection and Force Off or Force On. Yes, it's magic.

### Force Object State

This feature can activate, deactivate, or delete an object on your avatar during the upload process. Useful if you want to show clothing on your avatar in the editor but have it "off" during the upload for toggles to work properly. Also useful if you want to delete an object from a prefab without having to unpack it.

### Full Controller

This is usually only useful for prefab artists. Provide a controller, menu, and params, and it will be merged into your client's avatar automatically. If you're working on your own avatar, you should usually just add these things to your avatar's own controller, menu, and params instead.

NOTE: Animation clips in your specified controller should have animated paths relative
to your prop's root. VRCF will automatically add prefixes to all the animations so they
work wherever it is installed on the avatar. If you wish to animate properties on the avatar
itself (outside of your prop), you can specify a path from the root of the avatar (as you traditionally would), and they will
still work. VRCFury will automatically determine if the path is from the avatar root or from the prop root, and rewrite them
appropriately if needed.

### Gestures

This is the one-stop shop for adding hand gestures to your avatar! For each hand gesture, choose
 which hand is used for the gesture, and which hand sign needs to be acted. If
 you select COMBO, the gesture will only activate if you do the given gesture on both hands
 simultaneously. Use the + (Plus symbol) to set the animation clip / blend shape that
 your gesture will activate.

Advanced options for each gesture:

`Disable Blinking when Active`

If enabled, this gesture will automatically prevent your avatar from blinking while the gesture
is active. Useful if the gesture does something like sad eyes or eyes closed. Note: Only works
if your avatar's blinking is controlled by the VrcFury Blink Controller.

`Customize Transition Time`

By default, gestures will active in 0.1 seconds. You can adjust this with this option.

`Gesture Lock`

Set a menu path here, and an item will be created in your expression menu at the given path. The
menu item will allow you to easily lock the gesture "on" without having to hold the hand sign.

`Exclusive Tag`

If multiple gestures contain the same Exclusive Tag, only one can be active at a time. For instance,
if you have a Sad and an Angry gesture, you could give them both the same tag, and the system will
prevent them from being active simultaneously. The "highest" one in the list wins. A gesture
can have multiple Exclusive Tags separated by commas.

### Gizmo

All this does is show an editor gizmo. It does nothing in game. Useful primarily for prop artists
who wish to identify something on their prop without including it in the upload.

### Move Menu Item

Can move a menu item, either already on the avatar, or one created by VRCFury. Simply enter the path you'd like to move from and move to. For example:

* From: `My Folder/Clothing/Shirt`
* To: `Cool Stuff/Clothes/Shirt`

### Override Menu Icon

Will override the icon for the given item in your menu.

### Override Menu Settings

Allows you to change VRCFury's default "Next" menu item, when there are too many items to fit on a page.

### Remove Hand Gestures

When present, this feature eliminates any features in your avatar's non-vrcfury controllers that use hand gestures. This is useful if you'd like to implement your own hand gestures with VRCFury, don't want them to conflict with ones that came with a base avatar, and don't want to edit them manually.

### Security Lock

This feature allows you to set a pin number which, when entered in your avatar's Security submenu, will unlock any Toggleable Props which you've marked with the Security flag.

### Senky Gesture Driver

This feature sets up gestures the way that Senky likes them! Probably not super useful for you, unless you want this very specific gesture hand layout.

### Slot 4 Fix

Allows you to animate materials in slot 4 on meshes within your avatar. Typically, attempting to do this
results in corruption of the material in slot 2 due to a unity bug. This feature resolves the issue by
moving all slot 4 references to a new slot at the end of the list.

### Toes Puppet

Given an animation for up, down, and out, this creates a puppet for toe control in your menu.

### Toggle

Use this for a normal "on / off" prop. For simple object props, click the plus, choose Object toggle, and then drag the object into the field. If you choose blendshape, the blendshape will be set to 100 when "on" (only works on root skinned meshes). For more advanced "on" states, you can provide an animation clip instead.

`Menu Entry`

The name you put in the prop's text field will be used as the name of the toggle in your VRChat menu. If you wish to put the prop in a sub-menu, use slashes. Ex: `Props/My Cool Piano`

`Default On`

Want to add an idle animation or "default prop" to your avatar? Create a new prop, click the `*` and select `Default On`. Your idle animation or prop will now be on all the time (but you can also trigger it back off in game!)

`Show in Rest Pose`

If set, this toggle will be enabled in the avatar's "Rest Pose".
This means the toggle will shown "on" in the in-game avatar selector, during
full body calibration, and for users who have disabled your avatar's
animations.

`Slider`

Select `Slider` from the `*` menu, and VRC Fury will make the prop into a slider rather than a toggle. 0 will be the avatar default state, and 100% will be your "enabled" state.
If `Default On` is also set, an arbitrary starting value can be set.

`Saved`

Not everything in VRC Fury has to be a temporary prop. Want to save your clothes (or anything else?) across worlds? Select `Saved between worlds` in the `*` menu.

`Security`

When a prop is flagged with the `Security` flag, it can only be enabled when the Security Lock feature is unlocked on your avatar (see the Security Lock section for more details).

`Physbone Reset`

Got an animation that changes parameters on a physbone?

Click the advanced `*` button on the VRC Fury prop for the animation, then click `Add PhysBone to Reset`. Drag the object for the physbone into the box (it should be on an empty by itself). VRC Fury will automatically flip the bone off and on any time your animation is run or reset, causing the physbone to reload your changed settings.

`Exclusive Tags`

If multiple toggles contain the same Exclusive Tag, only one can be active at a time. For example,
if you have multiple sets of clothing which interfere with each other, you can give them the
same tag. When one is enabled, all other toggles with the same tag will be disabled. Multiple tags
can be given, separated by commas.

`Exclusive Tag Off State`

If set, this toggle will automatically be activated when all other toggles with the same
`Exclusive Tags` are disabled. This makes it usable as an "Off" state for a set of conflicting
toggles.

`Separate Local State`

If set, this creates a separate animation for local and remote machines. The local state will be seen by the user in the avatar, and the remote state will be seen by everyone else.

`Enable Transition State`

If set, this will create 2 additional states for animating a transition between the off and on state. The transition animation will be played forwards when transitioning from off to on and backwards when transitioning from on to off when `Transition Out is reverse of Transition In` is on, otherwise an separate out transition can be set. If `Separate Local State` is also on, separate local transitions can also be set.

### When-Talking State

This is a very simple feature which activates the given animation only while the user is "talking" (with any viseme).

## Additional Features

### Controller-Less Setup

Your avatar doesn't even need to have a FX layer, menu, or params! If these are unset, VRCFury will create them automatically, and manage them fully (meaning it will be deleted and recreated from scratch before each upload). Beware of this! If you want to make your own changes to your controller, menu, or params, then you should create one yourself outside of the vrcf temp directory.

### VRCF Global Colliders

VRCFury can be used to add globally-synced colliders to any bone on your avatar. This means you can put one on your foot, your nose, or anywhere else you can imagine, then bap people with them! Simply create an empty on the bone you'd like to add a collider to, then add a `VRCF Global Collider` component to that empty.

Beware that this feature steals colliders from your fingers, so the more you add, the fewer contacts there will be on your fingers. It will try to steal from the least important fingers first. You've been warned!

### Write Defaults Auto-Fix

VRCFury will detect if your avatar has a mixture of Write Defaults, and will offer to fix it for you on your first upload. Don't worry, this change isn't destructive -- it simply adds a `Fix Write Defaults` VRCFury component to your avatar root, which you can always remove to undo if you choose.

### Action Controller Conflict Resolution

If you install multiple independent packages of avatar "dances" using vrcfury, they will be rewritten to work together. For instance, you can install GogoLoco AND CuteDancer using VRCFury, and will be able to use the dances from each. Typically this is impossible as the animations from one will override the other, however VRCFury rewrites the playable layer weight drivers to affect only the layers owned by each individual package. Hooray!

## How to remove / uninstall

* `Tools > VRCFury > Uninstall VRCFury`
* If that doesn't work, or the menu is missing entirely, go into Unity's `Package Manager` tab and remove all the VRCFury packages. **Remove the VRCFury Updater FIRST** so it doesn't try to reinstall the rest.
