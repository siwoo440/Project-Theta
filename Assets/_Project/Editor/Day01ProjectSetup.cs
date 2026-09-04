using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTheta.Editor
{
    [InitializeOnLoad]
    public static class Day01ProjectSetup
    {
        private const string Root = "Assets/_Project";
        private const string SceneFolder = Root + "/Scenes";
        private const string DevSpritePath = Root + "/Art/Sprites/Dev/DevSquare.png";
        private const string InputActionsPath = Root + "/Settings/ProjectInput.inputactions";

        private static readonly string[] RequiredFolders =
        {
            Root + "/Art/Characters",
            Root + "/Art/Environment",
            Root + "/Art/UI",
            Root + "/Art/VFX",
            Root + "/Art/Sprites/Dev",
            Root + "/Audio/BGM",
            Root + "/Audio/SFX",
            Root + "/Audio/Voice",
            Root + "/Data",
            Root + "/Materials",
            Root + "/Prefabs/Characters",
            Root + "/Prefabs/NPC",
            Root + "/Prefabs/Environment",
            Root + "/Prefabs/UI",
            SceneFolder,
            Root + "/Scripts/Core",
            Root + "/Scripts/Player",
            Root + "/Scripts/NPC",
            Root + "/Scripts/Hypnosis",
            Root + "/Scripts/Follower",
            Root + "/Scripts/Stage",
            Root + "/Scripts/UI",
            Root + "/Scripts/Utilities",
            Root + "/Settings",
            Root + "/Tests",
            Root + "/Editor",
            "Assets/ThirdParty"
        };

        private static readonly string[] RequiredTags = { "Player", "NPC" };
        private static readonly string[] RequiredLayers = { "Player", "NPC", "Environment", "Interactable" };
        private static readonly string[] RequiredSortingLayers =
        {
            "Background",
            "Environment_Back",
            "Characters",
            "Environment_Front",
            "VFX",
            "UI"
        };

        static Day01ProjectSetup()
        {
            EditorApplication.delayCall += AutoApplyOnce;
        }

        [MenuItem("Project Theta/Day 01/Apply Project Setup")]
        public static void ApplyProjectSetup()
        {
            try
            {
                EnsureFolders();
                ConfigureDevSprite();
                EnsureTagsAndLayers();
                EnsureSortingLayers();
                EnsureScenes();
                EnsureBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[Project Theta] Day 01 project setup completed.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[Project Theta] Day 01 setup failed:\n" + exception);
            }
        }

        private static void AutoApplyOnce()
        {
            string key = "ProjectTheta.Day01Setup." + Application.dataPath.GetHashCode();

            if (EditorPrefs.GetBool(key, false))
                return;

            ApplyProjectSetup();
            EditorPrefs.SetBool(key, true);
        }

        private static void EnsureFolders()
        {
            foreach (string folder in RequiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    Directory.CreateDirectory(Path.GetFullPath(folder));
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigureDevSprite()
        {
            if (!File.Exists(DevSpritePath))
                return;

            AssetDatabase.ImportAsset(DevSpritePath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(DevSpritePath) is TextureImporter importer)
            {
                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (changed)
                    importer.SaveAndReimport();
            }
        }

        private static UnityEngine.Object GetTagManagerAsset()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            return assets != null && assets.Length > 0 ? assets[0] : null;
        }

        private static void EnsureTagsAndLayers()
        {
            UnityEngine.Object tagManagerAsset = GetTagManagerAsset();
            if (tagManagerAsset == null)
            {
                Debug.LogWarning("[Project Theta] TagManager.asset could not be loaded.");
                return;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAsset);
            SerializedProperty tags = tagManager.FindProperty("tags");
            SerializedProperty layers = tagManager.FindProperty("layers");

            if (tags != null)
            {
                foreach (string tag in RequiredTags)
                    AddUniqueString(tags, tag);
            }

            if (layers != null)
            {
                foreach (string layer in RequiredLayers)
                    AddLayerToFirstEmptyUserSlot(layers, layer);
            }

            tagManager.ApplyModifiedProperties();
        }

        private static void AddUniqueString(SerializedProperty array, string value)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).stringValue == value)
                    return;
            }

            int index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            array.GetArrayElementAtIndex(index).stringValue = value;
        }

        private static void AddLayerToFirstEmptyUserSlot(SerializedProperty layers, string layerName)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                    return;
            }

            for (int i = 8; i < Mathf.Min(32, layers.arraySize); i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrWhiteSpace(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    return;
                }
            }

            Debug.LogWarning("[Project Theta] No empty user layer slot for: " + layerName);
        }

        private static void EnsureSortingLayers()
        {
            UnityEngine.Object tagManagerAsset = GetTagManagerAsset();
            if (tagManagerAsset == null)
                return;

            SerializedObject tagManager = new SerializedObject(tagManagerAsset);
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

            if (sortingLayers == null)
            {
                Debug.LogWarning("[Project Theta] Sorting Layer data could not be found. Add Sorting Layers manually if needed.");
                return;
            }

            foreach (string sortingLayerName in RequiredSortingLayers)
            {
                if (ContainsSortingLayer(sortingLayers, sortingLayerName))
                    continue;

                int index = sortingLayers.arraySize;
                sortingLayers.InsertArrayElementAtIndex(index);
                SerializedProperty element = sortingLayers.GetArrayElementAtIndex(index);

                SerializedProperty name = element.FindPropertyRelative("name");
                SerializedProperty uniqueId = element.FindPropertyRelative("uniqueID");
                SerializedProperty locked = element.FindPropertyRelative("locked");

                if (name != null)
                    name.stringValue = sortingLayerName;

                if (uniqueId != null)
                {
                    int candidate = Guid.NewGuid().GetHashCode() & 0x7fffffff;
                    if (candidate == 0)
                        candidate = 1;
                    uniqueId.intValue = candidate;
                }

                if (locked != null)
                    locked.boolValue = false;
            }

            tagManager.ApplyModifiedProperties();
        }

        private static bool ContainsSortingLayer(SerializedProperty sortingLayers, string layerName)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                SerializedProperty element = sortingLayers.GetArrayElementAtIndex(i);
                SerializedProperty name = element.FindPropertyRelative("name");

                if (name != null && name.stringValue == layerName)
                    return true;
            }

            return false;
        }

        private static void EnsureScenes()
        {
            EnsureScene(SceneFolder + "/Boot.unity", CreateBootScene);
            EnsureScene(SceneFolder + "/MainMenu.unity", CreateMainMenuScene);
            EnsureScene(SceneFolder + "/TestStage.unity", CreateTestStageScene);
        }

        private static void EnsureScene(string path, Action<Scene> populate)
        {
            if (File.Exists(path))
                return;

            Scene previousActive = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            SceneManager.SetActiveScene(scene);
            populate(scene);
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);

            if (previousActive.IsValid() && previousActive.isLoaded)
                SceneManager.SetActiveScene(previousActive);
        }

        private static void CreateBootScene(Scene scene)
        {
            GameObject systems = new GameObject("Systems");
            SceneManager.MoveGameObjectToScene(systems, scene);

            GameObject boot = new GameObject("ProjectTheta_Bootstrap");
            boot.transform.SetParent(systems.transform);

            CreateCamera(scene);
        }

        private static void CreateMainMenuScene(Scene scene)
        {
            CreateRoot(scene, "Systems");
            CreateRoot(scene, "UI");
            CreateCamera(scene);
        }

        private static void CreateTestStageScene(Scene scene)
        {
            GameObject systems = CreateRoot(scene, "Systems");
            GameObject environment = CreateRoot(scene, "Environment");
            GameObject characters = CreateRoot(scene, "Characters");
            GameObject gameplay = CreateRoot(scene, "Gameplay");
            CreateRoot(scene, "UI");

            Camera camera = CreateCamera(scene);
            camera.transform.SetParent(systems.transform);

            Sprite devSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DevSpritePath);
            if (devSprite == null)
            {
                Debug.LogWarning("[Project Theta] DevSquare sprite was not loaded. TestStage roots were still created.");
                return;
            }

            CreateSpriteObject(
                scene, characters.transform, "PlayerPlaceholder", devSprite,
                new Vector3(0f, 0f, 0f), new Vector3(0.7f, 1.4f, 1f),
                new Color(0.25f, 0.65f, 1f, 1f), "Characters", "Player", "Player"
            );

            CreateSpriteObject(
                scene, characters.transform, "NPCPlaceholder", devSprite,
                new Vector3(2f, 0.8f, 0f), new Vector3(0.7f, 1.4f, 1f),
                new Color(1f, 0.45f, 0.55f, 1f), "Characters", "NPC", "NPC"
            );

            CreateSpriteObject(
                scene, environment.transform, "GroundPlaceholder", devSprite,
                new Vector3(0f, -2.5f, 0f), new Vector3(12f, 0.65f, 1f),
                new Color(0.28f, 0.28f, 0.32f, 1f), "Environment_Back", null, "Environment"
            );

            CreateSpriteObject(
                scene, gameplay.transform, "RecoveryPointPlaceholder", devSprite,
                new Vector3(-3f, 1.4f, 0f), new Vector3(1.4f, 1.4f, 1f),
                new Color(0.45f, 1f, 0.55f, 0.55f), "Environment_Back", null, "Interactable"
            );
        }

        private static GameObject CreateRoot(Scene scene, string name)
        {
            GameObject root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static Camera CreateCamera(Scene scene)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 1f);

            cameraObject.tag = "MainCamera";
            return camera;
        }

        private static GameObject CreateSpriteObject(
            Scene scene,
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector3 position,
            Vector3 scale,
            Color color,
            string sortingLayer,
            string tagName,
            string layerName)
        {
            GameObject go = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;

            if (!string.IsNullOrEmpty(tagName) && TagExists(tagName))
                go.tag = tagName;

            if (!string.IsNullOrEmpty(layerName))
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer >= 0)
                    go.layer = layer;
            }

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;

            if (SortingLayerExists(sortingLayer))
                renderer.sortingLayerName = sortingLayer;

            renderer.sortingOrder = -(int)(position.y * 100f);
            return go;
        }

        private static bool TagExists(string tagName)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(tagName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool SortingLayerExists(string sortingLayerName)
        {
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                if (layer.name == sortingLayerName)
                    return true;
            }

            return false;
        }

        private static void EnsureBuildSettings()
        {
            string[] desired =
            {
                SceneFolder + "/Boot.unity",
                SceneFolder + "/MainMenu.unity",
                SceneFolder + "/TestStage.unity"
            };

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            HashSet<string> seen = new HashSet<string>();

            foreach (string path in desired)
            {
                if (File.Exists(path) && seen.Add(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (seen.Add(scene.path))
                    scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
