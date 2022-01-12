using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    class NewtonsoftJsonX
    {
        /// <summary>
        /// Provides functionality to SerializePrimitives().
        /// (Re-)defines what primitive types are.
        /// See: https://stackoverflow.com/questions/15929848/serialize-only-simple-types-using-json-net
        /// </summary>
        public class PrimitiveContractResolver : DefaultContractResolver
        {
            protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                var propertyType = property.PropertyType;
                if (propertyType.IsPrimitive || propertyType == typeof(string))
                {
                    property.ShouldSerialize = instance => true;
                }
                else
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }
    }
}
