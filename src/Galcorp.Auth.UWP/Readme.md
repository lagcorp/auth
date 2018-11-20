Only works in fall creators update

based on https://github.com/googlesamples/oauth-apps-for-windows

Google Documentation
--------------------

The protocols referenced in this sample are documented here:

- [OAuth 2.0](https://developers.google.com/identity/protocols/OAuth2)
- [Using OAuth 2.0 for Mobile and Desktop Applications](https://developers.google.com/identity/protocols/OAuth2InstalledApp)

Using your own credentials
--------------------------

The Sample comes backed with some demo client credentials, which are fine for
testing, but make sure you use your own credentials before releasing any app,
or sharing it with friends.

1. Visit the [Credentials page of the Developers Console](https://console.developers.google.com/apis/credentials?project=_)
2. Create a new OAuth 2.0 client, select `iOS` (yes, it's a little strange to
select iOS, but the way the OAuth client works with UWP is similar to iOS, 
so this is currently the correct client type to create).
3. As your bundle ID, enter your domain name in reverse DNS notation. E.g.
if your domain was "example.com", use "com.example" as your bundle ID.
Note that your bundle ID MUST contain a period character `.`, and MUST be
less than 39 characters long
4. Copy the created client-id and replace the clientID value in this sample
5. Edit the manifest by right-clicking and selecting "View Code" (due to a
limitation of Visual Studio it wasn't possible to declare a URI scheme
containing a period in the UI).
6. Find the "Protocol" scheme, and replace it with the bundle id you registered
in step 3. (e.g. "com.example")


--------------------------
protected override void OnActivated(IActivatedEventArgs args)
{
    Galcorp.Auth.UWP.AppEventWrapper.OnApplicationActivationEvent(args);
}
