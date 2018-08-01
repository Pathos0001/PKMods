using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

namespace PKMods
{
    public class BrowsePanel : MonoBehaviour
    {
        public List<string> installedTexturePacks;
        List<GameObject> resultItems;

        GameObject ScrollViewPanel;
        GameObject ResultsPanel;
        RawImage ResultRawImage;
        Text ResultNameText;
        Text ResultAuthorText;
        Text ResultDescriptionText;
        Button ResultDownloadButton;

        Button SearchButton;
        Button PreviousButton;
        Button NextButton;
        Dropdown SearchTypeDropdown;
        Dropdown ForDropdown;

        public string[] SearchTypes = new string[]
        {
            "Textures",
            "Scenery",
            "Maps",
            "Animals",
            "Misc"
        };
        public static string[] AvailableGenus =
        {
            "Gallimimus",
            "Tyrannosaurus",
            "Velociraptor",
            "Triceratops",
            "Camarasaurus",
            "Allosaurus",
            "Stegosaurus",
            "Dryosaurus"
        };

        // Use this for initialization
        void Start()
        {
            installedTexturePacks = new List<string>();

            ScrollViewPanel = GameObject.Find("ResultContent");
            ResultsPanel = GameObject.Find("ResultPanel");
            ResultRawImage = GameObject.Find("ResultRawImage").GetComponentInChildren<RawImage>();
            ResultNameText = GameObject.Find("ResultNameText").GetComponentInChildren<Text>();
            ResultAuthorText = GameObject.Find("ResultAuthorText").GetComponentInChildren<Text>();
            ResultDescriptionText = GameObject.Find("ResultDescriptionText").GetComponentInChildren<Text>();
            ResultDownloadButton = GameObject.Find("ResultDownloadButton").GetComponentInChildren<Button>();

            SearchButton = GameObject.Find("SearchButton").GetComponentInChildren<Button>();
            NextButton = GameObject.Find("NextButton").GetComponentInChildren<Button>();
            PreviousButton = GameObject.Find("PreviousButton").GetComponentInChildren<Button>();
            SearchTypeDropdown = GameObject.Find("SearchTypeDropdown").GetComponentInChildren<Dropdown>();
            ForDropdown = GameObject.Find("ForDropdown").GetComponentInChildren<Dropdown>();

            ResultsPanel.SetActive(false);
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://pkmods-dc5d2.firebaseio.com/");
            // Get the root reference location of the database.
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

            //SearchTypeDropdown.options.Clear();
            SearchTypeDropdown.options = new List<Dropdown.OptionData>();
            foreach (string type in SearchTypes)
            {
                SearchTypeDropdown.options.Add(new Dropdown.OptionData(type));
            }

            SearchTypeDropdown.onValueChanged.AddListener((newValue) =>
            {
                var newType = SearchTypes[newValue];
                if (newType == "Textures")
                {
                    ForDropdown.interactable = true;
                    ForDropdown.options.Clear();
                    ForDropdown.options.Add(new Dropdown.OptionData("Any"));
                    foreach (string genus in AvailableGenus)
                    {
                        ForDropdown.options.Add(new Dropdown.OptionData(genus));
                    }
                    //ForDropdown.value = 0;
                    StartCoroutine(SetForDropdown());
                }
                else
                {
                    ForDropdown.interactable = false;
                    ForDropdown.options.Clear();
                }
            });

            //SearchTypeDropdown.value = 0;
            StartCoroutine(SetSearchDropdown());


            SearchButton.onClick.AddListener(() =>
            {
                var search = SearchTypes[ForDropdown.value];
                var orderBy = "id";
                var limitToLast = 10;

                Debug.Log("Searching for " + search + ". OrderByChild: " + orderBy + ". limitToLast: " + limitToLast);
                FirebaseDatabase.DefaultInstance
                 .GetReference(search).OrderByChild(orderBy).LimitToLast(limitToLast)
                 .ValueChanged += HandleValueChanged;

            });
        }

        private IEnumerator SetForDropdown()
        {
            yield return new WaitForEndOfFrame();
            ForDropdown.value = 0;
        }

        private IEnumerator SetSearchDropdown()
        {
            yield return new WaitForEndOfFrame();
            SearchTypeDropdown.value = 0;
        }

        void HandleValueChanged(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("HandleValueChanged");
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            Debug.Log(args.Snapshot.ToString());//Textures

            var rawjson = args.Snapshot.GetRawJsonValue();
            var parsedOBjects = JsonConvert.DeserializeObject<TextureItem[]>(rawjson);
            UpdateResults(parsedOBjects);
            //var parsed2 = args.Snapshot.Child("Textures");
            // Do something with the data in args.Snapshot
        }

        private void UpdateResults(TextureItem[] results)
        {
            var loadedTexturePacks = this.gameObject.GetComponent<TexturePackMod>().loadedTexturePackNames;
            if (resultItems != null)
            {
                foreach (var item in resultItems)
                {
                    if (item != null)
                        Destroy(item);
                }
            }
            resultItems = new List<GameObject>();

            for (int i = 0; i < results.Length; i++)
            {
                Debug.Log(results[i]);

                var newPanel = Instantiate(ResultsPanel, ResultsPanel.transform.parent);
                newPanel.SetActive(true);

                var tResultRawImage = newPanel.transform.FindDeepChild("ResultRawImage").GetComponentInChildren<RawImage>();
                var tResultNameText = newPanel.transform.FindDeepChild("ResultNameText").GetComponentInChildren<Text>();
                var tResultAuthorText = newPanel.transform.FindDeepChild("ResultAuthorText").GetComponentInChildren<Text>();
                var tResultDescriptionText = newPanel.transform.FindDeepChild("ResultDescriptionText").GetComponentInChildren<Text>();
                var tResultDownloadButton = newPanel.transform.FindDeepChild("ResultDownloadButton").GetComponentInChildren<Button>();

                tResultNameText.text = results[i].name;
                tResultAuthorText.text = results[i].author;
                tResultDescriptionText.text = results[i].description;

                ScrollViewPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollViewPanel.GetComponent<RectTransform>().sizeDelta.x, ScrollViewPanel.GetComponent<RectTransform>().sizeDelta.y + (100f * i));
                newPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPanel.GetComponent<RectTransform>().anchoredPosition.x, newPanel.GetComponent<RectTransform>().anchoredPosition.y - (100f * i));
                StartCoroutine(DownloadTextureToImage(tResultRawImage, results[i].imageurl));

                string downloadUrl = results[i].downloadurl;
                string downloadName = results[i].name;
                string searchType = SearchTypes[SearchTypeDropdown.value];
                if (loadedTexturePacks.Contains(downloadName))
                {
                    tResultDownloadButton.interactable = false;
                    tResultDownloadButton.GetComponentInChildren<Text>().text = "Installed";
                }
                tResultDownloadButton.onClick.AddListener(() =>
                {
                    Debug.Log("Downloading " + downloadName + ". from: " + downloadUrl);
                    StartCoroutine(DownloadObject(downloadUrl, searchType, downloadName));
                });
            }
        }

        IEnumerator DownloadTextureToImage(RawImage image, string URL)
        {
            Texture2D tex;
            tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            using (WWW www = new WWW(URL))
            {
                yield return www;
                try
                {
                    www.LoadImageIntoTexture(tex);
                    image.texture = tex;
                }
                catch (Exception e)
                {
                    Debug.Log("Couldent download image: " + e.ToString());
                }
            }
        }

        IEnumerator DownloadObject(string url, string searchType, string name)
        {
            Debug.Log(Path.GetFileNameWithoutExtension(url));
            UnityWebRequest www = UnityWebRequest.Get(url);
            bool abort = false;
            www.Send();
            while (www.downloadProgress < 1f && !abort)
            {
                Debug.Log("Downloading: " + www.downloadProgress.ToString());
                yield return new WaitForSeconds(0.5f);
            }
            if (www.isError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var data = www.downloadHandler.data;
                Debug.Log("Downloaded " + data.Length + " bytes");
                Debug.Log("searchType:" + searchType);

                if (searchType == "Textures")
                {
                    var dir = Application.dataPath + "\\Mods";
                    Directory.CreateDirectory(dir); //if it doesnt exsit
                    dir += "\\Texturepacks";
                    Directory.CreateDirectory(dir); //if it doesnt exsit
                    dir = dir + "\\" + name + ".pktex";
                    Debug.Log(dir);
                    File.WriteAllBytes(dir, data);
                }
            }
        }

    }
    [Serializable]
    public class TextureItem
    {
        public string id;
        public string name;
        public string author;
        public string description;
        public string target;
        public string downloadurl;
        public string imageurl;

        public override string ToString()
        {
            return "id: " + id + " name: " + name + " author: " + author + " target: " + target;
        }
    }

}

