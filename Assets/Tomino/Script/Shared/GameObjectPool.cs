using UnityEngine;
using System.Collections.Generic;

namespace Tomino.Shared
{
    public class GameObjectPool<T> where T : MonoBehaviour
    {
        private readonly List<T> _pool = new List<T>();
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private int _nextAvailableIndex = 0;

        public GameObjectPool(GameObject prefab, int initialSize, GameObject parent)
        {
            _prefab = prefab;
            _parent = parent.transform;
            for (var i = 0; i < initialSize; ++i) CreateNewItem();
        }

        private T CreateNewItem()
        {
            var obj = Object.Instantiate(_prefab, _parent, true);
            var component = obj.GetComponent<T>();
            obj.SetActive(false);
            _pool.Add(component);
            return component;
        }

        public T GetAndActivate()
        {
            T item = (_nextAvailableIndex >= _pool.Count) ? CreateNewItem() : _pool[_nextAvailableIndex];
            item.gameObject.SetActive(true);
            _nextAvailableIndex++;
            return item;
        }

        public void DeactivateAll()
        {
            for (int i = 0; i < _pool.Count; i++) if (_pool[i] != null) _pool[i].gameObject.SetActive(false);
            _nextAvailableIndex = 0; 
        }
    }
}