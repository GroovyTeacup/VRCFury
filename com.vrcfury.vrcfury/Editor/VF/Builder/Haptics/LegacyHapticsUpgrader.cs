using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VF.Component;
using VF.Inspector;
using VF.Menu;
using VF.Model;
using VF.Model.StateAction;
using VRC.SDK3.Dynamics.Contact.Components;
using Component = UnityEngine.Component;

namespace VF.Builder.Haptics {
    public static class LegacyHapticsUpgrader {
        private static string dialogTitle = "VRCFury Legacy Haptics Upgrader";
        
        public static void Run() {
            var avatarObject = MenuUtils.GetSelectedAvatar();
            if (avatarObject == null) { 
                avatarObject = Selection.activeGameObject;
                while (avatarObject.transform.parent != null) avatarObject = avatarObject.transform.parent.gameObject;
            }

            var messages = Apply(avatarObject, true);
            if (string.IsNullOrWhiteSpace(messages)) {
                EditorUtility.DisplayDialog(
                    dialogTitle,
                    "VRCFury failed to find any parts to upgrade! Ask on the discord?",
                    "Ok"
                );
                return;
            }
        
            var doIt = EditorUtility.DisplayDialog(
                dialogTitle,
                messages + "\n\nContinue?",
                "Yes, Do it!",
                "Cancel"
            );
            if (!doIt) return;

            Apply(avatarObject, false);
            EditorUtility.DisplayDialog(
                dialogTitle,
                "Upgrade complete!",
                "Ok"
            );

            SceneView sv = EditorWindow.GetWindow<SceneView>();
            if (sv != null) sv.drawGizmos = true;
        }

        public static bool Check() {
            if (Selection.activeGameObject == null) return false;
            return true;
        }

        private static bool IsHapticContact(UnityEngine.Component c, List<string> collisionTags) {
            if (collisionTags.Any(t => t.StartsWith("TPSVF_"))) return true;
            else if (c.gameObject.name.StartsWith("OGB_")) return true;
            return false;
        }

        public static string Apply(GameObject avatarObject, bool dryRun) {
            var objectsToDelete = new List<GameObject>();
            var componentsToDelete = new List<UnityEngine.Component>();
            var hasExistingSocket = new HashSet<Transform>();
            var hasExistingPlug = new HashSet<Transform>();
            var addedSocket = new HashSet<Transform>();
            var addedSocketNames = new Dictionary<Transform,string>();
            var addedPlug = new HashSet<Transform>();
            var foundParentConstraint = false;

            bool AlreadyExistsAboveOrBelow(GameObject obj, IEnumerable<Transform> list) {
                var parentIsDeleted = obj.GetComponentsInParent<Transform>(true)
                    .Any(t => objectsToDelete.Contains(t.gameObject));
                if (parentIsDeleted) return true;
                return obj.GetComponentsInChildren<Transform>(true)
                    .Concat(obj.GetComponentsInParent<Transform>(true))
                    .Any(list.Contains);
            }

            string GetPath(Transform obj) {
                return AnimationUtility.CalculateTransformPath(obj, avatarObject.transform);
            }
            VRCFuryHapticPlug AddPlug(GameObject obj) {
                if (AlreadyExistsAboveOrBelow(obj, hasExistingPlug.Concat(addedPlug))) return null;
                addedPlug.Add(obj.transform);
                if (dryRun) return null;
                return obj.AddComponent<VRCFuryHapticPlug>();
            }
            VRCFuryHapticSocket AddSocket(GameObject obj) {
                if (AlreadyExistsAboveOrBelow(obj, hasExistingSocket.Concat(addedSocket))) return null;
                addedSocket.Add(obj.transform);
                if (dryRun) return null;
                var socket = obj.AddComponent<VRCFuryHapticSocket>();
                socket.addLight = VRCFuryHapticSocket.AddLight.None;
                socket.addMenuItem = false;
                return socket;
            }

            foreach (var c in avatarObject.GetComponentsInChildren<VRCFuryHapticPlug>(true)) {
                hasExistingPlug.Add(c.transform);
                foreach (var renderer in VRCFuryHapticPlugEditor.GetRenderers(c)) {
                    hasExistingPlug.Add(renderer.transform);
                }
            }
            foreach (var c in avatarObject.GetComponentsInChildren<VRCFuryHapticSocket>(true)) {
                hasExistingSocket.Add(c.transform);
            }
            
            // Upgrade "parent-constraint" DPS setups
            foreach (var parent in avatarObject.GetComponentsInChildren<Transform>(true)) {
                var constraint = parent.gameObject.GetComponent<ParentConstraint>();
                if (constraint == null) continue;
                if (constraint.sourceCount < 2) continue;
                var sourcesWithWeight = 0;
                for (var i = 0; i < constraint.sourceCount; i++) {
                    if (constraint.GetSource(i).weight > 0) sourcesWithWeight++;
                }
                if (sourcesWithWeight > 1) {
                    // This is probably not a parent constraint socket, but rather an actual position splitter.
                    // (used to position a socket between two bones)
                    continue;
                }
                
                var parentInfo = GetIsParent(parent.gameObject);
                if (parentInfo == null) continue;

                var parentLightType = parentInfo.Item1;
                var parentPosition = parentInfo.Item2;
                var parentRotation = parentInfo.Item3;

                foundParentConstraint = true;
                objectsToDelete.Add(parent.gameObject);
                
                for (var i = 0; i < constraint.sourceCount; i++) {
                    var source = constraint.GetSource(i);
                    var sourcePositionOffset = constraint.GetTranslationOffset(i);
                    var sourceRotationOffset = Quaternion.Euler(constraint.GetRotationOffset(i));
                    var t = source.sourceTransform;
                    if (t == null) continue;
                    var obj = t.gameObject;
                    var name = obj.name;
                    var id = name.IndexOf("(");
                    if (id >= 0) name = name.Substring(id+1);
                    id = name.IndexOf(")");
                    if (id >= 0) name = name.Substring(0, id);
                    // Convert camel case to spaces
                    name = name.Replace("EZDPS_CB_", "");
                    name = name.Replace("EZDPS_OB", "");
                    name = name.Replace("EZDPS_OA", "");
                    name = name.Replace("EZDPS_T_", "");
                    name = name.Replace("EZDPS_", "");
                    name = name.Replace("Adv_", "");
                    name = Regex.Replace(name, "(\\B[A-Z])", " $1");
                    name = name.ToLower();
                    name = name.Replace(VRCFuryEditorUtils.Rev("spd"), "");
                    name = name.Replace(VRCFuryEditorUtils.Rev("ecifiro"), "");
                    name = name.Replace('_', ' ');
                    name = name.Replace('-', ' ');
                    while (name.Contains("  ")) {
                        name = name.Replace("  ", " ");
                    }
                    name = name.Trim();
                    name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);

                    var fullName = "Socket (" + name + ")";

                    var socket = AddSocket(obj);
                    addedSocketNames[obj.transform] = name;
                    if (socket != null) {
                        socket.position = (sourcePositionOffset + sourceRotationOffset * parentPosition)
                            * constraint.transform.lossyScale.x / socket.transform.lossyScale.x;
                        socket.rotation = (sourceRotationOffset * parentRotation).eulerAngles;
                        socket.addLight = VRCFuryHapticSocket.AddLight.Auto;
                        socket.name = name;
                        socket.addMenuItem = true;
                        obj.name = fullName;
                        
                        if (name.ToLower().Contains("vag")) {
                            AddBlendshapeIfPresent(avatarObject, socket, VRCFuryEditorUtils.Rev("2ECIFIRO"), -0.03f, 0);
                        }
                        if (VRCFuryHapticSocketEditor.ShouldProbablyHaveTouchZone(socket)) {
                            AddBlendshapeIfPresent(avatarObject, socket, VRCFuryEditorUtils.Rev("egluBymmuT"), 0, 0.15f);
                        }
                    }
                }
            }
            
            // Un-bake baked components
            foreach (var t in avatarObject.GetComponentsInChildren<Transform>(true)) {
                if (!t) continue; // this can happen if we're visiting one of the things we deleted below

                void UnbakePen(Transform baked) {
                    if (!baked) return;
                    var info = baked.Find("Info");
                    if (!info) info = baked;
                    var p = AddPlug(baked.parent.gameObject);
                    if (p) {
                        var size = info.Find("size");
                        if (size) {
                            p.length = size.localScale.x;
                            p.radius = size.localScale.y;
                        }
                        p.name = GetNameFromBakeInfo(info.gameObject);
                    }
                    objectsToDelete.Add(baked.gameObject);
                }
                void UnbakeOrf(Transform baked) {
                    if (!baked) return;
                    var info = baked.Find("Info");
                    if (!info) info = baked;
                    var o = AddSocket(baked.parent.gameObject);
                    if (o) {
                        o.name = GetNameFromBakeInfo(info.gameObject);
                    }
                    objectsToDelete.Add(baked.gameObject);
                }

                UnbakePen(t.Find("OGB_Baked_Pen"));
                UnbakePen(t.Find("BakedOGBPenetrator"));
                UnbakePen(t.Find("BakedHapticPlug"));
                UnbakeOrf(t.Find("OGB_Baked_Orf"));
                UnbakeOrf(t.Find("BakedOGBOrifice"));
                UnbakePen(t.Find("BakedHapticSocket"));
            }
            
            // Auto-add plugs from DPS and TPS
            foreach (var tuple in RendererIterator.GetRenderersWithMeshes(avatarObject)) {
                var (renderer, _, _) = tuple;
                if (PlugSizeDetector.HasDpsMaterial(renderer) && PlugSizeDetector.GetAutoWorldSize(renderer) != null)
                    AddPlug(renderer.gameObject);
            }
            
            // Auto-add sockets from DPS
            foreach (var light in avatarObject.GetComponentsInChildren<Light>(true)) {
                var parent = light.gameObject.transform.parent;
                if (parent) {
                    var parentObj = parent.gameObject;
                    if (VRCFuryHapticSocketEditor.GetInfoFromLights(parentObj, true) != null)
                        AddSocket(parentObj);
                }
            }
            
            // Upgrade old OGB markers to components
            foreach (var t in avatarObject.GetComponentsInChildren<Transform>(true)) {
                if (!t) continue; // this can happen if we're visiting one of the things we deleted below
                var penMarker = t.Find("OGB_Marker_Pen");
                if (penMarker) {
                    AddPlug(t.gameObject);
                    objectsToDelete.Add(penMarker.gameObject);
                }

                var holeMarker = t.Find("OGB_Marker_Hole");
                if (holeMarker) {
                    var o = AddSocket(t.gameObject);
                    if (o) o.addLight = VRCFuryHapticSocket.AddLight.Hole;
                    objectsToDelete.Add(holeMarker.gameObject);
                }
                
                var ringMarker = t.Find("OGB_Marker_Ring");
                if (ringMarker) {
                    var o = AddSocket(t.gameObject);
                    if (o) o.addLight = VRCFuryHapticSocket.AddLight.Ring;
                    objectsToDelete.Add(ringMarker.gameObject);
                }
            }
            
            // Claim lights on all OGB components
            foreach (var transform in hasExistingSocket.Concat(addedSocket)) {
                if (!dryRun) {
                    foreach (var socket in transform.GetComponents<VRCFuryHapticSocket>()) {
                        if (socket.addLight == VRCFuryHapticSocket.AddLight.None) {
                            var info = VRCFuryHapticSocketEditor.GetInfoFromLights(socket.gameObject);
                            if (info != null) {
                                var type = info.Item1;
                                var position = info.Item2;
                                var rotation = info.Item3;
                                socket.addLight = type;
                                socket.position = position;
                                socket.rotation = rotation.eulerAngles;
                            }
                        }
                    }
                }

                VRCFuryHapticSocketEditor.ForEachPossibleLight(transform, false, light => {
                    componentsToDelete.Add(light);
                });
            }

            // Clean up
            var deletions = AvatarCleaner.Cleanup(
                avatarObject,
                perform: !dryRun,
                ShouldRemoveObj: obj => {
                    return obj.name == "GUIDES_DELETE"
                           || objectsToDelete.Contains(obj);
                },
                ShouldRemoveAsset: asset => {
                    if (asset == null) return false;
                    var path = AssetDatabase.GetAssetPath(asset);
                    if (path == null) return false;
                    var lower = path.ToLower();
                    if (lower.Contains("dps_attach")) return true;
                    return false;
                },
                ShouldRemoveLayer: layer => {
                    var lower = layer.ToLower();
                    if (foundParentConstraint && lower.Contains("tps") && lower.Contains("orifice")) {
                        return true;
                    }
                    if (foundParentConstraint && layer == "EZDPS Orifices") {
                        return true;
                    }
                    return layer == "DPS_Holes"
                           || layer == "DPS_Rings"
                           || layer == "HotDog"
                           || layer == "DPS Orifice"
                           || layer == "Orifice Position";
                },
                ShouldRemoveParam: param => {
                    return param == "DPS_Hole"
                           || param == "DPS_Ring"
                           || param == "HotDog"
                           || param == "fluff/dps/orifice"
                           || param == "EZDPS/Orifice"
                           || (param.StartsWith("TPS") && param.Contains("/VF"))
                           || param.StartsWith("OGB/")
                           || param.StartsWith("Nsfw/Ori/");
                },
                ShouldRemoveComponent: component => {
                    if (component is VRCContactSender sender && IsHapticContact(sender, sender.collisionTags)) return true;
                    if (component is VRCContactReceiver rcv && IsHapticContact(rcv, rcv.collisionTags)) return true;
                    if (componentsToDelete.Contains(component)) return true;
                    return false;
                }
            );

            var parts = new List<string>();
            var alreadyExists = hasExistingSocket
                .Concat(hasExistingPlug)
                .ToImmutableHashSet();
            if (addedPlug.Count > 0)
                parts.Add("Plug component will be added to:\n" + string.Join("\n", addedPlug.Select(GetPath)));

            string GetSocketLine(Transform t) {
                if (addedSocketNames.ContainsKey(t)) {
                    return GetPath(t) + " (" + addedSocketNames[t] + ")";
                }
                return GetPath(t);
            }
            if (addedSocket.Count > 0)
                parts.Add("Socket component will be added to:\n" + string.Join("\n", addedSocket.Select(GetSocketLine)));
            if (deletions.Count > 0)
                parts.Add("These objects will be deleted:\n" + string.Join("\n", deletions));
            if (alreadyExists.Count > 0)
                parts.Add("Haptics already exists on:\n" + string.Join("\n", alreadyExists.Select(GetPath)));

            if (parts.Count == 0) return "";
            return string.Join("\n\n", parts);
        }

        private static string GetNameFromBakeInfo(GameObject marker) {
            foreach (Transform child in marker.transform) {
                if (child.name.StartsWith("name=")) {
                    return child.name.Substring(5);
                }
            }
            return "";
        }

        private static Tuple<VRCFuryHapticSocket.AddLight, Vector3, Quaternion> GetIsParent(GameObject obj) {
            var lightInfo = VRCFuryHapticSocketEditor.GetInfoFromLights(obj, true);
            if (lightInfo == null) {
                var child = obj.transform.Find("Orifice");
                if (child != null && obj.transform.childCount == 1) {
                    lightInfo = VRCFuryHapticSocketEditor.GetInfoFromLights(child.gameObject, true);
                }
            }
            if (lightInfo != null) {
                return lightInfo;
            }

            // For some reason, on some avatars, this one doesn't have child lights even though it's supposed to
            if (obj.name == "__dps_lightobject") {
                return Tuple.Create(VRCFuryHapticSocket.AddLight.Ring, Vector3.zero, Quaternion.Euler(90, 0, 0));
            }

            return null;
        }

        private static void AddBlendshapeIfPresent(
            GameObject avatarObject,
            VRCFuryHapticSocket orf,
            string name,
            float minDepth,
            float maxDepth
        ) {
            if (HasBlendshape(avatarObject, name)) {
                orf.depthActions.Add(new VRCFuryHapticSocket.DepthAction() {
                    state = new State() {
                        actions = {
                            new BlendShapeAction {
                                blendShape = name
                            }
                        }
                    },
                    minDepth = minDepth,
                    maxDepth = maxDepth
                });
            }
        }
        private static bool HasBlendshape(GameObject avatarObject, string name) {
            var skins = avatarObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skin in skins) {
                if (!skin.sharedMesh) continue;
                var blendShapeIndex = skin.sharedMesh.GetBlendShapeIndex(name);
                if (blendShapeIndex < 0) continue;
                return true;
            }
            return false;
        }
    }
}
