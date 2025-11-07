import { test, expect } from '@playwright/test'

test.skip('shipping selection updates total cost', async ({ page }) => {
  await page.goto('/')

  // Add two items: one normal, one promo if visible; fall back to adding two normals
  const addButtons = page.getByText('Add')
  await expect(addButtons.first()).toBeVisible()
  await addButtons.nth(0).click()
  // try add another distinct item if present
  const second = addButtons.nth(1)
  if (await second.isVisible()) {
    await second.click()
  }

  // Go to basket/checkout
  await page.getByRole('link', { name: /^(Basket)(\b|\s|$)/ }).click()

  // Shipping toggle affects total
  const totalLabel = page.getByText('Total:').first()
  const shippingTotal = totalLabel.locator('..').locator('span.font-semibold').first()
  const withUk = await shippingTotal.textContent()
  await page.getByLabel('Outside UK (£20)').check()
  await expect(shippingTotal).not.toHaveText(withUk || '')
})
