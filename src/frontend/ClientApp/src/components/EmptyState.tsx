export default function EmptyState({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="p-8 text-center bg-white border rounded">
      <div className="text-lg font-medium">{title}</div>
      {subtitle && <div className="text-sm text-gray-500 mt-1">{subtitle}</div>}
    </div>
  )
}
