import ValidatedForm from '../components/ValidatedForm';
import { validateForm, markServerErrors } from '../utils/formValidation';
import { useState, useEffect, useCallback, useRef } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Modal from '../components/Modal';
import Select from '../components/Select';
import PatientCard from '../components/PatientCard';
import RequestCard from '../components/RequestCard';
import AppointmentCard from '../components/AppointmentCard';
import StatusMessage from '../components/StatusMessage';
import * as api from '../services/api';
import { useAuth } from '../context/AuthContext';

const formatShortFio = fullName => {
  if (!fullName) return 'Пациент';
  const parts = fullName.split(' ').filter(Boolean);
  if (parts.length === 0) return 'Пациент';
  return `${parts[0]}${parts[1] ? ` ${parts[1][0]}.` : ''}${
    parts[2] ? `${parts[2][0]}.` : ''
  }`;
};

function Dashboard() {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, username } = useAuth();
  const districtRequest = useRef(0);
  const patientCheck = useRef(0);
  const patientForm = useRef(null);
  const mutationLock = useRef(false);
  const checkPending = useRef(false);
  const [activeTab, setActiveTab] = useState(location.state?.activeTab || 'patients');
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Основные списки
  const [patients, setPatients] = useState([]);
  const [requests, setRequests] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [requestToManage, setRequestToManage] = useState(null);
  const [appointmentToCancel, setAppointmentToCancel] = useState(null);
  const [patientToDelete, setPatientToDelete] = useState(null);
  const [requestActionLoading, setRequestActionLoading] = useState(false);
  const [appointmentActionLoading, setAppointmentActionLoading] = useState(false);
  const [patientDeleteLoading, setPatientDeleteLoading] = useState(false);

  // Для создания пациента
  const [districts, setDistricts] = useState([]);
  const [lpus, setLpus] = useState([]);
  const [newPatient, setNewPatient] = useState({
    districtId: '',
    lpuId: '',
    patientLastName: '',
    patientFirstName: '',
    patientMiddleName: '',
    patientBirthdate: '',
    patientId: '', // Будет заполняться после проверки в Городздрав
    recipientEmail: '',
    mobilePhoneNumber: '',
  });

  const [loading, setLoading] = useState(true);
  const [creatingPatient, setCreatingPatient] = useState(false);
  const [checkingPatient, setCheckingPatient] = useState(false);
  const [formError, setFormError] = useState('');
  const [loadErrors, setLoadErrors] = useState({});
  const [actionError, setActionError] = useState('');
  const [lpusLoading, setLpusLoading] = useState(false);
  const activeRequests = requests.filter(request => ['Pending', 'InProgress'].includes(request.status));

  const loadData = useCallback(async () => {
    setLoading(true);
    const results = await Promise.allSettled([
      api.getPatients(), api.getRequests(), api.getAppointments(), api.getDistricts(),
    ]);
    const keys = ['patients', 'requests', 'appointments', 'districts'];
    const setters = [setPatients, setRequests, setAppointments, setDistricts];
    const messages = ['Не удалось загрузить пациентов.', 'Не удалось загрузить заявки.',
      'Не удалось загрузить записи к врачам.', 'Не удалось загрузить районы.'];
    const errors = {};
    results.forEach((result, index) => {
      if (result.status === 'fulfilled') setters[index](result.value || []);
      else errors[keys[index]] = messages[index];
    });
    setLoadErrors(errors);
    setLoading(false);
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  const invalidatePatientCheck = () => {
    checkPending.current = false;
    patientCheck.current += 1;
    setCheckingPatient(false);
    setNewPatient(prev => ({ ...prev, patientId: '' }));
  };

  // Загрузка поликлиник при выборе района
  const handleDistrictChange = async districtId => {
    invalidatePatientCheck();
    const requestId = ++districtRequest.current;
    setNewPatient(prev => ({ ...prev, districtId, lpuId: '' }));
    setLpus([]);
    setFormError('');
    setLpusLoading(!!districtId);
    if (districtId) {
      try {
        const lpusData = await api.getLpusByDistrict(districtId);
        if (requestId === districtRequest.current) setLpus(lpusData || []);
      } catch {
        if (requestId === districtRequest.current) setFormError('Не удалось загрузить поликлиники. Выберите район повторно.');
      } finally {
        if (requestId === districtRequest.current) setLpusLoading(false);
      }
    }
  };

  const validatePatientForm = () => {
    const valid = validateForm(patientForm.current);
    const birthdate = patientForm.current.elements.namedItem('patientBirthdate');
    const invalidName = [...patientForm.current.elements].find(field => field.name?.endsWith('Name') && field.validity?.patternMismatch);
    if (birthdate?.validity.rangeUnderflow) setFormError('Дата рождения не может быть более 150 лет назад');
    else if (birthdate?.validity.rangeOverflow) setFormError('Дата рождения должна быть в прошлом');
    else if (invalidName) setFormError('Фамилия, имя и отчество должны содержать только буквы, пробелы, дефисы или апострофы.');
    return valid;
  };

  // Проверка пациента в Городздрав (поиск patientId)
  const handleCheckPatient = async () => {
    if (checkPending.current || mutationLock.current || !validatePatientForm()) return;
    setFormError('');

    if (
      !newPatient.lpuId ||
      !newPatient.patientLastName ||
      !newPatient.patientFirstName ||
      !newPatient.patientMiddleName ||
      !newPatient.patientBirthdate
    ) {
      setFormError('Заполните поликлинику, фамилию, имя, отчество и дату рождения.');
      return;
    }

    setCheckingPatient(true);
    checkPending.current = true;
    const checkId = ++patientCheck.current;
    try {
      const patientId = await api.findPatientId({
        lpuId: newPatient.lpuId.toString(),
        lastName: newPatient.patientLastName,
        firstName: newPatient.patientFirstName,
        middleName: newPatient.patientMiddleName,
        birthDate: newPatient.patientBirthdate,
      });

      if (checkId === patientCheck.current) setNewPatient(prev => ({ ...prev, patientId }));
    } catch (error) {
      if (checkId === patientCheck.current) {
        markServerErrors(patientForm.current, error.validationErrors);
        setFormError(error.message || 'Не удалось проверить пациента в Городздрав.');
        setNewPatient(prev => ({ ...prev, patientId: '' }));
      }
    } finally {
      if (checkId === patientCheck.current) { checkPending.current = false; setCheckingPatient(false); }
    }
  };

  const handleInputChange = e => {
    if (['patientLastName', 'patientFirstName', 'patientMiddleName', 'patientBirthdate'].includes(e.target.name)) {
      invalidatePatientCheck();
    }
    setNewPatient(prev => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleCreatePatient = async e => {
    e.preventDefault();
    if (mutationLock.current || !validatePatientForm()) return;
    setFormError('');

    if (!newPatient.patientId) {
      setFormError('Сначала проверьте данные пациента в Городздрав.');
      return;
    }

    const selectedLpu = lpus.find(l => String(l.id) === String(newPatient.lpuId));

    mutationLock.current = true;
    setCreatingPatient(true);
    try {
      await api.createPatient({
        lpuId: newPatient.lpuId,
        lpuShortName: selectedLpu?.lpuShortName || selectedLpu?.lpuFullName,
        lpuAddress: selectedLpu?.address,
        patientId: newPatient.patientId,
        patientLastName: newPatient.patientLastName,
        patientFirstName: newPatient.patientFirstName,
        patientMiddleName: newPatient.patientMiddleName,
        patientBirthdate: newPatient.patientBirthdate,
        recipientEmail: newPatient.recipientEmail || null,
        mobilePhoneNumber: newPatient.mobilePhoneNumber || null,
      });

      setIsModalOpen(false);
      setNewPatient({
        districtId: '',
        lpuId: '',
        patientLastName: '',
        patientFirstName: '',
        patientMiddleName: '',
        patientBirthdate: '',
        patientId: '',
        recipientEmail: '',
        mobilePhoneNumber: '',
      });

      loadData(); // Перезагрузить список
    } catch (error) {
      markServerErrors(patientForm.current, error.validationErrors);
      setFormError(error.message || 'Ошибка создания пациента.');
    } finally {
      setCreatingPatient(false);
      mutationLock.current = false;
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const handleConfirmCancelRequest = async () => {
    if (!requestToManage || mutationLock.current) return;
    mutationLock.current = true;
    setActionError('');
    setRequestActionLoading(true);
    try {
      await api.deleteRequest(requestToManage.id);
      setRequestToManage(null);
      await loadData();
    } catch {
      setActionError('Не удалось отменить заявку. Попробуйте снова.');
    } finally {
      setRequestActionLoading(false);
      mutationLock.current = false;
    }
  };

  const handleConfirmCancelAppointment = async () => {
    if (!appointmentToCancel || mutationLock.current) return;
    mutationLock.current = true;
    setActionError('');
    setAppointmentActionLoading(true);
    try {
      await api.deleteAppointment(appointmentToCancel.id);
      setAppointmentToCancel(null);
      await loadData();
    } catch {
      setActionError('Не удалось отменить запись. Попробуйте снова.');
    } finally {
      setAppointmentActionLoading(false);
      mutationLock.current = false;
    }
  };

  const handleConfirmDeletePatient = async () => {
    if (!patientToDelete || mutationLock.current) return;
    mutationLock.current = true;
    setActionError('');
    setPatientDeleteLoading(true);
    try {
      await api.deletePatient(patientToDelete.id);
      setPatientToDelete(null);
      await loadData();
    } catch {
      setActionError('Не удалось удалить пациента. Попробуйте снова.');
    } finally {
      setPatientDeleteLoading(false);
      mutationLock.current = false;
    }
  };

  return (
    <div className="flex flex-col gap-6">
      <StatusMessage kind="success">{location.state?.notice}</StatusMessage>
      {/* Верхняя панель дашборда */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-emerald-300">
            Рабочий кабинет
          </p>
          <h2 className="mt-1 text-xl font-semibold text-slate-50 sm:text-2xl">
            Управление автоматическими заявками на запись
          </h2>
          <p className="mt-1 text-xs text-slate-400 sm:text-sm">
            Добавляйте пациентов, отслеживайте заявки и подтверждённые записи к врачу в одном месте.
          </p>
        </div>

        <div className="flex flex-col items-end gap-2 sm:flex-row sm:items-center">
          {username && (
            <span className="rounded-full border border-emerald-500/40 bg-emerald-500/10 px-3 py-1 text-[11px] font-medium text-emerald-100">
              Пользователь: <span className="font-mono">{username}</span>
            </span>
          )}
          <button
            type="button"
            onClick={() => navigate('/time-preferences')}
            className="btn-secondary text-xs px-3 py-1.5"
          >
            Временные предпочтения
          </button>
          <button
            type="button"
            onClick={handleLogout}
            className="btn-secondary text-xs px-3 py-1.5"
          >
            Выйти из аккаунта
          </button>
        </div>
      </div>

      {/* Быстрые метрики */}
      <div className="grid gap-3 sm:grid-cols-3">
        <div className="glass-panel border-emerald-500/20 px-4 py-3">
          <p className="text-[11px] font-medium uppercase tracking-[0.18em] text-emerald-300">
            Пациенты
          </p>
          <p className="mt-1 text-2xl font-semibold text-slate-50">
            {loading || loadErrors.patients ? '—' : patients.length}
          </p>
          <p className="mt-1 text-xs text-slate-400">
            Профили пациентов, с которыми ведётся работа.
          </p>
        </div>
        <div className="glass-panel border-emerald-500/20 px-4 py-3">
          <p className="text-[11px] font-medium uppercase tracking-[0.18em] text-emerald-300">
            Активные заявки
          </p>
          <p className="mt-1 text-2xl font-semibold text-slate-50">
            {loading || loadErrors.requests ? '—' : activeRequests.length}
          </p>
          <p className="mt-1 text-xs text-slate-400">
            Поиск доступных талонов в очереди.
          </p>
        </div>
        <div className="glass-panel border-emerald-500/20 px-4 py-3">
          <p className="text-[11px] font-medium uppercase tracking-[0.18em] text-emerald-300">
            Запланированные записи к врачу
          </p>
          <p className="mt-1 text-2xl font-semibold text-slate-50">
            {loading || loadErrors.appointments ? '—' : appointments.length}
          </p>
          <p className="mt-1 text-xs text-slate-400">
            Подтверждённые записи к врачам.
          </p>
        </div>
      </div>

      {/* Вкладки */}
      <div className="glass-panel border-emerald-500/20 px-3 py-2">
        <div className="inline-flex max-w-full flex-wrap gap-1 rounded-full bg-slate-900/80 p-1" role="group" aria-label="Раздел кабинета">
          <button
            type="button"
            onClick={() => setActiveTab('patients')}
            aria-pressed={activeTab === 'patients'}
            className={`inline-flex shrink-0 items-center gap-1 whitespace-nowrap rounded-full px-3 py-1.5 text-xs font-medium transition ${
              activeTab === 'patients'
                ? 'bg-emerald-500 text-slate-950'
                : 'text-slate-300 hover:bg-slate-800/80'
            }`}
          >
            <span>Пациенты</span>
            <span className="text-[10px] opacity-80">{patients.length}</span>
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('requests')}
            aria-pressed={activeTab === 'requests'}
            className={`inline-flex shrink-0 items-center gap-1 whitespace-nowrap rounded-full px-3 py-1.5 text-xs font-medium transition ${
              activeTab === 'requests'
                ? 'bg-emerald-500 text-slate-950'
                : 'text-slate-300 hover:bg-slate-800/80'
            }`}
          >
            <span>Заявки</span>
            <span className="text-[10px] opacity-80">{requests.length}</span>
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('appointments')}
            aria-pressed={activeTab === 'appointments'}
            className={`inline-flex shrink-0 items-center gap-1 whitespace-nowrap rounded-full px-3 py-1.5 text-xs font-medium transition ${
              activeTab === 'appointments'
                ? 'bg-emerald-500 text-slate-950'
                : 'text-slate-300 hover:bg-slate-800/80'
            }`}
          >
            <span>Записи к врачу</span>
            <span className="text-[10px] opacity-80">
              {appointments.length}
            </span>
          </button>
        </div>
      </div>

      {/* Контент вкладок */}
      <div className="glass-panel border-emerald-500/20 px-4 py-5">
        {loading ? (
          <div className="flex min-h-[180px] items-center justify-center text-sm text-slate-300">
            Загружаем данные...
          </div>
        ) : (
          <>
            {activeTab === 'patients' && (
              <>
                <div className="mb-3 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <h3 className="text-sm font-semibold text-slate-50">
                      Мои пациенты
                    </h3>
                    <p className="text-xs text-slate-400">
                      Карточки пациентов, с которыми будут создаваться заявки.
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => {
                      setFormError('');
                      setIsModalOpen(true);
                    }}
                    className="btn-primary text-xs px-3 py-2"
                  >
                    + Добавить пациента
                  </button>
                </div>

                <StatusMessage onRetry={loadData}>{loadErrors.patients}</StatusMessage>
                {patients.length === 0 && !loadErrors.patients ? (
                  <div className="rounded-2xl border border-dashed border-emerald-500/40 bg-slate-950/40 px-4 py-6 text-center text-xs text-slate-300">
                    Пока ни одного пациента. Нажмите{' '}
                    <span className="font-semibold text-emerald-200">
                      «Добавить пациента»
                    </span>
                    , чтобы создать первый профиль.
                  </div>
                ) : (
                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {patients.map(patient => (
                      <PatientCard
                        key={patient.id}
                        patient={patient}
                        onDelete={p => { setActionError(''); setPatientToDelete(p); }}
                      />
                    ))}
                  </div>
                )}
              </>
            )}

            {activeTab === 'requests' && (
              <>
                <div className="mb-3 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <h3 className="text-sm font-semibold text-slate-50">
                      Заявки на поиск талонов
                    </h3>
                    <p className="text-xs text-slate-400">
                      Список активных заявок на автопоиск записей.
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => navigate('/requests/new')}
                    className="btn-primary text-xs px-3 py-2"
                  >
                    + Создать заявку
                  </button>
                </div>
                <StatusMessage onRetry={loadData}>{loadErrors.requests}</StatusMessage>
                {requests.length === 0 && !loadErrors.requests ? (
                  <div className="rounded-2xl border border-dashed border-emerald-500/40 bg-slate-950/40 px-4 py-6 text-center text-xs text-slate-300">
                    Заявок пока нет. Нажмите{' '}
                    <span className="font-semibold text-emerald-200">
                      «Создать заявку»
                    </span>
                    , чтобы добавить первую заявку.
                  </div>
                ) : (
                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {requests.map(request => (
                      <div key={request.id} className="cursor-pointer">
                        <RequestCard
                          request={request}
                          onClick={() => { setActionError(''); setRequestToManage(request); }}
                        />
                      </div>
                    ))}
                  </div>
                )}
              </>
            )}

            {activeTab === 'appointments' && (
              <>
                <h3 className="text-sm font-semibold text-slate-50">
                  Запланированные записи к врачу
                </h3>
                <p className="mb-3 text-xs text-slate-400">
                  Список запланированных посещений к врачам.
                </p>
                <StatusMessage onRetry={loadData}>{loadErrors.appointments}</StatusMessage>
                {appointments.length === 0 && !loadErrors.appointments ? (
                  <div className="rounded-2xl border border-dashed border-emerald-500/40 bg-slate-950/40 px-4 py-6 text-center text-xs text-slate-300">
                    Подтверждённых посещений пока нет.
                  </div>
                ) : (
                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {appointments.map(appointment => (
                      <AppointmentCard
                        key={appointment.id}
                        appointment={appointment}
                        onCancel={() => { setActionError(''); setAppointmentToCancel(appointment); }}
                      />
                    ))}
                  </div>
                )}
              </>
            )}
          </>
        )}
      </div>

      {/* МОДАЛКА СОЗДАНИЯ ПАЦИЕНТА */}
      <Modal
        isOpen={isModalOpen}
        busy={creatingPatient || checkingPatient}
        onClose={() => {
          setIsModalOpen(false);
          setFormError('');
        }}
        title="Добавить пациента"
      >
        <ValidatedForm ref={patientForm} validate={validatePatientForm} onSubmit={handleCreatePatient} className="space-y-4">
          <fieldset disabled={creatingPatient} className="space-y-4">
          {formError && (
            <div role="alert" className="message-error">
              {formError}
            </div>
          )}

          {/* 1. Район */}
          <StatusMessage onRetry={loadData}>{loadErrors.districts}</StatusMessage>
          <div>
            <label htmlFor="patient-district" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
              Район *
            </label>
            <Select
              id="patient-district"
              value={newPatient.districtId}
              onChange={v => {
                handleDistrictChange(v);
              }}
              options={districts.map(d => ({ value: d.id, label: d.name }))}
              placeholder="Выберите район"
              required
            />
          </div>

          {/* 2. Поликлиника */}
          {newPatient.districtId && (
            <div>
              <label htmlFor="patient-lpu" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                Поликлиника *
              </label>
              <Select
                id="patient-lpu"
                value={newPatient.lpuId}
                onChange={v => { invalidatePatientCheck(); setNewPatient(prev => ({ ...prev, lpuId: v })); }}
                options={lpus.map(l => ({
                  value: String(l.id),
                  label: l.lpuShortName || l.lpuFullName || String(l.id),
                }))}
                placeholder={lpusLoading ? 'Загружаем поликлиники...' : 'Выберите поликлинику'}
                disabled={lpusLoading}
                required
              />
            </div>
          )}

          {/* 3. Поиск пациента */}
          {newPatient.lpuId && (
            <div className="border-t border-slate-800 pt-4">
              <h4 className="mb-3 text-xs font-semibold uppercase tracking-[0.18em] text-emerald-300">
                Проверка данных пациента в Городздрав
              </h4>
              <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
                <div>
                  <label htmlFor="patient-last-name" className="mb-1 block text-xs font-medium text-slate-300">
                    Фамилия *
                  </label>
                  <input
                    id="patient-last-name"
                    name="patientLastName"
                    pattern="[\p{L}]+(?:[\s'’\-][\p{L}]+)*"
                    maxLength={100}
                    value={newPatient.patientLastName}
                    onChange={handleInputChange}
                    className="input-field"
                    required
                  />
                </div>
                <div>
                  <label htmlFor="patient-first-name" className="mb-1 block text-xs font-medium text-slate-300">
                    Имя *
                  </label>
                  <input
                    id="patient-first-name"
                    name="patientFirstName"
                    pattern="[\p{L}]+(?:[\s'’\-][\p{L}]+)*"
                    maxLength={100}
                    value={newPatient.patientFirstName}
                    onChange={handleInputChange}
                    className="input-field"
                    required
                  />
                </div>
                <div>
                  <label htmlFor="patient-middle-name" className="mb-1 block text-xs font-medium text-slate-300">
                    Отчество *
                  </label>
                  <input
                    id="patient-middle-name"
                    name="patientMiddleName"
                    pattern="[\p{L}]+(?:[\s'’\-][\p{L}]+)*"
                    maxLength={100}
                    value={newPatient.patientMiddleName}
                    onChange={handleInputChange}
                    className="input-field"
                    required
                  />
                </div>
                <div>
                  <label htmlFor="patient-birthdate" className="mb-1 block text-xs font-medium text-slate-300">
                    Дата рождения *
                  </label>
                  <input
                    id="patient-birthdate"
                    type="date"
                    max={new Date().toLocaleDateString('en-CA')}
                    min={(() => { const date = new Date(); date.setFullYear(date.getFullYear() - 150); date.setDate(date.getDate() + 1); return date.toLocaleDateString('en-CA'); })()}
                    name="patientBirthdate"
                    value={newPatient.patientBirthdate}
                    onChange={handleInputChange}
                    className="input-field"
                    required
                  />
                </div>
              </div>

              <button
                type="button"
                onClick={handleCheckPatient}
                disabled={checkingPatient}
                className="btn-secondary mb-2 w-full justify-center text-xs"
              >
                {checkingPatient
                  ? 'Проверяем данные в Городздрав...'
                  : 'Проверить данные в Городздрав'}
              </button>
            </div>
          )}

          {/* 4. Дополнительные поля */}
          {newPatient.patientId && (
            <div className="space-y-3 border-t border-emerald-500/30 pt-4">
              <p className="text-xs text-emerald-200">
                Данные пациента подтверждены в Городздрав. Можно добавить
                контактную информацию.
              </p>
              <div>
                <label htmlFor="patient-email" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                  Email
                </label>
                <input
                  id="patient-email"
                  type="email"
                  autoComplete="email"
                  name="recipientEmail"
                  value={newPatient.recipientEmail}
                  onChange={handleInputChange}
                  className="input-field"
                />
              </div>
              <div>
                <label htmlFor="patient-phone" className="mb-2 block text-xs font-medium uppercase tracking-[0.16em] text-slate-300">
                  Телефон
                </label>
                <input
                  id="patient-phone"
                  type="tel"
                  inputMode="numeric"
                  pattern="[0-9]{10}"
                  aria-describedby="patient-phone-help"
                  name="mobilePhoneNumber"
                  value={newPatient.mobilePhoneNumber}
                  onChange={handleInputChange}
                  className="input-field"
                />
                <p id="patient-phone-help" className="mt-1 text-sm text-slate-300">10 цифр, например 9991234567.</p>
              </div>
            </div>
          )}

          <div className="mt-5 flex justify-end gap-2">
            <button
              type="button"
              disabled={creatingPatient || checkingPatient}
              onClick={() => {
                setIsModalOpen(false);
                setFormError('');
              }}
              className="btn-secondary text-xs px-3 py-2"
            >
              Отмена
            </button>
            <button
              type="submit"
              disabled={!newPatient.patientId || creatingPatient}
              className="btn-primary text-xs px-4 py-2"
            >
              {creatingPatient ? 'Создаём...' : 'Создать пациента'}
            </button>
          </div>
          </fieldset>
        </ValidatedForm>
      </Modal>

      {/* Окно управления заявкой */}
      <Modal
        isOpen={!!requestToManage}
        busy={requestActionLoading}
        onClose={() => !requestActionLoading && setRequestToManage(null)}
        title="Управление заявкой"
      >
        {requestToManage && (
          <div className="space-y-4 text-sm text-slate-200">
            <StatusMessage>{actionError}</StatusMessage>
            <p>
              Выберите действие для заявки по пациенту{' '}
              <span className="font-semibold">
                {requestToManage.patientFullName || '—'}
              </span>{' '}
              в ЛПУ{' '}
              <span className="font-semibold">
                {requestToManage.lpuName || '—'}
              </span>
              .
            </p>
            <div className="space-y-2 text-xs text-slate-400">
              <p>
                <span className="font-medium text-slate-200">
                  Специальность:
                </span>{' '}
                {requestToManage.speciality || '—'}
              </p>
              <p>
                <span className="font-medium text-slate-200">Шаблон:</span>{' '}
                {requestToManage.timePreferencesPresetName || '—'}
              </p>
            </div>

            <div className="mt-4 flex flex-col gap-2 sm:flex-row sm:justify-end">
              <button
                type="button"
                className="btn-secondary text-xs px-3 py-2"
                onClick={() => {
                  setRequestToManage(null);
                  navigate(
                    `/requests/new?updateRequestId=${requestToManage.id}&currentPreset=${encodeURIComponent(
                      requestToManage.timePreferencesPresetName || '',
                    )}`,
                    { state: { restoredRequestForm: {
                      selectedPresetName: requestToManage.timePreferencesPresetName || '',
                      maxDaysToSearch: requestToManage.maxDaysToSearch ?? 30,
                      viewOnly: requestToManage.viewOnly ?? false,
                      searchIntervalMinutes: requestToManage.searchInterval?.startsWith('1.') ? 1440 :
                        Number(requestToManage.searchInterval?.split(':')[0] || 1) * 60 + Number(requestToManage.searchInterval?.split(':')[1] || 0),
                      specificPoints: (requestToManage.specificStartPoints || []).map(point => ({ time:
                        new Date(point).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }) })),
                    } } },
                  );
                }}
                disabled={requestActionLoading}
              >
                Изменить временные предпочтения
              </button>
              <button
                type="button"
                className="btn-primary text-xs px-4 py-2"
                onClick={handleConfirmCancelRequest}
                disabled={requestActionLoading}
              >
                {requestActionLoading ? 'Отменяем...' : 'Отменить заявку'}
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Окно отмены записи */}
      <Modal
        isOpen={!!appointmentToCancel}
        busy={appointmentActionLoading}
        onClose={() => !appointmentActionLoading && setAppointmentToCancel(null)}
        title="Отменить запись"
      >
        {appointmentToCancel && (
          <div className="space-y-4 text-sm text-slate-200">
            <StatusMessage>{actionError}</StatusMessage>
            {(() => {
              const fullName =
                appointmentToCancel.patientFullName || appointmentToCancel.patientName || '';
              const shortPatient = formatShortFio(fullName);
              const doctor =
                appointmentToCancel.doctor || appointmentToCancel.doctorName || 'Врач не указан';
              const start = appointmentToCancel.visitStart
                ? new Date(appointmentToCancel.visitStart)
                : null;
              const dateTime = start ? start.toLocaleString('ru-RU') : '—';

              return (
                <>
                  <p>
                    Вы действительно хотите отменить запись к{' '}
                    <span className="font-semibold">{doctor}</span> для пациента{' '}
                    <span className="font-semibold">{shortPatient}</span>?
                  </p>
                  <p className="text-xs text-slate-400">
                    Дата и время:{' '}
                    <span className="text-slate-200">{dateTime}</span>
                  </p>
                </>
              );
            })()}

            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                className="btn-secondary text-xs px-3 py-2"
                onClick={() => setAppointmentToCancel(null)}
                disabled={appointmentActionLoading}
              >
                Отмена
              </button>
              <button
                type="button"
                className="btn-primary text-xs px-4 py-2"
                onClick={handleConfirmCancelAppointment}
                disabled={appointmentActionLoading}
              >
                {appointmentActionLoading ? 'Отменяем...' : 'Отменить запись'}
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Окно удаления пациента */}
      <Modal
        isOpen={!!patientToDelete}
        busy={patientDeleteLoading}
        onClose={() => !patientDeleteLoading && setPatientToDelete(null)}
        title="Удалить пациента?"
      >
        {patientToDelete && (
          <div className="space-y-4 text-sm text-slate-200">
            <StatusMessage>{actionError}</StatusMessage>
            <p>
              Удалить профиль пациента{' '}
              <span className="font-semibold">
                {[patientToDelete.patientLastName, patientToDelete.patientFirstName, patientToDelete.patientMiddleName]
                  .filter(Boolean)
                  .join(' ') || patientToDelete.patientFullName || '—'}
              </span>
              {patientToDelete.lpuShortName && (
                <> из {patientToDelete.lpuShortName}</>
              )}
              ? Удаление профиля нельзя отменить.
            </p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                className="btn-secondary text-xs px-3 py-2"
                onClick={() => setPatientToDelete(null)}
                disabled={patientDeleteLoading}
              >
                Отмена
              </button>
              <button
                type="button"
                className="rounded-xl bg-rose-500 px-4 py-2 text-xs font-semibold text-white transition hover:bg-rose-400 disabled:opacity-60"
                onClick={handleConfirmDeletePatient}
                disabled={patientDeleteLoading}
              >
                {patientDeleteLoading ? 'Удаляем...' : 'Удалить'}
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

export default Dashboard;
