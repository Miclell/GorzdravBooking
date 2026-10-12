import { test, expect } from '@playwright/test';
import { mockApi } from './fixtures';

test('сопоставимые скриншоты входа и редактора времени', async ({ page }, testInfo) => {
  const baseline = Boolean(process.env.UI_AUDIT_BASE_URL);
  const baseURL = process.env.UI_AUDIT_BASE_URL || 'http://127.0.0.1:5094';
  await mockApi(page);
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto(baseURL + '/login');
  await expect(page.getByRole('heading', { name: 'Вход в рабочий кабинет' })).toBeVisible();
  await page.screenshot({ path: testInfo.outputPath('login.png'), fullPage: true });
  await testInfo.attach('login', { path: testInfo.outputPath('login.png'), contentType: 'image/png' });
  await page.goto(baseURL + '/time-preferences');
  await page.getByRole('button', { name: baseline ? '+ Создать пресет' : '+ Создать шаблон', exact: true }).click();
  await page.getByRole('button', { name: 'По дням недели', exact: true }).click();
  if (!baseline) await expect(page.getByRole('button', { name: 'По дням недели', exact: true })).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByLabel('День', { exact: true }).first()).toBeVisible();
  await page.screenshot({ path: testInfo.outputPath('weekday-dialog.png') });
  await testInfo.attach('weekday-dialog', { path: testInfo.outputPath('weekday-dialog.png'), contentType: 'image/png' });
});

