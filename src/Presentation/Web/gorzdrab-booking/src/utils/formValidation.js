export function validateForm(form) {
  let valid = true;
  for (const field of form.elements) {
    if (!field.willValidate) continue;
    const invalid = !field.validity.valid;
    if (invalid) field.setAttribute('aria-invalid', 'true');
    else field.removeAttribute('aria-invalid');
    valid = valid && !invalid;
  }
  if (!valid) form.querySelector('[aria-invalid="true"]')?.focus();
  return valid;
}

export function markServerErrors(form, errors = {}) {
  for (const [key] of Object.entries(errors)) {
    const normalized = key.split('.').pop().toLowerCase();
    for (const field of form.elements) {
      if (field.name?.toLowerCase() === normalized) field.setAttribute('aria-invalid', 'true');
    }
  }
}

