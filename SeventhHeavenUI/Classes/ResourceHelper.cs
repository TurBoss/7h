using _7thHeaven.Code;
using SeventhHeavenUI;
using System;

namespace SeventhHeaven.Classes
{
    public static class ResourceHelper
    {
        public static string Get(StringKey key)
        {
            try
            {
                return App.Current.Resources[key.ToString()].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
