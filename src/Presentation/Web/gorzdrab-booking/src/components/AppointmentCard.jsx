function AppointmentCard({ appointment, onCancel }) {
  const fullName = appointment.patientFullName || appointment.patientName || '';
  const fioParts = fullName.split(' ').filter(Boolean);
  const shortPatient =
    fioParts.length === 0
      ? 'Пациент'
      : `${fioParts[0]}${fioParts[1] ? ` ${fioParts[1][0]}.` : ''}${fioParts[2] ? `${fioParts[2][0]}.` : ''}`;

  const lpu = appointment.lpuShortName || appointment.address || '—';

  const start = appointment.visitStart
    ? new Date(appointment.visitStart)
    : appointment.date
      ? new Date(appointment.date)
      : null;

  const date = start ? start.toLocaleDateString('ru-RU') : '—';
  const time = start
    ? start.toLocaleTimeString('ru-RU', {
        hour: '2-digit',
        minute: '2-digit',
      })
    : appointment.time || '—';

  const doctor = appointment.doctor || appointment.doctorName || 'Врач не указан';

  return (
    <div className="group flex flex-col rounded-2xl border border-emerald-500/20 bg-slate-900/60 p-4 text-left shadow-sm shadow-black/40 transition hover:border-emerald-300/60 hover:bg-slate-900 hover:shadow-md hover:shadow-emerald-500/30">
      <div className="flex items-start justify-between gap-3">
        <div className="flex-1 min-w-0">
          <h3 className="mb-1 text-sm font-semibold text-slate-50">
            {doctor}
          </h3>
          <p className="mb-0.5 text-xs text-slate-300">
            <span className="font-medium text-slate-200">Пациент:</span>{' '}
            {shortPatient}
          </p>
          <p className="mb-0.5 text-xs text-slate-300">
            <span className="font-medium text-slate-200">Поликлиника:</span>{' '}
            {lpu}
          </p>
          <p className="mb-0.5 text-xs text-slate-300">
            <span className="font-medium text-slate-200">Дата:</span> {date}
          </p>
          <p className="mb-0.5 text-xs text-slate-300">
            <span className="font-medium text-slate-200">Время:</span> {time}
          </p>
        </div>

        <div className="flex flex-col items-end gap-2 shrink-0">
          <span className="inline-flex rounded-full border border-emerald-400/50 bg-emerald-500/15 px-2.5 py-1 text-[11px] font-medium text-emerald-200">
            Подтверждена
          </span>
          <button
            type="button"
            onClick={() => onCancel(appointment)}
            className="text-[11px] font-medium text-rose-300 underline-offset-4 hover:text-rose-200 hover:underline"
          >
            Отменить
          </button>
        </div>
      </div>
    </div>
  );
}

export default AppointmentCard;
