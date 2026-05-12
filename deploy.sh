#!/bin/bash
set -e
echo "=== Bible App — Build + Firebase Deploy ==="

# 1. Install function dependencies
echo ">>> npm install functions..."
cd functions && npm install && cd ..

# 2. Build Blazor WASM
echo ">>> dotnet publish..."
dotnet publish -c Release --nologo

# 3. Copy output to functions/hosting
PUBLISH_DIR="bin/Release/net9.0/publish/wwwroot"
HOSTING_DIR="functions/hosting"
echo ">>> Copying $PUBLISH_DIR to $HOSTING_DIR..."
rm -rf "$HOSTING_DIR"
cp -r "$PUBLISH_DIR" "$HOSTING_DIR"

# 4. Deploy to Firebase
echo ">>> firebase deploy..."
firebase deploy --only hosting,functions

echo "=== Done ==="
