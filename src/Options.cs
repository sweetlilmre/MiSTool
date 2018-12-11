using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace MiSTer_BootSharp
{
  public class BaseOptions
  {
    [Option('o', "output", HelpText = "Output path for cores", Default = "mister")]
    public string Path { get; set; }

    [Option('v', "verbose", HelpText = "Display verbose output", Default = false)]
    public bool Verbose { get; set; }
  }

  [Verb("update", HelpText = "Update the repo and download cores")]
  public class UpdateOptions : BaseOptions
  {
    [Usage()]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Update scenario", new UpdateOptions { Key = "aabbccddeeff00112233445566778899" });
      }
    }
    [Option('k', "key", Required = true, HelpText = "Github API Key")]
    public string Key { get; set; }
  }

  [Verb("download", HelpText = "Read repo and download cores")]
  public class DownloadOptions : BaseOptions
  {
    [Usage()]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Download scenario", new DownloadOptions { Path = "my-cores" });
      }
    }
  }

  [Verb("fetch", HelpText = "Fetch the repo from a URL, update and download cores")]
  public class FetchOptions : BaseOptions
  {
    [Usage()]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Fetch scenario", new FetchOptions { URL = "https://raw.githubusercontent.com/OpenVGS/MiSTer-repository/master/repo.json" });
      }
    }

    [Option('u', "url", Required = true, HelpText = "URL to repo.json file")]
    public string URL { get; set; }
  }
}
