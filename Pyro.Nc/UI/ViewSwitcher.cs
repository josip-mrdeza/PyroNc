using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pyro.Nc.UI
{
    public static class ViewSwitcher
    {
        public static Dictionary<string, GameObject> Views;
        public static List<GameObject> Hierarchy = new List<GameObject>();

        static ViewSwitcher()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scene.GetRootGameObjects(Hierarchy);
            Views = Hierarchy.ToDictionary(ConvertToKey, ConvertToValue);
        }

        private static string ConvertToKey(GameObject go) => go.name;
        private static GameObject ConvertToValue(GameObject go) => go;

        public static void HideALl()
        {
            foreach (var value in Views.Values)
            {
                value.SetActive(false);
            }
        }

        public static void ShowAllExceptSelf(MonoBehaviour self)
        {
            foreach (var value in Hierarchy)
            {
                if (self.name != value.name)
                {
                    value.SetActive(true);
                }
            }
            self.gameObject.SetActive(false);
        }

        public static void Activate(string objName)
        {
            Views[objName].SetActive(true);
        }
        
        public static void Activate(MonoBehaviour go)
        {
            Views[go.name].SetActive(true);
        }
    }
}