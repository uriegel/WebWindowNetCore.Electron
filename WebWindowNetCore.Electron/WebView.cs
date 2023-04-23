using System.Diagnostics;
using System.Text.Json;
using CsTools.Extensions;
using LinqTools;
using WebWindowNetCore.Base;
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
        using var file = File.OpenWrite(mainjs);
        mainjsStream?.CopyTo(file);        

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
        file.Flush();

        electron.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        electron.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
        electron.StartInfo.Environment.Add("StartInfo", JsonSerializer.Serialize(new StartInfo(settings!.Title), JsonDefault.Value));
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
    string Title
);