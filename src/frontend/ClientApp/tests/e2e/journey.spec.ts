import { test, expect } from '@playwright/test'

// Basic Amazon-like journey: view products, add to basket, checkout, view orders

test('happy path: product -> basket -> checkout -> orders', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByText('Shopping Basket')).toBeVisible()

  // Wait for some product tiles
  await expect(page.getByText('Add').first()).toBeVisible()
  // Add first item
  await page.getByText('Add').first().click()

  // Go to checkout via the Basket link (Basket now routes to Checkout)
  await page.getByRole('link', { name: /^(Basket)(\b|\s|$)/ }).click()
  // Checkout now renders two VAT toggles (per-item and master). Assert visibility without strict-mode conflicts.
  await expect(page.getByRole('button', { name: /(With VAT|Without VAT)/ }).first()).toBeVisible()
  // Prefer a stable test id for the primary checkout action
  await page.getByTestId('confirm-order').click()

  // Orders page should render with at least one order
  await expect(page.getByText('Orders')).toBeVisible()
})

// Responsive check
for (const size of [
  { name: 'mobile', width: 390, height: 844 },
  { name: 'tablet', width: 820, height: 1180 },
]) {
  test(`responsive layout - ${size.name}`, async ({ page }) => {
    await page.setViewportSize(size)
    await page.goto('/')
    await expect(page.getByText('Shopping Basket')).toBeVisible()
  })
}
