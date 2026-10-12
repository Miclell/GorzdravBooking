import { validateForm } from '../utils/formValidation';

export default function ValidatedForm({ onSubmit, onInput, onChange, validate = validateForm, ...props }) {
  const clear = event => event.target.removeAttribute('aria-invalid');
  return <form {...props} noValidate onSubmit={event => {
    event.preventDefault();
    if (validate(event.currentTarget)) onSubmit?.(event);
  }} onInput={event => { clear(event); onInput?.(event); }}
    onChange={event => { clear(event); onChange?.(event); }} />;
}
