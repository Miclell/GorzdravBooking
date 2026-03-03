import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Modal from '../components/Modal';
import Select from '../components/Select';
import * as api from '../services/api';

const DOCTOR_MODE = {
  ANY: 0,
  SPECIFIC: 1,
};

function CreateRequest() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const { userId } = useAuth();
  const updateRequestId = searchParams.get('updateRequestId');
  const currentPreset = searchParams.get('currentPreset') || '';

  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState('');

  const [specialities, setSpecialities] = useState([]);
  const [selectedSpecialityId, setSelectedSpecialityId] = useState('');

  const [doctors, setDoctors] = useState([]);
  const [selectedDoctorIds, setSelectedDoctorIds] = useState([]);

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [mode, setMode] = useState('referral'); // 'referral' | 'manual'
  const [referralNumber, setReferralNumber] = useState('');
  const [doctorMode, setDoctorMode] = useState(DOCTOR_MODE.ANY);

  const [advancedOpen, setAdvancedOpen] = useState(false);
  const [searchIntervalMinutes, setSearchIntervalMinutes] = useState(60);
  const [maxDaysToSearch, setMaxDaysToSearch] = useState(30);
  const [viewOnly, setViewOnly] = useState(false);
  const [specificPoints, setSpecificPoints] = useState([]);

  const [presetNames, setPresetNames] = useState([]);
  const [selectedPresetName, setSelectedPresetName] = useState('');

  const restoredForm = location.state?.restoredRequestForm;

  const selectedPatient = useMemo(
    () => patients.find(p => p.id === selectedPatientId),
    [patients, selectedPatientId],
  );

  const presetOptions = useMemo(() => {
    const names = [...presetNames];
    if (updateRequestId && currentPreset && !names.includes(currentPreset))
      names.push(currentPreset);
    return names.map(name => ({ value: name, label: name }));
  }, [presetNames, updateRequestId, currentPreset]);

  // Загрузка пациентов
  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        // В режиме изменения пресета пациентов не нужно грузить
        if (!updateRequestId) {
          const data = await api.getPatients();
          setPatients(data || []);
        }
      } catch (e) {
        setError('Не удалось загрузить список пациентов.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [updateRequestId]);

  // Восстановление состояния формы после возврата из настроек пресетов
  useEffect(() => {
    if (!restoredForm) return;
    if (restoredForm.selectedPatientId) {
      setSelectedPatientId(restoredForm.selectedPatientId);
    }
    if (restoredForm.mode) {
      setMode(restoredForm.mode);
    }
    if (typeof restoredForm.referralNumber === 'string') {
      setReferralNumber(restoredForm.referralNumber);
    }
    if (restoredForm.selectedSpecialityId) {
      setSelectedSpecialityId(restoredForm.selectedSpecialityId);
    }
    if (typeof restoredForm.doctorMode === 'number') {
      setDoctorMode(restoredForm.doctorMode);
    }
    if (Array.isArray(restoredForm.selectedDoctorIds)) {
      setSelectedDoctorIds(restoredForm.selectedDoctorIds);
    }
    if (typeof restoredForm.searchIntervalMinutes === 'number') {
      setSearchIntervalMinutes(restoredForm.searchIntervalMinutes);
    }
    if (typeof restoredForm.maxDaysToSearch === 'number') {
      setMaxDaysToSearch(restoredForm.maxDaysToSearch);
    }
    if (typeof restoredForm.viewOnly === 'boolean') {
      setViewOnly(restoredForm.viewOnly);
    }
    if (Array.isArray(restoredForm.specificPoints)) {
      setSpecificPoints(restoredForm.specificPoints);
    }
    if (typeof restoredForm.selectedPresetName === 'string') {
      setSelectedPresetName(restoredForm.selectedPresetName);
    }
  }, [restoredForm]);

  // Загрузка пресетов из API
  useEffect(() => {
    api.getAllTimePreferences()
      .then(presets => setPresetNames((presets || []).map(p => p.name).filter(Boolean).sort()))
      .catch(() => setPresetNames([]));
  }, []);

  // Загрузка специальностей после выбора пациента
  useEffect(() => {
    const loadSpecialities = async () => {
      if (updateRequestId) return;
      if (!selectedPatient?.lpuId) {
        setSpecialities([]);
        setSelectedSpecialityId('');
        return;
      }

      try {
        const specs = await api.getSpecialities(selectedPatient.lpuId);
        setSpecialities(specs || []);
      } catch (e) {
        setError('Не удалось загрузить специальности для выбранной поликлиники.');
      }
    };

    loadSpecialities();
  }, [selectedPatient, updateRequestId]);

  // Загрузка врачей при выборе специальности и режиме "конкретные врачи"
  useEffect(() => {
    const loadDoctors = async () => {
      if (updateRequestId) return;
      if (
        !selectedPatient?.lpuId ||
        !selectedSpecialityId ||
        doctorMode !== DOCTOR_MODE.SPECIFIC
      ) {
        setDoctors([]);
        setSelectedDoctorIds([]);
        return;
      }

      try {
        const docs = await api.getDoctors(
          selectedPatient.lpuId,
          selectedSpecialityId,
        );
        setDoctors(docs || []);
      } catch (e) {
        setError('Не удалось загрузить список врачей.');
      }
    };

    loadDoctors();
  }, [selectedPatient, selectedSpecialityId, doctorMode, updateRequestId]);

  // Префилл текущего пресета при изменении запроса
  useEffect(() => {
    if (!updateRequestId) return;
    if (currentPreset) setSelectedPresetName(currentPreset);
  }, [updateRequestId, currentPreset]);

  const toggleDoctor = id => {
    setSelectedDoctorIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id],
    );
  };

  const addSpecificPoint = () => {
    setSpecificPoints(prev => [...prev, { time: '' }]);
  };

  const updateSpecificPoint = (index, field, value) => {
    setSpecificPoints(prev =>
      prev.map((item, i) => (i === index ? { ...item, [field]: value } : item)),
    );
  };

  const removeSpecificPoint = index => {
    setSpecificPoints(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async e => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (updateRequestId) {
      const presetName = selectedPresetName?.trim();
      if (!presetName) {
        setError('Выберите пресет временных предпочтений.');
        return;
      }

      const todayIso = new Date().toISOString().split('T')[0];
      const filteredStartPoints = specificPoints
          .filter(p => p.time)
          .map(p => new Date(`${todayIso}T${p.time}`));
      const specificStartPoints = filteredStartPoints.length ? filteredStartPoints : null;

      const toTimeSpan = minutes => {
        const total = Math.max(0, Number(minutes) || 0);
        const h = String(Math.floor(total / 60)).padStart(2, '0');
        const m = String(total % 60).padStart(2, '0');
        return `${h}:${m}:00`;
      };

      setSubmitting(true);
      try {
        await api.updateRequestPreferences({
          requestId: updateRequestId,
          timePreferencesName: presetName,
          specificStartPoints,
          searchInternaval: toTimeSpan(searchIntervalMinutes),
          maxDaysToSearch,
          viewOnly,
        });
        setSuccess('Временные предпочтения обновлены.');
        setTimeout(() => navigate('/dashboard'), 800);
      } catch (e2) {
        setError('Не удалось обновить предпочтения. Попробуйте ещё раз.');
      } finally {
        setSubmitting(false);
      }
      return;
    }

    if (!selectedPatient) {
      setError('Выберите пациента.');
      return;
    }

    if (mode === 'referral' && !referralNumber.trim()) {
      setError('Укажите номер направления.');
      return;
    }

    if (mode === 'manual' && !selectedSpecialityId) {
      setError('Выберите специальность.');
      return;
    }

    if (
      mode === 'manual' &&
      doctorMode === DOCTOR_MODE.SPECIFIC &&
      selectedDoctorIds.length === 0
    ) {
      setError('Выберите хотя бы одного врача или переключите режим на «любой врач».');
      return;
    }

    if (!selectedPresetName?.trim()) {
      setError('Выберите пресет временных предпочтений или создайте его в разделе «Временные предпочтения».');
      return;
    }

    const presetName = selectedPresetName.trim();
    const specialityEntity = specialities.find(s => s.id === selectedSpecialityId);
    const selectedDoctorsFull = doctors.filter(d =>
      selectedDoctorIds.includes(d.id),
    );

    const todayIso = new Date().toISOString().split('T')[0];
    const filteredPoints = specificPoints
        .filter(p => p.time)
        .map(p => new Date(`${todayIso}T${p.time}`));
    const specificStartPoints = filteredPoints.length ? filteredPoints : null;

    const toTimeSpan = minutes => {
      const total = Math.max(0, Number(minutes) || 0);
      const h = String(Math.floor(total / 60)).padStart(2, '0');
      const m = String(total % 60).padStart(2, '0');
      return `${h}:${m}:00`;
    };

    const requestBody = {
      patientProfileId: selectedPatient.id,
      referralNumber: mode === 'referral' ? referralNumber.trim() : null,
      lpuName: selectedPatient.lpuShortName || selectedPatient.lpuId || null,
      speciality:
        mode === 'manual'
          ? specialityEntity?.name || specialityEntity?.ferId || null
          : null,
      doctorMode:
        mode === 'manual' && doctorMode === DOCTOR_MODE.SPECIFIC
          ? DOCTOR_MODE.SPECIFIC
          : DOCTOR_MODE.ANY,
      doctorIds:
        mode === 'manual' && doctorMode === DOCTOR_MODE.SPECIFIC
          ? selectedDoctorsFull.map(d => d.id)
          : null,
      doctorNames:
        mode === 'manual' && doctorMode === DOCTOR_MODE.SPECIFIC
          ? selectedDoctorsFull.map(d => d.name)
          : null,
      timePreferencesPresetName: presetName,
      searchInterval: toTimeSpan(searchIntervalMinutes),
      specificStartPoints,
      maxDaysToSearch,
      viewOnly,
    };

    setSubmitting(true);
    try {
      await api.createRequest(requestBody);
      setSuccess('Запрос успешно создан.');
      setTimeout(() => navigate('/dashboard'), 800);
    } catch (e) {
      setError('Не удалось создать запрос. Проверьте данные и попробуйте ещё раз.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-emerald-300">
            {updateRequestId ? 'Изменение запроса' : 'Новый запрос'}
          </p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50 sm:text-2xl">
            {updateRequestId
              ? 'Изменить временные предпочтения'
              : 'Создание запроса на запись'}
          </h2>
          <p className="mt-1 text-xs text-slate-400 sm:text-sm">
            {updateRequestId
              ? 'Выберите готовый пресет или создайте новый, затем сохраните изменения.'
              : 'Выберите пациента, укажите тип запроса, врача и временные предпочтения.'}
          </p>
        </div>

        <button
          type="button"
          onClick={() => navigate('/dashboard')}
          className="btn-secondary text-xs px-3 py-2"
        >
          Назад к дашборду
        </button>
      </div>

      <form
        onSubmit={handleSubmit}
        className={`grid gap-4 ${
          updateRequestId
            ? 'lg:grid-cols-[minmax(0,1fr)]'
            : 'lg:grid-cols-[minmax(0,1.25fr)_minmax(0,1fr)]'
        }`}
      >
        {!updateRequestId && (
          <div className="space-y-4">
          <div className="glass-panel border-emerald-500/20 px-4 py-4">
            <h3 className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
              Пациент и тип заявки
            </h3>

            {error && (
              <div className="mb-3 rounded-xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-xs text-rose-100">
                {error}
              </div>
            )}
            {success && (
              <div className="mb-3 rounded-xl border border-emerald-500/40 bg-emerald-500/10 px-4 py-3 text-xs text-emerald-100">
                {success}
              </div>
            )}

            <div className="space-y-3">
              <div>
                <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                  Пациент *
                </label>
                <Select
                  value={selectedPatientId}
                  onChange={setSelectedPatientId}
                  options={patients.map(p => ({
                    value: p.id,
                    label: [
                      [
                        p.patientLastName,
                        p.patientFirstName && `${p.patientFirstName[0]}.`,
                        p.patientMiddleName && `${p.patientMiddleName[0]}.`,
                      ]
                        .filter(Boolean)
                        .join(' '),
                      p.lpuShortName || 'ЛПУ',
                    ].join(' · '),
                  }))}
                  placeholder="Выберите пациента"
                  required
                />
              </div>

              {selectedPatient && (
                <div className="rounded-xl border border-emerald-500/30 bg-slate-950/60 px-3 py-2 text-[11px] text-slate-300">
                  <p>
                    <span className="font-medium text-slate-100">Пациент:</span>{' '}
                    {[selectedPatient.patientLastName, selectedPatient.patientFirstName, selectedPatient.patientMiddleName]
                      .filter(Boolean)
                      .join(' ') || selectedPatient.patientFullName || selectedPatient.id}
                  </p>
                  <p>
                    <span className="font-medium text-slate-100">Поликлиника:</span>{' '}
                    {selectedPatient.lpuShortName || '—'}
                  </p>
                </div>
              )}

              <div>
                <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                  Тип запроса
                </label>
                <div className="inline-flex gap-1 rounded-full bg-slate-900/80 p-1">
                  <button
                    type="button"
                    onClick={() => setMode('referral')}
                    className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${
                      mode === 'referral'
                        ? 'bg-emerald-500 text-slate-950'
                        : 'text-slate-300 hover:bg-slate-800/80'
                    }`}
                  >
                    По направлению
                  </button>
                  <button
                    type="button"
                    onClick={() => setMode('manual')}
                    className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${
                      mode === 'manual'
                        ? 'bg-emerald-500 text-slate-950'
                        : 'text-slate-300 hover:bg-slate-800/80'
                    }`}
                  >
                    Ручная
                  </button>
                </div>
              </div>

              {mode === 'referral' && (
                <div>
                  <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                    Номер направления *
                  </label>
                  <input
                    value={referralNumber}
                    onChange={e => setReferralNumber(e.target.value)}
                    className="input-field"
                    placeholder="Например, 1234567890"
                    required
                  />
                </div>
              )}

              {mode === 'manual' && (
                <>
                  <div>
                    <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                      Специальность *
                    </label>
                    <Select
                      value={selectedSpecialityId}
                      onChange={setSelectedSpecialityId}
                      options={specialities.map(s => ({
                        value: s.id,
                        label: s.name || s.ferId || String(s.id),
                      }))}
                      placeholder="Выберите специальность"
                      required
                    />
                  </div>

                  <div>
                    <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                      Выбор врача
                    </label>
                    <div className="inline-flex gap-1 rounded-full bg-slate-900/80 p-1">
                      <button
                        type="button"
                        onClick={() => setDoctorMode(DOCTOR_MODE.ANY)}
                        className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${
                          doctorMode === DOCTOR_MODE.ANY
                            ? 'bg-emerald-500 text-slate-950'
                            : 'text-slate-300 hover:bg-slate-800/80'
                        }`}
                      >
                        Любой доступный врач
                      </button>
                      <button
                        type="button"
                        onClick={() => setDoctorMode(DOCTOR_MODE.SPECIFIC)}
                        className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${
                          doctorMode === DOCTOR_MODE.SPECIFIC
                            ? 'bg-emerald-500 text-slate-950'
                            : 'text-slate-300 hover:bg-slate-800/80'
                        }`}
                      >
                        Конкретный врач
                      </button>
                    </div>
                  </div>

                  {doctorMode === DOCTOR_MODE.SPECIFIC && (
                    <div className="mt-2 max-h-52 space-y-1 overflow-y-auto rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 text-xs text-slate-200">
                      {doctors.length === 0 && (
                        <p className="text-slate-400">
                          Врачи по выбранной специальности не найдены или ещё не подгружены.
                        </p>
                      )}
                      {doctors.map(doc => (
                        <label
                          key={doc.id}
                          className="flex cursor-pointer items-center gap-2 rounded-lg px-2 py-1 hover:bg-slate-900"
                        >
                          <input
                            type="checkbox"
                            className="h-3 w-3 rounded border-slate-600 bg-slate-900"
                            checked={selectedDoctorIds.includes(doc.id)}
                            onChange={() => toggleDoctor(doc.id)}
                          />
                          <span>
                            {doc.name}
                            {doc.nearestDate && (
                              <span className="ml-1 text-[10px] text-slate-400">
                                (c{' '}
                                {new Date(doc.nearestDate).toLocaleDateString(
                                  'ru-RU',
                                )}
                                )
                              </span>
                            )}
                          </span>
                        </label>
                      ))}
                    </div>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
        )}

        {/* Временные предпочтения */}
        <div className="space-y-4">
          <div className="glass-panel border-emerald-500/20 px-4 py-4">
            <h3 className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
              Временные предпочтения
            </h3>

            <div className="mb-3">
              <label className="mb-1 block text-xs font-medium text-slate-300">
                Пресет времени *
              </label>
              <Select
                value={selectedPresetName}
                onChange={setSelectedPresetName}
                options={presetOptions}
                placeholder="Выберите пресет"
                size="sm"
              />
              {presetNames.length === 0 && (
                <p className="mt-2 text-[11px] text-slate-400">
                  Создайте пресет в разделе «Временные предпочтения».
                </p>
              )}
            </div>

            <div className="mb-3 flex flex-wrap items-center gap-2">
              <button
                type="button"
                onClick={() =>
                  navigate('/time-preferences', {
                    state: {
                      returnTo: '/requests/new',
                      fromRequestState: {
                        selectedPatientId,
                        mode,
                        referralNumber,
                        selectedSpecialityId,
                        doctorMode,
                        selectedDoctorIds,
                        searchIntervalMinutes,
                        maxDaysToSearch,
                        viewOnly,
                        specificPoints,
                        selectedPresetName,
                      },
                    },
                  })
                }
                className="btn-secondary text-xs px-3 py-1.5"
              >
                Управление пресетами
              </button>
              <button
                type="button"
                className="btn-secondary w-full justify-center text-xs sm:w-auto"
                onClick={() => setAdvancedOpen(true)}
              >
                Доп. настройки поиска
              </button>
            </div>

            <div className="mt-4 border-t border-slate-800 pt-3">
              <h4 className="mb-2 text-[11px] font-semibold uppercase tracking-[0.18em] text-emerald-300">
                Специфичные точки запуска (опционально)
              </h4>
              <p className="mb-2 text-[11px] text-slate-400">
                Если нужно стартовать поиск в конкретные моменты времени (например
                08:00, 12:30), добавьте их здесь.
              </p>
              <div className="space-y-2">
                {specificPoints.map((p, index) => (
                  <div
                    key={index}
                    className="flex flex-col gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 sm:flex-row sm:items-center"
                  >
                    <input
                      type="time"
                      value={p.time}
                      onChange={e =>
                        updateSpecificPoint(index, 'time', e.target.value)
                      }
                      className="input-field text-xs"
                    />
                    <button
                      type="button"
                      onClick={() => removeSpecificPoint(index)}
                      className="text-[11px] text-slate-400 hover:text-rose-300"
                    >
                      ✕
                    </button>
                  </div>
                ))}
                <button
                  type="button"
                  onClick={addSpecificPoint}
                  className="btn-secondary mt-1 w-full justify-center text-xs"
                >
                  Добавить точку запуска
                </button>
              </div>
            </div>
          </div>

          <div className="flex justify-end">
            <button
              type="submit"
              disabled={submitting || loading}
              className="btn-primary px-5 py-2 text-sm"
            >
              {updateRequestId
                ? submitting
                  ? 'Сохраняем...'
                  : 'Сохранить изменения'
                : submitting
                  ? 'Создаём запрос...'
                  : 'Создать запрос'}
            </button>
          </div>
        </div>
      </form>

      <Modal
        isOpen={advancedOpen}
        onClose={() => setAdvancedOpen(false)}
        title="Дополнительные настройки поиска"
      >
        <div className="space-y-4">
          <div>
            <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
              Интервал поиска (мин)
            </label>
            <input
              type="number"
              min={1}
              max={720}
              value={searchIntervalMinutes}
              onChange={e => setSearchIntervalMinutes(Number(e.target.value) || 1)}
              className="input-field"
            />
            <p className="mt-1 text-[11px] text-slate-400">
              Как часто сервис будет выполнять попытку поиска слота.
            </p>
          </div>

          <div>
            <label className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
              Макс. дней для поиска
            </label>
            <input
              type="number"
              min={1}
              max={365}
              value={maxDaysToSearch}
              onChange={e => setMaxDaysToSearch(Number(e.target.value) || 1)}
              className="input-field"
            />
            <p className="mt-1 text-[11px] text-slate-400">
              Ограничение длительности поиска слотов по дням вперёд.
            </p>
          </div>

          <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/60 px-4 py-3 text-xs text-slate-200">
            <input
              type="checkbox"
              className="h-4 w-4 rounded border-slate-600 bg-slate-900"
              checked={viewOnly}
              onChange={e => setViewOnly(e.target.checked)}
            />
            <span>
              Только просмотр (не создавать запись автоматически)
            </span>
          </label>

          <div className="flex justify-end gap-2">
            <button
              type="button"
              className="btn-primary px-4 py-2 text-sm"
              onClick={() => setAdvancedOpen(false)}
            >
              Готово
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default CreateRequest;

