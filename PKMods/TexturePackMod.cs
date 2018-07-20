using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PKMods
{
    class TexturePackMod : MonoBehaviour
    {


        public static void AddSkinByType(AnimalPreview preview, Texture2D tex, string type, bool feathered)
        {
            if (feathered)
            {
                bool flag = type == "Male";
                if (flag)
                {
                    preview.featheredMaleSkins.Add(tex);
                }
                else
                {
                    bool flag2 = type == "Female";
                    if (flag2)
                    {
                        preview.featheredFemaleSkins.Add(tex);
                    }
                    else
                    {
                        bool flag3 = type == "Male_and_Female";
                        if (flag3)
                        {
                            preview.featheredMaleSkins.Add(tex);
                            preview.featheredFemaleSkins.Add(tex);
                        }
                        else
                        {
                            bool flag4 = type == "Adolescent";
                            if (flag4)
                            {
                                preview.featheredAdolescentSkins.Add(tex);
                            }
                            else
                            {
                                bool flag5 = type == "Baby";
                                if (flag5)
                                {
                                    preview.featheredBabySkins.Add(tex);
                                }
                                else
                                {
                                    bool flag6 = type == "NormalMap";
                                    if (flag6)
                                    {
                                        preview.featheredNormalMapSkins.Add(tex);
                                    }
                                    else
                                    {
                                        bool flag7 = type == "Albino";
                                        if (flag7)
                                        {
                                            preview.featheredAlbinoSkins.Add(tex);
                                        }
                                        else
                                        {
                                            bool flag8 = type == "Melanistic";
                                            if (flag8)
                                            {
                                                preview.featheredMelanisticSkins.Add(tex);
                                            }
                                            else
                                            {
                                                bool flag9 = type == "Baby Albino";
                                                if (flag9)
                                                {
                                                    preview.featheredBabyAlbinoSkins.Add(tex);
                                                }
                                                else
                                                {
                                                    bool flag10 = type == "Baby Melanistic";
                                                    if (flag10)
                                                    {
                                                        preview.featheredBabyMelanisticSkins.Add(tex);
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
                bool flag11 = type == "Male";
                if (flag11)
                {
                    preview.scalyMaleSkins.Add(tex);
                }
                else
                {
                    bool flag12 = type == "Female";
                    if (flag12)
                    {
                        preview.scalyFemaleSkins.Add(tex);
                    }
                    else
                    {
                        bool flag13 = type == "Male_and_Female";
                        if (flag13)
                        {
                            preview.scalyMaleSkins.Add(tex);
                            preview.scalyFemaleSkins.Add(tex);
                        }
                        else
                        {
                            bool flag14 = type == "Adolescent";
                            if (flag14)
                            {
                                preview.scalyAdolescentSkins.Add(tex);
                            }
                            else
                            {
                                bool flag15 = type == "Baby";
                                if (flag15)
                                {
                                    preview.scalyBabySkins.Add(tex);
                                }
                                else
                                {
                                    bool flag16 = type == "NormalMap";
                                    if (flag16)
                                    {
                                        preview.scalyNormalMapsSkins.Add(tex);
                                    }
                                    else
                                    {
                                        bool flag17 = type == "Albino";
                                        if (flag17)
                                        {
                                            preview.scalyAlbinoSkins.Add(tex);
                                        }
                                        else
                                        {
                                            bool flag18 = type == "Melanistic";
                                            if (flag18)
                                            {
                                                preview.scalyMelanisticSkins.Add(tex);
                                            }
                                            else
                                            {
                                                bool flag19 = type == "Baby Albino";
                                                if (flag19)
                                                {
                                                    preview.scalyBabyAlbinoSkins.Add(tex);
                                                }
                                                else
                                                {
                                                    bool flag20 = type == "Baby Melanistic";
                                                    if (flag20)
                                                    {
                                                        preview.scalyBabyMelanisticSkins.Add(tex);
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
    }
}
