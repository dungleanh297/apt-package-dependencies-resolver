using System.Diagnostics;

public class ProcessWithStandardOutput : IDisposable
{
    private readonly Process _process;
    private bool _hasStarted;
    private Stream? _standardOutputStream;

    public int ExitCode => _process.ExitCode;

    public ProcessWithStandardOutput(params ReadOnlySpan<string> args)
    {
        _process = new Process();
        _process.StartInfo.FileName = args[0];
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.RedirectStandardOutput = true;

        var argsList = _process.StartInfo.ArgumentList;
        foreach (var arg in argsList)
        {
            argsList.Add(arg);
        }
    }

    public void Start()
    {
        if (_hasStarted)
        {
            throw new InvalidOperationException("Process has been started already");
        }

        _hasStarted = true;
        _process.Start();
    }

    public Stream OpenStandardOutputStream()
    {
        if (!_hasStarted)
        {
            throw new InvalidOperationException("Process is not started");
        }

        if (_standardOutputStream is null)
        {
            _standardOutputStream = _process.StandardOutput.BaseStream;
        }

        return _standardOutputStream;
    }

    public void Dispose()
    {
        _standardOutputStream?.Close();
        _process.WaitForExit();
    }
}