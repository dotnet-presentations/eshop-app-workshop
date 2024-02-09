#!/usr/bin/env bash

# Install the latest version of the Aspire workload from the release branches
./build/installLatestFromReleaseBranch.sh

dotnet build ./build/Build.proj