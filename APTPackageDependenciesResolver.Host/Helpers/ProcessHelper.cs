using System.Diagnostics;
using System.Text;

public static class ProcessHelper
{
    /// <summary>
    /// Execute the command and save standard out result as string
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>

    private static (int ExitCode, string Result) ExecuteCommand(params ReadOnlySpan<string> args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Must have at least 1 item in args for execution program", nameof(args));
        }

        var process = new Process();
        
        process.StartInfo.FileName = args[0];
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        
        var argsList = process.StartInfo.ArgumentList;
        foreach (var arg in args.Slice(1))
        {
            argsList.Add(arg);
        }

        bool processStarted = process.Start();

        if (!processStarted)
        {
            throw new InvalidOperationException($"Unable to execute program: {string.Join(' ', args)}");
        }

        Span<char> buffer = new char[8192];
        
        var stringBuilder = new StringBuilder();

        var stdoutStreamReader = new StreamReader(process.StandardOutput.BaseStream);
        int read;

        while ((read = stdoutStreamReader.ReadBlock(buffer)) != 0)
        {
            stringBuilder.Append(buffer.Slice(0, read));
        }

        process.WaitForExit();

        return (process.ExitCode, stringBuilder.ToString());
    }
}