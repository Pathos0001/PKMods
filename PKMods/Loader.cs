using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace PKMods
{

    public class Loader
    {
        static void Load()
        {
            //var harmony = HarmonyInstance.Create("com.github.pathos0001.mods.pk");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            Debug.Log("Loading PKMods");
            if (GameObject.Find("ModHolder"))
            {
                Debug.Log("Destroying previous holder");
                GameObject.Destroy(GameObject.Find("ModHolder"));
            }
            var modholder = new GameObject("ModHolder");
            modholder.AddComponent<ModMenu>();
            modholder.AddComponent<TexturePackMod>();

            GameObject.DontDestroyOnLoad(modholder);

        }
    }
}
