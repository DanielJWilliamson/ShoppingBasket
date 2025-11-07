import { render, screen } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import App from './App'

// Mock live updates hook to prevent real SignalR connections during unit tests
vi.mock('./signalr/liveClient', () => ({
  useLiveUpdates: () => ({ connected: false })
}))

// Mock basket hook to avoid needing a QueryClient in this unit test
vi.mock('./hooks/useBasket', () => ({
  useBasket: () => ({ basketQuery: { data: { items: [] } } })
}))

describe('App shell', () => {
  it('renders header links without live status', () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>
    )

  // Header title present
  expect(screen.getByText(/Shopping Basket/i)).toBeTruthy()
  // Navigation links present
  expect(screen.getByRole('link', { name: 'Products' })).toBeTruthy()
  expect(screen.getByRole('link', { name: 'Orders' })).toBeTruthy()
  // Disambiguate from the site title link "Shopping Basket" by using exact name
  expect(screen.getByRole('link', { name: 'Basket' })).toBeTruthy()
  // Live/Offline badge was removed in recent UX cleanup
  expect(screen.queryByText(/Offline/i)).toBeNull()
  expect(screen.queryByText(/Online/i)).toBeNull()
  })
})
