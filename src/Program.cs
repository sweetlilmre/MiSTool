using CommandLine;

namespace MiSTer_BootSharp
{
  class Program
  {
    //a210bedfeea4c789c1833179c230ee5c92a6c353
    static int Main(string[] args)
    {
      CoreTools p = new CoreTools();
      return Parser.Default.ParseArguments<UpdateOptions, DownloadOptions, FetchOptions>(args)
        .MapResult(
          (UpdateOptions opts) => p.RunUpdateAndReturnExitCode(opts),
          (DownloadOptions opts) => p.RunDownloadAndReturnExitCode(opts),
          (FetchOptions opts) => p.RunFetchAndReturnExitCode(opts),
          errs => 1);
    }
  }
}
