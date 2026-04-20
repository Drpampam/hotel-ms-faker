import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Navigate } from 'react-router-dom';
import { Eye, EyeOff, Hotel, Lock, Mail, ArrowRight, XCircle, CheckCircle } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';
import { useAuthStore } from '../../lib/store';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { authService } from '../../services/auth.service';

const loginSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
  rememberMe: z.boolean().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

// ── Forgot password modal ─────────────────────────────────────────────────────

function ForgotPasswordModal({ onClose }: { onClose: () => void }) {
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) return;
    setIsLoading(true);
    setError(null);
    try {
      await authService.forgotPassword(email);
      setSent(true);
    } catch {
      setError('Something went wrong. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="bg-slate-900 border border-white/20 rounded-2xl p-6 w-full max-w-sm shadow-2xl">
        {sent ? (
          <div className="text-center py-2">
            <CheckCircle className="h-10 w-10 text-emerald-400 mx-auto mb-3" />
            <h3 className="text-white font-semibold text-base mb-1">Check Your Email</h3>
            <p className="text-slate-400 text-sm mb-5">
              If <span className="text-indigo-300">{email}</span> is registered, you'll receive a reset link shortly.
            </p>
            <button onClick={onClose}
              className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold rounded-xl transition-all text-sm">
              Done
            </button>
          </div>
        ) : (
          <>
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-white font-semibold text-base">Reset Password</h3>
              <button onClick={onClose} className="text-slate-400 hover:text-slate-200 transition-colors">
                <XCircle className="h-5 w-5" />
              </button>
            </div>
            <p className="text-slate-400 text-sm mb-4">
              Enter your email address and we'll send you a link to reset your password.
            </p>
            <form onSubmit={handleSubmit} className="space-y-3">
              <div className="relative">
                <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                  <Mail className="h-4 w-4" />
                </div>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@hotel.com"
                  required
                  autoFocus
                  className="w-full h-11 pl-10 pr-4 bg-white/10 border border-white/20 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all"
                />
              </div>
              {error && (
                <div className="flex items-center gap-2 p-2.5 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-xs">
                  <XCircle className="h-3.5 w-3.5 flex-shrink-0" />
                  {error}
                </div>
              )}
              <button type="submit" disabled={isLoading}
                className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 disabled:bg-indigo-800 disabled:cursor-not-allowed text-white font-semibold rounded-xl flex items-center justify-center gap-2 transition-all text-sm">
                {isLoading
                  ? <div className="h-4 w-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  : 'Send Reset Link'
                }
              </button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}

// ── Login page ────────────────────────────────────────────────────────────────

export function LoginPage() {
  const [showPassword, setShowPassword] = useState(false);
  const [loginError, setLoginError] = useState<string | null>(null);
  const [showForgot, setShowForgot] = useState(false);
  const { login, isLoading } = useAuth();
  const { isAuthenticated, token } = useAuthStore();

  const storedToken = localStorage.getItem('hotel_ms_token');
  if (isAuthenticated || storedToken || token) {
    return <Navigate to="/dashboard" replace />;
  }

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    setLoginError(null);
    const result = await login(data);
    if (!result?.success) {
      setLoginError(result?.message ?? 'Invalid email or password');
    }
    if (result?.success && 'PasswordCredential' in window) {
      try {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const cred = new (window as any).PasswordCredential({ id: data.email, password: data.password, name: data.email });
        await navigator.credentials.store(cred);
      } catch {
        // Credential Management API not supported or blocked — silently ignore
      }
    }
  };

  return (
    <>
    {showForgot && <ForgotPasswordModal onClose={() => setShowForgot(false)} />}
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 flex items-center justify-center p-4">
      {/* Background decorations */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 -left-16 w-64 h-64 bg-indigo-500/10 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 -right-16 w-80 h-80 bg-violet-500/10 rounded-full blur-3xl" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-indigo-600/5 rounded-full blur-3xl" />
      </div>

      <div className="relative w-full max-w-md animate-slide-up">
        {/* Logo / Branding */}
        <div className="text-center mb-4">
          <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-2xl shadow-lg shadow-indigo-600/30 mb-2">
            <Hotel className="h-7 w-7 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white">
            Hotel<span className="text-indigo-400">MS</span>
          </h1>
          <p className="text-slate-400 text-xs mt-0.5">Hotel Management System</p>
        </div>

        {/* Card */}
        <div className="bg-white/10 backdrop-blur-xl rounded-3xl border border-white/20 p-6 shadow-2xl">
          <div className="mb-4">
            <h2 className="text-lg font-semibold text-white">Welcome back</h2>
            <p className="text-slate-400 text-sm mt-0.5">Sign in to your account to continue</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} autoComplete="on" className="space-y-3">
            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1.5">
                Email address <span className="text-red-400">*</span>
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                  <Mail className="h-4 w-4" />
                </div>
                <input
                  {...register('email')}
                  type="email"
                  autoComplete="email"
                  placeholder="you@hotel.com"
                  className="w-full h-11 pl-10 pr-4 bg-white/10 border border-white/20 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                />
              </div>
              {errors.email && (
                <p className="mt-1.5 text-xs text-red-400">{errors.email.message}</p>
              )}
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1.5">
                Password <span className="text-red-400">*</span>
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                  <Lock className="h-4 w-4" />
                </div>
                <input
                  {...register('password')}
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="••••••••"
                  className="w-full h-11 pl-10 pr-12 bg-white/10 border border-white/20 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 hover:text-slate-200 transition-colors"
                >
                  {showPassword ? (
                    <EyeOff className="h-4 w-4" />
                  ) : (
                    <Eye className="h-4 w-4" />
                  )}
                </button>
              </div>
              {errors.password && (
                <p className="mt-1.5 text-xs text-red-400">{errors.password.message}</p>
              )}
            </div>

            {/* Remember me + Forgot password */}
            <div className="flex items-center justify-between -mt-1">
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  {...register('rememberMe')}
                  type="checkbox"
                  className="w-4 h-4 rounded border-white/20 bg-white/10 text-indigo-500 focus:ring-indigo-500 focus:ring-offset-0 cursor-pointer"
                />
                <span className="text-xs text-slate-300">Remember me</span>
              </label>
              <button
                type="button"
                onClick={() => setShowForgot(true)}
                className="text-xs text-indigo-400 hover:text-indigo-300 transition-colors"
              >
                Forgot password?
              </button>
            </div>

            {/* Login error */}
            {loginError && (
              <div className="flex items-center gap-2 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
                <XCircle className="h-4 w-4 flex-shrink-0" />
                <span>{loginError}</span>
              </div>
            )}

            {/* Submit */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 disabled:bg-indigo-800 disabled:cursor-not-allowed text-white font-semibold rounded-xl flex items-center justify-center gap-2 transition-all duration-150 active:scale-[0.98] shadow-lg shadow-indigo-600/30"
            >
              {isLoading ? (
                <div className="h-5 w-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <>
                  Sign in
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </button>
          </form>

          {/* Demo hint */}
          <div className="mt-4 p-2.5 rounded-xl bg-white/5 border border-white/10">
            <p className="text-xs text-slate-400 text-center">
              Use your hotel system credentials to sign in
            </p>
          </div>

        </div>

        <p className="text-center text-xs text-slate-600 mt-6">
          © {new Date().getFullYear()} HotelMS. All rights reserved.
        </p>
      </div>
    </div>
    </>
  );
}

// Default export needed for lazy loading
export default LoginPage;
