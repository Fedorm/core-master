cd MobileClient
xbuild /p:Configuration=Release /p:Platform=iPhone /p:BuildIpa=true /target:Build MobileClient.MT.sln
cd Droid
xbuild Droid.csproj /p:Configuration=Release /t:SignAndroidPackage
