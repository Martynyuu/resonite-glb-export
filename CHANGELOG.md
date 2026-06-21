# Changelog

All notable changes to this package are documented here. Format loosely follows
[Keep a Changelog](https://keepachangelog.com/); versions follow [SemVer](https://semver.org/).

## [1.0.0] - 2026-06-21

### Added
- One-click **Resonite ▸ Export Selected Avatar to GLB (baked)** menu: bakes the avatar through
  NDMF / Modular Avatar (non-destructively) so Merge-Armature accessories keep correct weights, then
  exports a single `.glb` with embedded textures via glTFast.
- **Resonite ▸ Export Selected to GLB (raw, no bake)** menu for plain models / non-VRChat rigs.
- Workaround for glTFast's `ArgumentNullException: key` crash when a SkinnedMeshRenderer has a null
  bone: null bones are replaced with a valid fallback before export.
- Disabled toggle/clothing meshes are included (`OnlyActiveInHierarchy = false`).
- glTFast declared as a package dependency; NDMF is optional (graceful fallback when absent).
