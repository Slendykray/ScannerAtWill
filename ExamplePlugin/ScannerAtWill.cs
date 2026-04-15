using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using UnityEngine;

namespace ScannerAtWill
{

    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ScannerAtWill : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Slendykray";
        public const string PluginName = "ScannerAtWill";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);
            Asset.Init();
            InitConfig();

            revealed.SettingChanged += Revealed_SettingChanged;
        }

        public static ConfigEntry<bool> revealed; 
        ConfigEntry<KeyboardShortcut> keyBind;

        private void InitConfig()
        {
            Sprite icon = Asset.mainBundle.LoadAsset<Sprite>("icon.png");
            ModSettingsManager.SetModIcon(icon);

            revealed = Config.Bind<bool>("Main", "Enabled", false, "enable ping all? hotkey will toggle this value");
            keyBind = Config.Bind("Main", "Hotkey", new KeyboardShortcut(KeyCode.None));

            ModSettingsManager.AddOption(new CheckBoxOption(revealed));
            ModSettingsManager.AddOption(new KeyBindOption(keyBind));

        }

        private void Update()
        {
            if (Input.GetKeyDown(keyBind.Value.MainKey))
            {
                Toogle();
            }
        }

        private void Revealed_SettingChanged(object sender, EventArgs e)
        {
            Reveal();
        }

        private void Toogle()
        {
            revealed.Value = !revealed.Value;
        }

        void Reveal()
        {
            Type[] array = ChestRevealer.typesToCheck;
            for (int i = 0; i < array.Length; i++)
            {
                foreach (MonoBehaviour monoBehaviour in InstanceTracker.FindInstancesEnumerable(array[i]))
                {
                    if (((IInteractable)monoBehaviour).ShouldShowOnScanner())
                    {
                        GameObject interactable = monoBehaviour.gameObject;

                        if (revealed.Value)
                        {              
                            ChestRevealer.RevealedObject revealedObject;
                            revealedObject = interactable.AddComponent<ChestRevealer.RevealedObject>();

                            revealedObject.lifetime = Mathf.Infinity;
                        }
                        else
                        {
                            if (interactable.GetComponent<ChestRevealer.RevealedObject>())
                                Destroy(interactable.GetComponent<ChestRevealer.RevealedObject>());
                        }
                    }
                }
            }
        }
    }
}
