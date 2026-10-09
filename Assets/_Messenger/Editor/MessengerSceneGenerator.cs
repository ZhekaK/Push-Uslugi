using System;
using System.IO;
using System.Linq;
using PushPelmesh.Messenger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PushPelmesh.Messenger.Editor
{
    public static class MessengerSceneGenerator
    {
        private const string ScenePath = "Assets/_Messenger/Scenes/MessengerScene.unity";

        [MenuItem("Tools/Push Uslugi/Messenger/Create Messenger Scene")]
        public static void Generate()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log("Messenger scene already exists; keeping its current UI.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var controller = new GameObject("MessengerController").AddComponent<MessengerScreen>();
            controller.BakeEditableUiInEditor();
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(x => x.path == ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Created MessengerScene and added it to Build Settings.");
        }

        [MenuItem("Tools/Push Uslugi/Messenger/Bake Existing Scene UI")]
        public static void BakeExistingScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(ScenePath))
                return;

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.isLoaded;
            try
            {
                if (openedHere)
                    scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                else if (scene.isDirty)
                {
                    Debug.LogWarning("MessengerScene has unsaved edits; save it before baking the UI.");
                    return;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                if (roots.Any(x => x.GetComponentInChildren<Canvas>(true) != null))
                    return;

                MessengerScreen controller = roots.Select(x => x.GetComponent<MessengerScreen>())
                    .FirstOrDefault(x => x != null);
                if (controller == null)
                    throw new InvalidOperationException("MessengerController was not found in MessengerScene.");

                SceneManager.SetActiveScene(scene);
                controller.BakeEditableUiInEditor();
                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("MessengerScene now contains editable Canvas, panels, controls, and row templates.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Could not bake MessengerScene UI: " + exception);
            }
            finally
            {
                if (previousScene.IsValid() && previousScene.isLoaded && previousScene != scene)
                    SceneManager.SetActiveScene(previousScene);
                if (openedHere && scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
