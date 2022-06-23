using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ViLA.PluginBase;

namespace ViLA.Extensions.EliteDangerous;

public class EliteStatus
{
    public string Timestamp { get; set; }
    public string Event { get; set; }
    public int Flags { get; set; }
}


public class EliteDangerous : PluginBase.PluginBase, IDisposable
    {
    /// <summary>
    /// ConfigPath is relative to ViLA, *not* to this dll
    /// </summary>

    public const string ConfigPath = "Plugins/EliteDangerous/config.json";

    private Task _thread = null;

    public override async Task<bool> Start()
    {
        var log = LoggerFactory.CreateLogger<EliteDangerous>();
        log.LogInformation("Starting Elite Dangerous Plugin");

        var cfg = await GetConfiguration();

        _thread = Task.Run(() => RunMethod(cfg.DelayMs, default), default);
        return true;
    }

    public override Task Stop()
    {
        return Task.CompletedTask;
    }

    private static async Task<PluginConfiguration> GetConfiguration()
    {
        PluginConfiguration? pluginConfig = null;
        var getAndWriteDefaultConfig = true;

        if (File.Exists(ConfigPath))
        {
            getAndWriteDefaultConfig = false;

            var configString = await File.ReadAllTextAsync(ConfigPath);
            pluginConfig = JsonConvert.DeserializeObject<PluginConfiguration>(configString);

            if (pluginConfig is null)
            {
                getAndWriteDefaultConfig = true;
            }
        }

        if (getAndWriteDefaultConfig)
        {
            pluginConfig = PluginConfiguration.Default();
            await File.WriteAllTextAsync(ConfigPath, JsonConvert.SerializeObject(pluginConfig));
        }

        return pluginConfig;
    }

    private void RunMethod(int delay, CancellationToken ct)
    {

        while (!ct.IsCancellationRequested)
        {

            StreamReader r = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Saved Games\Frontier Developments\Elite Dangerous\Status.json");
            string json = r.ReadToEnd();
            r.Close();

            EliteStatus Status = JsonConvert.DeserializeObject<EliteStatus>(json);

            int data = Status.Flags;

            bool[] bits = new bool[32];

            for (int i = 0; i < 32; i++, data >>= 1)
            {
                bits[i] = (data & 1) != 0;
            }

            Send("ELITE_DOCKED", bits[0]);
            Send("ELITE_LANDED", bits[1]);
            Send("ELITE_LDG_DWN", bits[2]);
            Send("ELITE_SHIELD_UP", bits[3]);
            Send("ELITE_SUPERCRUISE", bits[4]);
            Send("ELITE_FA_OFF", bits[5]);
            Send("ELITE_HP_DEPLOYED", bits[6]);
            Send("ELITE_IN_WING", bits[7]);
            Send("ELITE_LIGHT_ON", bits[8]);
            Send("ELITE_CS_DEPLOYED", bits[9]);
            Send("ELITE_SILENT_RUNNING", bits[10]);
            Send("ELITE_FUELSCOOP", bits[11]);
            Send("ELITE_SRV_HANDBRAKE", bits[12]);
            Send("ELITE_SRV_TURRETVIEW", bits[13]);
            Send("ELITE_SRV_TURRET_RET", bits[14]);
            Send("ELITE_SRV_DRIVEASSIST", bits[15]);
            Send("ELITE_FSD_MASSLOCKED", bits[16]);
            Send("ELITE_FSD_CHARGING", bits[17]);
            Send("ELITE_FSD_COOLDOWN", bits[18]);
            Send("ELITE_LOW_FUEL", bits[19]);
            Send("ELITE_OVERHEAT", bits[20]);
            Send("ELITE_HAS_LAT_LONG", bits[21]);
            Send("ELITE_IS_IN_DANGER", bits[22]);
            Send("ELITE_INTERDICTED", bits[23]);
            Send("ELITE_IN_MAINSHIP", bits[24]);
            Send("ELITE_IN_FIGHTER", bits[25]);
            Send("ELITE_IN_SRV", bits[26]);
            Send("ELITE_ANALYSISMODE", bits[27]);
            Send("ELITE_NIGHTVISION", bits[28]);
            Send("ELITE_ALT_AVERAGE", bits[29]);
            Send("ELITE_FSD_JUMP", bits[30]);
            Send("ELITE_SRV_HIGHBEAM", bits[31]);

            Thread.Sleep(delay);

        }


    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _thread.Dispose();
    }


}
