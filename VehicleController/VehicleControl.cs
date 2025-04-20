using MelonLoader;
using Pathfinding.RVO.Sampled;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using System.Collections;
using UnityEngine;
using static ScheduleOne.PlayerScripts.Player;

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
            NPC npc = NPCManager.NPCRegistry.Find(n => n.FirstName == "Jeff");
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
            npc.EnterVehicle(player.Connection, targetVehicle);
            vehicleAgent.Flags.StuckDetection = true;
            targetVehicle.POI.AutoUpdatePosition = true;
            vehicleAgent.Navigate(playerPos, new NavigationSettings { endAtRoad = true, teleportToGraphIfCalculationFails = true }, (result) => {
                npc.SendTextMessage($"Vehicle Arrived. I've deducted ${cost} from your online balance. Thank you for using my service.");
                vehicleAgent.Flags.ResetFlags();
                npc.ExitVehicle();
                targetVehicle.SetIsPlayerOwned(player.Connection, true); // Working
                targetVehicle = null;
                moneyManager.CreateOnlineTransaction("Chauffeur services", -cost, 1f, "Non-refundable");
                targetVehicle.OverrideMaxSteerAngle(targetVehicle.ActualMaxSteeringAngle);
                vehicleAgent.StopNavigating();
            });
            // send otw message with vehicle color and name
            npc.SendTextMessage($"Your {targetVehicle.VehicleName} is on the way.");
        }

        public void SetTargetVehicle(LandVehicle veh)
        {
            targetVehicle = veh;
        }
    }
}
