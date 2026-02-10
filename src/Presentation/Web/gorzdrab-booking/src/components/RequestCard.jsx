function RequestCard({ request, onClick }) {
  const statusColors = {
    Pending: 'bg-emerald-500/10 text-emerald-300 border border-emerald-500/30',
    InProgress: 'bg-emerald-500/20 text-emerald-200 border border-emerald-400/50',
    Completed: 'bg-emerald-500/30 text-emerald-100 border border-emerald-300/70',
    Failed: 'bg-slate-500/15 text-slate-400 border border-slate-500/40',
    Cancelled: 'bg-slate-500/10 text-slate-500 border border-slate-600/30', 
  };

  const statusText = {
    Pending: 'Ожидает',
    InProgress: 'В обработке',
    Completed: 'Завершён',
    Failed: 'Ошибка',
    Cancelled: "Отменен"
  };

  const statusKey = typeof request.status === 'string' ? request.status : request.status;
  const createdAt = request.createdAt
    ? new Date(request.createdAt).toLocaleString('ru-RU')
    : '—';
  const patientName = request.patientFullName || request.patientName || '—';
  const speciality = request.speciality || request.specialty || '—';
  const presetName = request.timePreferencesPresetName || 'Без пресета';

  return (
    <div
      role={onClick ? 'button' : undefined}
      onClick={onClick}
      className={`group flex flex-col rounded-2xl border border-emerald-500/20 bg-slate-900/60 p-4 text-left shadow-sm shadow-black/40 transition hover:border-emerald-300/60 hover:bg-slate-900 hover:shadow-md hover:shadow-emerald-500/30 ${onClick ? 'cursor-pointer' : ''}`}
    >
      <div className="mb-3 flex items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-slate-50">
            {patientName}
          </h3>
          <p className="mt-1 text-[11px] text-slate-400">
            {request.lpuName || 'ЛПУ'} · {presetName}
          </p>
        </div>
        <span
          className={`shrink-0 inline-flex rounded-full px-2.5 py-1 text-[11px] font-medium ${statusColors[statusKey] || 'bg-slate-500/15 text-slate-200 border border-slate-400/40'}`}
        >
          {statusText[statusKey] ?? request.status ?? 'Статус'}
        </span>
      </div>

      <div className="space-y-1 text-xs text-slate-300">
        {speciality !== '—' && (
          <p>
            <span className="font-medium text-slate-200">Специальность:</span>{' '}
            {speciality}
          </p>
        )}
        {request.doctorNames?.length > 0 && (
          <p>
            <span className="font-medium text-slate-200">Врачи:</span>{' '}
            {request.doctorNames.join(', ')}
          </p>
        )}
        <p>
          <span className="font-medium text-slate-200">Создана:</span>{' '}
          {createdAt}
        </p>
      </div>
    </div>
  );
}

export default RequestCard;
