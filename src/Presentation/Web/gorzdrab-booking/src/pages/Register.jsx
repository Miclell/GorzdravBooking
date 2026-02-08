import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

function Register() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { register } = useAuth();

  const handleSubmit = async e => {
    e.preventDefault();
    setLoading(true);
    setError('');

    if (password !== confirmPassword) {
      setError('Пароль и подтверждение не совпадают.');
      setLoading(false);
      return;
    }

    const result = await register(username, password);

    if (result.success) {
      navigate('/dashboard');
    } else {
      setError(result.error || 'Ошибка регистрации');
    }

    setLoading(false);
  };

  return (
    <div className="flex min-h-[calc(100vh-5rem)] items-center justify-center">
      <div className="grid w-full max-w-4xl gap-10 md:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
        <section className="glass-panel p-8 md:p-10">
          <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
            Создаём рабочий профиль
          </p>
          <h2 className="mb-3 text-2xl font-semibold text-slate-50 sm:text-3xl">
            Регистрация учётной записи
          </h2>
          <p className="max-w-md text-sm text-slate-300">
            Учётная запись используется для привязки пациентов, запросов на
            поиск талонов и индивидуальных временных предпочтений.
          </p>

          <form
            className="mt-8 space-y-5"
            onSubmit={handleSubmit}
          >
            {error && (
              <div className="rounded-xl border border-red-500/40 bg-red-500/10 px-4 py-3 text-xs text-red-100">
                {error}
              </div>
            )}

            <div className="space-y-2">
              <label
                htmlFor="username"
                className="block text-xs font-medium uppercase tracking-[0.16em] text-slate-300"
              >
                Логин
              </label>
              <input
                id="username"
                name="username"
                type="text"
                required
                value={username}
                onChange={e => setUsername(e.target.value)}
                className="input-field"
                placeholder="Имя пользователя"
              />
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
                name="password"
                type="password"
                required
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="input-field"
                placeholder="Надёжный пароль"
              />
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
          </form>
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
              предпочтений и история запросов. Это позволяет конфигурировать
              автопоиск записей под конкретного врача или подразделение.
            </p>
          </div>
        </aside>
      </div>
    </div>
  );
}

export default Register;
