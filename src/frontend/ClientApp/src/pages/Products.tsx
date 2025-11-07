import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'
import { useBasket } from '../hooks/useBasket'
import { useState } from 'react'
import BasketSidebar from '../components/BasketSidebar'

export default function Products() {
  const [q, setQ] = useState('')
  const { add } = useBasket()
  const products = useQuery({
    queryKey: ['products', q],
    queryFn: () => api.products.list(q || undefined)
  })

  // Small inline placeholder image used if a product image URL 404s
  const placeholder =
    'data:image/svg+xml;utf8,' +
    encodeURIComponent(
      `<svg xmlns="http://www.w3.org/2000/svg" width="256" height="160">
        <defs>
          <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
            <stop offset="0%" stop-color="#dbeafe"/>
            <stop offset="100%" stop-color="#bfdbfe"/>
          </linearGradient>
        </defs>
        <rect width="100%" height="100%" fill="url(#g)"/>
        <text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" fill="#1e40af" font-size="20" font-family="system-ui,Segoe UI,Roboto">No image</text>
      </svg>`
    )

  return (
    <div className="grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-6">
      <div className="space-y-4">
        <div className="flex items-center gap-3">
          <input value={q} onChange={e => setQ(e.target.value)} placeholder="Search" className="border rounded px-3 py-2 w-full max-w-md" />
        </div>

        {products.isLoading && <div>Loading…</div>}
        {products.isError && <div className="text-red-600">Failed to load products</div>}

        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          {products.data?.map(p => (
            <div
              key={p.id}
              className="bg-white border rounded p-4 flex flex-col"
              data-testid="product-card"
              data-sku={p.sku}
              data-product-id={p.id}
            >
              <img
                src={p.imageUrl || placeholder}
                onError={e => {
                  const img = e.currentTarget
                  if (img.src !== placeholder) img.src = placeholder
                }}
                alt={p.name}
                className="w-full h-40 object-cover rounded mb-3 border"
                loading="lazy"
              />
              <div className="font-medium">{p.name}</div>
              <div className="text-sm text-gray-500">{p.sku}</div>
              <div className="text-sm my-2 flex-1">{p.description}</div>
              <div className="flex items-center justify-between mt-2">
                <div className="font-semibold">£{p.price.toFixed(2)}</div>
                <button
                  onClick={() => add({ productId: p.id, quantity: 1 })}
                  className="bg-blue-600 text-white text-sm px-3 py-2 rounded"
                  data-testid="add-to-basket"
                >
                  Add
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Right-hand live basket panel (sticky on large screens) */}
      <div className="lg:sticky lg:top-20">
        <BasketSidebar />
      </div>
    </div>
  )
}
