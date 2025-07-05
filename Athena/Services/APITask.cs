using Athena.Core;
using Athena.Models.API.Fortnite;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace Athena.Services;

public class APITask
{
    private const int TASK_COOLDOWN = 5 * 1000;

    public static async Task<List<DynamicKey>> GetNewDynamicKeys()
    {
        Log.ForContext("NoConsole", true).Information(
            "APITask->GetNewDynamicKeys(): Starting. Cooldown set to {cd}ms", TASK_COOLDOWN);

        List<DynamicKey> newKeys;
        while (true)
        {
            Log.Information("Checking for new AES keys.");
            await Task.Delay(TASK_COOLDOWN); // waits TASK_COOLDOWN milliseconds

            var res = await APEndpoints.Instance.FortniteCentral.GetAESKeysAsync(false);
            if (res is null || res.DynamicKeys.Count == 0) continue;

            newKeys = res.DynamicKeys
                .Where(key => !Dataminer.Instance.AESKeys?.GuidsList.Contains(new FGuid(key.Guid)) ?? true)
                .ToList();

            if (newKeys.Count == 0)
                continue;

            Log.Information("Detected {tot} new Dynamic Keys!", newKeys.Count);

            Dataminer.Instance.AESKeys = res;
            return newKeys;
        }
    }
}