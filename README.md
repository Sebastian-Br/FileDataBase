# FileDataBase
Provides easy to use functionality for serializing/deserializing objects from/to the filesystem. </br>
The classes which provide this functionality are located in FileSerializationDemo/Classes/.</br>
## Known Issues
<h3>HIGH Priority</h3>
1. Nonprimitive, nonstring or non-FileDataBase-deriving types are not serialized. The Database will not remember them.</br>
  Fix Ideas: Serialize all non-FileDataBase-deriving Types into (TypeName).primitives.json and let the developer decide which properties are ineligible for Serialization via an attribute</br>
2. The FileDataBase class still is confounded and hard to understand.</br>
  Fix Ideas: Export functions that are not uniquely tied to that class into new and more fitting classes.
<h3>MEDIUM priority</h3>
3. Unchanged objects will be re-written to the filesystem upon Serialization.</br>
  Fix Ideas: Compute a NonDerivedContentHash and compare hashes before writing the primitives.json.
  Non-Fix Ideas: Copying the non-FileDataBase-deriving properties is unfit because it will double memory usage.
<h3>LOW Priority</h3>
4. When the same object is serialized twice (in different contexts), there is currently no system in place that could reference a path to the first serialization to save disk space.</br>
  Fix Ideas: Introduce a second FilePath property that tracks where the identical object was first serialized. The Deserializer then needs to switch context to that directory.
