using Newtonsoft.Json;
using NLog;
using System;
using System.Collections;
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

        public static bool IsObjectList(object obj)
        {
            try
            {
                if (obj == null)
                    return false;

                return obj is IList &&  obj.GetType().IsGenericType &&
                        obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static object WalkObject(List<PropertyLinq> propertyLinqs, object originalRoot)
        {
            try
            {
                object currentRoot = originalRoot;
                int linqs = propertyLinqs.Count;
                int c = 1;
                foreach (PropertyLinq propertyLinq in propertyLinqs)
                {
                    logger.Info("WalkObject() @currentRoot: " + currentRoot.GetType().Name);
                    PropertyInfo property = currentRoot.GetType().GetProperty(propertyLinq.PropertyName);
                    if(property != null)
                    {
                        logger.Info("WalkObject() @Property: " + property.Name);
                    }
                    else
                    {
                        logger.Info("WalkObject() Property \"" + propertyLinq.PropertyName + "\" does not exist in this object");
                    }

                    if (c == propertyLinqs.Count)
                    {
                        logger.Info("WalkObject() This is the final property");
                        if (propertyLinq.DBid == -1) // just get this object without indexing to any children
                        {   //     Singlet Property
                            if (property == null)
                            {
                                logger.Error("WalkObject() Did not find Singlet Property!");
                                return null;
                            }
                            else
                            {
                                return property.GetValue(currentRoot);
                            }
                        }
                        else if (propertyLinq.PropertyName == "")
                        {   //     List<List<(or deeper)>> Property
                            IEnumerable<object> collection = (IEnumerable<object>)currentRoot;
                            object returnObject = collection.ElementAt(propertyLinq.DBid - 1);
                            if (returnObject == null)
                                logger.Error("WalkObject() Did not find List<List<>> element!");

                            return returnObject;
                        }
                        else // DBid is the list index (starting from 1).
                        {   //      List<> Property
                            IEnumerable<object> collection = (IEnumerable<object>)currentRoot;
                            object returnObject = collection.ElementAt(propertyLinq.DBid - 1);
                            if (returnObject == null)
                                logger.Error("WalkObject() Did not find element!");

                            return returnObject;
                        }
                    }

                    if (propertyLinq.DBid == -1) // can ignore DBid.
                    {   //     Singlet Property
                        if (property == null)
                        {
                            logger.Error("WalkObject()'nonfinal Did not find Singlet Property!");
                            return null;
                        }
                        else
                        {
                            currentRoot = property.GetValue(currentRoot);
                        }
                    }
                    else if (propertyLinq.PropertyName == "")
                    {   //     List<List<List<(or deeper)>>> Property
                        IEnumerable<object> collection = (IEnumerable<object>)currentRoot;
                        object nextRoot = collection.ElementAt(propertyLinq.DBid - 1);
                        if (nextRoot == null)
                        {
                            logger.Error("WalkObject()'nonfinal Did not find List<List<>> element!");
                            return null;
                        }

                        currentRoot = nextRoot;
                    }
                    else // DBid is the list index (starting from 1).
                    {   //      List<> Property
                        IEnumerable<object> collection = (IEnumerable<object>)currentRoot;
                        object nextRoot = collection.ElementAt(propertyLinq.DBid - 1);
                        if (nextRoot == null)
                        {
                            logger.Error("WalkObject()'nonfinal Did not find element!");
                            return null;
                        }

                        currentRoot = nextRoot;
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