using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VF.Builder;
using VF.Feature.Base;

namespace VF.Feature {
    /** This builder is responsible for moving objects for other builders,
     * then fixing any animations that referenced those objects.
     */
    public class ObjectMoveBuilder : FeatureBuilder {
        private List<Tuple<string, string>> redirects = new List<Tuple<string, string>>();
        private readonly List<EasyAnimationClip> additionalClips = new List<EasyAnimationClip>();

        public void Move(GameObject obj, GameObject newParent = null, string newName = null, bool worldPositionStays = true) {
            var oldPath = clipBuilder.GetPath(obj);
            if (newParent != null)
                obj.transform.SetParent(newParent.transform, worldPositionStays);
            if (newName != null)
                obj.name = newName;
            var newPath = clipBuilder.GetPath(obj);
            redirects.Add(Tuple.Create(oldPath, newPath));
        }

        public void AddDirectRewrite(GameObject oldObj, GameObject newObj) {
            var oldPath = clipBuilder.GetPath(oldObj);
            var newPath = clipBuilder.GetPath(newObj);
            redirects.Add(Tuple.Create(oldPath, newPath));
        }

        public void AddAdditionalManagedClip(EasyAnimationClip clip) {
            additionalClips.Add(clip);
        }
        
        [FeatureBuilderAction(FeatureOrder.ObjectMoveBuilderFixAnimations)]
        public void FixAnimations() {
            if (redirects.Count == 0) return;
            
            var clips = new HashSet<EasyAnimationClip>();
            var masks = new HashSet<AvatarMask>();

            clips.UnionWith(additionalClips);

            foreach (var controller in manager.GetAllUsedControllers()) {
                controller.ForEachClip(clip => {
                    clips.Add(clip);
                });
                
                var layers = controller.GetLayers().ToList();
                for (var layerId = 0; layerId < layers.Count; layerId++) {
                    var mask = controller.GetMask(layerId);
                    if (mask != null) masks.Add(mask);
                }
            }

            foreach (var clip in clips) {
                foreach (var binding in clip.GetFloatBindings()) {
                    var oldPath = binding.path;
                    var newPath = RewritePath(oldPath);
                    if (oldPath != newPath) {
                        var newBinding = binding;
                        newBinding.path = newPath;
                        clip.SetFloatCurve(newBinding, clip.GetFloatCurve(binding));
                        clip.SetFloatCurve(binding, null);
                    }
                }

                foreach (var binding in clip.GetObjectBindings()) {
                    var oldPath = binding.path;
                    var newPath = RewritePath(oldPath);
                    if (oldPath != newPath) {
                        var newBinding = binding;
                        newBinding.path = newPath;
                        clip.SetObjectCurve(newBinding, clip.GetObjectCurve(binding));
                        clip.SetObjectCurve(binding, null);
                    }
                }
            }

            foreach (var mask in masks) {
                for (var i = 0; i < mask.transformCount; i++) {
                    var oldPath = mask.GetTransformPath(i);
                    var newPath = RewritePath(oldPath);
                    if (oldPath != newPath) {
                        mask.SetTransformPath(i, newPath);
                    }
                }
            }
        }

        private string RewritePath(string path) {
            foreach (var redirect in redirects) {
                var (from, to) = redirect;
                if (path.StartsWith(from + "/") || path == from) {
                    path = to + path.Substring(from.Length);
                }
            }
            return path;
        }
    }
}
