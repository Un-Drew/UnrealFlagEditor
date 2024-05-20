# UnrealFlagEditor
 An unofficial tool that allows editing the flags of various object types from Unreal package files. This tool makes use of [UELib](https://github.com/EliotVU/Unreal-Library).

![image](https://github.com/Un-Drew/UnrealFlagEditor/assets/69184314/d6c5c5d7-03f0-493f-9f24-b0c03bcebd76)


 Through a user-friendly interface, you can modify:
 - Object Flags (present in all objects)
 - Root Package Flags
 - Class Flags
 - Struct Flags
 - Property Flags
 - Function Flags
 - State Flags

# But why, though?
This tool was primarily made to provide an easy way to disable compiler-enforced script restrictions such as **const** or **protected**, in cases where there's no easy way to access or modify them.

Where, previously, one would have to manually edit the flag bytes in a Hex editor, now it's just a few clicks away! Additionally, this tool also includes a **Headless Mode**, for mod developers that want to include these flag changes into their build pipeline.

# Disclaimers
- ***TO AVOID ANY POTENTIAL DATA LOSS, PLEASE MAKE BACKUPS OF YOUR PACKAGES BEFORE USING THIS TOOL!*** While I tried my best to handle every little edge-case, I can't 100% guarantee that this tool is fool-proof.
- Before using this tool to bypass the script compiler, make sure you know what you're doing! Ensure there isn't already a setter that does what you want. **Basically, only use this as a last resort!**

# Limitations
- Packages from UE4 upwards are not supported.
- While most UE1, UE2 and UE3 packages should work, some specific engine builds might not be fully supported.
- Only **uncompressed** packages are supported (such as editor packages). Thus, cooked packages will have to be decompressed first.

# Requirements
Since this tool makes use of [UELib](https://github.com/EliotVU/Unreal-Library), you will need [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) to run this app.
