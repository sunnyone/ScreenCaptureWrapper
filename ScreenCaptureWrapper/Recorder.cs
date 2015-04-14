using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenCaptureWrapper
{
    public class RecordParam
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public string OutputPath { get; set; }

        public void SetToCottleStore(Cottle.IStore store)
        {
            store["width"] = this.Width;
            store["height"] = this.Height;
            store["left"] = this.Left;
            store["top"] = this.Top;
            store["output_path"] = this.OutputPath;
        }
    }

    public class Recorder
    {
        public static Task Record(string ffmpegPath, string ffmpegArguments, RecordParam recordParam, CancellationToken cancellationToken, IProgress<string> logProgress)
        {
            return Task.Run(() =>
            {
                // TODO: check more?
                if (recordParam.OutputPath.IndexOf('"') >= 0)
                {
                    throw new ArgumentException("OutputPath contains '\"'");
                }

                if (System.IO.File.Exists(recordParam.OutputPath))
                {
                    logProgress.Report("Deleting the file.");
                    System.IO.File.Delete(recordParam.OutputPath);
                }

                var argumentTemplate = ffmpegArguments;

                var simpleDocument = new Cottle.Documents.SimpleDocument(argumentTemplate);
                var store = new Cottle.Stores.BuiltinStore();
                recordParam.SetToCottleStore(store);
                var arguments = simpleDocument.Render(store);

                string startLine = string.Format("Start: {0} {1}", ffmpegPath, arguments);
                logProgress.Report(startLine);

                var psi = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var proc = new System.Diagnostics.Process()
                {
                    StartInfo = psi,
                };

                proc.OutputDataReceived += (sender, e) =>
                {
                    logProgress.Report(e.Data);
                };
                proc.ErrorDataReceived += (sender, e) =>
                {
                    logProgress.Report(e.Data);
                };

                // TODO: wait process with modern non-blocking way
                var mre = new ManualResetEventSlim();
                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, e) =>
                {
                    mre.Set();
                };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                WaitHandle.WaitAny(new WaitHandle[] { mre.WaitHandle, cancellationToken.WaitHandle });
                if (cancellationToken.IsCancellationRequested) {
                    if (!proc.HasExited) {
                        var io = proc.StandardInput;
                        io.WriteLine("q");
                    }

                    logProgress.Report("Cancellation requested. Pressed q and waiting...");
                    var waitResult = mre.Wait(10000);
                    if (waitResult) 
                    {
                        logProgress.Report("The process finished.");
                        return;
                    } 
                    else 
                    {
                        logProgress.Report("The process is not finished. Killing it.");
                        proc.Kill();
                        return;
                    }
                }

                logProgress.Report(string.Format("Process finished: exit code: {0}", proc.ExitCode));
            });
        }

    }
}
