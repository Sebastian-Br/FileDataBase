using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FileSerializationDemo.Classes
{
    [ObjectHashIgnore]
    public class ObjectHash
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Logger logger = LogManager.GetCurrentClassLogger();
        public ObjectHash()
        {
            currentHindex = 1;
            H1 = new byte[32];
            H2 = new byte[32];
            H3 = new byte[32];
            Sha256 = SHA256.Create();
        }

        public byte[] H1 { get; set; }
        public byte[] H2 { get; set; }
        public byte[] H3 { get; set; }

        private int currentHindex { get; set; }

        private SHA256 Sha256 { get; set; }

        public void AddObject(object obj)
        {
            try
            {
                if (obj is null)
                {
                    logger.Info("AddObject() Object was null!");
                    return;
                }
                if (obj is ObjectHash)
                    return;

                Attribute ObjHashIgnoreAttribute = obj.GetType().GetCustomAttribute(typeof(ObjectHashIgnoreAttribute));

                if(ObjHashIgnoreAttribute != null)
                {
                    logger.Info("AddObject() This object had the ObjectHashIgnore attribute!");
                    return;
                }

                logger.Info("AddObject() Trying to add object of type " + obj.GetType());

                if (currentHindex == 1)
                    H1 = Sha256.ComputeHash(combine(ObjectToByteArray(obj), H2));
                if (currentHindex == 2)
                    H2 = Sha256.ComputeHash(combine(ObjectToByteArray(obj), H3));
                if (currentHindex == 3)
                    H3 = Sha256.ComputeHash(combine(ObjectToByteArray(obj), H1));

                currentHindex++;

                if (currentHindex > 3)
                    currentHindex = 1;
            }
            catch (Exception e)
            {
                logger.Error(e, "AddObject() exception.");
            }
        }

        public static bool operator ==(ObjectHash a, ObjectHash b)
        {
            try
            {
                if (a is null)
                    return b is null;
                if (b is null)
                    return a is null;

                if (a.H1.SequenceEqual(b.H1) && a.H2.SequenceEqual(b.H2) && a.H3.SequenceEqual(b.H3))
                    return true;
            }
            catch(Exception e)
            {

            }
            
            return false;
        }
        public static bool operator !=(ObjectHash a, ObjectHash b)
        {
            if (!(a.H1.SequenceEqual(b.H1) && a.H2.SequenceEqual(b.H2) && a.H3.SequenceEqual(b.H3)))
                return true;
            return false;
        }

        public override string ToString()
        {
            return BitConverter.ToString(H1).Replace("-", string.Empty) + "." + BitConverter.ToString(H2).Replace("-", string.Empty) + "." + BitConverter.ToString(H3).Replace("-", string.Empty);
        }

        // Convert an object to a byte array
        private byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private byte[] combine(byte[] a, byte[] b)
        {
            if (b.Length == 0)
                return a;
            if (a.Length == 0)
                return b;

            byte[] combined = new byte[a.Length + b.Length];
            Array.Copy(a, combined, b.Length);
            Array.Copy(b, 0, combined, a.Length, b.Length);
            return combined;
        }

        [System.AttributeUsage(AttributeTargets.All)]
        public class ObjectHashIgnoreAttribute : System.Attribute
        {
        }
    }
}