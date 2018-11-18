namespace Galcorp.Auth.UWP
{
    using Windows.ApplicationModel.Activation;

    public delegate void ApplicationActivationEvent(IActivatedEventArgs args);

    public static class AppEventWrapper
    {
        public static event ApplicationActivationEvent ApplicationActivationEvent;

        public static void OnApplicationActivationEvent(IActivatedEventArgs args)
        {
            ApplicationActivationEvent?.Invoke(args);
        }
    }
}