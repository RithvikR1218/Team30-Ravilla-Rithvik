using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BeatTiming.EditorTools
{
    /// <summary>
    /// Menu: Tools > Beat Timing > Add To Current Scene.
    /// Adds the Beat System and puts the pulse ring on the selected object (or the object named "Player").
    /// Everything it adds can be undone with Ctrl/Cmd+Z.
    /// </summary>
    public static class BeatSceneSetup
    {
        [MenuItem("Tools/Beat Timing/Add To Current Scene")]
        public static void AddToCurrentScene()
        {
            GameObject player = Selection.activeGameObject;
            if (player == null || EditorUtility.IsPersistent(player)) player = GameObject.Find("Player");
            if (player == null)
            {
                EditorUtility.DisplayDialog("Beat Timing", "Select the player object in the Hierarchy first (or name it \"Player\").", "OK");
                return;
            }
            AddTo(player);
            Selection.activeGameObject = player;
        }

        public static void AddTo(GameObject player)
        {
            if (Object.FindFirstObjectByType<BeatConductor>() == null)
            {
                var system = new GameObject("Beat System");
                Undo.RegisterCreatedObjectUndo(system, "Add Beat System");
                system.AddComponent<BeatConductor>();
                system.AddComponent<TimingJudge>();
                system.AddComponent<BeatMetronome>();
                system.AddComponent<BeatDebugInput>();
            }

            var ring = player.GetComponent<BeatPulseRing>();
            if (ring == null) ring = Undo.AddComponent<BeatPulseRing>(player);

            // Size the ring to the player's sprite so it closes just outside the character.
            var sr = player.GetComponentInChildren<Renderer>();
            if (sr != null)
            {
                Vector3 size = sr.bounds.size;
                float body = Mathf.Max(size.x, size.y);
                var so = new SerializedObject(ring);
                so.FindProperty("endDiameter").floatValue = body * 1.15f;
                so.FindProperty("startDiameter").floatValue = body * 2.4f;
                so.FindProperty("flashDiameter").floatValue = body * 1.4f;
                so.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(player.scene);
            Debug.Log($"Beat Timing: added Beat System and pulse ring on '{player.name}'. Press Play and hit Space on the beat.");
        }
    }
}
