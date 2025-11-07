export type ProductDto = {
  id: number
  sku: string
  name: string
  description: string
  price: number
  imageUrl?: string | null
  category?: string | null
}

export type BasketItemDto = {
  productId: number
  sku: string
  name: string
  unitPrice: number
  quantity: number
  lineTotal: number
  isDiscountExempt: boolean
  originalUnitPrice?: number | null
}

export type BasketDto = {
  id: number
  customerId?: string | null
  subtotal: number
  vat: number
  total: number
  items: BasketItemDto[]
  appliedDiscountCodes: string[]
}

export type OrderItemDto = {
  productId: number
  sku: string
  name: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export type OrderDto = {
  id: number
  orderNumber: string
  createdAtUtc: string
  subtotal: number
  discountedSubtotal: number
  shipping: number
  vat: number
  total: number
  items: OrderItemDto[]
}
