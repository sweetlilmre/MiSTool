using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MiSTer_BootSharp
{
  public class CoreTools
  {
    const string REPO_DB_FILE = "repo.json";
    const string GITHUB_ORG = "MiSTer-devel";
    const string RELEASE_PATH = "releases";
    const string PRODUCT_NAME = "MiSTer-BootSharp";

    public CoreTools()
    {
    }

    public int RunUpdateAndReturnExitCode(UpdateOptions opts)
    {
      var client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));
      var tokenAuth = new Credentials(opts.Key);
      client.Credentials = tokenAuth;

      var options = new ApiOptions
      {
        PageSize = 500
      };


      Console.WriteLine("Searching for cores...");
      var repositories = client.Repository.GetAllForOrg(GITHUB_ORG, options).Result;
      List<CoreData> repoDB = new List<CoreData>();

      int top = Console.CursorTop;
      foreach (var repo in repositories)
      {
        Console.CursorTop = top;
        WriteCleanLine("Inspecting repo: {0}", repo.Name);
        try
        {
          var contentList = client.Repository.Content.GetAllContents(GITHUB_ORG, repo.Name, RELEASE_PATH).Result;
          var core = contentList.Where(t => t.Name.EndsWith(".rbf")).OrderBy(t => t.Name).Reverse().FirstOrDefault();
          if (core != null)
          {
            WriteCleanLine("\t found core: {0}.", core.Name);
            repoDB.Add(new CoreData { Name = repo.Name, URL = core.DownloadUrl, File = core.Name });
          }
          else
          {
            WriteCleanLine("\t no cores found.");
          }
        }
        catch (Exception e)
        {
          WriteCleanLine("\t no releases found.");
        }
      }

      var serializerSettings = new JsonSerializerSettings();
      serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      string db = JsonConvert.SerializeObject(repoDB, Formatting.Indented, serializerSettings);
      File.WriteAllText(REPO_DB_FILE, db);
      DownloadCores(repoDB, opts);
      return (0);
    }

    public int RunDownloadAndReturnExitCode(DownloadOptions opts)
    {
      DownloadCores(LoadRepoDB(opts), opts);
      return (0);
    }

    public int RunFetchAndReturnExitCode(FetchOptions opts)
    {
      Console.Write("Fetching repository... ");
      try
      {
        using (var client = new WebClient())
        {
          client.DownloadFile(opts.URL, REPO_DB_FILE);
          Console.WriteLine("success!");
          DownloadCores(LoadRepoDB(opts), opts);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("failed!");
        if (opts.Verbose)
        {
          Console.WriteLine(e.Message);
        }
      }
      return 0;
    }


    List<CoreData> LoadRepoDB(BaseOptions opts)
    {
      List<CoreData> oldRepoDB = new List<CoreData>();
      Console.Write("Loading repository... ");
      try
      {
        if (File.Exists(REPO_DB_FILE))
        {
          string db = File.ReadAllText(REPO_DB_FILE);
          oldRepoDB = JsonConvert.DeserializeObject<List<CoreData>>(db);
          Console.WriteLine("done!");
        }
        else
        {
          Console.WriteLine("no repository found!");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("FAILED!");
        if (opts.Verbose)
        {
          Console.WriteLine(e.Message);
        }

      }
      return oldRepoDB;
    }

    public void WriteClean(string str, params object[] parms)
    {
      int currentLineCursor = Console.CursorTop;
      Console.SetCursorPosition(0, Console.CursorTop);
      Console.Write(new string(' ', Console.WindowWidth));
      Console.SetCursorPosition(0, currentLineCursor);
      Console.Write(str, parms);
    }
    public void WriteCleanLine(string str, params object[] parms)
    {
      WriteClean(str, parms);
      Console.WriteLine();
    }

    private void DownloadCores(List<CoreData> repoDB, BaseOptions opts)
    {
      if (!Directory.Exists(opts.Path))
      {
        Directory.CreateDirectory(opts.Path);
      }

      Console.WriteLine("Starting downloads...");
      Console.WriteLine("{0} cores to download / update", repoDB.Count);
      int top = Console.CursorTop;

      foreach (var core in repoDB)
      {
        string coreFile = Path.Combine(opts.Path, core.File);
        Console.CursorTop = top;
        WriteCleanLine("Core: {0}", core.Name);

        if (!File.Exists(coreFile))
        {
          WriteClean("\t downloading... ");
          try
          {
            using (var client = new WebClient())
            {
              client.DownloadFile(core.URL, coreFile);
              Console.WriteLine("success!");
            }
          }
          catch (Exception e)
          {
            Console.WriteLine("failed!");
          }
        }
        else
        {
          Console.WriteLine("\t already exists");
        }
      }
      Console.WriteLine("Downloads complete.");
    }
  }
}
