import ValidatedForm from '../components/ValidatedForm';
import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Modal from '../components/Modal';
import Select from '../components/Select';
import StatusMessage from '../components/StatusMessage';
import * as api from '../services/api';

const TIME_MODE = { ANY: 0, WEEKDAYS: 1, DATES: 2 };
const WEEKDAYS = [
  { value: 1, label: 'Пн' }, { value: 2, label: 'Вт' }, { value: 3, label: 'Ср' },
  { value: 4, label: 'Чт' }, { value: 5, label: 'Пт' }, { value: 6, label: 'Сб' }, { value: 0, label: 'Вс' },
];
const defaultForm = () => ({
  name: '', timeMode: TIME_MODE.ANY, maxDaysAhead: 14, minHoursFromNow: 2, excludedDates: [],
  weekdayPrefs: [{ day: 1, from: '09:00', to: '13:00' }, { day: 3, from: '14:00', to: '18:00' }],
  datePrefs: [],
});

function TimePreferences() {
  const navigate = useNavigate();
  const location = useLocation();
  const { returnTo, fromRequestState } = location.state || {};
  const { userId } = useAuth();
  const mutationLock = useRef(false);
  const [presetNames, setPresetNames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [editingName, setEditingName] = useState(null);
  const [editingLoading, setEditingLoading] = useState(false);
  const [editingError, setEditingError] = useState('');
  const [form, setForm] = useState(defaultForm);
  const [submitError, setSubmitError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [deleteModal, setDeleteModal] = useState(null);
  const [deleteError, setDeleteError] = useState('');
  const [deleting, setDeleting] = useState(false);

  const loadPresetNames = useCallback(async () => {
    setLoading(true);
    setLoadError('');
    try {
      const presets = await api.getAllTimePreferences();
      setPresetNames([...new Set((presets || []).map(p => p.name).filter(Boolean))].sort());
    } catch {
      setLoadError('Не удалось загрузить шаблоны времени.');
    } finally { setLoading(false); }
  }, []);

  useEffect(() => { loadPresetNames(); }, [loadPresetNames]);

  const openCreate = () => {
    setEditingName(null);
    setEditingError('');
    setForm(defaultForm());
    setSubmitError('');
    setFormOpen(true);
  };

  const openEdit = async name => {
    setSubmitError('');
    setEditingError('');
    setEditingName(name);
    setEditingLoading(true);
    setFormOpen(true);
    try {
      const data = await api.getTimePreferences(name);
      const timeMode = data.timeMode;
      const preferences = data.preferences || [];
      setForm({
        ...defaultForm(), name, timeMode, maxDaysAhead: data.maxDaysAhead,
        minHoursFromNow: data.minHoursFromNow, excludedDates: data.excludedDates || [],
        weekdayPrefs: timeMode === TIME_MODE.WEEKDAYS
          ? preferences.map(p => ({ day: p.day, from: p.from?.slice(0, 5) || '', to: p.to?.slice(0, 5) || '' }))
          : defaultForm().weekdayPrefs,
        datePrefs: timeMode === TIME_MODE.DATES
          ? preferences.map(p => ({ date: p.date || '', from: p.from?.slice(0, 5) || '', to: p.to?.slice(0, 5) || '' }))
          : [],
      });
    } catch {
      setEditingError('Не удалось загрузить шаблон. Изменения недоступны, пока данные не загружены.');
    } finally { setEditingLoading(false); }
  };

  const handleFormSubmit = async event => {
    event.preventDefault();
    if (mutationLock.current || editingLoading || editingError) return;
    setSubmitError('');
    const name = (editingName || form.name).trim();
    if (!name) { setSubmitError('Укажите название шаблона.'); return; }
    const preferences = form.timeMode === TIME_MODE.ANY ? [{}]
      : form.timeMode === TIME_MODE.WEEKDAYS ? form.weekdayPrefs : form.datePrefs;
    if (!preferences.length) { setSubmitError('Добавьте хотя бы один интервал.'); return; }
    if (preferences.some(p => p.from && p.to && p.from >= p.to)) {
      setSubmitError('Начало интервала должно быть раньше окончания.'); return;
    }
    const payload = preferences.map(p => ({
      name, userId, timeMode: form.timeMode,
      date: form.timeMode === TIME_MODE.DATES ? p.date : null,
      day: form.timeMode === TIME_MODE.WEEKDAYS ? p.day : null,
      preferredTimeFrom: p.from || null, preferredTimeTo: p.to || null,
      excludedDates: form.excludedDates.length ? form.excludedDates : null,
      maxDaysAhead: Number(form.maxDaysAhead), minHoursFromNow: Number(form.minHoursFromNow),
    }));
    mutationLock.current = true;
    setSubmitting(true);
    try {
      if (editingName) await api.updateTimePreferences(editingName, payload);
      else await api.createTimePreferences(payload);
      setFormOpen(false);
      await loadPresetNames();
    } catch (error) { setSubmitError(error.message || 'Не удалось сохранить шаблон.'); }
    finally { mutationLock.current = false; setSubmitting(false); }
  };

  const handleDelete = async () => {
    if (!deleteModal || mutationLock.current) return;
    mutationLock.current = true;
    setDeleting(true);
    setDeleteError('');
    try {
      await api.deleteTimePreferences(deleteModal);
      setDeleteModal(null);
      await loadPresetNames();
    } catch (error) { setDeleteError(error.message || 'Не удалось удалить шаблон.'); }
    finally { mutationLock.current = false; setDeleting(false); }
  };

  const updateForm = updates => setForm(prev => ({ ...prev, ...updates }));
  const updatePreference = (key, index, field, value) => setForm(prev => ({
    ...prev, [key]: prev[key].map((p, i) => i === index ? { ...p, [field]: value } : p),
  }));
  const removePreference = (key, index) => setForm(prev => ({
    ...prev, [key]: prev[key].filter((_, i) => i !== index),
  }));
  const preferenceKey = form.timeMode === TIME_MODE.WEEKDAYS ? 'weekdayPrefs' : 'datePrefs';

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-emerald-300">Настройки</p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50 sm:text-2xl">Временные предпочтения</h2>
          <p className="mt-1 text-xs text-slate-400 sm:text-sm">Создавайте шаблоны времени для заявок на запись.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {returnTo && <button type="button" className="btn-secondary text-xs px-3 py-2" onClick={() => navigate(returnTo, {
            state: fromRequestState ? { restoredRequestForm: fromRequestState } : undefined,
          })}>← Назад к заявке</button>}
          <button type="button" onClick={() => navigate('/dashboard')} className="btn-secondary text-xs px-3 py-2">В рабочий кабинет</button>
          <button type="button" onClick={openCreate} className="btn-primary px-4 py-2 text-xs">+ Создать шаблон</button>
        </div>
      </div>
      <StatusMessage onRetry={loadPresetNames}>{loadError}</StatusMessage>
      {loading ? <div role="status" className="glass-panel border-emerald-500/20 px-6 py-8 text-center text-sm text-slate-400">Загружаем шаблоны...</div>
        : presetNames.length === 0 && !loadError ? <div className="glass-panel border-emerald-500/20 px-6 py-12 text-center">
          <p className="text-sm text-slate-400">Шаблонов пока нет. Создайте первый для заявки на запись.</p>
          <button type="button" onClick={openCreate} className="btn-primary mt-4">Создать шаблон</button>
        </div> : <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {presetNames.map(name => <div key={name} className="glass-panel flex min-w-0 items-center justify-between gap-3 border-emerald-500/20 px-4 py-3">
            <h3 className="break-words text-sm font-medium text-slate-100">{name}</h3>
            <div className="flex shrink-0 gap-1">
              <button type="button" onClick={() => openEdit(name)} className="rounded-lg border border-slate-600/80 bg-slate-800/60 px-2.5 py-1.5 text-[11px] font-medium text-slate-200 transition hover:bg-slate-700/60">Изменить</button>
              <button type="button" onClick={() => { setDeleteError(''); setDeleteModal(name); }} className="rounded-lg border border-rose-500/40 bg-rose-500/10 px-2.5 py-1.5 text-[11px] font-medium text-rose-200 transition hover:bg-rose-500/20">Удалить</button>
            </div>
          </div>)}
        </div>}
      <Modal isOpen={formOpen} onClose={() => setFormOpen(false)} busy={submitting || editingLoading}
        title={editingName ? 'Редактировать шаблон' : 'Новый шаблон'}>
        {editingLoading ? <p role="status">Загружаем шаблон...</p>
          : editingError ? <StatusMessage onRetry={() => openEdit(editingName)}>{editingError}</StatusMessage>
          : <ValidatedForm onSubmit={handleFormSubmit} className="space-y-4">
            <StatusMessage>{submitError}</StatusMessage>
            <fieldset disabled={submitting} className="flex min-w-0 flex-col gap-4">
              <div>
                <label htmlFor="preset-name" className="mb-1 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">Название шаблона</label>
                <input id="preset-name" value={editingName ?? form.name} onChange={e => updateForm({ name: e.target.value })}
                  readOnly={!!editingName} required maxLength={100} className="input-field" placeholder="Например: Утро будних" />
              </div>
              <div className="inline-flex max-w-full flex-none flex-wrap items-start self-start gap-1 rounded-full bg-slate-900/80 p-1" role="group" aria-label="Режим выбора времени">
                {[TIME_MODE.ANY, TIME_MODE.WEEKDAYS, TIME_MODE.DATES].map(mode => <button key={mode} type="button"
                  aria-pressed={form.timeMode === mode} onClick={() => updateForm({ timeMode: mode })}
                  className={`inline-flex h-7 flex-none items-center rounded-full px-3 text-[11px] font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-emerald-200/70 ${form.timeMode === mode ? 'bg-emerald-500 text-slate-950' : 'text-slate-300 hover:bg-slate-800/80'}`}>
                  {mode === TIME_MODE.ANY ? 'Любое время' : mode === TIME_MODE.WEEKDAYS ? 'По дням недели' : 'По датам'}
                </button>)}
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                <div>
                  <label htmlFor="max-days" className="field-label">Максимум дней вперёд</label>
                  <input id="max-days" type="number" min={1} max={365} required value={form.maxDaysAhead}
                    onChange={e => updateForm({ maxDaysAhead: e.target.value })} className="input-field" />
                </div>
                <div>
                  <label htmlFor="min-hours" className="field-label">Минимум часов до приёма</label>
                  <input id="min-hours" type="number" min={0} max={720} required value={form.minHoursFromNow}
                    onChange={e => updateForm({ minHoursFromNow: e.target.value })} className="input-field" />
                </div>
              </div>
              {form.timeMode !== TIME_MODE.ANY && <div className="space-y-2">
                <p className="text-xs font-medium text-slate-300">{form.timeMode === TIME_MODE.WEEKDAYS ? 'Интервалы по дням недели' : 'Интервалы по датам'}</p>
                {form[preferenceKey].map((pref, index) => <div key={index} className="time-interval">
                  <div className={form.timeMode === TIME_MODE.WEEKDAYS ? 'min-w-0 w-full' : 'min-w-0 w-36 max-w-full'}>
                    <label htmlFor={`interval-day-${index}`} className="field-label">{form.timeMode === TIME_MODE.WEEKDAYS ? 'День' : 'Дата'}</label>
                    {form.timeMode === TIME_MODE.WEEKDAYS
                      ? <Select id={`interval-day-${index}`} size="sm" value={pref.day} required options={WEEKDAYS}
                        onChange={value => updatePreference(preferenceKey, index, 'day', Number(value))} />
                      : <input id={`interval-day-${index}`} type="date" required value={pref.date}
                        onChange={e => updatePreference(preferenceKey, index, 'date', e.target.value)} className="input-field text-xs" />}
                  </div>
                  <div className="min-w-0 w-24 max-w-full">
                    <label htmlFor={`interval-from-${index}`} className="field-label">С</label>
                    <input id={`interval-from-${index}`} type="time" value={pref.from}
                      onChange={e => updatePreference(preferenceKey, index, 'from', e.target.value)} className="input-field text-xs" />
                  </div>
                  <div className="min-w-0 w-24 max-w-full">
                    <label htmlFor={`interval-to-${index}`} className="field-label">До</label>
                    <input id={`interval-to-${index}`} type="time" value={pref.to}
                      onChange={e => updatePreference(preferenceKey, index, 'to', e.target.value)} className="input-field text-xs" />
                  </div>
                  <button type="button" className="text-slate-400 hover:text-rose-300 text-xs interval-remove" aria-label={`Удалить интервал ${index + 1}`}
                    onClick={() => removePreference(preferenceKey, index)}>×</button>
                </div>)}
                <button type="button" className="btn-secondary w-full justify-center text-xs" onClick={() => updateForm({
                  [preferenceKey]: [...form[preferenceKey], form.timeMode === TIME_MODE.WEEKDAYS
                    ? { day: 1, from: '09:00', to: '13:00' } : { date: '', from: '09:00', to: '13:00' }],
                })}>Добавить интервал</button>
              </div>}
              <div className="flex flex-wrap justify-end gap-2 pt-2">
                <button type="button" onClick={() => setFormOpen(false)} className="btn-secondary px-4 py-2 text-sm">Отмена</button>
                <button type="submit" className="btn-primary">{submitting ? 'Сохраняем...' : editingName ? 'Сохранить' : 'Создать'}</button>
              </div>
            </fieldset>
          </ValidatedForm>}
      </Modal>
      <Modal isOpen={!!deleteModal} onClose={() => setDeleteModal(null)} busy={deleting} title="Удалить шаблон?">
        <div className="space-y-4">
          <StatusMessage>{deleteError}</StatusMessage>
          <p className="text-sm text-slate-300">Шаблон <strong className="break-words text-slate-100">{deleteModal}</strong> будет удалён.
            Заявки, которые его используют, останутся, но шаблон нельзя будет выбрать заново.</p>
          <div className="flex flex-wrap justify-end gap-2">
            <button type="button" disabled={deleting} onClick={() => setDeleteModal(null)} className="btn-secondary px-4 py-2 text-sm">Отмена</button>
            <button type="button" disabled={deleting} onClick={handleDelete} className="rounded-xl bg-rose-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-rose-400 disabled:opacity-60">{deleting ? 'Удаляем...' : 'Удалить'}</button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

export default TimePreferences;
