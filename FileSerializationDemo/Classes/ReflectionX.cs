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
        /// Determines whether a type is either
        /// A) Derived from FileDataBase.
        /// B) A List-Type where the nested type is derived from FileDataBase.
        /// </summary>
        /// <param name="t">The type in question.</param>
        /// <returns>True: The type or nested type is derived from FileDataBase. False otherwise.</returns>
        public static bool IsDerivedFileDB(Type t)
        {
            try
            {
                logger.Info("IsDerivedFileDB() Type t = " + t.Name);

                bool bIsList = false;
                try
                {
                    if (t.GetGenericTypeDefinition() == typeof(List<>))
                        bIsList = true;
                }
                catch { }

                if(bIsList)
                {
                    logger.Info("IsDerivedFileDB() " + t.Name + " is a List.");
                    Type listType = t.GenericTypeArguments.ToList().First();
                    if (listType.IsAssignableTo(typeof(FileDataBase)))
                    {
                        logger.Info("IsDerivedFileDB() " + t.Name + " is a List<" + listType.Name + ":FileDB>Type.");
                        return true;
                    }
                    else
                    {
                        logger.Info("IsDerivedFileDB() " + t.Name + " is a foreign List<> Type.");
                    }
                }
                else // nonlist
                {
                    logger.Info("IsDerivedFileDB() " + t.Name + " is not a List.");
                    if (t.IsAssignableTo(typeof(FileDataBase)))
                    {
                        logger.Info("IsDerivedFileDB() " + t.Name + " is :FileDB.");
                        return true;
                    }
                    else
                    {
                        logger.Info("IsDerivedFileDB() " + t.Name + " is not :FileDB.");
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                logger.Error(e, "IsDerivedFileDB() exception.");
                return false;
            }
        }

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
        /// Determines an arbitrary object's hash.
        /// Used to check if an object has changed in value.
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
                        if (!IsDerivedFileDB(property.PropertyType))
                        {
                            if (!IsPropertyList(property))
                                oH.AddObject(property.GetValue(f, null));
                            else
                            {
                                Object collection = property.GetValue(f, null);
                                IEnumerable<object> iCollection = (IEnumerable<object>)collection;

                                foreach (object objProperty in iCollection)
                                {
                                    oH.AddObject(objProperty);
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
    }
}