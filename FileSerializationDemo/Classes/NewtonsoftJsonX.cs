using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
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
                Logger logger = LogManager.GetCurrentClassLogger();

                var property = base.CreateProperty(member, memberSerialization);

                Type propertyType = property.PropertyType;
                Attribute FileDBignoreAttribute = member.GetCustomAttribute(typeof(FileDataBaseIgnoreAttribute));
                if (FileDBignoreAttribute == null && !ReflectionX.IsDerivedFileDB(propertyType))
                {
                    property.ShouldSerialize = instance => true;
                }
                else
                {
                    logger.Info("PrimitiveContractResolver: NOT Serializing " + property.PropertyName);
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }
    }
}
