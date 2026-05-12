#!/bin/bash
set -e
echo "=== Bible App — Build + Vercel Deploy ==="

echo ">>> dotnet publish..."
dotnet publish -c Release --nologo

echo ">>> vercel deploy..."
vercel --prod --yes

echo "=== Done ==="
