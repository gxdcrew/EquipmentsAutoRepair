using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using UnityEngine;

namespace EquimentsAR {
   [BepInPlugin("com.luckyyy.EquipmentsAutoRepair", "In-Raid Equipments Auto Repair(EAR)", "1.1.0")]
   public class Plugin : BaseUnityPlugin {

      // Config Entries
      public static ConfigEntry<bool> ModEnabled;
      public static ConfigEntry<bool> RepairWeapon;
      public static ConfigEntry<bool> RepairArmor;
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
         if (!ModEnabled.Value) return;

         if (!EARGameWorld || EARPlayer == null) return;

         _timer += Time.deltaTime;

         if (_timer >= RepairInterval.Value) {
            Logger.LogInfo($"[EAR] Repair timer trigggered ({RepairInterval.Value}s). Checking gear...");

            RepairPlayerGear(EARPlayer);
            _timer = 0.0f;
         }
      }

      private void RepairPlayerGear(Player player) {
         bool repairedAnything = false;
         var equipment = player.Inventory.Equipment;

         List<EquipmentSlot> slotsToRepair = new List<EquipmentSlot>();

         if (RepairWeapon.Value) {
            slotsToRepair.Add(EquipmentSlot.FirstPrimaryWeapon);
            slotsToRepair.Add(EquipmentSlot.SecondPrimaryWeapon);
            slotsToRepair.Add(EquipmentSlot.Holster);
         }

         if (RepairArmor.Value) {
            slotsToRepair.Add(EquipmentSlot.ArmorVest);
            slotsToRepair.Add(EquipmentSlot.TacticalVest);
            slotsToRepair.Add(EquipmentSlot.Headwear);
            slotsToRepair.Add(EquipmentSlot.FaceCover);
            slotsToRepair.Add(EquipmentSlot.Eyewear);
            slotsToRepair.Add(EquipmentSlot.Earpiece);
         }

         foreach (var slot in slotsToRepair) {
            var slotItem = equipment.GetSlot(slot).ContainedItem;

            if (slotItem == null) continue;

            foreach (var item in slotItem.GetAllItems()) {
               var repairable = item.GetItemComponent<RepairableComponent>();

               if (repairable != null && repairable.Durability < repairable.MaxDurability) {
                  float oldDurability = repairable.Durability;
                  float newDurability = Mathf.Min(repairable.Durability + RepairAmount.Value, repairable.MaxDurability);
                  repairable.Durability = newDurability;
                  Logger.LogInfo($"[EAR] Repaired item {item.Id} from {oldDurability} to {newDurability}");
                  repairedAnything = true;
               }
            }
         }

         if (repairedAnything) {
            Logger.LogInfo("[EAR] Repair cycle complete.");
         }
      }
   }
}