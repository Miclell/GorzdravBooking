import { useState, useRef, useEffect } from 'react';

function Select({
  value,
  onChange,
  options = [],
  placeholder = 'Выберите...',
  required,
  name,
  className = '',
  size = 'md',
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    const handleClickOutside = e => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selectedOption = options.find(o => String(o.value) === String(value));
  const display = selectedOption ? selectedOption.label : placeholder;

  const sizeClasses =
    size === 'sm'
      ? 'py-1.5 px-2.5 text-xs'
      : 'py-2.5 px-3 text-sm';

  const listTextSize = size === 'sm' ? 'text-xs' : 'text-sm';

  return (
    <div ref={ref} className={`relative w-full ${className}`}>
      <button
        type="button"
        onClick={() => setOpen(prev => !prev)}
        className={`w-full rounded-2xl border border-slate-700/70 bg-slate-900/60 pl-3 pr-10 text-left text-slate-100 shadow-inner shadow-black/20 transition focus:border-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-500/60 cursor-pointer ${sizeClasses}`}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-required={required}
      >
        <span className={!selectedOption ? 'text-slate-500' : ''}>
          {display}
        </span>
        <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
          <svg
            className={`h-5 w-5 text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`}
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
          </svg>
        </span>
      </button>

      {name && value && (
        <input type="hidden" name={name} value={value} required={required} />
      )}

      {open && (
        <ul
          role="listbox"
          className={`absolute z-50 mt-1.5 max-h-56 w-full overflow-y-auto rounded-2xl border border-slate-700/70 bg-slate-900/95 py-1.5 shadow-xl shadow-black/40 backdrop-blur ${listTextSize}`}
        >
          {!required && (
            <li
              role="option"
              aria-selected={!value}
              onClick={() => {
                onChange('');
                setOpen(false);
              }}
              className="cursor-pointer px-3 py-2 text-slate-400 hover:bg-slate-800/80 hover:text-slate-200"
            >
              {placeholder}
            </li>
          )}
          {options.map(opt => (
            <li
              key={opt.value}
              role="option"
              aria-selected={String(opt.value) === String(value)}
              onClick={() => {
                onChange(opt.value);
                setOpen(false);
              }}
              className={`cursor-pointer px-3 py-2 text-slate-100 hover:bg-slate-800/80 ${
                String(opt.value) === String(value)
                  ? 'bg-emerald-500/20 text-emerald-100'
                  : ''
              }`}
            >
              {opt.label}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Select;
