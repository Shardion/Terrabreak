using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NetCord.JsonModels;
using Serilog;

namespace Shardion.Terrabreak.Features.Documentation;

public class DocumentationManager : ITerrabreakFeature
{
    public ConcurrentDictionary<string, JsonComponent> Changelogs { get; } = [];
    public JsonComponent? UserGuide { get; set; } = null;

    public async Task StartAsync()
    {
        await Task.WhenAll(LoadChangelogsAsync(), LoadUserGuideAsync());
    }

    public async Task LoadChangelogsAsync()
    {
        string changelogDir = Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "documentation/changelogs");
        IEnumerable<string> changelogDirFiles;
        try
        {
            changelogDirFiles = Directory.EnumerateFiles(changelogDir);
        }
        catch (DirectoryNotFoundException)
        {
            Log.Information($"Changelogs directory not found at `{changelogDir}/`. Not loading changelogs.");
            return;
        }
        catch (PathTooLongException)
        {
            Log.Error(
                "Changelogs directory path is too long. Not loading changelogs.");
            return;
        }
        catch (IOException)
        {
            Log.Error(
                $"Changelogs directory expected at `{changelogDir}/`, but path refers to a file. Not loading changelogs.");
            return;
        }

        IEnumerable<string> changelogDirJsonFiles = changelogDirFiles.Where(file => file.EndsWith(".json"));
        await Parallel.ForEachAsync(changelogDirJsonFiles, async (changelogFilename, token) =>
        {
            await using FileStream changelogStream = File.OpenRead(changelogFilename);
            JsonComponent? loadedChangelogMessage =
                await JsonSerializer.DeserializeAsync<JsonComponent>(changelogStream, cancellationToken: token);
            if (loadedChangelogMessage is null)
            {
                Log.Error($"Changelog file `{changelogFilename}` failed to deserialize. Not loading.");
                return;
            }

            Changelogs[Path.GetFileName(changelogFilename).Replace(".json", "")] = loadedChangelogMessage;
        });
    }

    public async Task LoadUserGuideAsync()
    {
        string userGuideJsonFilename =
            Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "documentation/user-guide.json");
        FileStream userGuideJsonStream;
        try
        {
            userGuideJsonStream = File.OpenRead(userGuideJsonFilename);
        }
        catch (FileNotFoundException)
        {
            Log.Information(
                $"User guide file expected at `{userGuideJsonFilename}`, but it does not exist. Not loading user guide.");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            Log.Information(
                $"Documentation directory not found while trying to read `{userGuideJsonFilename}`. Not loading user guide.");
            return;
        }
        catch (PathTooLongException)
        {
            Log.Error("User guide file path is too long. Not loading user guide.");
            return;
        }
        catch (UnauthorizedAccessException)
        {
            Log.Error(
                $"User guide file expected at `{userGuideJsonFilename}`, but path refers to a directory. Not loading user guide.");
            return;
        }

        await using (userGuideJsonStream)
        {
            JsonComponent? loadedUserGuideMessage =
                await JsonSerializer.DeserializeAsync<JsonComponent>(userGuideJsonStream);
            if (loadedUserGuideMessage is null)
            {
                Log.Error(
                    $"User guide file at `{userGuideJsonFilename}` failed to deserialize. Not loading user guide.");
                return;
            }

            UserGuide = loadedUserGuideMessage;
        }
    }
}
