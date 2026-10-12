using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


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

            if (NetworkServer.active)
            {
                Reveal();
            }            
        }

        private void Toogle()
        {
            revealed.Value = !revealed.Value;         
        }

        void Reveal()
        {
            List<IInteractable> instancesList = InterfaceInstanceTracker<IInteractable>.GetInstancesList();

            for (int i = 0; i < instancesList.Count; i++)
            {
                IInteractable interactable = instancesList[i];

                MonoBehaviour monoBehaviour = interactable as MonoBehaviour;

                GameObject interactableObj = monoBehaviour.gameObject;

                if (interactable.ShouldShowOnScanner())
                {
                    if (revealed.Value)
                    {
                        if (!interactableObj.GetComponent<ChestRevealer.RevealedObject>())
                        {
                            ChestRevealer.RevealedObject revealedObject;
                            revealedObject = interactableObj.AddComponent<ChestRevealer.RevealedObject>();

                            revealedObject.lifetime = Mathf.Infinity;
                        }
                    }
                    else
                    {
                        var revealedObjects = interactableObj.GetComponentsInChildren<ChestRevealer.RevealedObject>();

                        foreach (var revealedObject in revealedObjects)
                        {
                            Destroy(revealedObject);
                        }
                    }
                }
                else
                {
                    if (revealed.Value)
                    {
                        var revealedObjects = interactableObj.GetComponentsInChildren<ChestRevealer.RevealedObject>();

                        foreach (var revealedObject in revealedObjects)
                        {
                            Destroy(revealedObject);
                        }
                    }
                }
            }
        }
    }
}
