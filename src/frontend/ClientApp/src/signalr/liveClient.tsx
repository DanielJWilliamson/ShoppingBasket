import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useEffect, useMemo, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'

export function useLiveUpdates() {
  const qc = useQueryClient()
  const [connected, setConnected] = useState(false)

  const conn = useMemo(() => new HubConnectionBuilder()
    .withUrl('/hubs/catalog')
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build(), [])

  useEffect(() => {
    let active = true
    conn.start()
      .then(() => {
        if (!active) return
        setConnected(true)
        conn.on('catalog:changed', () => {
          // invalidate product/basket/orders related caches on any change notification
          qc.invalidateQueries({ queryKey: ['products'] })
          qc.invalidateQueries({ queryKey: ['basket'] })
          qc.invalidateQueries({ queryKey: ['orders'] })
        })
      })
      .catch(() => setConnected(false))

    return () => {
      active = false
      conn.stop()
    }
  }, [conn, qc])

  return { connected }
}
