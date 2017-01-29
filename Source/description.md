Allows building floors out of stuff*.

# Important
This mod **requires** both [HugsLib](http://steamcommunity.com/sharedfiles/filedetails/?id=818773962) and [Architect Sense](http://steamcommunity.com/sharedfiles/filedetails/?id=852998459) to be loaded before it. HugsLib must be loaded before Architect Sense, and Architect Sense must be loaded before Stuffed Floors.

You can safely add this mod to you save games, but **you can not remove it** from save games. (If you really must do this, you'd have to remove all stuffed floors added by this mod first).

# Features
This mod does two things. First, using Architect Sense it provides a framework for modders to define their own stuffed floor types. Second, it uses this framework to create a number of new floors that are stuffed. 

**For players**
Adds several floor types (borrowed with permission from Telkir's [More Floors](http://steamcommunity.com/sharedfiles/filedetails/?id=725623521)) in a variety of stone, wood and metal types. Works great with other mods that add more resources, e.g. [Minerals and Materials](http://steamcommunity.com/sharedfiles/filedetails/?id=728233992&searchtext), [Extended Woodworking](http://steamcommunity.com/sharedfiles/filedetails/?id=836912371) and [GlitterTech](http://steamcommunity.com/sharedfiles/filedetails/?id=725576127). 

This mod also cleans up the other floor types added by More Floors, Extended Woodworking, Minerals and Materials and GlitterTech. 

**For modders**
Adds a custom FloorTypeDef that derives from TerrainDef, and allows modders to create floortypes by setting a texture (grayscale for best effect, will be coloured by stuff) and a list of stuffCategories to generate terrain defs for. The resulting designators will be placed together in a category, and any vanilla or other mod's terrains made obsolete can also be provided in the XML, and will then be hidden for the user (but not removed, so it won't break save games).

# Known issues
While this mod will happily generate more floors for any mods that add materials to the metallic, stony and/or woody stuff types, this mod only cleans up the floor types added by More Floors, Extended Woodworking, Minerals and Materials and GlitterTech. Any other mods that add floor types may appear as duplicates. If you encounter such an issue, or know of mods that add more floors, please let me know! 