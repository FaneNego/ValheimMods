﻿using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomTextures
{
    public partial class BepInExPlugin
    {

        [HarmonyPatch(typeof(FejdStartup), "SetupObjectDB")]
        static class FejdStartup_SetupObjectDB_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value)
                    return;
                outputDump.Clear();

                Dbgl($"SetupObjectDB postfix");

                ReplaceObjectDBTextures();
            }
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        static class ZNetScene_Awake_Patch
        {
            static void Postfix(ZNetScene __instance, Dictionary<int, GameObject> ___m_namedPrefabs)
            {
                Dbgl($"ZNetScene awake");

                logDump.Clear();

                Dbgl($"Checking {___m_namedPrefabs.Count} prefabs");
                foreach (GameObject go in ___m_namedPrefabs.Values)
                {
                    LoadOneTexture(go, go.name, "object");
                }

                if (logDump.Any())
                    Dbgl("\n" + string.Join("\n", logDump));

                if (dumpSceneTextures.Value)
                {
                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomTextures", "scene_dump.txt");
                    Dbgl($"Writing {path}");
                    File.WriteAllLines(path, outputDump);
                }
            }
        }

        [HarmonyPatch(typeof(ClutterSystem), "Awake")]
        static class ClutterSystem_Awake_Patch
        {
            static void Postfix(ClutterSystem __instance)
            {
                Dbgl($"Clutter system awake");

                logDump.Clear();

                Dbgl($"Checking {__instance.m_clutter.Count} clutters");
                foreach (ClutterSystem.Clutter clutter in __instance.m_clutter)
                {
                    LoadOneTexture(clutter.m_prefab, clutter.m_prefab.name, "object");
                }

                if (logDump.Any())
                    Dbgl("\n" + string.Join("\n", logDump));

            }
        }

        [HarmonyPatch(typeof(VisEquipment), "Awake")]
        static class VisEquipment_Awake_Patch
        {
            static void Postfix(VisEquipment __instance)
            {
                for (int i = 0; i < __instance.m_models.Length; i++)
                {
                    foreach(string property in __instance.m_models[i].m_baseMaterial.GetTexturePropertyNames())
                    {

                        if (HasCustomTexture($"player_model_{i}{property}"))
                        {
                            __instance.m_models[i].m_baseMaterial.SetTexture(property, LoadTexture($"player_model_{i}{property}", __instance.m_models[i].m_baseMaterial.GetTexture(property)));
                            Dbgl($"set player_model_{i}_texture custom texture.");
                        }
                        else if (property == "_MainTex" && HasCustomTexture($"player_model_{i}_texture")) // legacy
                        {
                            __instance.m_models[i].m_baseMaterial.SetTexture(property, LoadTexture($"player_model_{i}_texture", __instance.m_models[i].m_baseMaterial.GetTexture(property)));
                        }
                        else if (property == "_SkinBumpMap" && HasCustomTexture($"player_model_{i}_bump")) // legacy
                        {
                            __instance.m_models[i].m_baseMaterial.SetTexture(property, LoadTexture($"player_model_{i}_bump", __instance.m_models[i].m_baseMaterial.GetTexture(property)));
                        }
                    }
                }
            }
        }
    }
}