using CallVehicle.Phone;
using CallVehicle.Utilities;
using MelonLoader;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Money;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.UI.Phone;
using System.Collections;
using UnityEngine;
using static ScheduleOne.Dialogue.DialogueController;

[assembly: MelonInfo(typeof(CallVehicle.Core), "CallVehicle", "1.0.0", "Dixie")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CallVehicle
{
    public class Core : MelonMod
    {

        public override void OnInitializeMelon()
        {
            Preferences.Setup(LoggerInstance);
            LoggerInstance.Msg("CallVehicle mod initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (sceneName != "Main") return;
            MelonCoroutines.Start(AppUICoro());
            return;
        }

        public IEnumerator AppUICoro()
        {
            while(true)
            {
                yield return new WaitForSecondsRealtime(3f);

                CallVehicleAppSaveData? app = ModUtilities.GetLatestSaveData();
                CallVehicleAppSaveData appData = app == null ? ModUtilities.SaveModData(new CallVehicleAppSaveData { }) : app.Value;
                if (appData.isPurchased)
                {
                    InitAppAndUI();
                    break;
                }
                int appPrice = Preferences.GetPrefValue<int>(PreferenceFields.AppPrice);
                MoneyManager moneyMan = NetworkSingleton<MoneyManager>.Instance;
                Marco marco = GameObject.FindObjectOfType<Marco>();

                string choiceText = $"Purchase Call Vehicle App ${appPrice}";

                DialogueChoice marcoChoice = marco.dialogueHandler.GetComponent<DialogueController>().Choices.Find(x => x.ChoiceText == choiceText);
                DialogueController.DialogueChoice choice3 = marcoChoice ?? new DialogueController.DialogueChoice
                {
                    ChoiceText = choiceText,
                    Enabled = true,
                };
                if (moneyMan.cashBalance < appPrice)
                {
                    if (marcoChoice != null)
                    {
                        marco.dialogueHandler.GetComponent<DialogueController>().Choices.Remove(marcoChoice);
                    }
                    continue;
                }
                if (marcoChoice != null) continue;

                choice3.onChoosen.AddListener(delegate
                {
                    ModUtilities.SaveModData(new CallVehicleAppSaveData { isPurchased = true });
                    moneyMan.ChangeCashBalance(-appPrice, true, true);
                    marco.dialogueHandler.GetComponent<DialogueController>().Choices.Remove(choice3);
                    marco.SendTextMessage("Hey boss, Thank you for the purchase. If you need your vehicle FAST, use the app on your phone.");
                    // Add app
                    InitAppAndUI();
                });
                marco.dialogueHandler.GetComponent<DialogueController>().Choices.Add(choice3);
                MelonLogger.Msg("CallVehicle: Postfix - Added purchase option to dialogue.");
                break;
            }
        }
        private static void InitAppAndUI()
        {
            if (PlayerSingleton<CallVehicleApp>.InstanceExists) return;
            GameObject appGameObject = new GameObject("CallVehicleApp_Instance");
            appGameObject.transform.SetParent(AppsCanvas.Instance.transform, false);
            CallVehicleApp callVehicleAppInstance = appGameObject.AddComponent<CallVehicleApp>();
            appGameObject.SetActive(true);
        }
    }
}