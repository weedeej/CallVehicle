using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;

namespace CallVehicle.VehicleController
{
    public class VehicleControl
    {
        public LandVehicle targetVehicle;
        Player player;
        public List<LandVehicle> ownedVehicles;

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

        public void CallVehicle(string id, int cost)
        {
            // Scoped call
            LandVehicle targetVehicle = ownedVehicles.Find((veh) => veh.SaveFolderName == id);
            if (targetVehicle == null) return;
            NPC npc = NPCManager.NPCRegistry.Find(n => n.FirstName == "Marco");
            if (targetVehicle.Agent.AutoDriving)
            {
                npc.SendTextMessage($"Sorry boss, I'm still driving your {targetVehicle.VehicleName}.");
                return;
            }
            MoneyManager moneyManager = NetworkSingleton<MoneyManager>.Instance;
            if (moneyManager.onlineBalance < cost)
            {
                npc.SendTextMessage($"Sorry boss, You are poor as dirt. You need ${cost} for this service.");
                return;
            }
            Vector3 playerPos = player.Avatar.MiddleSpine.position;
            targetVehicle.OverrideMaxSteerAngle(90f);
            VehicleAgent vehicleAgent = targetVehicle.Agent;
            if (!vehicleAgent.IsOnVehicleGraph())
                vehicleAgent.Teleporter.MoveToGraph(true);
            vehicleAgent.StuckDistanceThreshold = 1f;
            vehicleAgent.StuckTimeThreshold = 5f;
            vehicleAgent.Flags.IgnoreTrafficLights = true;
            vehicleAgent.Flags.ObstacleMode = DriveFlags.EObstacleMode.IgnoreAll;
            targetVehicle.IsPlayerOwned = false;
            if (Preferences.GetPrefValue<bool>(PreferenceFields.BypassCheckpoints))
                npc.EnterVehicle(player.Connection, targetVehicle);
            vehicleAgent.Flags.StuckDetection = true;
            targetVehicle.POI.AutoUpdatePosition = true;
            vehicleAgent.Navigate(playerPos, new NavigationSettings { endAtRoad = true, teleportToGraphIfCalculationFails = true }, (result) => {
                vehicleAgent.StopNavigating();
                vehicleAgent.Flags.ResetFlags();
                if (Preferences.GetPrefValue<bool>(PreferenceFields.BypassCheckpoints))
                    npc.ExitVehicle();
                targetVehicle.OverrideMaxSteerAngle(targetVehicle.ActualMaxSteeringAngle);
                targetVehicle.SetIsPlayerOwned(player.Connection, true);
                targetVehicle.POI.AutoUpdatePosition = false;
                if (result == VehicleAgent.ENavigationResult.Failed)
                {
                    npc.SendTextMessage($"Sorry boss, I couldn't find a route to you. You won't be charged for this.");
                    return;
                }
                if (result != VehicleAgent.ENavigationResult.Complete) return;
                bool useCash = Preferences.GetPrefValue<bool>(PreferenceFields.UseCash);
                targetVehicle = null;
                if (useCash)
                {
                    moneyManager.ChangeCashBalance(-cost, true, true);
                    if (Preferences.GetPrefValue<bool>(PreferenceFields.BypassCheckpoints))
                    {
                        ItemInstance cash = moneyManager.GetCashInstance(cost);
                        npc.Inventory.InsertItem(cash, true);
                    }
                }
                else
                {
                    moneyManager.CreateOnlineTransaction("Chauffeur services", -cost, 1f, "Non-refundable");
                }
                string paymentMethod = useCash ? "cash" : "online balance";
                npc.SendTextMessage($"Vehicle Arrived. I've deducted ${cost} from your {paymentMethod}. Thank you for using my service.");
            });
            // send otw message with vehicle color and name
            npc.SendTextMessage($"Your ({targetVehicle.OwnedColor}) {targetVehicle.VehicleName} is on the way.");
        }

        public void SetTargetVehicle(LandVehicle veh)
        {
            targetVehicle = veh;
        }
    }
}
