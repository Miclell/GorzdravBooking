using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AppointmentSearchRequestRepository(AppDbContext context) : IAppointmentSearchRequestRepository
{
    public async Task<AppointmentSearchRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.AppointmentSearchRequests
            .Include(r => r.PatientProfile)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AppointmentSearchRequest>> GetByPatientProfileIdAsync(Guid patientProfileId, CancellationToken cancellationToken = default)
    {
        return await context.AppointmentSearchRequests
            .Where(r => r.PatientProfileId == patientProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppointmentSearchRequest>> GetActiveByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await  context.AppointmentSearchRequests
            .Where(r => r.PatientProfile.UserId == userId)
            .Where(r =>
                r.Status == SearchRequestStatus.InProgress ||
                r.Status == SearchRequestStatus.Pending)
            .Include(r => r.PatientProfile)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppointmentSearchRequest>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.AppointmentSearchRequests
            .Where(r => r.Status == SearchRequestStatus.InProgress)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AppointmentSearchRequest request, CancellationToken cancellationToken = default)
    {
        await context.AppointmentSearchRequests.AddAsync(request, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AppointmentSearchRequest request, CancellationToken cancellationToken = default)
    {
        context.AppointmentSearchRequests.Update(request);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await context.AppointmentSearchRequests.FindAsync([id], cancellationToken);
        if (request != null)
        {
            context.AppointmentSearchRequests.Remove(request);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}