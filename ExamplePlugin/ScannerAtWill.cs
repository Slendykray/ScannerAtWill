using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


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
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            if (revealed.Value)
            {
                StartCoroutine(DelayedReveal(5f));
            }
        }
        private IEnumerator DelayedReveal(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (revealed.Value)
            {
                Revealed_SettingChanged(revealed, EventArgs.Empty);
            }
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

            foreach (var revealedObject in revealedObjects)
            {
                GameObject parentObj = revealedObject?.gameObject?.transform?.parent?.gameObject;

                if (parentObj != null)
                {
                    // 2. Check if the parent has ANY of the component types from the array
                    Type[] array = ChestRevealer.typesToCheck;
                    bool hasMatchingComponent = array.Any(type => parentObj.GetComponent(type) != null);

                    // 3. If none of the types exist on the parent, destroy the parent
                    if (!hasMatchingComponent)
                    {
                        GameObject.Destroy(parentObj);
                    }
                }
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

        private List<ChestRevealer.RevealedObject> revealedObjects = new List<ChestRevealer.RevealedObject>();
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

                            revealedObjects.Add(revealedObject);
                        }
                        else
                        {
                            //var revealedObjects = interactable.GetComponentsInChildren<ChestRevealer.RevealedObject>();

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
}
