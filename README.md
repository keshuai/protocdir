The "protoc" command can only compile a single file or a single folder.
This "protocdir" allows you to compile multiple folders based on a JSON configuration.

client.json:

```json
{
  "name" : "client",
  
  "input" : [
    "src/shared",
    "src/client"
  ],
  
  "output" : [
    {
      "language" : "csharp",
      "path" : "csharp_out/client"
    }
  ]
}
```

server.json:
```json
{
  "name" : "server",
  
  "input" : [
    "src/shared",
    "src/server"
  ],
  
  "output" : [
    {
      "language" : "csharp",
      "path" : "csharp_out/server"
    }
  ]
}
```



build command:

```bat
protocdir.exe clien.json server.json
```
