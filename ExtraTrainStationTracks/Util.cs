using UnityEngine;
using Object = UnityEngine.Object;

namespace ElevatedTrainStationTrack
{
    public static class Util
    {
        public static NetInfo ClonePrefab(NetInfo originalPrefab, string newName, Transform parentTransform)
        {
            var instance = Object.Instantiate(originalPrefab.gameObject);
            instance.name = newName;
            instance.transform.SetParent(parentTransform);
            instance.transform.localPosition = new Vector3(-7500, -7500, -7500);
            var newPrefab = instance.GetComponent<NetInfo>();
            instance.SetActive(false);
            newPrefab.m_prefabInitialized = false;
            return newPrefab;
        }
    }
}