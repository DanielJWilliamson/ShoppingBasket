import { useMemo, useState } from 'react'
import { useBasket } from '../hooks/useBasket'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import { useNavigate } from 'react-router-dom'

export default function Checkout() {
  const { basketId, customerId, basketQuery, update, remove, applyVoucher, removeVoucher } = useBasket()
  const [includeVat, setIncludeVat] = useState(true)
  const [lineVat, setLineVat] = useState<Record<number, boolean>>({})
  const [voucher, setVoucher] = useState('')
  const [shippingCountry, setShippingCountry] = useState<'GB' | 'OTHER'>('GB')
  const qc = useQueryClient()
  const nav = useNavigate()

  const basket = basketQuery.data

  // Initialize per-line VAT map when basket loads or when default includeVat changes
  const ensureLineVat = () => {
    if (!basket) return
    setLineVat((prev) => {
      const next: Record<number, boolean> = { ...prev }
      for (const it of basket.items) {
        if (next[it.productId] == null) next[it.productId] = includeVat
      }
      return next
    })
  }

  // Run once when basket data arrives
  useMemo(() => {
    ensureLineVat()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [basket])

  const vatRate = 0.2
  const preview = useMemo(() => {
    if (!basket) return { subtotal: 0, vat: 0, total: 0 }
    // Apply client-side discount preview consistent with server: vouchers apply only to non-exempt items
    // We use basket.appliedDiscountCodes sequence to reduce each non-exempt line
    const codes = basket.appliedDiscountCodes.map(c => c.toLowerCase())
    const discountPct = (code: string) => code === '10percent' ? 0.10 : code === '20percent' ? 0.20 : code === '30percent' ? 0.30 : 0
    const applySeq = (amount: number) => {
      let v = amount
      for (const c of codes) {
        const pct = discountPct(c)
        if (pct > 0) v = Math.round(v * (1 - pct) * 100) / 100
      }
      return v
    }
    const discountedSubtotal = basket.items.reduce((sum, it) => {
      let line = it.unitPrice * it.quantity
      if (!it.isDiscountExempt) {
        for (const c of codes) {
          const pct = discountPct(c)
          if (pct > 0) line = Math.round(line * (1 - pct) * 100) / 100
        }
      }
      return sum + line
    }, 0)
    const shippingBase = shippingCountry === 'GB' ? 10 : 20
    const discountedShipping = applySeq(shippingBase)
    const vat = Number(
      basket.items.reduce((s, it) => {
        const on = lineVat[it.productId] ?? includeVat
        let line = it.unitPrice * it.quantity
        if (!it.isDiscountExempt) {
          for (const c of codes) {
            const pct = discountPct(c)
            if (pct > 0) line = Math.round(line * (1 - pct) * 100) / 100
          }
        }
        return s + (on ? line * vatRate : 0)
      }, 0).toFixed(2)
    ) as unknown as number
    const subtotalN = Number(discountedSubtotal.toFixed(2))
    const total = Number((subtotalN + discountedShipping + Number(vat)).toFixed(2))
    return { subtotal: subtotalN, vat: Number(vat), total, shipping: discountedShipping }
  }, [basket, lineVat, includeVat, shippingCountry])

  const checkout = useMutation({
    mutationFn: async () => {
      if (basketId == null) throw new Error('No basket')
      // Only send overrides that differ from the default includeVat
      const overrides = basket?.items
        .map(it => ({ productId: it.productId, includeVat: lineVat[it.productId] ?? includeVat }))
        .filter(ov => ov.includeVat !== includeVat)
      return api.orders.checkout(basketId, customerId, includeVat, overrides && overrides.length ? overrides : undefined, shippingCountry === 'GB' ? 'GB' : 'OTHER')
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['basket'] })
      await qc.invalidateQueries({ queryKey: ['orders'] })
      nav('/orders')
    }
  })

  return (
    <div className="space-y-6">
      {/* Voucher and Shipping controls */}
      <div className="flex flex-wrap items-end gap-4">
        <div>
          <label className="block text-sm text-gray-700 mb-1">Voucher code</label>
          <div className="flex gap-2">
            <input
              value={voucher}
              onChange={e => setVoucher(e.target.value)}
              placeholder="10percent / 20percent / 30percent"
              className="border rounded px-3 py-2"
              aria-label="Voucher code"
            />
            <button
              type="button"
              className="px-3 py-2 rounded border bg-gray-100 hover:bg-gray-200"
              onClick={async () => { if (voucher.trim()) { await applyVoucher(voucher.trim()); setVoucher('') } }}
            >Apply</button>
          </div>
          {basket && basket.appliedDiscountCodes.length > 0 && (
            <div className="mt-2 text-sm">
              Applied: {basket.appliedDiscountCodes.map(c => (
                <span key={c} className="inline-flex items-center gap-1 px-2 py-1 text-xs rounded bg-blue-50 border border-blue-200 mr-2">
                  {c}
                  <button aria-label={`Remove voucher ${c}`} onClick={() => removeVoucher(c)}>×</button>
                </span>
              ))}
            </div>
          )}
        </div>
        <div>
          <label className="block text-sm text-gray-700 mb-1">Shipping destination</label>
          <div className="flex gap-3">
            <label className="inline-flex items-center gap-2">
              <input type="radio" name="ship" checked={shippingCountry==='GB'} onChange={() => setShippingCountry('GB')} />
              UK (£10)
            </label>
            <label className="inline-flex items-center gap-2">
              <input type="radio" name="ship" checked={shippingCountry==='OTHER'} onChange={() => setShippingCountry('OTHER')} />
              Outside UK (£20)
            </label>
          </div>
        </div>
      </div>
      {/* Top spacer (master VAT toggle moved to totals area) */}

      {/* Basket lines with per-item VAT toggle */}
      {basket && (
        <div className="overflow-x-auto">
          <table className="w-full bg-white border">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Item</th>
                <th className="p-2">Unit</th>
                <th className="p-2">Qty</th>
                <th className="p-2">Line (net)</th>
                <th className="p-2">VAT</th>
                <th className="p-2">Gross</th>
                <th className="p-2">VAT on?</th>
                <th className="p-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {basket.items.map(it => {
                const net = Number((it.unitPrice * it.quantity).toFixed(2))
                const on = lineVat[it.productId] ?? includeVat
                const vat = Number((on ? net * vatRate : 0).toFixed(2))
                const gross = Number((net + vat).toFixed(2))
                return (
                  <tr key={it.productId} className="border-t">
                    <td className="p-2">
                      <div className="font-medium flex items-center gap-2">
                        {it.name}
                        {it.isDiscountExempt && (
                          <span className="text-[10px] uppercase tracking-wide bg-yellow-100 border border-yellow-300 text-yellow-800 px-1.5 py-0.5 rounded">Promo</span>
                        )}
                      </div>
                      <div className="text-xs text-gray-500">{it.sku}</div>
                    </td>
                    <td className="p-2">£{it.unitPrice.toFixed(2)}</td>
                    <td className="p-2">
                      <div className="inline-flex items-center gap-2">
                        <button
                          type="button"
                          aria-label={`Decrease ${it.name} quantity`}
                          className="px-2 py-1 rounded border bg-gray-100 hover:bg-gray-200"
                          onClick={() => update({ productId: it.productId, quantity: it.quantity - 1 })}
                        >
                          −
                        </button>
                        <span className="min-w-6 text-center" aria-live="polite">{it.quantity}</span>
                        <button
                          type="button"
                          aria-label={`Increase ${it.name} quantity`}
                          className="px-2 py-1 rounded border bg-gray-100 hover:bg-gray-200"
                          onClick={() => update({ productId: it.productId, quantity: it.quantity + 1 })}
                        >
                          +
                        </button>
                      </div>
                    </td>
                    <td className="p-2">£{net.toFixed(2)}</td>
                    <td className="p-2">£{vat.toFixed(2)}</td>
                    <td className="p-2 font-medium">£{gross.toFixed(2)}</td>
                    <td className="p-2">
                      <button
                        type="button"
                        aria-pressed={on}
                        onClick={() => setLineVat(m => ({ ...m, [it.productId]: !on }))}
                        className={`text-xs px-2 py-1 rounded border ${on ? 'bg-blue-600 text-white border-blue-600' : 'bg-gray-100 text-gray-800'}`}
                      >
                        {on ? 'With VAT' : 'No VAT'}
                      </button>
                    </td>
                    <td className="p-2">
                      <button
                        type="button"
                        aria-label={`Remove all ${it.name} from basket`}
                        className="text-xs px-2 py-1 rounded border bg-red-600 text-white border-red-600 hover:bg-red-700"
                        onClick={() => remove(it.productId)}
                      >
                        Clear
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Totals preview */}
      <div className="flex items-start gap-6">
        <div className="ml-auto text-right space-y-2">
          {/* Master VAT toggle (bottom-right), clearly labeled */}
          <div className="flex items-center justify-end gap-3">
            <div className="text-sm text-gray-700">All items VAT:</div>
            <button
              type="button"
              aria-pressed={includeVat}
              onClick={() => {
                const next = !includeVat
                setIncludeVat(next)
                // apply to all current lines (user can still tweak after)
                if (basket) {
                  const m: Record<number, boolean> = {}
                  for (const it of basket.items) m[it.productId] = next
                  setLineVat(m)
                }
              }}
              className={`inline-flex items-center gap-2 px-3 py-2 rounded border transition ${includeVat ? 'bg-blue-600 text-white border-blue-600' : 'bg-gray-100 text-gray-800'}`}
            >
              {includeVat ? 'With VAT' : 'Without VAT'}
            </button>
          </div>
          <div className="text-xs text-gray-500">Sets the default for the whole order. Override per item above if needed.</div>
          <div className="pt-1 space-y-1" data-testid="totals">
            <div data-testid="subtotal-label">{(basket?.appliedDiscountCodes.length ?? 0) > 0 ? 'Discounted subtotal' : 'Subtotal'}: <span className="font-semibold">£{preview.subtotal.toFixed(2)}</span></div>
            <div>Shipping: <span className="font-semibold">£{(preview as any).shipping?.toFixed ? (preview as any).shipping.toFixed(2) : (shippingCountry==='GB'?10:20).toFixed(2)}</span></div>
            <div>VAT: <span className="font-semibold">£{preview.vat.toFixed(2)}</span></div>
            <div>Total: <span className="font-semibold">£{(preview as any).total?.toFixed ? (preview as any).total.toFixed(2) : (preview.subtotal + preview.vat + (shippingCountry==='GB'?10:20)).toFixed(2)}</span></div>
          </div>
        </div>
      </div>

      <button data-testid="confirm-order" onClick={() => checkout.mutate()} className="bg-green-600 text-white px-4 py-2 rounded">
        {checkout.isPending ? 'Processing…' : 'Confirm order'}
      </button>
      {checkout.isError && <div className="text-red-600 text-sm">Checkout failed</div>}
    </div>
  )
}
