using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polytopia.IO;
using PolytopiaBackendBase.Game;
using UnityEngine;
using static UnityEngine.PlayerPrefs;
using UnityEngine.UI;
using System.Text.Json;
using Cpp2IL.Core.Extensions;
using I2.Loc;
using Il2CppInterop.Runtime;
using Polytopia.Data;
using TMPro;
using static PopupBase;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Il2CppSystem.Linq;

namespace BgLib;

public static class Main
{
    public static ManualLogSource? modLogger;
    public static void Load(ManualLogSource logger)
    {
        modLogger = logger;
        Harmony.CreateAndPatchAll(typeof(Main));
    }

    public static Sprite NatureBg = new Sprite();
    public static Sprite TitleBg = new Sprite();
    public static Color OgColor = new Color();

    public class MenuData
    {
        public string bgImageName = "default";
        public string buttonImageName = "polyicon";
        public string titleImageName = "default";
        public Color? scrollerGradientRGBA = null;
    }
    public static Dictionary<string, MenuData> menuDataDict = new Dictionary<string, MenuData>();
    public static string currentlySelectedBg = "polytopia";

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    private static void GameLogicData_Parse(GameLogicData __instance, JObject rootObject)
    {
        foreach (JToken jtoken in rootObject.SelectTokens("$.menuData.*").ToList())
        {
            JObject? token = jtoken.TryCast<JObject>();
            if (token != null)
            {
                MenuData menuData = new MenuData();
                if (token["background"] != null)
                {
                    string val = token["background"]!.ToObject<string>();
                    menuData.bgImageName = val;
                    token.Remove("background");
                }
                if (token["icon"] != null)
                {
                    string val = token["icon"]!.ToObject<string>();
                    menuData.buttonImageName = val;
                    token.Remove("icon");
                }
                if (token["title"] != null)
                {
                    string val = token["title"]!.ToObject<string>();
                    menuData.titleImageName = val;
                    token.Remove("title");
                }
                if (token["scrollerGradientColor"] != null)
                {
                    string val = token["scrollerGradientColor"]!.ToObject<string>();
                    string[] vals = val.Split(',');

                    float r = 0;
                    float g = 0;
                    float b = 0;
                    float a = 1;

                    float.TryParse(vals[0], out r);
                    float.TryParse(vals[1], out g);
                    float.TryParse(vals[2], out b);
                    float.TryParse(vals[3], out a);

                    menuData.scrollerGradientRGBA = new Color(r, g, b, a);
                    token.Remove("scrollerGradientColor");
                }
                menuDataDict[token.Path.Split('.').Last()] = menuData;
            }
        }
        menuDataDict["polytopia"] = new MenuData();
        currentlySelectedBg = PlayerPrefs.GetString("selectedBgModName", "polytopia");
    }

    [HarmonyPrefix] 
    [HarmonyPatch(typeof(ScrollerGradient), nameof(ScrollerGradient.OnEnable))]
    private static void ScrollerGradient_OnEnable(ScrollerGradient __instance)
    {
        Image[] allImages = GameObject.FindObjectsOfType<UnityEngine.UI.Image>();
        foreach (Image image in allImages)
        {
            if (image.gameObject.name == "ScrollerTopGradient")
            {
                if(menuDataDict[currentlySelectedBg].scrollerGradientRGBA == null)
                {
                    return;
                }
                image.color = (Color)menuDataDict[currentlySelectedBg].scrollerGradientRGBA!;
                
                
                

                modLogger!.LogInfo(menuDataDict[currentlySelectedBg].scrollerGradientRGBA!.ToString());
                modLogger!.LogInfo(image.color.ToString());
            }
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(StartScreen), nameof(StartScreen.Start))] //this piece of fucking SHIT
    public static void StartScreen_Start(StartScreen __instance)
    {
        



        LogoContainer? logoContainer = null;
        StartSceneBg? background = null;

        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> LogoContainers = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(LogoContainer)));

        foreach (UnityEngine.Object item in LogoContainers)
        {
            LogoContainer? container = item.TryCast<LogoContainer>();
            if (container == null)
            {
                continue;
            }
            logoContainer = container;
        }

        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> StartSceneBackgrounds = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(StartSceneBg)));

        foreach (UnityEngine.Object item in StartSceneBackgrounds)
        {
            StartSceneBg? startSceneBg = item.TryCast<StartSceneBg>();
            if (startSceneBg == null)
            {
                continue;
            }
            background = startSceneBg;
        }

        TitleBg = logoContainer!.bigLogo.sprite;
        NatureBg = background!.nature.sprite;




        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> allLocalizers = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(TMPLocalizer)));

        foreach (UnityEngine.Object item in allLocalizers)
        {
            TMPLocalizer? localizer = item.TryCast<TMPLocalizer>();
            if (localizer == null)
            {
                continue;
            }

            Transform? parent = localizer?.gameObject?.transform?.parent;
            if (parent == null)
            {
                continue;
            }

            string parentName = parent.name;

            if (parentName == "NewsButton")
            {
                GameObject originalButton = parent.gameObject;
                GameObject button = GameObject.Instantiate(originalButton, originalButton.transform.parent);
                button.name = "BgButton";
                button.transform.position = originalButton.transform.position - new Vector3(180, 0, 0);

                UIRoundButton buttonComponent = button.GetComponent<UIRoundButton>();
                buttonComponent.bg.transform.localScale = new Vector3(1.2f, 1.2f, 0);
                buttonComponent.bg.sprite = PolyMod.Registry.GetSprite("default");
                buttonComponent.bg.color = Color.white;


                GameObject.Destroy(buttonComponent.icon.gameObject);
                GameObject.Destroy(buttonComponent.outline.gameObject);

                buttonComponent.OnClicked += (UIButtonBase.ButtonAction)BackgroundButtonClicked;
                BgSwitchAction(buttonComponent, currentlySelectedBg);
            }
        }

        static void BackgroundButtonClicked(int buttonId, BaseEventData eventData)
        {
            UIRoundButton? buttonComponent = null;
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> allLocalizers = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(TMPLocalizer)));

            foreach (UnityEngine.Object item in allLocalizers)
            {
                TMPLocalizer? localizer = item.TryCast<TMPLocalizer>();
                if (localizer == null)
                {
                    continue;
                }

                Transform? parent = localizer?.gameObject?.transform?.parent;
                if (parent == null)
                {
                    continue;
                }

                string parentName = parent.name;

                if (parentName == "BgButton")
                {
                    GameObject button = parent.gameObject;
                    buttonComponent = button.GetComponent<UIRoundButton>();
                }
            }
            if (buttonComponent != null)
            {
                BgSwitchAction(buttonComponent!, Next(menuDataDict.Keys.ToArray<string>(), currentlySelectedBg));
            }
        }
    }
    public static void BgSwitchAction(UIRoundButton buttonComponent, string modName)
    {
        MenuData menuData = new MenuData();

        Sprite? bg = null;
        Sprite? icon = null;
        Sprite? title = null;

        LogoContainer? logoContainer = null;
        StartSceneBg? background = null;

        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> LogoContainers = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(LogoContainer)));

        foreach (UnityEngine.Object item in LogoContainers)
        {
            LogoContainer? container = item.TryCast<LogoContainer>();
            if (container == null)
            {
                continue;
            }
            logoContainer = container;
        }

        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> StartSceneBackgrounds = GameObject.FindObjectsOfTypeAll(Il2CppType.From(typeof(StartSceneBg)));

        foreach (UnityEngine.Object item in StartSceneBackgrounds)
        {
            StartSceneBg? startSceneBg = item.TryCast<StartSceneBg>();
            if (startSceneBg == null)
            {
                continue;
            }
            background = startSceneBg;
        }

        if (modName == "polytopia")
        {
            bg = NatureBg;
            icon = PolyMod.Registry.GetSprite("polyicon");
            title = TitleBg;
        }
        else
        {
            bg = menuDataDict[modName].bgImageName == "default" ? NatureBg : PolyMod.Registry.GetSprite(menuDataDict[modName].bgImageName);
            icon = PolyMod.Registry.GetSprite(menuDataDict[modName].buttonImageName);
            title = menuDataDict[modName].bgImageName == "default" ? TitleBg : PolyMod.Registry.GetSprite(menuDataDict[modName].titleImageName);
        }

        buttonComponent.bg.sprite = icon != null ? icon : PolyMod.Registry.GetSprite("polyicon");

        background!.nature.sprite = bg != null ? bg : NatureBg;

        logoContainer!.bigLogo.sprite = title != null ? title : TitleBg;

        currentlySelectedBg = modName;
        PlayerPrefs.SetString("selectedBgModName", modName);
    }

    public static string Next(string[] strings, string current)
    {
        int currentIndex = Array.IndexOf(strings, current);

        if (currentIndex == -1)
        {
            return "polytopia";
        }

        int nextIndex = (currentIndex + 1) % strings.Length;

        return strings[nextIndex];
    }
}

