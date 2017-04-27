
for dir in $(find . -type d -name 'Kinetix*' -maxdepth 1)
do
  pushd .
  cd $dir
  echo $dir
  rm *.nupkg
  #nuget pack *.csproj -Symbols
  nuget pack *.csproj -properties Configuration=Release
  popd
done

