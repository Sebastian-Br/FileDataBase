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
            //logger.Info("GetObjectHash() Called on " + objectToBeHashed.GetType().Name);
            if (!Success)
            {
                logger.Error("GetObjectHash() Success = FALSE.");
                return false;
            }

            try
            {
                if (ReflectionX.IsObjectList(objectToBeHashed))
                {
                    IEnumerable<object> listElements = (IEnumerable<object>)objectToBeHashed;
                    foreach (object listElement in listElements)
                    {
                        if (listElement == null)
                            continue;
                        else
                            GetObjectHash(listElement);
                    }
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
                                    if (!ReflectionX.IsPropertyList(property))
                                        GetObjectHash(property.GetValue(objectToBeHashed));
                                    else
                                    {
                                        Object collection = property.GetValue(objectToBeHashed);
                                        IEnumerable<object> objects = (IEnumerable<object>)collection;
                                        if (objects != null)
                                        {
                                            foreach (object obj in objects)
                                            {
                                                GetObjectHash(obj);
                                            }
                                        }
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
