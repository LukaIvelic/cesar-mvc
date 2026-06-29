# Google Sign-In Setup

Google Sign-In is implemented through ASP.NET Core Identity external login.

The provider is registered only when both configuration values exist:

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
```

Make sure the copied values do not include leading or trailing spaces.

For local development over the default HTTP profile, add this authorized redirect URI in Google Cloud Console:

```text
http://localhost:5030/signin-google
```

If you start the `https` launch profile instead, also add:

```text
https://localhost:7115/signin-google
```

The redirect URI in Google Cloud Console must exactly match the scheme, host, port, and path used by the app.

After setting credentials, restart the application and open:

```text
/Account/Login
```
