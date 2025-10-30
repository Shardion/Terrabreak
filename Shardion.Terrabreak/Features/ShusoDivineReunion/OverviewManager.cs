using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NetCord.JsonModels;
using Serilog;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public class OverviewManager : ITerrabreakFeature
{
    public JsonComponent? SdrOverview { get; set; } = null;

    public async Task StartAsync()
    {
        await LoadSdrOverviewAsync();
    }

    public async Task LoadSdrOverviewAsync()
    {
        string userGuideJsonFilename =
            Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "documentation/sdr-overview.json");
        FileStream userGuideJsonStream;
        try
        {
            userGuideJsonStream = File.OpenRead(userGuideJsonFilename);
        }
        catch (FileNotFoundException)
        {
            Log.Information(
                $"SDR overview file expected at `{userGuideJsonFilename}`, but it does not exist. Not loading SDR overview.");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            Log.Information(
                $"Documentation directory not found while trying to read `{userGuideJsonFilename}`. Not loading SDR overview.");
            return;
        }
        catch (PathTooLongException)
        {
            Log.Error("SDR overview file path is too long. Not loading SDR overview.");
            return;
        }
        catch (UnauthorizedAccessException)
        {
            Log.Error(
                $"SDR overview file expected at `{userGuideJsonFilename}`, but path refers to a directory. Not loading SDR overview.");
            return;
        }

        await using (userGuideJsonStream)
        {
            JsonComponent? loadedUserGuideMessage =
                await JsonSerializer.DeserializeAsync<JsonComponent>(userGuideJsonStream);
            if (loadedUserGuideMessage is null)
            {
                Log.Error(
                    $"SDR overview file at `{userGuideJsonFilename}` failed to deserialize. Not loading SDR overview.");
                return;
            }

            SdrOverview = loadedUserGuideMessage;
        }
    }
}
