import { useBasket } from '../hooks/useBasket'

export default function Basket() {
  const { basketQuery, update, remove, clear } = useBasket()
  const b = basketQuery.data

  if (basketQuery.isLoading) return <div>Loading…</div>
  if (!b) return <div>No basket</div>

  return (
    <div className="space-y-6">
      <table className="w-full bg-white border">
        <thead>
          <tr className="bg-gray-50 text-left">
            <th className="p-2">Item</th>
            <th className="p-2">Unit</th>
            <th className="p-2">Qty</th>
            <th className="p-2">Line</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {b.items.map(it => (
            <tr key={it.productId} className="border-t">
              <td className="p-2">
                <div className="font-medium">{it.name}</div>
                <div className="text-xs text-gray-500">{it.sku}</div>
              </td>
              <td className="p-2">£{it.unitPrice.toFixed(2)}</td>
              <td className="p-2">
                <input type="number" min={0} value={it.quantity} onChange={e => update({ productId: it.productId, quantity: Number(e.target.value) })} className="border rounded px-2 py-1 w-20" />
              </td>
              <td className="p-2">£{it.lineTotal.toFixed(2)}</td>
              <td className="p-2">
                <button onClick={() => remove(it.productId)} className="text-sm text-red-600">Remove</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="flex items-center gap-6">
        <div className="ml-auto text-right space-y-1">
          <div>Subtotal: <span className="font-semibold">£{b.subtotal.toFixed(2)}</span></div>
          <div>VAT: <span className="font-semibold">£{b.vat.toFixed(2)}</span></div>
          <div>Total: <span className="font-semibold">£{b.total.toFixed(2)}</span></div>
        </div>
      </div>

      <div className="flex items-center gap-3">
        <button onClick={() => clear()} className="bg-gray-100 border px-3 py-2 rounded">Clear</button>
      </div>
    </div>
  )
}
