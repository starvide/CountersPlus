﻿using System;
using System.Collections.Generic;
using System.Linq;
using CountersPlus.Config;
using UnityEngine;
using TMPro;
using CountersPlus.Custom;
using BS_Utils.Gameplay;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage;
using CountersPlus.UI.ViewControllers.ConfigModelControllers;
using IPA.Utilities;
using HMUI;
using UnityEngine.UI;
using System.Collections;

namespace CountersPlus.UI.ViewControllers
{
    class CountersPlusEditViewController : BSMLResourceViewController
    {
        private static FieldAccessor<ScrollView, Button>.Accessor upButtonAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageUpButton");
        private static FieldAccessor<ScrollView, Button>.Accessor downButtonAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageDownButton");

        public override string ResourceName => "CountersPlus.UI.BSML.EditBase.bsml";
        public static CountersPlusEditViewController Instance;
        private static RectTransform rect;

        internal static List<GameObject> LoadedElements = new List<GameObject>(); //Mass clearing

        [UIObject("body")] internal GameObject SettingsContainer;
        [UIObject("settings_parent")] private GameObject SettingsParent;
        [UIComponent("ScrollContent")] private BSMLScrollableContainer ScrollView;
        [UIComponent("name")] private TextMeshProUGUI SettingsName;

        private static ConfigModel SelectedConfigModel = null;
        private static bool wasInMainSettingsMenu = false;

        private const int ItemsPerColumn = 17;

        private static void SetPositioning(RectTransform r, float x, float y, float w, float h, float pivotX)
        {
            r.anchorMin = new Vector2(x, y);
            r.anchorMax = new Vector2(x + w, y + h);
            r.pivot = new Vector2(pivotX, 1);
            r.sizeDelta = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
            Instance = this;
            rect = rectTransform;
            if (!firstActivation && SelectedConfigModel != null)
                UpdateSettings(SelectedConfigModel);
            else if (!firstActivation && wasInMainSettingsMenu)
                ShowMainSettings();
        }

        internal static void ShowContributors()
        {
            Dictionary<string, string> contributors = new Dictionary<string, string>(ContributorsAndDonators.Contributors);
            string user = GetUserInfo.GetUserName();
            if (user == null) user = "You";
            if (contributors.ContainsKey(user))
                contributors.Add($"{user}, again!", "For enjoying this mod!");
            else contributors.Add(user, "For enjoying this mod!"); //Teehee :)

            ClearScreen();
            TextMeshProUGUI contributorLabel;
            contributorLabel = BeatSaberUI.CreateText(rect, "Thanks to these contributors for, directly or indirectly, helping make Counters+ what it is!", Vector2.zero);
            contributorLabel.fontSize = 3;
            contributorLabel.alignment = TextAlignmentOptions.Center;
            SetPositioning(contributorLabel.rectTransform, 0, 0.85f, 1, 0.166f, 0.5f);
            LoadedElements.Add(contributorLabel.gameObject);

            for (int i = 0; i < contributors.Count; i++)
            {
                string contributorName = contributors.Keys.ToList()[i];
                string contributorContent = contributors[contributorName];
                TextMeshProUGUI contributor = BeatSaberUI.CreateText(rect, $"<color=#00c0ff>{contributorName}</color> | {contributorContent}", Vector2.zero);
                contributor.fontSize = 3;
                contributor.alignment = TextAlignmentOptions.Left;

                float X = (Mathf.Floor(i / ItemsPerColumn) * 0.5f) + 0.05f;
                float Y = 0.8f - ((i % ItemsPerColumn) * 0.05f);
                SetPositioning(contributor.rectTransform, X, Y, 1, 0.166f, 0.5f);
                LoadedElements.Add(contributor.gameObject);
            }
        }

        internal static void ShowDonators()
        {
            List<string> allDonatorsAndPatreonSupporters = ContributorsAndDonators.Donators.Distinct().OrderBy(x => x).ToList();
            TextMeshProUGUI donatorLabel = BeatSaberUI.CreateText(rect,
                "Thanks to these <color=#FF0048>Ko-fi</color> and <color=#FF0048>Patreon</color> supporters! " +
                "<i>DM me on Discord for any corrections to these names.</i>", Vector2.zero);

            ClearScreen();
            donatorLabel.fontSize = 3;
            donatorLabel.alignment = TextAlignmentOptions.Center;
            SetPositioning(donatorLabel.rectTransform, 0, 0.85f, 1, 0.166f, 0.5f);
            LoadedElements.Add(donatorLabel.gameObject);

            for (int i = 0; i < allDonatorsAndPatreonSupporters.Count(); i++)
            {
                string supporter = allDonatorsAndPatreonSupporters[i];
                TextMeshProUGUI donator = BeatSaberUI.CreateText(rect, $"<color=#FF0048>{supporter}</color>", Vector2.zero);
                donator.fontSize = 3;
                donator.alignment = TextAlignmentOptions.Left;
                float X = (Mathf.Floor(i / ItemsPerColumn) * 0.35f) + 0.05f;
                float Y = 0.8f - ((i % ItemsPerColumn) * 0.05f);
                SetPositioning(donator.rectTransform, X, Y, 1, 0.166f, 0.5f);
                LoadedElements.Add(donator.gameObject);
            }
        }

        internal static void ShowMainSettings()
        {
            ClearScreen(true);
            Instance.SettingsName.text = "Main Settings";
            Type controllerType = Type.GetType($"CountersPlus.UI.ViewControllers.ConfigModelControllers.MainSettingsController");
            ConfigModelController.GenerateController(controllerType, Instance.SettingsContainer,"CountersPlus.UI.BSML.MainSettings.bsml");
            MockCounter.Highlight<ConfigModel>(null);
            SelectedConfigModel = null;
            wasInMainSettingsMenu = true;
            ResetScrollViewContent();
        }

        internal static void UpdateTitle(string title)
        {
            Instance.SettingsName.text = title;
        }

        public static void UpdateSettings<T>(T settings) where T : ConfigModel
        {
            try
            {
                if (settings is null) return;
                wasInMainSettingsMenu = false;
                SelectedConfigModel = settings;
                ClearScreen(true);
                MockCounter.Highlight(settings);
                ConfigModelController.ClearAllControllers();
                string name = string.Join("", settings.DisplayName.Split(' '));
                if (settings is CustomConfigModel custom)
                {
                    ConfigModelController.GenerateController(custom.CustomCounter.CustomSettingsHandler,
                        Instance.SettingsContainer, custom.CustomCounter.CustomSettingsResource, true, settings);
                    name = custom.CustomCounter.Name;
                }
                else
                {
                    Type controllerType = Type.GetType($"CountersPlus.UI.ViewControllers.ConfigModelControllers.{name}Controller");
                    ConfigModelController controller = ConfigModelController.GenerateController(settings, controllerType, Instance.SettingsContainer);
                }
                Instance.SettingsName.text = $"{(settings is null ? "Oops!" : $"{settings.DisplayName} Settings")}";
                ResetScrollViewContent();
            }
            catch (Exception e) { Plugin.Log(e.ToString(), LogInfo.Fatal, "Go to the Counters+ GitHub and open an Issue. This shouldn't happen!"); }
        }

        internal static void ResetScrollViewContent() => Instance.StartCoroutine(Instance.WaitThenDirtyTheFuckingScrollView());

        private IEnumerator WaitThenDirtyTheFuckingScrollView()
        {
            yield return new WaitUntil(() => Instance != null && Instance.ScrollView != null && Instance.ScrollView.ContentRect != null);
            Instance.ScrollView.ContentRect.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            Instance.ScrollView.ContentRect.gameObject.SetActive(true);
        }

        internal static void ClearScreen(bool enableSettings = false)
        {
            foreach (GameObject go in LoadedElements) Destroy(go);
            for (int i = 0; i < Instance.SettingsContainer.transform.childCount; i++) Destroy(Instance.SettingsContainer.transform.GetChild(i).gameObject);
            Instance.SettingsParent.SetActive(enableSettings);
        }
    }
}