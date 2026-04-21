using Microsoft.EntityFrameworkCore;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<House> Houses { get; }
    DbSet<Resident> Residents { get; }
    DbSet<ResidentDevice> ResidentDevices { get; }
    DbSet<VisitorLog> VisitorLogs { get; }
    DbSet<CameraConfiguration> CameraConfigurations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
