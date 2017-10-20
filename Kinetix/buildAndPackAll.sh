

MSBUILD=$(reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0" -v "MSBuildToolsPath" | tail -n 2 | head -n 1 | tr -s ' ' | cut -d ' ' -f 4)

echo "Building Solution"
$MSBUILD\msbuild.exe "Solution.sln" "-t:Clean,Build" "-verbosity:quiet" "-logger:FileLogger,Microsoft.Build.Engine;logfile=build.log"

if [ $? != 0 ] 
then
	echo "Build Failed"
	exit 1
fi

echo "Build Successful"

for dir in $(find . -type d -name 'Kinetix*' -maxdepth 1)
do
  pushd .
  cd $dir
  echo $dir
  rm *.nupkg 2>&-
  nuget pack *.csproj -properties Configuration=Release -Symbols
  popd
done

