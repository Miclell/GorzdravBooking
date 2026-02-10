import { useNavigate } from 'react-router-dom';

function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-[calc(100vh-5rem)] items-center justify-center">
      <div className="glass-panel w-full max-w-2xl p-8 md:p-10">
        <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
          Ошибка 404
        </p>
        <h2 className="mb-3 text-2xl font-semibold text-slate-50 sm:text-3xl">
          Страница не найдена
        </h2>
        <p className="text-sm text-slate-300">
          Похоже, вы перешли по неверной ссылке или страница была перемещена.
          Вернитесь в рабочий кабинет и продолжайте работу.
        </p>

        <div className="mt-6 flex flex-col gap-2 sm:flex-row sm:justify-end">
          <button
            type="button"
            className="btn-secondary px-4 py-2 text-sm"
            onClick={() => navigate(-1)}
          >
            Назад
          </button>
          <button
            type="button"
            className="btn-primary px-4 py-2 text-sm"
            onClick={() => navigate('/dashboard')}
          >
            В рабочий кабинет
          </button>
        </div>
      </div>
    </div>
  );
}

export default NotFound;
