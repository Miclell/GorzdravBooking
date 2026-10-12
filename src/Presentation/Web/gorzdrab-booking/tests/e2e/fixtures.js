export const user = { id: '11111111-1111-1111-1111-111111111111', username: 'ui_audit' };
export const patients = [
  { id: '22222222-2222-2222-2222-222222222222', lpuId: '100', lpuShortName: 'Поликлиника № 1',
    patientLastName: 'Иванов', patientFirstName: 'Александр', patientMiddleName: 'Александрович', patientBirthdate: '1985-02-03' },
  { id: '33333333-3333-3333-3333-333333333333', lpuId: '200', lpuShortName: 'Поликлиника с очень длинным названием',
    patientLastName: 'Петров', patientFirstName: 'Пётр', patientMiddleName: 'Петрович', patientBirthdate: '1990-10-11' },
];
export const doctors = [{ id: 'doctor-1', name: 'Смирнова Анна Ивановна' }];
export const specialities = [{ id: 'speciality-1', name: 'Терапевт', doctors }];
export const presets = [
  { name: 'Утро', userId: user.id, timeMode: 1, preferences: [{ day: 0, from: '09:00:00', to: '13:00:00' }],
    excludedDates: ['2027-01-01'], maxDaysAhead: 14, minHoursFromNow: 2 },
  { name: 'Любое время', userId: user.id, timeMode: 0, preferences: [], excludedDates: [], maxDaysAhead: 30, minHoursFromNow: 0 },
];
export const requests = ['Pending', 'Completed'].map((status, index) => ({
  id: '44444444-4444-4444-4444-' + String(index + 1).padStart(12, '0'),
  patientFullName: 'Иванов Александр Александрович', patientProfileId: patients[0].id,
  lpuName: patients[0].lpuShortName, speciality: 'Терапевт', doctorNames: ['Смирнова Анна Ивановна'],
  timePreferencesPresetName: 'Утро', searchInterval: '00:15:00', maxDaysToSearch: 45, viewOnly: true,
  specificStartPoints: ['2026-10-11T08:15:00'], createdAt: '2026-10-10T09:00:00', status,
}));
export const appointments = [{ id: 'appointment-1', patientFullName: 'Иванов Александр Александрович',
  lpuShortName: patients[0].lpuShortName, doctorName: 'Смирнова Анна Ивановна', visitStart: '2026-11-02T10:30:00' }];

export const respond = (route, data, status = 200) => route.fulfill({
  status, contentType: 'application/json', body: JSON.stringify(data),
});

export async function mockApi(page, { authenticated = true, empty = false } = {}) {
  await page.route('**/api/**', async route => {
    const path = decodeURIComponent(new URL(route.request().url()).pathname);
    const method = route.request().method();
    if (path.endsWith('/auth/me')) return respond(route, authenticated ? user : {}, authenticated ? 200 : 401);
    if (path.endsWith('/auth/login') || path.endsWith('/auth/register')) return respond(route, user);
    if (path.endsWith('/locations/districts')) return respond(route, [{ id: 1, name: 'Центральный район' }, { id: 2, name: 'Другой район' }]);
    if (/\/locations\/district\/\d+\/lpus$/.test(path)) return respond(route, [{ id: '100', lpuShortName: 'Поликлиника № 1', address: 'Тестовый адрес' }]);
    if (path.endsWith('/patient/find')) return respond(route, 'external-patient-id');
    if (path.endsWith('/referral/validate')) return respond(route, { patientProfile: patients[0], specialities });
    if (path.includes('/speciality/')) return respond(route, specialities);
    if (path.includes('/doctor/')) return respond(route, doctors);
    if (method === 'GET') {
      if (path.endsWith('/patient')) return respond(route, empty ? [] : patients);
      if (path.endsWith('/appointment-search-request')) return respond(route, empty ? [] : requests);
      if (path.endsWith('/appointment')) return respond(route, empty ? [] : appointments);
      if (path.endsWith('/time-preferences')) return respond(route, empty ? [] : presets);
      if (path.includes('/time-preferences/')) return respond(route, presets.find(p => path.endsWith('/' + p.name)) || {}, 200);
    }
    if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(method)) return respond(route, {});
    return respond(route, { title: 'Unexpected fixture request: ' + method + ' ' + path }, 404);
  });
}

