using SkiaSharp;
using System.IO;
using System.Collections.Generic;

namespace AGTCore
{
    public static class Constants
    {
        static List<string> blocks = new List<string>();
        public static string[] blockNames // reading this variable returns the result from the getter method
        {
            get => blocks.ToArray();
        }
        static Constants()
        {
            if (File.Exists("config.txt"))
            {
                string[] lines = File.ReadAllLines("config.txt");
                foreach (string line in lines)
                {
                    if (line.StartsWith("block "))
                    {
                        string blockName;
                        blockName = line.Substring(6);
                        if (File.Exists("textures/" + blockName + ".png"))
                            blocks.Add(blockName);

                    }
                }
            }
        }
    }
}
