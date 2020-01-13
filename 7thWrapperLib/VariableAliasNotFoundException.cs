/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Runtime.Serialization;

namespace _7thWrapperLib
{
    [Serializable]
    public class VariableAliasNotFoundException : Exception
    {
        public VariableAliasNotFoundException()
        {
        }

        public VariableAliasNotFoundException(string message) : base(message)
        {
        }

        public VariableAliasNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VariableAliasNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}