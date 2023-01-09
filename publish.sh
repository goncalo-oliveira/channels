#!/bin/sh
if [ $# -eq 0 ]
  then
    echo "use: nuget-push <api-key> [component]"
    echo
    echo "example: nuget-push my-secret-api-key abstractions"
    echo
    exit 1
fi

if [ $# -eq 2 ]
  then
    # pack and push a single component package

    # pack component package
    dotnet pack -c release src/$2

    # retrieve latest component package
    package=`ls -Art src/$2/bin/release/*.nupkg | tail -n 1`

    dotnet nuget push -s nuget.org -k $1 $package
  else
    # pack and push all component packages

    # pack abstractions
    dotnet pack -c release src/abstractions

    # retrieve latest abstractions package
    package=`ls -Art src/abstractions/bin/release/*.nupkg | tail -n 1`

    dotnet nuget push -s nuget.org -k $1 $package

    # pack buffers
    dotnet pack -c release src/buffers

    # retrieve latest buffers package
    package=`ls -Art src/buffers/bin/release/*.nupkg | tail -n 1`

    dotnet nuget push -s nuget.org -k $1 $package

    # pack channels
    dotnet pack -c release src/channels

    # retrieve latest channels package
    package=`ls -Art src/channels/bin/release/*.nupkg | tail -n 1`

    dotnet nuget push -s nuget.org -k $1 $package
fi
