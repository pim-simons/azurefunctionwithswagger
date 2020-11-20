# Azure Function with Swagger

To add the swagger UI to your Azure function, execute the following steps:

1. Add the nuget package Aliencube.AzureFunctions.Extensions.OpenApi.

2. Add the `OpenApiOperation`, `OpenApiRequestBody` and `OpenApiResponseWithBody` attributes to your Function method.

3. Add the following to your local.settings.json:

`"Values": { "OpenApi__Info__Version": "3.0.0", "OpenApi__Info__Title": "{Title of your function}"}`

4. Add the following to your host.json:

`"openApi": { "info": { "version": "3.0.0", "title": "{Title of your function}", "description": "{Description}" } }`