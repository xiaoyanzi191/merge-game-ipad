using System;
using System.IO;
using Core.GridPawns;
using Core.GridPawns.Enum;
using Core.Helpers;
using UnityEngine;

namespace Core.GridSerialization
{
    public static class GridSerializer
    {
        private static readonly string PersistentPath = Path.Combine(Application.persistentDataPath, "grid_data.json");

        public static GridInfo SerializeToGridInfo()
        {
            string jsonString = BrowserSaveStorage.ReadFile(PersistentPath);
            if (jsonString == null) return null;
            var gridJson = JsonUtility.FromJson<GridJson>(jsonString);
            var (gridPawnTypes, gridPawnLevels, gridPawnCapacities) = ProcessGridJson(gridJson);
            return new GridInfo(gridPawnTypes, gridPawnLevels, gridPawnCapacities);
        }

        public static void SaveGrid(GridPawn[,] grid)
        {
            try
            {
                var columnCount = grid.GetLength(0);
                var rowCount = grid.GetLength(1);
                JsonPawn[,] jsonPawn = new JsonPawn[columnCount, rowCount];
                for (int i = 0; i < columnCount; i++)
                {
                    for (int j = 0; j < rowCount; j++)
                    {
                        var type = grid[i, j] is null ? ApplianceType.None : grid[i, j].Type;
                        var level = grid[i, j] is null ? -1 : grid[i, j].Level;
                        var capacity = grid[i, j] is Producer producer ? producer.Capacity : -1;
                        JsonPawn pawn = new JsonPawn
                        {
                            pawn_type = JsonEnumConverter.ConvertPawnTypeToJson(type).ToString(),
                            level = level,
                            capacity = capacity
                        };
                        jsonPawn[i, j] = pawn;
                    }
                }

                var gridJson = ConvertToGridJson(columnCount, rowCount, jsonPawn);
                string json = JsonUtility.ToJson(gridJson, true);
                BrowserSaveStorage.WriteFile(PersistentPath, json);
                Debug.Log("JSON saved to: " + PersistentPath);
            }
            catch (Exception e)
            {
                Debug.Log("JSON error:" + e);
                throw;
            }
        }

        public static (Enum[,] gridPawnTypes, int[,] gridPawnLevels, int[,] gridPawnCapacities) ProcessGridJson(
            GridJson gridJson)
        {
            var gridPawnTypes = new Enum[gridJson.grid_height, gridJson.grid_width];
            var gridPawnLevels = new int[gridJson.grid_height, gridJson.grid_width];
            var gridPawnCapacities = new int[gridJson.grid_height, gridJson.grid_width];

            int gridIndex = 0;
            for (int i = 0; i < gridJson.grid_height; i++)
            for (int j = 0; j < gridJson.grid_width; j++)
            {
                gridPawnCapacities[i, j] = gridJson.grid[gridIndex].capacity;
                gridPawnLevels[i, j] = gridJson.grid[gridIndex].level;
                gridPawnTypes[i, j] = JsonEnumConverter.ConvertJsonToPawnType(gridJson.grid[gridIndex].pawn_type);
                gridIndex++;
            }

            return (gridPawnTypes, gridPawnLevels, gridPawnCapacities);
        }


        public static GridJson ConvertToGridJson(int gridWidth, int gridHeight, JsonPawn[,] pawns)
        {
            GridJson gridJson = new GridJson
            {
                grid_width = gridWidth,
                grid_height = gridHeight,
                grid = new JsonPawn[gridWidth * gridHeight],
            };

            for (int x = 0; x < gridHeight; x++)
            {
                for (int y = 0; y < gridWidth; y++)
                {
                    int index = (x * gridWidth) + y; // Correct indexing from 0 to (gridHeight * gridWidth - 1)
                    gridJson.grid[index] = pawns[x, y];
                }
            }

            return gridJson;
        }

        public static JsonPawn[,] ConvertToJsonPawn(Enum[,] gridInfoGridPawnTypes, int[,] gridInfoGridPawnLevels)
        {
            JsonPawn[,] jsonPawns =
                new JsonPawn[gridInfoGridPawnTypes.GetLength(0), gridInfoGridPawnTypes.GetLength(1)];
            for (int i = 0; i < jsonPawns.GetLength(0); i++)
            for (int j = 0; j < jsonPawns.GetLength(1); j++)
            {
                JsonPawn jsonPawn = new JsonPawn
                {
                    pawn_type = JsonEnumConverter.ConvertPawnTypeToJson(gridInfoGridPawnTypes[i, j]).ToString(),
                    level = gridInfoGridPawnLevels[i, j]
                };
                jsonPawns[i, j] = jsonPawn;
            }

            return jsonPawns;
        }
    }
}
