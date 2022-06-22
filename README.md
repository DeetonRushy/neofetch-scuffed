## neofetch - scuffed edition
It's just neofetch for windows. It's very scuffed and hard to read so edit at your own risk.

it will not work on linux, I tried to make it so it would but couldn't be bothered writing the same check over and over.
this doesn't matter anyways scene as you guys already have it :sweat_smile:

however, it looks cool and has some decent functionality.

![default colours](https://media.discordapp.net/attachments/989233326973915139/989233349266665543/unknown.png)
with a different colour
![pink color](https://media.discordapp.net/attachments/989233326973915139/989233511686877214/unknown.png?width=1093&height=609)

images above are using [Windows Terminal](https://apps.microsoft.com/store/detail/windows-terminal/9N0DX20HK701?hl=en-us&gl=US), so it may
not look so nice in native cmd

## CommandLine

|Option| Functionality |
|--|--|
| -v, --verbose | enables logs and stuff |
| -c, --color | sets the default colour to the argument supplied, to see colours just activate verbose mode & -L then enter anything. it will then list all colours |
| -r, --reset | will reload all values that aren't likely to change that are cached |
| -L, --let-me-read | (requires verbose) will wait for a prompt after all logs have printed |
| -l, --live | goes into a mode that refreshes the output every 1s, exit with ctrl+c |
| -t, --time | will time how long it takes to fetch all info, scene as windows 'time' doesn't do this |
| -s, --specific | takes multiple arguments, will display specific specs |
| --help | will display all the above information in the commandline |

# Dependencies

 - [System.Management](https://www.nuget.org/packages/System.Management/)
 - [CommandLine](https://www.nuget.org/packages/CommandLineParser/)
 - [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/13.0.2-beta1)
 - System.Windows.Forms, switch application to 'Windows Application' then back to 'Console Application' to get the reference
 - Microsoft.Win32, should be there already but might be different on things like VsCode

# Contributions
 Stack overflow, of course.

