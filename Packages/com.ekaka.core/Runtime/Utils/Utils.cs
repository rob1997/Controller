using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Core.Utils
{
    public static class Utils
    {
        public static void LoadAsset<T>(string address, Action<T> onLoad)
        {
            Addressables.LoadAssetAsync<T>(address).Completed += handle =>
            {
                if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Can't load {typeof(T)} asset at {address}");
                    
                    //in case of exception
                    LogException(handle.OperationException);
                    
                    return;
                }
            
                onLoad?.Invoke(handle.Result);
            };
        }
        
        /// <summary>
        /// load a component on a GameObject from an address
        /// loaded component isn't instantiated/needs instantiation
        /// </summary>
        /// <param name="address">address of GameObject</param>
        /// <param name="onLoad">callback for successful component load</param>
        /// <typeparam name="T">typeof component to be loaded</typeparam>
        public static void LoadObjComponent<T>(string address, Action<T> onLoad) where T : Component
        {
            LoadAsset<GameObject>(address, obj =>
            {
                if (!obj.TryGetComponent(out T component))
                {
                    Debug.LogError($"{typeof(T)} component not found on loaded object");
                    
                    return;
                }
                
                onLoad?.Invoke(component);
            });
        }

        public static void LogException(Exception exception)
        {
            Debug.LogError($"{exception?.Message} {exception?.StackTrace} {exception}");
        }
        
        public static T[] GetEnumValues<T>() where T : struct, Enum
        {
            if (!typeof(T).IsEnum) 
            {
                Debug.LogError($"Argument Error, Type Mismatch ; {typeof(T)} must be of Type Enum");
            
                throw new ArgumentException();
            }
        
            return (T[])Enum.GetValues(typeof(T));
        }
        
        public static Array GetEnumValues(Type type)
        {
            if (!type.IsEnum) 
            {
                throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", nameof(type));
            }
        
            return Enum.GetValues(type);
        }
        
        public static string GetDisplayName(string name)
        {
            string displayName = Regex.Replace($"{name}", "(\\B[A-Z])", " $1");

            //make first char upper case
            return string.Concat(displayName[0].ToString().ToUpper(), displayName.Substring(1));
        }
        
        public static T Cast<T>(object obj)
        {
            try
            {
                return (T) obj;
            }
        
            catch (Exception e)
            {
                Debug.LogError($"Error Trying to Cast From {obj.GetType()} To {typeof(T)} {e}");
            
                throw;
            }
        }

        public static float NormalizeValue(float value, float lowerLimit, float upperLimit)
        {
            float cachedValue = value;
            float cachedLowerLimit = lowerLimit;
            float cachedUpperLimit = upperLimit;
            
            lowerLimit = Mathf.Clamp(lowerLimit, float.MinValue, upperLimit);

            value = Mathf.Clamp(value, lowerLimit, upperLimit);

            value -= lowerLimit;
            
            float delta = upperLimit - lowerLimit;

            if (delta <= 0)
            {
                Debug.Log($"{cachedValue} {cachedLowerLimit} {cachedUpperLimit}");
            }
            
            return value / delta;
        }
        
        public static int[] FindIndexes<T>(this IEnumerable<T> values, Predicate<T> predicate)
        {
            return values.Select((b, i) => predicate.Invoke(b) ? i : - 1).Where(i => i != - 1).ToArray();
        }

        public static void Destroy(this GameObject obj)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }

            else
            {
                Object.DestroyImmediate(obj);
            }
#else
            Object.Destroy(obj);
#endif
        }

        public static string NewGuid()
        {
            //maybe check if it's not a collision/duplicate
            return Guid.NewGuid().ToString();
        }

        public static void OverrideClips(this Animator animator, ClipOverride clipOverride)
        {
            //initialize override controller if there isn't one
            if (!(animator.runtimeAnimatorController is AnimatorOverrideController overrideController))
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            }
            
            List<KeyValuePair<AnimationClip, AnimationClip>> controllerOverrides = 
                new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
            
            overrideController.GetOverrides(controllerOverrides);
            
            foreach (var pair in clipOverride.Overrides.ToList())
            {
                string key = pair.Key;
                
                //clip name must be a static value before hand in ClipOverride.OverrideNames
                if (!ClipOverride.OverrideNames.Contains(key)) clipOverride.Remove(key);

                int index = controllerOverrides.FindIndex(o => o.Key.name == key);
                    
                //if clip isn't in animator remove it
                if (index == - 1) clipOverride.Remove(key);

                controllerOverrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(controllerOverrides[index].Key, pair.Value);
            }
                
            overrideController.ApplyOverrides(controllerOverrides);
        }

        public static void LocalReset(this Transform transform, Transform parent = null)
        {
            if (parent != null) transform.transform.SetParent(parent);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        public static void SetLocalPositionAndRotation(this Transform transform, Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        public static void DrawRectangle(Transform origin, Vector2 size, Color color = default)
        {
            DrawRectangle(origin.position, origin.right, origin.up, size, color);
        }
        
        public static void DrawRectangle(Vector3 position, Vector3 right, Vector3 up, Vector2 size, Color color = default)
        {
            float halfWidth = size.x / 2f;
            float halfHeight = size.y / 2f;

            Vector3 topLeft = position + (- right * halfWidth) + (up * halfHeight);
            Vector3 topRight = position + (right * halfWidth) + (up * halfHeight);
        
            Vector3 bottomLeft = position + (- right * halfWidth) + (- up * halfHeight);
            Vector3 bottomRight = position + (right * halfWidth) + (- up * halfHeight);

            Gizmos.color = color == default ? Color.white : color;
            
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomLeft, bottomRight);
        }
        
        public static int WrapIndex(this Array array, int index)
        {
            return index % array.Length;
        }
        
        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsType(this object obj, object compareObj)
        {
            return obj.GetType() == compareObj.GetType();
        }

        public static string[] GetAllSceneNamesInBuildSettings()
        {
            string[] sceneNames = new string[SceneManager.sceneCountInBuildSettings];
            
            for (int i = 0; i < sceneNames.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            }

            return sceneNames;
        }

        public static int MaxIndex(this Array array)
        {
            return array.Length - 1;
        }
        
        public static int MaxIndex<T>(this List<T> list)
        {
            return list.Count - 1;
        }

        public static float Positive(this float value)
        {
            if (value < 0)
            {
                return value *= - 1f;
            }

            return value;
        }
        
        #region Scene Load/Unload

        #region LoadingScene

        public delegate void LoadingScene(int sceneBuildIndex);

        public static event LoadingScene OnLoadingScene;

        private static void InvokeLoadingScene(int sceneBuildIndex)
        {
            OnLoadingScene?.Invoke(sceneBuildIndex);
        }

        #endregion
        
        #region SceneLoaded

        public delegate void SceneLoaded(int loadedSceneBuildIndex);

        public static event SceneLoaded OnSceneLoaded;

        private static void InvokeSceneLoaded(int loadedSceneBuildIndex)
        {
            OnSceneLoaded?.Invoke(loadedSceneBuildIndex);
        }

        #endregion

        /// <summary>
        /// load scene with utilities
        /// </summary>
        /// <param name="sceneBuildIndex">build index of scene in build settings</param>
        /// <param name="onSceneLoaded">called once scene has finished loading</param>
        /// <returns></returns>
        public static bool LoadScene(int sceneBuildIndex, Action onSceneLoaded = null)
        {
            //check if scene is at build index
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"no {nameof(Scene)} to load at {nameof(Scene.buildIndex)} {sceneBuildIndex}");
                
                return false;
            }
            
            Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
            
            if (scene.isLoaded)
            {
                Debug.LogError($"[{sceneBuildIndex}] {scene.name} {nameof(Scene)} already loaded");
                
                return false;
            }
            
            InvokeLoadingScene(sceneBuildIndex);
            
            SceneManager.LoadSceneAsync(sceneBuildIndex).completed += handle =>
            {
                if (!handle.isDone)
                {
                    Debug.LogError($"{nameof(LoadScene)} [{sceneBuildIndex}] {scene.name} {nameof(Scene)} unsuccessful");
                    
                    return;
                }

                //invoke scene loaded
                onSceneLoaded?.Invoke();
                
                InvokeSceneLoaded(sceneBuildIndex);
            };

            return true;
        }
        
        /// <summary>
        /// unload active scene with utilities
        /// </summary>
        /// <param name="onSceneUnloaded">called once scene has finished unloading</param>
        /// <returns></returns>
        public static bool UnloadActiveScene(Action onSceneUnloaded = null)
        {
            Scene activeScene = SceneManager.GetActiveScene();

            return UnloadScene(activeScene.buildIndex, onSceneUnloaded);
        }

        /// <summary>
        /// unload scene with utilities
        /// </summary>
        /// <param name="sceneBuildIndex">scene to unload build index in build settings</param>
        /// <param name="onSceneUnloaded">called once scene has finished unloading</param>
        /// <returns></returns>
        public static bool UnloadScene(int sceneBuildIndex, Action onSceneUnloaded = null)
        {
            //check if scene is at build index
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"no {nameof(Scene)} to unload at {nameof(Scene.buildIndex)} {sceneBuildIndex}");
                
                return false;
            }
            
            Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);

            //check if scene is loaded
            if (!scene.isLoaded)
            {
                Debug.LogError($"[{sceneBuildIndex}] {scene.name} {nameof(Scene)} already unloaded");
                
                return false;
            }
            
            SceneManager.UnloadSceneAsync(scene).completed += handle =>
            {
                if (!handle.isDone)
                {
                    Debug.LogError($"{nameof(UnloadScene)} [{sceneBuildIndex}] {scene.name} {nameof(Scene)} unsuccessful");
                    
                    return;
                }
                //invoke scene unloaded
                onSceneUnloaded?.Invoke();
            };

            return true;
        }

        #endregion

        #region Serializables

        public static SerializableVector3 ToSerializableVector3(this Vector3 value)
        {
            return new SerializableVector3(value.x, value.y, value.z);
        }

        #endregion
        
        /// <summary>
        /// returns t
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 ab = b - a;
            Vector3 av = value - a;
            
            return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
        }
        
        /// <summary>
        /// returns t
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float InverseLerp(Vector2 a, Vector2 b, Vector2 value)
        {
            Vector2 ab = b - a;
            Vector2 av = value - a;
            
            return Vector2.Dot(av, ab) / Vector2.Dot(ab, ab);
        }

        public static bool HasLayer(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }
        
        /// <summary>
        /// clamp any angle between - 180f and 180f
        /// </summary>
        /// <param name="angle">angle to clamp</param>
        /// <param name="min">minClamp limit</param>
        /// <param name="max">maxClamp limit</param>
        /// <returns></returns>
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle > 360f || angle < - 360f)
            {
                angle = Mathf.Sign(angle) * (Mathf.Abs(angle) % 360f);
            }
            
            if (angle < - 180F) angle += 360f;
            
            if (angle > 180F) angle -= 360F;
            
            return Mathf.Clamp(angle, min, max);
        }

        public static Vector3[] GetBoundVertices(this Bounds bounds)
        {
            Vector3 min = bounds.min;
            
            Vector3 max = bounds.max;
            
            return new []
            {
                min,
                max,
                new Vector3( min.x, min.y, max.z ),
                new Vector3( min.x, max.y, min.z ),
                new Vector3( max.x, min.y, min.z ),
                new Vector3( min.x, max.y, max.z ),
                new Vector3( max.x, min.y, max.z ),
                new Vector3( max.x, max.y, min.z )
            };
        }
    }
}
