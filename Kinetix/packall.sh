
for dir in $(find . -type d -name 'Kinetix*' -maxdepth 1)
do
  pushd .
  cd $dir
  nuget pack *.csproj -Symbols
  popd
done

