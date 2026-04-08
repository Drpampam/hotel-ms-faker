import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Hotel, Lock, Eye, EyeOff, CheckCircle, XCircle } from 'lucide-react';
import { authService } from '../../services/auth.service';

const schema = z.object({
  newPassword: z.string().min(6, 'Password must be at least 6 characters'),
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

type FormData = z.infer<typeof schema>;

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const email = searchParams.get('email') ?? '';
  const token = searchParams.get('token') ?? '';

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<{ success: boolean; message: string } | null>(null);

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  if (!email || !token) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 flex items-center justify-center p-4">
        <div className="bg-white/10 backdrop-blur-xl rounded-3xl border border-white/20 p-8 max-w-md w-full text-center">
          <XCircle className="h-12 w-12 text-red-400 mx-auto mb-4" />
          <h2 className="text-white text-lg font-semibold mb-2">Invalid Reset Link</h2>
          <p className="text-slate-400 text-sm mb-6">This password reset link is invalid or has expired.</p>
          <button
            onClick={() => navigate('/login')}
            className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold rounded-xl transition-all"
          >
            Back to Sign In
          </button>
        </div>
      </div>
    );
  }

  const onSubmit = async (data: FormData) => {
    setIsLoading(true);
    try {
      const res = await authService.resetPassword(email, token, data.newPassword);
      setResult(res);
    } catch {
      setResult({ success: false, message: 'An error occurred. Please try again.' });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 flex items-center justify-center p-4">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 -left-16 w-64 h-64 bg-indigo-500/10 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 -right-16 w-80 h-80 bg-violet-500/10 rounded-full blur-3xl" />
      </div>

      <div className="relative w-full max-w-md">
        <div className="text-center mb-4">
          <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-2xl shadow-lg shadow-indigo-600/30 mb-2">
            <Hotel className="h-7 w-7 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white">
            Hotel<span className="text-indigo-400">MS</span>
          </h1>
        </div>

        <div className="bg-white/10 backdrop-blur-xl rounded-3xl border border-white/20 p-6 shadow-2xl">
          {result?.success ? (
            <div className="text-center py-4">
              <CheckCircle className="h-12 w-12 text-emerald-400 mx-auto mb-4" />
              <h2 className="text-white text-lg font-semibold mb-2">Password Reset!</h2>
              <p className="text-slate-400 text-sm mb-6">{result.message}</p>
              <button
                onClick={() => navigate('/login')}
                className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold rounded-xl transition-all"
              >
                Sign In Now
              </button>
            </div>
          ) : (
            <>
              <div className="mb-5">
                <h2 className="text-lg font-semibold text-white">Set New Password</h2>
                <p className="text-slate-400 text-sm mt-0.5">
                  Enter a new password for <span className="text-indigo-300">{email}</span>
                </p>
              </div>

              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-1.5">New Password</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                      <Lock className="h-4 w-4" />
                    </div>
                    <input
                      {...register('newPassword')}
                      type={showPassword ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="••••••••"
                      className="w-full h-11 pl-10 pr-12 bg-white/10 border border-white/20 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    />
                    <button type="button" onClick={() => setShowPassword(!showPassword)}
                      className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 hover:text-slate-200">
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                  {errors.newPassword && <p className="mt-1.5 text-xs text-red-400">{errors.newPassword.message}</p>}
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-1.5">Confirm Password</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
                      <Lock className="h-4 w-4" />
                    </div>
                    <input
                      {...register('confirmPassword')}
                      type={showConfirm ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="••••••••"
                      className="w-full h-11 pl-10 pr-12 bg-white/10 border border-white/20 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    />
                    <button type="button" onClick={() => setShowConfirm(!showConfirm)}
                      className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 hover:text-slate-200">
                      {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                  {errors.confirmPassword && <p className="mt-1.5 text-xs text-red-400">{errors.confirmPassword.message}</p>}
                </div>

                {result && !result.success && (
                  <div className="flex items-center gap-2 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
                    <XCircle className="h-4 w-4 flex-shrink-0" />
                    <span>{result.message}</span>
                  </div>
                )}

                <button
                  type="submit"
                  disabled={isLoading}
                  className="w-full h-10 bg-indigo-600 hover:bg-indigo-500 disabled:bg-indigo-800 disabled:cursor-not-allowed text-white font-semibold rounded-xl flex items-center justify-center gap-2 transition-all"
                >
                  {isLoading ? (
                    <div className="h-5 w-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : 'Reset Password'}
                </button>

                <button
                  type="button"
                  onClick={() => navigate('/login')}
                  className="w-full text-center text-xs text-slate-400 hover:text-slate-200 transition-colors pt-1"
                >
                  Back to Sign In
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default ResetPasswordPage;
