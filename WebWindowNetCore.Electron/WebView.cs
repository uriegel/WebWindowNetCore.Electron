using System.Diagnostics;
using WebWindowNetCore.Base;
using WebWindowNetCore.Data;

namespace WebWindowNetCore;

public class WebView : WebWindowNetCore.Base.WebView
{
    public static WebViewBuilder Create()
        => new WebViewBuilder();

    public override int Run(string gtkId = "")
    {
        var electron = new Process()
        {
            StartInfo = new()
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "electron", // TODO electron.cmd Windows
                CreateNoWindow = true,
                Arguments = null,
            },
            EnableRaisingEvents = true
        };

        electron.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        electron.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
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
