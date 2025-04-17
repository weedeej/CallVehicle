using MelonLoader;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using System.Collections;
using UnityEngine;

namespace CallVehicle.VehicleController
{
    public class VehicleControl
    {
        LandVehicle targetVehicle;
        Player player;
        List<LandVehicle> ownedVehicles;

        public VehicleControl(string id)
        {
            player = Player.Local;
            ownedVehicles = NetworkSingleton<VehicleManager>.Instance.PlayerOwnedVehicles.Where((veh) => veh.IsOwner).ToList();
            if (ownedVehicles.Count > 0)
                targetVehicle = ownedVehicles.Find((veh) => veh.SaveFolderName == id);
        }

        public VehicleControl()
        {
            player = Player.Local;
            ownedVehicles = NetworkSingleton<VehicleManager>.Instance.PlayerOwnedVehicles.Where((veh) => veh.IsOwner).ToList();
        }

        private IEnumerator _StartVehiclePOIUpdater()
        {
            while (targetVehicle.Agent.AutoDriving) {
                targetVehicle.POI.UpdatePosition();
                yield return new WaitForSecondsRealtime(2f);
            }

        }
    }
}
