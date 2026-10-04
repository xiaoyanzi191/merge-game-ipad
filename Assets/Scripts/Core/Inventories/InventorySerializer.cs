using System;
using System.Collections.Generic;
using System.IO;
using Core.GridSerialization;
using Core.Helpers;
using UnityEngine;

namespace Core.Inventories
{
    public static class InventorySerializer
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "inventory.json");


        //  Save inventory to JSON
        public static void SaveInventory(List<InventoryPawn> items)
        {
            var jsonItems = ConvertToInventoryJson(items);
            string json = JsonUtility.ToJson(new InventoryData(jsonItems), true);
            BrowserSaveStorage.WriteFile(SavePath, json);
            Debug.Log($"Inventory saved to: {SavePath}");
        }

        private static List<InventoryPawnJson> ConvertToInventoryJson(List<InventoryPawn> items)
        {
            List<InventoryPawnJson> jsonPawns = new List<InventoryPawnJson>(items.Count);
            foreach (var inventoryPawn in items)
            {
                //Debug.Log(inventoryPawn);
                var stringType = JsonEnumConverter.ConvertPawnTypeToJson(inventoryPawn.Type);
                InventoryPawnJson jsonPawn = new InventoryPawnJson
                {
                    id = inventoryPawn.ID,
                    level = inventoryPawn.Level,
                    type = stringType.ToString()
                };
                jsonPawns.Add(jsonPawn);
            }

            return jsonPawns;
        }

        //  Load inventory from JSON
        public static List<InventoryPawn> LoadInventory()
        {
            string json = BrowserSaveStorage.ReadFile(SavePath);
            if (json == null)
            {
                Debug.LogWarning("No inventory save file found!");
                return new List<InventoryPawn>();
            }

            InventoryData inventoryData = JsonUtility.FromJson<InventoryData>(json);
            var items = ConvertToInventoryPawn(inventoryData.Items);
            return items;
        }

        private static List<InventoryPawn> ConvertToInventoryPawn(List<InventoryPawnJson> inventoryDataItems)
        {
            List<InventoryPawn> inventoryPawns = new List<InventoryPawn>(inventoryDataItems.Count);
            foreach (var dataItem in inventoryDataItems)
            {
                var type = JsonEnumConverter.ConvertJsonToPawnType(dataItem.type);
                InventoryPawn newPawn = new InventoryPawn(type, dataItem.level);
                inventoryPawns.Add(newPawn);
            }

            return inventoryPawns;
        }
    }

    // ✅ Serializable wrapper for storing list of inventory items
    [Serializable]
    public class InventoryData
    {
        public List<InventoryPawnJson> Items;

        public InventoryData(List<InventoryPawnJson> items)
        {
            Items = items;
        }
    }
}
