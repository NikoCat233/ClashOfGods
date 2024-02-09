﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using COG.Config.Impl;
using COG.Utils;
using COG.Utils.Version;
using Il2CppSystem.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace COG.Patch;

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuPatch
{
    public static GameObject? CustomBG;
    public static readonly List<PassiveButton> Buttons = new();
    public static bool PopupCreated = false;

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    private static void LoadButtons(MainMenuManager __instance)
    {
        Buttons.Clear();
        var template = __instance.creditsButton;

        if (!template) return;

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.2f, 0.38f),
            LanguageConfig.Instance.Github,
            () => { Application.OpenURL("https://github.com/CognifyDev/ClashOfGods/"); }, Color.blue);

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.7f, 0.38f),
            LanguageConfig.Instance.QQ,
            () =>
            {
                Application.OpenURL(
                    "https://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=R63D8foTESsZ9TmGFbkaSPix0q9WGwtq&authKey=1rfvioSJhdni%2BpFvBqS5rFXkZKXNDeeFO50ZKGPzwtlLKwmJqftlDcolx%2FkJ3jLC&noverify=0&group_code=607761127");
            }, Color.cyan);

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f),
            LanguageConfig.Instance.Discord, () => { Application.OpenURL("https://discord.gg/uWZGh4Chde"); },
            Color.gray);
        
        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f / 2),
            LanguageConfig.Instance.UpdateButtonString, () =>
            {
                if (Main.LatestVersion.Equals(VersionInfo.Empty))
                {
                    SystemUtils.OpenMessageBox(LanguageConfig.Instance.NonCheck, "WARNING");
                    return;
                }

                if (!Main.LatestVersion.IsNewerThan(Main.VersionInfo))
                {
                    SystemUtils.OpenMessageBox(LanguageConfig.Instance.UpToDate, "WARNING");
                    return;
                }

#pragma warning disable SYSLIB0014
                using var client = new WebClient();
                client.DownloadFile(
                    $"https://download.yzuu.cf/CognifyDev/ClashOfGods/releases/download/{Main.LatestVersion}/ClashOfGods.dll",
                    "BepInEx/plugins/ClashOfGods.dll.new"
                );
                
                File.WriteAllText("BepInEx/plugins/do.vbs", "WScript.Sleep 1000\n\nstrFileToDelete = \"ClashOfGods.dll\"\nstrFileToRename = \"ClashOfGods.dll.new\"\nstrScriptToDelete = WScript.ScriptFullName\n\nSet fs = CreateObject(\"Scripting.FileSystemObject\")\n\nIf fs.FileExists(strFileToDelete) Then\n    fs.DeleteFile strFileToDelete\nEnd If\n\nIf fs.FileExists(strFileToRename) Then\n    fs.MoveFile strFileToRename, strFileToDelete\nEnd If\n\nIf fs.FileExists(strScriptToDelete) Then\n    fs.DeleteFile strScriptToDelete\nEnd If");

                Process.Start("BepInEx/plugins/do.vbs");
                Application.Quit();
            }, Color.yellow);
/*
        if (Main.BetaVersion)
            CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f / 2),
                LanguageConfig.Instance.BetaVersionRegisteredUserDisplay.CustomFormat(Main.RegisteredBetaUsers.Count),
                () => { }, Color.yellow);*/
    }

    /// <summary>
    ///     在主界面创建一个按钮
    /// </summary>
    /// <param name="__instance">MainMenuManager 的实例</param>
    /// <param name="template">按钮模板</param>
    /// <param name="parent">父游戏物体</param>
    /// <param name="anchorPoint">与父游戏物体的相对位置</param>
    /// <param name="text">按钮文本</param>
    /// <param name="action">点击按钮的动作</param>
    /// <returns>返回这个按钮</returns>
    private static void CreateButton(MainMenuManager __instance, PassiveButton template, Transform? parent,
        Vector2 anchorPoint, string text, Action action, Color color)
    {
        if (!parent) return;

        var button = Object.Instantiate(template, parent);
        button.GetComponent<AspectPosition>().anchorPoint = anchorPoint;
        var buttonSprite = button.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        buttonSprite.color = color;
        __instance.StartCoroutine(Effects.Lerp(0.5f,
            new Action<float>(_ => { button.GetComponentInChildren<TMP_Text>().SetText(text); })));

        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(action);

        Buttons.Add(button);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void LoadImage()
    {
        CustomBG = new GameObject("CustomBG");
        CustomBG.transform.position = new Vector3(1.8f, 0.2f, 0f);
        var bgRenderer = CustomBG.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 295f);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void InitPopup()
    {
        if (PopupCreated) return;
        if (!Camera.main) return;
        var popup = Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
        var bg = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = bg.size;
        size.x *= 2.5f;
        bg.size = size;
        popup.TextAreaTMP.fontSizeMin = popup.TextAreaTMP.fontSizeMax = popup.TextAreaTMP.fontSize;
        GameUtils.Popup = popup;
        PopupCreated = true;
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    private static void Hide()
    {
        if (CustomBG != null) CustomBG.SetActive(false);
        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    private static void Show()
    {
        if (CustomBG != null) CustomBG.SetActive(true);
        foreach (var btn in Buttons.Where(btn => btn != null && btn.gameObject != null)) btn.gameObject.SetActive(true);
    }
}