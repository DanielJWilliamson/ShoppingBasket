import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../api/client'
import type { BasketDto } from '../api/types'

const KEY = 'sb:basketId'
const CUSTOMER = 'guest'

export function useBasket() {
  const qc = useQueryClient()
  const [basketId, setBasketId] = useState<number | null>(null)

  useEffect(() => {
    const stored = localStorage.getItem(KEY)
    if (stored) {
      const id = parseInt(stored)
      if (!Number.isNaN(id)) setBasketId(id)
    }
  }, [])

  const ensure = async () => {
    // Always prefer a stored basket ID to avoid creating a new one before state hydrates
    const fromStorage = (() => {
      const s = localStorage.getItem(KEY)
      if (!s) return null
      const id = parseInt(s)
      return Number.isNaN(id) ? null : id
    })()

    if (basketId != null) return basketId
    if (fromStorage != null) {
      if (fromStorage !== basketId) setBasketId(fromStorage)
      return fromStorage
    }
    const created = await api.baskets.create(CUSTOMER)
    setBasketId(created.id)
    localStorage.setItem(KEY, String(created.id))
    return created.id
  }

  const basketQuery = useQuery<BasketDto>({
    queryKey: ['basket', basketId],
    queryFn: async () => {
      const id = await ensure()
      return api.baskets.get(id)
    },
    enabled: true,
  })

  const addMutation = useMutation({
    mutationFn: async ({ productId, quantity }: { productId: number, quantity: number }) => {
      const id = await ensure()
      return api.baskets.addItem(id, productId, quantity)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['basket'] })
    }
  })

  const updateMutation = useMutation({
    mutationFn: async ({ productId, quantity }: { productId: number, quantity: number }) => {
      if (basketId == null) throw new Error('basket missing')
      return api.baskets.updateItem(basketId, productId, quantity)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['basket'] })
  })

  const removeMutation = useMutation({
    mutationFn: async (productId: number) => {
      if (basketId == null) throw new Error('basket missing')
      return api.baskets.removeItem(basketId, productId)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['basket'] })
  })

  const clearMutation = useMutation({
    mutationFn: async () => {
      if (basketId == null) return
      await api.baskets.clear(basketId)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['basket'] })
  })

  const applyVoucherMutation = useMutation({
    mutationFn: async (code: string) => {
      const id = await ensure()
      return api.baskets.applyVoucher(id, code)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['basket'] })
  })

  const removeVoucherMutation = useMutation({
    mutationFn: async (code: string) => {
      if (basketId == null) throw new Error('basket missing')
      return api.baskets.removeVoucher(basketId, code)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['basket'] })
  })

  return {
    basketId,
    basketQuery,
    add: addMutation.mutateAsync,
    update: updateMutation.mutateAsync,
    remove: removeMutation.mutateAsync,
    clear: clearMutation.mutateAsync,
    applyVoucher: applyVoucherMutation.mutateAsync,
    removeVoucher: removeVoucherMutation.mutateAsync,
    customerId: CUSTOMER,
  }
}
