using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    class ObjectInformation
    {
        public ObjectInformation()
        {
            VisitedOnCurrentSerialization = false;
            PropertyLinqs = new();
            ObjectHash = new();
        }

        public ObjectInformation(bool visited)
        {
            VisitedOnCurrentSerialization = visited;
            PropertyLinqs = new();
            ObjectHash = new();
        }

        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The Serialize() function should use this property directly
        /// When deserializing, the to-be-deserialized object is walked with the information stored here.
        /// </summary>
        public List<PropertyLinq> PropertyLinqs { get; set; }

        /// <summary>
        /// A hash generated from an object. Used to determine if the object has changed.
        /// </summary>
        public ObjectHash ObjectHash { get; set; }

        /// <summary>
        /// If an entry was not visited on the last serialization, remove it from the dictionary.
        /// </summary>
        public bool VisitedOnCurrentSerialization { get; set; }

        /// <summary>
        /// Converts the current SerializationPath of an object to its PropertyLinq representation.
        /// </summary>
        /// <param name="serializationPath">A list of strings representing the subfolders where this object is serialized to.</param>
        /// <returns>A List of PropertyLinqs from the Root to that object.</returns>
        public bool ImportPropertyLinqsFromSerializationPath(List<string> serializationPath, Regex regex)
        {
            try
            {
                int dummy = 0;
                if(int.TryParse(serializationPath.First(), out dummy)) // first element must not be integer.
                {
                    return false;
                }
                this.PropertyLinqs = new();

                string serializationPathStr = String.Join("\\", serializationPath) + "\\";
                //((?<propertyName>([A-Z]|[a-z]|_)+)\\((?<DBid>\d+)|))|(?<DBid2>\d+)
                MatchCollection matchCollection = regex.Matches(serializationPathStr);
                logger.Info("String to be matched: " + serializationPathStr);
                logger.Info("Amount of matches: " + matchCollection.Count);
                foreach(Match match in matchCollection)
                {
                    if(match.Groups["propertyName"].Success)
                    {
                        if (match.Groups["DBid"].Success)
                        {
                            this.PropertyLinqs.Add(new() { DBid = int.Parse(match.Groups["DBid"].Value), PropertyName = match.Groups["propertyName"].Value });
                        }
                        else
                        {
                            this.PropertyLinqs.Add(new() { DBid = -1, PropertyName = match.Groups["propertyName"].Value });
                        }
                    }
                    else if (match.Groups["DBid2"].Success)
                    {
                        this.PropertyLinqs.Add(new() { DBid = int.Parse(match.Groups["DBid2"].Value), PropertyName = "" });
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
            }
            return false;
        }
    }
}