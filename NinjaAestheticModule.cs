using System;
using Mod.Courier;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using Mod.Courier.UI;
using UnityEngine;

namespace NinjaAesthetic {
    public class NinjaAestheticModule : CourierModule {
        MultipleOptionButtonInfo switchSkinButton;
        ToggleButtonInfo rainbowModeButton;
        TextEntryButtonInfo rainbowSpeedButton;

        bool rainbowMode;

        float rainbowSpeedMultiplier = 1;

        HSBColor rainbowColor = HSBColor.FromColor(Color.white);

        public override void Load() {
            switchSkinButton = Courier.UI.RegisterMultipleModOptionButton(() => "Switch Skin", null, ChangeSkin, GetSkinIndex, GetTextForSkinIndex);
            rainbowModeButton = Courier.UI.RegisterToggleModOptionButton(() => "Rainbow Mode", OnRainbowModeToggle, (b) => rainbowMode);
            rainbowSpeedButton = Courier.UI.RegisterTextEntryModOptionButton(() => "Rainbow Speed", OnRainbowSpeedEntry, 3, null, () => rainbowSpeedMultiplier.ToString(), TextEntryButtonInfo.CharsetFlags.Dot | TextEntryButtonInfo.CharsetFlags.Number);

            switchSkinButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;

            Courier.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
        }

        void PlayerController_OnUpdate(PlayerController obj) {
            if (rainbowMode) {
                if (rainbowColor.s < 1) rainbowColor.s = Math.Min(rainbowColor.s + .03f, 1);
                rainbowColor.h += .01f * rainbowSpeedMultiplier;
                if (rainbowColor.h > 1) rainbowColor.h = 0;
                Color c = rainbowColor.ToColor();
                foreach (SpriteRenderer renderer in obj.animator.transform.Find("Player_8Bit").GetComponentsInChildren<SpriteRenderer>()) {
                    renderer.color = c;
                }
                foreach (SpriteRenderer renderer in obj.animator.transform.Find("Player_16Bit").GetComponentsInChildren<SpriteRenderer>()) {
                    renderer.color = c;
                }
            }
        }

        bool OnRainbowSpeedEntry(string entry) {
            if (float.TryParse(entry, out float res))
                rainbowSpeedMultiplier = res;
            return true;
        }

        void OnRainbowModeToggle() {
            rainbowMode = !rainbowMode;
            rainbowColor = HSBColor.FromColor(Color.white);
            rainbowModeButton.UpdateStateText();
            CourierLogger.Log("NinjaAesthetic", "Rainbow Mode: " + rainbowMode);
        }

        void ChangeSkin(int index) {
            if (index > (int)ESkin.GHEESLING_BLUE) index = 0;
            if (index < 0) index = (int)ESkin.GHEESLING_BLUE;
            Manager<SkinManager>.Instance.EquipSkin((ESkin)index);
        }

        int GetSkinIndex(MultipleOptionButtonInfo buttonInfo) {
            return (int)Manager<SkinManager>.Instance.CurrentSkinID;
        }

        string GetTextForSkinIndex(int index) {
            switch((ESkin)index) {
                case ESkin.DEFAULT:
                    return "Default";
                case ESkin.DARK_MESSENGER:
                    return "Dark Messenger";
                case ESkin.GHEESLING_RED:
                    return "Red Gheesling";
                case ESkin.GHEESLING_BLUE:
                    return "Blue Gheesling";
            }
            return "???";
        }
    }
}
