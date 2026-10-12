import { test, expect } from '@playwright/test';
import { mockApi } from './fixtures';

test.beforeEach(async ({ page }) => { await mockApi(page); });

test('стилизованный список сохраняет клавиатурный выбор и Escape внутри диалога', async ({ page }, testInfo) => {
  await page.goto('/time-preferences');
  await page.getByRole('button', { name: '+ Создать шаблон' }).click();
  await page.getByRole('button', { name: 'По дням недели', exact: true }).click();
  const day = page.getByLabel('День', { exact: true }).first();
  await day.focus();
  await page.keyboard.press('Space');
  await expect(day).toHaveJSProperty('tagName', 'SELECT');
  await expect(day.getByRole('option', { name: 'Вс', exact: true })).toBeVisible();
  await testInfo.attach('styled-picker', { body: await page.screenshot(), contentType: 'image/png' });
  await page.keyboard.press('End');
  await page.keyboard.press('Enter');
  await expect(day).toHaveValue('0');
  await day.focus();
  await page.keyboard.press('Space');
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(day).toBeFocused();
  await expect(day).toHaveValue('0');
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toHaveCount(0);
});

test('раскрытый список с длинным пунктом помещается на мобильном экране', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 320, height: 844 });
  await page.goto('/requests/new');
  const patient = page.getByLabel('Пациент *', { exact: true });
  await expect(patient).toBeEnabled();
  await patient.click();
  const option = page.getByRole('option', { name: /Петров/ });
  await expect(option).toBeVisible();
  const box = await option.boundingBox();
  expect(box.x).toBeGreaterThanOrEqual(0);
  expect(box.x + box.width).toBeLessThanOrEqual(320);
  await testInfo.attach('mobile-picker', { body: await page.screenshot(), contentType: 'image/png' });
  await option.click();
  await expect(patient).toHaveValue('33333333-3333-3333-3333-333333333333');
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});
