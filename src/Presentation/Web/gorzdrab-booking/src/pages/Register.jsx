import ValidatedForm from '../components/ValidatedForm';
import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import StatusMessage from '../components/StatusMessage';

function Register() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const submitLock = useRef(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { register } = useAuth();

  const handleSubmit = async e => {
    e.preventDefault();
    if (submitLock.current) return;
    submitLock.current = true;
    setLoading(true);
    setError('');

    if (!/\p{L}/u.test(password) || !/\d/.test(password)) {
      setError('Пароль должен содержать хотя бы одну букву и цифру.');
      submitLock.current = false;
      setLoading(false); return;
    }

    if (password !== confirmPassword) {
      setError('Пароль и подтверждение не совпадают.');
      submitLock.current = false;
      setLoading(false);
      return;
    }

    const result = await register(username, password);

    if (result.success) {
      navigate('/dashboard');
    } else {
      setError(result.error || 'Ошибка регистрации');
    }

    submitLock.current = false;
    setLoading(false);
  };

  return (
    <div className="flex flex-1 items-center justify-center">
      <div className="grid w-full max-w-4xl gap-10 md:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
        <section className="glass-panel p-8 md:p-10">
          <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
            Создаём рабочий профиль
          </p>
          <h2 className="mb-3 text-2xl font-semibold text-slate-50 sm:text-3xl">
            Регистрация учётной записи
          </h2>
          <p className="max-w-md text-sm text-slate-300">
            Учётная запись используется для привязки пациентов, заявок на
            поиск талонов и индивидуальных временных предпочтений.
          </p>

          <ValidatedForm
            className="mt-8 space-y-5"
            onSubmit={handleSubmit}
          >
            <StatusMessage>{error}</StatusMessage>

            <fieldset disabled={loading} className="space-y-5">
            <div className="space-y-2">
              <label
                htmlFor="username"
                className="block text-xs font-medium uppercase tracking-[0.16em] text-slate-300"
              >
                Логин
              </label>
              <input
                id="username"
                minLength={3}
                maxLength={50}
                pattern="[a-zA-Z0-9_]+"
                aria-describedby="username-help"
                autoComplete="username"
                name="username"
                type="text"
                required
                value={username}
                onChange={e => setUsername(e.target.value)}
                className="input-field"
                placeholder="Имя пользователя"
              />
              <p id="username-help" className="text-sm text-slate-300">3–50 латинских букв, цифр или символов подчёркивания.</p>
            </div>

            <div className="space-y-2">
              <label
                htmlFor="password"
                className="block text-xs font-medium uppercase tracking-[0.16em] text-slate-300"
              >
                Пароль
              </label>
              <input
                id="password"
                minLength={6}
                maxLength={100}
                aria-describedby="password-help"
                autoComplete="new-password"
                name="password"
                type="password"
                required
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="input-field"
                placeholder="Надёжный пароль"
              />
              <p id="password-help" className="text-sm text-slate-300">6–100 символов, минимум одна буква и цифра.</p>
            </div>

            <div className="space-y-2">
              <label
                htmlFor="confirmPassword"
                className="block text-xs font-medium uppercase tracking-[0.16em] text-slate-300"
              >
                Подтверждение пароля
              </label>
              <input
                id="confirmPassword"
                autoComplete="new-password"
                name="confirmPassword"
                type="password"
                required
                value={confirmPassword}
                onChange={e => setConfirmPassword(e.target.value)}
                className="input-field"
                placeholder="Повторите пароль"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="btn-primary w-full justify-center py-2.5 text-sm"
            >
              {loading ? 'Создаём аккаунт...' : 'Зарегистрироваться'}
            </button>

            <p className="pt-2 text-center text-xs text-slate-400">
              Уже есть аккаунт?{' '}
              <button
                type="button"
                onClick={() => navigate('/login')}
                className="font-medium text-emerald-300 underline-offset-4 hover:text-emerald-200 hover:underline"
              >
                Войти
              </button>
            </p>
            </fieldset>
          </ValidatedForm>
        </section>

        <aside className="glass-panel hidden flex-col p-8 md:flex md:p-9">
          <div>
            <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
              Зачем нужна регистрация
            </p>
            <h3 className="mb-3 text-lg font-semibold text-slate-50">
              Персональные настройки расписания
            </h3>
            <p className="text-sm text-slate-300">
              У каждого пользователя свои пациенты, шаблоны временных
              предпочтений и история заявок. Это позволяет конфигурировать
              автопоиск записей под конкретного врача или подразделение.
            </p>
          </div>
        </aside>
      </div>
    </div>
  );
}

export default Register;
