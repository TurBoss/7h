using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7thHeaven.Code
{
    public enum ModCategory
    {
        Miscellaneous,
        Animations,
        BattleModels,
        BattleTextures,
        FieldModels,
        FieldTextures,
        Gameplay,
        Media,
        Minigames,
        SpellTextures,
        UserInterface,
        WorldModels,
        WorldTextures,
        Unknown
    }

    public static class ModLoadOrder
    {
        public static Dictionary<string, int> Orders = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Miscellaneous", 0},
            { "Animations", 1},
            { "Battle Models", 2},
            { "Battle Textures", 3},
            { "Field Models", 4},
            { "Field Textures", 5},
            { "Gameplay", 6},
            { "Media", 7},
            { "Minigames", 8},
            { "Spell Textures", 9},
            { "User Interface", 10},
            { "World Models", 11},
            { "World Textures", 12},
            { "Unknown", 13}
        };

        public static int Get(ModCategory category)
        {
            return (int)category;
        }

        /// <summary>
        /// Returns the load order based on the <paramref name="category"/>
        /// </summary>
        /// <param name="category"></param>
        /// <returns> 
        /// returns an int >= 0 if valid; 
        /// -1 if not valid category; 
        /// returns load order of Unknown if <paramref name="category"/> is null or empty string 
        /// </returns>
        public static int Get(string category)
        {            
            if (string.IsNullOrWhiteSpace(category))
            {
                return (int)ModCategory.Unknown;
            }

            if (Orders.TryGetValue(category, out int loadOrder))
            {
                return loadOrder;
            }

            if (Enum.TryParse<ModCategory>(category, true, out ModCategory result))
            {
                return (int)result;
            }

            return -1;
        }
    }
}
