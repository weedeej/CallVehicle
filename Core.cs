using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CallVehicle.Core), "CallVehicle", "1.0.0", "Dixie", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CallVehicle
{
    public class Core : MelonMod
    {

        public override void OnInitializeMelon()
        {
            // Initialize the mod
            MelonLogger.Msg("CallVehicle mod initialized.");
        }
    }
}