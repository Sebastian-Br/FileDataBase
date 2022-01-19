# FileDataBase
Provides easy to use functionality for serializing/deserializing objects from/to the filesystem. </br>
The classes which provide this functionality are located in FileSerializationDemo/Classes/.</br>
## Known Issues
<h3>HIGH Priority :heavy_check_mark: :heavy_check_mark: :heavy_check_mark:</h3>
1. :heavy_check_mark: FIXED - Nonprimitive, nonstring or non-FileDataBase-deriving types are not serialized. The Database will not remember them.</br>
  Fix Ideas: Serialize all non-FileDataBase-deriving Types into (TypeName).primitives.json and let the developer decide which properties are ineligible for Serialization via an attribute :heavy_check_mark:</br>
2. :heavy_check_mark: --Decently fixed-- The FileDataBase class can be difficult to understand.</br>
  -- Continue to improve variable names, documentation, and structure. ⚠️</br>
4. :heavy_check_mark: FIXED - When the same object is serialized twice (in different contexts), there was no system in place to establish a reference (this also applied when deserializing). </br>
:FileDataBase properties now have a List<ObjectLinq> and Root object. These are used to navigate back to the original object and establish the reference. :heavy_check_mark:</br>
<h3>MEDIUM Priority :heavy_check_mark:</h3>
3. :heavy_check_mark: FIXED - Unchanged objects will be re-written to the filesystem upon Serialization.</br>
  Fix Ideas: Compute a hash and compare hashes before writing to primitives.json. :heavy_check_mark: Small todo: Add [ObjectHashIgnore] attribute. :heavy_check_mark:</br>
<h3>LOW Priority</h3>
5. Update version.
