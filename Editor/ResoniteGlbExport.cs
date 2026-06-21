// Resonite GLB Avatar Exporter
// -----------------------------
// Exports a Unity avatar to a single .glb for native drag-and-drop import into Resonite.
// This deliberately avoids the live-bridge exporters (no Resonite mod, no renderer, no headless
// engine) — it just writes a .glb that Resonite's own glTF importer loads: meshes, skeleton,
// blendshapes (morph targets) and embedded textures.
//
// "Baked" export first runs the avatar through NDMF / Modular Avatar (non-destructively), so
// Merge-Armature accessories (tails, ears, clothing, ...) get their bones merged into the main
// skeleton and keep correct weights. Without baking those accessory meshes export with broken skin.
//
// Requires the glTFast package (com.unity.cloud.gltfast, declared as a dependency).
// NDMF (nadena.dev.ndmf) is optional and only needed for the "baked" path on VRChat/MA avatars.
#if HAS_GLTFAST
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Export;
using UnityEditor;
using UnityEngine;

namespace ResoniteGlbExporter
{
    public static class ResoniteGlbExport
    {
        // VRChat / Modular Avatar avatars: bake first so accessory weights are correct.
        [MenuItem("Resonite/Export Selected Avatar to GLB (baked)", false, 0)]
        static void ExportBakedMenu() => Run(bake: true);

        // Plain models / non-VRChat rigs: export the selection as-is.
        [MenuItem("Resonite/Export Selected to GLB (raw, no bake)", false, 1)]
        static void ExportRawMenu() => Run(bake: false);

        static async void Run(bool bake)
        {
            var src = Selection.activeGameObject;
            if (src == null)
            {
                EditorUtility.DisplayDialog("Resonite GLB Export",
                    "Select the avatar / model root in the Hierarchy first, then run this again.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export to GLB (for Resonite)", "", src.name, "glb");
            if (string.IsNullOrEmpty(path)) return;

            GameObject work = null;
            try
            {
                if (bake)
                {
#if HAS_NDMF
                    // NDMF "manual bake": clones the avatar, runs all build phases (Modular Avatar
                    // merge-armature, AAO, VRCFury, ...) on the clone, returns the baked clone.
                    // The original in the scene is left untouched.
                    work = nadena.dev.ndmf.AvatarProcessor.ProcessAvatarUI(src);
                    if (work == null)
                    {
                        EditorUtility.DisplayDialog("Resonite GLB Export", "NDMF bake returned nothing — aborting.", "OK");
                        return;
                    }
                    Debug.Log("[ResoniteGlbExport] NDMF bake done -> " + work.name);
#else
                    work = Object.Instantiate(src);
                    work.name = src.name;
                    Debug.LogWarning("[ResoniteGlbExport] NDMF not installed — exporting WITHOUT bake. " +
                                     "Modular Avatar accessory weights may be broken. Install nadena.dev.ndmf for the baked path.");
#endif
                }
                else
                {
                    work = Object.Instantiate(src);
                    work.name = src.name;
                }

                work.transform.position = Vector3.zero;
                work.transform.rotation = Quaternion.identity;

                int fixedBones = FixNullBones(work);
                Debug.Log($"[ResoniteGlbExport] fixed {fixedBones} null bone slot(s) before export.");

                var exportSettings = new ExportSettings
                {
                    Format = GltfFormat.Binary,                   // .glb, textures embedded
                    FileConflictResolution = FileConflictResolution.Overwrite,
                };
                // include disabled toggle meshes (clothing etc.)
                var goSettings = new GameObjectExportSettings { OnlyActiveInHierarchy = false };

                var export = new GameObjectExport(exportSettings, goSettings);
                export.AddScene(new[] { work }, src.name);
                bool ok = await export.SaveToFileAndDispose(path);

                Debug.Log("[ResoniteGlbExport] " + (ok ? "OK -> " : "FAILED -> ") + path);
                EditorUtility.DisplayDialog("Resonite GLB Export",
                    ok ? ("Exported:\n" + path + "\n\nNow drag this .glb into Resonite, then run Resonite's Avatar Creator.")
                       : "Export FAILED — see the Console for details.",
                    "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[ResoniteGlbExport] " + e);
                EditorUtility.DisplayDialog("Resonite GLB Export", "Exception during export:\n" + e.Message, "OK");
            }
            finally
            {
                if (work != null) Object.DestroyImmediate(work);
            }
        }

        // glTFast 6.x throws "ArgumentNullException: key" if a SkinnedMeshRenderer has a null bone in
        // its bones[] array (common on NDMF/VRCFury-edited VRChat avatars). Replace nulls with a valid
        // fallback so skin export never throws.
        static int FixNullBones(GameObject root)
        {
            int fixedBones = 0;
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var bones = smr.bones;
                if (bones == null || bones.Length == 0) continue;
                Transform fallback = smr.rootBone != null ? smr.rootBone : root.transform;
                bool changed = false;
                for (int i = 0; i < bones.Length; i++)
                    if (bones[i] == null) { bones[i] = fallback; fixedBones++; changed = true; }
                if (changed) smr.bones = bones;
            }
            return fixedBones;
        }
    }
}
#endif
