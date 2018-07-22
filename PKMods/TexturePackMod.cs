using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace PKMods
{
    public class TexturePackMod : MonoBehaviour
    {
        public Dictionary<string, List<Texture>> addedTextures;
        Dictionary<string, Texture2D> loadedTextures;
        public List<string> loadedTexturePackNames;
        public static string PREFIX_PKTEX = "!!PKTEX";
        public static int VERSION = 1;
        public static string ENCODING_ASCII = "ASC";
        public static string ENCODING_BINARY = "BIN";
        public static string ENCODING_BINARY_COMPRESSED = "CMP";

        public static string TEXTURE_NAME_DELIMITER = "=";

        public Action<bool> OnTexturepacksAdded;

        public void Start()
        {
            //check dependencies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            bool found = false;
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.ToUpper().Contains(("SevenZipSharp").ToUpper()))
                {
                    found = true;
                }
            }
            if (!found)
            {
                Debug.LogError("PKMODS ERROR:  File 7za.dll is missing from the Managed subfolder! In order to load compressed files, please copy it over and restart the game.");
            }


            string[] files = Directory.GetFiles(Application.dataPath + "\\Mods\\Dependencies", "7za.dll");
            if (files.Length == 0)
            {
                Debug.LogError("PKMODS ERROR: File 7za.dll is missing from the Mods/Dependencies subfolder! In order to load compressed files, please copy it over and restart the game.");
            }

            //manually check for SevenZipSharp assembly?




            addedTextures = new Dictionary<string, List<Texture>>();
            loadedTextures = new Dictionary<string, Texture2D>();

            if (!Directory.Exists(TEXTUREPACK_BASE_DIRECTORY))
            {
                Directory.CreateDirectory(TEXTUREPACK_BASE_DIRECTORY);
            }

            /*
            List<GameObject> allAnimals = UnityEngine.Object.FindObjectOfType<ObjectHolderSelection>().allAnimals;
            foreach(var preview in allAnimals)
            {
                Debug.Log(preview.name);
                Debug.Log(preview.name.Trim().ToLower());
            }
            */
        }

        public static string TEXTUREPACK_BASE_DIRECTORY = Application.dataPath + "\\Mods\\Texturepacks";


        public void ScanTexturePacks()
        {
            if (UnityEngine.Object.FindObjectOfType<ObjectHolderSelection>() == null)
                return;

            loadedTexturePackNames = new List<string>();
            RemoveAllAddedTextures();
            Debug.Log("Scanning for new texturepacks...");
            string[] files = Directory.GetFiles(TEXTUREPACK_BASE_DIRECTORY + "\\", "*.PKTEX");
            foreach( string filename in files)
            {
                Debug.Log("Adding " + Path.GetFileNameWithoutExtension(filename));
                var pack = ReadTexturePack(filename);
                AddTexturePack(pack);
                loadedTexturePackNames.Add(Path.GetFileNameWithoutExtension(filename));
            }
            if (OnTexturepacksAdded != null)
                OnTexturepacksAdded(true);
        }

        private void AddTexturePack(List<IncludedTexture> texturePack)
        {
            List<GameObject> allAnimals = UnityEngine.Object.FindObjectOfType<ObjectHolderSelection>().allAnimals;
            if (allAnimals == null)
                Debug.LogError("allAnimals not found!!");

            //this finds each unique name per genus. In case the user has a skin for two or more different types of dinosaurs and wants to use the same name
            List<string> uniqueSkinNames = new List<string>();
            foreach (var skin in texturePack)//we can avoid using linq here
            {
                string uniqueName = skin.Genus + "\n" + skin.Name + "\n" + skin.Integument;
                if (!uniqueSkinNames.Contains(uniqueName))
                {
                    Debug.Log(uniqueName);
                    uniqueSkinNames.Add(uniqueName);
                }
            }

            //var uniqueNames = texturePack.Select(e => e.Name).Distinct();

            foreach (var skinName in uniqueSkinNames)
            {
                string genus = skinName.Split('\n')[0];
                string name = skinName.Split('\n')[1];
                string integument = skinName.Split('\n')[2];

                AnimalPreview component = allAnimals.First((GameObject e) => e.name.Trim().ToLower() == genus.ToLower()).GetComponent<AnimalPreview>();
                if (!component)
                    Debug.Log("AnimalPreview component is null: " + genus);

                for (int i = 0; i < IncludedTexture.AvailableType.Length; i++)
                {
                    string typeName = IncludedTexture.AvailableType[i];
                    //Debug.Log("typeName: " + typeName);
                    var item = texturePack.FirstOrDefault(e => e.Name == name && e.Genus.ToLower() == genus.ToLower() && e.Type.ToLower() == typeName.ToLower() && e.Integument.ToLower() == integument.ToLower());
                    if (item != null)
                    {
                        AddSingleTexture(item, component);
                        if (item.Integument.ToLower() == "feathered")
                        {
                            if (!component.featheredSkinNames.Contains(item.Name))
                            {
                                component.featheredSkinNames.Add(item.Name);
                            }
                        }
                        else
                        {
                            if (!component.scalySkinNames.Contains(item.Name))
                            {
                                component.scalySkinNames.Add(item.Name);
                            }
                        }
                        //Debug.Log("Adding: " + item.Name);
                    }
                    else
                    {
                        Texture defaultTexture;
                        if (integument.ToLower() == "feathered")
                        {
                            defaultTexture = GetDefaultTextureFromPreviewByType(component, typeName, true);
                        }
                        else
                        {
                            defaultTexture = GetDefaultTextureFromPreviewByType(component, typeName, false);
                        }
                        AddSingleTexture(name, genus, integument, typeName, defaultTexture as Texture2D, component);
                    }
                }
            }
        }

        private List<IncludedTexture> ReadTexturePack(string openFileName)
        {
            List<IncludedTexture> includedTextures = new List<IncludedTexture>();

            using (var fileStream = File.OpenRead(openFileName))
            {
                Debug.Log("Loading " + Path.GetFileNameWithoutExtension(openFileName));
                byte[] encodingData = new byte[12];
                fileStream.Read(encodingData, 0, encodingData.Length);
                string prefix = Encoding.UTF8.GetString(encodingData, 0, PREFIX_PKTEX.Length);
                int versionNumber = int.Parse(Encoding.UTF8.GetString(encodingData, PREFIX_PKTEX.Length, 2));
                string encodingType = Encoding.UTF8.GetString(encodingData, PREFIX_PKTEX.Length + 2, 3);
                Console.WriteLine("Prefix: " + prefix + ". Version: " + versionNumber + ". Encoding: " + encodingType);


                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

                if (prefix != PREFIX_PKTEX)
                {
                    Debug.Log("This is not a valid PKTEX file!");
                    return new List<IncludedTexture>();
                }
                if (encodingType == ENCODING_ASCII)
                {
                    byte[] checkForAscii = new byte[5];
                    fileStream.Read(checkForAscii, 0, 5);
                    var firstCharacters = System.Text.Encoding.Default.GetString(checkForAscii);
                    fileStream.Position -= 5;

                    if (firstCharacters == "<?xml")
                    {
                        var serializer = new XmlSerializer(typeof(List<IncludedTexture>));
                        includedTextures = (List<IncludedTexture>)serializer.Deserialize(fileStream);
                        Console.WriteLine("Deserializing Ascii xml");
                    }
                    else
                    {
                        //not xml
                    }
                }
                else if (encodingType == ENCODING_BINARY)
                {
                    try
                    {
                        Console.WriteLine("Deserializing Binary");
                        BinaryFormatter formatter = new BinaryFormatter();
                        //formatter.Binder = new typeconvertor();
                        includedTextures = (List<IncludedTexture>)formatter.Deserialize(fileStream);
                    }
                    catch (SerializationException e2)
                    {
                        Console.WriteLine("Failed to deserialize: " + e2.Message);
                    }
                }
                else if (encodingType == ENCODING_BINARY_COMPRESSED)
                {
                    s.Reset();
                    s.Start();

                    string path = Application.dataPath;
                    path += "\\Mods\\Dependencies" + "\\7za.dll";
                    Debug.Log(path);
                    SevenZipCompressor.SetLibraryPath(path);

                    byte[] uncompressedSizeBytes = new byte[8];
                    fileStream.Read(uncompressedSizeBytes, 0, 8);
                    var uncompressedSize = BitConverter.ToUInt64(uncompressedSizeBytes, 0);
                    Debug.Log("Uncomoressed size is " + uncompressedSize.ToString());


                    MemoryStream m2 = new MemoryStream();
                    CopyTo(fileStream, m2);


                    //todo: close filestream to save memory?
                    m2.Position = 0;
                    Debug.Log("m2.Length:" + m2.Length);

                    MemoryStream mem = new MemoryStream();
                    //using (var extractor = new SevenZipExtractor(m2))
                    {
                        Debug.Log("Extracting 7z stream");
                        try
                        {
                            //Debug.Log("FilesCount:" + extractor.FilesCount);
                            //Debug.Log("Format:" + extractor.Format);

                            var compressed = ReadFully(m2);

                            var decompressedBuffer = new byte[uncompressedSize];
                            var result = lzma.decompressBufferFixed(compressed, ref decompressedBuffer, false, false, (int)uncompressedSize);
                            Debug.Log("result: " + result);
                            //extractor.ExtractFile(0, mem);
                        }
                        catch(Exception e)
                        {
                            Debug.Log(e.ToString());
                        }
                    }
                    mem.Position = 0;

                    BinaryFormatter formatter = new BinaryFormatter();
                    //formatter.Binder = new typeconvertor();
                    includedTextures = (List<IncludedTexture>)formatter.Deserialize(mem);

                    s.Stop();
                    Console.WriteLine("Extraction took: " + s.ElapsedMilliseconds);
                }
            }

            return includedTextures;
        }

        private void RemoveAllAddedTextures()
        {
            Debug.Log("Removing all textures...");

            if (loadedTextures == null)
                loadedTextures = new Dictionary<string, Texture2D>();

            if (FindObjectOfType<ObjectHolderSelection>() == null || FindObjectOfType<ObjectHolderSelection>().allAnimals == null)
            {
                Debug.Log("allAnimals null");
                return;
            }

            List<GameObject> allAnimals = UnityEngine.Object.FindObjectOfType<ObjectHolderSelection>().allAnimals;

            foreach (var singleTexture in loadedTextures)
            {
                Debug.Log("Removing: " + singleTexture.Key);
                var data = IncludedTextureFromName(singleTexture.Key);
                Debug.Log(data.Genus + " - " + data.Integument + " - " + data.Name + " - " + data.Type);
                if (allAnimals == null)
                {
                    Debug.Log("allAnimals is null?");
                    continue;
                }
                var previewGameobject = allAnimals.FirstOrDefault(e => e.name.Trim().ToLower() == data.Genus.ToLower());
                if (previewGameobject != null)
                {
                    AnimalPreview component = previewGameobject.GetComponent<AnimalPreview>();
                    if (component == null)
                    {
                        Debug.Log("No AnimalPreview component for this genus: " + data.Genus.ToLower());
                    }
                    else
                    {
                        RemoveSkinByType(component, singleTexture.Value, data.Type, data.Integument.ToLower() == "feathered");

                        if (data.Integument.ToLower() == "feathered")
                        {
                            //scan case insensitive
                            var nameFound = component.featheredSkinNames.FirstOrDefault(e => e.ToLower() == data.Name.ToLower());
                            if (nameFound != null)
                            {
                                component.featheredSkinNames.Remove(nameFound);
                                Debug.Log(nameFound + " removed");
                            }
                            else
                            {
                                //Debug.Log("Name not found for: " + data.Name.ToLower());
                            }
                        }
                        else
                        {
                            var nameFound = component.scalySkinNames.FirstOrDefault(e => e.ToLower() == data.Name.ToLower());
                            if (nameFound != null)
                            {
                                component.scalySkinNames.Remove(nameFound);
                                Debug.Log(nameFound + " removed");
                            }
                            else
                            {
                                //Debug.Log("Name not found for: " + data.Name.ToLower());
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("No AnimalPreview for this genus: " + data.Genus.ToLower());
                }
                
                
            }
            loadedTextures.Clear();
        }


        public static void AddSkinByType(AnimalPreview preview, Texture2D tex, string type, bool feathered)
        {

            if (feathered)
            {
                switch (type)
                {
                    case "Male":
                        preview.featheredMaleSkins.Add(tex);
                        break;
                    case "Female":
                        preview.featheredFemaleSkins.Add(tex);
                        break;
                    case "Male and Female":
                        preview.featheredMaleSkins.Add(tex);
                        preview.featheredFemaleSkins.Add(tex);
                        break;
                    case "Adolescent":
                        preview.featheredAdolescentSkins.Add(tex);
                        break;
                    case "Baby":
                        preview.featheredBabySkins.Add(tex);
                        break;
                    case "NormalMap":
                        preview.featheredNormalMapSkins.Add(tex);
                        break;
                    case "Albino":
                        preview.featheredAlbinoSkins.Add(tex);
                        break;
                    case "Melanistic":
                        preview.featheredMelanisticSkins.Add(tex);
                        break;
                    case "Baby Albino":
                        preview.featheredBabyAlbinoSkins.Add(tex);
                        break;
                    case "Baby Melanistic":
                        preview.featheredBabyMelanisticSkins.Add(tex);
                        break;
                }                
            }
            else
            {
                switch (type)
                {
                    case "Male":
                        preview.scalyMaleSkins.Add(tex);
                        break;
                    case "Female":
                        preview.scalyFemaleSkins.Add(tex);
                        break;
                    case "Male and Female":
                        preview.scalyMaleSkins.Add(tex);
                        preview.scalyFemaleSkins.Add(tex);
                        break;
                    case "Adolescent":
                        preview.scalyAdolescentSkins.Add(tex);
                        break;
                    case "Baby":
                        preview.scalyBabySkins.Add(tex);
                        break;
                    case "NormalMap":
                        preview.scalyNormalMapsSkins.Add(tex);
                        break;
                    case "Albino":
                        preview.scalyAlbinoSkins.Add(tex);
                        break;
                    case "Melanistic":
                        preview.scalyMelanisticSkins.Add(tex);
                        break;
                    case "Baby Albino":
                        preview.scalyBabyAlbinoSkins.Add(tex);
                        break;
                    case "Baby Melanistic":
                        preview.scalyBabyMelanisticSkins.Add(tex);
                        break;
                }
            }
        }
        public static void RemoveSkinByType(AnimalPreview preview, Texture2D tex, string type, bool feathered)
        {
            Debug.Log("RemoveSkinByType: " + type);
            if (feathered)
            {
                switch (type)
                {
                    case "Male":
                    case "male":
                        preview.featheredMaleSkins.Remove(tex);
                        break;
                    case "Female":
                    case "female":
                        preview.featheredFemaleSkins.Remove(tex);
                        break;
                    case "Male and Female":
                    case "male and female":
                        preview.featheredMaleSkins.Remove(tex);
                        preview.featheredFemaleSkins.Remove(tex);
                        break;
                    case "Adolescent":
                    case "adolescent":
                        preview.featheredAdolescentSkins.Remove(tex);
                        break;
                    case "Baby":
                    case "baby":
                        preview.featheredBabySkins.Remove(tex);
                        break;
                    case "NormalMap":
                    case "normalmap":
                        preview.featheredNormalMapSkins.Remove(tex);
                        break;
                    case "Albino":
                    case "albino":
                        preview.featheredAlbinoSkins.Remove(tex);
                        break;
                    case "Melanistic":
                    case "melanistic":
                        preview.featheredMelanisticSkins.Remove(tex);
                        break;
                    case "Baby Albino":
                    case "baby albino":
                        preview.featheredBabyAlbinoSkins.Remove(tex);
                        break;
                    case "Baby Melanistic":
                    case "baby melanistic":
                        preview.featheredBabyMelanisticSkins.Remove(tex);
                        break;
                    default:
                        Debug.Log("RemoveSkinByType hit default. Check the name!");
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case "Male":
                    case "male":
                        preview.scalyMaleSkins.Remove(tex);
                        break;
                    case "Female":
                    case "female":
                        preview.scalyFemaleSkins.Remove(tex);
                        break;
                    case "Male and Female":
                    case "male and female":
                        preview.scalyMaleSkins.Remove(tex);
                        preview.scalyFemaleSkins.Remove(tex);
                        break;
                    case "Adolescent":
                    case "adolescent":
                        preview.scalyAdolescentSkins.Remove(tex);
                        break;
                    case "Baby":
                    case "baby":
                        preview.scalyBabySkins.Remove(tex);
                        break;
                    case "NormalMap":
                    case "normalmap":
                        preview.scalyNormalMapsSkins.Remove(tex);
                        break;
                    case "Albino":
                    case "albino":
                        preview.scalyAlbinoSkins.Remove(tex);
                        break;
                    case "Melanistic":
                    case "melanistic":
                        preview.scalyMelanisticSkins.Remove(tex);
                        break;
                    case "Baby Albino":
                    case "baby albino":
                        preview.scalyBabyAlbinoSkins.Remove(tex);
                        break;
                    case "Baby Melanistic":
                    case "baby melanistic":
                        preview.scalyBabyMelanisticSkins.Remove(tex);
                        break;
                    default:
                        Debug.Log("RemoveSkinByType hit default. Check the name!");
                        break;
                }
            }
        }
        public static Texture2D GetDefaultTextureFromPreviewByType(AnimalPreview preview, string type, bool feathered)
        {
            Texture2D result = new Texture2D(2, 2);
            if (feathered)
            {
                bool flag = type == "Male";
                if (flag)
                {
                    bool flag2 = preview.featheredMaleSkins.Count > 0;
                    if (flag2)
                    {
                        result = (preview.featheredMaleSkins[0] as Texture2D);
                    }
                }
                else
                {
                    bool flag3 = type == "Female";
                    if (flag3)
                    {
                        bool flag4 = preview.featheredFemaleSkins.Count > 0;
                        if (flag4)
                        {
                            result = (preview.featheredFemaleSkins[0] as Texture2D);
                        }
                    }
                    else
                    {
                        bool flag5 = type == "Baby";
                        if (flag5)
                        {
                            bool flag6 = preview.featheredBabySkins.Count > 0;
                            if (flag6)
                            {
                                result = (preview.featheredBabySkins[0] as Texture2D);
                            }
                        }
                        else
                        {
                            bool flag7 = type == "NormalMap";
                            if (flag7)
                            {
                                bool flag8 = preview.featheredNormalMapSkins.Count > 0;
                                if (flag8)
                                {
                                    result = (preview.featheredNormalMapSkins[0] as Texture2D);
                                }
                            }
                            else
                            {
                                bool flag9 = type == "Adolescent";
                                if (flag9)
                                {
                                    bool flag10 = preview.featheredAdolescentSkins.Count > 0;
                                    if (flag10)
                                    {
                                        result = (preview.featheredAdolescentSkins[0] as Texture2D);
                                    }
                                }
                                else
                                {
                                    bool flag11 = type == "Albino";
                                    if (flag11)
                                    {
                                        bool flag12 = preview.featheredAlbinoSkins.Count > 0;
                                        if (flag12)
                                        {
                                            result = (preview.featheredAlbinoSkins[0] as Texture2D);
                                        }
                                    }
                                    else
                                    {
                                        bool flag13 = type == "Melanistic";
                                        if (flag13)
                                        {
                                            bool flag14 = preview.featheredMelanisticSkins.Count > 0;
                                            if (flag14)
                                            {
                                                result = (preview.featheredMelanisticSkins[0] as Texture2D);
                                            }
                                        }
                                        else
                                        {
                                            bool flag15 = type == "Baby Albino";
                                            if (flag15)
                                            {
                                                bool flag16 = preview.featheredBabyAlbinoSkins.Count > 0;
                                                if (flag16)
                                                {
                                                    result = (preview.featheredBabyAlbinoSkins[0] as Texture2D);
                                                }
                                            }
                                            else
                                            {
                                                bool flag17 = type == "Baby Melanistic";
                                                if (flag17)
                                                {
                                                    bool flag18 = preview.featheredBabyMelanisticSkins.Count > 0;
                                                    if (flag18)
                                                    {
                                                        result = (preview.featheredBabyMelanisticSkins[0] as Texture2D);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                bool flag19 = type == "Male";
                if (flag19)
                {
                    bool flag20 = preview.scalyMaleSkins.Count > 0;
                    if (flag20)
                    {
                        result = (preview.scalyMaleSkins[0] as Texture2D);
                    }
                }
                else
                {
                    bool flag21 = type == "Female";
                    if (flag21)
                    {
                        bool flag22 = preview.scalyFemaleSkins.Count > 0;
                        if (flag22)
                        {
                            result = (preview.scalyFemaleSkins[0] as Texture2D);
                        }
                    }
                    else
                    {
                        bool flag23 = type == "Baby";
                        if (flag23)
                        {
                            bool flag24 = preview.scalyBabySkins.Count > 0;
                            if (flag24)
                            {
                                result = (preview.scalyBabySkins[0] as Texture2D);
                            }
                        }
                        else
                        {
                            bool flag25 = type == "NormalMap";
                            if (flag25)
                            {
                                bool flag26 = preview.scalyNormalMapsSkins.Count > 0;
                                if (flag26)
                                {
                                    result = (preview.scalyNormalMapsSkins[0] as Texture2D);
                                }
                            }
                            else
                            {
                                bool flag27 = type == "Adolescent";
                                if (flag27)
                                {
                                    bool flag28 = preview.scalyAdolescentSkins.Count > 0;
                                    if (flag28)
                                    {
                                        result = (preview.scalyAdolescentSkins[0] as Texture2D);
                                    }
                                }
                                else
                                {
                                    bool flag29 = type == "Albino";
                                    if (flag29)
                                    {
                                        bool flag30 = preview.scalyAlbinoSkins.Count > 0;
                                        if (flag30)
                                        {
                                            result = (preview.scalyAlbinoSkins[0] as Texture2D);
                                        }
                                    }
                                    else
                                    {
                                        bool flag31 = type == "Melanistic";
                                        if (flag31)
                                        {
                                            bool flag32 = preview.scalyMelanisticSkins.Count > 0;
                                            if (flag32)
                                            {
                                                result = (preview.scalyMelanisticSkins[0] as Texture2D);
                                            }
                                        }
                                        else
                                        {
                                            bool flag33 = type == "Baby Albino";
                                            if (flag33)
                                            {
                                                bool flag34 = preview.scalyBabyAlbinoSkins.Count > 0;
                                                if (flag34)
                                                {
                                                    result = (preview.scalyBabyAlbinoSkins[0] as Texture2D);
                                                }
                                            }
                                            else
                                            {
                                                bool flag35 = type == "Baby Melanistic";
                                                if (flag35)
                                                {
                                                    bool flag36 = preview.scalyBabyMelanisticSkins.Count > 0;
                                                    if (flag36)
                                                    {
                                                        result = (preview.scalyBabyMelanisticSkins[0] as Texture2D);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public List<Texture> GetAllSkins()
        {
            if (!GameObject.FindObjectOfType< AnimalPreview>())
            {
                Debug.LogError("Could not find AnimalPreview!");
                return new List<Texture>();
            }
            return GameObject.FindObjectOfType<AnimalPreview>().allSkins;
        }
        public void SaveTexture(Texture tex)
        {
            string text2 = Application.dataPath + "/Mods/Textures/";
            Directory.CreateDirectory(text2);
        }
        public static void SaveTextureToFile(Texture2D texture, string filename)
        {
            Texture2D texture2D = CreateCopy(texture);
            File.WriteAllBytes(filename, texture2D.EncodeToPNG());
        }
        public static Texture2D CreateCopy(Texture2D source)
        {
            int width = source.width;
            int height = source.height;
            source.filterMode = 0;
            RenderTexture temporary = RenderTexture.GetTemporary(width, height);
            temporary.filterMode = 0;
            RenderTexture.active = temporary;
            Graphics.Blit(source, temporary);
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.ReadPixels(new Rect(0f, 0f, (float)width, (float)width), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            return texture2D;
        }

        //this method is not available in .net 3.5, so we have to make one ourself
        public static void CopyTo(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        private void AddSingleTexture(IncludedTexture includedTexture, AnimalPreview component)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.name = string.Concat(new string[]
            {
            "T" + TEXTURE_NAME_DELIMITER,
                includedTexture.Name.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Genus.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Integument.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Type.ToLower()
            });
            Debug.Log("tex.name: " + tex.name);
            tex.LoadImage(includedTexture.Texture);
            AddSkinByType(component, tex, includedTexture.Type, includedTexture.Integument.ToLower() == "feathered");
            component.allSkins.RemoveAll((Texture e) => e.name == tex.name);
            component.allSkins.Add(tex);
            loadedTextures.Add(GetFormattedTextureName(includedTexture), tex);
        }
        private void AddSingleTexture(string name, string genus, string integument, string type, Texture2D tex, AnimalPreview component)
        {
            tex.name = string.Concat(new string[]
            {
            "T" + TEXTURE_NAME_DELIMITER,
                name.ToLower(),
                TEXTURE_NAME_DELIMITER,
                genus.ToLower(),
                TEXTURE_NAME_DELIMITER,
                integument.ToLower(),
                TEXTURE_NAME_DELIMITER,
                type.ToLower()
            });
            Debug.Log("tex.name: " + tex.name);
            AddSkinByType(component, tex, type, integument.ToLower() == "feathered");
            component.allSkins.RemoveAll((Texture e) => e.name == tex.name);
            component.allSkins.Add(tex);
            loadedTextures.Add(GetFormattedTextureName(name, genus, integument, type), tex);
        }

        private string GetFormattedTextureName(string name, string genus, string integument, string type)
        {
            return string.Concat(new string[]
            {
            "T" + TEXTURE_NAME_DELIMITER,
                name.ToLower(),
                TEXTURE_NAME_DELIMITER,
                genus.ToLower(),
                TEXTURE_NAME_DELIMITER,
                integument.ToLower(),
                TEXTURE_NAME_DELIMITER,
                type.ToLower()
            });
        }
        private string GetFormattedTextureName(IncludedTexture includedTexture)
        {
            return string.Concat(new string[]
             {
            "T" + TEXTURE_NAME_DELIMITER,
                includedTexture.Name.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Genus.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Integument.ToLower(),
                TEXTURE_NAME_DELIMITER,
                includedTexture.Type.ToLower()
            });
        }
        private IncludedTexture IncludedTextureFromName(string name)
        {
            IncludedTexture newIncludedTexture = new IncludedTexture();
            var split = name.Split('=');
            newIncludedTexture.Name = split[1];
            newIncludedTexture.Genus = split[2];
            newIncludedTexture.Integument = split[3];
            newIncludedTexture.Type = split[4];
            return newIncludedTexture;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


    }

    /*
    sealed class typeconvertor : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type returntype = null;
            if (assemblyName == "TexturePackCreator, Version=1.0.0.0, Culture = neutral, PublicKeyToken = null")
            {
                assemblyName = Assembly.GetExecutingAssembly().FullName;
                returntype =
                    Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));
                return returntype;
            }

            if (typeName == "System.Collections.Generic.List`1[[Mousemove.Curve,draw on desktop, Version = 1.0.0.0, Culture = neutral,PublicKeyToken = null]]")
            {
                typeName =
                    typeName.Replace("draw on desktop,Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",Assembly.GetExecutingAssembly().FullName);
                returntype =
                    Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));
                return returntype;
            }
            return returntype;
        }
    }
    */
    

}
