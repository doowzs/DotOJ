#!/usr/bin/env bash
set -e

DOTNET_DIR=$(dirname "$(dirname "$(dotnet --info | grep "Base Path" | cut -d' ' -f 6)")")
CSC_PATH=$(find "$DOTNET_DIR" -name csc.dll -print | sort | tail -n1)
NETSTANDARD_PATH=$(find "$DOTNET_DIR" -path "*sdk/*/ref/netstandard.dll" ! -path "*NuGetFallback*" -print | sort | tail -n1)
DOTNET_CSC_RUNTIME_CONFIG="$DOTNET_DIR"/csc-console-apps.runtimeconfig.json

if [ ! -f "$DOTNET_CSC_RUNTIME_CONFIG" ]; then
  DOTNET_RUNTIME_VERSION=$(dotnet --list-runtimes | grep Microsoft\.NETCore\.App | tail -1 | cut -d' ' -f2)

  cat << EOF > "$DOTNET_CSC_RUNTIME_CONFIG"
{
  "runtimeOptions": {
    "framework": {
      "name": "Microsoft.NETCore.App",
      "version": "$DOTNET_RUNTIME_VERSION"
    }
  }
}
EOF
fi

# Prepare csc
cat <<EOF >/usr/bin/csc
#!/usr/bin/env bash
set -e
dotnet $CSC_PATH /r:$NETSTANDARD_PATH \${1:+"\$@"} 
EOF
chmod +x /usr/bin/csc

# Prepare csr
cat <<EOF >/usr/bin/csr
#!/usr/bin/env bash
set -e
dotnet exec --runtimeconfig $DOTNET_CSC_RUNTIME_CONFIG \${1:+"\$@"} 
EOF
chmod +x /usr/bin/csr
