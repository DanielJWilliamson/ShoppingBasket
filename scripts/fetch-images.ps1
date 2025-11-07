# Downloads royalty-free demo images for the catalog into wwwroot/img
# Uses picsum.photos with deterministic seeds (no API key required).
# Run from the repo root or adjust $root accordingly.

param(
  [string]$Root = "$(Split-Path -Parent $PSScriptRoot)"
)

$ErrorActionPreference = 'Stop'

$imgDir = Join-Path $Root 'src/backend/ShoppingBasket/wwwroot/img'
New-Item -ItemType Directory -Force -Path $imgDir | Out-Null

$files = @(
  @{ Name = 'book1.jpg';    Query = 'programming book cover' },
  @{ Name = 'book2.jpg';    Query = 'dotnet book cover' },
  @{ Name = 'cable.jpg';    Query = 'usb c cable' },
  @{ Name = 'charger.jpg';  Query = '65w power adapter' },
  @{ Name = 'kettle.jpg';   Query = 'electric kettle' },
  @{ Name = 'mug.jpg';      Query = 'ceramic mug' },
  @{ Name = 'rccar.jpg';    Query = 'rc car toy' },
  @{ Name = 'yogamat.jpg';  Query = 'yoga mat' }
)

function Get-ImageUrl([string]$seed) {
  # Deterministic 800x500 image per seed
  $encoded = [uri]::EscapeDataString($seed)
  return "https://picsum.photos/seed/${encoded}/800/500"
}

foreach ($f in $files) {
  $url = Get-ImageUrl $f.Query
  $out = Join-Path $imgDir $f.Name
  Write-Host "Downloading $($f.Name) from: $url"
  Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing
}

Write-Host "Images saved to: $imgDir"
