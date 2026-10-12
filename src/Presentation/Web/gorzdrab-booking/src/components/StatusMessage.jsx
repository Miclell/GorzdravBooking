function StatusMessage({ children, onRetry, kind = 'error' }) {
  if (!children) return null;
  return (
    <div role={kind === 'error' ? 'alert' : 'status'} className={kind === 'error' ? 'message-error' : 'message-success'}>
      <p>{children}</p>
      {onRetry && <button type="button" onClick={onRetry} className="btn-secondary mt-3">Повторить загрузку</button>}
    </div>
  );
}

export default StatusMessage;

