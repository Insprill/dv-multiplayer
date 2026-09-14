using Multiplayer.Components.Networking.Jobs;
using Multiplayer.Components.Networking.World;
using Multiplayer.Networking.Data.Jobs;
using System.Collections.Generic;

namespace Multiplayer.Networking.Packets.Clientbound.Jobs;

public class ClientboundJobsCreatePacket
{
    public uint StationNetId { get; set; }
    public JobData[] Jobs { get; set; }

    public static ClientboundJobsCreatePacket FromNetworkedJobs(NetworkedStationController netStation, NetworkedJob[] jobs)
    {
        List<JobData> jobData = [];
        foreach (var job in jobs)
        {
            JobData jd = JobData.FromJob(job);
            jobData.Add(jd);
        }

        return new ClientboundJobsCreatePacket
        {
            StationNetId = netStation.NetId,
            Jobs = jobData.ToArray()
        };
    }
}
