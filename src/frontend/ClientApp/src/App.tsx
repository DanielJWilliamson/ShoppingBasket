import { Outlet, Link, NavLink } from 'react-router-dom'
import { useEffect } from 'react'
import { useLiveUpdates } from './signalr/liveClient'
import { useBasket } from './hooks/useBasket'

export default function App() {
  const { connected } = useLiveUpdates()
  const { basketQuery } = useBasket()
  const count = (basketQuery.data?.items ?? []).reduce((s, it) => s + it.quantity, 0)
  useEffect(() => {
    // Side effect when connection changes; noop for now
  }, [connected])

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="max-w-6xl mx-auto px-4 py-3 flex items-center gap-6">
          <Link className="text-xl font-semibold" to="/">Shopping Basket</Link>
          <nav className="flex items-center gap-4 text-sm">
            <NavLink to="/" className={({isActive}) => isActive ? 'text-blue-600' : ''}>Products</NavLink>
              {/* Route Basket link directly to checkout to simplify the flow */}
              <NavLink to="/checkout" aria-label="Basket" className={({isActive}) => isActive ? 'text-blue-600' : ''}>
                Basket
                <span aria-label="basket-count" aria-hidden="true" className="ml-1 inline-flex items-center justify-center min-w-5 h-5 px-1 text-[11px] rounded-full bg-blue-600 text-white">
                  {count}
                </span>
              </NavLink>
            <NavLink to="/orders" className={({isActive}) => isActive ? 'text-blue-600' : ''}>Orders</NavLink>
          </nav>
          {/* Removed Live/Offline badge to reduce header noise */}
          <div className="ml-auto" />
        </div>
      </header>
      <main className="flex-1">
        <div className="max-w-6xl mx-auto px-4 py-6">
          <Outlet />
        </div>
      </main>
      <footer className="text-center text-xs text-gray-500 py-4 border-t">© {new Date().getFullYear()}</footer>
    </div>
  )
}
