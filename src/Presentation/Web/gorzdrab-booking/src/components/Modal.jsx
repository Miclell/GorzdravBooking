function Modal({ isOpen, onClose, title, children }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 backdrop-blur-sm">
      <div className="glass-panel w-full max-w-3xl border-emerald-500/30 bg-slate-950/90 shadow-soft">
        <div className="flex items-center justify-between border-b border-emerald-500/20 px-6 py-4">
          <h2 className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-200">
            {title}
          </h2>
          <button
            onClick={onClose}
            className="inline-flex h-8 w-8 items-center justify-center rounded-full border border-slate-700/70 bg-slate-900/60 text-slate-300 shadow-sm transition hover:border-emerald-400/70 hover:bg-slate-900 hover:text-emerald-200"
            aria-label="Закрыть модальное окно"
          >
            ×
          </button>
        </div>

        <div className="max-h-[80vh] overflow-y-auto px-6 py-5">{children}</div>
      </div>
    </div>
  );
}

export default Modal;
