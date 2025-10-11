using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AppointmentRepository(AppDbContext context) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.PatientProfile)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientProfileIdAsync(Guid patientProfileId, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Where(r => r.PatientProfileId == patientProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.PatientProfile)
            .Where(a => a.PatientProfile.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(appointment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        context.Appointments.Update(appointment);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await context.Appointments.FindAsync([id], cancellationToken);
        if (appointment != null)
        {
            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}