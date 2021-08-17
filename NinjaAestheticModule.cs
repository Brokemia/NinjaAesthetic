using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mod.Courier;
using Mod.Courier.GFX;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using Mod.Courier.Save;
using Mod.Courier.UI;
using UnityEngine;

namespace NinjaAesthetic {
    public class NinjaAestheticModule : CourierModule {
        MultipleOptionButtonInfo switchSkinButton;
        ToggleButtonInfo rainbowModeButton;
        TextEntryButtonInfo rainbowSpeedButton;
        ToggleButtonInfo hideHUDButton;

        Vector2[] pivots = { new Vector2(0.5003458f, 0),
                            new Vector2(0.5157939f, 0),
                            new Vector2(0.5367216f, 0),
                            new Vector2(0.4620033f, -0.6121122f),
                            new Vector2(0.4632693f, -0.6172074f),
                            new Vector2(0.5379966f, -0.5386481f),
                            new Vector2(0.5367309f, -0.697152f),
                            new Vector2(0.3467451f, 0),
                            new Vector2(0.52679f, 0),
                            new Vector2(0.6674278f, 0),
                            new Vector2(0.6674278f, 0),
                            new Vector2(0.4138422f, -0.5546134f),
                            new Vector2(0.4138422f, -0.5200078f),
                            new Vector2(0.4138422f, -0.5546134f),
                            new Vector2(0.4138422f, -0.5200078f),
                            new Vector2(0.5239263f, -0.02511475f),
                            new Vector2(0.52447f, 0),
                            new Vector2(0.5234884f, -0.07719659f),
                            new Vector2(0.5924678f, 0),
                            new Vector2(0.7998599f, -0.0257149f),
                            new Vector2(0.7962418f, 0),
                            new Vector2(0.6058617f, 0),
                            new Vector2(0.5602508f, 0),
                            new Vector2(0.6424991f, 0),
                            new Vector2(0.6497322f, 0),
                            new Vector2(0.5491664f, -0.742268f),
                            new Vector2(0.5405772f, -0.3649045f),
                            new Vector2(0.5578618f, -0.8421474f),
                            new Vector2(0.558104f, -0.3250237f),
                            new Vector2(0.5493414f, -0.8354886f),
                            new Vector2(0.5688329f, -0.4450455f),
                            new Vector2(0.5497262f, -0.675361f),
                            new Vector2(0.5688329f, -0.3639083f),
                            new Vector2(0.285259f, 0),
                            new Vector2(0.2675261f, 0),
                            new Vector2(0.3884169f, 0),
                            new Vector2(0.3713144f, 0),
                            new Vector2(0.3010883f, -0.6469844f),
                            new Vector2(0.6829998f, -0.5242937f),
                            new Vector2(0.6393334f, -0.6469844f),
                            new Vector2(0.6259928f, -0.6469844f),
                            new Vector2(0.4391377f, 0),
                            new Vector2(0.4394747f, 0),
                            new Vector2(0.29234f, -0.5572742f),
                            new Vector2(0.7214338f, 0),
                            new Vector2(0.4916461f, 0),
                            new Vector2(0.3605213f, -0.001526866f),
                            new Vector2(0.5334466f, -0.5198563f),
                            new Vector2(0.6181761f, -0.3588411f),
                            new Vector2(0.5436226f, -0.7992503f),
                            new Vector2(0.5106729f, -0.725169f),
                            new Vector2(0.5106729f, -0.725169f),
                            new Vector2(0.5106729f, -0.725169f),
                            new Vector2(0.5106728f, -0.7244467f),
                            new Vector2(0.5102359f, 0),
                            new Vector2(0.510236f, -0.0003589771f),
                            new Vector2(0.510236f, -0.2142214f),
                            new Vector2(0.5106728f, -0.371914f),
                            new Vector2(0.3467451f, 0),
                            new Vector2(0.4136759f, -0.001776665f),
                            new Vector2(0.6386614f, 0),
                            new Vector2(0.5891146f, 0),
                            new Vector2(0.285259f, 0),
                            new Vector2(0.2675261f, 0),
                            new Vector2(0.3606665f, 0),
                            new Vector2(0.3615268f, 0),
                            new Vector2(0.4704561f, 0),
                            new Vector2(0.347643f, 0),
                            new Vector2(0.3323885f, 0),
                            new Vector2(0.6115884f, 0),
                            new Vector2(0.6674702f, 0),
                            new Vector2(0.4990468f, 0)
                            };

        public override Type ModuleSaveType => typeof(NinjaAestheticSave);

        public NinjaAestheticSave Save => (NinjaAestheticSave)ModuleSave;

        Dictionary<string, Sprite> customNinjaSprites;

        string customNinjaPrefix;

        bool HUDhidden;

        bool changeHUDHide;

        float rainbowSpeedMultiplier = 1;

        HSBColor rainbowColor = HSBColor.FromColor(Color.white);

        public override void Load() {
            switchSkinButton = Courier.UI.RegisterMultipleModOptionButton(() => "Switch Skin", null, ChangeSkin, GetSkinIndex, GetTextForSkinIndex);
            rainbowModeButton = Courier.UI.RegisterToggleModOptionButton(() => "Rainbow Mode", OnRainbowModeToggle, (b) => Save.rainbowMode);
            rainbowSpeedButton = Courier.UI.RegisterTextEntryModOptionButton(() => "Rainbow Speed", OnRainbowSpeedEntry, 3, null, () => rainbowSpeedMultiplier.ToString(), TextEntryButtonInfo.CharsetFlags.Dot | TextEntryButtonInfo.CharsetFlags.Number);
            hideHUDButton = Courier.UI.RegisterToggleModOptionButton(() => "Hide HUD", OnHUDToggle, (b) => HUDhidden);
            // TODO enable/disable custom skin
            switchSkinButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;

            customNinjaSprites = new Dictionary<string, Sprite>();
            string customSpritesDir = Path.Combine(Courier.ModsFolder, "CustomNinja");
            if (!Directory.Exists(customSpritesDir)) {
                Directory.CreateDirectory(customSpritesDir);
            } else {
                try {
                    string[] textures = Directory.GetFiles(customSpritesDir);
                    foreach (string t in textures.Where((s) => s.EndsWith(".png", StringComparison.InvariantCulture) && Path.GetFileNameWithoutExtension(s).Contains("_"))) {
                        string fName = Path.GetFileNameWithoutExtension(t);
                        customNinjaPrefix = fName.Substring(0, fName.LastIndexOf('_'));
                        string spriteIndex = fName.Split('_').Last();
                        Vector2 pivot;
                        if (int.TryParse(spriteIndex, out int index) && index >= 0 && index < pivots.Length) {
                            pivot = pivots[index];
                        } else {
                            pivot = new Vector2(.5f, .5f);
                        }
                        customNinjaSprites[fName] = ResourceHelper.GetSpriteFromFile(t, new SpriteParams { pixelsPerUnit = 20, meshType = SpriteMeshType.Tight, pivot = pivot });
                        customNinjaSprites[fName].texture.wrapMode = TextureWrapMode.Clamp;
                        customNinjaSprites[fName].texture.filterMode = FilterMode.Point;
                    }
                } catch (Exception e) {
                    CourierLogger.Log(LogType.Exception, "CustomNinja", "Error loading custom sprites");
                    e.LogDetailed("CustomNinja");
                }
            }

            Courier.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.PlayerController.LateUpdate += PlayerController_LateUpdate;
        }

        void PlayerController_LateUpdate(On.PlayerController.orig_LateUpdate orig, PlayerController self) {
            orig(self);
            /*string currSpriteName = self.spriteRenderer.sprite_8.sprite.name;
            string currSpriteIndex = currSpriteName.Split('_').Last();
            //Console.WriteLine("OG: " + GameObjectSerializer.SerializeObject(self.spriteRenderer.sprite_8.sprite, 0));
            //Console.WriteLine("new: " + GameObjectSerializer.SerializeObject(customNinjaSprites[customNinjaPrefix + "_" + currSpriteIndex], 0));
            if (customNinjaSprites.ContainsKey(customNinjaPrefix + "_" + currSpriteIndex))
                self.spriteRenderer.sprite_8.sprite = customNinjaSprites[customNinjaPrefix + "_" + currSpriteIndex];*/
        }

        void PlayerController_OnUpdate(PlayerController obj) {
            if (Save.rainbowMode) {
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

            InGameHud hud;
            if(changeHUDHide && (hud = Manager<UIManager>.Instance.GetView<InGameHud>()) != null) {
                if (HUDhidden) {
                    hud.HideHud();
                } else {
                    hud.ShowHud();
                }
                changeHUDHide = false;
            }
        }

        bool OnRainbowSpeedEntry(string entry) {
            if (float.TryParse(entry, out float res))
                rainbowSpeedMultiplier = res;
            return true;
        }

        void OnRainbowModeToggle() {
            Save.rainbowMode = !Save.rainbowMode;
            rainbowColor = HSBColor.FromColor(Color.white);
            rainbowModeButton.UpdateStateText();
            CourierLogger.Log("NinjaAesthetic", "Rainbow Mode: " + Save.rainbowMode);
        }

        void OnHUDToggle() {
            HUDhidden = !HUDhidden;
            changeHUDHide = true;
            hideHUDButton.UpdateStateText();
            CourierLogger.Log("NinjaAesthetic", "Hide HUD: " + HUDhidden);
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
