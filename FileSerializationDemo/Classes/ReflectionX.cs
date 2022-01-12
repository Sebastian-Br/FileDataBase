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
                if(t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type listType = t.GenericTypeArguments.ToList().First();
                    if (listType.IsAssignableTo(typeof(FileDataBase)))
                        return true;
                }
                else // nonlist
                {
                    if (t.IsAssignableTo(typeof(FileDataBase)))
                        return true;
                }
                return false;
            }
            catch(Exception e)
            {

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
    }
}
