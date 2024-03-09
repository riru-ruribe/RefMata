## import post process

#### ◆ overview
it is mainly processed in the following cases  
- when compiled a script that inherit 'IRefMataHookable'
- if there is a change in tracked asset

the process here is the content implemented in 'IRefMataHookable'.  
in other words, references hold by (scenes, prefabs, etc.) will be rewritten.

#### ◆ tracked asset
**tracked asset** is in a folder with label starting with 'RefMata'.  
"whether it's in a folder" is determined recursively toward the parent.

a menu exists to label folders.  
**Assets/RefMata/Add Label/Hook Folder (x)**  
x is depends on how much of the parent folder is included in the label name.

#### ◆ hookable asset
about assets whose references are overwritten.  
in addition to checking if "IRefMataHookable" is inherited, it also checks for label matching and if "RefMataLoadAttribute" exists.  
however, labels are not checked when compiled script.

label is specified by arguments of 'RefMatableAttribute'.  
be careful as label output is $"RefMata{argument}".

a menu exists to label hookable assets.  
**Assets/RefMata/Add Label/Hookable**

by default it handles scenes(.unity), prefabs(.prefab), and 'ScriptableObject'.  
if you want to process other assets, please implement the partial method of 'RefMataPostProcessor'.
