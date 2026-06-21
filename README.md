# Resonite GLB Avatar Exporter

Export a Unity (VRChat) avatar to a single **`.glb`** file that you drag straight into
[Resonite](https://resonite.com). Resonite's own glTF importer then loads the meshes, skeleton,
blendshapes (morph targets) and **embedded textures** — no Resonite mod, no live bridge, no
headless engine, nothing to crash.

It optionally **bakes** the avatar through **NDMF / Modular Avatar** first, so Merge-Armature
accessories (tails, ears, clothing toggles, chokers, …) get their bones merged into the main
skeleton and keep **correct weights**. It also works around a glTFast bug that crashes export when a
SkinnedMeshRenderer has a `null` bone.

## Why

The community "live" Unity→Resonite exporters stream data into a running Resonite over a bridge.
On many setups that hits the renderer's native crash (big/blendshape-heavy meshes) or, headless,
deadlocks on texture import. Exporting a plain `.glb` and using Resonite's mature, built-in importer
sidesteps all of that.

## Requirements

- **Unity 2022.3+**
- **glTFast** (`com.unity.cloud.gltfast`) — installed automatically as a dependency.
- **NDMF / Modular Avatar** (`nadena.dev.ndmf`) — *optional*, only needed for the **baked** export of
  VRChat avatars that use Modular Avatar. If it isn't present, the baked menu falls back to a plain
  export (with a warning) and the *raw* menu always works.

## Install

Pick one:

### A. Unity Package Manager (git URL)
`Window ▸ Package Manager ▸ + ▸ Add package from git URL…` and paste:
```
https://github.com/Martynyuu/resonite-glb-export.git
```
(This assumes the package files — `package.json`, `Editor/`, … — sit at the repository root.)

### B. VCC / ALCOM (VRChat Creator Companion)
If you publish a VPM listing, users can add your repo and install it from the Manage Project screen.
Otherwise use option A or C — both work fine inside a VCC project.

### C. Manual
Copy the `io.github.martynyuu.resonite-glb-export` folder into your project's `Packages/` folder, or
import the provided `.unitypackage`. Make sure glTFast is also installed.

## Usage

1. In the **Hierarchy**, select your avatar root (the object with the VRChat Avatar Descriptor).
2. Menu **Resonite ▸ Export Selected Avatar to GLB (baked)** for a VRChat/Modular-Avatar avatar
   (recommended), or **Resonite ▸ Export Selected to GLB (raw, no bake)** for a plain model.
3. Choose where to save the `.glb`.
4. **Drag the `.glb` into Resonite.** It imports as a model.
5. Run Resonite's built-in **Avatar Creator** (Create New ▸ Avatar Tools ▸ Avatar Creator) to set up
   IK/rig, then save it to your Inventory and equip.

Disabled toggle meshes (clothing variants) are included in the export, so you can switch them in
Resonite by enabling/disabling their slots.

## Notes & limitations

- **Materials:** custom VRChat shaders (e.g. Poiyomi) are approximated to glTF PBR on export — base
  color / normal / etc. come through; fancy shader effects do not. Tweak materials in Resonite.
- **Blendshapes:** exported as glTF morph targets (so face-tracking/visemes shapes are present in the
  model). Driving them in Resonite is a separate setup step.
- **Baking** writes temporary assets under `Assets/ZZZ_GeneratedAssets` (NDMF). Safe to delete.
- The exporter never modifies your scene avatar — it always works on a throwaway clone.

## Support

If this saved you some pain getting your avatar into Resonite, you can support development here:

☕ **[ko-fi.com/strudel9](https://ko-fi.com/strudel9)**

## License

MIT — see [LICENSE.md](LICENSE.md).
