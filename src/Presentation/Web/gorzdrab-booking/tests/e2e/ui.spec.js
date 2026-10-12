import { test, expect } from '@playwright/test';
import { mockApi, respond, patients, doctors, specialities, presets, requests } from './fixtures';

test.beforeEach(async ({ page }) => { await mockApi(page); });

test('ошибка загрузки отличается от пустого списка и повторная загрузка работает', async ({ page }) => {
  let failed = true;
  await page.route('**/api/v1/patient', route => respond(route, failed ? {} : patients, failed ? 503 : 200));
  await page.goto('/dashboard');
  await expect(page.getByRole('alert')).toContainText('Не удалось загрузить пациентов');
  await expect(page.getByText('Пока ни одного пациента.', { exact: false })).toHaveCount(0);
  await page.getByRole('button', { name: 'Заявки 2', exact: true }).click();
  await expect(page.getByRole('button', { name: /Иванов.*Ожидает/ })).toBeVisible();
  await page.getByRole('button', { name: 'Пациенты 0', exact: true }).click();
  failed = false;
  await page.getByRole('button', { name: 'Повторить загрузку' }).click();
  await expect(page.getByRole('heading', { name: 'Иванов Александр Александрович' })).toBeVisible();
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('счётчик включает только активные заявки, карточка работает с клавиатуры', async ({ page }) => {
  await page.goto('/dashboard');
  await expect(page.getByText('Активные заявки', { exact: true }).locator('..').locator('p').nth(1)).toHaveText('1');
  await page.getByRole('button', { name: 'Заявки 2', exact: true }).click();
  const card = page.getByRole('button', { name: /Иванов.*Ожидает/ });
  await card.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('dialog')).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toHaveCount(0);
  await expect(card).toBeFocused();
  await page.keyboard.press('Space');
  await expect(page.getByRole('dialog')).toBeVisible();
});

test('диалог удерживает фокус, закрывается Escape и возвращает фокус', async ({ page }) => {
  await page.goto('/dashboard');
  const trigger = page.getByRole('button', { name: '+ Добавить пациента' });
  await trigger.click();
  const dialog = page.getByRole('dialog', { name: 'Добавить пациента' });
  await expect(dialog.getByLabel('Район *', { exact: true })).toBeFocused();
  for (let index = 0; index < 12; index++) {
    await page.keyboard.press(index % 2 ? 'Shift+Tab' : 'Tab');
    expect(await page.evaluate(() => !!document.activeElement.closest('dialog'))).toBe(true);
  }
  expect(await page.evaluate(() => document.body.style.overflow)).toBe('hidden');
  await page.keyboard.press('Escape');
  await expect(dialog).toHaveCount(0);
  await expect(trigger).toBeFocused();
  expect(await page.evaluate(() => document.body.style.overflow)).toBe('');
});

async function fillPatient(page) {
  await page.getByRole('button', { name: '+ Добавить пациента' }).click();
  await page.getByLabel('Район *', { exact: true }).selectOption('1');
  await page.getByLabel('Поликлиника *', { exact: true }).selectOption('100');
  await page.getByLabel('Фамилия *', { exact: true }).fill('Иванов');
  await page.getByLabel('Имя *', { exact: true }).fill('Александр');
  await page.getByLabel('Отчество *', { exact: true }).fill('Александрович');
  await page.getByLabel('Дата рождения *', { exact: true }).fill('1985-02-03');
}

test('изменение данных сбрасывает подтверждение пациента', async ({ page }) => {
  await page.goto('/dashboard');
  await fillPatient(page);
  await page.getByRole('button', { name: 'Проверить данные в Городздрав', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Создать пациента', exact: true })).toBeEnabled();
  await page.getByLabel('Фамилия *', { exact: true }).fill('Петров');
  await expect(page.getByRole('button', { name: 'Создать пациента', exact: true })).toBeDisabled();
  await expect(page.getByText('Данные пациента подтверждены', { exact: false })).toHaveCount(0);
});

test('поздний ответ проверки пациента не подтверждает изменённые данные', async ({ page }) => {
  let release;
  const gate = new Promise(resolve => { release = resolve; });
  await page.route('**/api/v1/patient/find', async route => { await gate; await respond(route, 'old-patient'); });
  await page.goto('/dashboard');
  await fillPatient(page);
  await page.getByRole('button', { name: 'Проверить данные в Городздрав', exact: true }).click();
  await page.getByLabel('Фамилия *', { exact: true }).fill('Петров');
  const response = page.waitForResponse('**/api/v1/patient/find');
  release();
  await response;
  await expect(page.getByRole('button', { name: 'Создать пациента', exact: true })).toBeDisabled();
});

test('ошибка отмены остаётся в диалоге, повторная отправка и Escape блокируются', async ({ page }) => {
  let calls = 0, release;
  const gate = new Promise(resolve => { release = resolve; });
  await page.route('**/api/v1/appointment-search-request', async route => {
    if (route.request().method() === 'GET') return respond(route, requests);
    calls++;
    await gate;
    return respond(route, {}, 503);
  });
  await page.goto('/dashboard');
  await page.getByRole('button', { name: 'Заявки 2', exact: true }).click();
  await page.getByRole('button', { name: /Иванов.*Ожидает/ }).click();
  const button = page.getByRole('button', { name: 'Отменить заявку', exact: true });
  await button.evaluate(element => { element.click(); element.click(); });
  await expect(page.getByRole('button', { name: 'Отменяем...' })).toBeDisabled();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toBeVisible();
  release();
  await expect(page.getByRole('dialog').getByRole('alert')).toContainText('Не удалось отменить заявку');
  expect(calls).toBe(1);
});

test('воскресенье сохраняется как 0, повторная отправка шаблона блокируется', async ({ page }) => {
  let payload, calls = 0, release;
  const gate = new Promise(resolve => { release = resolve; });
  await page.route('**/api/v1/time-preferences/create', async route => {
    calls++;
    payload = route.request().postDataJSON();
    await gate;
    await respond(route, {}, 503);
  });
  await page.goto('/time-preferences');
  await page.getByRole('button', { name: '+ Создать шаблон' }).click();
  await page.getByLabel('Название шаблона', { exact: true }).fill('Воскресенье');
  await page.getByRole('button', { name: 'По дням недели', exact: true }).click();
  await page.getByLabel('День', { exact: true }).first().selectOption('0');
  await page.getByRole('button', { name: 'Создать', exact: true }).evaluate(element => { element.click(); element.click(); });
  await expect(page.getByRole('button', { name: 'Сохраняем...' })).toBeDisabled();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toBeVisible();
  release();
  await expect(page.getByRole('alert')).toBeVisible();
  await expect(page.getByLabel('Название шаблона', { exact: true })).toHaveValue('Воскресенье');
  expect(calls).toBe(1);
  expect(payload[0].day).toBe(0);
});

test('обратный интервал времени не отправляется', async ({ page }) => {
  let calls = 0;
  await page.route('**/api/v1/time-preferences/create', route => { calls++; return respond(route, {}); });
  await page.goto('/time-preferences');
  await page.getByRole('button', { name: '+ Создать шаблон' }).click();
  await page.getByLabel('Название шаблона', { exact: true }).fill('Неверный');
  await page.getByRole('button', { name: 'По дням недели', exact: true }).click();
  await page.getByLabel('С', { exact: true }).first().fill('14:00');
  await page.getByLabel('До', { exact: true }).first().fill('09:00');
  await page.getByRole('button', { name: 'Создать', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Начало интервала');
  expect(calls).toBe(0);
});

test('ошибка получения редактируемого шаблона не подставляет значения по умолчанию', async ({ page }) => {
  await page.route('**/api/v1/time-preferences/%D0%A3%D1%82%D1%80%D0%BE', route => respond(route, {}, 503));
  await page.goto('/time-preferences');
  await page.getByText('Утро', { exact: true }).locator('..').getByRole('button', { name: 'Изменить' }).click();
  await expect(page.getByRole('dialog').getByRole('alert')).toContainText('Не удалось загрузить шаблон');
  await expect(page.getByRole('button', { name: 'Сохранить', exact: true })).toHaveCount(0);
  await expect(page.getByRole('dialog').getByRole('button', { name: 'Повторить загрузку' })).toBeVisible();
});

test('редактирование шаблона сохраняет исключённые даты и диапазоны API', async ({ page }) => {
  let payload;
  await page.route('**/api/v1/time-preferences', route => {
    if (route.request().method() === 'GET') return respond(route, presets);
    payload = route.request().postDataJSON();
    return respond(route, {});
  });
  await page.goto('/time-preferences');
  await page.getByText('Утро', { exact: true }).locator('..').getByRole('button', { name: 'Изменить' }).click();
  await expect(page.getByLabel('День', { exact: true })).toHaveValue('0');
  await page.getByLabel('Максимум дней вперёд', { exact: true }).fill('365');
  await page.getByLabel('Минимум часов до приёма', { exact: true }).fill('720');
  await page.getByRole('button', { name: 'Сохранить', exact: true }).click();
  await expect(page.getByRole('dialog')).toHaveCount(0);
  expect(payload[0]).toMatchObject({ day: 0, excludedDates: ['2027-01-01'], maxDaysAhead: 365, minHoursFromNow: 720 });
});

test('ручная заявка восстанавливает специальность, врачей и настройки после шаблонов', async ({ page }) => {
  let payload;
  await page.route('**/api/v1/appointment-search-request/create', route => { payload = route.request().postDataJSON(); return respond(route, {}); });
  await page.goto('/requests/new');
  await page.getByRole('button', { name: 'Вручную', exact: true }).click();
  await page.getByLabel('Пациент *', { exact: true }).selectOption(patients[0].id);
  await page.getByLabel('Специальность *', { exact: true }).selectOption('speciality-1');
  await page.getByRole('button', { name: 'Конкретный врач', exact: true }).click();
  await page.getByLabel(doctors[0].name, { exact: true }).check();
  await page.getByLabel('Шаблон времени *', { exact: true }).selectOption('Утро');
  await page.getByRole('button', { name: 'Настройки поиска', exact: true }).click();
  await page.getByLabel('Интервал поиска (мин)', { exact: true }).fill('120');
  await page.getByLabel('Максимум дней для поиска', { exact: true }).fill('45');
  await page.getByLabel('Только просмотр', { exact: false }).check();
  await page.getByRole('button', { name: 'Готово', exact: true }).click();
  await page.getByRole('button', { name: 'Добавить время запуска' }).click();
  await page.getByLabel('Время запуска 1', { exact: true }).fill('08:15');
  await page.getByRole('button', { name: 'Управление шаблонами' }).click();
  await page.getByRole('button', { name: '← Назад к заявке' }).click();
  await expect(page.getByLabel('Специальность *', { exact: true })).toHaveValue('speciality-1');
  await expect(page.getByLabel(doctors[0].name, { exact: true })).toBeChecked();
  await expect(page.getByLabel('Время запуска 1', { exact: true })).toHaveValue('08:15');
  await page.getByRole('button', { name: 'Создать заявку', exact: true }).click();
  await expect(page).toHaveURL(/dashboard$/);
  expect(payload).toMatchObject({ patientProfileId: patients[0].id, specialityId: 'speciality-1',
    doctorIds: ['doctor-1'], timePreferencesPresetName: 'Утро', searchInterval: '02:00:00', maxDaysToSearch: 45, viewOnly: true });
  await expect(page.getByRole('status')).toContainText('Заявка создана');
});

test('редактирование заявки сохраняет URL, новый шаблон и существующие настройки', async ({ page }) => {
  let payload;
  await page.route('**/api/v1/appointment-search-request/update', route => { payload = route.request().postDataJSON(); return respond(route, {}); });
  await page.goto('/dashboard');
  await page.getByRole('button', { name: 'Заявки 2', exact: true }).click();
  await page.getByRole('button', { name: /Иванов.*Ожидает/ }).click();
  await page.getByRole('button', { name: 'Изменить временные предпочтения', exact: true }).click();
  const originalUrl = page.url();
  await page.getByLabel('Шаблон времени *', { exact: true }).selectOption('Любое время');
  await page.getByRole('button', { name: 'Управление шаблонами' }).click();
  await page.getByRole('button', { name: '← Назад к заявке' }).click();
  expect(page.url()).toBe(originalUrl);
  await expect(page.getByLabel('Шаблон времени *', { exact: true })).toHaveValue('Любое время');
  await page.getByRole('button', { name: 'Сохранить изменения', exact: true }).click();
  await expect(page).toHaveURL(/dashboard$/);
  expect(payload).toMatchObject({ requestId: requests[0].id, timePreferencesName: 'Любое время',
    searchInterval: '00:15:00', maxDaysToSearch: 45, viewOnly: true });
});

test('проверенное направление и выбранные врачи восстанавливаются после шаблонов', async ({ page }) => {
  let payload;
  await page.route('**/api/v1/appointment-search-request/create', route => { payload = route.request().postDataJSON(); return respond(route, {}); });
  await page.goto('/requests/new');
  await page.getByRole('button', { name: 'По направлению', exact: true }).click();
  await page.getByLabel('Номер направления', { exact: true }).fill('123456');
  await page.getByLabel('Фамилия пациента', { exact: true }).fill('Иванов');
  await page.getByRole('button', { name: 'Проверить направление', exact: true }).click();
  await page.getByLabel('Специальность из направления *', { exact: true }).selectOption('speciality-1');
  await page.getByRole('button', { name: 'Конкретный врач', exact: true }).click();
  await page.getByLabel(doctors[0].name, { exact: true }).check();
  await page.getByLabel('Шаблон времени *', { exact: true }).selectOption('Утро');
  await page.getByRole('button', { name: 'Управление шаблонами' }).click();
  await page.getByRole('button', { name: '← Назад к заявке' }).click();
  await expect(page.getByLabel(doctors[0].name, { exact: true })).toBeChecked();
  await page.getByRole('button', { name: 'Создать заявку', exact: true }).click();
  await expect(page).toHaveURL(/dashboard$/);
  expect(payload).toMatchObject({ referralNumber: '123456', doctorIds: ['doctor-1'] });
});

test('поздний ответ специальностей не заменяет выбор другой поликлиники', async ({ page }) => {
  let release;
  const gate = new Promise(resolve => { release = resolve; });
  await page.route('**/api/v1/speciality/100', async route => { await gate; await respond(route, specialities); });
  await page.route('**/api/v1/speciality/200', route => respond(route, [{ id: 'speciality-2', name: 'Хирург' }]));
  await page.goto('/requests/new');
  await page.getByRole('button', { name: 'Вручную', exact: true }).click();
  const requested = page.waitForRequest('**/api/v1/speciality/100');
  await page.getByLabel('Пациент *', { exact: true }).selectOption(patients[0].id);
  await requested;
  await page.getByLabel('Пациент *', { exact: true }).selectOption(patients[1].id);
  await page.getByLabel('Специальность *', { exact: true }).selectOption('speciality-2');
  const response = page.waitForResponse('**/api/v1/speciality/100');
  release();
  await response;
  await expect(page.getByLabel('Специальность *', { exact: true })).toHaveValue('speciality-2');
  await expect(page.locator('#request-speciality option')).toHaveText(['Выберите специальность', 'Хирург']);
});

test('невалидные настройки поиска не отправляются и черновик сохраняется', async ({ page }) => {
  await page.goto('/requests/new?updateRequestId=test&currentPreset=' + encodeURIComponent('Утро'));
  await page.getByRole('button', { name: 'Настройки поиска', exact: true }).click();
  await page.getByLabel('Интервал поиска (мин)', { exact: true }).fill('');
  await page.getByRole('button', { name: 'Готово', exact: true }).click();
  await page.getByRole('button', { name: 'Сохранить изменения', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('от 1 до 1440');
  await page.getByRole('button', { name: 'Настройки поиска', exact: true }).click();
  await expect(page.getByLabel('Интервал поиска (мин)', { exact: true })).toHaveValue('');
});

for (const width of [320, 390, 768, 1440]) {
  test('экраны и диалоги без горизонтальной прокрутки, ширина ' + width, async ({ page }, testInfo) => {
    await page.setViewportSize({ width, height: 900 });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    for (const path of ['/login', '/register', '/missing-page', '/dashboard', '/requests/new', '/time-preferences']) {
      await page.goto(path);
      await expect(page.locator('main h2').first()).toBeVisible();
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
      await testInfo.attach(path.slice(1) || 'home', { body: await page.screenshot({ fullPage: true }), contentType: 'image/png' });
      if (path === '/time-preferences') {
        await page.getByRole('button', { name: '+ Создать шаблон' }).click();
        await page.getByRole('button', { name: 'По дням недели', exact: true }).click();
        expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
        const dialog = page.getByRole('dialog');
        const box = await dialog.boundingBox();
        expect(box.x).toBeGreaterThanOrEqual(0);
        expect(box.y).toBeGreaterThanOrEqual(0);
        expect(box.y + box.height).toBeLessThanOrEqual(901);
        expect(await dialog.locator('input, select').evaluateAll(elements => elements.every(e => e.labels?.length > 0))).toBe(true);
        await testInfo.attach('weekday-dialog', { body: await page.screenshot(), contentType: 'image/png' });
      }
    }
    expect(errors).toEqual([]);
  });
}

test('короткие страницы входа и 404 не создают лишнюю вертикальную прокрутку', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 1024 });
  for (const path of ['/login', '/missing-page']) {
    await page.goto(path);
    expect(await page.evaluate(() => document.documentElement.scrollHeight <= innerHeight)).toBe(true);
  }
});

test('регистрация показывает ошибки до отправки и сохраняет поля', async ({ page }) => {
  let calls = 0;
  await page.route('**/api/v1/auth/register', route => { calls++; return respond(route, {}); });
  await page.goto('/register');
  await page.getByLabel('Логин', { exact: true }).fill('test_user');
  await page.getByLabel('Пароль', { exact: true }).fill('abcdef1');
  await page.getByLabel('Подтверждение пароля', { exact: true }).fill('abcdef2');
  await page.getByRole('button', { name: 'Зарегистрироваться', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('не совпадают');
  await expect(page.getByLabel('Логин', { exact: true })).toHaveValue('test_user');
  expect(calls).toBe(0);
});



test('ручная заявка первая по умолчанию, пустой шаблон выделяется без браузерной плашки', async ({ page }) => {
  let submitted = false;
  await page.route('**/api/v1/appointment-search-request/create', route => { submitted = true; return respond(route, {}); });
  await page.goto('/requests/new');
  const modes = page.getByRole('group', { name: 'Тип заявки' }).getByRole('button');
  await expect(modes.first()).toHaveText('Вручную');
  await expect(modes.first()).toHaveAttribute('aria-pressed', 'true');
  await page.getByLabel('Пациент *', { exact: true }).selectOption(patients[0].id);
  await page.getByLabel('Специальность *', { exact: true }).selectOption(specialities[0].id);
  await page.getByRole('button', { name: 'Создать заявку', exact: true }).click();
  await expect(page.getByLabel('Шаблон времени *', { exact: true })).toHaveAttribute('aria-invalid', 'true');
  await expect(page.locator('form')).toHaveAttribute('novalidate', '');
  expect(submitted).toBe(false);
  await page.getByLabel('Шаблон времени *', { exact: true }).selectOption('Утро');
  await expect(page.getByLabel('Шаблон времени *', { exact: true })).not.toHaveAttribute('aria-invalid', 'true');
});

test('цифры в ФИО и возраст более 150 лет не отправляются на проверку', async ({ page }) => {
  let checked = false;
  await page.route('**/api/v1/patient/find', route => { checked = true; return respond(route, 'patient-id'); });
  await page.goto('/dashboard');
  await fillPatient(page);
  await page.getByLabel('Фамилия *', { exact: true }).fill('12345');
  await page.getByRole('button', { name: 'Проверить данные в Городздрав', exact: true }).click();
  await expect(page.getByLabel('Фамилия *', { exact: true })).toHaveAttribute('aria-invalid', 'true');
  expect(checked).toBe(false);
  await page.getByLabel('Фамилия *', { exact: true }).fill('Иванов');
  await page.getByLabel('Дата рождения *', { exact: true }).fill('1800-01-01');
  await page.getByRole('button', { name: 'Проверить данные в Городздрав', exact: true }).click();
  await expect(page.getByLabel('Дата рождения *', { exact: true })).toHaveAttribute('aria-invalid', 'true');
  await expect(page.getByRole('alert')).toContainText('Дата рождения не может быть более 150 лет назад');
  expect(checked).toBe(false);
});

test('RFC ошибка PatientBirthdate отображается и выделяет поле, введённые данные сохраняются', async ({ page }) => {
  await page.route('**/api/v1/patient/create', route => respond(route, {
    type: 'https://example.test/validation', title: 'Validation failed', status: 400,
    errors: { PatientBirthdate: ['Дата рождения не может быть более 150 лет назад'] },
  }, 400));
  await page.goto('/dashboard');
  await fillPatient(page);
  await page.getByRole('button', { name: 'Проверить данные в Городздрав', exact: true }).click();
  await page.getByRole('button', { name: 'Создать пациента', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Дата рождения не может быть более 150 лет назад');
  await expect(page.getByLabel('Дата рождения *', { exact: true })).toHaveAttribute('aria-invalid', 'true');
  await expect(page.getByLabel('Фамилия *', { exact: true })).toHaveValue('Иванов');
});
