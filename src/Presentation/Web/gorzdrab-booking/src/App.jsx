import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Dashboard from './pages/Dashboard';
import Register from './pages/Register';
import Login from './pages/Login';
import CreateRequest from './pages/CreateRequest';
import TimePreferences from './pages/TimePreferences';
import NotFound from './pages/NotFound';

function ProtectedRoute({ children }) {
  const { userId, loading } = useAuth();

  if (loading)
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-900 text-slate-100">
        <div className="glass-panel px-6 py-4 text-sm text-slate-200">
          Загружаем ваш рабочий кабинет...
        </div>
      </div>
    );

  return userId ? children : <Navigate to="/login" />;
}

function ShellLayout({ children }) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-emerald-900 text-slate-50">
      <div className="pointer-events-none fixed inset-0 opacity-60 mix-blend-soft-light">
        <div className="absolute -left-32 top-10 h-64 w-64 rounded-full bg-emerald-500/10 blur-3xl" />
        <div className="absolute bottom-0 right-0 h-72 w-72 rounded-full bg-emerald-400/10 blur-3xl" />
      </div>

      <header className="relative z-10 border-b border-emerald-500/20 bg-slate-950/70 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-2xl bg-gradient-to-br from-emerald-400 to-emerald-600 shadow-md shadow-emerald-500/60">
              <span className="text-lg font-black text-slate-950">G</span>
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-sm font-semibold tracking-wide text-emerald-100 sm:text-base">
                  Gorzdrav Booking
                </h1>
              </div>
              <p className="text-xs text-slate-400">
                Панель управления автоматической записью к врачам
              </p>
            </div>
          </div>
        </div>
      </header>

      <main className="relative z-10 mx-auto flex max-w-6xl flex-1 px-4 py-6 sm:py-10">
        <div className="w-full">{children}</div>
      </main>
    </div>
  );
}

function HomeOrRedirect() {
  const { userId, loading } = useAuth();
  if (loading)
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="glass-panel px-6 py-4 text-sm text-slate-200">
          Загружаем...
        </div>
      </div>
    );
  return userId ? <Navigate to="/dashboard" replace /> : <Login />;
}

function AppContent() {
  return (
    <ShellLayout>
      <Router>
        <Routes>
          <Route path="/" element={<HomeOrRedirect />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/requests/new"
            element={
              <ProtectedRoute>
                <CreateRequest />
              </ProtectedRoute>
            }
          />
          <Route
            path="/time-preferences"
            element={
              <ProtectedRoute>
                <TimePreferences />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </Router>
    </ShellLayout>
  );
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
