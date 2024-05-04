// Project:         BetonyRestored for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Cliffworms
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Cliffworms

using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Weather;

namespace BetonyRestored
{
    public class BetonyRestoredMod : MonoBehaviour
    {
        public const byte npcFlagHideDay        = 1;    // 0b_0000_0001
        public const byte npcFlagHideNight      = 2;    // 0b_0000_0010
        public const byte npcFlagHideWeather    = 4;    // 0b_0000_0100

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<BetonyRestoredMod>();
        }

        void Awake()
        {
            InitMod();
            mod.IsReady = true;
        }

        public static void InitMod()
        {
            Debug.Log("Begin mod init: BetonyRestored");

            PlayerGPS.OnEnterLocationRect += UpdateExteriorNPCs_OnEnterLocationRect;
            WeatherManager.OnWeatherChange += UpdateExteriorNPCs_OnWeatherChange;
            WorldTime.OnDawn += UpdateExteriorNPCs;
            WorldTime.OnDusk += UpdateExteriorNPCs;

            if (!RegisterFactionIds())
            {
                Debug.LogWarning("BetonyRestored: Failed to register faction ids.");
            }

            Debug.Log("Finished mod init: BetonyRestored");
        }

        private static bool RegisterFactionIds()
        {
            return FactionFile.RegisterCustomFaction(1432, new FactionFile.FactionData()
            {
                id = 1432,
                parent = 203,
                type = 4,
                name = "Lord Mogref",
                summon = -1,
                region = 20,
                power = 18,
                face = 405,
                race = 3,
                sgroup = 3,
                ggroup = -1,
                children = null
            });
        }

        static void UpdateExteriorNPCs_OnEnterLocationRect(DFLocation location)
        {
            UpdateExteriorNPCs();
        }

        static void UpdateExteriorNPCs_OnWeatherChange(WeatherType weather)
        {
            UpdateExteriorNPCs();
        }

        static void UpdateExteriorNPCs()
        {
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            bool day = DaggerfallUnity.Instance.WorldTime.Now.IsDay;
            bool raining = GameManager.Instance.WeatherManager.IsRaining;

            if (playerGPS.IsPlayerInTown(false, true))
            {
                StaticNPC staticNpc = null;
                Billboard[] dfBillboards = playerEnterExit.ExteriorParent.GetComponentsInChildren<Billboard>(true);
                foreach (Billboard billboard in dfBillboards)
                {
                    staticNpc = billboard.GetComponent<StaticNPC>();
                    if (staticNpc != null && staticNpc.Data.factionID != 0)
                    {
                        // Show/Hide depending on NPC flag data.

                        bool hidingFromRain = (staticNpc.Data.flags & npcFlagHideWeather) != 0 && raining;

                        if ((staticNpc.Data.flags & npcFlagHideDay) != 0)
                        {
                            billboard.gameObject.SetActive(!day && !hidingFromRain);
                        }
                        else if ((staticNpc.Data.flags & npcFlagHideNight) != 0)
                        {
                            billboard.gameObject.SetActive(day && !hidingFromRain);
                        }
                        else
                        {
                            billboard.gameObject.SetActive(!hidingFromRain);
                        }

                        //Debug.LogFormat("Updated NPC sprite {0} {1} flags: {2}  enabled: {3}", billboard.Summary.Archive, billboard.Summary.Record, staticNpc.Data.flags, billboard.gameObject.activeInHierarchy);
                    }
                }
            }
        }
    }
}