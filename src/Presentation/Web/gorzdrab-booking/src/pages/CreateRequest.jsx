import ValidatedForm from '../components/ValidatedForm';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import Modal from '../components/Modal';
import Select from '../components/Select';
import StatusMessage from '../components/StatusMessage';
import * as api from '../services/api';

const ANY_DOCTOR = 0;
const SPECIFIC_DOCTOR = 1;

function DoctorPicker({ mode, onModeChange, doctors, selectedIds, onToggle, loading = false, label }) {
  return <div>
    <p className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">{label}</p>
    <div className="inline-flex flex-wrap gap-1 rounded-full bg-slate-900/80 p-1" role="group" aria-label={label}>
      <button type="button" aria-pressed={mode === ANY_DOCTOR} onClick={() => onModeChange(ANY_DOCTOR)}
        className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${mode === ANY_DOCTOR ? 'bg-emerald-500 text-slate-950' : 'text-slate-300 hover:bg-slate-800/80'}`}>Любой доступный врач</button>
      <button type="button" aria-pressed={mode === SPECIFIC_DOCTOR} onClick={() => onModeChange(SPECIFIC_DOCTOR)}
        className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${mode === SPECIFIC_DOCTOR ? 'bg-emerald-500 text-slate-950' : 'text-slate-300 hover:bg-slate-800/80'}`}>Конкретный врач</button>
    </div>
    {mode === SPECIFIC_DOCTOR && <div className="mt-2 max-h-52 space-y-1 overflow-y-auto rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 text-xs text-slate-200">
      {loading ? <p role="status">Загружаем врачей...</p> : doctors.length === 0 ? <p className="text-sm text-slate-300">Врачи не найдены.</p>
        : doctors.map(doctor => <label key={doctor.id} className="flex cursor-pointer items-center gap-2 rounded-lg px-2 py-1 hover:bg-slate-900">
          <input type="checkbox" checked={selectedIds.includes(doctor.id)} onChange={() => onToggle(doctor.id)} className="h-3 w-3 rounded border-slate-600 bg-slate-900" />
          <span className="min-w-0 break-words">{doctor.name}
            {doctor.nearestDate && <span className="ml-1 text-[10px] text-slate-400">(с {new Date(doctor.nearestDate).toLocaleDateString('ru-RU')})</span>}
          </span>
        </label>)}
    </div>}
  </div>;
}

function CreateRequest() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const updateRequestId = searchParams.get('updateRequestId');
  const currentPreset = searchParams.get('currentPreset') || '';
  const [form, setForm] = useState(() => ({
    mode: 'manual', selectedPatientId: '', selectedSpecialityId: '', doctorMode: ANY_DOCTOR,
    selectedDoctorIds: [], referralNumber: '', referralLastName: '', referralResult: null,
    referralSelectedSpecialityId: '', referralDoctorMode: ANY_DOCTOR, referralSelectedDoctorIds: [],
    selectedPresetName: currentPreset, searchIntervalMinutes: 60, maxDaysToSearch: 30,
    viewOnly: false, specificPoints: [], ...location.state?.restoredRequestForm,
  }));
  const [patients, setPatients] = useState([]);
  const [specialities, setSpecialities] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [presetNames, setPresetNames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [patientsError, setPatientsError] = useState('');
  const [presetsLoading, setPresetsLoading] = useState(true);
  const [presetsError, setPresetsError] = useState('');
  const [specialitiesLoading, setSpecialitiesLoading] = useState(false);
  const [doctorsLoading, setDoctorsLoading] = useState(false);
  const [lookupError, setLookupError] = useState('');
  const [lookupRetry, setLookupRetry] = useState(0);
  const [referralValidating, setReferralValidating] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [advancedOpen, setAdvancedOpen] = useState(false);
  const referralCheck = useRef(0);
  const referralPending = useRef(false);
  const submitLock = useRef(false);
  const updateForm = updates => setForm(prev => ({ ...prev, ...updates }));
  const selectedPatient = useMemo(() => patients.find(p => p.id === form.selectedPatientId), [patients, form.selectedPatientId]);
  const lpuId = selectedPatient?.lpuId;
  const selectedSpecialityId = form.selectedSpecialityId;
  const doctorMode = form.doctorMode;
  const presetOptions = useMemo(() => {
    const names = [...presetNames];
    if (updateRequestId && currentPreset && !names.includes(currentPreset)) names.push(currentPreset);
    return names.map(name => ({ value: name, label: name }));
  }, [presetNames, updateRequestId, currentPreset]);
  const referralSpeciality = form.referralResult?.specialities?.find(item => String(item.id) === String(form.referralSelectedSpecialityId));

  const loadPatients = useCallback(async () => {
    setLoading(true);
    setPatientsError('');
    try { if (!updateRequestId) setPatients((await api.getPatients()) || []); }
    catch { setPatientsError('Не удалось загрузить пациентов.'); }
    finally { setLoading(false); }
  }, [updateRequestId]);
  const loadPresets = useCallback(async () => {
    setPresetsLoading(true);
    setPresetsError('');
    try {
      const presets = await api.getAllTimePreferences();
      setPresetNames([...new Set((presets || []).map(p => p.name).filter(Boolean))].sort());
    } catch { setPresetsError('Не удалось загрузить шаблоны времени.'); }
    finally { setPresetsLoading(false); }
  }, []);
  useEffect(() => { loadPatients(); }, [loadPatients]);
  useEffect(() => { loadPresets(); }, [loadPresets]);

  useEffect(() => {
    let active = true;
    if (updateRequestId || !lpuId) return;
    setSpecialitiesLoading(true);
    api.getSpecialities(lpuId).then(items => {
      if (!active) return;
      setSpecialities(items || []);
      setForm(prev => ({ ...prev, selectedSpecialityId:
        items?.some(item => String(item.id) === String(prev.selectedSpecialityId)) ? prev.selectedSpecialityId : '' }));
    }).catch(() => { if (active) setLookupError('Не удалось загрузить специальности.'); })
      .finally(() => { if (active) setSpecialitiesLoading(false); });
    return () => { active = false; };
  }, [lpuId, updateRequestId, lookupRetry]);

  useEffect(() => {
    let active = true;
    if (updateRequestId || !lpuId || !selectedSpecialityId || doctorMode !== SPECIFIC_DOCTOR) return;
    setDoctorsLoading(true);
    api.getDoctors(lpuId, selectedSpecialityId).then(items => {
      if (!active) return;
      setDoctors(items || []);
      setForm(prev => ({ ...prev, selectedDoctorIds: prev.selectedDoctorIds.filter(id => items?.some(item => item.id === id)) }));
    }).catch(() => { if (active) setLookupError('Не удалось загрузить врачей.'); })
      .finally(() => { if (active) setDoctorsLoading(false); });
    return () => { active = false; };
  }, [lpuId, selectedSpecialityId, doctorMode, updateRequestId, lookupRetry]);

  const changePatient = value => {
    updateForm({ selectedPatientId: value, selectedSpecialityId: '', selectedDoctorIds: [] });
    setSpecialities([]); setDoctors([]); setLookupError(''); setSpecialitiesLoading(false); setDoctorsLoading(false);
  };
  const changeSpeciality = value => {
    updateForm({ selectedSpecialityId: value, selectedDoctorIds: [] });
    setDoctors([]); setLookupError(''); setDoctorsLoading(false);
  };
  const toggleDoctor = (key, id) => setForm(prev => ({
    ...prev, [key]: prev[key].includes(id) ? prev[key].filter(item => item !== id) : [...prev[key], id],
  }));
  const changeReferral = (key, value) => {
    referralPending.current = false;
    referralCheck.current += 1;
    setReferralValidating(false);
    updateForm({ [key]: value, referralResult: null, referralSelectedSpecialityId: '', referralSelectedDoctorIds: [] });
  };
  const validateReferral = async () => {
    if (referralPending.current || submitLock.current) return;
    setError('');
    if (!form.referralNumber.trim() || !form.referralLastName.trim()) {
      setError('Укажите номер направления и фамилию пациента.'); return;
    }
    const checkId = ++referralCheck.current;
    referralPending.current = true;
    setReferralValidating(true);
    updateForm({ referralResult: null });
    try {
      const result = await api.validateReferral(form.referralNumber.trim(), form.referralLastName.trim());
      if (checkId === referralCheck.current) updateForm({
        referralResult: result, referralSelectedSpecialityId: '', referralSelectedDoctorIds: [], referralDoctorMode: ANY_DOCTOR,
      });
    } catch {
      if (checkId === referralCheck.current) setError('Не удалось проверить направление. Проверьте данные и попробуйте снова.');
    } finally { if (checkId === referralCheck.current) { referralPending.current = false; setReferralValidating(false); } }
  };

  const handleSubmit = async event => {
    event.preventDefault();
    if (submitLock.current || referralPending.current || presetsLoading || specialitiesLoading || doctorsLoading) return;
    setError('');
    if (presetsError || (!updateRequestId && form.mode === 'manual' && (patientsError || lookupError))) {
      setError('Сначала загрузите данные формы.'); return;
    }
    const interval = Number(form.searchIntervalMinutes), maxDays = Number(form.maxDaysToSearch);
    if (!Number.isInteger(interval) || interval < 1 || interval > 1440 || !Number.isInteger(maxDays) || maxDays < 1 || maxDays > 365) {
      setError('Интервал поиска: от 1 до 1440 минут. Длительность поиска: от 1 до 365 дней.'); return;
    }
    if (!form.selectedPresetName.trim()) { setError('Выберите шаблон времени.'); return; }
    const now = new Date();
    const today = [now.getFullYear(), String(now.getMonth() + 1).padStart(2, '0'), String(now.getDate()).padStart(2, '0')].join('-');
    const points = form.specificPoints.filter(p => p.time).map(p => new Date(today + 'T' + p.time));
    const common = {
      searchInterval: interval === 1440 ? '1.00:00:00' : String(Math.floor(interval / 60)).padStart(2, '0') + ':' + String(interval % 60).padStart(2, '0') + ':00',
      maxDaysToSearch: maxDays, viewOnly: form.viewOnly, specificStartPoints: points.length ? points : null,
    };
    let payload;
    if (updateRequestId) payload = { ...common, requestId: updateRequestId, timePreferencesName: form.selectedPresetName.trim() };
    else {
      const isReferral = form.mode === 'referral';
      const patient = isReferral ? form.referralResult?.patientProfile : selectedPatient;
      const speciality = isReferral ? referralSpeciality : specialities.find(item => String(item.id) === String(form.selectedSpecialityId));
      const chosenMode = isReferral ? form.referralDoctorMode : form.doctorMode;
      const ids = isReferral ? form.referralSelectedDoctorIds : form.selectedDoctorIds;
      const availableDoctors = isReferral ? referralSpeciality?.doctors || [] : doctors;
      const chosenDoctors = availableDoctors.filter(item => ids.includes(item.id));
      if (!patient) { setError(isReferral ? 'Сначала проверьте направление.' : 'Выберите пациента.'); return; }
      if (!speciality) { setError('Выберите специальность.'); return; }
      if (chosenMode === SPECIFIC_DOCTOR && !chosenDoctors.length) { setError('Выберите хотя бы одного врача.'); return; }
      payload = {
        ...common, patientProfileId: patient.id, referralNumber: isReferral ? form.referralNumber.trim() : null,
        lpuName: patient.lpuShortName || patient.lpuId || null, speciality: speciality.name || speciality.ferId || null,
        specialityId: isReferral ? null : speciality.id, doctorMode: chosenMode,
        doctorIds: chosenMode === SPECIFIC_DOCTOR ? chosenDoctors.map(d => d.id) : null,
        doctorNames: chosenMode === SPECIFIC_DOCTOR ? chosenDoctors.map(d => d.name) : null,
        timePreferencesPresetName: form.selectedPresetName.trim(),
      };
    }
    submitLock.current = true;
    setSubmitting(true);
    try {
      if (updateRequestId) await api.updateRequestPreferences(payload);
      else await api.createRequest(payload);
      navigate('/dashboard', { state: { activeTab: 'requests', notice: updateRequestId ? 'Заявка обновлена.' : 'Заявка создана.' } });
    } catch (err) {
      setError(err.message || 'Не удалось сохранить заявку. Попробуйте снова.');
      setSubmitting(false);
      submitLock.current = false;
    }
  };

  return <div className="flex flex-col gap-6">
    <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.22em] text-emerald-300">{updateRequestId ? 'Изменение заявки' : 'Новая заявка'}</p>
        <h2 className="mt-1 text-xl font-semibold text-slate-50 sm:text-2xl">{updateRequestId ? 'Изменить временные предпочтения' : 'Создание заявки на запись'}</h2>
        <p className="mt-1 text-xs text-slate-400 sm:text-sm">Укажите данные и выберите шаблон времени для поиска записи.</p>
      </div>
      <button type="button" onClick={() => navigate('/dashboard')} className="btn-secondary text-xs px-3 py-2">В рабочий кабинет</button>
    </div>
    <StatusMessage>{error}</StatusMessage>
    <ValidatedForm onSubmit={handleSubmit}>
      <fieldset disabled={submitting} className={updateRequestId ? 'space-y-4' : 'grid gap-4 lg:grid-cols-[minmax(0,1.25fr)_minmax(0,1fr)]'}>
        {!updateRequestId && <div className="min-w-0 space-y-4"><section className="glass-panel min-w-0 space-y-3 border-emerald-500/20 px-4 py-4">
          <h3 className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">Пациент и тип заявки</h3>
          <div>
          <p className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Тип заявки</p>
          <div className="inline-flex flex-wrap gap-1 rounded-full bg-slate-900/80 p-1" role="group" aria-label="Тип заявки">
            <button type="button" aria-pressed={form.mode === 'manual'} onClick={() => updateForm({ mode: 'manual' })}
              className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${form.mode === 'manual' ? 'bg-emerald-500 text-slate-950' : 'text-slate-300 hover:bg-slate-800/80'}`}>Вручную</button>
            <button type="button" aria-pressed={form.mode === 'referral'} onClick={() => updateForm({ mode: 'referral' })}
              className={`inline-flex items-center rounded-full px-3 py-1.5 text-[11px] font-medium transition ${form.mode === 'referral' ? 'bg-emerald-500 text-slate-950' : 'text-slate-300 hover:bg-slate-800/80'}`}>По направлению</button>
          </div>
          </div>
          {form.mode === 'manual' ? <>
            <StatusMessage onRetry={loadPatients}>{patientsError}</StatusMessage>
            <StatusMessage onRetry={() => { setLookupError(''); setLookupRetry(prev => prev + 1); }}>{lookupError}</StatusMessage>
            {loading && <p role="status">Загружаем пациентов...</p>}
            {!loading && !patientsError && !patients.length && <p className="text-sm text-slate-300">Сначала добавьте пациента в рабочем кабинете.</p>}
            <div>
              <label htmlFor="request-patient" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Пациент *</label>
              <Select id="request-patient" required value={form.selectedPatientId} onChange={changePatient} disabled={loading || !!patientsError}
                options={patients.map(p => ({ value: p.id, label: [p.patientLastName, p.patientFirstName, p.patientMiddleName].filter(Boolean).join(' ') + ' · ' + (p.lpuShortName || 'ЛПУ') }))}
                placeholder="Выберите пациента" />
            </div>
            {selectedPatient && <div className="rounded-xl border border-emerald-500/30 bg-slate-950/60 px-3 py-2 text-[11px] text-slate-300">
              <p><span className="font-medium text-slate-100">Пациент:</span>{' '}
                {[selectedPatient.patientLastName, selectedPatient.patientFirstName, selectedPatient.patientMiddleName].filter(Boolean).join(' ') || selectedPatient.id}
              </p>
              <p><span className="font-medium text-slate-100">Поликлиника:</span>{' '}{selectedPatient.lpuShortName || '—'}</p>
            </div>}
            <div>
              <label htmlFor="request-speciality" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Специальность *</label>
              <Select id="request-speciality" required value={form.selectedSpecialityId} onChange={changeSpeciality}
                disabled={!selectedPatient || specialitiesLoading || !!lookupError} options={specialities.map(s => ({ value: s.id, label: s.name || s.ferId || String(s.id) }))}
                placeholder={specialitiesLoading ? 'Загружаем специальности...' : 'Выберите специальность'} />
            </div>
            <DoctorPicker label="Выбор врача" mode={form.doctorMode} doctors={doctors} selectedIds={form.selectedDoctorIds} loading={doctorsLoading}
              onModeChange={value => { if (value !== form.doctorMode) { updateForm({ doctorMode: value, selectedDoctorIds: [] }); setDoctorsLoading(false); } }}
              onToggle={id => toggleDoctor('selectedDoctorIds', id)} />
          </> : <>
            <div>
              <label htmlFor="referral-number" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Номер направления</label>
              <input id="referral-number" value={form.referralNumber} maxLength={50} onChange={e => changeReferral('referralNumber', e.target.value)}
                className="input-field" placeholder="Введите номер направления" />
            </div>
            <div>
              <label htmlFor="referral-last-name" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Фамилия пациента</label>
              <input id="referral-last-name" value={form.referralLastName} onChange={e => changeReferral('referralLastName', e.target.value)}
                className="input-field" placeholder="Фамилия из направления" />
            </div>
            <button type="button" disabled={referralValidating} onClick={validateReferral} className="btn-secondary w-full justify-center text-xs">
              {referralValidating ? 'Проверяем направление...' : 'Проверить направление'}
            </button>
            {form.referralResult && <>
              <div role="status" className="rounded-xl border border-emerald-500/30 bg-slate-950/60 px-3 py-2 text-[11px] text-slate-300">
                <p className="mb-1 text-[10px] font-semibold uppercase tracking-[0.15em] text-emerald-400">Направление проверено.</p>
                <p><span className="font-medium text-slate-100">Пациент:</span>{' '}{[form.referralResult.patientProfile.patientLastName,
                  form.referralResult.patientProfile.patientFirstName, form.referralResult.patientProfile.patientMiddleName].filter(Boolean).join(' ')}</p>
                <p><span className="font-medium text-slate-100">Поликлиника:</span>{' '}{form.referralResult.patientProfile.lpuShortName || '—'}</p>
              </div>
              <div>
                <label htmlFor="referral-speciality" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Специальность из направления *</label>
                <Select id="referral-speciality" required value={form.referralSelectedSpecialityId}
                  onChange={value => updateForm({ referralSelectedSpecialityId: value, referralSelectedDoctorIds: [] })}
                  options={(form.referralResult.specialities || []).map(s => ({ value: s.id, label: s.name || s.ferId || String(s.id) }))}
                  placeholder="Выберите специальность" />
              </div>
              <DoctorPicker label="Выбор врача по направлению" mode={form.referralDoctorMode} doctors={referralSpeciality?.doctors || []}
                selectedIds={form.referralSelectedDoctorIds} onModeChange={value => { if (value !== form.referralDoctorMode) updateForm({ referralDoctorMode: value, referralSelectedDoctorIds: [] }); }}
                onToggle={id => toggleDoctor('referralSelectedDoctorIds', id)} />
            </>}
          </>}
        </section></div>}
        <div className="min-w-0 space-y-4">
          <section className="glass-panel border-emerald-500/20 px-4 py-4">
            <h3 className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">Временные предпочтения</h3>
            <StatusMessage onRetry={loadPresets}>{presetsError}</StatusMessage>
            {presetsLoading && <p role="status">Загружаем шаблоны...</p>}
            <div>
              <label htmlFor="request-preset" className="field-label">Шаблон времени *</label>
              <Select id="request-preset" size="sm" required value={form.selectedPresetName} onChange={value => updateForm({ selectedPresetName: value })}
                disabled={presetsLoading || !!presetsError} options={presetOptions} placeholder="Выберите шаблон" />
              {!presetsLoading && !presetsError && !presetNames.length && <p className="mt-2 text-sm text-slate-300">Создайте шаблон времени, чтобы продолжить.</p>}
            </div>
            <div className="mb-3 mt-3 flex flex-wrap items-center gap-2">
              <button type="button" disabled={referralValidating} className="btn-secondary text-xs px-3 py-1.5" onClick={() => navigate('/time-preferences', {
                state: { returnTo: location.pathname + location.search, fromRequestState: form },
              })}>Управление шаблонами</button>
              <button type="button" className="btn-secondary w-full justify-center text-xs sm:w-auto" onClick={() => setAdvancedOpen(true)}>Настройки поиска</button>
            </div>
            <div className="mt-4 space-y-2 border-t border-slate-800 pt-3">
              <h4 className="mb-2 text-[11px] font-semibold uppercase tracking-[0.18em] text-emerald-300">Время запуска поиска</h4>
              <p className="mb-2 text-[11px] text-slate-400">При необходимости укажите конкретное время запуска, например 08:00.</p>
              {form.specificPoints.map((point, index) => <div key={index} className="flex flex-col gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 sm:flex-row sm:items-center">
                <div className="min-w-0 flex-1">
                  <label htmlFor={`start-point-${index}`} className="field-label">Время запуска {index + 1}</label>
                  <input id={`start-point-${index}`} type="time" value={point.time} className="input-field text-xs"
                    onChange={e => updateForm({ specificPoints: form.specificPoints.map((p, i) => i === index ? { time: e.target.value } : p) })} />
                </div>
                <button type="button" className="text-[11px] text-slate-400 hover:text-rose-300" aria-label={`Удалить время запуска ${index + 1}`}
                  onClick={() => updateForm({ specificPoints: form.specificPoints.filter((_, i) => i !== index) })}>×</button>
              </div>)}
              <button type="button" className="btn-secondary mt-1 w-full justify-center text-xs" onClick={() => updateForm({ specificPoints: [...form.specificPoints, { time: '' }] })}>Добавить время запуска</button>
            </div>
          </section>
          <div className="flex justify-end">
            <button type="submit" disabled={submitting || presetsLoading || !!presetsError || referralValidating ||
              (!updateRequestId && form.mode === 'manual' && (loading || !!patientsError || !!lookupError || specialitiesLoading || doctorsLoading))}
              className="btn-primary px-5 py-2 text-sm">{submitting ? 'Сохраняем...' : updateRequestId ? 'Сохранить изменения' : 'Создать заявку'}</button>
          </div>
        </div>
      </fieldset>
    </ValidatedForm>
    <Modal isOpen={advancedOpen} onClose={() => setAdvancedOpen(false)} title="Настройки поиска">
      <div className="space-y-4">
        <div>
          <label htmlFor="search-interval" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Интервал поиска (мин)</label>
          <input id="search-interval" type="number" min={1} max={1440} value={form.searchIntervalMinutes} className="input-field"
            onChange={e => updateForm({ searchIntervalMinutes: e.target.value })} />
          <p className="mt-1 text-[11px] text-slate-400">От 1 до 1440 минут между попытками поиска.</p>
        </div>
        <div>
          <label htmlFor="search-days" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Максимум дней для поиска</label>
          <input id="search-days" type="number" min={1} max={365} value={form.maxDaysToSearch} className="input-field"
            onChange={e => updateForm({ maxDaysToSearch: e.target.value })} />
        </div>
        <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/60 px-4 py-3 text-xs text-slate-200">
          <input type="checkbox" checked={form.viewOnly} onChange={e => updateForm({ viewOnly: e.target.checked })} className="h-4 w-4 rounded border-slate-600 bg-slate-900" />
          Только просмотр (без автоматической записи)
        </label>
        <div className="flex justify-end"><button type="button" className="btn-primary" onClick={() => setAdvancedOpen(false)}>Готово</button></div>
      </div>
    </Modal>
  </div>;
}

export default CreateRequest;
