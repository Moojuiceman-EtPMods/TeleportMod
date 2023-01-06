using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace TeleportMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public static ConfigEntry<KeyboardShortcut> openMenuKey;

        private void Awake()
        {
            openMenuKey = Config.Bind("General", "Open Menu Key", new KeyboardShortcut(KeyCode.F4), "Key to open teleport menu");

            // Plugin startup logic
            logger = Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded");
            Logger.LogInfo($"Patching...");
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Patched");
        }

        [HarmonyPatch(typeof(PlayerManager), "Start")]
        [HarmonyPrefix]
        static void Start_Prefix()
        {
            new GameObject("__Teleporter__").AddComponent<TeleportMod>().Init(logger, openMenuKey);
        }
    }

    internal class Location
    {
        public Location(float x, float y, float z, string name)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.name = name;
        }

        public string GetName()
        {
            return this.name;
        }

        public float GetY()
        {
            return this.y;
        }

        public float GetZ()
        {
            return this.z;
        }

        public float GetX()
        {
            return this.x;
        }

        private float x;

        private float y;

        private float z;

        private string name;
    }

    internal class TeleportMod : MonoBehaviour
    {
        static ManualLogSource logger;
        static ConfigEntry<KeyboardShortcut> openMenuKey;
        static string locationsPath = $"{Paths.ConfigPath}/TeleportModLocations.xml";

        private bool visible;
        private GUIStyle labelStyle;
        public string TPname = "";
        public string Message = "";
        private static Xml xml = new Xml();
        public Rect TPWindow = new Rect(420f, 0f, 300f, 300f);
        private bool clicked;

        public void Init(ManualLogSource logSource, ConfigEntry<KeyboardShortcut> menuKey)
        {
            logger = logSource;
            openMenuKey = menuKey;
        }

        private void Start()
        {
            if (!File.Exists(locationsPath))
            {
                TeleportMod.xml.Create(locationsPath);
            }
        }

        private void Update()
        {
            if (openMenuKey.Value.IsDown())
            {
                logger.LogDebug("Open");
                if (this.visible)
                {
                    this.Message = "";
                    CursorManager.Instance.HideCursor();
                }
                else
                {
                    CursorManager.Instance.ShowCursor();
                }
                this.visible = !this.visible;
                if (this.clicked)
                {
                    this.clicked = false;
                }
                logger.LogDebug(this.visible.ToString());
            }
        }

        private void OnGUI()
        {
            if (!this.visible)
            {
                return;
            }
            Matrix4x4 matrix = GUI.matrix;
            if (this.labelStyle == null)
            {
                this.labelStyle = new GUIStyle(GUI.skin.label);
                this.labelStyle.fontSize = 12;
            }
            GUI.Box(new Rect(10f, 10f, 400f, 280f), "Teleport menu", GUI.skin.window);
            float num = 50f;
            GUI.Label(new Rect(20f, num, 150f, 20f), "Location Name", this.labelStyle);
            this.TPname = GUI.TextField(new Rect(170f, num, 200f, 20f), this.TPname, 25);
            num += 30f;
            float x = PlayerManager.Instance.PlayerTransform().transform.position.x;
            float y = PlayerManager.Instance.PlayerTransform().transform.position.y;
            float z = PlayerManager.Instance.PlayerTransform().transform.position.z;
            if (GUI.Button(new Rect(20f, num, 100f, 20f), "SAVE"))
            {
                TeleportMod.xml.Update(locationsPath, this.TPname, x, y, z);
                this.Message = "Location '" + this.TPname + "' added!";
                this.TPname = "";
            }
            num += 30f;
            if (GUI.Button(new Rect(280f, num, 100f, 20f), "Locations"))
            {
                this.clicked = true;
            }
            if (this.clicked)
            {
                this.TPWindow = GUI.Window(0, this.TPWindow, new GUI.WindowFunction(this.FillTPWindow), "TP Locations");
            }
            num += 30f;
            GUI.Label(new Rect(20f, num, 150f, 20f), "x: " + x, this.labelStyle);
            num += 20f;
            GUI.Label(new Rect(20f, num, 150f, 20f), "y: " + y, this.labelStyle);
            num += 20f;
            GUI.Label(new Rect(20f, num, 150f, 20f), "z: " + z, this.labelStyle);
            num += 30f;
            GUI.Label(new Rect(20f, num, 150f, 20f), this.Message, this.labelStyle);
            GUI.matrix = matrix;
        }

        private void FillTPWindow(int windowID)
        {
            float num = 30f;
            new List<Location>();
            foreach (Location location in TeleportMod.xml.Read(locationsPath))
            {
                GUI.Label(new Rect(20f, num, 150f, 20f), location.GetName(), this.labelStyle);
                if (GUI.Button(new Rect(120f, num, 80f, 20f), "Teleport"))
                {
                    PlayerManager.Instance.PlayerTransform().localPosition = new Vector3(location.GetX(), location.GetY(), location.GetZ());
                }
                if (GUI.Button(new Rect(205f, num, 80f, 20f), "Remove"))
                {
                    TeleportMod.xml.Delete(locationsPath, location);
                }
                num += 30f;
            }
            if (GUI.Button(new Rect(20f, num, 100f, 20f), "Close"))
            {
                this.clicked = false;
            }
        }
    }

    internal class Xml
    {
        public void Create(string path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement documentElement = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(newChild, documentElement);
            XmlElement newChild2 = xmlDocument.CreateElement("body");
            xmlDocument.AppendChild(newChild2);
            xmlDocument.Save(path);
        }

        public void Update(string path, string name, float x, float y, float z)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/body");
            XmlElement xmlElement = xmlDocument.CreateElement("Location");
            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("x");
            xmlAttribute.Value = x.ToString();
            xmlElement.Attributes.Append(xmlAttribute);
            xmlAttribute = xmlDocument.CreateAttribute("y");
            xmlAttribute.Value = y.ToString();
            xmlElement.Attributes.Append(xmlAttribute);
            xmlAttribute = xmlDocument.CreateAttribute("z");
            xmlAttribute.Value = z.ToString();
            xmlElement.Attributes.Append(xmlAttribute);
            xmlAttribute = xmlDocument.CreateAttribute("name");
            xmlAttribute.Value = name;
            xmlElement.Attributes.Append(xmlAttribute);
            xmlNode.AppendChild(xmlElement);
            xmlDocument.Save(path);
        }

        public List<Location> Read(string path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/body/Location");
            List<Location> list = new List<Location>();
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode = (XmlNode)obj;
                XmlAttribute xmlAttribute = xmlNode.Attributes["name"];
                XmlAttribute xmlAttribute2 = xmlNode.Attributes["x"];
                XmlAttribute xmlAttribute3 = xmlNode.Attributes["y"];
                XmlAttribute xmlAttribute4 = xmlNode.Attributes["z"];
                string value = xmlAttribute.Value;
                float x = float.Parse(xmlAttribute2.Value);
                float y = float.Parse(xmlAttribute3.Value);
                float z = float.Parse(xmlAttribute4.Value);
                Location item = new Location(x, y, z, value);
                list.Add(item);
            }
            return list;
        }

        public void Delete(string path, Location location)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/body/Location[@name='" + location.GetName() + "']");
            xmlNode.ParentNode.RemoveChild(xmlNode);
            xmlDocument.Save(path);
        }
    }
}
