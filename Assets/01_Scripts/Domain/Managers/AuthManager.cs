using System;
using System.Threading.Tasks;

namespace ThreeMatch
{
    public class AuthManager : IManager
    {
        public AuthType CurrentType { get; private set; }
        public bool Authenticated { get; private set; }
        public AuthManager()
        {
            Initialize();
        }

        public void Initialize()
        {
            CurrentType = AuthType.None;
            Authenticated = false;
        }

        public async void Login(AuthType authType)
        {
            CurrentType = AuthType.None;

            Task authTask = null;
            switch (authType)
            {
                case AuthType.None:
                    throw new AuthException($"AuthType:{authType}");
                case AuthType.Guest:
                    authTask = this.GuestLogin();
                    break;
                case AuthType.Apple:
                    authTask = this.AppleLogin();
                    break;
                case AuthType.Google:
                    authTask = this.GoogleLogin();
                    break;
            }

            await authTask;

            if (CurrentType != AuthType.None)
                Authenticated = true;
        }

        private async Task GuestLogin()
        {
            CurrentType = AuthType.Guest;
            await Task.CompletedTask;
        }

        private async Task AppleLogin()
        {
            CurrentType = AuthType.Apple;
            await Task.CompletedTask;
        }

        private async Task GoogleLogin()
        {
            CurrentType = AuthType.Google;
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }
    }

    public class AuthException : Exception
    {
        public AuthException(string msg) : base(msg) { }
    }
}
