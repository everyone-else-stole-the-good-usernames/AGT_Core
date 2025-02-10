// using System.Collections.Generic;
// using System.Text.Json;
// using System.Text.Json.Serialization;

// namespace MC2Server
// {
//     public static class FileHandler
//     {
//         static string savefile = "header.chnk";
//         public static Dictionary<string, Player> savedPlayers;

//         static void loadSavedPlayers()
//         {
//             savedPlayers = new Dictionary<string, Player>();
//         }
//         public Dictionary<string, byte> loadChunk()
//         {
//             var cereal = File.ReadAllBytes($"save/{chunkPos.X}, {chunkPos.Y}.chnk");
//             // gets deserialised block dictionary from file
//             // ?? checks if a value is null, if yes: right value is used
//             blocks = MemoryPackSerializer.Deserialize<Dictionary<Vector3i, byte>>(cereal) ?? new Dictionary<Vector3i, byte>();

//             return blocks;
//         }

//         public void saveChunk()
//         {
//             if (blocks.Count < 1) return; // no point saving empty chunks
//             var cereal = MemoryPackSerializer.Serialize(blocks); // serialises the blocks to a byte[] array
//             // a file is created and used for each chunk. this allows multiple chunks to be saved and loaded concurrently
//             using (var fileStream = new FileStream($"save/{chunkPos.X}, {chunkPos.Y}.chnk", FileMode.OpenOrCreate))
//             {
//                 // writes the byte array to a file
//                 fileStream.Write(cereal);
//             }
//         }
//     }
// }