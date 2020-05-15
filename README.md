# API WITH SWAGGER AND AAD

This repository contains a sample API protected by AAD and a Swagger UI accessing the API as a client through oauth2.
The Swagger UI is using implicit flow.

## Prerequisites - Register API in AAD

1. Register an API on AAD 
	1. Add Application ID URI (used as audience and as prefix for scopes) (e.g. api://b1e43d8e-5c99-47e1-8adb-1967abfac058/test-api)
	2. Add sample scopes (e.g. Invoice.Read, Product.Read)

2. Register a CLIENT on AAD (to assign to swagger UI)
	1. Set the api scopes that swagger UI needs to access
	2. Set the following redirect uri: https://localhost:5001/swagger/oauth2-redirect.html
	3. [Optional - Implicit flow] Set implicit flow to return id_token and access_token from authorization endpoint if you want to use this flow
	4. [Optional - Authorization Code flow] Create a client secret if you want to use authorization code flow on swagger (i didn't achieve to make it work because of CORS errors)

3. Edit startup.cs to enable jwt authentication and authorization for API and Swagger

4. Add [authorize] on controllers

### Partial references

https://jpda.dev/using-nswag-and-swagger-ui-with-b2c // this one uses aad b2c whose endpoint are slightly different. This is also a nice reference to see how to set app registration on AAD.
https://joonasw.net/view/testing-azure-ad-protected-apis-part-1-swagger-ui // this one uses scopes but the article isn't flowless 