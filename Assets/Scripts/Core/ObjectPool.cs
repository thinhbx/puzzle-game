using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace  PuzzleGame.Core
{
     public class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;
        public int initialSize;

        private readonly Stack<GameObject> instances = new Stack<GameObject>();

        private void Awake()
        {
            Assert.IsNotNull(prefab);
        }

        private void Start()
        {
            for (var i = 0; i < initialSize; i++)
            {
                var obj = CreateInstance();
                obj.SetActive(false);
                instances.Push(obj);
            }
        }

        /// <summary>
        /// Returns a new object from the pool.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public GameObject GetObject()
        {
            var obj = instances.Count > 0 ? instances.Pop() : CreateInstance();
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Returns the specified game object to the pool where it came from.
        /// </summary>
        /// <param name="obj">The object to return to its origin pool.</param>
        public void ReturnObject(GameObject obj)
        {
            var pooledObject = obj.GetComponent<PooledObject>();
            Assert.IsNotNull(pooledObject);
            Assert.IsTrue(pooledObject.pool == this);

            obj.SetActive(false);
            instances.Push(obj);
        }

        /// <summary>
        /// Resets the object pool to its initial state.
        /// </summary>
        public void Reset()
        {
            var objectsToReturn = new List<GameObject>();
            foreach (var instance in transform.GetComponentsInChildren<PooledObject>())
            {
                if (instance.gameObject.activeSelf)
                {
                    objectsToReturn.Add(instance.gameObject);
                }
            }
            foreach (var instance in objectsToReturn)
            {
                ReturnObject(instance);
            }
        }

        /// <summary>
        /// Creates a new instance of the pooled object type.
        /// </summary>
        /// <returns>A new instance of the pooled object type.</returns>
        private GameObject CreateInstance()
        {
            var obj = Instantiate(prefab);
            var pooledObject = obj.AddComponent<PooledObject>();
            pooledObject.pool = this;
            obj.transform.SetParent(transform);
            return obj;
        }
    }

    /// <summary>
    /// Utility class to identify the pool of a pooled object.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public ObjectPool pool;
    }
}
