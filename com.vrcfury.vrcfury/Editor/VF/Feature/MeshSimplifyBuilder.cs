using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VF.Feature.Base;
using VF.Inspector;
using VF.Model.Feature;
using VRC.SDKBase;
using UnityMeshSimplifier;

namespace VF.Feature {
    public class MeshSimplifyBuilder : FeatureBuilder<MeshSimplify> {
        public override string GetEditorTitle() {
            return "Mesh Simplifier";
        }
        
        public override VisualElement CreateEditor(SerializedProperty prop) {
            var content = new VisualElement();
            content.Add(VRCFuryEditorUtils.Info(
                "This feature will automatically decimate a provided mesh at runtime, reducing polycount."
            ));
            
            content.Add(VRCFuryEditorUtils.Prop(prop.FindPropertyRelative("singleRenderer"), "Mesh To Optimize"));
            content.Add(VRCFuryEditorUtils.Prop(prop.FindPropertyRelative("quality"), "Decimation Quality"));
            
            return content;
        }

        public override bool AvailableOnProps() {
            return true;
        }

        [FeatureBuilderAction(FeatureOrder.MeshSimplifier)]
        public void Apply() {

            SkinnedMeshRenderer renderer = model.singleRenderer;

            if (renderer == null) return;

            Mesh sourceMesh = renderer.sharedMesh;
            if (sourceMesh == null) // verify that the mesh filter actually has a mesh
                return;

            // Create our mesh simplifier and setup our entire mesh in it
            var meshSimplifier = new MeshSimplifier();
            meshSimplifier.Initialize(sourceMesh);

            // This is where the magic happens, lets simplify!
            meshSimplifier.SimplifyMesh(model.quality);

            // Create our final mesh and apply it back to our mesh filter
            renderer.sharedMesh = meshSimplifier.ToMesh();

        }
    }
}
