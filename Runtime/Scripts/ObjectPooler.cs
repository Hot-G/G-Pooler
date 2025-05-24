using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTools.GPooler
{
    public class ObjectPoolItem<T> where T : Component
    {
        public string tag;
        public T objectToPool;
        [NonSerialized] public List<T> poolObjects;
        [Min(1)]
        public int amountToPool;
        public ExpandType expandType;
    }

    public enum ExpandType
    {
        None,
        Expand,
        UseLast
    }

    public class ObjectPooler : MonoBehaviour
    {
        private static ObjectPooler _instance;
        private static readonly Dictionary<string, ObjectPoolItem<Component>> poolList = new();

        private void Awake()
        {
            _instance = this;
            ResetPoolObjects();
        }
        
        /// <summary>
        ///   <para>Reset all pool objects.</para>
        /// </summary>
        /// <param name="destroyOldObjects">First, destroy spawned objects and respawn pool objects.</param>
        public static void ResetPoolObjects(bool destroyOldObjects = false)
        {
            if (destroyOldObjects)
            {
                foreach (var poolKeys in poolList.Keys)
                {
                    foreach (var poolObject in poolList[poolKeys].poolObjects)
                    {
                        if (poolObject == null) continue;
                        Destroy(poolObject.gameObject);
                    }
                }
            }
            
            poolList.Clear();

            var poolObjects = Resources.LoadAll<PoolObject>("/");
            
            foreach (var poolObject in poolObjects)
            {
                CreatePoolObject(new ObjectPoolItem<Component>
                {
                    tag = poolObject.poolTag,
                    objectToPool = poolObject.objectToPool,
                    amountToPool = poolObject.amountToPool,
                    expandType = poolObject.expandType
                }, poolObject.objectToPool.GetType());
            }
        }
        
        /// <summary>
        ///   <para>Create new pool object</para>
        /// </summary>
        /// <param name="newPoolItem">Object properties you want to add.</param>
        /// <param name="type">Added object type</param>
        public static void CreatePoolObject<T>(ObjectPoolItem<T> newPoolItem, Type type) where T : Component
        {
            if (!poolList.ContainsKey(newPoolItem.tag))
            {
                poolList.Add(newPoolItem.tag, newPoolItem as ObjectPoolItem<Component>);
                poolList[newPoolItem.tag].poolObjects = new List<Component>(newPoolItem.amountToPool);   
            }
        
            for (var i = 0; i < newPoolItem.amountToPool; i++)
            {
                SpawnPoolObject(newPoolItem, type).gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///   <para>Return game object in the pool.</para>
        /// </summary>
        /// <param name="tag">The tag enum of the wanted game object.</param>
        /// <param name="spawnPosition">Set Position at Spawned Object.</param>
        /// <param name="eulerAngles">Set Euler Angles at Spawned Object.</param>
        /// <param name="attachTransform">Set Parent at Spawned Object.</param>
        /// <returns>
        ///   <para>Game object as ready in pool.</para>
        /// </returns>
        public static T GetPoolObject<T>(string tag, Vector3 spawnPosition, Vector3 eulerAngles, Transform attachTransform) where T : Component
        {
            var returnedObject = GetPoolObject<T>(tag, spawnPosition, eulerAngles);
            if (returnedObject) returnedObject.transform.SetParent(attachTransform);
            return returnedObject;
        }
        
        /// <summary>
        ///   <para>Return game object in the pool.</para>
        /// </summary>
        /// <param name="tag">The tag enum of the wanted game object.</param>
        /// <param name="attachTransform">Set Parent at Spawned Object.</param>
        /// <returns>
        ///   <para>Game object as ready in pool.</para>
        /// </returns>
        public static T GetPoolObject<T>(string tag, Transform attachTransform) where T : Component
        {
            return GetPoolObject<T>(tag, default, default, attachTransform);
        }


        /// <summary>
        ///   <para>Return game object in the pool.</para>
        /// </summary>
        /// <param name="tag">The tag of the wanted game object.</param>
        /// <param name="spawnPosition">Set Position at Spawned Object.</param>
        /// <param name="eulerAngles">Set Euler Angles at Spawned Object.</param>
        /// <returns>
        ///   <para>Game object as ready in pool.</para>
        /// </returns>
        public static T GetPoolObject<T>(string tag, Vector3 spawnPosition = default, Vector3 eulerAngles = default) where T : Component
        {
            var currentPoolList = poolList[tag].poolObjects;
            T returnedObject = null;

            for (int i = 0, n = currentPoolList.Count; i < n; i++)
            {
                var currentPoolObject = currentPoolList[i];

                if (!currentPoolObject.gameObject.activeSelf)
                {
                    returnedObject = (T)currentPoolObject;
                    break;
                }
            }
            
            if (returnedObject == null)
            {
                switch (poolList[tag].expandType)
                {
                    case ExpandType.None:
                        return null;
                    case ExpandType.Expand:
                        returnedObject = (T)SpawnPoolObject(poolList[tag], poolList[tag].objectToPool.GetType());
                        break;
                    case ExpandType.UseLast:
                        returnedObject = (T)currentPoolList[0];
                        if (returnedObject == null) return null;
                        currentPoolList.RemoveAt(0);
                        currentPoolList.Add(returnedObject);
                        break;
                }
            }

            returnedObject.gameObject.SetActive(true);
            returnedObject.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(eulerAngles));

            return returnedObject;
        }

        /// <summary>
        ///   <para>Add game object in the pool.</para>
        /// </summary>
        /// <param name="addedObject">The game object you want to add in pool.</param>

        public static void AddPool(GameObject addedObject)
        {
            addedObject.SetActive(false);
        }
    
        /// <summary>
        ///   <para>Add game object in the pool with delay.</para>
        /// </summary>
        /// <param name="selectedGameObject">The game object you want to add in pool.</param>
        /// <param name="delay">The game object you want to add in pool after a delay.</param>
    
        public static Coroutine AddPoolWithDelay(GameObject selectedGameObject, float delay = 3f)
        {
            return _instance.StartCoroutine(DisableWithDelay(selectedGameObject, delay));
        }

        public static void StopAddPool(Coroutine coroutine)
        {
            if (coroutine != null) _instance.StopCoroutine(coroutine);
        }
        
        private static T SpawnPoolObject<T>(ObjectPoolItem<T> newPoolItem, Type type) where T : Component
        {
            var spawnedObject = Instantiate(newPoolItem.objectToPool.gameObject);
            spawnedObject.name = newPoolItem.tag;
            
            var spawnedComponent = spawnedObject.GetComponent(type);
            poolList[newPoolItem.tag].poolObjects.Add(spawnedComponent);
            
            Destroy(spawnedObject.GetComponent<PoolObject>());
            
            return (T)spawnedComponent;
        }
    
        private static IEnumerator DisableWithDelay(GameObject selectedGameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (selectedGameObject != null)
            {
                AddPool(selectedGameObject);
            }
        }
    }
}