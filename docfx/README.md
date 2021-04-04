This folder generates the API docs for TM. 

The API documentation is generated using the [DocFX tool](https://github.com/dotnet/docfx). The output of docfx gets put into the `./docs` folder which is then checked in. The `./docs` folder is then picked up by Github Pages and published to Miguel's Github Pages (https://migueldeicaza.github.io/gui.cs/).

## To Generate the Docs

0. Install DotFX https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html
1. Change to the `./docfx` folder and run `./build.ps1`
2. Browse to http://localhost:8080 and verify everything looks good.
3. Hit ctrl-c to stop the script.
