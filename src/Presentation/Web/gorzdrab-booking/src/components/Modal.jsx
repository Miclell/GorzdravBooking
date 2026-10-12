import { useEffect, useId, useRef } from 'react';
import { createPortal } from 'react-dom';

function Modal({ isOpen, onClose, title, children, busy = false }) {
  const dialogRef = useRef(null);
  const titleId = useId();

  useEffect(() => {
    if (!isOpen) return;
    const dialog = dialogRef.current;
    const trigger = document.activeElement;
    const previousOverflow = document.body.style.overflow;
    dialog.showModal();
    document.body.style.overflow = 'hidden';
    const firstField = dialog.querySelector('input:not([disabled]), select:not([disabled]), textarea:not([disabled])');
    firstField?.focus();
    return () => {
      dialog.close();
      document.body.style.overflow = previousOverflow;
      if (trigger?.isConnected) trigger.focus();
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return createPortal(
    <dialog ref={dialogRef} aria-labelledby={titleId} aria-modal="true" aria-busy={busy}
      className="ui-dialog glass-panel border-emerald-500/30 bg-slate-950/90 shadow-soft text-slate-50"
      onCancel={event => { event.preventDefault(); if (!busy) onClose(); }}
    >
      <div className="flex items-center justify-between border-b border-emerald-500/20 px-6 py-4">
        <h2 id={titleId} className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-200">{title}</h2>
        <button type="button" onClick={onClose} disabled={busy} className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-full border border-slate-700/70 bg-slate-900/60 text-slate-300 shadow-sm transition hover:border-emerald-400/70 hover:bg-slate-900 hover:text-emerald-200"
          aria-label="Закрыть модальное окно">×</button>
      </div>
      <div className="min-h-0 max-h-[80vh] overflow-y-auto px-6 py-5">{children}</div>
    </dialog>, document.body,
  );
}

export default Modal;
