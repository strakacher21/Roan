# **Roan**

## [VRChat](https://hello.vrchat.com/) furry fox avatar.
___
<div style="text-align: center;">
  <img src="Gallery/Roan.png" alt="Roan shaded" width="90%">
  <img src="Gallery/Roan showcase shaded.png" alt="Roan shaded" width="45%">
  <img src="Gallery/Roan showcase mesh.png" alt="Roan mesh" width="45%">
</div>

*images `tag 2.2` avatar. I'll need to change the images later*

___

# А Request
I don't expect this avatar to be widely used, but just in case:

**Please do not use this model to post pornographic or suggestive content.<br/>
I will also be grateful if you indicate my authorship (and not indicate the above content).**

*About all mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D*

# Get

### [:arrow_forward:Sketchfab Preview](https://skfb.ly/pAzHI)

### [:arrow_forward:Link to the VRChat avatar (tag 2.2)](https://vrchat.com/home/avatar/avtr_2a8b73c0-5a67-499c-b3f3-67398d269035)
(basic face tracking and in-game color change for PC only)

### [:arrow_forward:Link to the VRChat *only VRCFT* avatar (tag 2.2)](https://vrchat.com/home/avatar/avtr_7260a101-a39a-4ac1-9139-1c206a64d397)
(better face tracking but no in-game color change. ***This will become the main version soon, meaning "only VRCFT" version will be removed will be removed soon***)

### [:arrow_forward:Link to the VRChat *Lab Edition* avatar](https://vrchat.com/home/avatar/avtr_7260a101-a39a-4ac1-9139-1c206a64d397)
(where I test the latest changes)

### [:arrow_forward:Download Project](https://github.com/strakacher21/Roan/releases)

___
# Info

The project includes the **Blend 5.0** file itself and the **Unity 2022.3.22f1** project.

> [!WARNING]
>**To properly export a model from Blender to Unity, you need to use the >export script in Blender!**</br>
>Simply click the '▶' button in Blender to export the model correctly to Unity.
>
>To properly configure your Unity project, use this **[:bulb:Unity project setup guide](Unity-setup.md)**.

**The character currently has no texture (uses vertex paint and has baked vertex paint).** This is a simple way to make temporary coloring without using a UV map, which is useful when the body geometry changes frequently. This works well for solid colors, but is not suitable for fancy pattern, but your avatar file size will stay small and load quickly without texture files.

The Unity project has a prefab model, as well as two scenes for **PC** and **Quest&IOS** (The only difference is in the quality of textures). All prefab changes go into changing the scene. Аlso includes **AnimatorWizard** script (attached to the avatar prefab). That allows you to customise gestures, facial expressions, eye/face tracking, etc. You can disable some features to save [VRChat parameters](https://creators.vrchat.com/avatars/animator-parameters/#custom-parameters).

# TODO
### Global:
- [x] full face tracking support ([VRCFT](https://docs.vrcft.io/docs/intro))
- [x] UV map for textures
- [ ] сreate textures (something better than that regular vertex paint!) ***(working on it)***
- [ ] optimize the character mesh, add details, and also need to work on his style ***(working on it)***
- [ ] grooming hair (in Blender only)
- [ ] add body geometry?
- [ ] add different clothes?
___

### Minor:
- [ ] make an adequate "weight paint" for the whole character to move better!
- [ ] revise visemes
- [x] create simple expressions
- [ ] revise the character's physical bones
- [x] add expressions menu, FX, Additive (now they don't exist at all, lol)
- [ ] idle anims
- [ ] adapt [AnimatorWizard](https://github.com/strakacher21/Roan/blob/main/Roan%20unity%20project/Assets/scripts/AnimatorWizard.cs) script for this project
- [ ] make locomotion better!
- [ ] [VRM](https://vrm.dev/en/vrm/vrm_about/) file

## Attribution
uses the [v3-animator-as-code](https://github.com/hai-vr/av3-animator-as-code) and [AnimatorWizard script](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/blob/main/vrcfox%20unity%20project%20(B%26C)/Assets/scripts/AnimatorWizard.cs) to set up animators.

The texture of the pants was taken from: [ambientCG](https://ambientcg.com/view?id=Fabric003).