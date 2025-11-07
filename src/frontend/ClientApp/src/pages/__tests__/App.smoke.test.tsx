import { render, screen } from '@testing-library/react'
import { vi, test, expect } from 'vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import App from '../../App'

// Prevent real SignalR connection during unit tests
vi.mock('../../signalr/liveClient', () => ({
  useLiveUpdates: () => ({ connected: false })
}))

test('renders header and nav', () => {
  const qc = new QueryClient()
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <App />
      </MemoryRouter>
    </QueryClientProvider>
  )
  expect(screen.getByText('Shopping Basket')).toBeTruthy()
  expect(screen.getByText('Products')).toBeTruthy()
  expect(screen.getByText('Basket')).toBeTruthy()
  expect(screen.getByText('Orders')).toBeTruthy()
})
