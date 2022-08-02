using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VF.Builder;
using VF.Model;

namespace VF.Menu {
    public class VRCFuryForceRunMenuItem {
        public static void Run() {
            var obj = MenuUtils.GetSelectedAvatar();
            var builder = new VRCFuryBuilder();
            builder.SafeRun(obj);
        }

        public static bool Check() {
            var obj = MenuUtils.GetSelectedAvatar();
            if (obj == null) return false;
            if (obj.GetComponentsInChildren<VRCFury>(true).Length > 0) return true;
            return false;
        }
        
        public static void RunFakeUpload() {
            var obj = MenuUtils.GetSelectedAvatar();
            var clone = Object.Instantiate(obj);
            if (clone.scene != obj.scene) SceneManager.MoveGameObjectToScene(clone, obj.scene);
            var builder = new VRCFuryBuilder();
            builder.SafeRun(obj, clone);
        }

        public static bool CheckFakeUpload() {
            var obj = MenuUtils.GetSelectedAvatar();
            if (obj == null) return false;
            if (obj.GetComponentsInChildren<VRCFury>(true).Length > 0) return true;
            return false;
        }
        
        public static void RunPurge() {
            var obj = MenuUtils.GetSelectedAvatar();
            VRCFuryBuilder.DetachFromAvatar(obj);
        }

        public static bool CheckPurge() {
            var obj = MenuUtils.GetSelectedAvatar();
            if (obj == null) return false;
            return true;
        }
    }
}
