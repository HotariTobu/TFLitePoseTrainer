#!/bin/bash

PROTOBUF_URL='https://files.pythonhosted.org/packages/52/f3/b9655a711b32c19720253f6f06326faf90580834e2e83f840472d752bc8b/protobuf-6.31.1.tar.gz'

SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
cd "$SCRIPT_DIR"

echo "Setting up trainer..."
echo "Current directory: $(pwd)"

# Install uv
which uv
if [ $? -eq 0 ]; then
    echo "uv is already installed"
else
    echo "Installing uv..."

    powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"

    if [ $? -eq 0 ]; then
        echo "uv installed successfully"
    else
        echo "Failed to install uv"
        exit 1
    fi
fi

# Setup another-protobuf
if [ -d another-protobuf ]; then
    echo "another-protobuf is already installed"
else
    echo "Installing another-protobuf..."

    mkdir another-protobuf
    curl -L -o another-protobuf/source.tar.gz "$PROTOBUF_URL"
    tar -xzf another-protobuf/source.tar.gz -C another-protobuf --strip-components=1

    if [ $? -eq 0 ]; then
        echo "another-protobuf installed successfully"
    else
        echo "Failed to install another-protobuf"
        exit 2
    fi
fi

echo "Trainer setup complete."
