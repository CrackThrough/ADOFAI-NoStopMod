﻿using NoStopMod.InputFixer.HitIgnore;
using NoStopMod.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityModManagerNet;
using NoStopMod.InputFixer.SyncFixer;

namespace NoStopMod.InputFixer
{
    class InputFixerManager
    {
        public static InputFixerSettings settings;
        
        private static Thread thread;
        public static Queue<Tuple<long, List<KeyCode>>> keyQueue = new Queue<Tuple<long, List<KeyCode>>>();
        

        //public static long offsetTick;
        public static long currPressTick;

        public static bool jumpToOtherClass = false;
        public static bool editInputLimit = false;

        private static bool[] mask;
        
        public static void Init()
        {
            NoStopMod.onToggleListener.Add(UpdateEnableAsync);
            NoStopMod.onGUIListener.Add(OnGUI);
            NoStopMod.onApplicationQuitListener.Add(_ => Stop());
            Settings.settingsLoadListener.Add(_ => UpdateEnableAsync(settings.enableAsync));

            settings = new InputFixerSettings();
            Settings.settings.Add(settings);
            
            mask = Enumerable.Repeat(false, 1024).ToArray();

            HitIgnoreManager.Init();
            SyncFixerManager.Init();
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            //GUILayout.BeginVertical("Input");
            SimpleGUI.Toggle(ref settings.enableAsync, "Toggle Input Asynchronously", UpdateEnableAsync);
            //GUILayout.EndVertical(); 
        }

        private static void UpdateEnableAsync(bool value)
        {
            if (value)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public static void Start()
        {
            Stop();
            if (settings.enableAsync) {
                thread = new Thread(Run);
                thread.Start();
            }
        }

        public static void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            keyQueue.Clear();
        }

        private static bool GetKeyDown(int idx)
        {
            if (mask[idx])
            {
                if (!Input.GetKey((KeyCode)idx))
                {
                    mask[idx] = false;
                }
            }
            else
            {
                if (Input.GetKey((KeyCode)idx))
                {
                    mask[idx] = true;
                    return true;
                }
            }
            return false;
        }

        private static void Run()
        {
            long prevTick, currTick;
            prevTick = DateTime.Now.Ticks;
            while (settings.enableAsync)
            {
                currTick = DateTime.Now.Ticks;
                if (currTick > prevTick)
                {
                    prevTick = currTick;
                    UpdateKeyQueue(currTick);
                }
            }
        }

        public static void UpdateKeyQueue(long currTick)
        {
            List<KeyCode> keyCodes = getPressedKeys();
            if (keyCodes.Any())
            {
                keyQueue.Enqueue(new Tuple<long, List<KeyCode>>(currTick, keyCodes));
            }
        }

        private static List<KeyCode> getPressedKeys()
        {
            List<KeyCode> keyCodes = new List<KeyCode>();

            for (int i = 0; i < 320; i++)
            {
                if (GetKeyDown(i))
                {
                    keyCodes.Add((KeyCode)i);
                }
            }

            for (int i = 323; i <= 329; i++)
            {
                if (GetKeyDown(i))
                {
                    keyCodes.Add((KeyCode)i);
                }
            }

            return keyCodes;
        }

        public static double getAngle(scrPlanet __instance, double ___snappedLastAngle, long nowTick)
        {
            return ___snappedLastAngle + (SyncFixerManager.newScrConductor.getSongPosition(__instance.conductor, nowTick) - __instance.conductor.lastHit) / __instance.conductor.crotchet
                * 3.141592653598793238 * __instance.controller.speed * (double)(__instance.controller.isCW ? 1 : -1);
        }

        
        
    }
}