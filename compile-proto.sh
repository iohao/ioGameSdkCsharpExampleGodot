#!/bin/bash
protoc --csharp_out=./script/gen --proto_path=. ./proto/common.proto