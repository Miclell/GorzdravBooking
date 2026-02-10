const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

const apiCall = async (endpoint, options = {}) => {
  const url = `${API_BASE_URL}${endpoint}`;
  const response = await fetch(url, {
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    throw new Error(`API Error: ${response.status} ${response.statusText}`);
  }

  if (response.status === 204) return null;

  const contentType = response.headers.get('content-type');
  if (contentType?.includes('application/json')) {
    return await response.json();
  }
  return await response.text();
};

export const getCurrentUserId = () => localStorage.getItem('userId');

// === AUTH ===
export const loginUser = async (username, password) =>
  apiCall('/api/v1/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });

export const registerUser = async (username, password) =>
  apiCall('/api/v1/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });

export const getMe = async () => apiCall('/api/v1/auth/me');

export const logoutApi = async () =>
  apiCall('/api/v1/auth/logout', { method: 'POST' });

// === PATIENTS ===
export const getPatients = async () => apiCall('/api/v1/patient');

export const findPatientId = async searchData => {
  return apiCall('/api/v1/patient/find', {
    method: 'POST',
    body: JSON.stringify(searchData),
  });
};

export const createPatient = async patientData => {
  const userId = getCurrentUserId();
  if (!userId) throw new Error('Не авторизован');
  return apiCall('/api/v1/patient/create', {
    method: 'POST',
    body: JSON.stringify({
      userId,
      ...patientData,
    }),
  });
};

export const deletePatient = async patientId => {
  return apiCall('/api/v1/patient', {
    method: 'DELETE',
    body: JSON.stringify(patientId),
  });
};

// === LOCATIONS ===
export const getDistricts = async () => {
  return apiCall('/api/v1/locations/districts');
};

export const getLpusByDistrict = async districtId => {
  return apiCall(`/api/v1/locations/district/${districtId}/lpus`);
};

// === SPECIALITIES & DOCTORS ===
export const getSpecialities = async lpuId => {
  return apiCall(`/api/v1/speciality/${lpuId}`);
};

export const getDoctors = async (lpuId, specialityId) => {
  return apiCall(`/api/v1/doctor/${lpuId}/${specialityId}`);
};

// === REQUESTS ===
export const getRequests = async () =>
  apiCall('/api/v1/appointment-search-request');

export const createRequest = async requestData => {
  return apiCall('/api/v1/appointment-search-request/create', {
    method: 'POST',
    body: JSON.stringify(requestData),
  });
};

export const deleteRequest = async requestId => {
  return apiCall('/api/v1/appointment-search-request', {
    method: 'DELETE',
    body: JSON.stringify(requestId),
  });
};

export const updateRequestPreferences = async updateDto => {
  return apiCall('/api/v1/appointment-search-request/update', {
    method: 'PATCH',
    body: JSON.stringify(updateDto),
  });
};

// === APPOINTMENTS ===
export const getAppointments = async () => apiCall('/api/v1/appointment');

export const deleteAppointment = async appointmentId => {
  return apiCall('/api/v1/appointment', {
    method: 'DELETE',
    body: JSON.stringify(appointmentId),
  });
};

// === TIME PREFERENCES ===
export const createTimePreferences = async preferences => {
  return apiCall('/api/v1/time-preferences/create', {
    method: 'POST',
    body: JSON.stringify(preferences),
  });
};

export const getTimePreferences = async presetName =>
  apiCall(`/api/v1/time-preferences/${encodeURIComponent(presetName)}`);

export const deleteTimePreferences = async presetName => {
  const userId = getCurrentUserId();
  if (!userId) throw new Error('Не авторизован');
  return apiCall('/api/v1/time-preferences', {
    method: 'DELETE',
    body: JSON.stringify({ userId, name: presetName }),
  });
};
