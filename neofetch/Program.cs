using Pastel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Diagnostics;

using CommandLine;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Forms;

Dictionary<string, Color> SupportedColours = new()
{
    { "Blue", Color.Blue },
    { "Green", Color.Green },
    { "Red", Color.Red },
    { "Yellow", Color.Yellow },
    { "Pink", Color.Pink },
    { "SkyBlue", Color.SkyBlue },
    { "SeaGreen", Color.SeaGreen },
    { "SlateBlue", Color.SlateBlue },
    { "Gray", Color.LightGray }
};
Dictionary<string, Action> SupportedActions = new()
{
    {
        "Host",
        () =>
        {
            // fetch host description
            var hostDesc = Environment.UserDomainName;

            // display the spec.
            WriteSpec("HostDescription", hostDesc);
        }
    },
    {
        "Os",
        () =>
        {
            // write the spec
            WriteSpec("OsDescription", RuntimeInformation.OSDescription);
        }
    },
    {
        "Uptime",
        () =>
        {
            // fetch uptime
            var uptime = GetUptime();

            // write spec

            WriteSpec("Uptime", uptime);
        }
    },
    {
        "MemoryStats",
        () =>
        {
            var memory = GetMemory();

            WriteSpec("MemoryStats", memory);
        }
    },
    {
        "Kernel",
        () =>
        {
            var kernel = GetOSInfo() + $" ({RuntimeInformation.RuntimeIdentifier})";

            WriteSpec("Kernel", kernel);
        }
    },
    {
        "CPU",
        () =>
        {
            List<string> descriptions = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_Processor");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var clockSpeed = obj["CurrentClockSpeed"].ToString();
                    var procName = obj["Name"].ToString();
                    var manufacturer = obj["Manufacturer"].ToString();
                    var version = obj["Version"].ToString();

                    descriptions.Add($"{procName} {version}");
                }
            }
            else
            {
                descriptions.Add("<Platform-Not-Supported>");
            }

            foreach (var cpuSpec in descriptions)
            {
                WriteSpec("CPU", cpuSpec);
            }
        }
    },
    {
        "GPU",
        () =>
        {
            List<string> gpuDescriptions = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    gpuDescriptions.Add(obj["Name"].ToString()!);
                }
            }
            else
            {
                gpuDescriptions.Add("<Platform-Not-Supported>");
            }

            foreach (var gpuSpec in gpuDescriptions)
            {
                WriteSpec("GPU", gpuSpec);
            }
        }
    },
    {
        "Packages",
        () =>
        {
            WriteSpec("Packages", GetPackageCount());
        }
    },
    {
        "Resolution",
        () =>
        {
            var resolution = GetResolution();
            WriteSpec("Resolution", resolution);
        }
    }
};

App.Instance = Parser.Default.ParseArguments<App>(Environment.GetCommandLineArgs()).Value;

if (App.Instance == null)
{
    Console.WriteLine($"[{"neofetch".Pastel(Color.Blue)}] parser displayed help message, exiting.");
    return;
}
if (App.Instance.TimeApp)
{
    if (!App.Instance.LiveMode)
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.Write("\n\n");
            WriteSpec("real", $"{App.Instance.ApplicationTimer!.Elapsed} ({App.Instance.ApplicationTimer.ElapsedMilliseconds}ms)");
        };

        App.Instance.ApplicationTimer = Stopwatch.StartNew();
    }
}

var AsciiArt = $@"
                               {DateTime.Now.ToLongTimeString().Pastel(Color.White)}
  {"███╗   ██╗███████╗ ██████╗ ███████╗███████╗████████╗ ██████╗██╗  ██╗".Pastel(Color.Red)}
  {"████╗  ██║██╔════╝██╔═══██╗██╔════╝██╔════╝╚══██╔══╝██╔════╝██║  ██║".Pastel(Color.White)}
  {"██╔██╗ ██║█████╗  ██║   ██║█████╗  █████╗     ██║   ██║     ███████║".Pastel(Color.Red)} windows edition
  {"██║╚██╗██║██╔══╝  ██║   ██║██╔══╝  ██╔══╝     ██║   ██║     ██╔══██║".Pastel(Color.White)}    by deeton rushton
  {"██║ ╚████║███████╗╚██████╔╝██║     ███████╗   ██║   ╚██████╗██║  ██║".Pastel(Color.Red)}
  {"╚═╝  ╚═══╝╚══════╝ ╚═════╝ ╚═╝     ╚══════╝   ╚═╝    ╚═════╝╚═╝  ╚═╝".Pastel(Color.White)}
                   [{"https://github.com/DeetonRushy".Pastel(Color.LightBlue)}]                                                                    
";
var AsciiArtFmt = $@"
                               {{0}}
  {"███╗   ██╗███████╗ ██████╗ ███████╗███████╗████████╗ ██████╗██╗  ██╗".Pastel(Color.Red)}
  {"████╗  ██║██╔════╝██╔═══██╗██╔════╝██╔════╝╚══██╔══╝██╔════╝██║  ██║".Pastel(Color.White)}
  {"██╔██╗ ██║█████╗  ██║   ██║█████╗  █████╗     ██║   ██║     ███████║".Pastel(Color.Red)} windows edition
  {"██║╚██╗██║██╔══╝  ██║   ██║██╔══╝  ██╔══╝     ██║   ██║     ██╔══██║".Pastel(Color.White)}    by deeton rushton
  {"██║ ╚████║███████╗╚██████╔╝██║     ███████╗   ██║   ╚██████╗██║  ██║".Pastel(Color.Red)}
  {"╚═╝  ╚═══╝╚══════╝ ╚═════╝ ╚═╝     ╚══════╝   ╚═╝    ╚═════╝╚═╝  ╚═╝".Pastel(Color.White)}
                   [{"https://github.com/DeetonRushy".Pastel(Color.LightBlue)}]                                                                    
";

if (SupportedColours.ContainsKey(App.Instance.Color))
{
    App.Instance.SelectedColor = SupportedColours[App.Instance.Color];
}
else
{
    VerboseLog($"color '{App.Instance.Color}' is not recognized. (choose out of {string.Join(", ", SupportedColours.Keys)})\n");
    App.Instance.SelectedColor = Color.Red;
}

VerboseLog($"fetching specs...");
var specs = FetchSpecs(!App.Instance.ResetSpecs);

if (App.Instance.PauseAfterVerboseLogs && App.Instance.Verbose)
{
    VerboseLog($"[-L] press any key to continue...");
    App.Instance.ApplicationTimer?.Stop();
    Console.ReadKey();
    App.Instance.ApplicationTimer?.Start();
}

VerboseAction(Console.Clear);
Console.Write(AsciiArt);

var user = $"{Environment.UserName.Pastel(App.Instance.SelectedColor)}{"@".Pastel(Color.White)}{Environment.MachineName.Pastel(App.Instance.SelectedColor)}";

if (App.Instance.LiveMode)
{
    App.Instance.ResetSpecs = true;
    Console.Title = "neofetch: Live";

    while (true)
    {
        Console.Write(String.Format(AsciiArtFmt, DateTime.Now.ToLongTimeString().Pastel(Color.White)));
        Console.WriteLine("\n\n" + user);
        Console.WriteLine(DashLineL(Environment.UserName.Length + Environment.MachineName.Length + 1));
        WriteSpec("OS", specs.OsDescription);
        WriteSpec("Host", specs.HostDescription);
        WriteSpec("Kernel", specs.Kernel);
        WriteSpec("Uptime", specs.Uptime);
        WriteSpec("Shell", SpecList.Shell);
        WriteSpec("Resolution", specs.ScreenResolution);
        WriteSpec("Packages", specs.PackageCount);

        foreach (var cpuSpec in specs.CpuDescriptions)
        {
            WriteSpec("CPU", cpuSpec);
        }
        foreach (var gpuSpec in specs.GpuDescriptions)
        {
            WriteSpec("GPU", gpuSpec);
        }

        WriteSpec("Memory", specs.MemoryCapacity);

        Console.Write("\n");
        WriteColorSquares();
        specs = FetchSpecs(false);
        Console.Clear();
    }
}
Console.WriteLine("\n" + user);
Console.WriteLine(DashLineL(Environment.UserName.Length + Environment.MachineName.Length + 1));

if (App.Instance.SpecificSpecs is not null && App.Instance.SpecificSpecs.Any())
{
    foreach (var spec in App.Instance.SpecificSpecs)
    {
        if (!SupportedActions.ContainsKey(spec))
        {
            VerboseLog($"cannot get spec '{spec}'. (available specs are '{string.Join(", ", SupportedActions.Keys)}')");
            break;
        }

        SupportedActions[spec]();
    }

    Console.Write("\n");
    WriteColorSquares();

    Console.Write("\n\n");
    Environment.Exit(0);
}

WriteSpec("OS", specs.OsDescription);
WriteSpec("Host", specs.HostDescription);
WriteSpec("Kernel", specs.Kernel);
WriteSpec("Uptime", specs.Uptime);
WriteSpec("Shell", SpecList.Shell);
WriteSpec("Packages", specs.PackageCount);
WriteSpec("Resolution", specs.ScreenResolution);

foreach (var cpuSpec in specs.CpuDescriptions)
{
    WriteSpec("CPU", cpuSpec);
}
foreach (var gpuSpec in specs.GpuDescriptions)
{
    WriteSpec("GPU", gpuSpec);
}

WriteSpec("MemoryStats", specs.MemoryCapacity);

Console.Write("\n");
WriteColorSquares();

Console.Write("\n\n");

[DllImport("kernel32.dll")]
extern static ulong GetTickCount64();

static unsafe string GetResolution()
{
    var width = Screen.PrimaryScreen.Bounds.Width;
    var height = Screen.PrimaryScreen.Bounds.Height;

    return $"{width}x{height}";
}

static void WriteSpec(string specName, string specDescription)
{
    Console.WriteLine($"{specName.Pastel(App.Instance.SelectedColor)}: {specDescription.Pastel(Color.White)}");
}
static string DashLineL(int sz)
{
    string result = "";

    for (int i = 0; i < sz; i++)
        result += '-';

    return result;
}

static SpecList FetchSpecs(bool useSaved)
{
    string fileName = "saved-specs.json";

    if (useSaved && File.Exists(fileName))
    {
        var contents = File.ReadAllText(fileName);
        var specs = JsonConvert.DeserializeObject<SpecList>(contents);
        VerboseLog($"read cached specs from '{fileName}'");
        specs!.Uptime = GetUptime();
        VerboseLog($"found '{"Uptime".Pastel(Color.Green)}'");
        specs!.MemoryCapacity = GetMemory();
        VerboseLog($"found '{"MemoryUsage".Pastel(Color.Green)}'");

        if (specs != null)
            return specs;
    }

    SpecList result = new();

    result.OsDescription = RuntimeInformation.OSDescription;
    VerboseLog($"\n\nfound '{"OsDescription".Pastel(Color.Green)}' [\"{result.OsDescription}\"]");
    result.HostDescription = Environment.UserDomainName;
    VerboseLog($"found '{"HostDescription".Pastel(Color.Green)}' [\"{result.HostDescription}\"]");
    result.Kernel = GetOSInfo() + $" ({RuntimeInformation.RuntimeIdentifier})";
    VerboseLog($"found '{"Kernel".Pastel(Color.Green)}' [\"{result.Kernel}\"]");
    result.Uptime = GetUptime();
    VerboseLog($"found '{"Uptime".Pastel(Color.Green)}' [\"{result.Uptime}\"]");

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        using var searcher = new ManagementObjectSearcher("select * from Win32_Processor");

        foreach (ManagementObject obj in searcher.Get())
        {
            var clockSpeed = obj["CurrentClockSpeed"].ToString();
            var procName = obj["Name"].ToString();
            var manufacturer = obj["Manufacturer"].ToString();
            var version = obj["Version"].ToString();

            result.CpuDescriptions.Add($"{procName} {version}");
        }
    }
    else
    {
        result.CpuDescriptions?.Add("<Platform-Not-Supported>");
    }
    VerboseLog($"found '{"Processors".Pastel(Color.Green)}' [\"{string.Join(", ", result.CpuDescriptions!)}\"]");
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");

        foreach (ManagementObject obj in searcher.Get())
        {
            result.GpuDescriptions?.Add(obj["Name"].ToString()!);
        }
    }
    else
    {
        result.GpuDescriptions?.Add("<Platform-Not-Supported>");
    }
    VerboseLog($"found '{"GraphicCards".Pastel(Color.Green)}' [\"{string.Join(", ", result.GpuDescriptions!)}\"]");
    if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows))
    {
        result.MemoryCapacity = GetMemory();
    }
    else
    {
        result.MemoryCapacity = "<Platform-Not-Supported>";
    }
    VerboseLog($"found '{"MemoryUsage".Pastel(Color.Green)}' [\"{result.MemoryCapacity}\"]");

    result.PackageCount = GetPackageCount();
    VerboseLog($"found '{"Packages".Pastel(Color.Green)}' [\"{result.PackageCount}\"]");
    result.ScreenResolution = GetResolution();
    VerboseLog($"found '{"Resolution".Pastel(Color.Green)}' [\"{result.ScreenResolution}\"]");

    var converted = JsonConvert.SerializeObject(result, Formatting.Indented);

    if (!File.Exists(fileName))
        File.Create(fileName).Close();

    File.WriteAllText(fileName, converted);

    return result;
}

static string GetUptime()
{
    var span = TimeSpan.FromMilliseconds(GetTickCount64());
    StringBuilder builder = new();

    if (span.Days != 0)
        builder.Append($"{span.Days} days, ");
    if (span.Hours != 0)
        builder.Append($"{span.Hours} hours, ");
    if (span.Minutes != 0)
        builder.Append($"{span.Minutes} mins, ");
    if (span.Seconds != 0)
        builder.Append($"{span.Seconds} secs");
    return builder.ToString();
}
static string GetMemory()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return "<Platform-Not-Supported>";

    double available = 0, capacity = 0;
    using var searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");

    foreach (ManagementObject obj in searcher.Get())
    {
        capacity += Math.Round(long.Parse(obj.Properties["Capacity"].Value.ToString()!) / 1024 / 1024.0, 1);
    }
    using ManagementClass cimobject2 = new("Win32_PerfFormattedData_PerfOS_Memory");
    ManagementObjectCollection moc2 = cimobject2.GetInstances();
    foreach (ManagementObject mo2 in moc2)
    {
        available += Math.Round(Int64.Parse(mo2.Properties["AvailableMBytes"].Value.ToString()!) / 1024.0, 1);
    }

    var free = Math.Round(((capacity / 1024) - available) * 1024.0, 1);

    return $"{free}MiB / {capacity}MiB";
}
static string GetOSInfo()
{
    //Get Operating system information.
    OperatingSystem os = Environment.OSVersion;
    //Get version information about the os.
    Version vs = os.Version;

    //Variable to hold our return value
    string operatingSystem = "";

    if (os.Platform == PlatformID.Win32Windows)
    {
        //This is a pre-NT version of Windows
        switch (vs.Minor)
        {
            case 0:
                operatingSystem = "95";
                break;
            case 10:
                if (vs.Revision.ToString() == "2222A")
                    operatingSystem = "98SE";
                else
                    operatingSystem = "98";
                break;
            case 90:
                operatingSystem = "Me";
                break;
            default:
                break;
        }
    }
    else if (os.Platform == PlatformID.Win32NT)
    {
        switch (vs.Major)
        {
            case 3:
                operatingSystem = "NT 3.51";
                break;
            case 4:
                operatingSystem = "NT 4.0";
                break;
            case 5:
                if (vs.Minor == 0)
                    operatingSystem = "2000";
                else
                    operatingSystem = "XP";
                break;
            case 6:
                if (vs.Minor == 0)
                    operatingSystem = "Vista";
                else if (vs.Minor == 1)
                    operatingSystem = "7";
                else if (vs.Minor == 2)
                    operatingSystem = "8";
                else
                    operatingSystem = "8.1";
                break;
            case 10:
                operatingSystem = "10";
                break;
            default:
                break;
        }
    }
    //Make sure we actually got something in our OS check
    //We don't want to just return " Service Pack 2" or " 32-bit"
    //That information is useless without the OS version.
    if (operatingSystem != "")
    {
        //Got something.  Let's prepend "Windows" and get more info.
        operatingSystem = "Windows " + operatingSystem;
        //See if there's a service pack installed.
        if (os.ServicePack != "")
        {
            //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
            operatingSystem += " " + os.ServicePack;
        }
        //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
        //operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
    }
    //Return the information we've gathered.
    return operatingSystem;
}
static string GetPackageCount()
{
    int installedPackages = 0;
    string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
    {
        foreach (string skName in rk.GetSubKeyNames())
        {
            installedPackages++;
        }
    }

    return installedPackages.ToString();
}
static void WriteColorSquares()
{
    string ch = "█";
    int cursor = 0;

    // add any colours and they'll be auto added
    Color[] colors = new[]
    {
        Color.Black,
        Color.Red,
        Color.Lime,
        Color.Yellow,
        Color.LightSkyBlue,
        Color.Purple,
        Color.Orange,
        Color.White
    };

    // 6 squares, 3 chars each, 9 chars total, 3 lines.

    for ( int q = 0; q < 2; q++ )
    {
        cursor = 0;
        Console.Write("\n");

        // colors.Length * 3 = colors.Length squares with a width of 3
        for (int i = 0; i < colors.Length * 3; i++)
        {
            // switch color every 3 characters written
            if (i % 3 == 0)
                cursor++;
            // break once we've done all colors
            if (cursor == colors.Length)
                break;
            Console.Write($"{ch.Pastel(colors[cursor])}");
        }
    }
}
static void VerboseLog(string message)
{
    if (App.Instance.Verbose)
    {
        Console.WriteLine(message);
    }
}
static void VerboseAction(Action act)
{
    if (App.Instance.Verbose)
        act();
}

public class SpecList
{
    public const string UnknownSpec = "<unknown>";

    [JsonProperty]
    public string OsDescription { get; set; } = UnknownSpec;
    [JsonProperty]
    public string HostDescription { get; set; } = UnknownSpec;
    [JsonProperty]
    public string Kernel { get; set; } = UnknownSpec;
    [JsonProperty]
    public string Uptime { get; set; } = UnknownSpec;
    public static string Shell
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "<Windows-Builtin>";
            }

            return "<Platform-Not-Supported>";
        }
    }
    [JsonProperty]
    public List<string> CpuDescriptions { get; set; } = new();
    [JsonProperty]
    public List<string> GpuDescriptions { get; set; } = new();
    [JsonProperty]
    public string MemoryCapacity { get; set; } = UnknownSpec;
    [JsonProperty]
    public string PackageCount { get; set; } = UnknownSpec;
    [JsonProperty]
    public string ScreenResolution { get; set; } = UnknownSpec;
}

public class App
{
    [Option('v', "verbose", HelpText = "enable verbose output")]
    public bool Verbose { get; set; } = false;

    [Option('c', "color", HelpText = "select a color. (ex: \"Red\")", Default = "Red")]
    public string Color { get; set; } = "Red";
    [Option('r', "reset", HelpText = "reload specs instead of reading cached data. (will take longer due to WMI)", Default = false)]
    public bool ResetSpecs { get; set; } = false;
    [Option('L', "let-me-read", HelpText = "ask for a prompt after all verbose output, so you can read the content", Default = false)]
    public bool PauseAfterVerboseLogs { get; set; } = false;
    [Option('l', "live", HelpText = "live mode will refresh the neofetch output every 2s. press [Ctrl+C] to exit this mode.", Default = false)]
    public bool LiveMode { get; set; } = false;
    [Option('t', "time", HelpText = "display how long the application took to run. (does not work with live)", Default = false)]
    public bool TimeApp { get; set; } = false;

    [Option('s', "specific", HelpText = "choose what specs to write. enable verbose mode for a list of items.", Default = null)]
    public IEnumerable<string>? SpecificSpecs { get; set; } = null;
    public Color SelectedColor { get; set; }
    public Stopwatch? ApplicationTimer { get; set; } = null;
    public static App? Instance { get; set; }
}
