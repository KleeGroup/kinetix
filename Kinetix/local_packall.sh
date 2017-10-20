
set -x

for dir in $(find . -type d -name 'Kinetix*' -maxdepth 1)
do
  pushd .
  cd $dir
  rm *.nupkg
  nuget pack *.csproj -properties Configuration=Release -Symbols
  nuget add *.symbols.nupkg -source ~/nupkg_local/
  #nuget add *.symbols.nupkg -source ~/nupkg_local/
  popd
done

