function PatientCard({ patient, onClick, onDelete }) {
  const fullName =
    patient.patientFullName ||
    patient.fullName ||
    [patient.patientLastName, patient.patientFirstName, patient.patientMiddleName]
      .filter(Boolean)
      .join(' ') ||
    'Пациент';

  const birthDate = patient.patientBirthdate
    ? new Date(patient.patientBirthdate).toLocaleDateString('ru-RU')
    : patient.birthDate
      ? new Date(patient.birthDate).toLocaleDateString('ru-RU')
      : '—';

  return (
    <div className="group flex w-full flex-col rounded-2xl border border-emerald-500/20 bg-slate-900/60 p-4 text-left shadow-sm shadow-black/40 transition hover:border-emerald-300/60 hover:bg-slate-900 hover:shadow-md hover:shadow-emerald-500/30">
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 min-w-0">
          <h3 className="mb-2 text-sm font-semibold text-slate-50">
            {fullName}
          </h3>
          <p className="mb-1 text-xs text-slate-300/90">
            <span className="font-medium text-slate-200">Поликлиника:</span>{' '}
            {patient.lpuShortName || '—'}
          </p>
          <p className="text-xs text-slate-300/90">
            <span className="font-medium text-slate-200">Дата рождения:</span>{' '}
            {birthDate}
          </p>
        </div>
        <div className="flex flex-col items-end gap-2 shrink-0">
          <span className="inline-flex rounded-full border border-emerald-400/50 bg-emerald-500/15 px-2.5 py-1 text-[11px] font-medium text-emerald-200">
            Пациент
          </span>
          {onDelete && (
            <button
              type="button"
              onClick={e => {
                e.stopPropagation();
                onDelete(patient);
              }}
              className="text-[11px] font-medium text-rose-300 underline-offset-4 hover:text-rose-200 hover:underline"
              title="Удалить пациента"
            >
              Удалить
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default PatientCard;
