using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PKMods
{
    class ModMenu : MonoBehaviour
    {
        public static string MOD_BASE_DIRECTORY = Application.dataPath + "\\Mods\\";
        private GameObject modMenu;
        private GameObject skinsPanel;
        private Dropdown TexturePackListDropdown;

        private GameObject MorphPanel;

        private Text ExportTextureResultText;
        private RawImage texturePreview;
        private Dropdown ExportSkinTextureSelectDropdown;

        void Start()
        {
            Application.runInBackground = true;
            Debug.Log("Loading modmenu...");
            LoadModMenuAssetbundle();
            SetupMenu();
            SetupTextureMods();


            if (!Directory.Exists(MOD_BASE_DIRECTORY))//checking if it exists is probably redundant, but it looks good aesthetically
            {
                Directory.CreateDirectory(MOD_BASE_DIRECTORY);
            }
            if (!Directory.Exists(TexturePackMod.TEXTUREPACK_BASE_DIRECTORY))
            {
                Directory.CreateDirectory(TexturePackMod.TEXTUREPACK_BASE_DIRECTORY);
            }

            modMenu.SetActive(true); //show after injection

            Debug.Log("Menu loaded!");
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                modMenu.SetActive(!modMenu.activeSelf);
                if (modMenu.active)
                {
                    //SetupTextureMods();
                    var texturePackMod = this.GetComponent<TexturePackMod>();
                    texturePackMod.ScanTexturePacks();
                }
            }
        }

        public void LoadModMenuAssetbundle()
        {
            var modMenuAssetbundleDirectory = MOD_BASE_DIRECTORY + "modmenu.ab";
            AssetBundle assetBundle = AssetBundle.LoadFromFile(modMenuAssetbundleDirectory);
            if (assetBundle == null)
            {
                Debug.LogError("Could not load modmenu.ab Ensure it is in the mods directory!");
                return;
            }
            foreach (var asset in assetBundle.GetAllAssetNames())
            {
                Debug.Log("Asset: " + asset);
            }

            var prefab = assetBundle.LoadAsset<GameObject>("ModMenuCanvas");
            modMenu = Instantiate(prefab, this.transform);

            assetBundle.Unload(false);
        }

        private void SetupMenu()
        {
            skinsPanel = modMenu.transform.FindDeepChild("SkinsPanel").gameObject;
            MorphPanel = modMenu.transform.FindDeepChild("MorphPanel").gameObject;

            var skinsButton = modMenu.transform.FindDeepChild("SkinsButton");
            skinsButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                skinsPanel.SetActive(true);
                MorphPanel.SetActive(false);
            });
            var morphButton = modMenu.transform.FindDeepChild("MorphButton");
            morphButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                MorphPanel.SetActive(true);
                skinsPanel.SetActive(false);
            });
            var CloseButton = modMenu.transform.FindDeepChild("CloseButton");
            CloseButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                modMenu.SetActive(false);
            });





            modMenu.SetActive(false);
        }

        private void SetupTextureMods()
        {
            Debug.Log("SetupTextureMods");

            ExportTextureResultText = modMenu.transform.FindDeepChild("ExportTextureResultText").GetComponentInChildren<Text>();
            texturePreview = modMenu.transform.FindDeepChild("TexturePreview").GetComponentInChildren<RawImage>();

            var ExportSkinAnimalSelectDropdown = skinsPanel.transform.FindDeepChild("ExportSkinAnimalSelectDropdown").GetComponentInChildren<Dropdown>();
            ExportSkinTextureSelectDropdown = skinsPanel.transform.FindDeepChild("ExportSkinTextureSelectDropdown").GetComponentInChildren<Dropdown>();
            ExportSkinAnimalSelectDropdown.options.Clear();

            if (FindObjectOfType<ObjectHolderSelection>() == null || FindObjectOfType<ObjectHolderSelection>().allAnimals == null)
            {
                Debug.Log("Could not load allAnimals. Try starting a game first!");
                return;
            }
            var animalListOptions = new List<Dropdown.OptionData>();
            var allAnimals = FindObjectOfType<ObjectHolderSelection>().allAnimals;
            
            foreach (var animal in allAnimals)
            {
                animalListOptions.Add(new Dropdown.OptionData(animal.GetComponentInChildren<AnimalPreview>().animalName));
            }
            ExportSkinAnimalSelectDropdown.options = animalListOptions;
            ExportSkinAnimalSelectDropdown.onValueChanged.RemoveAllListeners();
            ExportSkinAnimalSelectDropdown.onValueChanged.AddListener((newValue) =>
            {
                var textureListOptions = new List<Dropdown.OptionData>();
                var textureList = FindObjectOfType<ObjectHolderSelection>().allAnimals[newValue].GetComponentInChildren<AnimalPreview>().allSkins;
                foreach( var texture in textureList)
                {
                    textureListOptions.Add(new Dropdown.OptionData(texture.name));
                }
                ExportSkinTextureSelectDropdown.options = textureListOptions;
            });

            ExportSkinTextureSelectDropdown.onValueChanged.RemoveAllListeners();
            ExportSkinTextureSelectDropdown.onValueChanged.AddListener((newValue) =>
            {
                try
                {
                    texturePreview.texture = FindObjectOfType<ObjectHolderSelection>().allAnimals[newValue].GetComponentInChildren<AnimalPreview>().allSkins[newValue];
                }
                catch(Exception e)
                {
                    Debug.LogError("Could not load texture. " + e.ToString());
                    ExportTextureResultText.text = "Could not load texture.";
                }
            });

            var ExportButton = skinsPanel.transform.FindDeepChild("ExportButton").GetComponentInChildren<Button>();
            ExportButton.onClick.RemoveAllListeners();
            ExportButton.onClick.AddListener(() =>
            {
                var selectedAnimalIndex = ExportSkinAnimalSelectDropdown.value;
                var selectedTextureIndex = ExportSkinTextureSelectDropdown.value;
                if (selectedAnimalIndex > -1 && selectedTextureIndex > -1)
                {
                    var textureToExport = texturePreview.texture;
                    try
                    {
                        var filename = MOD_BASE_DIRECTORY + textureToExport.name + ".png";
                        TexturePackMod.SaveTextureToFile(textureToExport as Texture2D, filename);
                        ExportTextureResultText.text = "Texture saved to " + filename;
                    }
                    catch
                    {
                        Debug.LogError("Could not save texture!");
                        ExportTextureResultText.text = "Could not save texture! ";
                    }
                }
                else
                {
                    ExportTextureResultText.text = "Invalid selection";
                }
            });
            TexturePackListDropdown = skinsPanel.transform.FindDeepChild("TexturePacksDropdown").GetComponentInChildren<Dropdown>();

            var texturePackMod = this.GetComponent<TexturePackMod>();
            texturePackMod.OnTexturepacksAdded = (result) =>
            {
                var newOptions = new List<Dropdown.OptionData>();
                for (int i = 0; i < texturePackMod.loadedTexturePackNames.Count; i++)
                {
                    newOptions.Add(new Dropdown.OptionData(texturePackMod.loadedTexturePackNames[i]));
                }
                TexturePackListDropdown.options = newOptions;
            };
            texturePackMod.ScanTexturePacks();

        }


    }

    

    public static class TransformDeepChildExtension
    {
        //Breadth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }


        /*
        //Depth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            foreach(Transform child in aParent)
            {
                if(child.name == aName )
                    return child;
                var result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
        */
    }
}
