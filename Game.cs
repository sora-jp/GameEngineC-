﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace GameEngine
{
    public static class External {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow); // DON'T Mess with me
    }

    public static class Game
    {
        public static bool IsRunning {get; private set;}
        static Thread worker;
        static Stopwatch timer;
        internal static List<ConsoleKeyInfo> keys;
        internal static IDisplay display;

        public static Position cameraPosition {
            get {
                return display.GetCameraPosition();
            }
            set {
                display.SetCameraPosition(value);
            }
        }

        public static void Init()
        {
            IsRunning = true;
            display = new AsciiDisplay();
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            if (!External.ShowScrollBar(handle, 1, false)) return;

            //Start da gaem
            Start();
        }

        internal static void Start()
        {
            SceneManager.AddScene(new Scene(new GameObject[] {new GameObject(new AsciiRenderable(ConsoleColor.Green, ConsoleColor.Black, 85/2, 25/2, new char[,] {{'§', '#', '§'}, {'#', '#', '#'}, {'§', '#', '§'}}), 1, 0, new Player()), new GameObject(new AsciiRenderable(ConsoleColor.Gray, ConsoleColor.Black, 85 / 2, 25 / 2, new char[,] { {'¤'} }), 2, 2, new Wall(), Builtin.Collision) }));
            keys = new List<ConsoleKeyInfo>();
            __InputManager_OLDDONTUSEEVERAGAIN.lastKeys = new List<ConsoleKeyInfo>();
            Console.CursorVisible = false;
            timer = new Stopwatch();
            worker = new Thread(WorkerLoop);
            worker.Start();
        }

        public static void Instantiate(GameObject obj)
        {
            Events.OnCreateInstance(obj);
            SceneManager.Scenes[SceneManager.currentScene].addInstance(obj);
        }

        public static void Destroy(GameObject obj)
        {
            Events.OnDestroyInstance(obj);

            //Remove the object
            SceneManager.Scenes[SceneManager.currentScene].Objects.Remove(obj);
        }

        internal static void WorkerLoop() {
            while(IsRunning)
            {
                timer.Reset();
                timer.Start();

                keys.Clear();
                if (Console.KeyAvailable) {
                    while (Console.KeyAvailable) keys.Add(Console.ReadKey(true));
                }
                __InputManager_OLDDONTUSEEVERAGAIN.keys = keys;

#region Thread Work
                SceneManager.UpdateScene(Time.deltaTime, true);

                // Do rendering here
                Console.BackgroundColor = ConsoleColor.Black;
                SceneManager.RenderCurrentScene(display);

                SceneManager.UpdateScene(Time.deltaTime, true);
                Thread.Sleep(10);    
#endregion

                if (__InputManager_OLDDONTUSEEVERAGAIN.lastKeys == null) {
                    __InputManager_OLDDONTUSEEVERAGAIN.lastKeys = new List<ConsoleKeyInfo>();
                }

                __InputManager_OLDDONTUSEEVERAGAIN.lastKeys.Clear();
                __InputManager_OLDDONTUSEEVERAGAIN.lastKeys.AddRange(keys);
                timer.Stop();
                Time.deltaTime = timer.ElapsedMilliseconds / 1000f;
            }
        }
    }

    public static class __InputManager_OLDDONTUSEEVERAGAIN {
        internal static List<ConsoleKeyInfo> lastKeys;
        internal static List<ConsoleKeyInfo> keys;

        public static bool AnyKeyDown {
            get {
                return keys.Count() > 0;
            }
        }

        public static bool GetKey(ConsoleKey key) {
            return (from k in keys where k.Key == key select k).Count() > 0;
        }

        public static bool GetKeyPressed(ConsoleKey key)
        {
            return ((from k in keys where k.Key == key select k).Count() == 1) && ((from k in lastKeys where k.Key == key select k).Count() == 0);
        }

        public static bool GetKeyReleased(ConsoleKey key)
        {
            return ((from k in keys where k.Key == key select k).Count() < 1) && ((from k in lastKeys where k.Key == key select k).Count() > 0);
        }
    }

    public static class InputManager {
        public static bool GetKey(ConsoleKey key) {
            return (from k in keys where k.Key == key select k).Count() > 0;
        }

        public static bool GetKeyPressed(ConsoleKey key)
        {
            return ((from k in keys where k.Key == key select k).Count() == 1) && ((from k in lastKeys where k.Key == key select k).Count() == 0);
        }

        public static bool GetKeyReleased(ConsoleKey key)
        {
            return ((from k in keys where k.Key == key select k).Count() < 1) && ((from k in lastKeys where k.Key == key select k).Count() > 0);
        }
    }

    public static class Time
    {
        public static float deltaTime { get; internal set; }
    }

    public static class Utils
    {
        public static bool TileFree(int x, int y)
        {
            foreach(var pos in (from o in SceneManager.Scenes[SceneManager.currentScene].Objects where o.hasCollision select o.position))
            {
                if ((int)pos.x == x && (int)pos.y == y) return false;
            }
            return true;
        }

        public static bool TileFree(float x, float y)
        {
            foreach(var pos in (from o in SceneManager.Scenes[SceneManager.currentScene].Objects where o.hasCollision select o.position))
            {
                if ((int)pos.x == (int)x && (int)pos.y == (int)y) return false;
            }
            return true;
        }

        public static bool TileFull(int x, int y)
        {
            return !TileFree(x, y);
        }

        public static GameObject GetObjectAtTile(int x, int y)
        {
            for (int i = 0; i < SceneManager.Scenes[SceneManager.currentScene].Objects.Count(); i++)
            {
                if ((int)SceneManager.Scenes[SceneManager.currentScene].Objects[i].position.x == x && (int)SceneManager.Scenes[SceneManager.currentScene].Objects[i].position.y == y)
                {
                    return SceneManager.Scenes[SceneManager.currentScene].Objects[i];
                }
            }
            return null;
        }

       

    }
}
