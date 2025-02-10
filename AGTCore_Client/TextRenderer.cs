// using System;
// using System.Collections.Generic;
// using System.IO;
// using OpenTK.Mathematics;
// using OpenTK.Graphics.OpenGL4;
// using System.Drawing;
// using System.Linq;
// using System.IO;

// namespace AGTCORE2.1
// {
//     public struct Character
//     {
//         public int TextureID { get; set; }
//         public Vector2 Size { get; set; }
//         public Vector2 Bearing { get; set; }
//         public int Advance { get; set; }
//     }



//     public static class TextRenderer
//     {
//         TextureGenerator.g;
//             private readonly Vector4 _color;
//         public const string Characters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789µ§½!""#¤%&/()=?^*@£€${[]}\~¨'-_.:,;<>|°©®±¥";
//         private static readonly Dictionary<char, int> Lookup;
//         public static readonly float CharacterWidthNormalized;
//         // 21x48 per char, 
//         public readonly List<RenderCharacter> Text;
//         static TextRenderer()
//         {
//             Lookup = new Dictionary<char, int>();
//             for (int i = 0; i < Characters.Length; i++)
//             {
//                 if (!Lookup.ContainsKey(Characters[i]))
//                     Lookup.Add(Characters[i], i);
//             }
//             CharacterWidthNormalized = 1f / Characters.Length;
//         }
//     }
// }