using UnityEngine;

namespace Core
{
    // The upstream project has no economy or energy gate. Keep this local wallet
    // independent of orders and production, and initialize it once per save.
    public static class LocalResources
    {
        public const int InitialCoins = 99999999;
        public static bool HasUnlimitedEnergy => true;
        public static int Coins => PlayerPrefs.GetInt("Sandbox.Coins", InitialCoins);

        public static void Initialize()
        {
            if (PlayerPrefs.HasKey("Sandbox.Coins")) return;
            PlayerPrefs.SetInt("Sandbox.Coins", InitialCoins);
            PlayerPrefs.Save();
        }
    }
}
