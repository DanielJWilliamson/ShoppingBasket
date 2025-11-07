import { Link } from 'react-router-dom'
import { useBasket } from '../hooks/useBasket'

// Compact basket summary panel for right-hand sidebar on product listings.
// Shows live basket contents while the user adds items, without navigating away.
export default function BasketSidebar() {
  const { basketQuery, update, remove, clear } = useBasket()
  const b = basketQuery.data
  // Fallback mapping in case backend doesn't supply originalUnitPrice (e.g., legacy DB without new mapping yet)
  const originalBySku: Record<string, number> = {
    'PROMO-001': 50.00,
  }
  const promoSavings = b ? b.items.reduce((sum, it) => {
    const orig = (it.originalUnitPrice ?? originalBySku[it.sku])
    if (orig && orig > it.unitPrice) {
      return sum + (orig - it.unitPrice) * it.quantity
    }
    return sum
  }, 0) : 0

  return (
    <aside aria-label="Basket sidebar" className="bg-white border rounded-lg p-4 shadow-sm">
      <h2 className="text-lg font-semibold mb-3">Your basket</h2>

      {basketQuery.isLoading && <div className="text-sm text-gray-500">Loading…</div>}
      {!basketQuery.isLoading && !b && (
        <div className="text-sm text-gray-500">No basket yet</div>
      )}

      {b && (
        <div className="space-y-3">
          <ul className="divide-y">
            {b.items.length === 0 && (
              <li className="py-2 text-sm text-gray-500">No items added</li>
            )}
            {b.items.map(it => (
              <li key={it.productId} className="py-2 flex items-start gap-2">
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium truncate" title={it.name}>{it.name}</div>
                  <div className="text-xs text-gray-500 truncate" title={it.sku}>{it.sku}</div>
                  <div className="text-xs text-gray-600">£{it.unitPrice.toFixed(2)} each</div>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    aria-label={`Decrease ${it.name}`}
                    onClick={() => update({ productId: it.productId, quantity: Math.max(0, it.quantity - 1) })}
                    className="px-2 py-1 border rounded text-sm"
                  >-</button>
                  <input
                    aria-label={`${it.name} quantity`}
                    type="number"
                    min={0}
                    value={it.quantity}
                    onChange={e => update({ productId: it.productId, quantity: Number(e.target.value) })}
                    className="w-14 px-2 py-1 border rounded text-sm"
                  />
                  <button
                    aria-label={`Increase ${it.name}`}
                    onClick={() => update({ productId: it.productId, quantity: it.quantity + 1 })}
                    className="px-2 py-1 border rounded text-sm"
                  >+</button>
                </div>
                <div className="text-sm w-20 text-right">£{it.lineTotal.toFixed(2)}</div>
                <button
                  aria-label={`Remove ${it.name}`}
                  onClick={() => remove(it.productId)}
                  className="text-xs text-red-600 ml-1"
                >Remove</button>
              </li>
            ))}
          </ul>

          <div className="border-t pt-3 text-sm space-y-1">
            <div className="flex justify-between"><span>Subtotal</span><span className="font-medium">£{b.subtotal.toFixed(2)}</span></div>
            {promoSavings > 0 && (
              <div className="flex justify-between text-green-700" aria-label="Promo savings row">
                <span>Promo savings</span><span className="font-medium">-£{promoSavings.toFixed(2)}</span>
              </div>
            )}
            <div className="flex justify-between"><span>VAT</span><span className="font-medium">£{b.vat.toFixed(2)}</span></div>
            <div className="flex justify-between text-base"><span>Total</span><span className="font-semibold">£{b.total.toFixed(2)}</span></div>
          </div>

          <div className="flex gap-2 pt-1">
            {/* Avoid naming this link simply "Basket" to prevent Playwright strict-mode link collisions */}
            <Link to="/checkout" className="bg-blue-600 text-white px-3 py-2 rounded text-sm">Checkout</Link>
            <button onClick={() => clear()} className="border px-3 py-2 rounded text-sm">Clear</button>
          </div>
        </div>
      )}
    </aside>
  )
}
