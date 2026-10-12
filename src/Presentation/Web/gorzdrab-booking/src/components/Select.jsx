function Select({ value, onChange, options = [], placeholder = 'Выберите...', required,
  name, id, disabled = false, className = '', size = 'md', ...props }) {
  return (
    <select {...props} id={id} name={name} value={value ?? ''} required={required} disabled={disabled}
      onChange={event => {
        const option = options.find(item => String(item.value) === event.target.value);
        onChange(option ? option.value : '');
      }}
      data-placeholder={value === '' || value == null ? 'true' : undefined}
      className={`input-field styled-select rounded-2xl bg-slate-900/60 ${size === 'sm' ? 'py-1.5 px-2.5 text-xs' : 'py-2.5 px-3 text-sm'} ${className}`}
    >
      <option value="" disabled={required}>{placeholder}</option>
      {options.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}
    </select>
  );
}

export default Select;
