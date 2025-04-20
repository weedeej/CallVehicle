using CallVehicle.Phone;
using CallVehicle.Utilities;
using FluffyUnderware.Curvy.Generator;
using HarmonyLib;
using MelonLoader;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone;
using ScheduleOne.Vehicles.AI;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(CallVehicle.Core), "CallVehicle", "1.0.0", "Dixie", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CallVehicle
{
    public class Core : MelonMod
    {
        private float _sceneLoadDelay = 5f;

        public override void OnInitializeMelon()
        {
            Preferences.Initialize();
            LoggerInstance.Msg("CallVehicle mod initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                MelonCoroutines.Start(SceneCourotine());
                return;
            }
        }

        private IEnumerator SceneCourotine()
        {
            LoggerInstance.Msg($"Waiting for {_sceneLoadDelay} before initial coroutine.");
            yield return new WaitForSeconds(_sceneLoadDelay);
            InitAppAndUI();
        }

        private void InitAppAndUI()
        {
            if (PlayerSingleton<GenericApp>.InstanceExists)
            {
                MelonLogger.Msg("GenericApp instance already exists. Skipping creation.");
                return;
            }
            MelonLogger.Msg("AppsCanvas.Awake Postfix: Creating GenericApp...");

            // 1. Create a new GameObject to hold the GenericApp component
            // Naming it helps with debugging in the hierarchy if possible.
            GameObject appGameObject = new GameObject("GenericApp_Instance");

            // 2. Parent the new GameObject to the AppsCanvas GameObject
            // This keeps the hierarchy organized.
            appGameObject.transform.SetParent(AppsCanvas.Instance.transform, false); // Use worldPositionStays = false

            // 3. Add the GenericApp component to the new GameObject
            // This will automatically trigger GenericApp's Awake() and later Start() methods
            // because we're adding it to an active GameObject in the scene.
            GenericApp genericAppInstance = appGameObject.AddComponent<GenericApp>();

            // 4. Ensure the GameObject is active (it should be by default)
            appGameObject.SetActive(true);

            // Note: We don't manually call Awake or Start. Unity handles this when
            // AddComponent is called on an active GameObject.
            // The App<T>.OnStartClient will be called by Unity's lifecycle, 
            // which adds the app to the list and generates the icon.

            MelonLogger.Msg("GenericApp created and added to AppsCanvas.");
        }

        public void HandleVehicleCall(string vehicleId)
        {
            // Logic to handle vehicle call
        }
    }
}