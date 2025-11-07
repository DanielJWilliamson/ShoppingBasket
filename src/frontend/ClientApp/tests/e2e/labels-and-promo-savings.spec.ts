import { test, expect } from '@playwright/test'

// Verifies checkout subtotal label toggles to "Discounted subtotal" when a voucher is applied
test('checkout label shows when voucher applied', async ({ page }) => {
  await page.goto('/')
  // Add a normal item first (use first Add button for stability)
  await page.getByRole('button', { name: 'Add' }).first().click()

  // Go to checkout and check label is 'Subtotal' when no voucher
  await page.getByRole('link', { name: 'Checkout' }).click()
  await expect(page.getByRole('button', { name: /(With VAT|Without VAT)/ }).first()).toBeVisible()
  // Verify initial label shows Subtotal
  await expect(page.getByText('Subtotal:')).toBeVisible()

  // Apply a voucher and expect the applied tag and label change
  await page.getByLabel('Voucher code').fill('10percent')
  await page.getByRole('button', { name: 'Apply' }).click()
  await expect(page.getByText('Applied:')).toBeVisible()
  await expect(page.getByRole('button', { name: 'Remove voucher 10percent' })).toBeVisible()
  await expect(page.getByText('Discounted subtotal:')).toBeVisible()
})
