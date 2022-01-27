# FileDataBase
Provides easy to use functionality for serializing/deserializing objects from/to the filesystem. </br>
The classes which provide this functionality are located in FileSerializationDemo/Classes/.</br>
## Known Issues
<h3>HIGH Priority :heavy_check_mark: :heavy_check_mark: :heavy_check_mark: ⚠️ ⚠️</h3>
1. :heavy_check_mark: FIXED - Nonprimitive, nonstring or non-FileDataBase-deriving types are not serialized. The Database will not remember them.</br>
  Fix Ideas: Serialize all non-FileDataBase-deriving Types into (TypeName).primitives.json and let the developer decide which properties are ineligible for Serialization via an attribute :heavy_check_mark:</br>
2. :heavy_check_mark: --Decently fixed-- The FileDataBase class can be difficult to understand.</br>
  -- Continue to improve variable names, documentation, and structure. ⚠️</br>
4. :heavy_check_mark: FIXED - When the same object is serialized twice (in different contexts), there was no system in place to establish a reference (this also applied when deserializing). :FileDataBase properties now have a ObjectLinq List and a Root object. These are used to navigate back to the original object and establish the reference. :heavy_check_mark:</br>
6. TODO: New()/empty Lists<:FileFB> are now serialized :heavy_check_mark:, but references are only set up for the individual list elements. As a consequence, adding elements to that List will not produce the desired behavior (the List<> property is NOT the same accross all objects that reference it). ⚠️</br>
8. TODO: For non-:FileDB properties of a :FileDB object, references are only established correctly if that parent-object is the same. This is very similar to (6) and can probably only be efficiently solved by adding an ObjectLinqs extension attribute, containing an ObjectLinq List and a WasSerialized flag/boolean. Accordingly, a fitting name for the ObjectLinq-file might be *PropertyName*.ObjectLinqs.json. ⚠️
<h3>MEDIUM Priority :heavy_check_mark:</h3>
3. :heavy_check_mark: FIXED - Unchanged objects will be re-written to the filesystem upon Serialization.</br>
  Fix Ideas: Compute a hash and compare hashes before writing to primitives.json. :heavy_check_mark: Added [ObjectHashIgnore] attribute (only works on classes). :heavy_check_mark:</br>
<h3>LOW Priority</h3>
5. Update version? </br>
7. Rename project? Except storage, it provides no DB functionality atm and the "file"-part is obvious...
