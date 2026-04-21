import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Hotel, KeyRound, ArrowRight, LogOut } from 'lucide-react';
import { useAuthStore } from '../../lib/store';
import activationService from '../../services/activation.service';

export function SetupWorkspacePage() {
  const navigate = useNavigate();
  const { user, setAuth, logout } = useAuthStore();
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleActivate = async () => {
    const trimmed = code.trim();
    if (!trimmed) { setError('Please enter your activation code.'); return; }
    setError('');
    setIsLoading(true);
    try {
      const result = await activationService.activateMyAccount(trimmed);

      // The backend issues a fresh token with the new tenantId in headers
      const newToken = result.tokenFromHeader ?? result.token;
      const newRefreshToken = result.refreshTokenFromHeader ?? result.refreshToken;
      const newTenantId = result.tenantIdFromHeader ? Number(result.tenantIdFromHeader) : result.tenantId;

      // Persist fresh tokens
      localStorage.setItem('hotel_ms_token', newToken);
      if (newRefreshToken) localStorage.setItem('hotel_ms_refresh_token', newRefreshToken);

      // Update auth store with new tenantId and SuperAdmin role
      setAuth(
        {
          email: user?.email ?? '',
          fullName: user?.fullName ?? '',
          roles: ['SuperAdmin'],
          tenantId: newTenantId,
          picture: user?.picture,
        },
        newToken
      );

      navigate('/dashboard', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Activation failed. Please check your code and try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleActivate();
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-slate-50 dark:from-slate-900 dark:via-slate-800 dark:to-slate-900 flex flex-col items-center justify-center px-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex items-center justify-center gap-2.5 mb-8">
          <div className="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center shadow-sm">
            <Hotel className="h-6 w-6 text-white" />
          </div>
          <span className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Hotel<span className="text-indigo-600">MS</span>
          </span>
        </div>

        <div className="bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-700 p-8">
          <div className="flex items-center justify-center w-14 h-14 bg-indigo-50 dark:bg-indigo-900/30 rounded-2xl mx-auto mb-5">
            <KeyRound className="h-7 w-7 text-indigo-600" />
          </div>

          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100 text-center mb-2">
            Activate your workspace
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 text-center mb-6">
            Welcome, <strong className="text-slate-700 dark:text-slate-300">{user?.fullName ?? user?.email}</strong>.
            Enter the activation code you received when you registered.
          </p>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">
                Activation code
              </label>
              <input
                type="text"
                value={code}
                onChange={(e) => { setCode(e.target.value); setError(''); }}
                onKeyDown={handleKeyDown}
                placeholder="XXXX-XXXX-XXXX-XXXX"
                className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-4 py-3 text-center text-lg font-mono tracking-widest text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 uppercase"
              />
              {error && <p className="mt-2 text-sm text-red-500">{error}</p>}
            </div>

            <button
              onClick={handleActivate}
              disabled={isLoading}
              className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-semibold rounded-xl transition-colors"
            >
              {isLoading ? (
                <>
                  <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Activating your workspace…
                </>
              ) : (
                <>Activate workspace <ArrowRight className="h-4 w-4" /></>
              )}
            </button>
          </div>

          <div className="mt-5 flex items-center justify-center">
            <button
              onClick={() => { logout(); navigate('/login'); }}
              className="flex items-center gap-1.5 text-xs text-slate-400 dark:text-slate-500 hover:text-slate-600 dark:hover:text-slate-300 transition-colors"
            >
              <LogOut className="h-3.5 w-3.5" />
              Sign out and go to login
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
