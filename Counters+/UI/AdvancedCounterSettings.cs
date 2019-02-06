﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CountersPlus.Config;
using CustomUI.BeatSaber;
using CustomUI.Settings;
using UnityEngine;

namespace CountersPlus.UI
{
    public class AdvancedCounterSettings
    {
        internal class ModeViewController : TupleViewController<Tuple<ICounterMode, string>> { }
        static List<Tuple<ICounterMode, string>> speedSettings = new List<Tuple<ICounterMode, string>> {
            {ICounterMode.Average, "Mean Speed" },
            {ICounterMode.Top5Sec, "Top (5 Sec.)" },
            {ICounterMode.Both, "Both" },
            {ICounterMode.SplitAverage, "Split Mean" },
            {ICounterMode.SplitBoth, "Both w/Split" }
        };
        static List<Tuple<ICounterMode, string>> progressSettings = new List<Tuple<ICounterMode, string>> {
            {ICounterMode.BaseGame, "Base Game" },
            {ICounterMode.Original, "Original" },
            {ICounterMode.Percent, "Percentage" }
        };
        static List<Tuple<ICounterMode, string>> scoreSettings = new List<Tuple<ICounterMode, string>>
        {
            {ICounterMode.Both, "Original" }, //Counters+ Counter w/ Points
            {ICounterMode.BaseGame, "Base Game" }, //Base Game w/ Points
            {ICounterMode.BaseWithOutPoints, "Base No Points" }, //Base Game w/ Points Under Combo
            {ICounterMode.LeavePoints, "No Points" }, //Counters+ Counter w/ Points Under Combo
            {ICounterMode.ScoreOnly, "Score Only" }, //Counters+ Counter w/ Points Removed Entirely
        };

        private static HoverHint progressHover;
        private static HoverHint speedHover;
        private static HoverHint scoreHover;

        internal static Dictionary<IConfigModel, Action<SubMenu>> counterUIItems = new Dictionary<IConfigModel, Action<SubMenu>>()
        {
            { CountersController.settings.missedConfig, v => { } },
            { CountersController.settings.noteConfig,
                sub => {
                    var accuracyPercentage = CountersPlusEditViewController.AddList(ref sub, "Show Percentage", "Toggles the percentage of notes hit over total notes.", 2);
                    accuracyPercentage.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
                    accuracyPercentage.GetValue = () => CountersController.settings.noteConfig.ShowPercentage ? 1f : 0f;
                    accuracyPercentage.SetValue = (v) => CountersController.settings.noteConfig.ShowPercentage = v != 0f;

                    var accuracyPrecision = CountersPlusEditViewController.AddList(ref sub, "Percentage Precision", "How precise should the precentage be?", 6);
                    accuracyPrecision.GetTextForValue = (v) => Mathf.RoundToInt(v).ToString();
                    accuracyPrecision.GetValue = () => CountersController.settings.noteConfig.DecimalPrecision;
                    accuracyPrecision.SetValue = (v) => CountersController.settings.noteConfig.DecimalPrecision = Mathf.RoundToInt(v);
                } },
            { CountersController.settings.scoreConfig,
                sub => {
                        var scoreRank = CountersPlusEditViewController.AddList(ref sub, "Display Rank", "Displays the rank as you progress in a song.", 2);
                    scoreRank.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
                    scoreRank.GetValue = () => CountersController.settings.scoreConfig.DisplayRank ? 1f : 0f;
                    scoreRank.SetValue = (v) => CountersController.settings.scoreConfig.DisplayRank = v != 0f;

                    var scoreMode = CountersPlusEditViewController.AddList(ref sub, "Mode", "", scoreSettings.Count());
                    scoreMode.GetTextForValue = (v) => {
                        return scoreSettings[Mathf.RoundToInt(v)].Item2;
                    };
                    scoreMode.GetValue = () => {
                        if (scoreHover == null) scoreHover = BeatSaberUI.AddHintText(scoreMode.transform as RectTransform, determineModeText(CountersController.settings.scoreConfig.Mode));
                        return scoreSettings.ToList().IndexOf(scoreSettings.Where((Tuple<ICounterMode, string> x) => (x.Item1 == CountersController.settings.scoreConfig.Mode)).First());
                    };
                    scoreMode.SetValue = (v) => {
                        if (scoreHover == null) scoreHover = BeatSaberUI.AddHintText(scoreMode.transform as RectTransform, determineModeText(CountersController.settings.scoreConfig.Mode));
                        CountersController.settings.scoreConfig.Mode = scoreSettings[Mathf.RoundToInt(v)].Item1;
                        scoreHover.text = determineModeText(CountersController.settings.scoreConfig.Mode, true);
                    };

                    var scorePrecision = CountersPlusEditViewController.AddList(ref sub, "Percentage Precision", "How precise should the precentage be?", 6);
                    scorePrecision.GetTextForValue = (v) => Mathf.RoundToInt(v).ToString();
                    scorePrecision.GetValue = () => CountersController.settings.scoreConfig.DecimalPrecision;
                    scorePrecision.SetValue = (v) => CountersController.settings.scoreConfig.DecimalPrecision = Mathf.RoundToInt(v);
                } },
            { CountersController.settings.progressConfig,
                sub => {
                    var progressRank = CountersPlusEditViewController.AddList(ref sub, "Show Time Left", "Starts the counter from the end of the song and decreases while the song is played.", 2);
                    progressRank.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
                    progressRank.GetValue = () => CountersController.settings.progressConfig.ProgressTimeLeft ? 1f : 0f;
                    progressRank.SetValue = (v) => CountersController.settings.progressConfig.ProgressTimeLeft = v != 0f;

                    var progressMode = CountersPlusEditViewController.AddList(ref sub, "Mode", "", progressSettings.Count());
                    progressMode.GetTextForValue = (v) => {
                        return progressSettings[Mathf.RoundToInt(v)].Item2;
                    };
                    progressMode.GetValue = () => {
                        if (progressHover == null) progressHover = BeatSaberUI.AddHintText(progressMode.transform as RectTransform, determineModeText(CountersController.settings.progressConfig.Mode));
                        return progressSettings.ToList().IndexOf(progressSettings.Where((Tuple<ICounterMode, string> x) => (x.Item1 == CountersController.settings.progressConfig.Mode)).First());
                    };
                    progressMode.SetValue = (v) => {
                        if (progressHover == null) progressHover = BeatSaberUI.AddHintText(progressMode.transform as RectTransform, determineModeText(CountersController.settings.progressConfig.Mode));
                        CountersController.settings.progressConfig.Mode = progressSettings[Mathf.RoundToInt(v)].Item1;
                        progressHover.text = determineModeText(CountersController.settings.progressConfig.Mode);
                    };
                } },
            { CountersController.settings.speedConfig,
                sub => {
                    var speedPrecision = CountersPlusEditViewController.AddList(ref sub, "Percentage Precision", "How precise should the precentage be?", 6);
                    speedPrecision.GetTextForValue = (v) => Mathf.RoundToInt(v).ToString();
                    speedPrecision.GetValue = () => CountersController.settings.speedConfig.DecimalPrecision;
                    speedPrecision.SetValue = (v) => CountersController.settings.speedConfig.DecimalPrecision = Mathf.RoundToInt(v);

                    var speedMode = CountersPlusEditViewController.AddList(ref sub, "Mode", "", speedSettings.Count());
                    speedMode.GetTextForValue = (v) => {
                        return speedSettings[Mathf.RoundToInt(v)].Item2;
                    };
                    speedMode.GetValue = () => {
                        if (speedHover == null) speedHover = BeatSaberUI.AddHintText(speedMode.transform as RectTransform, determineModeText(CountersController.settings.speedConfig.Mode));
                        return speedSettings.ToList().IndexOf(speedSettings.Where((Tuple<ICounterMode, string> x) => (x.Item1 == CountersController.settings.speedConfig.Mode)).First());
                    };
                    speedMode.SetValue = (v) => {
                        if (speedHover == null) speedHover = BeatSaberUI.AddHintText(speedMode.transform as RectTransform, determineModeText(CountersController.settings.speedConfig.Mode));
                        CountersController.settings.speedConfig.Mode = speedSettings[Mathf.RoundToInt(v)].Item1;
                        speedHover.text = determineModeText(CountersController.settings.speedConfig.Mode);
                    };
                } },
            { CountersController.settings.cutConfig, v => { } },
        };

        private static string determineModeText(ICounterMode Mode, bool alternateText = false)
        {
            string mode = "Unavilable mode!";
            switch (Mode)
            {
                case ICounterMode.Average:
                    mode = "Mean Speed: Average speed of both sabers.";
                    break;
                case ICounterMode.Top5Sec:
                    mode = "Top: Fastest saber speed in the last 5 seconds.";
                    break;
                case ICounterMode.Both:
                    if (alternateText)
                        mode = "Both: Have Counters+ counter with the Points above the percentage.";
                    else
                        mode = "Both: A secondary Counter will be added so both Average and Top Speed will be displayed.";
                    break;
                case ICounterMode.SplitAverage:
                    mode = "Split Mean: Displays averages for each saber, separately.";
                    break;
                case ICounterMode.SplitBoth:
                    mode = "Split Both: Displays both metrics, except the Average is split between two sabers.";
                    break;
                case ICounterMode.BaseGame:
                    mode = "Base Game: Uses the base game counter.\n<color=#FF0000>Some settings will not apply in this mode.</color>";
                    break;
                case ICounterMode.Original:
                    mode = "Original: Uses the original display mode, with a white circle bordering a time.";
                    break;
                case ICounterMode.Percent:
                    mode = "Percent: Displays a simple percent of the completed song.\n<color=#FF0000>Some settings will not apply in this mode.</color>";
                    break;
                case ICounterMode.BaseWithOutPoints:
                    mode = "Base No Points: Uses the base game counter, except the Points will be under the combo.\n<color=#FF0000>This might conflict with counters positioned below the combo.</color>";
                    break;
                case ICounterMode.LeavePoints:
                    mode = "No Points: Uses Counters+ counter, except the Points will be under the combo.\n<color=#FF0000>This might conflict with counters positioned below the combo.</color>";
                    break;
                case ICounterMode.ScoreOnly:
                    mode = "Score Only: Uses Counters+ counter, and completely removes the Points.";
                    break;
            }
            return "How should this Counter display data?\n" + mode;
        }
    }
}
