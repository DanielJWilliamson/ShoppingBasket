import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'
import { useBasket } from '../hooks/useBasket'

export default function Orders() {
  const { customerId } = useBasket()
  const orders = useQuery({
    queryKey: ['orders', customerId],
    queryFn: () => api.orders.list(customerId)
  })

  if (orders.isLoading) return <div>Loading…</div>

  return (
    <div className="space-y-4">
      <table className="w-full bg-white border">
        <thead>
          <tr className="bg-gray-50 text-left">
            <th className="p-2">Order</th>
            <th className="p-2">Created</th>
            <th className="p-2">Items</th>
            <th className="p-2">Total</th>
          </tr>
        </thead>
        <tbody>
          {orders.data?.map(o => (
            <tr key={o.id} className="border-t">
              <td className="p-2 font-medium">{o.orderNumber}</td>
              <td className="p-2">{new Date(o.createdAtUtc).toLocaleString()}</td>
              <td className="p-2">{o.items.length}</td>
              <td className="p-2">£{o.total.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
