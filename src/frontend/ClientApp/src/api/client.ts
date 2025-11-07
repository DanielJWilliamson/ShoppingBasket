import type { ProductDto, BasketDto, OrderDto } from './types'

const base = '' // same origin

async function http<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  return res.json()
}

export const api = {
  products: {
    list: (q?: string) => http<ProductDto[]>(`${base}/api/products${q ? `?q=${encodeURIComponent(q)}` : ''}`),
    get: (id: number) => http<ProductDto>(`${base}/api/products/${id}`),
  },
  baskets: {
    create: (customerId?: string) => http<BasketDto>(`${base}/api/baskets/create${customerId ? `?customerId=${encodeURIComponent(customerId)}` : ''}`, { method: 'POST' }),
    get: (basketId: number) => http<BasketDto>(`${base}/api/baskets/${basketId}`),
    addItem: (basketId: number, productId: number, quantity: number) => http<BasketDto>(`${base}/api/baskets/${basketId}/items`, {
      method: 'POST', body: JSON.stringify({ productId, quantity })
    }),
    updateItem: (basketId: number, productId: number, quantity: number) => http<BasketDto>(`${base}/api/baskets/${basketId}/items`, {
      method: 'PUT', body: JSON.stringify({ productId, quantity })
    }),
    removeItem: (basketId: number, productId: number) => http<BasketDto>(`${base}/api/baskets/${basketId}/items/${productId}`, { method: 'DELETE' }),
    clear: (basketId: number) => fetch(`${base}/api/baskets/${basketId}`, { method: 'DELETE' }),
    applyVoucher: (basketId: number, code: string) => http<BasketDto>(`${base}/api/baskets/${basketId}/vouchers/${encodeURIComponent(code)}`, { method: 'POST' }),
    removeVoucher: (basketId: number, code: string) => http<BasketDto>(`${base}/api/baskets/${basketId}/vouchers/${encodeURIComponent(code)}`, { method: 'DELETE' }),
  },
  orders: {
    list: (customerId?: string) => http<OrderDto[]>(`${base}/api/orders${customerId ? `?customerId=${encodeURIComponent(customerId)}` : ''}`),
    get: (id: number) => http<OrderDto>(`${base}/api/orders/${id}`),
    checkout: (basketId: number, buyerId?: string, includeVat: boolean = true, vatOverrides?: { productId: number, includeVat: boolean }[], shippingCountry?: string) =>
      http<OrderDto>(`${base}/api/orders/checkout/${basketId}`, {
        method: 'POST', body: JSON.stringify({ buyerId, includeVat, vatOverrides, shippingCountry })
      })
  }
}
