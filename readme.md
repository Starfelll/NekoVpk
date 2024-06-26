<h1 align="center">
  <br>
  <img src="https://github.com/Starfelll/NekoVpk/blob/main/image/NekoVpk-128.png" alt="NekoVpk" width="128">
  <br>
  NekoVpk
  <br>
</h1>
<h4 align="center">🧰 Left4Dead2 addon manager </h4>
<h1 align="center">
  <img src="https://github.com/Starfelll/NekoVpk/blob/main/image/%7B92AF4B24-ECB7-4f37-A7EF-2452A7E1151B%7D.png" width="800">
</h1>

## 🐈feature
* Intelligently identify tags based on addon content
* Keyword search
* Read addoninfo
* Enable or disable addon
* Disable certain content in the addon, which is useful for survivor mods.
* ![](https://github.com/Starfelll/NekoVpk/blob/main/image/%7B5B7F3754-AEAB-478f-90C3-D0D2934D8D8A%7D.png)


## Special Dependencies
https://github.com/Starfelll/ValvePak/tree/nekovpk
https://github.com/Starfelll/ValveKeyValue/tree/nekovpk


## changelog
#### v0.1.5
- Fixed the issue that files would be lost when only one disabled vpk content was left.
- Setting: added option "Compression level".
#### v0.1.4
- Store window size and position.
- Tag: merge zoey_light, francis_light, bill_death_pose, modify script to vscript.
- Fixed a bug that caused the window to freeze when vpk content was enabled.
#### v0.1.3
- Skip reading failed vpk files
#### v0.1.2
- Tag: Support for infected assets.
- Add a new column to display the type of addon.
#### v0.1.1
- Automatically back up files when they are modified.
#### v0.1.0
- Breaking updates, mods that disable some content need to be fully enabled in the old version to be recognized in the new version.
- Enable file compression for disabled content.
- New UI layout.
- Add tags: zoey_light,bill_deathpose,francis_light.
#### v0.0.8
- Supports searching file names.
- During the scanning process, the workshop directory is no longer required.
- Close the file handle after reading the image.
- TaggedAssets.jsonc: added weapon-related tags.
#### v0.0.7.1
- TaggedAssets.jsonc add tags: particle,sound,spr,xdr,skybox
- Identify the vpk with a numeric file name as the workshop id
- Fixed an issue where addon titles with the same characters as tags would not be displayed in the search list
