using CallVehicle.VehicleController;
using MelonLoader;
using System.Collections;
using UnityEngine;

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
        }
    }
}