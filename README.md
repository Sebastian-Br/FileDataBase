# FileDataBase
Provides easy to use functionality for serializing/deserializing objects from/to the filesystem. </br>
The classes which provide this functionality can be found in FileSerializationDemo/Classes/.</br>
## Known Issues
<h3>HIGH Priority</h3>
1. Nonprimitive, nonstring or nonFileDataBase-deriving types are not serialized. The Database will not remember them.</br>
2. The FileDataBase class still is confounded and hard to understand.</br>
<h3>MEDIUM priority</h3>
3. Unchanged objects will be re-written to the filesystem upon Serialization.</br>
<h3>LOW Priority</h3>
1. When the same object is serialized twice (in different contexts), there is currently no system in place that could reference a path to the first serialization to save disk space.
