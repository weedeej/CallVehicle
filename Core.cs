using CallVehicle.Phone;
using MelonLoader;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(CallVehicle.Core), "CallVehicle", "1.0.0", "Dixie")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CallVehicle
{
    public class Core : MelonMod
    {
        private float _sceneLoadDelay = 5f;

        public override void OnInitializeMelon()
        {
            Preferences.Setup(LoggerInstance);
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
            if (PlayerSingleton<CallVehicleApp>.InstanceExists)
            {
                MelonLogger.Msg("CallVehicleApp instance already exists. Skipping creation.");
                return;
            }
            MelonLogger.Msg("AppsCanvas.Awake Postfix: Creating CallVehicleApp...");
            GameObject appGameObject = new GameObject("CallVehicleApp_Instance");
            appGameObject.transform.SetParent(AppsCanvas.Instance.transform, false);
            CallVehicleApp callVehicleAppInstance = appGameObject.AddComponent<CallVehicleApp>();
            appGameObject.SetActive(true);

            MelonLogger.Msg("CallVehicleApp created and added to AppsCanvas.");
        }

        public void HandleVehicleCall(string vehicleId)
        {
            // Logic to handle vehicle call
        }
    }
}