using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace PKMods
{
    public class Loader
    {
        public static void Load()
        {
            //Debug.Log("Loading Harmony");
            //var harmony = HarmonyInstance.Create("com.github.pathos0001.mods.pk");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            Debug.Log("Loading PKMods");
            Debug.Log("Application.dataPath: " + Application.dataPath);

            //create these folders if they dont allready exist
            Directory.CreateDirectory(Application.dataPath + "\\Mods");
            Directory.CreateDirectory(Application.dataPath + "\\Mods\\Dependencies");

            if (GameObject.Find("ModHolder"))
            {
                Debug.Log("Destroying previous holder");
                GameObject.Destroy(GameObject.Find("ModHolder"));
            }
            var modholder = new GameObject("ModHolder");

            modholder.AddComponent<TexturePackMod>();
            modholder.AddComponent<ModMenu>();
            modholder.AddComponent<BrowsePanel>();


            GameObject.DontDestroyOnLoad(modholder);

            //check dependencies
            /*
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Console.WriteLine(assembly.GetName());
            }
            */

        }

    }
}
