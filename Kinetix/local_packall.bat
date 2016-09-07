
for dir in $(find . -type d -name 'Kinetix*' -maxdepth 1)
do
  pushd .
  cd $dir
  rm *.nupkg
  nuget pack *.csproj -Symbols
  nuget add *.symbols.nupkg -source ~/nupkg_local/
  popd
done

