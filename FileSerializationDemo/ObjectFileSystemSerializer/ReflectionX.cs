using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    public static class ReflectionX
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Finds out if a property is a List<> type or not.
        /// </summary>
        /// <param name="p">PropertyInfo parameter.</param>
        /// <returns>True: Is a list. False: Is not a list.</returns>
        public static bool IsPropertyList(PropertyInfo p)
        {
            try
            {
                return p.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// EXAMPLE METHOD/DO NOT USE.
        /// </summary>
        /// <param name="f">The object.</param>
        /// <returns>The ObjectHash that is computed from it.</returns>
        public static ObjectHash GetExtensivePrimitivesHash(object f)
        {
            ObjectHash oH = new();
            logger.Info("GetExtensivePrimitivesHash() Called on " + f.GetType().Name);

            try
            {
                List<PropertyInfo> properties = f.GetType().GetProperties().ToList();

                if (properties == null)
                    return oH;

                foreach (PropertyInfo property in properties)
                {
                    if(property != null)
                    {
                        if (/*!IsDerivedFileDB(property.PropertyType)*/true || false)
                        {
                            if (!IsPropertyList(property))
                                oH.AddObject(property.GetValue(f));
                            else
                            {
                                Object collection = property.GetValue(f);
                                IEnumerable<object> objects = (IEnumerable<object>)collection;
                                if(objects != null)
                                {
                                    foreach (object obj in objects)
                                    {
                                        oH.AddObject(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e, "GetExtensivePrimitivesHash() Exception.");
            }
            
            return oH;
        }

        public static object GetFileDBObject(List<PropertyLinq> objectLinqs, object Root)
        {
            try
            {
                object currentRoot = Root;
                int linqs = objectLinqs.Count;
                int c = 1;
                foreach (PropertyLinq objectlinq in objectLinqs)
                {
                    PropertyInfo property = currentRoot.GetType().GetProperty(objectlinq.PropertyName);
                    logger.Info("GetFileDBObject() @currentRoot: " + currentRoot.GetType().Name);
                    logger.Info("GetFileDBObject() @Property: " + property.Name);
                    if (c == objectLinqs.Count)
                    {
                        logger.Info("GetFileDBObject() This is the final property.");
                        if (!IsPropertyList(property)) // can ignore DBid.
                        {
                            return property.GetValue(currentRoot);
                        }
                        else // DBid is the list index (starting from 1).
                        {
                            object list = property.GetValue(currentRoot);
                            IEnumerable<object> collection = (IEnumerable<object>)list;
                            bool bFoundElement = false;
                            foreach (object listItem in collection)
                            {
                                if (listItem is PropertyLinq @base)
                                {
                                    if (@base.DBid == objectlinq.DBid)
                                    {
                                        logger.Info("GetFileDBObject() Found object with DBid " + @base.DBid);
                                        return listItem;
                                    }
                                }
                            }

                            if (!bFoundElement)
                            {
                                logger.Error("GetFileDBObject() Did not find element!");
                                return null;
                            }
                        }
                    }

                    if(!IsPropertyList(property))
                        currentRoot = property.GetValue(currentRoot);
                    else
                    {
                        object list = property.GetValue(currentRoot);
                        IEnumerable<object> collection = (IEnumerable<object>)list;
                        if(collection == null)
                            logger.Info("GetFileDBObject() iCollection is null!");

                        logger.Info("GetFileDBObject() iCollection has " + collection.Count() + " elements.");

                        bool bFoundElement = false;
                        foreach (object listItem in collection)
                        {
                            if (listItem is PropertyLinq @base)
                            {
                                if (@base.DBid == objectlinq.DBid)
                                {
                                    currentRoot = listItem;
                                    bFoundElement = true;
                                }
                            }
                        }

                        if (!bFoundElement)
                        {
                            logger.Error("GetFileDBObject() Did not find element!2");
                            return null;
                        }
                    }
                    c++;
                }

                logger.Error("GetFileDBObject() Not enough ObjLinqs!");
                return null;
            }
            catch (Exception e)
            {
                logger.Error(e, "GetFileDBObject() Exception.");
                return null;
            }
        }
    }
}