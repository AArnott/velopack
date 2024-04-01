# Vendor Binaries
This folder contains pre-compiled binaries from a variety of sources. These should be updated periodically.

### signtool.exe v10.0.22621
- Signs application binaries while building packages.
- Can be found in the Windows SDK at "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x86\signtool.exe" or similar, depending on the version of the SDK you have installed.
- License? https://github.com/dotnet/docs/issues/10478

### 7za.exe / 7zz v21.07
- Incldued because it is much faster at zipping / unzipping than the available managed algorithms.
- Can be found at https://www.7-zip.org/
- License is LGPL & BSD 3: https://www.7-zip.org/license.txt

### zstd.exe v1.5.5
- Fast compression and diff/patch
- Can be found at https://github.com/facebook/zstd
- License is GPL-2.0 & BSD 3: https://github.com/facebook/zstd/blob/dev/LICENSE, https://github.com/facebook/zstd/blob/dev/COPYING

### appimagetool
- Create .AppImage for Linux
- Can be found at https://github.com/AppImage/AppImageKit
- License is MIT https://github.com/AppImage/AppImageKit/blob/master/LICENSE