using FileSerializationDemo;
using FileSerializationDemo.Classes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EZDB_Core.ObjectFileSystemSerializer
{
    public class ObjectHashMgr
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Logger logger = LogManager.GetCurrentClassLogger();

        private ObjectHash objectHash { get; set; }

        private bool Success { get; set; }

        public ObjectHashMgr()
        {
        }

        public ObjectHash Compute(object objectToBeHashed)
        {
            objectHash = new();
            Success = true;
            if (GetObjectHash(objectToBeHashed))
            {
                return objectHash;
            }
            else
            {
                return null;
            }
        }

        private bool GetObjectHash(object objectToBeHashed)
        {
            if(objectToBeHashed == null)
            {
                //logger.Info("GetObjectHash() Called on NULL");
                return true;
            }
            logger.Info("GetObjectHash() Called on " + objectToBeHashed.GetType().Name);
            if (!Success)
            {
                logger.Error("GetObjectHash() Success = FALSE.");
                return false;
            }

            try
            {
                if (ReflectionX.IsObjectList(objectToBeHashed))
                {
                    logger.Info("GetObjectHash() This is a list!");
                    /*IEnumerable<object> listElements = (IEnumerable<object>)objectToBeHashed;
                    foreach (object listElement in listElements)
                    {
                        if (listElement == null)
                            continue;
                        else
                            GetObjectHash(listElement);
                    }*/

                    PropertyInfo propInfo_Count = objectToBeHashed.GetType().GetProperty("Count");
                    PropertyInfo propInfo_Item = objectToBeHashed.GetType().GetProperty("Item");
                    int count = (int)propInfo_Count.GetValue(objectToBeHashed);

                    for (int i = 0; i < count; i++)
                    {
                        GetObjectHash(propInfo_Item.GetValue(objectToBeHashed, new object[] { i }));
                    }

                    return true;
                }
                else
                {
                    if (objectToBeHashed.GetType().IsValueType || objectToBeHashed is string)
                    {
                        objectHash.AddObject(objectToBeHashed);
                    }
                    else
                    {
                        //logger.Info("GetObjectHash() " + objectToBeHashed.GetType() + " was not a value type!");
                        List<PropertyInfo> properties = objectToBeHashed.GetType().GetProperties().ToList();
                        if (properties != null)
                        {
                            foreach (PropertyInfo property in properties)
                            {
                                if (property != null)
                                {
                                    logger.Info("GetObjectHash() Hashing propertyName: " + property.Name);
                                    if (!ReflectionX.IsPropertyList(property))
                                        GetObjectHash(property.GetValue(objectToBeHashed));
                                    else
                                    {
                                        Object collection = property.GetValue(objectToBeHashed);

                                        //Type nestedType = collection.GetType().GetGenericArguments()[0];
                                        PropertyInfo propInfo_Count = collection.GetType().GetProperty("Count");
                                        PropertyInfo propInfo_Item = collection.GetType().GetProperty("Item");
                                        int count = (int)propInfo_Count.GetValue(collection);

                                        for(int i = 0; i < count; i++)
                                        {
                                            GetObjectHash(propInfo_Item.GetValue(collection, new object[] { i }));
                                        }

                                        /*IEnumerable<object> objects = (IEnumerable<object>)collection;
                                        if (objects != null)
                                        {
                                            foreach (object obj in objects)
                                            {
                                                GetObjectHash(obj);
                                            }
                                        }*/
                                    }
                                }
                            }
                        }
                    }
                }

                return Success;
            }
            catch (Exception e)
            {
                logger.Error(e, "GetObjectHash() Exception.");
                Success = false;
                return Success;
            }
        }
    }
}
