using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RefMata
{
    public sealed partial class RefMataPostProcessor : AssetPostprocessor
    {
        sealed class EditorCoroutine : IDisposable
        {
            readonly IEnumerator enumerator;
            readonly Action onCompleted;

            public EditorCoroutine(IEnumerator enumerator, Action onCompleted = null)
            {
                this.enumerator = enumerator;
                this.onCompleted = onCompleted;
                EditorApplication.update += Process;
            }

            void Process()
            {
                if (!enumerator.MoveNext()) Dispose();
            }

            public void Dispose()
            {
                EditorApplication.update -= Process;
                onCompleted?.Invoke();
            }
        }

        const string LabelPrefix = "RefMata";
        sealed class RefMataHookable
        {
            public const string Name = nameof(RefMataHookable);
            public const string Label = "l:" + Name;
        }
        const string IndexOfValue = "Assets/";
        sealed class Suffix
        {
            public const string Scene = ".unity";
            public const string Prefab = ".prefab";
            public const string Scriptable = ".asset";
            public const string Script = ".cs";
        }
        const string ProgressBarTitle = nameof(RefMataPostProcessor);
        static readonly Type HookableType = typeof(IRefMataHookable);
        static readonly HashSet<string> fullNames = new(), hookLabels = new();
        static readonly HashSet<string> scenes = new(), prefabs = new(), scriptables = new();
        static readonly List<GameObject> sceneRootGos = new(64);
        static int sceneIndex;
        static bool sceneWait, isProgress;

        [MenuItem("Assets/RefMata/Add Label/Hookable", false, 100)]
        static void AddLabelHookable()
        {
            var labels = new[] { RefMataHookable.Name };
            foreach (var o in Selection.objects)
            {
                Debug.Log(o.name);
                AssetDatabase.SetLabels(o, labels);
                EditorUtility.SetDirty(o);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/RefMata/Add Label/Hook Folder (0)", false, 200)]
        static void AddLabelHooker0() => AddLabelHooker(0);
        [MenuItem("Assets/RefMata/Add Label/Hook Folder (1)", false, 201)]
        static void AddLabelHooker1() => AddLabelHooker(1);
        [MenuItem("Assets/RefMata/Add Label/Hook Folder (2)", false, 202)]
        static void AddLabelHooker2() => AddLabelHooker(2);
        [MenuItem("Assets/RefMata/Add Label/Hook Folder (3)", false, 203)]
        static void AddLabelHooker3() => AddLabelHooker(3);

        static void AddLabelHooker(int parent)
        {
            foreach (var o in Selection.objects)
            {
                var name = o.name;
                if (parent > 0)
                {
                    var di = Directory.GetParent(AssetDatabase.GetAssetPath(o));
                    while (di != null && parent > 0)
                    {
                        name = $"{di.Name}{name}";
                        di = di.Parent;
                        parent--;
                    }
                }
                Debug.Log(name);
                AssetDatabase.SetLabels(o, new[] { $"{LabelPrefix}{name}" });
                EditorUtility.SetDirty(o);
            }
            AssetDatabase.SaveAssets();
        }

        static void GetLabels(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            var di = Directory.GetParent(assetPath);
            while (di != null)
            {
                var fullName = di.FullName;
                if (fullNames.Contains(fullName)) return;
                if (fullName.AsSpan().IndexOf(IndexOfValue) is int indexOf and > 0)
                {
                    fullNames.Add(fullName);
                    hookLabels.UnionWith(
                        AssetDatabase.GetLabels(AssetImporter.GetAtPath(fullName.Substring(indexOf)))
                            .Where(x => x.AsSpan().StartsWith(LabelPrefix))
                    );
                    di = di.Parent;
                    continue;
                }
                return;
            }
        }

        static void GetHookable(string assetPath)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (monoScript == null || monoScript.GetClass() is not Type type) return;
            // NOTE: if use reflection, it may not be possible or slow, so do the following.
            if (!type.GetInterfaces().Contains(HookableType) ||
                Activator.CreateInstance(type) is not IRefMataHookable hookable ||
                !hookable.Kinds.HasFlag(RefMataKinds.Load))
                return;
            hookLabels.UnionWith(hookable.Labels);
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (isProgress) return;
            isProgress = true;

            fullNames.Clear();
            hookLabels.Clear();
            scenes.Clear();
            prefabs.Clear();
            scriptables.Clear();

            // add label to 'folder' contained tracking assets.

            // pick up a moved folder (label).
            // or pick up compiled script.
            foreach (var assetPath in importedAssets)
                if (assetPath.AsSpan().EndsWith(Suffix.Script)) GetHookable(assetPath);
                else GetLabels(assetPath);
            foreach (var assetPath in deletedAssets)
                if (assetPath.AsSpan().EndsWith(Suffix.Script)) GetHookable(assetPath);
                else GetLabels(assetPath);
            foreach (var assetPath in movedAssets)
                if (assetPath.AsSpan().EndsWith(Suffix.Script)) GetHookable(assetPath);
                else GetLabels(assetPath);
            foreach (var assetPath in movedFromAssetPaths)
                if (assetPath.AsSpan().EndsWith(Suffix.Script)) GetHookable(assetPath);
                else GetLabels(assetPath);

            if (hookLabels.Count <= 0) return;

            // search for hook target.
            foreach (var path in AssetDatabase.FindAssets(RefMataHookable.Label)
                .Select(AssetDatabase.GUIDToAssetPath))
            {
                var span = path.AsSpan();
                if (span.EndsWith(Suffix.Scene)) scenes.Add(path);
                else if (span.EndsWith(Suffix.Prefab)) prefabs.Add(path);
                else if (span.EndsWith(Suffix.Scriptable)) scriptables.Add(path);
                else HookAdditionalAssets(path);
            }

            // hook and update 'IRefMataHookable'.
            foreach (var so in scriptables
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>))
            {
                if (so is IRefMataHookable hookable &&
                    hookable.Kinds.HasFlag(RefMataKinds.Load) &&
                    hookLabels.Count > hookLabels.Except(hookable.Labels).Count())
                {
                    hookable.RunLoad(); // there is no hierarchy.
                    EditorUtility.SetDirty(so);
                }
            }

            foreach (var go in prefabs
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>))
            {
                if (FindComponent(go, isPrefab: true) > 0) EditorUtility.SetDirty(go);
            }

            RunAdditionalAssets();

            var currentScenePath = EditorSceneManager.GetActiveScene().path;
            EditorUtility.DisplayCancelableProgressBar(ProgressBarTitle, null, 0);
            EditorSceneManager.sceneOpened += OnOpendScene;
            new EditorCoroutine(SceneCo(), () =>
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(currentScenePath);
                isProgress = false;
            });
        }

        static int FindComponent(GameObject go, bool isPrefab)
        {
            int cnt = 0;
            if (go.TryGetComponent(out IRefMataHookable h1) &&
                (isPrefab || !PrefabUtility.IsPartOfAnyPrefab(go)) &&
                h1.Kinds.HasFlag(RefMataKinds.Load) &&
                hookLabels.Count > hookLabels.Except(h1.Labels).Count())
            {
                h1.RunOnValidate();
                cnt++;
            }
            foreach (var h2 in go.GetComponentsInChildren<IRefMataHookable>(includeInactive: true))
            {
                if (!PrefabUtility.IsPartOfAnyPrefab(h2 as Component) &&
                    h2.Kinds.HasFlag(RefMataKinds.Load) &&
                    hookLabels.Count > hookLabels.Except(h2.Labels).Count())
                {
                    h2.RunOnValidate();
                    cnt++;
                }
            }
            return cnt;
        }

        static IEnumerator SceneCo()
        {
            if (scenes.Count > 0)
            {
                sceneIndex = 0;
                foreach (var path in scenes)
                {
                    sceneWait = true;
                    EditorSceneManager.OpenScene(path);
                    while (sceneWait) yield return null;
                    EditorUtility.DisplayCancelableProgressBar(ProgressBarTitle, null, (float)sceneIndex++ / scenes.Count);
                }
            }
            EditorSceneManager.sceneOpened -= OnOpendScene;
        }

        static void OnOpendScene(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            int cnt = 0;
            scene.GetRootGameObjects(sceneRootGos);
            foreach (var go in sceneRootGos)
            {
                if (FindComponent(go, isPrefab: false) > 0)
                {
                    EditorUtility.SetDirty(go);
                    cnt++;
                }
            }
            if (cnt > 0) EditorSceneManager.SaveScene(scene);
            sceneWait = false;
        }

        /// <summary>
        /// please implement if you have any additional assets to hook.
        /// </summary>
        static partial void HookAdditionalAssets(string path);

        /// <summary>
        /// please implement if you have any additional assets to hook.
        /// </summary>
        static partial void RunAdditionalAssets();
    }
}
