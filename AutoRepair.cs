using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace EquimentsAR {
   [BepInPlugin("com.luckyyy.EquipmentsAutoRepair", "In-Raid Equipments Auto Repair(EAR)", "1.0.0")]
   public class Plugin : BaseUnityPlugin {
      
      // Config Entries
      public static ConfigEntry<bool> ModEnabled;
      public static ConfigEntry<float> RepairInterval;
      public static ConfigEntry<float> RepairAmount;

      // Static Helper
      public static GameWorld EARGameWorld => Singleton<GameWorld>.Instantiated ? Singleton<GameWorld>.Instance : null;
      public static Player EARPlayer => EARGameWorld?.MainPlayer;

      private float _timer = 0.0f;

      private void Awake() {
         ModEnabled = Config.Bind("1. General", "Enable Mod", true, "Enable In-Raid EAR.");

         RepairInterval = Config.Bind("1. General", "Repair Interval", 2.5f,
            new ConfigDescription("Interval between each repair.",
            new AcceptableValueRange<float>(1.0f, 60.0f)));

         RepairAmount = Config.Bind("1. General", "Repair Amount", 2.0f,
            new ConfigDescription("How much EAR repair durability each tick of Repair Interval.",
            new AcceptableValueRange<float>(0.1f, 10.0f)));

         // 1. Log when the mod is successfully loaded by BepInEx
         Logger.LogInfo($"[EAR] Equipments Auto Repair Client Loaded! Mod Enabled: {ModEnabled.Value}");
      }

      private void Update() {
         if(!ModEnabled.Value) return;

         if(!EARGameWorld || EARPlayer == null) return;

         _timer += Time.deltaTime;

         if(_timer >= RepairInterval.Value) {
            Logger.LogInfo($"[EAR] Repair timer trigggered ({RepairInterval.Value}s). Checking gear...");

            RepairPlayerGear(EARPlayer);
            _timer = 0.0f;
         }
      }

      private void RepairPlayerGear(Player player) {
         bool repairedAnything = false;

         foreach (var item in player.Inventory.Equipment.GetAllItems()) {
            var repairable = item.GetItemComponent<RepairableComponent>();

            if (repairable != null && repairable.Durability < repairable.MaxDurability) {
               float oldDurability = repairable.Durability;

               repairable.Durability = Mathf.Min(
                  repairable.Durability + RepairAmount.Value,
                  repairable.MaxDurability);

               Logger.LogInfo($"[EAR] Repaired item {item.Id} from {oldDurability} to {repairable.Durability}");
               repairedAnything = true;
            }
         }

         if(repairedAnything) {
            Logger.LogInfo("[EAR] Repair cycle complete.");
         }
      }
   }
}