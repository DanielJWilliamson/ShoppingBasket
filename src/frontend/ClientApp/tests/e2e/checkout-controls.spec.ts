import { test, expect } from '@playwright/test'

// Verifies quantity +/− controls and Clear button on Checkout, and absence of old Live badge.

test('checkout quantity adjust and clear item', async ({ page }) => {
  await page.goto('/')
  // Ensure we have at least one product tile
  const firstAdd = page.getByText('Add').first()
  await expect(firstAdd).toBeVisible()
  // Click Add and wait for the basket addItem POST to complete to avoid racing navigation
  const addCompleted = page.waitForResponse(r =>
    r.request().method() === 'POST' &&
    r.url().includes('/api/baskets/') &&
    r.url().endsWith('/items') &&
    r.ok()
  )
  await firstAdd.click()
  await addCompleted
  // Navigate directly to checkout route to avoid nav races
  await page.goto('/checkout')
  // Wait for checkout table to render rows
  const rows = page.locator('tbody tr')
  // Wait until at least one row exists (works whether data came from cache or network)
  await expect(rows).not.toHaveCount(0, { timeout: 10000 })
  const row = rows.first()
  // Quantity element: support both span[aria-live="polite"] and input quantity (fallback)
  const qtySpan = row.locator('span[aria-live="polite"]').first()
  const qtyInput = row.locator('input[aria-label$=" quantity"]').first()
  const qtyLocator = (await qtySpan.count()) > 0 ? qtySpan : qtyInput
  // Sanity: ensure we found a quantity element
  await expect(qtyLocator).toBeVisible()
  const initialQtyText = (await qtySpan.count()) > 0
    ? await qtyLocator.textContent()
    : await qtyLocator.inputValue()
  const initialQty = Number(initialQtyText || '0')
  expect(initialQty).toBeGreaterThan(0)

  // Click plus
  await row.getByRole('button', { name: /Increase .* quantity/ }).click()
  if ((await qtySpan.count()) > 0) {
    await expect(qtyLocator).toHaveText(String(initialQty + 1))
  } else {
    await expect(qtyLocator).toHaveValue(String(initialQty + 1))
  }

  // Click minus (should return to original quantity unless original was 1)
  await row.getByRole('button', { name: /Decrease .* quantity/ }).click()
  if ((await qtySpan.count()) > 0) {
    await expect(qtyLocator).toHaveText(String(initialQty))
  } else {
    await expect(qtyLocator).toHaveValue(String(initialQty))
  }

  // Clear item
  await row.getByRole('button', { name: /Remove all .* from basket/ }).click()
  // Item row should disappear; wait for basket refresh
  if ((await qtySpan.count()) > 0) {
    await expect(qtySpan).toHaveCount(0)
  } else {
    await expect(qtyInput).toHaveCount(0)
  }
})

test('header no longer shows Live/Offline badge', async ({ page }) => {
  await page.goto('/')
  // Ensure main heading present
  await expect(page.getByText('Shopping Basket').first()).toBeVisible()
  // Assert no visible Live or Offline text nodes in header area
  const header = page.locator('header')
  await expect(header.getByText('Live')).toHaveCount(0)
  await expect(header.getByText('Offline')).toHaveCount(0)
})
