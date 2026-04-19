import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Hotel, KeyRound, Building2, User, Lock, ArrowRight, CheckCircle } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import activationService from '../../services/activation.service';
import { useAuthStore } from '../../lib/store';

const schema = z.object({
  code: z.string().min(1, 'Activation code is required'),
  email: z.string().email('Valid email required'),
  tenantName: z.string().min(2, 'Hotel / company name required'),
  adminFullName: z.string().min(2, 'Your full name is required'),
  adminPassword: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine((d) => d.adminPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

type FormData = z.infer<typeof schema>;

export function ActivatePage() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormData) => {
    setError(null);
    try {
      const result = await activationService.activate({
        code: data.code,
        email: data.email,
        tenantName: data.tenantName,
        adminPassword: data.adminPassword,
        adminFullName: data.adminFullName,
      });

      // Persist auth state
      localStorage.setItem('hotel_ms_token', result.token);
      localStorage.setItem('hotel_ms_refresh_token', result.refreshToken);

      setAuth(
        {
          email: data.email,
          fullName: data.adminFullName,
          roles: ['SuperAdmin'],
          tenantId: result.tenantId,
        },
        result.token
      );

      setSuccess(true);
      setTimeout(() => navigate('/dashboard'), 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Activation failed. Please check your code and try again.');
    }
  };

  if (success) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-2xl p-10 max-w-md w-full text-center">
          <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Activation Successful!</h2>
          <p className="text-gray-500">Redirecting you to the dashboard…</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-lg">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-2xl mb-4 shadow-lg">
            <Hotel className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-white">Activate Your Hotel</h1>
          <p className="mt-2 text-blue-200 text-sm">Enter your activation code to set up your account</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="bg-white rounded-2xl shadow-2xl p-8 space-y-5">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700">
              {error}
            </div>
          )}

          {/* Activation Code */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Activation Code</label>
            <div className="relative">
              <KeyRound className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                {...register('code')}
                placeholder="XXXX-XXXX-XXXX-XXXX"
                className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm font-mono tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500 uppercase"
                autoComplete="off"
              />
            </div>
            {errors.code && <p className="mt-1 text-xs text-red-600">{errors.code.message}</p>}
          </div>

          {/* Email */}
          <Input
            label="Admin Email"
            type="email"
            leftIcon={<User className="w-4 h-4" />}
            {...register('email')}
            error={errors.email?.message}
            placeholder="email@yourhotel.com"
          />

          {/* Hotel Name */}
          <Input
            label="Hotel / Company Name"
            leftIcon={<Building2 className="w-4 h-4" />}
            {...register('tenantName')}
            error={errors.tenantName?.message}
            placeholder="Grand Palace Hotel"
          />

          {/* Full Name */}
          <Input
            label="Your Full Name"
            leftIcon={<User className="w-4 h-4" />}
            {...register('adminFullName')}
            error={errors.adminFullName?.message}
            placeholder="John Smith"
          />

          {/* Password */}
          <Input
            label="Admin Password"
            type="password"
            leftIcon={<Lock className="w-4 h-4" />}
            {...register('adminPassword')}
            error={errors.adminPassword?.message}
            placeholder="Minimum 8 characters"
          />

          {/* Confirm Password */}
          <Input
            label="Confirm Password"
            type="password"
            leftIcon={<Lock className="w-4 h-4" />}
            {...register('confirmPassword')}
            error={errors.confirmPassword?.message}
            placeholder="Repeat your password"
          />

          <Button
            type="submit"
            isLoading={isSubmitting}
            className="w-full"
            rightIcon={<ArrowRight className="w-4 h-4" />}
          >
            Activate Account
          </Button>

          <p className="text-center text-xs text-gray-400">
            Already have an account?{' '}
            <a href="/login" className="text-blue-600 hover:underline">Sign in</a>
          </p>
        </form>
      </div>
    </div>
  );
}
