# Speaker Identification Console Sample

## How to use

```
git clone
cd ./Speaker-Identification-Console-Sample 
```

Update `appsettings.json` with your Speaker service key and folder where enrollment audio files are placed (one user profile only).

```
dotnet build
cd ./bin/Debug/netcoreapp2.1/
```

Run:

```
dotnet SpeakerIdentificationProto.dll enroll [<GUID>]
```

* `enroll` = create enrollment
* `<GUID>` = optionally provide previously created profile GUID

