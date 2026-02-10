import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Modal from '../components/Modal';
import Select from '../components/Select';
import * as api from '../services/api';

const TIME_MODE = { ANY: 0, WEEKDAYS: 1, DATES: 2 };
const WEEKDAYS = [
  { value: 1, label: 'Пн' },
  { value: 2, label: 'Вт' },
  { value: 3, label: 'Ср' },
  { value: 4, label: 'Чт' },
  { value: 5, label: 'Пт' },
  { value: 6, label: 'Сб' },
  { value: 0, label: 'Вс' },
];

const PRESETS_STORAGE_KEY = 'gorzdrav_time_presets';

const defaultForm = () => ({
  name: '',
  timeMode: TIME_MODE.ANY,
  maxDaysAhead: 14,
  minHoursFromNow: 2,
  weekdayPrefs: [{ day: 1, from: '09:00', to: '13:00' }, { day: 3, from: '14:00', to: '18:00' }],
  datePrefs: [],
});

function TimePreferences() {
  const navigate = useNavigate();
  const location = useLocation();
  const returnTo = location.state?.returnTo;
  const fromRequestState = location.state?.fromRequestState;
  const { userId } = useAuth();
  const [presetNames, setPresetNames] = useState([]);
  const [loading, setLoading] = useState(true);
  const [formOpen, setFormOpen] = useState(false);
  const [editingName, setEditingName] = useState(null);
  const [form, setForm] = useState(defaultForm);
  const [submitError, setSubmitError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [deleteModal, setDeleteModal] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const loadPresetNames = useCallback(async () => {
    setLoading(true);
    try {
      let fromStorage = [];
      try {
        const raw = localStorage.getItem(PRESETS_STORAGE_KEY);
        fromStorage = raw ? JSON.parse(raw) : [];
      } catch {
        fromStorage = [];
      }
      const fromRequests = [];
      try {
        const requests = await api.getRequests();
        (requests || []).forEach(r => {
          if (r.timePreferencesPresetName && !fromRequests.includes(r.timePreferencesPresetName))
            fromRequests.push(r.timePreferencesPresetName);
        });
      } catch {
        // ignore
      }
      const names = [...new Set([...fromStorage.map(p => p.name), ...fromRequests])].filter(Boolean).sort();
      setPresetNames(names);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPresetNames();
  }, [loadPresetNames]);

  const savePresetsToStorage = useCallback(name => {
    try {
      const raw = localStorage.getItem(PRESETS_STORAGE_KEY);
      const list = raw ? JSON.parse(raw) : [];
      if (!list.some(p => p.name === name))
        list.push({ name, mode: TIME_MODE.ANY, createdAt: new Date().toISOString() });
      localStorage.setItem(PRESETS_STORAGE_KEY, JSON.stringify(list));
    } catch {
      // ignore
    }
  }, []);

  const removePresetFromStorage = useCallback(name => {
    try {
      const raw = localStorage.getItem(PRESETS_STORAGE_KEY);
      const list = raw ? JSON.parse(raw) : [];
      localStorage.setItem(
        PRESETS_STORAGE_KEY,
        JSON.stringify(list.filter(p => p.name !== name)),
      );
    } catch {
      // ignore
    }
  }, []);

  const buildPayload = useCallback(
    (presetName) => {
      if (!userId) return [];
      const { timeMode, weekdayPrefs, datePrefs, maxDaysAhead, minHoursFromNow } = form;
      if (timeMode === TIME_MODE.ANY) {
        return [
          {
            name: presetName,
            userId,
            timeMode: TIME_MODE.ANY,
            date: null,
            day: 0,
            preferredTimeFrom: null,
            preferredTimeTo: null,
            excludedDates: null,
            maxDaysAhead,
            minHoursFromNow,
          },
        ];
      }
      if (timeMode === TIME_MODE.WEEKDAYS) {
        return weekdayPrefs.map(pref => ({
          name: presetName,
          userId,
          timeMode: TIME_MODE.WEEKDAYS,
          date: null,
          day: pref.day,
          preferredTimeFrom: pref.from || null,
          preferredTimeTo: pref.to || null,
          excludedDates: null,
          maxDaysAhead,
          minHoursFromNow,
        }));
      }
      return datePrefs.map(pref => ({
        name: presetName,
        userId,
        timeMode: TIME_MODE.DATES,
        date: pref.date || null,
        day: 0,
        preferredTimeFrom: pref.from || null,
        preferredTimeTo: pref.to || null,
        excludedDates: null,
        maxDaysAhead,
        minHoursFromNow,
      }));
    },
    [userId, form],
  );

  const openCreate = () => {
    setEditingName(null);
    setForm(defaultForm());
    setSubmitError('');
    setFormOpen(true);
  };

  const openEdit = async (name) => {
    setSubmitError('');
    setFormOpen(true);
    setEditingName(name);
    try {
      const data = await api.getTimePreferences(name);
      const timeMode = data?.timeMode ?? TIME_MODE.ANY;
      const prefs = data?.preferences ?? [];
      let weekdayPrefs = [{ day: 1, from: '09:00', to: '13:00' }];
      let datePrefs = [];
      if (timeMode === TIME_MODE.WEEKDAYS && prefs.length)
        weekdayPrefs = prefs.map(p => ({ day: p.day ?? 1, from: p.from ?? '09:00', to: p.to ?? '13:00' }));
      else if (timeMode === TIME_MODE.DATES && prefs.length)
        datePrefs = prefs.map(p => ({ date: p.date ?? '', from: p.from ?? '09:00', to: p.to ?? '13:00' }));
      setForm({
        name,
        timeMode,
        maxDaysAhead: data?.maxDaysAhead ?? 14,
        minHoursFromNow: data?.minHoursFromNow ?? 2,
        weekdayPrefs,
        datePrefs,
      });
    } catch {
      setForm({ ...defaultForm(), name });
    }
  };

  const handleFormSubmit = async (e) => {
    e.preventDefault();
    setSubmitError('');
    const presetName = (editingName || form.name || '').trim();
    if (!presetName) {
      setSubmitError('Укажите название пресета.');
      return;
    }
    const payload = buildPayload(presetName);
    if (!payload.length) {
      setSubmitError('Заполните временные предпочтения.');
      return;
    }
    setSubmitting(true);
    try {
      await api.createTimePreferences(payload);
      savePresetsToStorage(presetName);
      setFormOpen(false);
      loadPresetNames();
    } catch (err) {
      setSubmitError(err?.message || 'Не удалось сохранить пресет.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteModal) return;
    setDeleting(true);
    try {
      await api.deleteTimePreferences(deleteModal);
      removePresetFromStorage(deleteModal);
      setDeleteModal(null);
      loadPresetNames();
    } catch (err) {
      setSubmitError(err?.message || 'Не удалось удалить пресет.');
    } finally {
      setDeleting(false);
    }
  };

  const updateForm = (updates) => setForm(prev => ({ ...prev, ...updates }));
  const updateWeekdayPref = (index, field, value) =>
    setForm(prev => ({
      ...prev,
      weekdayPrefs: prev.weekdayPrefs.map((p, i) => (i === index ? { ...p, [field]: value } : p)),
    }));
  const addWeekdayPref = () =>
    setForm(prev => ({
      ...prev,
      weekdayPrefs: [...prev.weekdayPrefs, { day: 1, from: '09:00', to: '13:00' }],
    }));
  const removeWeekdayPref = (index) =>
    setForm(prev => ({ ...prev, weekdayPrefs: prev.weekdayPrefs.filter((_, i) => i !== index) }));
  const updateDatePref = (index, field, value) =>
    setForm(prev => ({
      ...prev,
      datePrefs: prev.datePrefs.map((p, i) => (i === index ? { ...p, [field]: value } : p)),
    }));
  const addDatePref = () =>
    setForm(prev => ({
      ...prev,
      datePrefs: [...prev.datePrefs, { date: '', from: '09:00', to: '13:00' }],
    }));
  const removeDatePref = (index) =>
    setForm(prev => ({ ...prev, datePrefs: prev.datePrefs.filter((_, i) => i !== index) }));

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-emerald-300">
            Настройки
          </p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50 sm:text-2xl">
            Временные предпочтения
          </h2>
          <p className="mt-1 text-xs text-slate-400 sm:text-sm">
            Создавайте и редактируйте пресеты времени для запросов на запись.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          {returnTo && (
            <button
              type="button"
              onClick={() =>
                navigate(returnTo, {
                  state: fromRequestState
                    ? { restoredRequestForm: fromRequestState }
                    : undefined,
                })
              }
              className="btn-secondary px-3 py-2 text-xs"
            >
              ← Назад к созданию запроса
            </button>
          )}
          <button
            type="button"
            onClick={() => navigate('/dashboard')}
            className="btn-secondary px-3 py-2 text-xs"
          >
            Назад к дашборду
          </button>
          <button
            type="button"
            onClick={openCreate}
            className="btn-primary px-4 py-2 text-xs"
          >
            + Создать пресет
          </button>
        </div>
      </div>

      {loading ? (
        <div className="glass-panel border-emerald-500/20 px-6 py-8 text-center text-sm text-slate-400">
          Загружаем список пресетов...
        </div>
      ) : presetNames.length === 0 ? (
        <div className="glass-panel border-emerald-500/20 px-6 py-12 text-center">
          <p className="text-sm text-slate-400">
            Пресетов пока нет. Создайте первый — он пригодится при создании запросов на запись.
          </p>
          <button
            type="button"
            onClick={openCreate}
            className="btn-primary mt-4 px-4 py-2 text-sm"
          >
            Создать пресет
          </button>
        </div>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {presetNames.map(name => (
            <div
              key={name}
              className="glass-panel flex items-center justify-between gap-3 border-emerald-500/20 px-4 py-3"
            >
              <span className="truncate text-sm font-medium text-slate-100">{name}</span>
              <div className="flex shrink-0 gap-1">
                <button
                  type="button"
                  onClick={() => openEdit(name)}
                  className="rounded-lg border border-slate-600/80 bg-slate-800/60 px-2.5 py-1.5 text-[11px] font-medium text-slate-200 transition hover:bg-slate-700/60"
                >
                  Изменить
                </button>
                <button
                  type="button"
                  onClick={() => setDeleteModal(name)}
                  className="rounded-lg border border-rose-500/40 bg-rose-500/10 px-2.5 py-1.5 text-[11px] font-medium text-rose-200 transition hover:bg-rose-500/20"
                >
                  Удалить
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Модалка создания/редактирования */}
      <Modal
        isOpen={formOpen}
        onClose={() => !submitting && setFormOpen(false)}
        title={editingName ? 'Редактировать пресет' : 'Новый пресет'}
      >
        <form onSubmit={handleFormSubmit} className="space-y-4">
          {submitError && (
            <div className="rounded-xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-xs text-rose-100">
              {submitError}
            </div>
          )}

          <div>
            <label className="mb-1 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
              Название пресета
            </label>
            <input
              value={editingName ?? form.name}
              onChange={e => !editingName && updateForm({ name: e.target.value })}
              readOnly={!!editingName}
              className="input-field text-sm"
              placeholder="Например: Утро будних, после 18:00"
            />
          </div>

          <div className="inline-flex gap-1 rounded-full bg-slate-900/80 p-1">
            {[TIME_MODE.ANY, TIME_MODE.WEEKDAYS, TIME_MODE.DATES].map(mode => (
              <button
                key={mode}
                type="button"
                onClick={() => updateForm({ timeMode: mode })}
                className={`inline-flex rounded-full px-3 py-1.5 text-[11px] font-medium transition ${
                  form.timeMode === mode
                    ? 'bg-emerald-500 text-slate-950'
                    : 'text-slate-300 hover:bg-slate-800/80'
                }`}
              >
                {mode === TIME_MODE.ANY ? 'Любое время' : mode === TIME_MODE.WEEKDAYS ? 'По дням недели' : 'По датам'}
              </button>
            ))}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-300">Макс. дней вперёд</label>
              <input
                type="number"
                min={1}
                max={60}
                value={form.maxDaysAhead}
                onChange={e => updateForm({ maxDaysAhead: Number(e.target.value) || 1 })}
                className="input-field text-sm"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-300">Мин. часов от сейчас</label>
              <input
                type="number"
                min={0}
                max={72}
                value={form.minHoursFromNow}
                onChange={e => updateForm({ minHoursFromNow: Number(e.target.value) || 0 })}
                className="input-field text-sm"
              />
            </div>
          </div>

          {form.timeMode === TIME_MODE.WEEKDAYS && (
            <div className="space-y-2">
              <span className="text-xs font-medium text-slate-300">Интервалы по дням</span>
              {form.weekdayPrefs.map((pref, i) => (
                <div
                  key={i}
                  className="flex flex-wrap items-center gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2"
                >
                  <Select
                    value={pref.day}
                    onChange={v => updateWeekdayPref(i, 'day', Number(v))}
                    options={WEEKDAYS.map(d => ({ value: d.value, label: d.label }))}
                    placeholder="День"
                    size="sm"
                  />
                  <input
                    type="time"
                    value={pref.from}
                    onChange={e => updateWeekdayPref(i, 'from', e.target.value)}
                    className="input-field w-24 text-xs"
                  />
                  <input
                    type="time"
                    value={pref.to}
                    onChange={e => updateWeekdayPref(i, 'to', e.target.value)}
                    className="input-field w-24 text-xs"
                  />
                  <button type="button" onClick={() => removeWeekdayPref(i)} className="text-slate-400 hover:text-rose-300 text-xs">
                    ✕
                  </button>
                </div>
              ))}
              <button type="button" onClick={addWeekdayPref} className="btn-secondary w-full justify-center text-xs">
                Добавить интервал
              </button>
            </div>
          )}

          {form.timeMode === TIME_MODE.DATES && (
            <div className="space-y-2">
              <span className="text-xs font-medium text-slate-300">Интервалы по датам</span>
              {form.datePrefs.map((pref, i) => (
                <div
                  key={i}
                  className="flex flex-wrap items-center gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2"
                >
                  <input
                    type="date"
                    value={pref.date}
                    onChange={e => updateDatePref(i, 'date', e.target.value)}
                    className="input-field w-36 text-xs"
                  />
                  <input
                    type="time"
                    value={pref.from}
                    onChange={e => updateDatePref(i, 'from', e.target.value)}
                    className="input-field w-24 text-xs"
                  />
                  <input
                    type="time"
                    value={pref.to}
                    onChange={e => updateDatePref(i, 'to', e.target.value)}
                    className="input-field w-24 text-xs"
                  />
                  <button type="button" onClick={() => removeDatePref(i)} className="text-slate-400 hover:text-rose-300 text-xs">
                    ✕
                  </button>
                </div>
              ))}
              <button type="button" onClick={addDatePref} className="btn-secondary w-full justify-center text-xs">
                Добавить дату
              </button>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={() => !submitting && setFormOpen(false)}
              className="btn-secondary px-4 py-2 text-sm"
            >
              Отмена
            </button>
            <button type="submit" disabled={submitting} className="btn-primary px-4 py-2 text-sm">
              {submitting ? 'Сохраняем...' : editingName ? 'Сохранить' : 'Создать'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Модалка подтверждения удаления */}
      <Modal
        isOpen={!!deleteModal}
        onClose={() => !deleting && setDeleteModal(null)}
        title="Удалить пресет?"
      >
        {deleteModal && (
          <div className="space-y-4">
            <p className="text-sm text-slate-300">
              Пресет <span className="font-semibold text-slate-100">{deleteModal}</span> будет удалён.
              Запросы, которые его используют, останутся, но пресет нельзя будет выбрать заново.
            </p>
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setDeleteModal(null)}
                disabled={deleting}
                className="btn-secondary px-4 py-2 text-sm"
              >
                Отмена
              </button>
              <button
                type="button"
                onClick={handleDelete}
                disabled={deleting}
                className="rounded-xl bg-rose-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-rose-400 disabled:opacity-60"
              >
                {deleting ? 'Удаляем...' : 'Удалить'}
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default TimePreferences;
