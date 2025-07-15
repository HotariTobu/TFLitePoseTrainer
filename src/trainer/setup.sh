#!/bin/bash

PROTOBUF_URL='https://files.pythonhosted.org/packages/52/f3/b9655a711b32c19720253f6f06326faf90580834e2e83f840472d752bc8b/protobuf-6.31.1.tar.gz'

SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
cd "$SCRIPT_DIR"

# Install uv
which uv
if [ $? -ne 0 ]; then
    powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
fi

# Setup another-protobuf
mkdir -p another-protobuf
curl -L -o another-protobuf/source.tar.gz "$PROTOBUF_URL"
tar -xzf another-protobuf/source.tar.gz -C another-protobuf --strip-components=1
