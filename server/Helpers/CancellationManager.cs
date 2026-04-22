namespace server.Helpers;


public class CancellationManager
{

    // Ny hjälpklass – kombinerar request-CT med app shutdown
    public sealed class TokenLinker(IHostApplicationLifetime appLifetime)
    {
        private readonly CancellationToken _appStopping = appLifetime.ApplicationStopping;

        /// <summary>
        /// Kombinerar request-CT med app shutdown-token.
        /// Returnerar en CT som avbryts om antingen klienten kopplar från ELLER servern stängs.
        /// Användning: using var ct = tokenLinker.Link(requestCt);
        /// </summary>
        public LinkedToken Link(CancellationToken requestCt = default)
        {
            if (!requestCt.CanBeCanceled)
                return new LinkedToken(null, _appStopping);

            var linked = CancellationTokenSource.CreateLinkedTokenSource(requestCt, _appStopping);
            return new LinkedToken(linked, linked.Token);
        }
    }

    // Wrapper som exponerar CT och dispoasar CTS
    public readonly struct LinkedToken(CancellationTokenSource? cts, CancellationToken token) : IDisposable
    {
        public CancellationToken Token => token;
        public static implicit operator CancellationToken(LinkedToken lt) => lt.Token;
        public void Dispose() => cts?.Dispose();
    }

    // struct = allokeras på stacken, inte heapen
    // readonly = kan inte ändras efter skapande
    // Tillsammans = noll heap-allokering, perfekt för något som skapas/förstörs per request
}
