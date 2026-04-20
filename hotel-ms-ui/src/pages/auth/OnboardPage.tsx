import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Hotel, Check, Copy, ArrowRight, ArrowLeft, Eye, EyeOff } from 'lucide-react';
import activationService, { type SelfRegisterResponse } from '../../services/activation.service';

type PlanKey = 'Trial' | 'Monthly3' | 'Monthly6' | 'FiveYear' | 'Unlimited';

const PLANS: { key: PlanKey; label: string; duration: string; price: string; features: string[] }[] = [
  {
    key: 'Trial',
    label: '30-Day Trial',
    duration: '30 days',
    price: 'Free',
    features: ['Full feature access', 'Up to 20 rooms', 'Email support'],
  },
  {
    key: 'Monthly3',
    label: '3-Month Plan',
    duration: '3 months',
    price: 'Starter',
    features: ['Full feature access', 'Unlimited rooms', 'Priority support', 'Audit logs'],
  },
  {
    key: 'Monthly6',
    label: '6-Month Plan',
    duration: '6 months',
    price: 'Growth',
    features: ['Everything in Starter', 'Multi-property', 'Advanced reports', 'Dedicated support'],
  },
  {
    key: 'FiveYear',
    label: '5-Year Plan',
    duration: '5 years',
    price: 'Enterprise',
    features: ['Everything in Growth', 'SLA guarantee', 'Custom integrations', 'On-site training'],
  },
  {
    key: 'Unlimited',
    label: 'Unlimited',
    duration: 'Forever',
    price: 'Unlimited',
    features: ['Everything in Enterprise', 'Lifetime updates', 'White-label option', 'API access'],
  },
];

export function OnboardPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [selectedPlan, setSelectedPlan] = useState<PlanKey>('Monthly3');
  const [form, setForm] = useState({ fullName: '', email: '', hotelName: '', password: '', confirmPassword: '' });
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<SelfRegisterResponse | null>(null);
  const [copied, setCopied] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));
    setErrors((err) => ({ ...err, [e.target.name]: '' }));
  };

  const validate = () => {
    const e: Record<string, string> = {};
    if (!form.fullName.trim()) e.fullName = 'Full name is required';
    if (!form.email.trim() || !/\S+@\S+\.\S+/.test(form.email)) e.email = 'Valid email is required';
    if (!form.hotelName.trim()) e.hotelName = 'Hotel / business name is required';
    if (form.password.length < 8) e.password = 'Password must be at least 8 characters';
    if (form.password !== form.confirmPassword) e.confirmPassword = 'Passwords do not match';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    setIsLoading(true);
    try {
      const data = await activationService.selfRegister({
        email: form.email,
        fullName: form.fullName,
        hotelName: form.hotelName,
        password: form.password,
        planType: selectedPlan,
      });
      setResult(data);
      setStep(3);
    } catch (err) {
      setErrors({ submit: err instanceof Error ? err.message : 'Registration failed. Please try again.' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopy = () => {
    if (!result) return;
    navigator.clipboard.writeText(result.plaintextCode);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-slate-50 dark:from-slate-900 dark:via-slate-800 dark:to-slate-900 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4">
        <Link to="/login" className="flex items-center gap-2.5">
          <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center shadow-sm">
            <Hotel className="h-5 w-5 text-white" />
          </div>
          <span className="text-xl font-bold text-slate-900 dark:text-slate-100">
            Hotel<span className="text-indigo-600">MS</span>
          </span>
        </Link>
        <Link to="/login" className="text-sm text-slate-500 dark:text-slate-400 hover:text-indigo-600 transition-colors">
          Already have an account? <span className="font-medium text-indigo-600">Sign in</span>
        </Link>
      </div>

      {/* Step indicator */}
      {step !== 3 && (
        <div className="flex justify-center mt-6 mb-2">
          <div className="flex items-center gap-2">
            {[1, 2].map((s) => (
              <div key={s} className="flex items-center gap-2">
                <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-semibold transition-colors ${
                  step >= s ? 'bg-indigo-600 text-white' : 'bg-slate-200 dark:bg-slate-700 text-slate-500 dark:text-slate-400'
                }`}>
                  {step > s ? <Check className="h-3.5 w-3.5" /> : s}
                </div>
                {s < 2 && <div className={`w-12 h-0.5 ${step > s ? 'bg-indigo-600' : 'bg-slate-200 dark:bg-slate-700'}`} />}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Step 1 — Plan selection */}
      {step === 1 && (
        <div className="flex-1 flex flex-col items-center px-4 py-8">
          <div className="max-w-4xl w-full">
            <h1 className="text-3xl font-bold text-slate-900 dark:text-slate-100 text-center mb-2">
              Choose your plan
            </h1>
            <p className="text-slate-500 dark:text-slate-400 text-center mb-8">
              Start free or commit to a longer plan. You can always upgrade later.
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
              {PLANS.map((plan) => (
                <button
                  key={plan.key}
                  onClick={() => setSelectedPlan(plan.key)}
                  className={`relative text-left rounded-2xl border-2 p-5 transition-all ${
                    selectedPlan === plan.key
                      ? 'border-indigo-600 bg-indigo-50 dark:bg-indigo-900/20 shadow-md'
                      : 'border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 hover:border-indigo-300 hover:shadow-sm'
                  }`}
                >
                  {selectedPlan === plan.key && (
                    <div className="absolute top-3 right-3 w-5 h-5 bg-indigo-600 rounded-full flex items-center justify-center">
                      <Check className="h-3 w-3 text-white" />
                    </div>
                  )}
                  <p className="text-xs font-semibold text-indigo-600 uppercase tracking-wide mb-1">{plan.price}</p>
                  <h3 className="text-base font-bold text-slate-900 dark:text-slate-100 mb-1">{plan.label}</h3>
                  <p className="text-xs text-slate-500 dark:text-slate-400 mb-3">{plan.duration}</p>
                  <ul className="space-y-1.5">
                    {plan.features.map((f) => (
                      <li key={f} className="flex items-start gap-1.5 text-xs text-slate-600 dark:text-slate-400">
                        <Check className="h-3 w-3 text-green-500 mt-0.5 flex-shrink-0" />
                        {f}
                      </li>
                    ))}
                  </ul>
                </button>
              ))}
            </div>
            <div className="flex justify-center mt-8">
              <button
                onClick={() => setStep(2)}
                className="flex items-center gap-2 px-8 py-3 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-xl transition-colors shadow-sm"
              >
                Continue with {PLANS.find((p) => p.key === selectedPlan)?.label}
                <ArrowRight className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Step 2 — Registration details */}
      {step === 2 && (
        <div className="flex-1 flex items-start justify-center px-4 py-8">
          <div className="w-full max-w-md">
            <button
              onClick={() => setStep(1)}
              className="flex items-center gap-1.5 text-sm text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 mb-6 transition-colors"
            >
              <ArrowLeft className="h-4 w-4" />
              Back to plan selection
            </button>
            <div className="bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-700 p-8">
              <div className="mb-6">
                <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-indigo-600 bg-indigo-50 dark:bg-indigo-900/30 px-3 py-1 rounded-full mb-3">
                  {PLANS.find((p) => p.key === selectedPlan)?.label}
                </span>
                <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Create your account</h2>
                <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                  You'll receive an activation code to complete your workspace setup.
                </p>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Full name</label>
                  <input
                    name="fullName"
                    value={form.fullName}
                    onChange={handleChange}
                    placeholder="Jane Smith"
                    className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-3 py-2.5 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  {errors.fullName && <p className="mt-1 text-xs text-red-500">{errors.fullName}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Work email</label>
                  <input
                    name="email"
                    type="email"
                    value={form.email}
                    onChange={handleChange}
                    placeholder="jane@grandhotel.com"
                    className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-3 py-2.5 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  {errors.email && <p className="mt-1 text-xs text-red-500">{errors.email}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Hotel / business name</label>
                  <input
                    name="hotelName"
                    value={form.hotelName}
                    onChange={handleChange}
                    placeholder="Grand Horizon Hotel"
                    className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-3 py-2.5 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  {errors.hotelName && <p className="mt-1 text-xs text-red-500">{errors.hotelName}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Password</label>
                  <div className="relative">
                    <input
                      name="password"
                      type={showPassword ? 'text' : 'password'}
                      value={form.password}
                      onChange={handleChange}
                      placeholder="Min. 8 characters"
                      className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-3 py-2.5 pr-10 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword((v) => !v)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                    >
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                  {errors.password && <p className="mt-1 text-xs text-red-500">{errors.password}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">Confirm password</label>
                  <input
                    name="confirmPassword"
                    type={showPassword ? 'text' : 'password'}
                    value={form.confirmPassword}
                    onChange={handleChange}
                    placeholder="Repeat your password"
                    className="w-full rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 px-3 py-2.5 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  {errors.confirmPassword && <p className="mt-1 text-xs text-red-500">{errors.confirmPassword}</p>}
                </div>

                {errors.submit && (
                  <div className="rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-700 dark:text-red-400">
                    {errors.submit}
                  </div>
                )}

                <button
                  onClick={handleSubmit}
                  disabled={isLoading}
                  className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-semibold rounded-xl transition-colors mt-2"
                >
                  {isLoading ? (
                    <>
                      <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                      Creating your account…
                    </>
                  ) : (
                    <>Create account <ArrowRight className="h-4 w-4" /></>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Step 3 — Success: show activation code */}
      {step === 3 && result && (
        <div className="flex-1 flex items-start justify-center px-4 py-8">
          <div className="w-full max-w-md">
            <div className="bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-700 p-8 text-center">
              <div className="w-16 h-16 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mx-auto mb-5">
                <Check className="h-8 w-8 text-green-600" />
              </div>
              <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-100 mb-2">Account created!</h2>
              <p className="text-sm text-slate-500 dark:text-slate-400 mb-6">
                Copy your activation code. You'll need it to activate your workspace after logging in.
              </p>

              {/* Activation code */}
              <div className="bg-slate-50 dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-700 p-4 mb-2">
                <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-2">
                  Your Activation Code
                </p>
                <code className="text-2xl font-mono font-bold tracking-widest text-indigo-600 dark:text-indigo-400">
                  {result.plaintextCode}
                </code>
              </div>
              <button
                onClick={handleCopy}
                className="flex items-center gap-2 mx-auto text-sm text-slate-500 dark:text-slate-400 hover:text-indigo-600 transition-colors mb-6"
              >
                {copied ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
                {copied ? 'Copied!' : 'Copy code'}
              </button>

              <div className="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-lg px-4 py-3 text-left text-xs text-amber-700 dark:text-amber-400 mb-6">
                <strong>Important:</strong> Save this code — it will not be shown again. You will enter it after logging in to activate your{' '}
                <strong>{result.hotelName}</strong> workspace.
              </div>

              <div className="space-y-2">
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  Plan: <strong>{result.planLabel}</strong> · Email: <strong>{result.boundToEmail}</strong>
                </p>
                <button
                  onClick={() => navigate('/login')}
                  className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-xl transition-colors"
                >
                  Go to login <ArrowRight className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
