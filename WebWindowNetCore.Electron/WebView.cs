using System.Diagnostics;
using System.Text.Json;
using CsTools.Extensions;
using LinqTools;
using WebWindowNetCore.Data;

namespace WebWindowNetCore;

public class WebView : WebWindowNetCore.Base.WebView
{
    public static WebViewBuilder Create()
        => new WebViewBuilder();

    public override int Run(string gtkId)
    {
        var path = Environment
                        .GetFolderPath(Environment.SpecialFolder.Personal)
                        .AppendPath(".config")
                        .AppendPath(gtkId
                                    .WhiteSpaceToNull()
                                    .GetOrDefault("de.uriegel.webwindownetcore.electron"));
        Directory.CreateDirectory(path);
        var mainjs = path.AppendPath("mainjs.js");
        var mainjsStream = System.Reflection.Assembly
            .GetExecutingAssembly()
            ?.GetManifestResourceStream("MainElectron");
        using var file = File.Create(mainjs);
        mainjsStream?.CopyTo(file);
        file.Flush();

        var preloadjs = path.AppendPath("preload.js");
        var preloadjsStream = System.Reflection.Assembly
            .GetExecutingAssembly()
            ?.GetManifestResourceStream("PreloadElectron");
        using var preloadFile = File.Create(preloadjs);
        preloadjsStream?.CopyTo(preloadFile);
        preloadFile.Flush();

        string? iconfilename = null;
        if (settings?.ResourceIcon != null)
        {
#if Linux                
            iconfilename = path.AppendPath("icon.png");
#else
            iconfilename = path.AppendPath("icon.ico");
#endif
            var icon = Resources.Get(settings.ResourceIcon!);
            using var iconfile = File.Create(iconfilename);
            icon?.CopyTo(iconfile);
            iconfile.Flush();
        }

        var url = Debugger.IsAttached && !string.IsNullOrEmpty(settings?.DebugUrl)
            ? settings?.DebugUrl
            : settings?.Url != null
            ? settings.Url
            : $"http://localhost:{settings?.HttpSettings?.Port ?? 80}{settings?.HttpSettings?.WebrootUrl}/{settings?.HttpSettings?.DefaultHtml}";

        var electron = new Process()
        {
            StartInfo = new()
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
#if Linux                
                FileName = "electron",
#else
                FileName = "electron.cmd", 
#endif
                CreateNoWindow = true,
                Arguments = mainjs,
        },
            EnableRaisingEvents = true
        };

        electron.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        electron.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
        electron.StartInfo.Environment.Add("StartInfo", JsonSerializer.Serialize(new StartInfo(
                settings!.Title,
                url!,
                iconfilename,
                settings?.DevTools == true,
                settings?.SaveBounds == true

            ), JsonDefault.Value));
        electron.Start();
        electron.BeginOutputReadLine();
        electron.BeginErrorReadLine();
        electron.EnableRaisingEvents = true;
        electron.WaitForExit();
        return electron.ExitCode;
    }

    internal WebView(WebViewBuilder builder)
        => settings = builder.Data;

    WebViewSettings? settings;

    bool saveBounds;
}

record StartInfo(
    string Title,
    string Url,
    string? IconPath,
    bool ShowDevTools,
    bool SaveBounds
);